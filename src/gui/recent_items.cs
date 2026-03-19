using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;

using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Helpers;
using Archipelago.MultiClient.Net.MessageLog.Messages;
using Archipelago.MultiClient.Net.Models;

using Fahrenheit;
using Fahrenheit.FFX;
using Fahrenheit.Modules.ArchipelagoFFX.GUI;

using Hexa.NET.ImGui;

[FhLoad(FhGameId.FFX)]
public unsafe class RecentItemsModule : FhModule {
    public enum RecentItemsAnimation {
        SMOOTH = 0,
        INSTANT = 1,
    }

    public enum RecentItemsFadeMethod {
        FADE = 0,
        SLIDE = 1,
        CAPTURE = 2, // Like FADE, but with a dark blue tint
    }

    public enum RecentItemsBackground {
        NONE = 0,
        PER_ITEM = 1,
        BLOCK = 2,
    }

    public enum RecentItemsTextAlignment {
        LEFT = 0,
        CENTER = 1,
        RIGHT = 2,
    }

    // This is messy given `FhModule.settings` exists, but it allows us to access them more easily
    // Accessing settings through `FhModule.settings` goes through an array, which is rather unpleasant
    // So `RecentItemsSettings` exists to provide flat access to all settings
    public class RecentItemsSettings {
        public readonly FhSettingToggle display_items = new("display_items", true);
        public readonly FhSettingToggle display_only_personal = new("display_only_personal", false);
        public readonly FhSettingToggle display_locations = new("display_locations", true);
        public readonly FhSettingNumber<int> item_count = new("item_count", 3, 0, 10, 1);

        //TODO: Implement smooth scrolling
        public readonly FhSettingDropdown<RecentItemsAnimation> animation = new("animation", RecentItemsAnimation.SMOOTH);

        //TODO: Implement old items fading away
        public readonly FhSettingNumber<float> fade_after = new("fade_after", 10.0f, 0.0f, 60.0f, 1.0f);
        public readonly FhSettingDropdown<RecentItemsFadeMethod> fade_method = new("fade_method", RecentItemsFadeMethod.SLIDE);

        //TODO: Implement different background behavior
        public readonly FhSettingDropdown<RecentItemsBackground> background = new("background", RecentItemsBackground.NONE);

        //TODO: Implement configurable positioning
        public readonly FhSettingNumber<float> pos_x = new("x", 0.05f, 0.0f, 1.0f, 0.1f);
        public readonly FhSettingNumber<float> pos_y = new("y", 0.34f, 0.0f, 1.0f, 0.1f);
        public readonly FhSettingDropdown<RecentItemsTextAlignment> alignment = new("alignment", RecentItemsTextAlignment.LEFT);
    }

    public RecentItemsSettings module_settings = new();

    public enum RecentItemDirection {
        SelfReceive,
        Receive,
        Send,
    }

    [Flags]
    public enum RecentItemRelevance {
        Impersonal = 0,
        Sender = 1,
        Receiver = 2,
    }

    public record RecentItemInfo(RecentItemDirection direction, RecentItemRelevance relevance, PlayerInfo sender, PlayerInfo receiver, ItemInfo item);

    public static LinkedList<RecentItemInfo> recent_items = [ ];

    public RecentItemsModule() {
        settings = new FhSettingsCategory(
            "recent_items",
            [
                new FhSettingsCategory(
                    "filter",
                    [
                        module_settings.display_items,
                        module_settings.display_only_personal,
                        module_settings.display_locations,
                        module_settings.item_count,
                    ]
                ),

                new FhSettingsCategory(
                    "visuals",
                    [
                        module_settings.animation,
                        module_settings.fade_method,
                        module_settings.fade_after,
                        module_settings.background,
                    ]
                ),

                new FhSettingsCategory(
                    "position",
                    [
                        module_settings.pos_x,
                        module_settings.pos_y,
                        module_settings.alignment,
                    ]
                ),
            ]
        );
    }

    public override bool init(FhModContext mod_context, FileStream global_state_file) {
        return true;
    }

    public static void post_item_message(LogMessage message) {
        if (message is not ItemSendLogMessage send_message) return;
        if (message is HintItemSendLogMessage) return;

        RecentItemDirection direction = RecentItemDirection.Send;
        if (send_message.Sender == send_message.Receiver) {
            direction = RecentItemDirection.SelfReceive;
        } else if (send_message.IsReceiverTheActivePlayer) {
            direction = RecentItemDirection.Receive;
        }

        RecentItemRelevance relevance = RecentItemRelevance.Impersonal;

        if (send_message.IsSenderTheActivePlayer) {
            relevance |= RecentItemRelevance.Sender;
        }

        if (send_message.IsReceiverTheActivePlayer) {
            relevance |= RecentItemRelevance.Receiver;
        }

        RecentItemInfo info = new(direction, relevance, send_message.Sender, send_message.Receiver, send_message.Item);
        recent_items.AddFirst(info);
    }

    private Vector4 color_to_vector4(Color color) {
        return new Vector4(color.R / 255f, color.G / 255f, color.B / 255f, 1.0f);
    }

#if DEBUG
    private readonly Random bogus_item_rng = new();
    private readonly LinkedList<(bool direction, int item_rng, int player_rng)> bogus_items = [];
    private static float bogus_item_gen_seconds_passed;

    public override void post_update() {
        bogus_item_gen_seconds_passed += 1.0f/60.0f;

        if (bogus_item_gen_seconds_passed > 20f) {
            bogus_item_gen_seconds_passed -= 20f;

            bogus_items.AddFirst((
                bogus_item_rng.Next(0, 2) == 0,
                bogus_item_rng.Next(0, 4),
                bogus_item_rng.Next(0, 20)
            ));
        }
    }
#endif

    public override void render_imgui() {
        if (!module_settings.display_items.get()) return;

        // Do not display anything where it'd be rude
        if (Globals.save_data->current_room_id ==  23 // Main Menu
         || Globals.save_data->current_room_id ==   0 // Tutorial room
         || Globals.save_data->current_room_id == 348 // Intro
         || Globals.save_data->current_room_id == 382 // Airship Menu
        ) {
            return;
        }

        // Set up Archipelago's font size
        int font_size = ArchipelagoGUI.font_size;
        if (font_size == -1) font_size = (int)ImGui.GetFontSize();
        ImGui.PushFont(null, font_size);

        ImGuiWindowFlags display_flags =
                ImGuiWindowFlags.NoBackground
              | ImGuiWindowFlags.NoBringToFrontOnFocus
              | ImGuiWindowFlags.NoDecoration
              | ImGuiWindowFlags.NoDocking
              | ImGuiWindowFlags.NoFocusOnAppearing
              | ImGuiWindowFlags.NoInputs
              | ImGuiWindowFlags.NoMove
              | ImGuiWindowFlags.NoScrollbar;

        var io = ImGui.GetIO();

        ImGui.SetNextWindowSize(io.DisplaySize);
        ImGui.SetNextWindowPos(new Vector2());

        if (!ImGui.Begin("Recent Items", display_flags)) {
            ImGui.End();
            ImGui.PopFont();
            return;
        }

        Vector2 start_pos = new(
            io.DisplaySize.X * module_settings.pos_x.get(),
            io.DisplaySize.Y * module_settings.pos_y.get()
        );
        ImGui.SetCursorPos(start_pos);
        ImGui.Dummy(new());

        var item = recent_items.First;
#if DEBUG
        var bogus_item = bogus_items.First;
#endif
        for (int i = 0; i < module_settings.item_count.get(); i++) {
#if DEBUG
            if (item is null) {
                if (bogus_item is null) break;

                render_bogus_item(bogus_item.Value);
                bogus_item = bogus_item.Next;
                continue;
            }
#else
            if (item is null) break;
#endif

            if (module_settings.display_only_personal.get() && item.Value.relevance == RecentItemRelevance.Impersonal) {
                // Skip this item
                i--;
                continue;
            }

            render_item(item.Value);
            item = item.Next;
        }

        ImGui.PopFont();
        ImGui.End();
    }

    private void render_item(RecentItemInfo info) {
        ItemInfo item = info.item;

        Color item_color = Color.Cyan;
        if (item.Flags.HasFlag(ItemFlags.Trap)) {
            item_color = Color.Salmon;
        } else if (item.Flags.HasFlag(ItemFlags.Advancement)) {
            item_color = Color.Plum;
        } else if (item.Flags.HasFlag(ItemFlags.NeverExclude)) {
            item_color = Color.SlateBlue;
        }

        List<(Color color, string part)> message = info.direction switch {
            RecentItemDirection.SelfReceive => [
                (info.relevance.HasFlag(RecentItemRelevance.Receiver) ? Color.Magenta : Color.Yellow, info.receiver.Alias),
                (Color.White, "found their"),
                (item_color, item.ItemDisplayName),
            ],

            RecentItemDirection.Receive => [
                (info.relevance.HasFlag(RecentItemRelevance.Receiver) ? Color.Magenta : Color.Yellow, info.receiver.Alias),
                (Color.White, "received"),
                (item_color, item.ItemDisplayName),
                (Color.White, "from"),
                (Color.Yellow, info.sender.Alias),
            ],

            RecentItemDirection.Send => [
                (info.relevance.HasFlag(RecentItemRelevance.Sender) ? Color.Magenta : Color.Yellow, info.sender.Alias),
                (Color.White, "sent"),
                (item_color, item.ItemDisplayName),
                (Color.White, "to"),
                (Color.Yellow, info.receiver.Alias),
            ],

            _ => throw new NotImplementedException(),
        };

        for (int i = 0; i < message.Count; i++) {
            ImGui.TextColored(color_to_vector4(message[i].color), message[i].part);

            if (i != message.Count - 1) {
                ImGui.SameLine();
            }
        }

        if (module_settings.display_locations.get()) {
            ImGui.Indent();

            ImGui.TextColored(color_to_vector4(Color.White), "(");
            ImGui.SameLine();
            ImGui.TextColored(color_to_vector4(Color.Green) + new Vector4(0.2f, 0.2f, 0.2f, 0.0f), item.LocationDisplayName);
            ImGui.SameLine();
            ImGui.TextColored(color_to_vector4(Color.White), ")");

            ImGui.Unindent();
        }
    }

#if DEBUG
    private void render_bogus_item((bool direction, int item_rng, int player_rng) item) {
        (string item_name, Color item_color) = item.item_rng switch {
            1 => ("A Trap", Color.Salmon),
            2 => ("Prog", Color.Plum),
            3 => ("A Goodie", Color.SlateBlue),
            _ => ("Trash", Color.Cyan),
        };

        string verb = item.direction ? "Sent" : "Received";
        string direction = item.direction ? "to" : "from";

        string player = item.player_rng switch {
             0 => "Tidus",
             1 => "Yuna",
             2 => "Auron",
             3 => "Kimahri",
             4 => "Wakka",
             5 => "Lulu",
             6 => "Rikku",
             7 => "Seymour",
             8 => "Valefor",
             9 => "Ifrit",
            10 => "Ixion",
            11 => "Shiva",
            12 => "Bahamut",
            13 => "Anima",
            14 => "Yojimbo",
            15 => "Cindy",
            16 => "Sandy",
            17 => "Mindy",
            18 => "You",
            _  => "????",
        };

        ImGui.TextColored(color_to_vector4(Color.White), verb);
        ImGui.SameLine();
        ImGui.TextColored(color_to_vector4(item_color), item_name);
        ImGui.SameLine();
        ImGui.TextColored(color_to_vector4(Color.White), direction);
        ImGui.SameLine();
        ImGui.TextColored(color_to_vector4(Color.Yellow), player);

        if (module_settings.display_locations.get()) {
            ImGui.Indent();

            ImGui.TextColored(color_to_vector4(Color.White), "(");
            ImGui.SameLine();
            ImGui.TextColored(color_to_vector4(Color.Green) + new Vector4(0.2f, 0.2f, 0.2f, 0.0f), "Bogus Item for Debugging");
            ImGui.SameLine();
            ImGui.TextColored(color_to_vector4(Color.White), ")");

            ImGui.Unindent();
        }
    }
#endif
}
