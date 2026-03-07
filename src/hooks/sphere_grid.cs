using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

using Hexa.NET.ImGui;

using Fahrenheit.FFX;

using static Fahrenheit.FFX.Globals;

namespace Fahrenheit.Modules.ArchipelagoFFX;

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate void eiAbmCalc();

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate void abmap_get_panel(int ply_id, int node_idx);

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate void abmap_ctrl();

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate void abmap_confirm_move(int p1, int p2, int p3);

public unsafe partial class ArchipelagoFFXModule {
    private FhMethodHandle<eiAbmCalc> _eiAbmCalc;
    private FhMethodHandle<abmap_ctrl> _sphere_grid_move_speed;
    private FhMethodHandle<abmap_confirm_move> _sphere_grid_confirm_move;
    private FhMethodHandle<abmap_ctrl> _sphere_grid_state_moving;
    private FhMethodHandle<abmap_ctrl> _sphere_grid_state_warping;
    private FhMethodHandle<abmap_ctrl> _sphere_grid_change_node;

    private readonly abmap_get_panel _abmap_get_panel = FhUtil.get_fptr<abmap_get_panel>(0x6458a0);

    // Normal = 645010
    // ChoosingMoveTarget = 644EF0
    // MovingToTarget = 659990
    // AwaitingMoveConfirm = 648230
    // ChoosingActivationTarget = 6452D0
    // Activating = 648280
    // HomingOnNode = 659E80

    internal void init_abmap_hooks() {
        const string GAME = "FFX.exe";

        _eiAbmCalc = new(this, GAME, 0x653570, h_eiAbmCalc);
        _sphere_grid_move_speed = new(this, GAME, 0x659990, h_move_speed);
        _sphere_grid_confirm_move = new(this, GAME, 0x656160, h_move_confirm);
        _sphere_grid_state_moving = new(this, GAME, 0x659990, h_state_moving);
        _sphere_grid_state_warping = new(this, GAME, 0x647f00, h_state_warping);
        _sphere_grid_change_node = new(this, GAME, 0x647d50, h_change_node);

        // _abmap_menu_init.hook();
        _eiAbmCalc.hook();
        _sphere_grid_move_speed.hook();
        _sphere_grid_confirm_move.hook();
        _sphere_grid_state_moving.hook();
        _sphere_grid_state_warping.hook();
        _sphere_grid_change_node.hook();
    }

    public void h_eiAbmCalc() {
        _eiAbmCalc.orig_fptr();
    }

    private string get_state_name_for_address(uint address) {
        return address switch{
            0x645010 => "Normal",
            0x644EF0 => "ChoosingMoveTarget",
            0x659990 => "MovingToTarget",
            0x648230 => "AwaitingMoveConfirmation",
            0x6452D0 => "ChoosingActivationTarget",
            0x648280 => "Activating",
            0x659E80 => "HomingOnNode",
            0x647F00 => "Warping",
            0x647D50 => "ClearingNode",
            _ => $"?????? (0x{address:X8})",
        };
    }

    private static int knots_counted;
    private static bool freeze_move;
    private static bool skip_moves;
    private static int sound_id;
    private static bool sphere_grid_debug_open;
    public void render_sphere_grid_debug() {
        if (ImGui.IsKeyPressed(ImGuiKey.GraveAccent))
            sphere_grid_debug_open ^= true;

        if (!sphere_grid_debug_open) return;

        if (!ImGui.Begin("Sphere Grid Debug")) {
            ImGui.End();
            return;
        }

        ImGui.InputInt("Sound ID", ref sound_id, 1);
        ImGui.SameLine();
        if (ImGui.Button("Play Sound")) {
            _SndSepPlaySimple(unchecked((uint)sound_id));
        }

        if (ImGui.CollapsingHeader("LpAbilityMapEngine")) {
            var lpamng = SphereGrid.lpamng;
            ImGui.Text($"{lpamng->node_count} nodes are connected by {lpamng->link_count} links over {lpamng->cluster_count} clusters.");
            ImGui.Text($"State: {get_state_name_for_address((uint)lpamng->__0x115A8 - (uint)FhUtil.ptr_at<byte>(0))}");

            float current_move_speed = lpamng->moving_speed;
            float current_t = lpamng->moving_progress;
            float min_t = 0.0f;
            float max_t = 1.0f;

            ImGui.Text($"Moving at {current_move_speed} cbrt(tbsp) per second");
            ImGui.BeginDisabled();
            ImGui.SliderScalar("Progress", ImGuiDataType.Float, &current_t, &min_t, &max_t);
            ImGui.Text($"Knots so far: {knots_counted}");
            ImGui.EndDisabled();

            if (ImGui.Button(skip_moves ? "Don't Skip Moves" : "Skip Moves")) {
                skip_moves ^= true;
            }

            if (ImGui.Button(freeze_move ? "Unfreeze Move" : "Freeze Move")) {
                freeze_move ^= true;
            }

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

        ImGui.End();
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

        byte activated_by = selected_node->activated_by;
        ImGui.Text("Activation status:");

        //TODO: Change this to `i < 8` once Seymour has a sphere grid
        for (int i = 0; i < 7; i++) {
            bool activated = (activated_by & (1 << i)) != 0;

            if (save_data->ply_saves[i].join) {
                byte[] name_buffer = new byte[FhEncoding.compute_decode_buffer_size(save_data->character_names[i])];
                FhEncoding.decode(save_data->character_names[i], name_buffer, null, null, FhEncodingFlags.IMPLICIT_END);

                string activation = activated ? "Active" : "Inactive";
                ImGui.Text($"{Encoding.UTF8.GetString(name_buffer)}: {activation}");

                ImGui.SameLine(100);

                if (ImGui.Button($"Toggle#{i}")) {
                    if (activated) {
                        selected_node->activated_by.set_bit(i, !activated);
                        lpamng->should_update = 1;
                        lpamng->should_update_node = lpamng->selected_node_idx;
                        _SndSepPlaySimple(0x8000006e);
                    } else {
                        _abmap_get_panel(i, lpamng->selected_node_idx);
                    }
                }
            }
        }

        ImGui.Text($"Can move to? {selected_node->properties.can_target()}");
        ImGui.Text($"Has outline? {selected_node->properties.is_highlighted()}");

        ImGui.Text($"Thingy: {selected_node->move_cost}");
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

    private void play_activation_sound() {
        _SndSepPlaySimple(0x80000050);
    }

    internal static readonly HashSet<short> activated_nodes = [];
    internal void on_move_knot(int chr_id, short node_idx) {
        var lpamng = SphereGrid.lpamng;

        if (node_idx < lpamng->node_count) {
            SphereGridNode* node = &lpamng->nodes[node_idx];

            bool activated_some = false;

            if (!node->activated_by.get_bit(chr_id) && can_activate(node->node_type)) {
                //_abmap_get_panel(chr_id, node_idx);
                node->activated_by.set_bit(chr_id, true);
                activated_some = true;
                play_activation_sound();
                activated_nodes.Add(node_idx);
            }

            for (int i = 0; i < 5; i++) {
                SphereGridLink* link = node->get_link(i);
                if (link == null) continue;

                short other_idx = link->node_a_idx == node_idx ? link->node_b_idx : link->node_a_idx;
                SphereGridNode* other = &lpamng->nodes[other_idx];

                if (other->activated_by.get_bit(chr_id)) continue;
                if (!can_activate(other->node_type)) continue;

                //_abmap_get_panel(chr_id, other_idx);
                other->activated_by.set_bit(chr_id, true);
                activated_some = true;
                play_activation_sound();
                activated_nodes.Add(other_idx);
            }

            if (activated_some) {
                lpamng->should_update = 1;
                lpamng->should_update_node = -1;
            }
        }
    }

    public void h_move_confirm(int p1, int p2, int p3) {
        var lpamng = SphereGrid.lpamng;

        knots_counted = 0;

        _sphere_grid_confirm_move.orig_fptr(p1, p2, p3);

        if (p3 == 0) {
            activated_nodes.Clear();
        }

        // If we cancelled it, also deactivate all activated nodes
        if (p3 == 1 && activated_nodes.Count > 0) {
            int chr_id = lpamng->current_chr_id;

            foreach (short node_idx in activated_nodes) {
                lpamng->nodes[node_idx].activated_by.set_bit(chr_id, false);
            }

            if (activated_nodes.Count > 0) {
                _SndSepPlaySimple(0x8000006e);
            }

            lpamng->should_update = 1;
            lpamng->should_update_node = -1;
            activated_nodes.Clear();
        }
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

        byte warp_state = *(byte*)((int)lpamng + 0x1164c);

        if (last_warp_state == 1 && warp_state == 2) {
            short node_idx = lpamng->move_last_target_node_idx;
            byte chr_id = lpamng->current_chr_id;
            SphereGridNode* node = &lpamng->nodes[node_idx];

            bool activated_some = false;

            if (!node->activated_by.get_bit(chr_id) && can_activate(node->node_type)) {
                //_abmap_get_panel(chr_id, node_idx);
                node->activated_by.set_bit(chr_id, true);
                activated_some = true;
            }

            for (int i = 0; i < 5; i++) {
                SphereGridLink* link = node->get_link(i);
                if (link == null) continue;

                short other_idx = link->node_a_idx == node_idx ? link->node_b_idx : link->node_a_idx;
                SphereGridNode* other = &lpamng->nodes[other_idx];

                if (!other->activated_by.get_bit(chr_id) && can_activate(other->node_type)) {
                    //_abmap_get_panel(chr_id, other_idx);
                    other->activated_by.set_bit(chr_id, true);
                    activated_some = true;
                }
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
