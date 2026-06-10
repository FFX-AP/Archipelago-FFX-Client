using System;
using System.IO;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;

using Archipelago.MultiClient.Net.BounceFeatures.DeathLink;

using ArchipelagoFFX.Client;

using Fahrenheit;
using Fahrenheit.FFX;
using Fahrenheit.FFX.Battle;
using Fahrenheit.FFX.Ids;

using FhXCall = Fahrenheit.FFX.FhCall;

namespace ArchipelagoFFX;

[FhLoad(FhGameId.FFX)]
public unsafe partial class DeathLinkModule : FhModule {
    public enum DeathLinkSendType {
        GameOver,
        KO,
    }

    public enum DeathLinkReceiveType {
        DOOM_STRICT,
        DOOM_LENIENT,
        GRAVE_HP,
        BAD_BREATH,
        RANDOM,
    }

    public static readonly Vector4 DEATHLINK_COLOR = new(1.0f, 0.18f, 0.21f, 1.0f);

    private ArchipelagoFFXModule.NativeCustomString _deathlink_announcement;

    private FhModuleHandle<ArchipelagoClientModule> _client_handle;
    private ArchipelagoClientModule? _client;

    private readonly FhModuleHandle<ToastModule> _toasts_handle;
    private ToastModule? _toasts;

    private readonly Random _deathlink_message_rng = new();
    private readonly Random _deathlink_type_rng = new();
    private readonly Random _deathlink_grave_hp_rng = new();

    public DeathLinkSendType deathlink_send_type = DeathLinkSendType.GameOver;
    public DeathLinkReceiveType deathlink_receive_type = DeathLinkReceiveType.DOOM_STRICT;
    private bool _deathlink_enabled;
    private uint _deathlinks_queued;
    private bool deathlink_grace = false;

    public DeathLinkModule() {
        _deathlink_announcement = new("Deathlink!");

        _client_handle = new(this);
        _toasts_handle = new(this);
    }

    public bool get_enabled() {
        return _deathlink_enabled;
    }

    public void set_enabled(bool value) {
        _deathlink_enabled = value;

        if (!_deathlink_enabled) {
            // Clear remaining deathlinks
            _deathlinks_queued = 0;
        }

        if (_deathlink_enabled) {
            _client!.current_death_link_service?.EnableDeathLink();
        } else {
            _client!.current_death_link_service?.DisableDeathLink();
        }
    }

    public string get_send_type() {
        return get_send_type_name(deathlink_send_type);
    }

    public string get_send_type_name(DeathLinkSendType send_type) {
        return send_type switch {
            DeathLinkSendType.GameOver => "Game Over",
            DeathLinkSendType.KO => "KO",
            _ => throw new NotImplementedException($"Unknown deathlink type: {(int)deathlink_receive_type}"),
        };
    }

    public void set_send_type(string type) {
        deathlink_send_type = type switch {
            "Game Over" => DeathLinkSendType.GameOver,
            "KO" => DeathLinkSendType.KO,
            _ => throw new NotImplementedException($"Unknown deathlink type: {type}"),
        };
    }

    public string get_receive_type() {
        return get_receive_type_name(deathlink_receive_type);
    }

    public string get_receive_type_name(DeathLinkReceiveType receive_type) {
        return receive_type switch {
            DeathLinkReceiveType.DOOM_STRICT => "Doom (1 turn)",
            DeathLinkReceiveType.DOOM_LENIENT => "Doom (3 turns)",
            DeathLinkReceiveType.GRAVE_HP => "Grave HP",
            DeathLinkReceiveType.BAD_BREATH => "Bad Breath",
            DeathLinkReceiveType.RANDOM => "Random",
            _ => throw new NotImplementedException($"Unknown deathlink type: {(int)deathlink_receive_type}"),
        };
    }

    public void set_receive_type(string type) {
        deathlink_receive_type = type switch {
            "Doom (1 turn)" => DeathLinkReceiveType.DOOM_STRICT,
            "Doom (3 turns)" => DeathLinkReceiveType.DOOM_LENIENT,
            "Grave HP" => DeathLinkReceiveType.GRAVE_HP,
            "Bad Breath" => DeathLinkReceiveType.BAD_BREATH,
            "Random" => DeathLinkReceiveType.RANDOM,
            _ => throw new NotImplementedException($"Unknown deathlink type: {type}"),
        };
    }

    public uint get_deathlinks_queued() {
        return _deathlinks_queued;
    }

    public void debug_add_queued() {
        _deathlinks_queued += 1;
    }

    public void debug_apply_deathlink() {
        apply_death_link();
    }

    public override bool init(FhModContext mod_context, FileStream global_state_file) {
        return _client_handle.try_get_module(out _client)
            && _toasts_handle.try_get_module(out _toasts)
            && FhXCall.h_MsBtlReadManage.hook(this, _h_MsBtlReadManage)
            && FhXCall.h_MsDamageCheckDeath.hook(this, _h_MsDamageCheckDeath)
            && FhXCall.h_MsGetBattleEndStatus.hook(this, _h_MsGetBattleEndStatus);
    }

    private void apply_death_link() {
        DeathLinkReceiveType receive_type = deathlink_receive_type;

        if (receive_type == DeathLinkReceiveType.RANDOM) {
            DeathLinkReceiveType[] types = Enum.GetValues<DeathLinkReceiveType>();
            if (types.Length < 2) {
                throw new Exception("The DeathLinkType enum is missing variants.");
            }

            receive_type = types[_deathlink_type_rng.Next(types.Length - 1)];
        }

        switch (receive_type) {
            case DeathLinkReceiveType.DOOM_STRICT:
            case DeathLinkReceiveType.DOOM_LENIENT:
                for (int chr_id = 0; chr_id <= PlySaveId.PC_MAGUS3; chr_id++) {
                    Chr* chr = Globals.Battle.player_characters + chr_id;
                    chr->ram.status_suffer_extra |= StatusExtraFlags.DOOM;
                    chr->ram.doom_counter = receive_type switch {
                        DeathLinkReceiveType.DOOM_STRICT => 2,
                        DeathLinkReceiveType.DOOM_LENIENT => 4,
                        _ => throw new NotImplementedException($"Unknown doom deathlink type: {receive_type}"),
                    };
                }
                break;

            case DeathLinkReceiveType.GRAVE_HP:
                for (int chr_id = 0; chr_id <= PlySaveId.PC_MAGUS3; chr_id++) {
                    Chr* chr = Globals.Battle.player_characters + chr_id;
                    chr->ram.hp = Math.Min(_deathlink_grave_hp_rng.Next(1, 10), Math.Min(chr->ram.hp, chr->ram.max_hp));
                }
                break;

            case DeathLinkReceiveType.BAD_BREATH:
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
                throw new NotImplementedException($"Unknown deathlink type: {(int)receive_type}");
        }

        deathlink_grace = true;
    }

    private void _h_MsBtlReadManage() {
        int old_state = Globals.Battle.btl->battle_state;

        FhXCall.h_MsBtlReadManage.chain_from(_h_MsBtlReadManage).fnptr!();

        if (Globals.Battle.btl->battle_state != 13 || old_state == Globals.Battle.btl->battle_state) return;

        // Post Battle Start
        _logger.Info("Post Battle Start");

        deathlink_grace = false;

        _logger.Info($"  Memory initialized? {Globals.Battle.player_characters != null}");

        if (Globals.Battle.player_characters == null) return;

        if (!_deathlink_enabled || _deathlinks_queued == 0) return;

        _logger.Info("  Applying death link...");

        apply_death_link();

        FhXCall.h_MsMessageCueRegist.fnptr!((uint)MessageCueType.FH_CUSTOM, (int)_deathlink_announcement.encoded, 0, 27, 35);

        _logger.Info("  Disabling Escape and Flee...");

        for (int chr_id = 0; chr_id <= PlySaveId.PC_SEYMOUR; chr_id++) {
            FhXCall.h_FUN_0079b480.fnptr!(chr_id, PlayerCommandId.PCOM_ESCAPE, 1);
            FhXCall.h_FUN_0079b480.fnptr!(chr_id, PlayerCommandId.PCOM_FLEE, 1);
        }

        _deathlinks_queued -= 1;

        _logger.Info("  Done!");
    }

    private int _h_MsDamageCheckDeath(int attacker_id, int target_id, int p3, int targetting_self) {
        int result = FhXCall.h_MsDamageCheckDeath.chain_from(_h_MsDamageCheckDeath).fnptr!(attacker_id, target_id, p3, targetting_self);

        if (result == 0 || target_id > PlySaveId.PC_MAGUS3) {
            return result;
        }

        _logger.Info("Post KO");

        if (!_deathlink_enabled || deathlink_send_type != DeathLinkSendType.KO || deathlink_grace) {
            return result;
        }

        _logger.Info("  Sending deathlink...");

        string player = _client!.active_player?.Alias ?? "Someone";

        Chr* target = FhXCall.h_MsGetChr.fnptr!(target_id);
        byte[] decoded = new byte[FhEncoding.compute_decode_buffer_size(target->ram.name, null, null, FhEncodingFlags.IMPLICIT_END)];
        FhEncoding.decode(target->ram.name, decoded, null, null, FhEncodingFlags.IMPLICIT_END);
        string target_name = Encoding.UTF8.GetString(decoded);

        string message = _get_deathlink_send_text($"{player}'s {target_name}");

        _client!.current_death_link_service?.SendDeathLink(new(player, message));

        ToastModule.Toast deathlink_toast = new(
            [
                new(DEATHLINK_COLOR, "Deathlink sent!"),
            ],
            [
                new(new(1f), message),
            ]
        );

        _toasts!.queue_toast(deathlink_toast);

        return result;
    }

    private uint _h_MsGetBattleEndStatus() {
        uint battle_end_type = FhXCall.h_MsGetBattleEndStatus.chain_from(_h_MsGetBattleEndStatus).fnptr!();

        if (!_deathlink_enabled || battle_end_type != 1 || Globals.Battle.btl->battle_state != 0x17) {
            return battle_end_type;
        }

        _logger.Info("Post Game Over");

        if (deathlink_send_type != DeathLinkSendType.GameOver || deathlink_grace) {
            return battle_end_type;
        }

        _logger.Info("  Sending deathlink...");

        string player = _client!.active_player?.Alias ?? "Someone";
        string message = _get_deathlink_send_text(player);

        _client!.current_death_link_service?.SendDeathLink(new(player, message));

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
        string message_id = "deathlink.received_backup_message." + deathlink_receive_type switch {
            DeathLinkReceiveType.DOOM_STRICT
         or DeathLinkReceiveType.DOOM_LENIENT => "doom",
            DeathLinkReceiveType.GRAVE_HP     => "grave_hp",
            DeathLinkReceiveType.BAD_BREATH   => "bad_breath",
            DeathLinkReceiveType.RANDOM       => "random",
            _ => "generic",
        };

        return String.Format(FhApi.Localization.localize(message_id), source_player);
    }

    public void post_deathlink(DeathLink death_msg) {
        if (!_deathlink_enabled) return;
        _deathlinks_queued += 1;

        // Display a toast
        ToastModule.Toast deathlink_toast = new(
            [
                new(DEATHLINK_COLOR, "Deathlink received!"),
            ],
            [
                new(new(1f), death_msg.Cause ?? _get_backup_deathlink_received_text(death_msg.Source)),
            ]
        );

        _toasts?.queue_toast(deathlink_toast);
    }
}
