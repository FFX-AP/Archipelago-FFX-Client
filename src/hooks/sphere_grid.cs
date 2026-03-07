using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

using Hexa.NET.ImGui;

using Fahrenheit.FFX;

using static Fahrenheit.FFX.Globals;

namespace Fahrenheit.Modules.ArchipelagoFFX;

[FhLoad(FhGameId.FFX)]
public unsafe class SphereGridQolModule : FhModule {
    // Delegates for handles
    //TODO: Remove these once FhCall is more up-to-date
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void abmap_get_panel(int ply_id, int node_idx);
    public const nint __addr_abmap_get_panel = 0x6458a0;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void abmap_ctrl();
    public const nint __addr_AbmapState_MovingToTarget = 0x659990;
    public const nint __addr_AbmapState_ChangingNode = 0x647d50;
    public const nint __addr_AbmapState_Warping = 0x647f00;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void abmap_confirm_move(int p1, int p2, int p3);
    public const nint __addr_FUN_00a56160 = 0x656160;

    // Sound IDs
    private const uint SND_PREFIX = 0x80000000;
    public const uint SND_ACTIVATE_NODE   = 0x50 | SND_PREFIX;
    public const uint SND_DEACTIVATE_NODE = 0x6e | SND_PREFIX;

    // Method handles
    private readonly FhMethodHandle<abmap_ctrl> _sphere_grid_move_speed;
    private readonly FhMethodHandle<abmap_confirm_move> _sphere_grid_confirm_move;
    private readonly FhMethodHandle<abmap_ctrl> _sphere_grid_state_moving;
    private readonly FhMethodHandle<abmap_ctrl> _sphere_grid_state_warping;
    private readonly FhMethodHandle<abmap_ctrl> _sphere_grid_change_node;

    // Function pointers
    //TODO: Move this into a library common between modules
    private static delegates.SndSepPlaySimple _SndSepPlaySimple = FhUtil.get_fptr<delegates.SndSepPlaySimple>(delegates.__addr_SndSepPlaySimple);
    private readonly abmap_get_panel _abmap_get_panel = FhUtil.get_fptr<abmap_get_panel>(__addr_abmap_get_panel);

    // Fahrenheit-related
    private FhModContext? _mod_context;
    private FileStream? _global_state;

    // Gui-related
    private static int knots_counted;
    private static bool freeze_move;
    private static bool skip_moves;
    private static bool debug_gui_open;

    private static readonly HashSet<short> temporarily_activated_nodes = [];

    public SphereGridQolModule() {
        const string GAME = "FFX.exe";

        _sphere_grid_move_speed = new(this, GAME, __addr_AbmapState_MovingToTarget, h_move_speed);
        _sphere_grid_confirm_move = new(this, GAME, __addr_FUN_00a56160, h_move_confirm);
        _sphere_grid_state_moving = new(this, GAME, __addr_AbmapState_MovingToTarget, h_state_moving);
        _sphere_grid_state_warping = new(this, GAME, __addr_AbmapState_Warping, h_state_warping);
        _sphere_grid_change_node = new(this, GAME, __addr_AbmapState_ChangingNode, h_change_node);
    }

    public override bool init(FhModContext mod_context, FileStream global_state_file) {
        _mod_context = mod_context;
        _global_state = global_state_file;

        return _sphere_grid_move_speed.hook()
            && _sphere_grid_confirm_move.hook()
            && _sphere_grid_state_moving.hook()
            && _sphere_grid_state_warping.hook()
            && _sphere_grid_change_node.hook();
    }

    private string get_state_name() {
        // Since Fahrenheit doesn't currently expose:
        // - the base address of the game, nor
        // - a first-party way to obtain this value,
        // we have to do this ugly hack.
        return get_state_name_for_address((uint)SphereGrid.lpamng->__0x115A8 - (uint)FhUtil.ptr_at<byte>(0));
    }

    private string get_state_name_for_address(uint address) {
        return address switch{
            0x645010 => "Normal",
            0x644ef0 => "ChoosingMoveTarget",
            0x659990 => "MovingToTarget",
            0x648230 => "AwaitingMoveConfirmation",
            0x6452d0 => "ChoosingActivationTarget",
            0x648280 => "Activating",
            0x659e80 => "HomingOnNode",
            0x647f00 => "Warping",
            0x647d50 => "ClearingNode",
            _ => $"?????? (0x{address:x8})",
        };
    }

    public override void render_imgui() {
#if DEBUG
        if (ImGui.IsKeyPressed(ImGuiKey.GraveAccent))
            debug_gui_open ^= true;

        if (!debug_gui_open) return;

        if (!ImGui.Begin("Sphere Grid Debug")) {
            ImGui.End();
            return;
        }

        render_lpamng_info();

        ImGui.End();
#endif
    }

    private void render_lpamng_info() {
        if (ImGui.CollapsingHeader("LpAbilityMapEngine")) {
            if (!*SphereGrid.is_open) {
                ImGui.Text("Please open the Sphere Grid.");
                return;
            }

            var lpamng = SphereGrid.lpamng;

            ImGui.Text($"{lpamng->node_count} nodes are connected by {lpamng->link_count} links over {lpamng->cluster_count} clusters.");
            ImGui.Text($"State: {get_state_name()}");

            ImGui.SeparatorText("Movement");
            ImGui.Indent();
            render_moving_info();
            ImGui.Unindent();

            ImGui.SeparatorText("Current node");
            ImGui.Indent();

            if (lpamng->selected_node_idx >= lpamng->node_count) {
                ImGui.Text("Please select a node");
            } else {
                render_node_info();
            }

            ImGui.Unindent();

            if (ImGui.Button("Update nodes")) {
                lpamng->should_update = 1;
                lpamng->should_update_node = -1;
            }
        }
    }

    private void render_moving_info() {
        var lpamng = SphereGrid.lpamng;

        float current_move_speed = lpamng->moving_speed;
        float current_t = lpamng->moving_progress;
        float min_t = 0.0f;
        float max_t = 1.0f;

        ImGui.Text($"Moving at {current_move_speed} units per second");

        // Disable the slider to make sure the user can't edit it at will
        ImGui.BeginDisabled();
        ImGui.SliderScalar("Progress", ImGuiDataType.Float, &current_t, &min_t, &max_t);
        ImGui.EndDisabled();

        ImGui.Text($"Knots so far: {knots_counted}");

        if (ImGui.Button(skip_moves ? "Don't Skip Moves" : "Skip Moves")) {
            skip_moves ^= true;
        }

        if (ImGui.Button(freeze_move ? "Unfreeze Move" : "Freeze Move")) {
            freeze_move ^= true;
        }
    }

    private void render_node_info() {
        var lpamng = SphereGrid.lpamng;

        SphereGridNode* selected_node = &lpamng->nodes[lpamng->selected_node_idx];

        ImGui.Text($"Index: {lpamng->selected_node_idx}");

        ImGui.Text($"Position: ({selected_node->x}, {selected_node->y})");

        int link_count = 0;
        for (int i = 0; i < 5; i++) {
            if (selected_node->get_link(i) != null) link_count++;
        }
        ImGui.Text($"Link count: {link_count}");

        ImGui.Indent();
        for (int i = 0; i < 5; i++) {
            SphereGridLink* link = selected_node->get_link(i);
            if (link != null) {
                int other_idx = link->node_a_idx == lpamng->selected_node_idx ? link->node_b_idx : link->node_a_idx;
                SphereGridNode* other = &lpamng->nodes[other_idx];
                ImGui.Text($"Connected to {other_idx} ({other->x - selected_node->x}, {other->y - selected_node->y}) through link #{i}");
            }
        }
        ImGui.Unindent();

        ImGui.Text($"Node type: {selected_node->node_type}");

        ImGui.Text("Activation status:");
        ImGui.Indent();

        byte activated_by = selected_node->activated_by;

        //TODO: Change this to `i < 8` once Seymour has a sphere grid
        for (int i = 0; i < 7; i++) {
            bool activated = (activated_by & (1 << i)) != 0;

            if (!save_data->ply_saves[i].join) continue;

            byte[] name_buffer = new byte[FhEncoding.compute_decode_buffer_size(save_data->character_names[i])];
            FhEncoding.decode(save_data->character_names[i], name_buffer, null, null, FhEncodingFlags.IMPLICIT_END);

            string activation = activated ? "Active" : "Inactive";
            ImGui.Text($"{Encoding.UTF8.GetString(name_buffer)}: {activation}");

            ImGui.SameLine(150);

            if (ImGui.Button(activated ? $"Deactivate##{i}" : $"Activate##{i}")) {
                if (activated) {
                    selected_node->activated_by.set_bit(i, false);
                    _SndSepPlaySimple(SND_DEACTIVATE_NODE);

                    lpamng->should_update = 1;
                    lpamng->should_update_node = lpamng->selected_node_idx;
                } else {
                    _abmap_get_panel(i, lpamng->selected_node_idx);
                }
            }
        }

        ImGui.Unindent();

        ImGui.Text($"Can target? {selected_node->properties.can_target()}");
        ImGui.Text($"Is highlighted? {selected_node->properties.is_highlighted()}");

        ImGui.Text($"Move cost: {selected_node->move_cost}");
    }

    public void h_move_speed() {
        var lpamng = SphereGrid.lpamng;

        float prev_t = lpamng->moving_progress;

        _sphere_grid_move_speed.orig_fptr();

        if (freeze_move) {
            lpamng->moving_progress = prev_t;
        } else if (skip_moves) {
            lpamng->moving_progress = 1.0f;
        }

        float speed_mult = FhUtil.get_at<int>(0x8e82a4) switch {
            0 => 1.0f,
            1 => 2.0f,
            2 => 4.0f,
            _ => 1.0f,
        };

        lpamng->moving_speed = 0.1f * speed_mult;
    }

    private HashSet<short> _get_neighbour_indices(short node_idx) {
        HashSet<short> set = [];

        SphereGridNode node = SphereGrid.lpamng->nodes[node_idx];

        foreach (uint ptr in node.link_ptrs) {
            SphereGridLink* link = (SphereGridLink*)ptr;

            if (link is null) continue;

            short other_idx = link->node_a_idx != node_idx ? link->node_a_idx : link->node_b_idx;
            set.Add(other_idx);
        }

        return set;
    }

    internal bool can_activate(NodeType node_type) {
        return (node_type < NodeType.LUCK_1 || node_type > NodeType.LUCK_4)
            && (
                node_type.is_attribute_node()
             || node_type.is_skill_node()
             || node_type.is_special_node()
             || node_type.is_white_magic()
             || node_type.is_black_magic()
            );
    }

    private bool try_activate(short node_idx, int ply_id, bool temporary) {
        SphereGridNode* node = &SphereGrid.lpamng->nodes[node_idx];

        if (node->activated_by.get_bit(ply_id) || !can_activate(node->node_type)) {
            return false;
        }

        node->activated_by.set_bit(ply_id, true);
        _SndSepPlaySimple(SND_ACTIVATE_NODE);

        if (temporary) {
            temporarily_activated_nodes.Add(node_idx);
        }

        return true;
    }

    internal void on_move_knot(int chr_id, short node_idx) {
        if (node_idx == -1) return;

        var lpamng = SphereGrid.lpamng;

        bool activated_some = false;
        activated_some |= try_activate(node_idx, chr_id, true);

        //TODO: Use the SphereGridNode's actual `get_neighbour_indices` method
        //      when fahrenheit-crew/fahrenheit#97 is resolved in release
        foreach (short neighbour_idx in _get_neighbour_indices(node_idx)) {
            activated_some |= try_activate(neighbour_idx, chr_id, true);
        }

        if (activated_some) {
            lpamng->should_update = 1;
            lpamng->should_update_node = -1;
        }
    }

    public void h_move_confirm(int p1, int p2, int p3) {
        var lpamng = SphereGrid.lpamng;

        knots_counted = 0;

        _sphere_grid_confirm_move.orig_fptr(p1, p2, p3);

        // If we cancelled it, also deactivate all activated nodes
        if (p3 == 1 && temporarily_activated_nodes.Count > 0) {
            int chr_id = lpamng->current_chr_id;

            foreach (short node_idx in temporarily_activated_nodes) {
                lpamng->nodes[node_idx].activated_by.set_bit(chr_id, false);
            }

            _SndSepPlaySimple(SND_DEACTIVATE_NODE);

            lpamng->should_update = 1;
            lpamng->should_update_node = -1;
        }

        temporarily_activated_nodes.Clear();
    }

    public void h_state_moving() {
        var lpamng = SphereGrid.lpamng;

        float prev_t = lpamng->moving_progress;
        short last_knot = lpamng->move_next_target_node_idx;

        _sphere_grid_state_moving.orig_fptr();

        float current_t = lpamng->moving_progress;

        if (lpamng->move_next_target_node_idx == lpamng->move_last_target_node_idx
                && lpamng->__0x115A8 - (uint)FhUtil.ptr_at<byte>(0) != 0x659990
                && current_t >= 1.0) {
            on_move_knot(lpamng->current_chr_id, last_knot);
            return;
        }

        if (current_t < prev_t) {
            // Don't activate the neighbours of the first node,
            // since that's a bit counterintuitive
            if (knots_counted > 0) {
                on_move_knot(lpamng->current_chr_id, last_knot);
            }

            knots_counted += 1;
        }
    }

    public void h_state_warping() {
        var lpamng = SphereGrid.lpamng;

        byte last_warp_state = *(byte*)((int)lpamng + 0x1164c);

        _sphere_grid_state_warping.orig_fptr();

        // Warp states:
        // 0 == Disappearing
        // 1 == Moving
        // 2 == Reappearing
        byte warp_state = *(byte*)((int)lpamng + 0x1164c);

        if (last_warp_state == 1 && warp_state == 2) {
            short node_idx = lpamng->move_last_target_node_idx;
            byte chr_id = lpamng->current_chr_id;

            bool activated_some = false;
            activated_some |= try_activate(node_idx, chr_id, false);

            //TODO: Use the SphereGridNode's actual `get_neighbour_indices` method
            //      when fahrenheit-crew/fahrenheit#97 is resolved in release
            foreach (short neighbour_idx in _get_neighbour_indices(node_idx)) {
                activated_some |= try_activate(neighbour_idx, chr_id, false);
            }

            if (activated_some) {
                lpamng->should_update = 1;
                lpamng->should_update_node = -1;
            }
        }
    }

    public void h_change_node() {
        var lpamng = SphereGrid.lpamng;

        NodeType new_type = *(NodeType*)((int)lpamng + 0x1164c);

        short node_idx = *(short*)((int)lpamng + 0x1164e);
        byte ply_id = lpamng->current_chr_id;

        _sphere_grid_change_node.orig_fptr();

        byte timing = *(byte*)((int)lpamng + 0x11650);

        if (timing == 20 && can_activate(new_type)) {
            lpamng->nodes[node_idx].activated_by.set_bit(ply_id, true);
            lpamng->should_update = 1;
            lpamng->should_update_node = node_idx;
        }
    }
}
