using Archipelago.MultiClient.Net.Enums;
using ArchipelagoFFX.Client;
using Fahrenheit;
using Fahrenheit.Atel;
using Fahrenheit.FFX;
using Fahrenheit.FFX.Battle;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using static ArchipelagoFFX.Client.ArchipelagoClientModule;
using static Fahrenheit.FFX.Globals;
using FhGCall = Fahrenheit.FhCall;
using FhXCall = Fahrenheit.FFX.FhCall;

namespace ArchipelagoFFX;

[FhLoad(FhGameId.FFX)]
public unsafe class CaptureModule : FhModule {
    private FhModuleHandle<ArchipelagoClientModule> _client_handle;
    private ArchipelagoClientModule? _client;

    private FhModuleHandle<ArchipelagoFFXModule> _ffx_interop_handle;
    private ArchipelagoFFXModule? _ffx_interop;

    public CaptureModule() {
        _client_handle = new(this);
        _ffx_interop_handle = new(this);
    }

    // TODO: Remove once Fahrenheit adds Atel call targets to FhCall
    // Mars Sigil location check (Atel CT_RetInt hook); not yet a curated FhCall entry, so kept as a local handle.
    private FhMethodHandle<FhGCall.d_CT_RetInt> h_ret_hasKeyItem_handle
        => new( new FhMethodLocation("FFX.exe", 0x45B7A0) );

    public override bool init(FhModContext mod_context, FileStream global_state_file) {
        return _client_handle.try_get_module(out _client)
            && _ffx_interop_handle.try_get_module(out _ffx_interop)
            && FhXCall.MsMonsterCapture.hook(this, h_MsMonsterCapture)
            && FhXCall.FUN_00783bb0.hook(this, h_FUN_00783bb0)
            && FhXCall.AtelEventSetUp.hook(this, h_AtelEventSetUp)
            && h_ret_hasKeyItem_handle.hook(this, ret_hasKeyItem)
            && FhXCall.MsDamageCheckDeath.hook(this, h_MsDamageCheckDeath)
            && FhXCall.MsSetRamChrParam.hook(this, h_MsSetRamChrParam)
            && FhXCall.MsSetSaveParam.hook(this, h_MsSetSaveParam)
            && FhXCall.MsCalcCommand.hook(this, h_MsCalcCommand)
            && FhXCall.MsBattleEncountExe.hook(this, h_MsBattleEncountExe);
    }

    private static void set(byte* code_ptr, uint offset, AtelInst[] opcodes) {
        byte* ptr = code_ptr + offset;
        foreach (AtelInst op in opcodes) {
            foreach (byte b in op.to_bytes()) {
                *ptr = b;
                ptr++;
            }
        }
    }

    private static void set(byte* code_ptr, uint[] offsets, AtelInst[] opcodes) {
        foreach (uint offset in offsets) {
            set(code_ptr, offset, opcodes);
        }
    }

    private bool h_MsMonsterCapture(int target_id, int arena_idx) {
        bool captured = FhXCall.MsMonsterCapture.chain_from(h_MsMonsterCapture).fnptr!(target_id, arena_idx);

        _logger.Info($"Fiend Capture: Target={target_id}, Arena Index={arena_idx}, Captured={captured}");

        // Send AP Location if successfully captured
        if (captured) {
            lock (_client!.client_lock) {
                if (_client!.sendLocation(arena_idx, ArchipelagoLocationType.Capture) && ArchipelagoFFXModule.item_locations.capture.TryGetValue(arena_idx, out var item)) {
                    _ffx_interop!.obtain_item(item.id);
                }

                int amount = save_data->monsters_captured[arena_idx];
                if (_client!.is_connected) {
                    if (amount > 0)
                        _client!.current_session!.DataStorage[Scope.Slot, "FFX_CAPTURE_" + arena_idx] = amount;
                    else
                        _client!.current_session!.DataStorage[Scope.Slot, "FFX_CAPTURE_" + arena_idx] = 0;
                }
            }
        }
        return captured;
    }

    private readonly HashSet<ushort> initialized_monsters = [];
    private static readonly HashSet<short> _incorrect_arena_idx = [
        041, // Sahagin
        042, // Sahagin
        043, // Sahagin
        068, // Piranha
        069, // Piranha
        070, // Piranha
        101, // Tros
        155, // Sahagin
        156, // Sahagin Chief
        157, // Garuda (Tutorial)
        230, // Garuda (Tutorial)
        231, // Dingo (Tutorial)
        232, // Water Flan (Tutorial)
        233, // Condor (Tutorial)
        234, // Ragora (Tutorial)
    ];
    private void h_FUN_00783bb0(byte mon_idx) {
        byte num_initialized = FhUtil.get_at<byte>(0xD2CA80);
        if (num_initialized == 0) initialized_monsters.Clear();

        FhXCall.FUN_00783bb0.chain_from(h_FUN_00783bb0).fnptr!(mon_idx);

        Chr* mon = FhXCall.MsGetMon.fnptr!(mon_idx);
        if (initialized_monsters.Add(mon->chr_id)) {
            MonStats* stats = mon->ptr_base_stats;

            // Corrects incorrect monster arena indexes to be uncapturable
            if (_incorrect_arena_idx.Contains((short)(mon->chr_id & 0xFFF))) {
                stats->monster_arena_idx = 0xFF;
            }
        }
        else {
            _logger.Debug($"{mon_idx}: already initialized ({mon->chr_id})");
        }
    }

    private string? _event_name;
    private void h_AtelEventSetUp(int event_id) {
        FhXCall.AtelEventSetUp.chain_from(h_AtelEventSetUp).fnptr!(event_id);

        _event_name = Marshal.PtrToStringAnsi((nint)FhXCall.AtelGetEventName.fnptr!((uint)event_id))!;
        _logger.Debug($"atel_event_setup: {_event_name}");
        byte* code_ptr = Atel.controllers[0].worker(0)->code_ptr;

        switch (_event_name) {
            case "nagi0700":
                // Always show Nirvana chest
                set(code_ptr, 0xE25D, [AtelOp.JMP.build(0)]);

                // Continue even if Calm Lands conquest not complete
                set(code_ptr, 0xE07B, [AtelOp.JMP.build(2)]);

                // Skip introduction and explanation
                set(code_ptr, 0xDCF3, [AtelOp.JMP.build(0xB29)]);

                // Unlock Monster Arena
                save_data->event_flags[0].set_bit(0, true);
                save_data->monsters_captured[43] = 99;
                save_data->monsters_captured[59] = 99;
                break;
        }
    }

    //Check Mars Sigil location instead of inventory
    private int ret_hasKeyItem(AtelBasicWorker* work, int* storage, AtelStack* atelStack) {
        if (_event_name == "nagi0700") {
            int item_id = atelStack->pop_int();

            if (item_id == 0xA028) {
                lock (_client!.client_lock) {
                    return _client!.local_checked_locations.Contains(276 | (long)ArchipelagoLocationType.Treasure) ? 1 : 0;
                }
            }
            else {
                atelStack->push_int(item_id);
            }
        }

        return h_ret_hasKeyItem_handle.chain_from(ret_hasKeyItem).fnptr!(work, storage, atelStack);
    }

    private int h_MsDamageCheckDeath(int attacker_id, int target_id, int param_3, int param_4) {
        Chr* target = FhXCall.MsGetChr.fnptr!(target_id);
        MonStats* mon_stats = target->ptr_base_stats;

        ushort capture_index = mon_stats is not null ? mon_stats->monster_arena_idx : (ushort)0xFF;

        if (ArchipelagoFFXModule.seed.Options.AlwaysCapture == 1
         && ArchipelagoFFXModule.seed.Options.CaptureDamage == 2
         && target_id >= 20 // Make sure to not capture ourselves
         && capture_index != 0xFF // Make sure to not capture rifles
         && Battle.btl->battle_type == 0 // ... and Monster Arena enemies
        ) {
            target->should_try_capture = true;
        }

        return FhXCall.MsDamageCheckDeath.chain_from(h_MsDamageCheckDeath).fnptr!(attacker_id, target_id, param_3, param_4);
    }

    private void h_MsSetRamChrParam(uint chr_id) {
        FhXCall.MsSetRamChrParam.chain_from(h_MsSetRamChrParam).fnptr!(chr_id);

        Chr* chr = FhXCall.MsGetChr.fnptr!((int)chr_id);

        if (ArchipelagoFFXModule.seed.Options.AlwaysCapture == 1) {
            chr->ram.auto_ability_effects.has_capture = true;
        }
    }

    private void h_MsSetSaveParam(uint chr_id) {
        FhXCall.MsSetSaveParam.chain_from(h_MsSetSaveParam).fnptr!(chr_id);

        // Does nothing??
        if (ArchipelagoFFXModule.seed.Options.AlwaysCapture == 1) {
            save_data->ply_saves[(int)chr_id].auto_ability_effects.has_capture = true;
        }
    }

    private void h_MsCalcCommand(AttackCue* param_1, int param_2) {
        FhXCall.MsCalcCommand.chain_from(h_MsCalcCommand).fnptr!(param_1, param_2);
        if (param_1 == null) return;

        uint local_6c;
        Command* command = FhXCall.MsGetCommand.fnptr!(param_1->attacker_id, 0, -1, &param_1->command_list[param_2], &local_6c);

        if (param_1->command_count <= param_2 || command == null) return;

        Chr* attacker = FhXCall.MsGetChr.fnptr!(param_1->attacker_id);

        int[] local_7c = [0, 0, 0, param_2];
        if (command->absorbs_dmg) {
            local_7c[2] = (int)FhXCall.FUN_0078d100.fnptr!(attacker);
        }

        //TODO: Figure out a way not to duplicate affection in _FUN_0078bb30
        byte[] targets = new byte[32];
        byte[] local_48 = new byte[32];
        fixed (byte* p_targets = targets) {
            fixed (byte* p_local_48 = local_48) {
                fixed (int* p_local_7c = local_7c) {
                    FhXCall.FUN_0078bb30.fnptr!(param_1->attacker_id, p_targets, p_local_48, command, local_6c, &param_1->command_list[param_2].targets, p_local_7c + 1);
                }
            }
        }

        for (uint target_id = 0; target_id < 32; target_id++) {
            if (targets[target_id] != 0) {
                if (local_7c[2] == 0 || target_id != param_1->attacker_id) {
                    Chr* target = FhXCall.MsGetChr.fnptr!((int)target_id);
                    uint iVar6 = FhXCall.FUN_0078d100.fnptr!(target);
                    if (iVar6 != 0) {
                        if (attacker->ram.auto_ability_effects.has_capture && Battle.btl->battle_type == 0 && (ArchipelagoFFXModule.seed.Options.CaptureDamage > 0 || command->uses_weapon_properties)) {
                            target->should_try_capture = true;
                        }
                        else {
                            target->should_try_capture = false;
                        }
                    }
                }
            }
        }
    }

    private int get_monster_arena_idx(int monster_id) {
        byte[] filename = Encoding.UTF8.GetBytes($"host0:/ffx/master/jppc/battle/mon/_m{monster_id:D3}/m{monster_id:D3}.bin");

        fixed (byte* filename_ptr = &filename[0]) {
            void* filestream = FhXCall.sceOpen.fnptr!(filename_ptr, 1);

            if (filestream is null) {
                _logger.Info("Failed to open monster file");
                return -1;
            }

            int filesize = FhXCall.sceLseek.fnptr!(filestream, 0, 2);
            FhXCall.sceLseek.fnptr!(filestream, 0, 0);

            void* file = NativeMemory.Alloc((nuint)filesize);
            FhXCall.sceRead.fnptr!(filestream, file, filesize);
            FhXCall.sceClose.fnptr!(filestream);

            int stats_offset = *(int*)((int)file + 0xC);
            MonStats* mon_stats = (MonStats*)((int)file + stats_offset);

            // Invalid idx
            // ushort so we only have to check >= 512, not the < 0 case
            if (mon_stats->monster_arena_idx >= 512 || mon_stats->monster_arena_idx == 0xFF) return -1;
            int arena_idx = mon_stats->monster_arena_idx;

            NativeMemory.Free(file);

            return arena_idx;
        }
    }

    private static readonly int[] species_capture_requirements = [
        3, // Raldo
        3, // Bunyip
        3, // Murussu
        3, // Mafdet
        3, // Shred

        4, // Gandarewa
        4, // Aerouge
        4, // Imp

        3, // Dingo
        3, // Mi'ihen Fang
        3, // Garm
        3, // Snow Wolf
        3, // Sand Wolf
        3, // Skoll
        3, // Bandersnatch

        3, // Water Flan
        3, // Thunder Flan
        3, // Snow Flan
        3, // Ice Flan
        3, // Flame Flan
        3, // Dark Flan

        3, // Dinonix
        3, // Ipiria
        3, // Raptor
        3, // Melusine
        3, // Iguion
        3, // Yowie

        5, // Condor
        5, // Simurgh
        5, // Alcyone

        4, // Killer Bee
        4, // Bite Bug
        4, // Wasp
        4, // Nebiros

        4, // Floating Eye
        4, // Buer
        4, // Evil Eye
        4, // Ahriman

        1, // Ragora
        1, // Grat

        1, // Garuda
        1, // Zu

        1, // Sand Worm

        0, // Unused

        1, // Ghost

        1, // Achelous
        1, // Maelspike

        5, // Dualhorn
        5, // Valaha
        5, // Grendel

        5, // Vouivre
        5, // Lamashtu
        5, // Kusariqqu
        5, // Mushussu
        5, // Nidhogg

        1, // Malboro
        1, // Great Malboro

        1, // Ogre
        1, // Bashura

        0, // Unused

        1, // Splasher

        3, // Yellow Element
        3, // White Element
        3, // Red Element
        3, // Gold Element
        3, // Blue Element
        3, // Dark Element
        3, // Black Element

        1, // Epaaj

        1, // Behemoth
        1, // Behemoth King

        1, // Chimera
        1, // Chimera Brain

        1, // Coeurl
        1, // Master Coeurl

        1, // Demonolith

        10, // Iron Giant
        10, // Gemini (Sword)
        10, // Gemini (Club)

        1, // Basilisk
        1, // Anacondaur

        1, // Adamantoise

        1, // Varuna

        1, // Ochu
        1, // Mandragora

        5, // Bomb
        5, // Grenade

        1, // Qactuar
        1, // Cactuar

        1, // Larva

        1, // Barbatos

        5, // Funguar
        5, // Thorn
        5, // Exoray

        1, // Xiphos

        5, // Puroboros

        1, // Spirit

        1, // Wraith

        1, // Tonberry
        1, // MastertTonberry

        3, // Zaurus

        3, // Halma

        4, // Floating Death

        1, // Machea
    ];

    private int get_monster_capture_requirement(int monster_arena_idx) {
        return ArchipelagoFFXModule.seed.Options.CaptureRequirement switch {
            ArchipelagoData.CaptureRequirement.None     => 0,
            ArchipelagoData.CaptureRequirement.Area     => 1,
            ArchipelagoData.CaptureRequirement.Species  => species_capture_requirements[monster_arena_idx],
            ArchipelagoData.CaptureRequirement.Original => 10,
            _ => throw new NotImplementedException(),
        };
    }

    private int get_monster_captures_left(int monster_arena_idx) {
        int capture_count = save_data->monsters_captured[monster_arena_idx];
        int capture_req = get_monster_capture_requirement(monster_arena_idx);

        return int.Max(0, capture_req - capture_count);
    }

    private int get_extra_weight_for_formation(BtlBinField* field, BtlBinFormation formation) {
        char[] field_name = new char[6];

        for (int i = 0; i < 6; i++) {
            field_name[i] = (char)field->name[i];
        }

        string file_name = $"{new string(field_name)}_{formation.index:D2}";

        byte[] filepath = Encoding.UTF8.GetBytes($"host0:/ffx/master/jppc/battle/btl/{file_name}/{file_name}.bin");

        float extra_weight = 0.0f;

        fixed (byte* filepath_ptr = &filepath[0]) {
            void* open_result = FhXCall.sceOpen.fnptr!(filepath_ptr, 1);

            if (open_result is null) return 0;

            int file_size = FhXCall.sceLseek.fnptr!(open_result, 0, 2);
            FhXCall.sceLseek.fnptr!(open_result, 0, 0);

            void* file = NativeMemory.Alloc((nuint)file_size);
            FhXCall.sceRead.fnptr!(open_result, file, file_size);
            FhXCall.sceClose.fnptr!(open_result);

            int chunk_ptr = *(int*)((nint)file + 0xC);
            if (chunk_ptr != 0) {
                short* mon_ids_ptr = (short*)((nint)file + chunk_ptr + 0xC);
                List<short> mon_ids = new(8);

                for (int i = 0; i < 8; i++) {
                    short mon_id = mon_ids_ptr[i];
                    if (mon_id == -1) continue;

                    mon_ids.Add((short)(mon_id & 0xFFF));
                }

                float[] weight_mults = new float[mon_ids.Count];

                for (int i = 0; i < mon_ids.Count; i++) {
                    int arena_idx = get_monster_arena_idx(mon_ids[i]);

                    if (arena_idx == -1) {
                        weight_mults[i] = 0.0f;
                    } else {
                        weight_mults[i] = (float)get_monster_captures_left(arena_idx)/get_monster_capture_requirement(arena_idx);
                    }
                }

                bool all_captured = true;
                foreach (float mult in weight_mults) {
                    extra_weight += ArchipelagoFFXModule.seed.Options.EncounterWeighting * mult;
                    if (mult > 0.0f) all_captured = false;
                }

                if (all_captured) {
                    extra_weight = -2;
                }

                // Magic Urn
                if (mon_ids.Contains(202)) {
                    extra_weight = -1;
                }
            }

            NativeMemory.Free(file);
        }
        return (int)extra_weight;
    }

    private int h_MsBattleEncountExe(int field_id, int group_idx, float walked_delta) {
        if (ArchipelagoFFXModule.seed.Options.CaptureRequirement == 0 || ArchipelagoFFXModule.seed.Options.EncounterWeighting == 0) {
            return FhXCall.MsBattleEncountExe.chain_from(h_MsBattleEncountExe).fnptr!(field_id, group_idx, walked_delta);
        }

        // Globals
        int* g_keybattle = FhUtil.ptr_at<int>(0xD2CA24);
        int* g_keydown_R = FhUtil.ptr_at<int>(0xD2CA20);
        int* DAT_0112ca28 = FhUtil.ptr_at<int>(0xD2CA28);
        int* g_encounter_level = FhUtil.ptr_at<int>(0x8421C8);
        int* EnableBattle = FhUtil.ptr_at<int>(0x8421BC);

        // _logger.Info($"  g_keybattle = {*g_keybattle}");
        // _logger.Info($"  g_keydown_R = {*g_keydown_R}");
        // _logger.Info($"  DAT_0112ca28 = {*DAT_0112ca28}");
        // _logger.Info($"  g_encounter_level = {*g_encounter_level}");
        // _logger.Info($"  EnableBattle = {*EnableBattle}");

        int field_idx = FhXCall.MsBtlListFieldNum.fnptr!(field_id);

        if (*g_keybattle != 0 && *g_keydown_R != 0) {
            Battle.btl->group_idx = (byte)group_idx;
            Battle.btl->formation_idx = (byte)*DAT_0112ca28;
            *(byte*)((nint)Battle.btl + 0x12) = 1;
            Battle.btl->field_idx = (ushort)field_idx;
            FhXCall.ResetEncountExe.fnptr!(1);
            return -1;
        }

        bool can_encounter = true;
        if (*g_encounter_level == 2) { // x4 encounter booster
            walked_delta *= 10.0f;
            can_encounter = true;
        } else if (*g_encounter_level == 1) {
            can_encounter = *(bool*)((nint)Battle.btl + 0x107);
        } else if (*g_encounter_level == 0) {
            walked_delta = 0.0f;
            can_encounter = false;
        }

        // _logger.Info($"  btl.__0x12: {*(byte*)((nint)Battle.btl + 0x12)}");
        // _logger.Info($"  can_encounter: {can_encounter}");

        if (*(byte*)((nint)Battle.btl + 0x12) == 2) return 0;
        if (field_idx < 0) return 0;
        if (walked_delta <= 0.0f || !can_encounter) return 0;

        BtlBinField* field = FhXCall.MsBtlListField.fnptr!(field_idx);
        BtlBinEncounter* encounter = FhXCall.MsBtlListEncount.fnptr!(field_idx);

        // _logger.Info($"  encounter->group_count: {encounter->group_count}");

        if (group_idx < 0 || group_idx >= encounter->group_count) return 0;
        if (*EnableBattle == 0) return 0;

        BtlBinGroup* group = FhXCall.MsBtlListGroup.fnptr!(field_idx, group_idx);

        if (group->grace == 0 || group->total_weight == 0 || group->formation_count == 0) return 0;

        Battle.btl->walked_dist += walked_delta;
        Battle.btl->walked_dist_total += walked_delta;
        Battle.btl->grace = group->grace;

        int rolls_so_far = (int)(Battle.btl->walked_dist_total / 10.0f);

        bool in_grace = group->grace / 2 >= rolls_so_far;

        if (Battle.btl->walked_dist <= 10.0f) return 0;

        for (; 10.0f < Battle.btl->walked_dist; Battle.btl->walked_dist -= 10.0f) {
            if (in_grace) continue;

            int encounter_chance = (rolls_so_far - (group->grace / 2)) * 256 / (group->grace * 4);
            *(float*)((nint)Battle.btl + 0x110) *= 256.0f - encounter_chance;
            *(float*)((nint)Battle.btl + 0x114) *= 256.0f;

            if ((FhXCall.brnd.fnptr!(0) & 0xFF) >= encounter_chance) continue;

            // We have to prepare weights first
            int total_weight = group->total_weight;
            int[] weights = new int[group->formation_count];
            bool all_complete = true;

            for (int formation_idx = 0; formation_idx < group->formation_count; formation_idx++) {
                //BtlBinFormation formation = group->formations[formation_idx];
                BtlBinFormation formation = MemoryMarshal.CreateSpan(ref group->first_formation, group->formation_count)[formation_idx];

                if (formation.weight == 0) {
                    weights[formation_idx] = 0;
                    continue;
                }

                weights[formation_idx] = formation.weight / 17;

                int extra_weight = get_extra_weight_for_formation(field, formation);

                if (extra_weight == -1) {
                    weights[formation_idx] = 0;
                    total_weight -= formation.weight / 17;
                } else if (extra_weight == -2) {
                    int old_weight = weights[formation_idx];
                    weights[formation_idx] = 1;
                    total_weight -= int.Max(0, old_weight - 1);
                } else {
                    weights[formation_idx] += extra_weight;
                    total_weight += extra_weight;
                    all_complete = false;
                }
            }

            // Adjust weights if all formations met their requirements
            if (all_complete)  {
                total_weight = group->total_weight;

                for (int formation_idx = 0; formation_idx < group->formation_count; formation_idx++) {
                    //BtlBinFormation formation = group->formations[formation_idx];
                    BtlBinFormation formation = MemoryMarshal.CreateSpan(ref group->first_formation, group->formation_count)[formation_idx];
                    weights[formation_idx] = formation.weight / 17;
                }
            }

            // Let's figure out which formation we should encounter!
            int formation_rng = FhXCall.brnd.fnptr!(1) % total_weight;
            int iVar7 = 0;

            _logger.Debug( "Encounter!");
            _logger.Debug($"  Requirements complete: {all_complete}");
            _logger.Debug($"  Total weight: {total_weight}");
            _logger.Debug($"  Weights: {String.Join(", ", weights)}");
            _logger.Debug($"  Formation RNG: {formation_rng}");

            for (int formation_idx = 0; formation_idx < group->formation_count; formation_idx++) {
                iVar7 += weights[formation_idx];
                if (!(formation_rng < iVar7)) continue;

                if (*(byte*)((nint)Battle.btl + 0x27) == 0) {
                    *(byte*)((nint)Battle.btl + 0x12) = 1;
                } else {
                    FhXCall.Sg_FadeInW.fnptr!(3);
                    save_data->battle_count += 1;
                }

                Battle.btl->field_idx = (ushort)field_idx;
                Battle.btl->group_idx = (byte)group_idx;
                Battle.btl->formation_idx = (byte)formation_idx;
                FhXCall.ResetEncountExe.fnptr!(1);

                return -1;
            }
        }

        return 0;
    }
}
