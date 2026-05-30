using Archipelago.MultiClient.Net.Enums;
using Fahrenheit.Atel;
using Fahrenheit.FFX;
using Fahrenheit.FFX.Battle;
using Fahrenheit.FFX.Ids;

using System.IO;
using System.Runtime.InteropServices;

using ArchipelagoFFX.Client;

using Fahrenheit;

using static Fahrenheit.FFX.Globals;
using static ArchipelagoFFX.ArchipelagoFFXModule;
using static ArchipelagoFFX.Client.FFXArchipelagoClient;
using static ArchipelagoFFX.delegates;

namespace ArchipelagoFFX;

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
        [FieldOffset(0x00)] public byte        field0_0x0;
        [FieldOffset(0x01)] public byte        field1_0x1;
        [FieldOffset(0x02)] public byte        field2_0x2;
        [FieldOffset(0x03)] public byte        field3_0x3;
        [FieldOffset(0x04)] public byte        field4_0x4;
        [FieldOffset(0x05)] public byte        field5_0x5;
        [FieldOffset(0x05)] public byte        field6_0x6;
        [FieldOffset(0x07)] public byte        field7_0x7;
        [FieldOffset(0x08)] public short       field8_0x8;
        [FieldOffset(0x0A)] public short       field10_0xA;
        [FieldOffset(0x0C)] public byte        field12_0xC;
        [FieldOffset(0x10)] public int         field16_0x10;
        [FieldOffset(0x17)] public byte        chr_id__0x17;
        [FieldOffset(0x18)] public DamageInfo  field24_0x18;
    };

    public OverdriveModule()
    {
        const string GAME = "FFX.exe";
        
        _MsGetSaveCommand = new FhMethodHandle<MsGetSaveCommand>(this, GAME, __addr_MsGetSaveCommand, h_MsGetSaveCommand);
        _MsSetRamChrAbility = new FhMethodHandle<MsSetRamChrAbility>(this, GAME, __addr_MsSetRamChrAbility, h_MsSetRamChrAbility);
        _MsLimitTidusLearn = new FhMethodHandle<MsLimitTidusLearn>(this, GAME, __addr_MsLimitTidusLearn, h_MsLimitTidusLearn);
        _MsAfterDamageProcess = new FhMethodHandle<MsAfterDamageProcess>(this, GAME, __addr_MsAfterDamageProcess, h_MsAfterDamageProcess);
        _ret_doesChrKnowCommand = new FhMethodHandle<delegates.CT_RetInt>(this, GAME, __addr_ret_doesChrKnowCommand, h_ret_doesChrKnowCommand);
        _MsSetSaveCommandWithPrefix = new FhMethodHandle<MsSetSaveCommandWithPrefix>(this, GAME, __addr_MsSetSaveCommandWithPrefix, h_MsSetSaveCommandWithPrefix);
        _TOBtlDrawLearningMessageWindow = new FhMethodHandle<TOBtlDrawLearningMessageWindow>(this, GAME, __addr_TOBtlDrawLearningMessageWindow, h_TOBtlDrawLearningMessageWindow);
    }

    // Helper class for overdrive provider functions
    public static class OverdriveProvider
    {
        private const int overdriveOffset = 0x3000;
        
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
            bool hasSpiralCut    = other_inventory.ContainsKey(PlayerCommandId.PCOM_SPIRAL_CUT);
            bool hasSliceAndDice = other_inventory.ContainsKey(PlayerCommandId.PCOM_SLICE_AND_DICE);
            bool hasEnergyRain   = other_inventory.ContainsKey(PlayerCommandId.PCOM_ENERGY_RAIN);
            bool hasBlitzAce     = other_inventory.ContainsKey(PlayerCommandId.PCOM_BLITZ_ACE);

            save_data->ability_map_limit.has_swordplay    = hasSpiralCut || hasSliceAndDice || hasEnergyRain || hasBlitzAce;

            save_data->ability_map_limit.has_spiral_cut   = hasSpiralCut;
            save_data->ability_map_limit.has_slice_n_dice = hasSliceAndDice;
            save_data->ability_map_limit.has_energy_rain  = hasEnergyRain;
            save_data->ability_map_limit.has_blitz_ace    = hasBlitzAce;
        }

        private static void auron_overdrive()
        {
            bool hasDragonFang     = other_inventory.ContainsKey(PlayerCommandId.PCOM_DRAGON_FANG);
            bool hasShootingStar   = other_inventory.ContainsKey(PlayerCommandId.PCOM_SHOOTING_STAR);
            bool hasBanishingBlade = other_inventory.ContainsKey(PlayerCommandId.PCOM_BANISHING_BLADE);
            bool hasTornado        = other_inventory.ContainsKey(PlayerCommandId.PCOM_TORNADO);

            save_data->ability_map_limit.has_bushido         = hasDragonFang || hasShootingStar || hasBanishingBlade || hasTornado;

            save_data->ability_map_limit.has_dragon_fang     = hasDragonFang;
            save_data->ability_map_limit.has_shooting_star   = hasShootingStar;
            save_data->ability_map_limit.has_banishing_blade = hasBanishingBlade;
            save_data->ability_map_limit.has_tornado         = hasTornado;
        }

        private static void kimahri_overdrive()
        {
            bool hasJump         = other_inventory.ContainsKey(PlayerCommandId.PCOM_JUMP);
            bool hasFireBreath   = other_inventory.ContainsKey(PlayerCommandId.PCOM_FIRE_BREATH);
            bool hasSeedCannon   = other_inventory.ContainsKey(PlayerCommandId.PCOM_SEED_CANNON);
            bool hasSelfDestruct = other_inventory.ContainsKey(PlayerCommandId.PCOM_SELF_DESTRUCT);
            bool hasThrustKick   = other_inventory.ContainsKey(PlayerCommandId.PCOM_THRUST_KICK);
            bool hasStoneBreath  = other_inventory.ContainsKey(PlayerCommandId.PCOM_STONE_BREATH);
            bool hasAquaBreath   = other_inventory.ContainsKey(PlayerCommandId.PCOM_AQUA_BREATH);
            bool hasDoom         = other_inventory.ContainsKey(PlayerCommandId.PCOM_DOOM);
            bool hasWhiteWind    = other_inventory.ContainsKey(PlayerCommandId.PCOM_WHITE_WIND);
            bool hasBadBreath    = other_inventory.ContainsKey(PlayerCommandId.PCOM_BAD_BREATH);
            bool hasMightyGuard  = other_inventory.ContainsKey(PlayerCommandId.PCOM_MIGHTY_GUARD);
            bool hasNova         = other_inventory.ContainsKey(PlayerCommandId.PCOM_NOVA);

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
            bool hasElementReels = other_inventory.ContainsKey(PlayerCommandId.PCOM_ELEMENT_REELS);
            bool hasAttackReels  = other_inventory.ContainsKey(PlayerCommandId.PCOM_ATTACK_REELS);
            bool hasStatusReels  = other_inventory.ContainsKey(PlayerCommandId.PCOM_STATUS_REELS);
            bool hasAurochsReels = other_inventory.ContainsKey(PlayerCommandId.PCOM_AUROCHS_REELS);

            save_data->ability_map_limit.has_slots         = hasElementReels || hasAttackReels || hasStatusReels || hasAurochsReels;

            save_data->ability_map_limit.has_element_reels = hasElementReels;
            save_data->ability_map_limit.has_attack_reels  = hasAttackReels;
            save_data->ability_map_limit.has_status_reels  = hasStatusReels;
            save_data->ability_map_limit.has_aurochs_reels = hasAurochsReels;
        }

        private static void seymour_overdrive()
        {
            bool hasRequiem      = other_inventory.ContainsKey(PlayerCommandId.PCOM_REQUIEM);

            save_data->ability_map_limit.has_requiem = hasRequiem;
        }

        private static void valefor_overdrive()
        {
            bool hasEnergyBlast  = other_inventory.ContainsKey(PlayerCommandId.PCOM_ENERGY_BLAST);

            save_data->ability_map_limit.has_energy_blast = hasEnergyBlast;
        }

        public static void set_overdrive_modes() {
            for (int i = 0; i <= PlySaveId.PC_SEYMOUR; i++) {
                PlySave* ply_save = &save_data->ply_saves[i];

                for (int n = 0; n <= 16; n++) {
                    (*(uint*)&ply_save->obtained_limit_modes).set_bit(n, true);
                }
            }
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
            && _MsSetSaveCommandWithPrefix.hook()
            && _TOBtlDrawLearningMessageWindow.hook();
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
        lock (FFXArchipelagoClient.client_lock) {
            if (FFXArchipelagoClient.is_connected) {
                FFXArchipelagoClient.current_session!.DataStorage[Scope.Slot, "FFX_TIDUS_OVERDRIVE"] = tidusLimitUses;
            }
        }

        if (tidusLimitUses >= 10) {
            if (send_overdrive(PlayerCommandId.PCOM_SLICE_AND_DICE)) {
                _MsMessageCueRegist(6, PlySaveId.PC_TIDUS, PlayerCommandId.PCOM_SLICE_AND_DICE, 0x1e, 0x32);
            }
        }

        if (tidusLimitUses >= 20) {
            if(send_overdrive(PlayerCommandId.PCOM_ENERGY_RAIN)) {
                _MsMessageCueRegist(6, PlySaveId.PC_TIDUS, PlayerCommandId.PCOM_ENERGY_RAIN, 0x1e, 0x32);
            }
        }

        if (tidusLimitUses >= 40) {
            if(send_overdrive(PlayerCommandId.PCOM_BLITZ_ACE)) {
                _MsMessageCueRegist(6, PlySaveId.PC_TIDUS, PlayerCommandId.PCOM_BLITZ_ACE, 0x1e, 0x32);
            }
        }
        
        return 0;
    }

    // Called multiple times on every instance damage. Complete reimplementation in order to interject on Kimahri's overdrive learning
    private uint h_MsAfterDamageProcess(int attacker_id, uint param_2, int target_id, uint* param_4, uint param_5)
    {
        uint uVar12 = 0;
        DamageInfo* local_30 = (DamageInfo*)0x0;
        Chr* attacker = _MsGetChr(attacker_id);
        Chr* target = _MsGetChr(target_id);
        Chr__0x774* target_0x774 = (Chr__0x774*)((int)target + 0x774);

        for (int n = 2; n > 0; n--)
        {
            if (target_0x774->field2_0x2 == attacker_id && target_0x774->field3_0x3 == param_2)
            {
                //set_at((int)target + 0xF5E, param_5 & 0x7f);
                set_at((int)target + 0xF5E, (byte)param_5.get_bits(0, 7));
                set_at((int)target + 0xDED, (byte)attacker_id);
                if (!param_5.get_bit(3))
                {
                    if (target_0x774->field7_0x7 != 0)
                    {
                        _MsMenuCloseTitleWindow(0);
                        target_0x774->field7_0x7 = 0;
                        //_MsMessageCueRegist(0x4, target_0x774->field7_0x7 + 3, target_0x774->field7_0x7 + 1, 0x1b, 0x23);
                        _MsMessageCueRegist(0x4, target_0x774->field10_0xA, target_0x774->field8_0x8, 0x1b, 0x23);
                        if (0 < target_0x774->field8_0x8)
                        {
                            _MsSetStealEffect(target_id, target_0x774->field1_0x1);
                            _MsRegSEplay2(target_id, 0x41);
                        }

                        target_0x774->field5_0x5 |= 1;
                    }

                    if (target_0x774->field12_0xC != 0)
                    {
                        _MsMenuCloseTitleWindow(0);
                        target_0x774->field12_0xC = 0;
                        _MsMessageCueRegist(0x8, target_0x774->field16_0x10, 0, 0x1b, 0x23);
                        if (0 < target_0x774->field16_0x10)
                        {
                            _MsPayGIL(-*(int*)(target_0x774 + 9));
                            _MsSetStealGillEffect(target_id, target_0x774->field1_0x1);
                            _MsRegSEplay2(target_id, 0x41);
                        }

                        target_0x774->field5_0x5 |= 1;
                    }
                }

                if (target_0x774->field4_0x4 != 0)
                {
                    if (target_0x774->field4_0x4.get_bit(0))
                    {
                        byte bVar1 = attacker->ram.limit_charge;
                        attacker->ram.limit_charge = 0;
                        target->ram.limit_charge = (byte)_MsCheckRange(target->ram.limit_charge + bVar1, 0, target->ram.limit_charge_max);
                    }

                    if (target_0x774->field4_0x4.get_bit(1))
                    {
                        Chr* pCVar8 = _MsGetChr(target_0x774->chr_id__0x17);
                        if (target_id == PlySaveId.PC_KIMAHRI && pCVar8->loot != (ChrLoot*)0x0)
                        {
                            ushort rage_to_learn = pCVar8->loot->ronso_rage;
                            if (rage_to_learn != 0)
                            {
                                if(send_overdrive(rage_to_learn)) {
                                    _MsMessageCueRegist(6, PlySaveId.PC_KIMAHRI, rage_to_learn, 0x1e, 0x32);
                                    target->ram.limit_charge = target->ram.limit_charge_max;
                                }

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

                    target_0x774->field4_0x4 = 0;
                }

                if (target_0x774->field0_0x0 < target_0x774->field1_0x1)
                {
                    DamageInfo* target_0x774_damage_info = &target_0x774->field24_0x18 + target_0x774->field0_0x0;

                    int local_20 = 0;
                    if (!param_5.get_bit(3))
                    {
                        int iVar7 = target_0x774_damage_info->field1_0x1;
                        uint uVar9 = target_0x774_damage_info->field2_0x2;

                        if (iVar7 == 1)
                        {
                            _MsNumberRegist(target_id, 3, 0, 0, 1, uVar9, 0x81);
                        }
                        else if (iVar7 == 2)
                        {
                            _MsNumberRegist(target_id, 4, 0, 0, 2, uVar9, 0x81);
                        }

                        _MsLimitTypeDamageCheck(attacker_id, attacker, target_id, target, target_0x774_damage_info->out_damage_hp, target_0x774_damage_info->out_damage_expected, target_0x774->field6_0x6);

                        if (target_0x774_damage_info->dmg_calc_flags1.get_bit(0))
                        {
                            _MsSubHP(target_id, target, target_0x774_damage_info->out_damage_hp, target_0x774_damage_info->out_damage_mp, iVar7, uVar9, 0x81);

                            set_at((int)target + 0xF60, get_at<int>((int)target + 0xF60) - target_0x774_damage_info->out_damage_hp);

                            local_20 += target_0x774_damage_info->out_damage_hp;
                        }

                        if (target_0x774_damage_info->dmg_calc_flags1.get_bit(1))
                        {
                            _MsSubMP(target_id, target, target_0x774_damage_info->out_damage_mp, target_0x774_damage_info->out_damage_hp, iVar7, uVar9, 0x81);
                        }

                        if (target_0x774_damage_info->dmg_calc_flags1.get_bit(2))
                        {
                            _MsSubCTB(target_id, target, target_0x774_damage_info->out_damage_ctb, iVar7, uVar9, 0x81);
                            //dbgPrintf("CTB DAMAGE %d %d : %d\n", target_id, iVar10, (target->ram).ctb);
                        }

                        _MsLimitTypeStatusCheck(attacker_id, attacker, target_id, target, target_0x774_damage_info->field4_0x4, target_0x774_damage_info->field3_0x3);
                        StatusPermanentFlags SVar5 = target->ram.status_suffer;
                        byte bVar1 = target->ram.status_suffer_turns_left.darkness;
                        byte bVar2 = target->ram.status_suffer_turns_left.silence;
                        byte bVar3 = target->ram.status_suffer_turns_left.regen;
                        StatusExtraFlags bVar4 = target->ram.status_suffer_extra;
                        target->ram.status_suffer = target_0x774_damage_info->target_status_suffer;

                        for (int i = 0; i < 0xD; i++)
                        {
                            // TODO: Improvements pending the availability of indexers from Fahrenheit, as per https://github.com/fahrenheit-crew/fahrenheit/issues/114
                            (&target->ram.status_suffer_turns_left.sleep)[i] = (&target_0x774_damage_info->target_status_suffer_turns_left.sleep)[i];
                        }

                        target->ram.status_suffer_extra = target_0x774_damage_info->target_status_suffer_extra;
                        _MsLimitStatusProcess(target_id, target, target_0x774_damage_info->flags_buffs_mix);

                        if ((target->ram.status_suffer_turns_left.regen != 0) && (bVar3 == 0))
                        {
                            target->ram.regen_strength = 0;
                        }

                        StatusPermanentFlags SVar6 = target->ram.status_suffer;
                        set_at((int)target + 0xDCE, SVar6.petrification());
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

                        target_0x774->field5_0x5 |= 1;
                        if (target->ram.auto_ability_effects.has_auto_med)
                        {
                            _MsAutoCureProcess(target_id, target, attacker_id, (int)SVar5 >> 3 & 1, (int)SVar5 >> 1 & 1, bVar1, bVar2);
                        }

                        if (0 < local_20 && target->ram.auto_ability_effects.has_auto_potion)
                        {
                            _MsAutoPotionProcess(target_id, target, attacker_id);
                        }

                        _MsSetChrWeak(target_id, -1);
                        uVar12 = uVar12 | 2;
                        target_0x774->field0_0x0 += 1;
                    }

                    if (get_at<bool>((int)&target->ram + 0x19C))
                    {
                        _MsAutoRelifeProcess(attacker_id, attacker, target_id, target);
                    }

                    if (!param_5.get_bit(1) && (target_0x774_damage_info->field0_0x0 != 1 || !target_0x774->field5_0x5.get_bit(2)))
                    {
                        target_0x774->field5_0x5 |= 4;
                        local_30 = target_0x774_damage_info;
                    }

                    if (!param_5.get_bit(4))
                    {
                        _MsStatusEffectCheck(target_id);
                        if (_MsStatusDefenseEffect(attacker_id, target_id, target_0x774_damage_info->dmg_calc_flags1) != 0)
                        {
                            *param_4 = (uint)target_id;
                        }
                    }
                }

                if (target_0x774->field0_0x0 < target_0x774->field1_0x1)
                {
                    uVar12 = uVar12 | 1;
                }
                else
                {
                    if (target_0x774->field5_0x5.get_bit(0) && !target_0x774->field5_0x5.get_bit(1))
                    {
                        set_at((int)&target->ram + 0x19D, false);
                        target_0x774->field5_0x5 |= 2;
                        _MsActionRequest(target_id, attacker_id, 3, 0, 1, null);
                    }

                    if (!param_5.get_bit(10))
                    {
                        _MsPopBtlPos(target);
                    }
                    else
                    {
                        target_0x774->field2_0x2 = 0xff;
                    }
                }
            }

            target_0x774 += 1;
        }

        if (!uVar12.get_bit(0) && _MsDamageCheckDeath(attacker_id, target_id, 0, (attacker_id != target_id) ? 1 : 0) != 0)
        {
            return uVar12;
        }

        if (local_30 == (DamageInfo*)0x0)
        {
            return uVar12;
        }

        if (!param_5.get_bit(5))
        {
            _MsDamageSetMotion(target_id, local_30->field0_0x0, (attacker_id != target_id) ? 1 : 0);
            return uVar12;
        }

        if (local_30->field0_0x0 != 5)
        {
            if (local_30->field0_0x0 == 6)
            {
                _MsDamageSetMotion(target_id, _brnd(9).get_bit(0) ? (byte)0x10 : (byte)0xF, (attacker_id != target_id) ? 1 : 0);
                return uVar12;
            }

            if (local_30->field0_0x0 != 8)
            {
                _MsDamageSetMotion(target_id, local_30->field0_0x0, (attacker_id != target_id) ? 1 : 0);
                return uVar12;
            }
        }

        _MsDamageSetMotion(target_id, _brnd(9).get_bits(0, 2) + 0xD, (attacker_id != target_id) ? 1 : 0);
        return uVar12;
    }

    // Required in order to allow Biran & Yenke's ChrLoot to progress to the next Ronso Rage based on location sent, rather than command known
    private int h_ret_doesChrKnowCommand(AtelBasicWorker* work, int* storage, AtelStack* atelStack)
    {
        int com_id = atelStack->pop_int();
        int chr_id = atelStack->pop_int();

        if (chr_id == PlySaveId.PC_KIMAHRI && 
            com_id is >= PlayerCommandId.PCOM_JUMP and <= PlayerCommandId.PCOM_NOVA)
        {
            return local_checked_locations.Contains(((com_id - PlayerCommandId.PCOM_SPIRAL_CUT) & 0xFF) | (long)FFXArchipelagoClient.ArchipelagoLocationType.Overdrive) ? 1 : 0;
        }

        atelStack->push_int(chr_id);
        atelStack->push_int(com_id);
        return _ret_doesChrKnowCommand.orig_fptr(work, storage, atelStack);
    }

    // Called from teachAbilityToPartyMemberSilently & teachAbilityToPartyMemberWithMsg
    private void h_MsSetSaveCommandWithPrefix(int chr_id, int com_id, int param_3) {
        if ((chr_id == PlySaveId.PC_WAKKA && (com_id | 0x3000) is >= PlayerCommandId.PCOM_ATTACK_REELS and <= PlayerCommandId.PCOM_AUROCHS_REELS) ||
            (chr_id == PlySaveId.PC_VALEFOR && (com_id | 0x3000) == PlayerCommandId.PCOM_ENERGY_BLAST)) {
            send_overdrive(com_id | 0x3000);
            return;
        }

        _MsSetSaveCommandWithPrefix.orig_fptr(chr_id, com_id, param_3);
        return;
    }

    private int h_TOBtlDrawLearningMessageWindow(int chr_id, int com_id) {
        byte* data_end;

        byte* chr_name = _TOGetSaveChrName(chr_id);
        _TOBtlSetMacroCommandType(7, 0, 0);
        _TOBtlSetMacroCommandValue(7, 0, chr_name);

        Command* com = _MsGetComData(com_id, &data_end);
        ushort com_name_offset = com->name_offset;

        _TOBtlSetMacroCommandType(7, 1, 0);

        if (item_locations.overdrive.TryGetValue(com_id - PlayerCommandId.PCOM_SPIRAL_CUT, out var item)) {
            ArchipelagoFFXModule.NativeCustomString custom_text;

            if (item.id != 0) {
                custom_text = new($"{item.name}");
            } else {
                custom_text = new($"{item.player}'s {item.name}");
            }

            _TOBtlSetMacroCommandValue(7, 1, custom_text.encoded);
        } else {
            _TOBtlSetMacroCommandValue(7, 1, data_end + com_name_offset);
        }

        byte* btl_text = _MsGetRomBtlText(0x300d, 0);
        _FUN_0089db10(0, btl_text);

        return 7;
    }

    public static bool send_overdrive(int com_id)
    {
        // Apworld defines Spiral Cut as overdrive location 0, and all other overdrives are treated as an offset from that value.
        int overdrive_id = com_id - PlayerCommandId.PCOM_SPIRAL_CUT;
        bool sent_overdrive = false;

        if (!FFXArchipelagoClient.local_checked_locations.Contains(overdrive_id | (long)FFXArchipelagoClient.ArchipelagoLocationType.Overdrive))
        {
            if (ArchipelagoFFXModule.item_locations.overdrive.TryGetValue(overdrive_id, out var item))
            {
                if (FFXArchipelagoClient.sendLocation(overdrive_id, FFXArchipelagoClient.ArchipelagoLocationType.Overdrive))
                {
                    ArchipelagoFFXModule.obtain_item(item.id);
                    sent_overdrive = true;
                }
            }
        }

        return sent_overdrive;
    }
}
