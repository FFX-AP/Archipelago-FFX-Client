using Fahrenheit.FFX.Battle;
using System.Runtime.InteropServices;
using static Fahrenheit.Modules.ArchipelagoFFX.delegates;

namespace Fahrenheit.Modules.ArchipelagoFFX;

public unsafe partial class OverdriveModule {
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
    private delegate int brnd(int rng_idx);
    private const nint __addr_brnd = 0x398900;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void MsSetSaveCommandWithPrefix(int chr_id, int com_id, int param_3);
    private const nint __addr_MsSetSaveCommandWithPrefix = 0x474190;

    private const nint __addr_ret_doesChrKnowCommand = 0x3A30C0;
    // Method Handles
    private readonly FhMethodHandle<MsGetSaveCommand> _MsGetSaveCommand;
    private readonly FhMethodHandle<MsSetRamChrAbility> _MsSetRamChrAbility;
    private readonly FhMethodHandle<MsLimitTidusLearn> _MsLimitTidusLearn;
    private readonly FhMethodHandle<MsAfterDamageProcess> _MsAfterDamageProcess;
    private readonly FhMethodHandle<CT_RetInt> _ret_doesChrKnowCommand;
    private readonly FhMethodHandle<MsSetSaveCommandWithPrefix> _MsSetSaveCommandWithPrefix;

    private readonly MsGetChr _MsGetChr = FhUtil.get_fptr<MsGetChr>(__addr_MsGetChr);
    private readonly MsMenuCloseTitleWindow _MsMenuCloseTitleWindow = FhUtil.get_fptr<MsMenuCloseTitleWindow>(__addr_MsMenuCloseTitleWindow);
    private readonly MsMessageCueRegist _MsMessageCueRegist = FhUtil.get_fptr<MsMessageCueRegist>(__addr_MsMessageCueRegist);
    private readonly MsSetStealEffect _MsSetStealEffect = FhUtil.get_fptr<MsSetStealEffect>(__addr_MsSetStealEffect);
    private readonly MsRegSEplay2 _MsRegSEplay2 = FhUtil.get_fptr<MsRegSEplay2>(__addr_MsRegSEplay2);
    private readonly MsPayGIL _MsPayGIL = FhUtil.get_fptr<MsPayGIL>(__addr_MsPayGIL);
    private readonly MsSetStealGillEffect _MsSetStealGillEffect = FhUtil.get_fptr<MsSetStealGillEffect>(__addr_MsSetStealGillEffect);
    private readonly MsCheckRange _MsCheckRange = FhUtil.get_fptr<MsCheckRange>(__addr_MsCheckRange);
    private readonly MsSetSaveCommand _MsSetSaveCommand = FhUtil.get_fptr<MsSetSaveCommand>(__addr_MsSetSaveCommand);
    private readonly achievementUnlockAchievement _achievementUnlockAchievement = FhUtil.get_fptr<achievementUnlockAchievement>(__addr_achievementUnlockAchievement);
    private readonly MsNumberRegist _MsNumberRegist = FhUtil.get_fptr<MsNumberRegist>(__addr_MsNumberRegist);
    private readonly MsLimitTypeDamageCheck _MsLimitTypeDamageCheck = FhUtil.get_fptr<MsLimitTypeDamageCheck>(__addr_MsLimitTypeDamageCheck);
    private readonly MsSubHP _MsSubHP = FhUtil.get_fptr<MsSubHP>(__addr_MsSubHP);
    private readonly MsSubMP _MsSubMP = FhUtil.get_fptr<MsSubMP>(__addr_MsSubMP);
    private readonly MsSubCTB _MsSubCTB = FhUtil.get_fptr<MsSubCTB>(__addr_MsSubCTB);
    private readonly MsLimitTypeStatusCheck _MsLimitTypeStatusCheck = FhUtil.get_fptr<MsLimitTypeStatusCheck>(__addr_MsLimitTypeStatusCheck);
    private readonly MsLimitStatusProcess _MsLimitStatusProcess = FhUtil.get_fptr<MsLimitStatusProcess>(__addr_MsLimitStatusProcess);
    private readonly MsAliveProcess _MsAliveProcess = FhUtil.get_fptr<MsAliveProcess>(__addr_MsAliveProcess);
    private readonly MsStoneProcess _MsStoneProcess = FhUtil.get_fptr<MsStoneProcess>(__addr_MsStoneProcess);
    private readonly MsBlowProcess _MsBlowProcess = FhUtil.get_fptr<MsBlowProcess>(__addr_MsBlowProcess);
    private readonly MsThreatProcess _MsThreatProcess = FhUtil.get_fptr<MsThreatProcess>(__addr_MsThreatProcess);
    private readonly MsAutoCureProcess _MsAutoCureProcess = FhUtil.get_fptr<MsAutoCureProcess>(__addr_MsAutoCureProcess);
    private readonly MsAutoPotionProcess _MsAutoPotionProcess = FhUtil.get_fptr<MsAutoPotionProcess>(__addr_MsAutoPotionProcess);
    private readonly MsSetChrWeak _MsSetChrWeak = FhUtil.get_fptr<MsSetChrWeak>(__addr_MsSetChrWeak);
    private readonly MsAutoRelifeProcess _MsAutoRelifeProcess = FhUtil.get_fptr<MsAutoRelifeProcess>(__addr_MsAutoRelifeProcess);
    private readonly MsStatusEffectCheck _MsStatusEffectCheck = FhUtil.get_fptr<MsStatusEffectCheck>(__addr_MsStatusEffectCheck);
    private readonly MsStatusDefenseEffect _MsStatusDefenseEffect = FhUtil.get_fptr<MsStatusDefenseEffect>(__addr_MsStatusDefenseEffect);
    private readonly MsActionRequest _MsActionRequest = FhUtil.get_fptr<MsActionRequest>(__addr_MsActionRequest);
    private readonly MsPopBtlPos _MsPopBtlPos = FhUtil.get_fptr<MsPopBtlPos>(__addr_MsPopBtlPos);
    private readonly MsDamageCheckDeath _MsDamageCheckDeath = FhUtil.get_fptr<MsDamageCheckDeath>(__addr_MsDamageCheckDeath);
    private readonly MsDamageSetMotion _MsDamageSetMotion = FhUtil.get_fptr<MsDamageSetMotion>(__addr_MsDamageSetMotion);
    private readonly brnd _brnd = FhUtil.get_fptr<brnd>(__addr_brnd);

    private readonly MsGetSaveCommand _fn_MsGetSaveCommand = FhUtil.get_fptr<MsGetSaveCommand>(__addr_MsGetSaveCommand);
}
