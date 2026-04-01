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
using Fahrenheit.Modules.ArchipelagoFFX;
using Fahrenheit.Modules.ArchipelagoFFX.GUI;

using Hexa.NET.ImGui;

[FhLoad(FhGameId.FFX)]
public unsafe class RecentItemsModule : FhModule {
    private static class Colors {
        private static Vector4 color_to_vector4(Color color) {
            return new Vector4(color.R / 255f, color.G / 255f, color.B / 255f, 1.0f);
        }

        // Pascal case names to mimic enums
        private static readonly Vector4 BrightenFactor = new(0.2f, 0.2f, 0.2f, 0.0f);

        public static readonly Vector4 Default = color_to_vector4(Color.White);

        public static readonly Vector4 PlayerSelf  = color_to_vector4(Color.Magenta) + BrightenFactor;
        public static readonly Vector4 PlayerOther = color_to_vector4(Color.Yellow);

        public static readonly Vector4 ItemFiller = color_to_vector4(Color.Cyan);
        public static readonly Vector4 ItemTrap   = color_to_vector4(Color.Salmon);
        public static readonly Vector4 ItemProg   = color_to_vector4(Color.Plum);
        public static readonly Vector4 ItemUseful = color_to_vector4(Color.SlateBlue) + BrightenFactor;

        // Location color is slightly modified to be more readable on dark backgrounds
        public static readonly Vector4 Location = color_to_vector4(Color.Green) + BrightenFactor;

        public static Vector4 get_item_color(ItemInfo item) {
            Vector4 item_color = ItemFiller;

            if (item.Flags.HasFlag(ItemFlags.Trap)) {
                item_color = ItemTrap;
            } else if (item.Flags.HasFlag(ItemFlags.Advancement)) {
                item_color = ItemProg;
            } else if (item.Flags.HasFlag(ItemFlags.NeverExclude)) {
                item_color = ItemUseful;
            }

            return item_color;
        }
    }

    public enum RecentItemsInterpolation {
        SMOOTH = 0,
        INSTANT = 1,
    }

    public enum RecentItemsFadeMethod {
        FADE = 0,
        SLIDE = 1,
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
        public readonly FhSettingNumber<int> item_count = new("item_count", 4, 0, 10, 1);

        //TODO: Implement smooth scrolling
        public readonly FhSettingDropdown<RecentItemsInterpolation> animation = new("interpolation", RecentItemsInterpolation.SMOOTH);

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

    [Flags]
    public enum RecentItemRelevance {
        Impersonal = 0,
        Sender = 1,
        Receiver = 2,
    }

    public record RecentItemInfo(RecentItemRelevance relevance, PlayerInfo sender, PlayerInfo receiver, ItemInfo item);

    private static List<(Vector4 color, string part)> construct_message(RecentItemInfo info) {
        ItemInfo item = info.item;

        Vector4 item_color = Colors.get_item_color(item);
        Vector4 sender_color =
            info.relevance.HasFlag(RecentItemRelevance.Sender)
                ? Colors.PlayerSelf
                : Colors.PlayerOther;

        Vector4 receiver_color =
            info.relevance.HasFlag(RecentItemRelevance.Receiver)
                ? Colors.PlayerSelf
                : Colors.PlayerOther;

        if (info.receiver == info.sender) {
            // Player found their item
            return [
                ( receiver_color, info.receiver.Alias ),
                ( Colors.Default, "found their" ),
                ( item_color, item.ItemDisplayName ),
            ];
        }

        // Amy sent item to Basket
        return [
            ( sender_color, info.sender.Alias ),
            ( Colors.Default, "sent" ),
            ( item_color, item.ItemDisplayName ),
            ( Colors.Default, "to" ),
            ( receiver_color, info.receiver.Alias ),
        ];
    }

    private static ToastModule.Toast construct_toast(RecentItemInfo info) {
        ToastModule.ToastMessagePart[] description;

        ItemInfo item = info.item;

        Vector4 item_color = Colors.get_item_color(item);
        string item_name = item.ItemDisplayName;
        if (item_name.Length > 50) {
            item_name = $"{item_name[..47]}...";
        }

        Vector4 sender_color =
            info.relevance.HasFlag(RecentItemRelevance.Sender)
                ? Colors.PlayerSelf
                : Colors.PlayerOther;
        string sender_name = info.sender.Alias;
        if (sender_name.Length > 20) {
            sender_name = $"{sender_name[..17]}...";
        }

        Vector4 receiver_color =
            info.relevance.HasFlag(RecentItemRelevance.Receiver)
                ? Colors.PlayerSelf
                : Colors.PlayerOther;
        string receiver_name = info.receiver.Alias;
        if (receiver_name.Length > 20) {
            receiver_name = $"{sender_name[..17]}...";
        }

        string location_name = item.LocationDisplayName;
        if (location_name.Length > 50) {
            location_name = $"{location_name[..47]}...";
        }

        if (info.receiver == info.sender) {
            // Player found their item
            description = [
                new(receiver_color, receiver_name),
                new(Colors.Default, "found their"),
                new(item_color, item_name),
                new(Colors.Location, $"\t{location_name}", "\n"),
            ];
        } else {
            // Amy sent item to Basket
            description = [
                new(sender_color, sender_name),
                new(Colors.Default, "sent"),
                new(item_color, item_name),
                new(Colors.Default, "to"),
                new(receiver_color, receiver_name),
                new(Colors.Location, $"\t{location_name}", "\n"),
            ];
        }

        // No title
        return new([], description);
    }

    public static LinkedList<RecentItemInfo> recent_items = [ ];
    private FhModuleHandle<ToastModule> _handle_toast_module;
    private static ToastModule? _toasts;

    public RecentItemsModule() {
        _handle_toast_module = new(this);

        // settings = new FhSettingsCategory(
        //     "recent_items",
        //     [
        //         new FhSettingsCategory(
        //             "filter",
        //             [
        //                 module_settings.display_items,
        //                 module_settings.display_only_personal,
        //                 module_settings.display_locations,
        //                 module_settings.item_count,
        //             ]
        //         ),
        //
        //         new FhSettingsCategory(
        //             "visuals",
        //             [
        //                 module_settings.animation,
        //                 module_settings.fade_method,
        //                 module_settings.fade_after,
        //                 module_settings.background,
        //             ]
        //         ),
        //
        //         new FhSettingsCategory(
        //             "position",
        //             [
        //                 module_settings.pos_x,
        //                 module_settings.pos_y,
        //                 module_settings.alignment,
        //             ]
        //         ),
        //     ]
        // );
    }

    public override bool init(FhModContext mod_context, FileStream global_state_file) {
        return _handle_toast_module.try_get_module(out _toasts);
    }

    public static void post_item_message(LogMessage message) {
        if (message is not ItemSendLogMessage send_message) return;
        if (message is HintItemSendLogMessage) return;

        RecentItemRelevance relevance = RecentItemRelevance.Impersonal;

        if (send_message.IsSenderTheActivePlayer) {
            relevance |= RecentItemRelevance.Sender;
        }

        if (send_message.IsReceiverTheActivePlayer) {
            relevance |= RecentItemRelevance.Receiver;
        }

        RecentItemInfo info = new(relevance, send_message.Sender, send_message.Receiver, send_message.Item);

        _toasts!.queue_toast(construct_toast(info));
        // recent_items.AddFirst(info);
    }

    public override void render_imgui() {
        return;

        if (!module_settings.display_items.get()) return;

        // Do not display anything where it'd be rude to
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
        for (int i = 0; i < module_settings.item_count.get(); i++) {
            if (item is null) break;

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
        List<(Vector4 color, string part)> message = construct_message(info);

        for (int i = 0; i < message.Count; i++) {
            ImGui.TextColored(message[i].color, message[i].part);

            if (i != message.Count - 1) {
                ImGui.SameLine();
            }
        }

        if (module_settings.display_locations.get()) {
            ImGui.Indent();

            ImGui.TextColored(Colors.Default, "(");
            ImGui.SameLine();
            ImGui.TextColored(Colors.Location, info.item.LocationDisplayName);
            ImGui.SameLine();
            ImGui.TextColored(Colors.Default, ")");

            ImGui.Unindent();
        }
    }
}
