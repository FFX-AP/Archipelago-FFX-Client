using Fahrenheit.Atel;
using Fahrenheit.FFX;
using Fahrenheit.FFX.Battle;
using System.Runtime.InteropServices;

namespace Fahrenheit.Modules.ArchipelagoFFX;

public unsafe partial class CaptureModule : FhModule {
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int CT_RetInt(AtelBasicWorker* work, int* storage, AtelStack* atelStack);

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

    // Method Handles
    private readonly FhMethodHandle<MsMonsterCapture> _MsMonsterCapture;
    private readonly FhMethodHandle<FUN_00783bb0> _FUN_00783bb0;
    private readonly FhMethodHandle<AtelEventSetUp> _AtelEventSetUp;
    private readonly FhMethodHandle<CT_RetInt> _ret_hasKeyItem;
    private readonly FhMethodHandle<MsDamageCheckDeath> _MsDamageCheckDeath;
    private readonly FhMethodHandle<MsSetSaveParam> _MsSetSaveParam;
    private readonly FhMethodHandle<MsSetRamChrParam> _MsSetRamChrParam;
    private readonly FhMethodHandle<MsCalcCommand> _MsCalcCommand;

    private static char* get_event_name(uint event_id) => FhUtil.get_fptr<AtelGetEventName>(__addr_AtelGetEventName)(event_id);
    private readonly MsGetMon _MsGetMon = FhUtil.get_fptr<MsGetMon>(__addr_MsGetMon);
    private readonly MsGetChr _MsGetChr = FhUtil.get_fptr<MsGetChr>(__addr_MsGetChr);
    private readonly MsGetCommand _MsGetCommand = FhUtil.get_fptr<MsGetCommand>(__addr_MsGetCommand);
    private readonly FUN_0078d100 _FUN_0078d100 = FhUtil.get_fptr<FUN_0078d100>(__addr_FUN_0078d100);
    private readonly FUN_0078bb30 _FUN_0078bb30 = FhUtil.get_fptr<FUN_0078bb30>(__addr_FUN_0078bb30);
}
