using Archipelago.MultiClient.Net.Enums;
using Fahrenheit.Atel;
using Fahrenheit.FFX;
using Fahrenheit.FFX.Battle;
using Fahrenheit.Modules.ArchipelagoFFX.Client;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using static Fahrenheit.FFX.Globals;
using static Fahrenheit.Modules.ArchipelagoFFX.Client.FFXArchipelagoClient;

namespace Fahrenheit.Modules.ArchipelagoFFX;

[FhLoad(FhGameId.FFX)]
public unsafe partial class CaptureModule : FhModule {
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
            && _MsCalcCommand.hook();
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

            int qty = save_data->monsters_captured[arena_idx];
            lock (FFXArchipelagoClient.client_lock) {
                if (FFXArchipelagoClient.is_connected) {
                    if (qty > 0)
                        FFXArchipelagoClient.current_session!.DataStorage[Scope.Slot, "FFX_CAPTURE_" + arena_idx] = qty;
                    else
                        FFXArchipelagoClient.current_session!.DataStorage[Scope.Slot, "FFX_CAPTURE_" + arena_idx] = 0;
                }
            }
        }
        return captured;
    }

    private HashSet<ushort> initialized_monsters = [];
    private void h_FUN_00783bb0(byte mon_idx) {
        byte num_initialized = FhUtil.get_at<byte>(0xD2CA80);
        if (num_initialized == 0) initialized_monsters.Clear();

        _FUN_00783bb0.orig_fptr(mon_idx);

        Chr* mon = _MsGetMon(mon_idx);
        if (initialized_monsters.Add(mon->chr_id)) {
            MonStats* stats = (MonStats*)mon->ptr_base_stats;
            //logger.Debug($"{mon->chr_id & 0xFFF} stats:  stats=\n{stats->ToString()}");
            if (
                (mon->chr_id & 0xFFF) == 041 ||  // Sahagin
                (mon->chr_id & 0xFFF) == 042 ||  // Sahagin
                (mon->chr_id & 0xFFF) == 043 ||  // Sahagin
                (mon->chr_id & 0xFFF) == 068 ||  // Piranha
                (mon->chr_id & 0xFFF) == 069 ||  // Piranha
                (mon->chr_id & 0xFFF) == 070 ||  // Piranha
                (mon->chr_id & 0xFFF) == 101 ||  // Tros
                (mon->chr_id & 0xFFF) == 155 ||  // Sahagin
                (mon->chr_id & 0xFFF) == 156 ||  // Sahagin Chief
                (mon->chr_id & 0xFFF) == 157 ||  // Garuda
                (mon->chr_id & 0xFFF) == 230 ||  // Garuda
                (mon->chr_id & 0xFFF) == 231 ||  // Dingo
                (mon->chr_id & 0xFFF) == 232 ||  // Water Flan
                (mon->chr_id & 0xFFF) == 233 ||  // Condor
                (mon->chr_id & 0xFFF) == 234     // Ragora - Lancet Tutorial
               )
                stats->monster_arena_idx = 0xFF;

            //ChrLoot* loot = (ChrLoot*)(((int*)mon->ptr_mon_wep_bin)[5] + mon->ptr_mon_wep_bin);
            //
            //loot->drop_chance_equipment = 255;
            //loot->equipment_loot.ability_count = 100; // Guaranteed 4 abilities?
            //loot->equipment_loot.slot_count = 20; // Guaranteed 4 slots
            //
            //for (int chr = 0; chr < 7; chr++) {
            //    // Guaranteed Capture
            //    loot->equipment_loot.abilities_tidus.weapon_abilities[0] = 0x807A;
            //    loot->equipment_loot.abilities_yuna.weapon_abilities[0] = 0x807A;
            //    loot->equipment_loot.abilities_auron.weapon_abilities[0] = 0x807A;
            //    loot->equipment_loot.abilities_kimahri.weapon_abilities[0] = 0x807A;
            //    loot->equipment_loot.abilities_wakka.weapon_abilities[0] = 0x807A;
            //    loot->equipment_loot.abilities_lulu.weapon_abilities[0] = 0x807A;
            //    loot->equipment_loot.abilities_rikku.weapon_abilities[0] = 0x807A;
            //    for (int i = 1; i < 8; i++) {
            //        loot->equipment_loot.abilities_tidus.weapon_abilities[i] = (ushort)(0x8000 + rng.Next(0x81));
            //        loot->equipment_loot.abilities_yuna.weapon_abilities[i] = (ushort)(0x8000 + rng.Next(0x81));
            //        loot->equipment_loot.abilities_auron.weapon_abilities[i] = (ushort)(0x8000 + rng.Next(0x81));
            //        loot->equipment_loot.abilities_kimahri.weapon_abilities[i] = (ushort)(0x8000 + rng.Next(0x81));
            //        loot->equipment_loot.abilities_wakka.weapon_abilities[i] = (ushort)(0x8000 + rng.Next(0x81));
            //        loot->equipment_loot.abilities_lulu.weapon_abilities[i] = (ushort)(0x8000 + rng.Next(0x81));
            //        loot->equipment_loot.abilities_rikku.weapon_abilities[i] = (ushort)(0x8000 + rng.Next(0x81));
            //    }
            //}

        }
        else {
            _logger.Debug($"{mon_idx}: already initialized ({mon->chr_id})");
        }
    }

    private string? _event_name = Marshal.PtrToStringAnsi((nint)get_event_name((uint)event_id))!;
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

    public void h_MsCalcCommand(AttackCue* param_1, int param_2) {
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
}

