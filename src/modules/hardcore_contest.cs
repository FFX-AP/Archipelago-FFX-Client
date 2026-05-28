using System.IO;
using System.Runtime.InteropServices;

using Fahrenheit;
using Fahrenheit.FFX;
using Fahrenheit.FFX.Ids;

namespace ArchipelagoFFX;

[FhLoad(FhGameId.FFX)]
public unsafe partial class HardcoreDreamsEndModule : FhModule {
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void MsBtlReadManage();
    public const nint __addr_MsBtlReadManage = 0x3830d0;

    private static HardcoreDreamsEndModule _this;
    private bool _hardcore_dreams_end_enabled;

    private FhMethodHandle<MsBtlReadManage> _MsBtlReadManage;

    public HardcoreDreamsEndModule() {
        _this = this;
        _MsBtlReadManage = new(this, "FFX.exe", __addr_MsBtlReadManage, _h_MsBtlReadManage);
    }

    public static bool get_enabled() {
        return _this._hardcore_dreams_end_enabled;
    }

    public static void set_enabled(bool enabled) {
        _this._hardcore_dreams_end_enabled = enabled;
    }

    public override bool init(FhModContext mod_context, FileStream global_state_file) {
        return _MsBtlReadManage.hook();
    }

    private void _h_MsBtlReadManage() {
        int old_state = Globals.Battle.btl->battle_state;

        _MsBtlReadManage.orig_fptr();

        if (Globals.Battle.btl->battle_state != 13 || old_state == Globals.Battle.btl->battle_state) return;

        if (!_hardcore_dreams_end_enabled) return;

        for (int chr_id = PlySaveId.PC_TIDUS; chr_id < PlySaveId.PC_VALEFOR; chr_id++) {
            (Globals.Battle.player_characters + chr_id)->eternal_autolife = false;
            (Globals.Battle.player_characters + chr_id)->ram.status_suffer_extra &= ~StatusExtraFlags.AUTO_LIFE;
        }
    }
}
