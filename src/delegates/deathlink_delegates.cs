using System.Runtime.InteropServices;

using Fahrenheit.FFX.Battle;

using static Fahrenheit.Modules.ArchipelagoFFX.delegates;

namespace Fahrenheit.Modules.ArchipelagoFFX;

public unsafe partial class DeathLinkModule {
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void FUN_0079b480(int chr_id, int com_id, int is_disabled);
    public const nint __addr_FUN_0079b480 = 0x39b480;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate Chr* MsGetChr(int chr_id);
    public const nint __addr_MsGetChr = 0x394030;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate uint MsGetBattleEndStatus();
    public const nint __addr_MsGetBattleEndStatus = 0x3928f0;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void MsBtlReadManage();
    public const nint __addr_MsBtlReadManage = 0x3830d0;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int MsDamageCheckDeath(int attacker_id, int target_id, int p3, int targetting_self);
    public const nint __addr_MsDamageCheckDeath = 0x38c800;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int MsMessageCueRegist(MessageCueType type, int arg1, int arg2, byte p4, byte p5);
    public const nint __addr_MsMessageCueRegist = 0x39cff0;

    // Method Handles
    private FhMethodHandle<MsGetBattleEndStatus> _MsGetBattleEndStatus;
    private FhMethodHandle<MsBtlReadManage> _MsBtlReadManage;
    private FhMethodHandle<MsDamageCheckDeath> _MsDamageCheckDeath;

    // Function library
    private FUN_0079b480 _set_command_disabled = FhUtil.get_fptr<FUN_0079b480>(__addr_FUN_0079b480);
    private MsGetChr _MsGetChr = FhUtil.get_fptr<MsGetChr>(__addr_MsGetChr);
    private MsMessageCueRegist _MsMessageCueRegist = FhUtil.get_fptr<MsMessageCueRegist>(__addr_MsMessageCueRegist);
}
