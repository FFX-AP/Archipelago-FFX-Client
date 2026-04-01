using Fahrenheit.FFX;
using Fahrenheit.FFX.Battle;
using System.Runtime.InteropServices;
using static Fahrenheit.Modules.ArchipelagoFFX.delegates;

namespace Fahrenheit.Modules.ArchipelagoFFX;

public unsafe partial class CaptureModule : FhModule {
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate bool MsMonsterCapture(int target_id, int arena_idx);
    private const nint __addr_MsMonsterCapture = 0x390B80;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void FUN_00783bb0(byte mon_idx);
    private const int __addr_FUN_00783bb0 = 0x383BB0;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate Chr* MsGetMon(byte mon_idx);
    private const int __addr_MsGetMon = 0x00395AB0;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void AtelEventSetUp(int event_id);
    private const nint __addr_AtelEventSetUp = 0x472E90;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate char* AtelGetEventName(uint event_id);
    private const nint __addr_AtelGetEventName = 0x4796E0;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate int MsDamageCheckDeath(int attacker_id, int target_id, int param_3, uint param_4);
    private const nint __addr_MsDamageCheckDeath = 0x38C800;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate Chr* MsGetChr(uint chr_id);
    private const nint __addr_MsGetChr = 0x394030;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public unsafe delegate void MsSetSaveParam(uint chr_id);
    public const nint __addr_MsSetSaveParam = 0x3861B0;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public unsafe delegate void MsSetRamChrParam(uint chr_id);
    public const nint __addr_MsSetRamChrParam = 0x39C610;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public unsafe delegate void MsCalcCommand(AttackCue* param_1, int param_2);
    public const nint __addr_MsCalcCommand = 0x3893A0;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public unsafe delegate Command* MsGetCommand(int chr_id, int unused, int quit_on_idx, AttackCommandInfo* param_4, uint* param_5);
    public const nint __addr_MsGetCommand = 0x38CF10;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public unsafe delegate uint FUN_0078d100(Chr* chr);
    public const nint __addr_FUN_0078d100 = 0x38D100;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public unsafe delegate uint FUN_0078bb30(int param_1, byte* param_2, byte* param_3, Command* param_4, uint param_5, uint* param_6, int* param_7);
    public const nint __addr_FUN_0078bb30 = 0x38BB30;

    private const nint __addr_ret_hasKeyItem = 0x45B7A0;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int MsBattleEncountExe(int field_id, int group_idx, float walked_delta);
    public const nint __addr_MsBattleEncountExe = 0x380de0;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int MsBtlListFieldNum(int field_id);
    public const nint __addr_MsBtlListFieldNum = 0x39d1e0;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void ResetEncountExe(int p1);
    public const nint __addr_ResetEncountExe = 0x3810c0;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int brnd(int rng_slot);
    public const nint __addr_brnd = 0x398900;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate BtlBinField* MsBtlListField(int field_idx);
    private const nint __addr_MsBtlListField = 0x39d1b0;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate BtlBinEncounter* MsBtlListEncount(int field_idx);
    private const nint __addr_MsBtlListEncount = 0x39d190;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate BtlBinGroup* MsBtlListGroup(int field_idx, int group_idx);
    private const nint __addr_MsBtlListGroup = 0x39d230;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void Sg_FadeInW(int p1);
    public const nint __addr_Sg_FadeInW = 0x42cc20;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void* sceOpen(char* p1, int p2);
    public const nint __addr_sceOpen = 0x22fbe0;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int sceLseek(void* p1, int p2, int p3);
    public const nint __addr_sceLseek = 0x22fa90;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int sceRead(void* p1, void* dst, int amount);
    public const nint __addr_sceRead = 0x22fdb0;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int sceClose(void* p1);
    public const nint __addr_sceClose = 0x22f7c0;

    // Method Handles
    private readonly FhMethodHandle<MsMonsterCapture> _MsMonsterCapture;
    private readonly FhMethodHandle<FUN_00783bb0> _FUN_00783bb0;
    private readonly FhMethodHandle<AtelEventSetUp> _AtelEventSetUp;
    private readonly FhMethodHandle<CT_RetInt> _ret_hasKeyItem;
    private readonly FhMethodHandle<MsDamageCheckDeath> _MsDamageCheckDeath;
    private readonly FhMethodHandle<MsSetSaveParam> _MsSetSaveParam;
    private readonly FhMethodHandle<MsSetRamChrParam> _MsSetRamChrParam;
    private readonly FhMethodHandle<MsCalcCommand> _MsCalcCommand;
    private readonly FhMethodHandle<MsBattleEncountExe> _MsBattleEncountExe;

    private static char* get_event_name(uint event_id) => FhUtil.get_fptr<AtelGetEventName>(__addr_AtelGetEventName)(event_id);
    private readonly MsGetMon _MsGetMon = FhUtil.get_fptr<MsGetMon>(__addr_MsGetMon);
    private readonly MsGetChr _MsGetChr = FhUtil.get_fptr<MsGetChr>(__addr_MsGetChr);
    private readonly MsGetCommand _MsGetCommand = FhUtil.get_fptr<MsGetCommand>(__addr_MsGetCommand);
    private readonly FUN_0078d100 _FUN_0078d100 = FhUtil.get_fptr<FUN_0078d100>(__addr_FUN_0078d100);
    private readonly FUN_0078bb30 _FUN_0078bb30 = FhUtil.get_fptr<FUN_0078bb30>(__addr_FUN_0078bb30);
    private readonly MsBtlListFieldNum _MsBtlListFieldNum = FhUtil.get_fptr<MsBtlListFieldNum>(__addr_MsBtlListFieldNum);
    private readonly ResetEncountExe _ResetEncountExe = FhUtil.get_fptr<ResetEncountExe>(__addr_ResetEncountExe);
    private readonly brnd _brnd = FhUtil.get_fptr<brnd>(__addr_brnd);
    private readonly MsBtlListField _MsBtlListField = FhUtil.get_fptr<MsBtlListField>(__addr_MsBtlListField);
    private readonly MsBtlListEncount _MsBtlListEncount = FhUtil.get_fptr<MsBtlListEncount>(__addr_MsBtlListEncount);
    private readonly MsBtlListGroup _MsBtlListGroup = FhUtil.get_fptr<MsBtlListGroup>(__addr_MsBtlListGroup);
    private readonly Sg_FadeInW _Sg_FadeInW = FhUtil.get_fptr<Sg_FadeInW>(__addr_Sg_FadeInW);
    private readonly sceOpen _sceOpen = FhUtil.get_fptr<sceOpen>(__addr_sceOpen);
    private readonly sceLseek _sceLseek = FhUtil.get_fptr<sceLseek>(__addr_sceLseek);
    private readonly sceRead _sceRead = FhUtil.get_fptr<sceRead>(__addr_sceRead);
    private readonly sceClose _sceClose = FhUtil.get_fptr<sceClose>(__addr_sceClose);
}
