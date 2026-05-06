using System.Runtime.InteropServices;
using static Fahrenheit.Modules.ArchipelagoFFX.delegates;

namespace Fahrenheit.Modules.ArchipelagoFFX;

public unsafe partial class DeathLinkModule {
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void FUN_0079b480(int chr_id, int com_id, int is_disabled);
    public const nint __addr_FUN_0079b480 = 0x39b480;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate uint MsGetBattleEndStatus();
    public const nint __addr_MsGetBattleEndStatus = 0x3928f0;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void MsBtlReadManage();
    public const nint __addr_MsBtlReadManage = 0x3830d0;

    // Method Handles
    private FhMethodHandle<MsGetBattleEndStatus> _MsGetBattleEndStatus;
    private FhMethodHandle<MsBtlReadManage> _MsBtlReadManage;

    // Function library
    private FUN_0079b480 _set_command_disabled = FhUtil.get_fptr<FUN_0079b480>(__addr_FUN_0079b480);
}
