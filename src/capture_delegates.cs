using Fahrenheit.Atel;
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
    
    private const nint __addr_ret_hasKeyItem = 0x45B7A0;

    // Method Handles
    private readonly FhMethodHandle<MsMonsterCapture> _MsMonsterCapture;
    private readonly FhMethodHandle<FUN_00783bb0> _FUN_00783bb0;
    private readonly FhMethodHandle<AtelEventSetUp> _AtelEventSetUp;
    private readonly FhMethodHandle<CT_RetInt> _ret_hasKeyItem;

    private static char* get_event_name(uint event_id) => FhUtil.get_fptr<AtelGetEventName>(__addr_AtelGetEventName)(event_id);
    private readonly MsGetMon _MsGetMon = FhUtil.get_fptr<MsGetMon>(__addr_MsGetMon);
    
}
