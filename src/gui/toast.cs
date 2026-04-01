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

    private const int _MAX_TOASTS_SHOWN = 3;

    private static float toast_margin => _TOAST_MARGIN * ImGui.GetIO().DisplaySize.X;
    private static float toast_min_width => _TOAST_MIN_WIDTH * ImGui.GetIO().DisplaySize.X;
    private static float toast_max_width => _TOAST_MAX_WIDTH * ImGui.GetIO().DisplaySize.X;

    // This is a horrible way of limiting the amount of toasts. Will improve in the future.
    //TODO: Figure out a better way of counting toasts.
    internal static int toasts_shown = 0;

    public record ToastMessagePart(Vector4 color, string text, string prefix = " ");

    public class Toast {
        internal static readonly TimeSpan TOAST_APPEAR_TIME = TimeSpan.FromSeconds(0.3);
        internal static readonly TimeSpan TOAST_SHOW_TIME = TimeSpan.FromSeconds(5);
        internal static readonly TimeSpan TOAST_DISAPPEAR_TIME = TimeSpan.FromSeconds(0.3);

        internal enum ToastPhase {
            QUEUED,
            APPEARING,
            SHOWN,
            DISAPPEARING,
            DONE,
        }

        public ToastMessagePart[] title;
        public ToastMessagePart[] description;

        internal DateTime phase_time;
        internal ToastPhase phase;
        internal Vector2? pos;

        public Toast(ToastMessagePart[] title, ToastMessagePart[] description) {
            this.title = title;
            this.description = description;

            phase = ToastPhase.QUEUED;
            phase_time = DateTime.Now;
        }

        internal void increment_phase() {
            if (phase == ToastPhase.DONE) return;


            phase += 1;
            phase_time = DateTime.Now;

            if (phase == ToastPhase.APPEARING) {
                toasts_shown += 1;
            } else if (phase == ToastPhase.DISAPPEARING) {
                toasts_shown -= 1;
            }
        }

        internal float get_phase_t() {
            TimeSpan time_spent_in_phase = DateTime.Now - phase_time;

            TimeSpan max_time = phase switch {
                ToastPhase.APPEARING => TOAST_APPEAR_TIME,
                ToastPhase.SHOWN => TOAST_SHOW_TIME,
                ToastPhase.DISAPPEARING => TOAST_DISAPPEAR_TIME,

                ToastPhase.QUEUED or ToastPhase.DONE => throw new InvalidOperationException(),

                _ => throw new NotImplementedException(),
            };

            return float.Clamp((float)(time_spent_in_phase.TotalMilliseconds/max_time.TotalMilliseconds), 0.0f, 1.0f);
        }

        internal float get_alpha(float t) {
            if (phase < ToastPhase.DISAPPEARING) return 1.0f;
            if (phase == ToastPhase.DONE) return 0.0f;

            return float.Lerp(1f, 0f, t);
        }

        internal Vector2 get_size() {
            // This algorithm is horrible and fails to account for quite a few edgecases
            // However, I am sick of accounting for linebreaks, so a different solution will come later.
            // Said future solution is likely to involve pre-splitting the individual parts of a message on linebreaks.

            ImGuiStylePtr style = ImGui.GetStyle();

            Vector2 size = style.WindowPadding * 2;

            ToastMessagePart? part = null;

            float temp_width = 0.0f;
            float max_width = 0.0f;

            ImGui.PushFont(null, ImGui.GetFontSize() * 0.9f);

            float line_height = ImGui.GetTextLineHeightWithSpacing();

            for (int part_idx = 0; part_idx < description.Length; part_idx++) {
                part = description[part_idx];

                Vector2 text_size = ImGui.CalcTextSize(part.text);
                Vector2 prefix_size = ImGui.CalcTextSize(part.prefix);

                if (part.prefix.Contains("\n")) {
                    max_width = float.Max(max_width, temp_width);
                    temp_width = 0.0f;
                    size.Y += line_height;
                } else {
                    temp_width += prefix_size.X;
                }

                if (part.text.Contains("\n")) {
                    max_width = float.Max(max_width, temp_width);
                    temp_width = 0.0f;
                    size.Y += line_height;
                } else {
                    temp_width += text_size.X;
                }
            }

            if (part is not null && (part.prefix.Length > 0 || part.text.Length > 0)) {
                size.Y += ImGui.GetTextLineHeight();
            }

            max_width = float.Max(max_width, temp_width);

            ImGui.PopFont();

            line_height = ImGui.GetTextLineHeightWithSpacing();

            part = null;
            temp_width = 0.0f;

            for (int part_idx = 0; part_idx < title.Length; part_idx++) {
                part = title[part_idx];

                Vector2 text_size = ImGui.CalcTextSize(part.text);
                Vector2 prefix_size = ImGui.CalcTextSize(part.prefix);

                if (part.prefix.Contains("\n")) {
                    max_width = float.Max(max_width, temp_width);
                    temp_width = 0.0f;
                    size.Y += line_height;
                } else {
                    temp_width += prefix_size.X;
                }

                if (part.text.Contains("\n")) {
                    max_width = float.Max(max_width, temp_width);
                    temp_width = 0.0f;
                    size.Y += line_height;
                } else {
                    temp_width += text_size.X;
                }

                temp_width += ImGui.CalcTextSize(part.text).X;
            }

            if (part is not null && (part.prefix.Length > 0 || part.text.Length > 0)) {
                size.Y += line_height;
            }

            max_width = float.Max(max_width, temp_width);

            size.X = max_width + _TOAST_RIGHT_EXTRA_PADDING;
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
        _toast_queue.AddFirst(new_toast);
    }

#if DEBUG
    private static bool is_debug_open = false;
    private static int spawned_debug_toasts = 0;
    private void render_debug() {
        if (ImGui.IsKeyPressed(ImGuiKey.Backslash)) {
            is_debug_open = !is_debug_open;
        }

        if (!is_debug_open) return;

        if (!ImGui.Begin("Toast Debug")) {
            ImGui.End();
            return;
        }

        var toast_node = _toast_queue.First;
        for (int i = 0; i < _toast_queue.Count; i++) {
            Toast toast = toast_node!.Value;

            ImGui.SeparatorText($"Toast {i}");
            ImGui.Indent();

            ImGui.Text($"Phase: {toast.phase}");

            if (toast.phase is Toast.ToastPhase.APPEARING or Toast.ToastPhase.SHOWN or Toast.ToastPhase.DISAPPEARING) {
                ImGui.Text($"Phase T: {toast.get_phase_t()}");

                Vector2 toast_size = toast.get_size();
                ImGui.Text($"Size: ({toast_size.X}, {toast_size.Y})");

                if (toast.pos.HasValue) {
                    ImGui.Text($"Position: ({toast.pos.Value.X}, {toast.pos.Value.Y})");
                }
            }

            ImGui.Unindent();

            toast_node = toast_node!.Next;
        }

        ImGui.End();
    }
#endif

    public override void render_imgui() {
#if DEBUG
        render_debug();

        if (ImGui.IsKeyPressed(ImGuiKey.Apostrophe)) {
            queue_toast(new(
                [
                    new(new(1.0f, 1.0f, 0.6f, 1.0f), $"My Debug Toast {spawned_debug_toasts}"),
                ],

                [
                    new(new(1.0f), $"My Debug Toast is very cool {spawned_debug_toasts}"),
                ]
            ));
            spawned_debug_toasts += 1;
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

        float base_y = 0;

        List<LinkedListNode<Toast>> nodes_to_remove = [];

        var toast_node = _toast_queue.First;
        for (int toast_idx = 0; toast_idx < _toast_queue.Count; toast_idx++) {
            Toast toast = toast_node!.Value;

            Vector2 toast_size;
            float phase_t;

            switch (toast.phase) {
                case Toast.ToastPhase.QUEUED:
                    // This is a horrible way of making sure appearing toasts don't jump around.
                    //TODO: Figure out an actual way to fix said issue in lieu of allowing only one toast to appear at a time.
                    if (toasts_shown < _MAX_TOASTS_SHOWN && (toast_node.Next is null || toast_node.Next.Value.phase > Toast.ToastPhase.APPEARING)) {
                        toast.increment_phase();
                    }
                    break;

                case Toast.ToastPhase.APPEARING:
                    toast_size = toast.get_size();
                    phase_t = toast.get_phase_t();

                    toast.pos = new(
                        io.DisplaySize.X - toast_margin - toast_size.X,
                        float.Lerp(-toast_size.Y, base_y + toast_margin, phase_t)
                    );

                    render_toast(toast);

                    if (phase_t == 1.0f) {
                        toast.increment_phase();
                    }

                    base_y += toast.pos.Value.Y + toast_size.Y;

                    break;

                case Toast.ToastPhase.SHOWN:
                    toast_size = toast.get_size();
                    phase_t = toast.get_phase_t();

                    toast.pos = new(
                        io.DisplaySize.X - toast_margin - toast_size.X,
                        base_y + toast_margin
                    );

                    if (phase_t == 1.0f) {
                        toast.increment_phase();
                    }

                    render_toast(toast);

                    base_y += toast_margin + toast_size.Y;
                    break;

                case Toast.ToastPhase.DISAPPEARING:
                    toast_size = toast.get_size();
                    phase_t = toast.get_phase_t();

                    toast.pos = new(
                        float.Lerp(io.DisplaySize.X - toast_margin - toast_size.X, io.DisplaySize.X, phase_t),
                        base_y + toast_margin
                    );

                    if (phase_t == 1.0f) {
                        toast.increment_phase();
                    }

                    ImGui.PushStyleVar(ImGuiStyleVar.Alpha, toast.get_alpha(phase_t));

                    render_toast(toast);

                    ImGui.PopStyleVar();

                    base_y += toast_margin + toast_size.Y;
                    break;

                case Toast.ToastPhase.DONE:
                    nodes_to_remove.Add(toast_node);
                    break;
            }

            toast_node = toast_node!.Next;
        }

        ImGui.PopFont();
        ImGui.End();

        foreach (var node in nodes_to_remove) {
            _toast_queue.Remove(node);
        }
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

        ImGui.SetNextWindowPos(toast.pos!.Value);
        ImGui.SetNextWindowSize(toast.get_size());

        if (ImGui.Begin($"Toast##{toast.pos!.Value.GetHashCode()}", toast_window_flags)) {
            render_message(toast.title);

            ImGui.PushFont(null, ImGui.GetFontSize() * 0.9f);

            render_message(toast.description);

            ImGui.PopFont();
        }

        ImGui.End();
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
