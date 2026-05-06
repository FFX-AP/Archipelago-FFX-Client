using System;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using Archipelago.MultiClient.Net.BounceFeatures.DeathLink;

using Fahrenheit.FFX;
using Fahrenheit.FFX.Battle;
using Fahrenheit.FFX.Ids;
using Fahrenheit.Modules.ArchipelagoFFX.Client;

namespace Fahrenheit.Modules.ArchipelagoFFX;

[FhLoad(FhGameId.FFX)]
public unsafe partial class DeathLinkModule : FhModule {
    public enum DeathLinkType {
        DOOM_STRICT,
        DOOM_LENIENT,
        ONE_HP,
        LOW_HP,
        BAD_BREATH,
        RANDOM,
    }

    public static readonly Vector4 DEATHLINK_COLOR = new(1.0f, 0.18f, 0.21f, 1.0f);

    // This is annoying but necessary for now because `FFXArchipelagoClient` wants everything to be static
    //TODO: Remove this once it's no longer necessary
    private static DeathLinkModule _this;

    private readonly FhModuleHandle<ToastModule> _toasts_handle;
    private ToastModule? _toasts;

    private readonly Random _deathlink_message_rng = new();
    private readonly Random _deathlink_type_rng = new();

    public DeathLinkType deathlink_type = DeathLinkType.DOOM_STRICT;
    private bool _deathlink_enabled;
    private uint _deathlinks_queued;
    private bool _send_deathlink_on_gameover = true;

    public DeathLinkModule() {
        _this = this;

        _toasts_handle = new(this);

        const string GAME = "FFX.exe";

        _MsGetBattleEndStatus = new(this, GAME, __addr_MsGetBattleEndStatus, _h_MsGetBattleEndStatus);
        _MsBtlReadManage = new(this, GAME, __addr_MsBtlReadManage, _h_MsBtlReadManage);
    }

    public static bool get_enabled() {
        return _this._deathlink_enabled;
    }

    public static void set_enabled(bool value) {
        _this._deathlink_enabled = value;

        if (!_this._deathlink_enabled) {
            // Clear remaining deathlinks
            _this._deathlinks_queued = 0;
        }
    }

    public static string get_type() {
        return get_type_name(_this.deathlink_type);
    }

    public static string get_type_name(DeathLinkType type) {
        return type switch {
            DeathLinkType.DOOM_STRICT => "Doom (1 turn)",
            DeathLinkType.DOOM_LENIENT => "Doom (3 turns)",
            DeathLinkType.ONE_HP => "One HP",
            DeathLinkType.LOW_HP => "Low HP",
            DeathLinkType.BAD_BREATH => "Bad Breath",
            DeathLinkType.RANDOM => "Random",
            _ => throw new NotImplementedException($"Unknown deathlink type: {(int)_this.deathlink_type}"),
        };
    }

    public static void set_type(string type) {
        _this.deathlink_type = type switch {
            "Doom (1 turn)" => DeathLinkType.DOOM_STRICT,
            "Doom (3 turns)" => DeathLinkType.DOOM_LENIENT,
            "One HP" => DeathLinkType.ONE_HP,
            "Low HP" => DeathLinkType.LOW_HP,
            "Bad Breath" => DeathLinkType.BAD_BREATH,
            "Random" => DeathLinkType.RANDOM,
            _ => throw new NotImplementedException($"Unknown deathlink type: {type}"),
        };
    }

    public static uint get_deathlinks_queued() {
        return _this._deathlinks_queued;
    }

    public static void debug_add_queued() {
        _this._deathlinks_queued += 1;
    }

    public static void debug_apply_deathlink() {
        _this._applyDeathlink();
    }

    public override bool init(FhModContext mod_context, FileStream global_state_file) {
        return _toasts_handle.try_get_module(out _toasts)
            && _MsGetBattleEndStatus.hook()
            && _MsBtlReadManage.hook();
    }

    private void _applyDeathlink() {
        DeathLinkType type = deathlink_type;

        if (type == DeathLinkType.RANDOM) {
            DeathLinkType[] types = Enum.GetValues<DeathLinkType>();
            if (types.Length < 2) {
                throw new Exception("The DeathLinkType enum is missing variants.");
            }

            type = types[_deathlink_type_rng.Next(types.Length - 2)];
        }

        switch (deathlink_type) {
            case DeathLinkType.DOOM_STRICT:
            case DeathLinkType.DOOM_LENIENT:
                for (int chr_id = 0; chr_id <= PlySaveId.PC_MAGUS3; chr_id++) {
                    Chr* chr = Globals.Battle.player_characters + chr_id;
                    chr->ram.status_suffer_extra |= StatusExtraFlags.DOOM;
                    chr->ram.doom_counter = deathlink_type switch {
                        DeathLinkType.DOOM_STRICT => 2,
                        DeathLinkType.DOOM_LENIENT => 4,
                        _ => throw new NotImplementedException($"Unknown doom deathlink type: {deathlink_type}"),
                    };
                }
                break;

            case DeathLinkType.ONE_HP:
                for (int chr_id = 0; chr_id <= PlySaveId.PC_MAGUS3; chr_id++) {
                    Chr* chr = Globals.Battle.player_characters + chr_id;
                    chr->ram.hp = 1;
                }
                break;

            case DeathLinkType.LOW_HP:
                for (int chr_id = 0; chr_id <= PlySaveId.PC_MAGUS3; chr_id++) {
                    Chr* chr = Globals.Battle.player_characters + chr_id;
                    chr->ram.hp = Math.Min(chr->ram.hp, chr->ram.max_hp / 2 - ((chr->ram.max_hp % 2) ^ 1));
                }
                break;

            case DeathLinkType.BAD_BREATH:
                for (int chr_id = 0; chr_id <= PlySaveId.PC_MAGUS3; chr_id++) {
                    Chr* chr = Globals.Battle.player_characters + chr_id;

                    // Bad Breath normally applies:
                    // - 150% Poison
                    // - 80% Confusion
                    // - 30% Berserk
                    // - 100% Silence (3 turns)
                    // - 100% Darkness (3 turns)
                    // - 130% Slow (3 turns)
                    // Confusion and Berserk feel bad, so we don't apply those.
                    chr->ram.status_suffer |= StatusPermanentFlags.POISON;
                    chr->ram.status_suffer_turns_left.silence = 3;
                    chr->ram.status_suffer_turns_left.darkness = 3;
                    chr->ram.status_suffer_turns_left.slow = 3;
                }
                break;

            default:
                throw new NotImplementedException($"Unknown deathlink type: {(int)deathlink_type}");
        }

        _send_deathlink_on_gameover = false;
    }

    private void _h_MsBtlReadManage() {
        int old_state = Globals.Battle.btl->battle_state;

        _MsBtlReadManage.orig_fptr();

        if (Globals.Battle.btl->battle_state != 13 || old_state == Globals.Battle.btl->battle_state) return;

        // Post Battle Start
        _logger.Info("Post Battle Start");

        _send_deathlink_on_gameover = true;

        _logger.Info($"  Memory initialized? {Globals.Battle.player_characters != null}");

        if (Globals.Battle.player_characters == null) return;

        if (!_deathlink_enabled || _deathlinks_queued == 0) return;

        _logger.Info("  Applying death link...");

        _applyDeathlink();

        //TODO: Add an MsMessageCueRegist call here with a custom message type once Fahrenheit supports that

        _logger.Info("  Disabling Escape and Flee...");

        for (int chr_id = 0; chr_id <= PlySaveId.PC_SEYMOUR; chr_id++) {
            _set_command_disabled(chr_id, PlayerCommandId.PCOM_ESCAPE, 1);
            _set_command_disabled(chr_id, PlayerCommandId.PCOM_FLEE, 1);
        }

        _deathlinks_queued -= 1;

        _logger.Info("  Done!");
    }

    private uint _h_MsGetBattleEndStatus() {
        uint battle_end_type = _MsGetBattleEndStatus.orig_fptr();

        if (!_send_deathlink_on_gameover) {
            return battle_end_type;
        }

        if (battle_end_type != 1 || Globals.Battle.btl->battle_state != 0x17) {
            return battle_end_type;
        }

        _logger.Info("Post Game Over");

        _logger.Info("  Sending death link...");

        string player = FFXArchipelagoClient.active_player?.Alias ?? "Someone";
        string message = _get_deathlink_send_text(player);

        FFXArchipelagoClient.death_link?.SendDeathLink(new(player, message));

        ToastModule.Toast deathlink_toast = new(
            [
                new(DEATHLINK_COLOR, "Deathlink sent!"),
            ],
            [
                new(new(1f), message),
            ]
        );

        _toasts!.queue_toast(deathlink_toast);

        return battle_end_type;
    }

    private string _get_deathlink_send_text(string player) {
        string encounter_name = Marshal.PtrToStringAnsi((nint)(&Globals.Battle.btl->field_name)) ?? "generic";

        int generic_rng = _deathlink_message_rng.Next(8);

        // Some encounters have unique messages
        string message_id = "deathlink.sent_message." + encounter_name switch {
            // Special
            _ when Globals.Battle.btl->ambush_state == 1 => "ambush",

            // Bosses
            "bjyt04_00" or "bjyt04_01" => "klikk",
            "cdsp07_00" => "tros",
            "klyt00_00" => "lord_ochu",
            "klyt01_00" => "sinspawn_geneaux",
            "cdsp02_00" => "oblitzerator",
            "mihn02_00" => "chocobo_eater",
            "kino02_00" or "kino03_10" => "sinspawn_gui",
            "genk09_00" => "extractor",
            "mcfr03_00" => "spherimorph",
            "maca02_00" => "crawler",
            "mcyt06_00" => "seymour",
            "maca02_01" => "wendigo",
            "hiku15_00" => "evrae",
            "stbv00_10" or "stbv00_11" or "stbv00_12" => "evrae_altana",
            "bvyt09_10" or "bvyt09_11" or "bvyt09_12" => "isaaru",
            "stbv01_10" => "seymour_natus",
            "nagi01_00" => "defender_x",
            "nagi05_10" => "lady_ginnem",
            "mtgz01_10" => "biran_and_yenke",
            "mtgz02_00" => "seymour_flux",
            "mtgz08_00" => "sanctuary_keeper",
            "dome02_00" => "spectral_keeper",
            "dome06_00" => "yunalesca",
            "ssbt00_00" or "ssbt01_00" => "sin_fins",
            "ssbt02_00" => "sin_core",
            "ssbt03_00" => "overdrive_sin",
            "sins03_00" => "seymour_omnis",
            "sins06_00" => "braskas_final_aeon",
            "sins07_10" => "yu_yevon",
            _ when encounter_name.StartsWith("sins07") => "contest_of_aeons",

            "omeg00_10" => "ultima_weapon",
            "omeg01_10" => "omega_weapon",

            "bjyt02_01" or "bjyt02_02" => "geosgaeno",

            "bsil07_70" => "dark_valefor",
            "bika03_70" => "dark_ifrit",
            "kami03_70" or "kami03_71" => "dark_ixion",
            "mcyt00_70" => "dark_shiva",
            "dome06_70" => "dark_bahamut",
            "mtgz01_70" => "dark_anima",
            "nagi05_70" or "nagi05_71" or "nagi05_72" or "nagi05_73" or "nagi05_74" => "dark_yojimbo",
            "kino00_70" or "kino01_70" or "kino01_72" => "dark_magus_sisters",
            "kino05_70" => "dark_sandy",
            "kino05_71" => "dark_mindy",
            "kino01_71" => "dark_cindy",

            // Area messages are treated as generic
            _ when generic_rng == 0 => "generic.0",
            _ when generic_rng == 1 => "generic.1",
            _ when generic_rng == 2 => "generic.2",
            _ when generic_rng == 3 => "generic.3",
            _ when generic_rng == 4 => "generic.4",
            _ when generic_rng == 5 => "generic.5",
            _ when generic_rng == 6 => "generic.6",

            // Areas
            _ when encounter_name.StartsWith("cdsp") => "al_bhed_ship",
            _ when encounter_name.StartsWith("bsil") => "besaid",
            _ when encounter_name.StartsWith("slik") => "ss_liki",
            _ when encounter_name.StartsWith("klyt") => "kilika",
            _ when encounter_name.StartsWith("lchb") => "luca",
            _ when encounter_name.StartsWith("mihn") => "miihen_highroad",
            _ when encounter_name.StartsWith("kino00")
                || encounter_name.StartsWith("kino01")
                || encounter_name.StartsWith("kino03")
                || encounter_name.StartsWith("kino05") => "mushroom_rock_road",
            _ when encounter_name.StartsWith("kino04")
                || encounter_name.StartsWith("kino07") => "djose",
            _ when encounter_name.StartsWith("genk") => "moonflow",
            _ when encounter_name.StartsWith("kami") => "thunder_plains",
            _ when encounter_name.StartsWith("mcfr") => "macalania_woods",
            "mcyt00_20" or "mcyt00_21" or "mcyt00_22"
         or "maca02_02" or "maca02_03" or "maca02_04"
         or "maca03_20" or "maca03_21" or "maca03_22" => "macalania_chase",
            _ when encounter_name.StartsWith("maca")
                || encounter_name.StartsWith("mcyt") => "macalania_lake",
            _ when encounter_name.StartsWith("bika") => "bikanel",
            _ when encounter_name.StartsWith("azit") => "home",
            _ when encounter_name.StartsWith("bvyt00") => "bevelle",
            _ when encounter_name.StartsWith("bvyt")
                || encounter_name.StartsWith("stbv00") => "via_purifico",
            _ when encounter_name.StartsWith("stbv01") => "highbridge",
            _ when encounter_name.StartsWith("nagi00") => "calm_lands",
            _ when encounter_name.StartsWith("nagi") => "cavern_of_the_stolen_fayth",
            _ when encounter_name.StartsWith("mtgz01") => "gagazet_trail",
            _ when encounter_name.StartsWith("mtgz06") => "gagazet_cave",
            _ when encounter_name.StartsWith("mtgz07") => "gagazet_cave_water",
            _ when encounter_name.StartsWith("zkrn") => "zanarkand_ruins",
            _ when encounter_name.StartsWith("dome") => "zanarkand_dome",
            _ when encounter_name.StartsWith("sins02") => "inside_sin_sea",
            _ when encounter_name.StartsWith("sins04") => "inside_sin_city",
            _ when encounter_name.StartsWith("omeg00") => "omega_ruins",
            _ when encounter_name.StartsWith("omeg01") => "omega_gauntlet",

            _ => "generic.0",
        };

        return String.Format(FhApi.Localization.localize(message_id), player);
    }

    private string _get_backup_deathlink_received_text(string source_player) {
        string message_id = "deathlink.received_backup_message." + deathlink_type switch {
            DeathLinkType.DOOM_STRICT or DeathLinkType.DOOM_LENIENT => "doom",
            DeathLinkType.ONE_HP => "one_hp",
            DeathLinkType.LOW_HP => "low_hp",
            DeathLinkType.BAD_BREATH => "bad_breath",
            DeathLinkType.RANDOM => "random",
            _ => "generic",
        };

        return String.Format(FhApi.Localization.localize(message_id), source_player);
    }

    public static void post_deathlink(DeathLink death_msg) {
        if (!_this._deathlink_enabled) return;
        _this._deathlinks_queued += 1;

        // Display a toast
        ToastModule.Toast deathlink_toast = new(
            [
                new(DEATHLINK_COLOR, "Deathlink received!"),
            ],
            [
                new(new(1f), death_msg.Cause ?? _this._get_backup_deathlink_received_text(death_msg.Source)),
            ]
        );

        _this._toasts!.queue_toast(deathlink_toast);
    }
}
