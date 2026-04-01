using System;

using Archipelago.MultiClient.Net.Enums;
using Fahrenheit.Atel;
using Fahrenheit.FFX;
using Fahrenheit.FFX.Battle;
using Fahrenheit.Modules.ArchipelagoFFX.Client;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static Fahrenheit.FFX.Globals;
using static Fahrenheit.Modules.ArchipelagoFFX.Client.FFXArchipelagoClient;
using static Fahrenheit.Modules.ArchipelagoFFX.delegates;

namespace Fahrenheit.Modules.ArchipelagoFFX;

[FhLoad(FhGameId.FFX)]
public unsafe partial class CaptureModule : FhModule {
    // Some structs that Fahrenheit is currently missing
    [StructLayout(LayoutKind.Sequential)]
    private struct BtlBinField {
        [InlineArray(8)]
        public struct FieldName {
            private char _data;
        }

        public short id;
        public short data_offset;
        public short __0x4;
        public FieldName name;
    }

    [StructLayout(LayoutKind.Explicit, Size = 0x2)]
    private struct BtlBinEncounter {
        [FieldOffset(0x0)] public byte total_formation_count;
        [FieldOffset(0x1)] public byte group_count;

        [FieldOffset(0x2)] private BtlBinGroup _first_group;

        [UnscopedRef]
        public Span<BtlBinGroup> groups => MemoryMarshal.CreateSpan(ref _first_group, group_count);
    }

    [StructLayout(LayoutKind.Explicit, Size = 0x5)]
    private struct BtlBinGroup {
        [FieldOffset(0x0)] public byte formation_count;
        [FieldOffset(0x1)] public short battlefield;
        [FieldOffset(0x3)] public byte grace;
        [FieldOffset(0x4)] public byte total_weight;

        [FieldOffset(0x5)] private BtlBinFormation _first_formation;

        [UnscopedRef]
        public Span<BtlBinFormation> formations => MemoryMarshal.CreateSpan(ref _first_formation, formation_count);
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct BtlBinFormation {
        public byte index;
        public byte weight;
    }

    private FhModContext? _mod_context;
    private FileStream? _global_state;

    public CaptureModule() {
        const string GAME = "FFX.exe";

        _MsMonsterCapture = new FhMethodHandle<MsMonsterCapture>(this, GAME, __addr_MsMonsterCapture, h_MsMonsterCapture);
        _FUN_00783bb0 = new FhMethodHandle<FUN_00783bb0>(this, GAME, __addr_FUN_00783bb0, h_FUN_00783bb0);
        _AtelEventSetUp = new FhMethodHandle<AtelEventSetUp>(this, GAME, __addr_AtelEventSetUp, h_AtelEventSetUp);
        _ret_hasKeyItem = new FhMethodHandle<CT_RetInt>(this, GAME, __addr_ret_hasKeyItem, h_ret_hasKeyItem);
        _MsDamageCheckDeath = new FhMethodHandle<MsDamageCheckDeath>(this, GAME, __addr_MsDamageCheckDeath, h_MsDamageCheckDeath);
        _MsSetRamChrParam = new FhMethodHandle<MsSetRamChrParam>(this, GAME, __addr_MsSetRamChrParam, h_MsSetRamChrParam);
        _MsSetSaveParam = new FhMethodHandle<MsSetSaveParam>(this, GAME, __addr_MsSetSaveParam, h_MsSetSaveParam);
        _MsCalcCommand = new FhMethodHandle<MsCalcCommand>(this, GAME, __addr_MsCalcCommand, h_MsCalcCommand);
        _MsBattleEncountExe = new FhMethodHandle<MsBattleEncountExe>(this, GAME, __addr_MsBattleEncountExe, h_MsBattleEncountExe);
    }

    public override bool init(FhModContext mod_context, FileStream global_state_file) {
        _mod_context = mod_context;
        _global_state = global_state_file;

        return _MsMonsterCapture.hook()
            && _FUN_00783bb0.hook()
            && _AtelEventSetUp.hook()
            && _ret_hasKeyItem.hook()
            && _MsDamageCheckDeath.hook()
            && _MsSetRamChrParam.hook()
            && _MsSetSaveParam.hook()
            && _MsCalcCommand.hook()
            && _MsBattleEncountExe.hook();
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
        bool captured = _MsMonsterCapture.orig_fptr(target_id, arena_idx);

        _logger.Info($"Fiend Capture: Target={target_id}, Arena Index={arena_idx}, Captured={captured}");

        // Send AP Location if successfully captured
        if (captured) {
            if (sendLocation(arena_idx, ArchipelagoLocationType.Capture) && ArchipelagoFFXModule.item_locations.capture.TryGetValue(arena_idx, out var item)) {
                ArchipelagoFFXModule.obtain_item(item.id);
            }

            int amount = save_data->monsters_captured[arena_idx];
            lock (FFXArchipelagoClient.client_lock) {
                if (FFXArchipelagoClient.is_connected) {
                    if (amount > 0)
                        FFXArchipelagoClient.current_session!.DataStorage[Scope.Slot, "FFX_CAPTURE_" + arena_idx] = amount;
                    else
                        FFXArchipelagoClient.current_session!.DataStorage[Scope.Slot, "FFX_CAPTURE_" + arena_idx] = 0;
                }
            }
        }
        return captured;
    }

    private HashSet<ushort> initialized_monsters = [];
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

        _FUN_00783bb0.orig_fptr(mon_idx);

        Chr* mon = _MsGetMon(mon_idx);
        if (initialized_monsters.Add(mon->chr_id)) {
            MonStats* stats = (MonStats*)mon->ptr_base_stats;

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
        _AtelEventSetUp.orig_fptr(event_id);

        _event_name = Marshal.PtrToStringAnsi((nint)get_event_name((uint)event_id))!;
        _logger.Debug($"atel_event_setup: {_event_name}");
        byte* code_ptr = Globals.Atel.controllers[0].worker(0)->code_ptr;

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
    private int h_ret_hasKeyItem(AtelBasicWorker* work, int* storage, AtelStack* atelStack) {
        if (_event_name == "nagi0700") {
            int item_id = atelStack->pop_int();

            if (item_id == 0xA028) {
                return local_checked_locations.Contains(276 | (long)ArchipelagoLocationType.Treasure) ? 1 : 0;
            }
            else {
                atelStack->push_int(item_id);
            }
        }

        return _ret_hasKeyItem.orig_fptr(work, storage, atelStack);
    }

    private int h_MsDamageCheckDeath(int attacker_id, int target_id, int param_3, uint param_4) {
        Chr* target = _MsGetChr((uint)target_id);
        MonStats* mon_stats = (MonStats*)target->ptr_base_stats;

        ushort capture_index = mon_stats is not null ? mon_stats->monster_arena_idx : (ushort)0xFF;

        if (ArchipelagoFFXModule.seed.Options.AlwaysCapture == 1
         && ArchipelagoFFXModule.seed.Options.CaptureDamage == 2
         && target_id >= 20 // Make sure to not capture ourselves
         && capture_index != 0xFF // Make sure to not capture rifles
         && Battle.btl->battle_type == 0 // ... and Monster Arena enemies
        ) {
            target->should_try_capture = true;
        }

        return _MsDamageCheckDeath.orig_fptr(attacker_id, target_id, param_3, param_4);
    }

    private void h_MsSetRamChrParam(uint chr_id) {
        _MsSetRamChrParam.orig_fptr(chr_id);

        Chr* chr = _MsGetChr(chr_id);

        if (ArchipelagoFFXModule.seed.Options.AlwaysCapture == 1) {
            chr->ram.auto_ability_effects.has_capture = true;
        }
    }

    private void h_MsSetSaveParam(uint chr_id) {
        _MsSetSaveParam.orig_fptr(chr_id);

        // Does nothing??
        if (ArchipelagoFFXModule.seed.Options.AlwaysCapture == 1) {
            save_data->ply_saves[(int)chr_id].auto_ability_effects.has_capture = true;
        }
    }

    private void h_MsCalcCommand(AttackCue* param_1, int param_2) {
        _MsCalcCommand.orig_fptr(param_1, param_2);
        if (param_1 == null) return;

        uint local_6c;
        Command* command = _MsGetCommand(param_1->attacker_id, 0, -1, &param_1->command_list[param_2], &local_6c);

        if (param_1->command_count <= param_2 || command == null) return;

        Chr* attacker = _MsGetChr(param_1->attacker_id);

        int[] local_7c = [0, 0, 0, param_2];
        if (command->absorbs_dmg) {
            local_7c[2] = (int)_FUN_0078d100(attacker);
        }

        //TODO: Figure out a way not to duplicate affection in _FUN_0078bb30
        byte[] targets = new byte[32];
        byte[] local_48 = new byte[32];
        fixed (byte* p_targets = targets) {
            fixed (byte* p_local_48 = local_48) {
                fixed (int* p_local_7c = local_7c) {
                    _FUN_0078bb30(param_1->attacker_id, p_targets, p_local_48, command, local_6c, &param_1->command_list[param_2].targets, p_local_7c + 1);
                }
            }
        }

        for (uint target_id = 0; target_id < 32; target_id++) {
            if (targets[target_id] != 0) {
                if (local_7c[2] == 0 || target_id != param_1->attacker_id) {
                    Chr* target = _MsGetChr(target_id);
                    uint iVar6 = _FUN_0078d100(target);
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

    private int h_MsBattleEncountExe(int field_id, int group_idx, float walked_delta) {
        _logger.Info($"MsBattleEncountExe(0x{field_id:X}, {group_idx}, {walked_delta})");

        // Globals
        int* g_keybattle = FhUtil.ptr_at<int>(0xD2CA24);
        int* g_keydown_R = FhUtil.ptr_at<int>(0xD2CA20);
        int* DAT_0112ca28 = FhUtil.ptr_at<int>(0xD2CA28);
        int* g_encounter_level = FhUtil.ptr_at<int>(0x8421C8);
        int* EnableBattle = FhUtil.ptr_at<int>(0x8421BC);

        _logger.Info($"  g_keybattle = {*g_keybattle}");
        _logger.Info($"  g_keydown_R = {*g_keydown_R}");
        _logger.Info($"  DAT_0112ca28 = {*DAT_0112ca28}");
        _logger.Info($"  g_encounter_level = {*g_encounter_level}");
        _logger.Info($"  EnableBattle = {*EnableBattle}");

        int field_idx = _MsBtlListFieldNum(field_id);

        if (*g_keybattle != 0 && *g_keydown_R != 0) {
            Battle.btl->group_idx = (byte)group_idx;
            Battle.btl->formation_idx = (byte)*DAT_0112ca28;
            *(byte*)((nint)Battle.btl + 0x12) = 1;
            Battle.btl->field_idx = (ushort)field_idx;
            _ResetEncountExe(1);
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

        if (*(byte*)((nint)Battle.btl + 0x12) == 2) return 0;
        if (field_idx < 0) return 0;
        if (walked_delta <= 0.0f || !can_encounter) return 0;

        BtlBinField* field = _MsBtlListField(field_idx);
        BtlBinEncounter* encounter = _MsBtlListEncount(field_idx);

        if (group_idx < 0 || group_idx >= encounter->group_count) return 0;
        if (*EnableBattle != 0) return 0;

        BtlBinGroup* group = _MsBtlListGroup(field_idx, group_idx);

        _logger.Info( "  Group:");
        _logger.Info($"    formation_count: {group->formation_count}");
        _logger.Info($"    battlefield: {group->battlefield:X}");
        _logger.Info($"    grace: {group->grace}");
        _logger.Info($"    total_weight: {group->total_weight}");

        if (group->grace == 0 || group->total_weight == 0) return 0;

        Battle.btl->walked_dist += walked_delta;
        Battle.btl->walked_dist_total += walked_delta;
        Battle.btl->grace = group->grace;

        int rolls_so_far = (int)(Battle.btl->walked_dist_total / 10.0f);

        _logger.Info($"  rolls_so_far: {rolls_so_far}");
        _logger.Info($"  rolls_to_do: {(int)(Battle.btl->walked_dist / 10.0f)}");
        _logger.Info( "  Rolls:");

        for (; 10.0f < Battle.btl->walked_dist; Battle.btl->walked_dist -= 10.0f) {
            if (group->grace / 2 >= rolls_so_far) continue;

            int encounter_chance = (rolls_so_far - (group->grace / 2)) * 256 / (group->grace * 4);
            *(float*)((nint)Battle.btl + 0x110) *= 256.0f - encounter_chance;
            *(float*)((nint)Battle.btl + 0x114) *= 256.0f;

            _logger.Info($"  encounter_chance: {encounter_chance}");
            _logger.Info($"  new btl__0x110: {*(float*)((nint)Battle.btl + 0x110)}");
            _logger.Info($"  new btl__0x114: {*(float*)((nint)Battle.btl + 0x114)}");

            if ((_brnd(0) & 0xFF) >= encounter_chance) continue;

            int formation_rng = _brnd(1);
            int iVar7 = 0;

            for (int formation_idx = 0; formation_idx < group->formation_count; formation_idx++) {
                BtlBinFormation formation = group->formations[formation_idx];

                iVar7 += formation.weight / 16;
                if (!(formation_rng % group->total_weight < iVar7)) continue;

                string file_name = $"{new string(field->name)}{group_idx}_{formation_idx}";
                _logger.Info($"  Rolled Formation #{formation_idx} ({file_name})!");

                string filepath = $"host0:/ffx/master/jppc/battle/btl/{file_name}/{file_name}.bin";
                char[] filepath_c = filepath.ToCharArray();

                fixed (char* filepath_ptr = &filepath_c[0]) {
                    void* open_result = _sceOpen(filepath_ptr, 1);

                    if (open_result is null) goto IGNORE_FILE;

                    int file_size = _sceLseek(open_result, 0, 2);
                    _sceLseek(open_result, 0, 0);

                    void* file = NativeMemory.Alloc((nuint)file_size);
                    _sceRead(open_result, file, file_size);

                    int chunk_ptr = *(int*)((nint)file + 0xC);
                    if (chunk_ptr != 0) {
                        short* mon_ids_ptr = (short*)((nint)file + chunk_ptr + 0x8);
                        List<string> mon_ids = new(8);

                        for (int i = 0; i < 8; i++) {
                            short mon_id = mon_ids_ptr[i];
                            if (mon_id == -1) continue;

                            mon_ids.Add($"{mon_id:X4}");
                        }

                        _logger.Info($"    Monsters: {string.Join(", ", mon_ids)}");
                    }

                    _sceClose(open_result);
                }

IGNORE_FILE:
                if (*(byte*)((nint)Battle.btl + 0x27) == 0) {
                    *(byte*)((nint)Battle.btl + 0x12) = 1;
                } else {
                    _Sg_FadeInW(3);
                    save_data->battle_count += 1;
                }

                Battle.btl->field_idx = (ushort)field_idx;
                Battle.btl->group_idx = (byte)group_idx;
                Battle.btl->formation_idx = (byte)formation_idx;
                _ResetEncountExe(1);

                return -1;
            }
        }

        return 0;
    }
}
