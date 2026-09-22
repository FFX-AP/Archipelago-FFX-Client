using System.IO;

using Fahrenheit;
using Fahrenheit.FFX;
using Fahrenheit.FFX.Ids;

using FhGCall = Fahrenheit.FhCall;
using FhXCall = Fahrenheit.FFX.FhCall;

namespace ArchipelagoFFX;

[FhLoad(FhGameId.FFX)]
public unsafe class HardcoreDreamsEndModule : FhModule {
    private bool _hardcore_dreams_end_enabled;

    public bool get_enabled() {
        return _hardcore_dreams_end_enabled;
    }

    public void set_enabled(bool enabled) {
        _hardcore_dreams_end_enabled = enabled;
    }

    public override bool init(FhModContext mod_context, FileStream global_state_file) {
        return FhXCall.MsBtlReadManage.hook(this, _h_MsBtlReadManage);
    }

    private void _h_MsBtlReadManage() {
        int old_state = Globals.Battle.btl->battle_state;

        FhXCall.MsBtlReadManage.chain_from(_h_MsBtlReadManage).fnptr!();

        if (Globals.Battle.btl->battle_state != 13 || old_state == Globals.Battle.btl->battle_state) return;

        if (!_hardcore_dreams_end_enabled) return;

        for (int chr_id = PlySaveId.PC_TIDUS; chr_id < PlySaveId.PC_VALEFOR; chr_id++) {
            (Globals.Battle.player_characters + chr_id)->eternal_autolife = false;
            (Globals.Battle.player_characters + chr_id)->ram.status_suffer_extra &= ~StatusExtraFlags.AUTO_LIFE;
        }
    }
}
