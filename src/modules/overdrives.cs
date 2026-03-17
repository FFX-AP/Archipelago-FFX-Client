using Fahrenheit.Atel;
using Fahrenheit.FFX;
using Fahrenheit.FFX.Battle;
using Fahrenheit.FFX.Ids;
using Fahrenheit.Modules.ArchipelagoFFX.Client;
using System.IO;
using System.Runtime.InteropServices;
using static Fahrenheit.FFX.Globals;
using static Fahrenheit.Modules.ArchipelagoFFX.ArchipelagoFFXModule;
using static Fahrenheit.Modules.ArchipelagoFFX.Client.FFXArchipelagoClient;
using static Fahrenheit.Modules.ArchipelagoFFX.delegates;

namespace Fahrenheit.Modules.ArchipelagoFFX;

[FhLoad(FhGameId.FFX)]
public unsafe partial class OverdriveModule : FhModule 
{
    // Fahrenheit-related
    private FhModContext? _mod_context;
    private FileStream? _global_state;

    // Damage Calc
    [StructLayout(LayoutKind.Explicit, Size = 0x2C)]
    private struct DamageInfo
    {
        [FieldOffset(0x00)] public byte                 field0_0x0;
        [FieldOffset(0x01)] public byte                 field1_0x1;
        [FieldOffset(0x02)] public byte                 field2_0x2;
        [FieldOffset(0x03)] public byte                 field3_0x3;
        [FieldOffset(0x04)] public byte                 field4_0x4;
        [FieldOffset(0x06)] public byte                 flags_buffs_mix;
        [FieldOffset(0x07)] public StatusDurationMap    target_status_suffer_turns_left;
        [FieldOffset(0x14)] public StatusPermanentFlags target_status_suffer;
        [FieldOffset(0x16)] public StatusExtraFlags     target_status_suffer_extra;
        [FieldOffset(0x18)] public short                dmg_calc_flags1;
        [FieldOffset(0x1A)] public short                dmg_calc_flags2;
        [FieldOffset(0x1C)] public int                  out_damage_expected;
        [FieldOffset(0x20)] public int                  out_damage_hp;
        [FieldOffset(0x24)] public int                  out_damage_mp;
        [FieldOffset(0x28)] public int                  out_damage_ctb;
    }

    // Chr inner struct
    [StructLayout(LayoutKind.Explicit)]
    private struct Chr__0x774
    {
        [FieldOffset(0x00)] public byte field0_0x0;
        [FieldOffset(0x07)] public byte field7_0x7;
        [FieldOffset(0x18)] DamageInfo  field24_0x18;
    };

    public OverdriveModule()
    {
        const string GAME = "FFX.exe";
        
        _MsGetSaveCommand = new FhMethodHandle<MsGetSaveCommand>(this, GAME, __addr_MsGetSaveCommand, h_MsGetSaveCommand);
        _MsSetRamChrAbility = new FhMethodHandle<MsSetRamChrAbility>(this, GAME, __addr_MsSetRamChrAbility, h_MsSetRamChrAbility);
        _MsLimitTidusLearn = new FhMethodHandle<MsLimitTidusLearn>(this, GAME, __addr_MsLimitTidusLearn, h_MsLimitTidusLearn);
        _MsAfterDamageProcess = new FhMethodHandle<MsAfterDamageProcess>(this, GAME, __addr_MsAfterDamageProcess, h_MsAfterDamageProcess);
        _ret_doesChrKnowCommand = new FhMethodHandle<CT_RetInt>(this, GAME, __addr_ret_doesChrKnowCommand, h_ret_doesChrKnowCommand);
        _ret_teachAbilityToPartyMemberSilently = new FhMethodHandle<CT_RetInt>(this, GAME, __addr_ret_teachAbilityToPartyMemberSilently, h_ret_teachAbilityToPartyMemberSilently);
        _ret_teachAbilityToPartyMemberWithMsg = new FhMethodHandle<CT_RetInt>(this, GAME, __addr_ret_teachAbilityToPartyMemberWithMsg, h_ret_teachAbilityToPartyMemberWithMsg);
    }

    // Helper class for overdrive provider functions
    public static class OverdriveProvider
    {
        private const int overdriveOffset = 0x4000;
        
        public static void provide_overdrive(int chr_id)
        {
            switch (chr_id)
            {
                case PlySaveId.PC_TIDUS:
                    tidus_overdrive();
                    break;
                case PlySaveId.PC_AURON:
                    auron_overdrive();
                    break;
                case PlySaveId.PC_KIMAHRI:
                    kimahri_overdrive();
                    break;
                case PlySaveId.PC_WAKKA:
                    wakka_overdrive();
                    break;
                case PlySaveId.PC_SEYMOUR:
                    seymour_overdrive();
                    break;
                case PlySaveId.PC_VALEFOR:
                    valefor_overdrive();
                    break;
            }
        }

        private static void tidus_overdrive()
        {
            bool hasSpiralCut    = other_inventory.ContainsKey(0x0000 | overdriveOffset);
            bool hasSliceAndDice = other_inventory.ContainsKey(0x0001 | overdriveOffset);
            bool hasEnergyRain   = other_inventory.ContainsKey(0x0002 | overdriveOffset);
            bool hasBlitzAce     = other_inventory.ContainsKey(0x0003 | overdriveOffset);

            save_data->ability_map_limit.has_swordplay    = hasSpiralCut || hasSliceAndDice || hasEnergyRain || hasBlitzAce;

            save_data->ability_map_limit.has_spiral_cut   = hasSpiralCut;
            save_data->ability_map_limit.has_slice_n_dice = hasSliceAndDice;
            save_data->ability_map_limit.has_energy_rain  = hasEnergyRain;
            save_data->ability_map_limit.has_blitz_ace    = hasBlitzAce;
        }

        private static void auron_overdrive()
        {
            bool hasDragonFang     = other_inventory.ContainsKey(0x0004 | overdriveOffset);
            bool hasShootingStar   = other_inventory.ContainsKey(0x0005 | overdriveOffset);
            bool hasBanishingBlade = other_inventory.ContainsKey(0x0006 | overdriveOffset);
            bool hasTornado        = other_inventory.ContainsKey(0x0007 | overdriveOffset);

            save_data->ability_map_limit.has_bushido         = hasDragonFang || hasShootingStar || hasBanishingBlade || hasTornado;

            save_data->ability_map_limit.has_dragon_fang     = hasDragonFang;
            save_data->ability_map_limit.has_shooting_star   = hasShootingStar;
            save_data->ability_map_limit.has_banishing_blade = hasBanishingBlade;
            save_data->ability_map_limit.has_tornado         = hasTornado;
        }

        private static void kimahri_overdrive()
        {
            bool hasJump         = other_inventory.ContainsKey(0x0008 | overdriveOffset);
            bool hasFireBreath   = other_inventory.ContainsKey(0x0009 | overdriveOffset);
            bool hasSeedCannon   = other_inventory.ContainsKey(0x000A | overdriveOffset);
            bool hasSelfDestruct = other_inventory.ContainsKey(0x000B | overdriveOffset);
            bool hasThrustKick   = other_inventory.ContainsKey(0x000C | overdriveOffset);
            bool hasStoneBreath  = other_inventory.ContainsKey(0x000D | overdriveOffset);
            bool hasAquaBreath   = other_inventory.ContainsKey(0x000E | overdriveOffset);
            bool hasDoom         = other_inventory.ContainsKey(0x000F | overdriveOffset);
            bool hasWhiteWind    = other_inventory.ContainsKey(0x0010 | overdriveOffset);
            bool hasBadBreath    = other_inventory.ContainsKey(0x0011 | overdriveOffset);
            bool hasMightyGuard  = other_inventory.ContainsKey(0x0012 | overdriveOffset);
            bool hasNova         = other_inventory.ContainsKey(0x0013 | overdriveOffset);

            save_data->ability_map_limit.has_ronso_rage    = hasJump       || hasFireBreath  || hasSeedCannon  || hasSelfDestruct ||
                                                             hasThrustKick || hasStoneBreath || hasAquaBreath  || hasDoom         ||
                                                             hasWhiteWind  || hasBadBreath   || hasMightyGuard || hasNova;

            save_data->ability_map_limit.has_jump          = hasJump;
            save_data->ability_map_limit.has_fire_breath   = hasFireBreath;
            save_data->ability_map_limit.has_seed_cannon   = hasSeedCannon;
            save_data->ability_map_limit.has_self_destruct = hasSelfDestruct;
            save_data->ability_map_limit.has_thrust_kick   = hasThrustKick;
            save_data->ability_map_limit.has_stone_breath  = hasStoneBreath;
            save_data->ability_map_limit.has_aqua_breath   = hasAquaBreath;
            save_data->ability_map_limit.has_doom          = hasDoom;
            save_data->ability_map_limit.has_white_wind    = hasWhiteWind;
            save_data->ability_map_limit.has_bad_breath    = hasBadBreath;
            save_data->ability_map_limit.has_mighty_guard  = hasMightyGuard;
            save_data->ability_map_limit.has_nova          = hasNova;
        }

        private static void wakka_overdrive()
        {
            bool hasElementReels = other_inventory.ContainsKey(0x0014 | overdriveOffset);
            bool hasAttackReels  = other_inventory.ContainsKey(0x0015 | overdriveOffset);
            bool hasStatusReels  = other_inventory.ContainsKey(0x0016 | overdriveOffset);
            bool hasAurochsReels = other_inventory.ContainsKey(0x0017 | overdriveOffset);

            save_data->ability_map_limit.has_slots         = hasElementReels || hasAttackReels || hasStatusReels || hasAurochsReels;

            save_data->ability_map_limit.has_element_reels = hasElementReels;
            save_data->ability_map_limit.has_attack_reels  = hasAttackReels;
            save_data->ability_map_limit.has_status_reels  = hasStatusReels;
            save_data->ability_map_limit.has_aurochs_reels = hasAurochsReels;
        }

        private static void seymour_overdrive()
        {
            bool hasRequiem      = other_inventory.ContainsKey(0x0083 | overdriveOffset);

            save_data->ability_map_limit.has_requiem = hasRequiem;
        }

        private static void valefor_overdrive()
        {
            bool hasEnergyBlast  = other_inventory.ContainsKey(0x00CD | overdriveOffset);

            save_data->ability_map_limit.has_energy_blast = hasEnergyBlast;
        }
    }

    public override bool init(FhModContext mod_context, FileStream global_state_file)
    {
        _mod_context  = mod_context;
        _global_state = global_state_file;

        return _MsGetSaveCommand.hook()
            && _MsSetRamChrAbility.hook()
            && _MsLimitTidusLearn.hook()
            && _MsAfterDamageProcess.hook()
            && _ret_doesChrKnowCommand.hook()
            && _ret_teachAbilityToPartyMemberSilently.hook()
            && _ret_teachAbilityToPartyMemberWithMsg.hook();
    }

    private static T* ptr_at<T>(nint address) where T : unmanaged { return (T*)(address); }
    private static T get_at<T>(nint address) where T : unmanaged { return *ptr_at<T>(address); }
    private static T set_at<T>(nint address, T value) where T : unmanaged { return *ptr_at<T>(address) = value; }

    // When game is attempting to give a character an overdrive, instead call the relevant overdrive provider function
    private int h_MsGetSaveCommand(int chr_id, uint com_id)
    {
        if (com_id is >= PlayerCommandId.PCOM_SPIRAL_CUT and <= PlayerCommandId.PCOM_AUROCHS_REELS 
            || com_id == PlayerCommandId.PCOM_REQUIEM
            || com_id == PlayerCommandId.PCOM_ENERGY_BLAST)
        {
            OverdriveProvider.provide_overdrive(chr_id);
        }

        return _MsGetSaveCommand.orig_fptr(chr_id, com_id);
    }

    // When game is attempting to set character abilities, ensure the correct overdrive provider is called first
    private void h_MsSetRamChrAbility(int chr_id, Chr* chr)
    {
        OverdriveProvider.provide_overdrive(chr_id);
        
        _MsSetRamChrAbility.orig_fptr(chr_id, chr);
        return;
    }

    // Runs on every Tidus Limit. Override the normal requirements, and send locations based on 10 / 20 / 40
    private int h_MsLimitTidusLearn(int chr_id)
    {
        if (chr_id != PlySaveId.PC_TIDUS)
            return 0;

        uint tidusLimitUses = ++save_data->tidus_limit_uses;

        if (tidusLimitUses >= 10)
            send_overdrive(PlayerCommandId.PCOM_SLICE_AND_DICE);
        if (tidusLimitUses >= 20)
            send_overdrive(PlayerCommandId.PCOM_ENERGY_RAIN);
        if (tidusLimitUses >= 40)
            send_overdrive(PlayerCommandId.PCOM_BLITZ_ACE);
        
        return 0;
    }

    // Called multiple times on every instance damage. Complete reimplementation in order to interject on Kimahri's overdrive learning
    private uint h_MsAfterDamageProcess(int attacker_id, uint param_2, int target_id, uint* param_4, uint param_5)
    {
        byte bVar1;
        byte bVar2;
        byte bVar3;
        StatusPermanentFlags SVar5;
        StatusPermanentFlags SVar6;
        int iVar7;
        Chr* pCVar8;
        uint uVar9;
        DamageInfo* pcVar11;
        int iVar10;
        int iVar11;
        int local_20;
        Chr__0x774* pcVar1;
        ushort rage_to_learn;

        uint uVar12 = 0;
        DamageInfo* local_30 = (DamageInfo*)0x0;
        Chr* attacker = _MsGetChr(attacker_id);
        Chr* target = _MsGetChr(target_id);
        //byte* pcVar10 = &target->field1402_0x774[0].field_0x7;
        byte* pcVar10 = (byte*)((int)target + 0x774 + 7);
        int local_28 = 2;

        do
        {
            if (((uint)pcVar10[-5] == attacker_id) && (pcVar10[-4] == param_2))
            {
                //target->field_0xf5e = (byte)((byte)param_5 & 0x7f);
                set_at<byte>((int)target + 0xF5E, (byte)((byte)param_5 & 0x7f));
                //target->field1589_0xded = (byte)attacker_id;
                set_at<byte>((int)target + 0xDED, (byte)attacker_id);
                if ((param_5 & 8) == 0)
                {
                    if (*pcVar10 != 0)
                    {
                        _MsMenuCloseTitleWindow(0);
                        *pcVar10 = 0;
                        _MsMessageCueRegist(0x4, *pcVar10 + 3, *pcVar10 + 1, 0x1b, 0x23);
                        if (0 < *(short*)(pcVar10 + 1))
                        {
                            _MsSetStealEffect(target_id, (int)pcVar10[-6]);
                            _MsRegSEplay2(target_id, 0x41);
                        }
                        pcVar10[-2] |= 1;
                    }
                    if (pcVar10[5] != 0)
                    {
                        _MsMenuCloseTitleWindow(0);
                        pcVar10[5] = 0;
                        _MsMessageCueRegist(0x8, *(int*)(pcVar10 + 9), 0, 0x1b, 0x23);
                        if (0 < *(int*)(pcVar10 + 9))
                        {
                            _MsPayGIL(-*(int*)(pcVar10 + 9));
                            _MsSetStealGillEffect(target_id, (int)pcVar10[-6]);
                            _MsRegSEplay2(target_id, 0x41);
                        }
                        pcVar10[-2] |= 1;
                    }
                }
                if (pcVar10[-3] != 0)
                {
                    if ((pcVar10[-3] & 1) != 0)
                    {
                        bVar1 = (attacker->ram).limit_charge;
                        (attacker->ram).limit_charge = 0;
                        iVar7 = _MsCheckRange(target->ram.limit_charge + bVar1, 0, target->ram.limit_charge_max);
                        (target->ram).limit_charge = (byte)iVar7;
                    }
                    if ((pcVar10[-3] & 2) != 0)
                    {
                        pCVar8 = _MsGetChr(pcVar10[0x10]);
                        if (target_id == 3 && pCVar8->loot != (ChrLoot*)0x0)
                        {
                            rage_to_learn = pCVar8->loot->ronso_rage;
                            if (rage_to_learn != 0)
                            {
                                iVar7 = _MsGetSaveCommand(3, rage_to_learn) ? 1 : 0;
                                if (iVar7 == 0)
                                {
                                    //_MsSetSaveCommand(3, rage_to_learn, 1);
                                    //_MsMessageCueRegist(0x1, 3, rage_to_learn, 0x1e, 0x32);

                                    send_overdrive(rage_to_learn);                                    

                                    target->ram.limit_charge = target->ram.limit_charge_max;

                                    if (save_data->ability_map_limit.has_jump         && save_data->ability_map_limit.has_fire_breath   &&
                                        save_data->ability_map_limit.has_seed_cannon  && save_data->ability_map_limit.has_self_destruct &&
                                        save_data->ability_map_limit.has_thrust_kick  && save_data->ability_map_limit.has_stone_breath  &&
                                        save_data->ability_map_limit.has_aqua_breath  && save_data->ability_map_limit.has_doom          &&
                                        save_data->ability_map_limit.has_white_wind   && save_data->ability_map_limit.has_bad_breath    &&
                                        save_data->ability_map_limit.has_mighty_guard && save_data->ability_map_limit.has_nova)
                                    {
                                        _achievementUnlockAchievement(0x19);
                                    }
                                }
                            }
                        }
                    }
                    pcVar10[-3] = 0;
                }
                pcVar1 = (Chr__0x774*)(pcVar10 + -7);
                if (pcVar10[-7] < pcVar10[-6])
                {
                    //pcVar11 = (DamageInfo*)&pcVar1->field20_0x14[pcVar10[-7]].field4_0x4;
                    pcVar11 = ptr_at<DamageInfo>((int)pcVar1 + 0x18) + get_at<byte>((int)pcVar1);
                    local_20 = 0;
                    if ((param_5 & 8) == 0)
                    {
                        iVar7 = pcVar11->field1_0x1;
                        uVar9 = pcVar11->field2_0x2;
                        if (iVar7 == 1)
                        {
                            iVar11 = 1;
                            iVar10 = 3;
                            _MsNumberRegist(target_id, iVar10, 0, 0, iVar11, uVar9, 0x81);
                        }
                        else if (iVar7 == 2)
                        {
                            iVar11 = 2;
                            iVar10 = 4;
                            _MsNumberRegist(target_id, iVar10, 0, 0, iVar11, uVar9, 0x81);
                        }
                        _MsLimitTypeDamageCheck(attacker_id, attacker, target_id, target, pcVar11->out_damage_hp, pcVar11->out_damage_expected, pcVar10[-1]);

                        if ((pcVar11->dmg_calc_flags1 & 1) != 0)
                        {
                            _MsSubHP(target_id, target, pcVar11->out_damage_hp, pcVar11->out_damage_mp, iVar7, uVar9, 0x81);

                            //target->field1960_0xf60 = target->field1960_0xf60 - iVar10;
                            set_at((int)target + 0xF60, get_at<int>((int)target + 0xF60) - pcVar11->out_damage_hp);

                            local_20 += pcVar11->out_damage_hp;
                        }
                        if ((pcVar11->dmg_calc_flags1 & 2) != 0)
                        {
                            _MsSubMP(target_id, target, pcVar11->out_damage_mp, pcVar11->out_damage_hp, iVar7, uVar9, 0x81);
                        }
                        if ((pcVar11->dmg_calc_flags1 & 4) != 0)
                        {
                            _MsSubCTB(target_id, target, pcVar11->out_damage_ctb, iVar7, uVar9, 0x81);
                            //dbgPrintf("CTB DAMAGE %d %d : %d\n", target_id, iVar10, (target->ram).ctb);
                        }

                        _MsLimitTypeStatusCheck(attacker_id, attacker, target_id, target, pcVar11->field4_0x4, pcVar11->field3_0x3);
                        SVar5 = (target->ram).status_suffer;
                        bVar1 = (target->ram).status_suffer_turns_left.darkness;
                        bVar2 = (target->ram).status_suffer_turns_left.silence;
                        bVar3 = (target->ram).status_suffer_turns_left.regen;
                        StatusExtraFlags bVar4 = target->ram.status_suffer_extra;
                        target->ram.status_suffer = pcVar11->target_status_suffer;
                        for (int i = 0; i < 0xD; i++)
                        {
                            (&target->ram.status_suffer_turns_left.sleep)[i] = (&pcVar11->target_status_suffer_turns_left.sleep)[i];
                        }

                        target->ram.status_suffer_extra = pcVar11->target_status_suffer_extra;
                        _MsLimitStatusProcess(target_id, target, pcVar11->flags_buffs_mix);

                        if (((target->ram).status_suffer_turns_left.regen != 0) && (bVar3 == 0))
                        {
                            (target->ram).regen_strength = 0;
                        }

                        SVar6 = target->ram.status_suffer;
                        //target->field_0xdce = (byte)SVar6 >> 2 & 1;
                        set_at<bool>((int)target + 0xDCE, SVar6.petrification());
                        if (target->ram.status_suffer.death() != SVar5.death())
                        {
                            _MsAliveProcess(target_id, target);
                        }
                        if (target->ram.status_suffer.petrification() != SVar5.petrification())
                        {
                            _MsStoneProcess(target_id, target);
                        }
                        if (target->ram.status_suffer_extra.eject() != bVar4.eject())
                        {
                            _MsBlowProcess(target_id, target);
                        }
                        if (target->ram.status_suffer.threaten() != SVar5.threaten())
                        {
                            _MsThreatProcess(target_id, target);
                        }
                        pcVar10[-2] |= 1;
                        if (target->ram.auto_ability_effects.has_auto_med)
                        {
                            _MsAutoCureProcess(target_id, target, attacker_id, (int)SVar5 >> 3 & 1, (int)SVar5 >> 1 & 1, bVar1, bVar2);
                        }
                        if ((0 < local_20) && (target->ram.auto_ability_effects.has_auto_potion))
                        {
                            _MsAutoPotionProcess(target_id, target, attacker_id);
                        }
                        _MsSetChrWeak(target_id, -1);
                        uVar12 = uVar12 | 2;
                        pcVar1->field0_0x0 += 1;
                    }
                    //if (target->ram.field_0x19c != '\0')
                    if (get_at<bool>((int)&target->ram + 0x19C))
                    {
                        _MsAutoRelifeProcess(attacker_id, attacker, target_id, target);
                    }
                    if (((param_5 & 2) == 0) && ((pcVar11->field0_0x0 != 1 || ((pcVar10[-2] & 4) == 0))))
                    {
                        pcVar10[-2] |= 4;
                        local_30 = pcVar11;
                    }
                    if ((param_5 & 0x10) == 0)
                    {
                        _MsStatusEffectCheck(target_id);
                        if (_MsStatusDefenseEffect(attacker_id, target_id, pcVar11->dmg_calc_flags1) != 0)
                        {
                            *param_4 = (uint)target_id;
                        }
                    }
                }
                if (pcVar10[-7] < pcVar10[-6])
                {
                    uVar12 = uVar12 | 1;
                }
                else
                {
                    if (((pcVar10[-2] & 1) != 0) && ((pcVar10[-2] & 2) == 0))
                    {
                        //target->ram.field_0x19d = 0;
                        get_at<bool>((int)&target->ram + 0x19D);
                        pcVar10[-2] |= 2;
                        _MsActionRequest(target_id, attacker_id, 3, 0, 1, null);
                    }
                    if ((param_5 & 0x400) == 0)
                    {
                        _MsPopBtlPos(target);
                    }
                    else
                    {
                        pcVar10[-5] = 0xff;
                    }
                }
            }
            pcVar10 = pcVar10 + 0x2d8;
            local_28 = local_28 + -1;
        } while (local_28 != 0);
        if ((uVar12 & 1) == 0 && _MsDamageCheckDeath(attacker_id, target_id, 0, (attacker_id != target_id) ? 1 : 0) != 0)
        {
            return uVar12;
        }
        if (local_30 == (DamageInfo*)0x0)
        {
            return uVar12;
        }
        iVar7 = (int)(char)local_30->field0_0x0;
        if ((param_5 & 0x20) == 0)
        {
            _MsDamageSetMotion(target_id, iVar7, (attacker_id != target_id) ? 1 : 0);
            return uVar12;
        }
        if (iVar7 != 5)
        {
            if (iVar7 == 6)
            {
                uVar9 = (uint)_brnd(9);
                iVar7 = (int)(uVar9 & 1) + 0xf;
                _MsDamageSetMotion(target_id, iVar7, (attacker_id != target_id) ? 1 : 0);
                return uVar12;
            }
            if (iVar7 != 8)
            {
                _MsDamageSetMotion(target_id, iVar7, (attacker_id != target_id) ? 1 : 0);
                return uVar12;
            }
        }
        uVar9 = (uint)_brnd(9);
        iVar7 = (int)(uVar9 & 3) + 0xd;
        _MsDamageSetMotion(target_id, iVar7, (attacker_id != target_id) ? 1 : 0);
        return uVar12;
    }

    // Required in order to allow Biran & Yenke's ChrLoot to progress to the next Ronso Rage based on location sent, rather than command known
    private int h_ret_doesChrKnowCommand(AtelBasicWorker* work, int* storage, AtelStack* atelStack)
    {
        int com_id = atelStack->pop_int();
        int chr_id = atelStack->pop_int();

        if (chr_id == PlySaveId.PC_KIMAHRI && 
            com_id is >= PlayerCommandId.PCOM_JUMP and <= PlayerCommandId.PCOM_NOVA)
                return local_checked_locations.Contains(((com_id - PlayerCommandId.PCOM_SPIRAL_CUT) & 0xFF) | (long)ArchipelagoLocationType.Overdrive) ? 1 : 0;

        atelStack->push_int(chr_id);
        atelStack->push_int(com_id);
        return _ret_doesChrKnowCommand.orig_fptr(work, storage, atelStack);
    }

    // Required to interject on receiving Wakka's overdrive from Blitzball
    private int h_ret_teachAbilityToPartyMemberSilently(AtelBasicWorker* work, int* storage, AtelStack* atelStack)
    {
        int com_id = atelStack->pop_int();
        int chr_id = atelStack->pop_int();

        if (chr_id == PlySaveId.PC_WAKKA &&
            (com_id | 0x3000) is >= PlayerCommandId.PCOM_ATTACK_REELS and <= PlayerCommandId.PCOM_AUROCHS_REELS)
        {
            send_overdrive(com_id | 0x3000);
            return chr_id;
        }

        atelStack->push_int(chr_id);
        atelStack->push_int(com_id);
        return _ret_teachAbilityToPartyMemberSilently.orig_fptr(work, storage, atelStack);
    }

    // Required to interject on receiving Valefor's overdrive from Dog
    private int h_ret_teachAbilityToPartyMemberWithMsg(AtelBasicWorker* work, int* storage, AtelStack* atelStack)
    {
        int com_id = atelStack->pop_int();
        int chr_id = atelStack->pop_int();
        int window_idx = atelStack->pop_int();

        if (chr_id == PlySaveId.PC_VALEFOR && 
            (com_id | 0x3000) == PlayerCommandId.PCOM_ENERGY_BLAST)
        {
            send_overdrive(PlayerCommandId.PCOM_ENERGY_BLAST);
        }
        
        atelStack->push_int(window_idx);
        atelStack->push_int(chr_id);
        atelStack->push_int(com_id);

        return _ret_teachAbilityToPartyMemberWithMsg.orig_fptr(work, storage, atelStack);
    }

    public static void send_overdrive(int com_id)
    {
        // Apworld defines Spiral Cut as overdrive location 0, and all other overdrives are as an offset from that value.
        int overdrive_id = com_id - PlayerCommandId.PCOM_SPIRAL_CUT;

        if (!FFXArchipelagoClient.local_checked_locations.Contains(overdrive_id | (long)FFXArchipelagoClient.ArchipelagoLocationType.Overdrive))
        {
            if (ArchipelagoFFXModule.item_locations.overdrive.TryGetValue(overdrive_id, out var item))
            {
                if (FFXArchipelagoClient.sendLocation(overdrive_id, FFXArchipelagoClient.ArchipelagoLocationType.Overdrive))
                {
                    ArchipelagoFFXModule.obtain_item(item.id);
                }
            }
        }
    }
}
