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
public unsafe class OverdriveModule : FhModule {
    // Fahrenheit-related
    private FhModContext? _mod_context;
    private FileStream? _global_state;

    // Delegates
    //TODO: Remove these once FhCall is more up-to-date
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate int MsGetSaveCommand(int char_id, uint com_id);
    private const int __addr_MsGetSaveCommand = 0x3850E0;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void MsSetRamChrAbility(int chr_id, Chr* chr);
    private const nint __addr_MsSetRamChrAbility = 0x39BB70;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate int MsLimitTidusLearn(int chr_id);
    private const nint __addr_MsLimitTidusLearn = 0x3B0CE0;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate uint MsAfterDamageProcess(int attacker_id, uint param_2, int target_id, uint* param_4, uint param_5);
    private const nint __addr_MsAfterDamageProcess = 0x38F0B0;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate Chr* MsGetChr(int chr_id);
    private const nint __addr_MsGetChr = 0x394030;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void MsMenuCloseTitleWindow(int param_1);
    private const nint __addr_MsMenuCloseTitleWindow = 0x38FA80;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate int MsMessageCueRegist(uint type, int param_2, int param_3, byte param_4, byte param_5);
    private const nint __addr_MsMessageCueRegist = 0x39CFF0;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void MsSetStealEffect(int param_1, int param_2);
    private const nint __addr_MsSetStealEffect = 0x39ED20;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate uint MsRegSEplay2(int param_1, uint param_2);
    private const nint __addr_MsRegSEplay2 = 0x3A0160;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate int MsPayGIL(int param_1);
    private const nint __addr_MsPayGIL = 0x385A60;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void MsSetStealGillEffect(int param_1, int param_2);
    private const nint __addr_MsSetStealGillEffect = 0x39ED40;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate int MsCheckRange(int param_1, int param_2, int param_3);
    private const nint __addr_MsCheckRange = 0x39A0D0;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate int MsSetSaveCommand(int chr_id, uint param_2, int param_3);
    private const nint __addr_MsSetSaveCommand = 0x385D10;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void achievementUnlockAchievement(int ach_id);
    private const nint __addr_achievementUnlockAchievement = 0x422410;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate int MsNumberRegist(int param_1, int param_2, int param_3, int param_4, int param_5, uint param_6, uint param_7);
    private const nint __addr_MsNumberRegist = 0x39FA20;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate int MsLimitTypeDamageCheck(int attacker_id, Chr* attacker, int target_id, Chr* target, int param_5, int param_6, int param_7);
    private const nint __addr_MsLimitTypeDamageCheck = 0x3B0D60;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void MsSubHP(int chr_id, Chr* chr, int param_3, int param_4, int param_5, uint param_6, uint param_7);
    private const nint __addr_MsSubHP = 0x38E2F0;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void MsSubMP(int chr_id, Chr* chr, int param_3, int param_4, int param_5, uint param_6, uint param_7);
    private const nint __addr_MsSubMP = 0x38E400;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void MsSubCTB(int chr_id, Chr* chr, int param_3, int param_4, uint param_5, uint param_6);
    private const nint __addr_MsSubCTB = 0x38E2A0;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate int MsLimitTypeStatusCheck(int attacker_id, Chr* attacker, int target_id, Chr* target, int param_5, uint param_6);
    private const nint __addr_MsLimitTypeStatusCheck = 0x3B12D0;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate int MsLimitStatusProcess(int chr_id, Chr* chr, uint param_3);
    private const nint __addr_MsLimitStatusProcess = 0x38D330;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate int MsAliveProcess(int chr_id, Chr* chr);
    private const nint __addr_MsAliveProcess = 0x389220;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate int MsStoneProcess(int chr_id, Chr* chr);
    private const nint __addr_MsStoneProcess = 0x38E210;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate int MsBlowProcess(int chr_id, Chr* chr);
    private const nint __addr_MsBlowProcess = 0x389270;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate int MsThreatProcess(int chr_id, Chr* chr);
    private const nint __addr_MsThreatProcess = 0x38E4B0;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate int MsAutoCureProcess(int target_id, Chr* target, int attacker_id, int poison, int zombie, int darkness, int silence);
    private const nint __addr_MsAutoCureProcess = 0x3B2520;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate int MsAutoPotionProcess(int target_id, Chr* target, int attacker_id);
    private const nint __addr_MsAutoPotionProcess = 0x3B2860;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate int MsSetChrWeak(int chr_id, int new_weak_level);
    private const nint __addr_MsSetChrWeak = 0x38D8B0;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate bool MsAutoRelifeProcess(int attacker_id, Chr* attacker, int target_id, Chr* target);
    private const nint __addr_MsAutoRelifeProcess = 0x38D990;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void MsStatusEffectCheck(int chr_id);
    private const nint __addr_MsStatusEffectCheck = 0x39F010;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate int MsStatusDefenseEffect(int attacker_id, int target_id, int dmg_calc_flags);
    private const nint __addr_MsStatusDefenseEffect = 0x39EE40;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate int MsActionRequest(int target_id, int attacker_id, int param_3, int param_4, int param_5, void* param_6);
    private const nint __addr_MsActionRequest = 0x3ACEC0;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void MsPopBtlPos(Chr* chr);
    private const nint __addr_MsPopBtlPos = 0x3AC620;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate int MsDamageCheckDeath(int attacker_id, int target_id, int param_3, int targeting_self);
    private const nint __addr_MsDamageCheckDeath = 0x38C800;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void MsDamageSetMotion(int chr_id, int param_2, int targeting_self);
    private const nint __addr_MsDamageSetMotion = 0x38CAE0;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate int Brnd(int rng_idx);
    private const nint __addr_Brnd = 0x398900;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate int TOBtlDrawLearningMessageWindow(int chr_id, int com_id);
    private const nint __addr_TOBtlDrawLearningMessageWindow = 0x495290;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate int FUN_007B4B80(int chr_id, int param_2, int param_3, uint param_4);
    private const nint __addr_FUN_007B4B80 = 0x3B4B80;

    private const nint __addr_ret_doesChrKnowCommand = 0x3A30C0;

    // Method Handles
    private readonly FhMethodHandle<MsGetSaveCommand> _MsGetSaveCommand;
    private readonly FhMethodHandle<MsSetRamChrAbility> _MsSetRamChrAbility;
    private readonly FhMethodHandle<MsLimitTidusLearn> _MsLimitTidusLearn;
    private readonly FhMethodHandle<MsAfterDamageProcess> _MsAfterDamageProcess;
    private readonly FhMethodHandle<TOBtlDrawLearningMessageWindow> _TOBtlDrawLearningMessageWindow;
    private readonly FhMethodHandle<FUN_007B4B80> _FUN_007B4B80;
    private readonly FhMethodHandle<CT_RetInt> _ret_doesChrKnowCommand;

    private readonly MsGetChr _MsGetChr;
    private readonly MsMenuCloseTitleWindow _MsMenuCloseTitleWindow;
    private readonly MsMessageCueRegist _MsMessageCueRegist;
    private readonly MsSetStealEffect _MsSetStealEffect;
    private readonly MsRegSEplay2 _MsRegSEplay2;
    private readonly MsPayGIL _MsPayGIL;
    private readonly MsSetStealGillEffect _MsSetStealGillEffect;
    private readonly MsCheckRange _MsCheckRange;
    private readonly MsSetSaveCommand _MsSetSaveCommand;
    private readonly achievementUnlockAchievement _achievementUnlockAchievement;
    private readonly MsNumberRegist _MsNumberRegist;
    private readonly MsLimitTypeDamageCheck _MsLimitTypeDamageCheck;
    private readonly MsSubHP _MsSubHP;
    private readonly MsSubMP _MsSubMP;
    private readonly MsSubCTB _MsSubCTB;
    private readonly MsLimitTypeStatusCheck _MsLimitTypeStatusCheck;
    private readonly MsLimitStatusProcess _MsLimitStatusProcess;
    private readonly MsAliveProcess _MsAliveProcess;
    private readonly MsStoneProcess _MsStoneProcess;
    private readonly MsBlowProcess _MsBlowProcess;
    private readonly MsThreatProcess _MsThreatProcess;
    private readonly MsAutoCureProcess _MsAutoCureProcess;
    private readonly MsAutoPotionProcess _MsAutoPotionProcess;
    private readonly MsSetChrWeak _MsSetChrWeak;
    private readonly MsAutoRelifeProcess _MsAutoRelifeProcess;
    private readonly MsStatusEffectCheck _MsStatusEffectCheck;
    private readonly MsStatusDefenseEffect _MsStatusDefenseEffect;
    private readonly MsActionRequest _MsActionRequest;
    private readonly MsPopBtlPos _MsPopBtlPos;
    private readonly MsDamageCheckDeath _MsDamageCheckDeath;
    private readonly MsDamageSetMotion _MsDamageSetMotion;
    private readonly Brnd _Brnd;

    private readonly MsGetSaveCommand _fn_MsGetSaveCommand = FhUtil.get_fptr<MsGetSaveCommand>(__addr_MsGetSaveCommand);

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
        _TOBtlDrawLearningMessageWindow = new FhMethodHandle<TOBtlDrawLearningMessageWindow>(this, GAME, __addr_TOBtlDrawLearningMessageWindow, h_TOBtlDrawLearningMessageWindow);
        _FUN_007B4B80 = new FhMethodHandle<FUN_007B4B80>(this, GAME, __addr_FUN_007B4B80, h_FUN_007B4B80);
        _ret_doesChrKnowCommand = new FhMethodHandle<CT_RetInt>(this, GAME, __addr_ret_doesChrKnowCommand, h_ret_doesChrKnowCommand);

        _MsGetChr = FhUtil.get_fptr<MsGetChr>(__addr_MsGetChr);
        _MsMenuCloseTitleWindow = FhUtil.get_fptr<MsMenuCloseTitleWindow>(__addr_MsMenuCloseTitleWindow);
        _MsMessageCueRegist = FhUtil.get_fptr<MsMessageCueRegist>(__addr_MsMessageCueRegist);
        _MsSetStealEffect = FhUtil.get_fptr<MsSetStealEffect>(__addr_MsSetStealEffect);
        _MsRegSEplay2 = FhUtil.get_fptr<MsRegSEplay2>(__addr_MsRegSEplay2);
        _MsPayGIL = FhUtil.get_fptr<MsPayGIL>(__addr_MsPayGIL);
        _MsSetStealGillEffect = FhUtil.get_fptr<MsSetStealGillEffect>(__addr_MsSetStealGillEffect);
        _MsCheckRange = FhUtil.get_fptr<MsCheckRange>(__addr_MsCheckRange);
        _MsSetSaveCommand = FhUtil.get_fptr<MsSetSaveCommand>(__addr_MsSetSaveCommand);
        _achievementUnlockAchievement = FhUtil.get_fptr<achievementUnlockAchievement>(__addr_achievementUnlockAchievement);
        _MsNumberRegist = FhUtil.get_fptr<MsNumberRegist>(__addr_MsNumberRegist);
        _MsLimitTypeDamageCheck = FhUtil.get_fptr<MsLimitTypeDamageCheck>(__addr_MsLimitTypeDamageCheck);
        _MsSubHP = FhUtil.get_fptr<MsSubHP>(__addr_MsSubHP);
        _MsSubMP = FhUtil.get_fptr<MsSubMP>(__addr_MsSubMP);
        _MsSubCTB = FhUtil.get_fptr<MsSubCTB>(__addr_MsSubCTB);
        _MsLimitTypeStatusCheck = FhUtil.get_fptr<MsLimitTypeStatusCheck>(__addr_MsLimitTypeStatusCheck);
        _MsLimitStatusProcess = FhUtil.get_fptr<MsLimitStatusProcess>(__addr_MsLimitStatusProcess);
        _MsAliveProcess = FhUtil.get_fptr<MsAliveProcess>(__addr_MsAliveProcess);
        _MsStoneProcess = FhUtil.get_fptr<MsStoneProcess>(__addr_MsStoneProcess);
        _MsBlowProcess = FhUtil.get_fptr<MsBlowProcess>(__addr_MsBlowProcess);
        _MsThreatProcess = FhUtil.get_fptr<MsThreatProcess>(__addr_MsThreatProcess);
        _MsAutoCureProcess = FhUtil.get_fptr<MsAutoCureProcess>(__addr_MsAutoCureProcess);
        _MsAutoPotionProcess = FhUtil.get_fptr<MsAutoPotionProcess>(__addr_MsAutoPotionProcess);
        _MsSetChrWeak = FhUtil.get_fptr<MsSetChrWeak>(__addr_MsSetChrWeak);
        _MsAutoRelifeProcess = FhUtil.get_fptr<MsAutoRelifeProcess>(__addr_MsAutoRelifeProcess);
        _MsStatusEffectCheck = FhUtil.get_fptr<MsStatusEffectCheck>(__addr_MsStatusEffectCheck);
        _MsStatusDefenseEffect = FhUtil.get_fptr<MsStatusDefenseEffect>(__addr_MsStatusDefenseEffect);
        _MsActionRequest = FhUtil.get_fptr<MsActionRequest>(__addr_MsActionRequest);
        _MsPopBtlPos = FhUtil.get_fptr<MsPopBtlPos>(__addr_MsPopBtlPos);
        _MsDamageCheckDeath = FhUtil.get_fptr<MsDamageCheckDeath>(__addr_MsDamageCheckDeath);
        _MsDamageSetMotion = FhUtil.get_fptr<MsDamageSetMotion>(__addr_MsDamageSetMotion);
        _Brnd = FhUtil.get_fptr<Brnd>(__addr_Brnd);
    }

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
    }

    public override bool init(FhModContext mod_context, FileStream global_state_file)
    {
        _mod_context  = mod_context;
        _global_state = global_state_file;

        return _MsGetSaveCommand.hook()
            && _MsSetRamChrAbility.hook()
            && _MsLimitTidusLearn.hook()
            && _MsAfterDamageProcess.hook()
            && _FUN_007B4B80.hook()
            && _ret_doesChrKnowCommand.hook();
    }

    private int h_MsGetSaveCommand(int chr_id, uint com_id)
    {
        if (com_id is >= PlayerCommandId.PCOM_SPIRAL_CUT and <= PlayerCommandId.PCOM_AUROCHS_REELS || com_id == PlayerCommandId.PCOM_REQUIEM)
        {
            OverdriveProvider.provide_overdrive(chr_id);
        }

        return _MsGetSaveCommand.orig_fptr(chr_id, com_id);
    }

    private void h_MsSetRamChrAbility(int chr_id, Chr* chr)
    {
        OverdriveProvider.provide_overdrive(chr_id);
        
        _MsSetRamChrAbility.orig_fptr(chr_id, chr);
        return;
    }

    private int h_MsLimitTidusLearn(int chr_id)
    {
        if (chr_id == PlySaveId.PC_TIDUS)
        {
            uint tidusLimitUses = ++save_data->tidus_limit_uses;

            if (tidusLimitUses >= 10)
                if (send_overdrive(PlayerCommandId.PCOM_SLICE_AND_DICE))
                {
                    h_TOBtlDrawLearningMessageWindow(chr_id, PlayerCommandId.PCOM_SLICE_AND_DICE);
                }

            if (tidusLimitUses >= 20)
                if (send_overdrive(PlayerCommandId.PCOM_ENERGY_RAIN))
                {
                    h_TOBtlDrawLearningMessageWindow(chr_id, PlayerCommandId.PCOM_ENERGY_RAIN);
                }

            if (tidusLimitUses >= 40)
                if (send_overdrive(PlayerCommandId.PCOM_BLITZ_ACE))
                {
                    h_TOBtlDrawLearningMessageWindow(chr_id, PlayerCommandId.PCOM_BLITZ_ACE);
                }
        }
        return 0;
    }

    private int h_TOBtlDrawLearningMessageWindow(int chr_id, int com_id)
    {
        return _TOBtlDrawLearningMessageWindow.orig_fptr(chr_id, com_id);
    }

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

                                    if (send_overdrive(rage_to_learn))
                                    {
                                        h_TOBtlDrawLearningMessageWindow(attacker_id, rage_to_learn);
                                    }

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
                uVar9 = (uint)_Brnd(9);
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
        uVar9 = (uint)_Brnd(9);
        iVar7 = (int)(uVar9 & 3) + 0xd;
        _MsDamageSetMotion(target_id, iVar7, (attacker_id != target_id) ? 1 : 0);
        return uVar12;
    }

    private int h_FUN_007B4B80(int chr_id, int param_2, int param_3, uint param_4)
    {
        //if (param_2 == 0x10A)
        
        return _FUN_007B4B80.orig_fptr(chr_id, param_2, param_3, param_4);
    }

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

    public static bool send_overdrive(int com_id)
    {
        int overdrive_id = com_id - PlayerCommandId.PCOM_SPIRAL_CUT;
        bool location_sent = false;

        if (!FFXArchipelagoClient.local_checked_locations.Contains(overdrive_id | (long)FFXArchipelagoClient.ArchipelagoLocationType.Overdrive))
        {
            if (ArchipelagoFFXModule.item_locations.overdrive.TryGetValue(overdrive_id, out var item))
            {
                if (FFXArchipelagoClient.sendLocation(overdrive_id, FFXArchipelagoClient.ArchipelagoLocationType.Overdrive))
                {
                    location_sent = true;
                    ArchipelagoFFXModule.obtain_item(item.id);
                }
            }
        }
        return location_sent;
    }

    private static T* ptr_at<T>(nint address)          where T : unmanaged { return (T*)(address); }
    private static T  get_at<T>(nint address)          where T : unmanaged { return *ptr_at<T>(address); }
    private static T  set_at<T>(nint address, T value) where T : unmanaged { return *ptr_at<T>(address) = value; }

}

