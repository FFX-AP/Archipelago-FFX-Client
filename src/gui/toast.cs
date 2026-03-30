using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;

using Fahrenheit.Modules.ArchipelagoFFX.GUI;

using Hexa.NET.ImGui;

namespace Fahrenheit.Modules.ArchipelagoFFX;

[FhLoad(FhGameId.FFX)]
public class ToastModule : FhModule {
    //TODO: Change these to be FhSettings when that API is functional
    private const float _TOAST_MARGIN = 0.01f; // as percentage of screen width
    private const float _TOAST_MIN_WIDTH = 0.1f; // as percentage of screen width
    private const float _TOAST_MAX_WIDTH = 0.4f; // as percentage of screen width

    private const float _TOAST_RIGHT_EXTRA_PADDING = 20f;

    private static float toast_margin => _TOAST_MARGIN * ImGui.GetIO().DisplaySize.X;
    private static float toast_min_width => _TOAST_MIN_WIDTH * ImGui.GetIO().DisplaySize.X;
    private static float toast_max_width => _TOAST_MAX_WIDTH * ImGui.GetIO().DisplaySize.X;

    public record ToastMessagePart(Vector4 color, string text, string prefix = " ");

    public class Toast {
        internal static readonly TimeSpan TOAST_TIME = TimeSpan.FromSeconds(5);
        internal static readonly TimeSpan TOAST_FADE_TIME = TimeSpan.FromSeconds(0.3);

        internal enum ToastPhase {
            QUEUED,
            APPEARING,
            SHOWN,
            DISAPPEARING,
            DONE,
        }

        public ToastMessagePart[] title;
        public ToastMessagePart[] description;
        public readonly DateTime time;

        internal ToastPhase phase;
        internal Vector2? pos;

        public Toast(ToastMessagePart[] title, ToastMessagePart[] description) {
            this.title = title;
            this.description = description;

            time = DateTime.Now;
            phase = ToastPhase.QUEUED;
        }

        internal float get_alpha() {
            TimeSpan time_spent_fading = DateTime.Now - time - TOAST_TIME;
            float alpha = (float)(time_spent_fading.TotalSeconds / TOAST_FADE_TIME.TotalSeconds);
            return phase < ToastPhase.DISAPPEARING
                ? 1.0f
                : float.Lerp(1f, 0f, float.Clamp(alpha, 0f, 1f));
        }

        internal Vector2 get_size() {
            ImGuiStylePtr style = ImGui.GetStyle();

            Vector2 size = style.WindowPadding * 2;

            ToastMessagePart? part = null;

            float temp_width = 0.0f;
            float max_width = 0.0f;

            ImGui.PushFont(null, ImGui.GetFontSize() * 0.9f);

            for (int part_idx = 0; part_idx < description.Length; part_idx++) {
                part = description[part_idx];

                if (part_idx != 0 && part.prefix.Length > 0) {
                    Vector2 prefix_size = ImGui.CalcTextSize(part.prefix);

                    if (part.prefix != "\n") {
                        temp_width += prefix_size.X;
                    } else {
                        max_width = float.Max(max_width, temp_width);
                        temp_width = 0.0f;
                        size.Y += prefix_size.Y;
                    }
                }

                temp_width += ImGui.CalcTextSize(part.text).X;
            }

            if (part is not null && (part.prefix.Length > 0 || part.text.Length > 0)) {
                size.Y += ImGui.CalcTextSize(part.text).Y;
            }

            max_width = float.Max(max_width, temp_width);

            ImGui.PopFont();

            part = null;

            for (int part_idx = 0; part_idx < title.Length; part_idx++) {
                part = title[part_idx];

                if (part_idx != 0 && part.prefix.Length > 0) {
                    Vector2 prefix_size = ImGui.CalcTextSize(part.prefix);

                    if (part.prefix != "\n") {
                        temp_width += prefix_size.X;
                    } else {
                        max_width = float.Max(max_width, temp_width);
                        temp_width = 0.0f;
                        size.Y += prefix_size.Y;
                    }
                }

                temp_width += ImGui.CalcTextSize(part.text).X;
            }

            if (part is not null && (part.prefix.Length > 0 || part.text.Length > 0)) {
                size.Y += ImGui.CalcTextSize(part.text).Y;
            }

            max_width = float.Max(max_width, temp_width);

            size.X += max_width + _TOAST_RIGHT_EXTRA_PADDING;
            size.X = float.Clamp(size.X, toast_min_width, toast_max_width);

            return size;
        }
    }

    private readonly LinkedList<Toast> _toast_queue = [];

    private FhModContext? _mod_context;
    private FileStream? _global_state;

    public override bool init(FhModContext mod_context, FileStream global_state_file) {
        _mod_context = mod_context;
        _global_state = global_state_file;

        return true;
    }

    public void queue_toast(Toast new_toast) {
        _toast_queue.AddLast(new_toast);
    }

    public override void render_imgui() {
#if DEBUG
        if (ImGui.IsKeyPressed(ImGuiKey.Apostrophe)) {
            queue_toast(new(
                [
                    new(new(1.0f, 1.0f, 0.6f, 1.0f), "My Debug Toast"),
                ],

                [
                    new(new(1.0f), "My Debug Toast is very cool"),
                ]
            ));
        }
#endif

        // Set up Archipelago's font size
        int font_size = ArchipelagoGUI.font_size;
        if (font_size == -1) font_size = (int)ImGui.GetFontSize();
        ImGui.PushFont(null, font_size);

        ImGuiWindowFlags window_flags =
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

        if (!ImGui.Begin("Toasts", window_flags)) {
            ImGui.PopFont();
            ImGui.End();
            return;
        }

        Toast debug_toast1 = new(
            [ new(new(1.0f, 1.0f, 0.6f, 1.0f), "My Debug Toast 1") ],
            [ new(new(1.0f), "My Debug Toast 1 is very cool") ]
        );

        Toast debug_toast2 = new(
            [ new(new(1.0f, 1.0f, 0.6f, 1.0f), "My Debug Toast 2") ],
            [ new(new(1.0f), "My Debug Toast 2 is very cool") ]
        );

        Vector2 toast1_size = debug_toast1.get_size();
        debug_toast1.pos = new(io.DisplaySize.X - toast1_size.X - toast_margin, toast_margin);

        render_toast(debug_toast1);

        Vector2 toast2_size = debug_toast2.get_size();
        debug_toast2.pos = new(io.DisplaySize.X - toast2_size.X - toast_margin, toast_margin + toast1_size.Y + toast_margin);

        render_toast(debug_toast2);

        //TODO: Update phase and position of the enqueued toasts.
        var toast = _toast_queue.First;
        for (int toast_idx = 0; toast_idx < _toast_queue.Count; toast_idx++) {
            if (toast is null) continue;
            if (toast.Value.phase == Toast.ToastPhase.DONE) break;

            toast = toast.Next;
        }

        ImGui.PopFont();
        ImGui.End();
    }

    private void render_toast(Toast toast) {
        ImGuiWindowFlags toast_window_flags =
            ImGuiWindowFlags.NoBringToFrontOnFocus
          | ImGuiWindowFlags.NoDecoration
          | ImGuiWindowFlags.NoDocking
          | ImGuiWindowFlags.NoFocusOnAppearing
          | ImGuiWindowFlags.NoInputs
          | ImGuiWindowFlags.NoMove
          | ImGuiWindowFlags.NoScrollbar;

        ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 2.0f);

        ImGui.SetNextWindowPos(toast.pos!.Value);
        ImGui.SetNextWindowSize(new(ImGui.GetIO().DisplaySize.X - toast.pos!.Value.X - toast_margin, 0));

        if (ImGui.Begin($"Toast##{toast.pos!.Value.GetHashCode()}", toast_window_flags)) {
            render_message(toast.title);

            ImGui.PushFont(null, ImGui.GetFontSize() * 0.9f);

            render_message(toast.description);

            ImGui.PopFont();
        }

        ImGui.End();

        ImGui.PopStyleVar();
    }

    private void render_message(ToastMessagePart[] message) {
        for (int part_idx = 0; part_idx < message.Length; part_idx++) {
            ToastMessagePart part = message[part_idx];

            if (part_idx != 0) {
                if (!part.prefix.StartsWith('\n')) {
                    ImGui.SameLine(0, 0);
                    ImGui.TextColored(part.color, part.prefix);
                } else {
                    ImGui.TextColored(part.color, part.prefix[1..]);
                }

                ImGui.SameLine(0, 0);
            }

            ImGui.TextColored(part.color, part.text);
        }
    }
}
