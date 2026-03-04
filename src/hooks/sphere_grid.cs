using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

using Hexa.NET.ImGui;

using Fahrenheit.FFX;

using static Fahrenheit.FFX.Globals;

namespace Fahrenheit.Modules.ArchipelagoFFX;

[StructLayout(LayoutKind.Sequential)]
public struct TkMenuStaticData {
    public int ctrl; // code ptr
    public int draw; // code ptr
    public int init; // code ptr
    public int vft4; // code ptr
    public int exit; // code ptr
    public int vft6; // code ptr
    private short __0x18;
    private int   __0x1c;
}

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate void eiAbmCalc();

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate void TkMenuInit();

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate void abmap_get_panel(int chr_id, int node_idx);

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate void CdeclVV();

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate void CdeclVI(int p1);

public unsafe partial class ArchipelagoFFXModule {
    private FhMethodHandle<eiAbmCalc> _eiAbmCalc;
    private FhMethodHandle<CdeclVV> _sphere_grid_move_speed;
    private FhMethodHandle<CdeclVV> _sphere_grid_confirm_move;

    private abmap_get_panel _abmap_get_panel = FhUtil.get_fptr<abmap_get_panel>(0x6458a0);


    internal void init_abmap_hooks() {
        const string game = "FFX.exe";

        _logger.Info("Initializing hooks!");

        _eiAbmCalc = new(this, game, 0x653570, h_eiAbmCalc);
        _sphere_grid_move_speed = new(this, game, 0x659990, h_move_speed);
        _sphere_grid_confirm_move = new(this, game, 0x644ef0, h_move_confirm);

        // _abmap_menu_init.hook();
        _eiAbmCalc.hook();
        _sphere_grid_move_speed.hook();
        _sphere_grid_confirm_move.hook();
    }

    public void h_eiAbmCalc() {
        _eiAbmCalc.orig_fptr();
    }

    private static float last_speed;
    private static float last_t;
    private static int knots_counted = 0;
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
            FhUtil.get_fptr<CdeclVI>(0x486de0)(sound_id);
        }

        if (ImGui.CollapsingHeader("LpAbilityMapEngine")) {
            var lpamng = SphereGrid.lpamng;
            ImGui.Text($"{lpamng->node_count} nodes are connected by {lpamng->link_count} links over {lpamng->cluster_count} clusters.");

            // ImGui.InputFloat("Move Speed", ref move_speed, 0.1f);

            float current_move_speed = *(float*)((int)lpamng + 0x11624);
            float current_t = *(float*)((int)lpamng + 0x11620);
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
                for (int i = 0; i < 7; i++) {
                    bool activated = (activated_by & (1 << i)) != 0;

                    if (save_data->ply_saves[i].join) {
                        byte[] name_buffer = new byte[FhEncoding.compute_decode_buffer_size(save_data->character_names[i])];
                        FhEncoding.decode(save_data->character_names[i], name_buffer, null, null, FhEncodingFlags.IMPLICIT_END);

                        string activation = activated ? "Active" : "Inactive";
                        ImGui.Text($"{Encoding.UTF8.GetString(name_buffer)}: {activation}");

                        ImGui.SameLine(100);

                        if (ImGui.Button("Toggle")) {
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

                ImGui.Text($"Can move to? {selected_node->properties.get(SphereGridNodeProperties.CAN_MOVE_TO)}");
                ImGui.Text($"Has outline? {selected_node->properties.get(SphereGridNodeProperties.HAS_MOVE_OUTLINE)}");

                ImGui.Text($"Thingy: {*(byte*)((int)selected_node + 0x24)}");
            }

            ImGui.Unindent();

            if (ImGui.Button("Update nodes")) {
                lpamng->should_update = 1;
                lpamng->should_update_node = -1;
            }
        }

        ImGui.End();
    }

    private static float last_last_t;
    private static ushort last_next_knot_idx = 0xFFFF;
    public void h_move_speed() {
        last_last_t = *(float*)((int)SphereGrid.lpamng + 0x11620);

        _sphere_grid_move_speed.orig_fptr();

        if (last_next_knot_idx == 0xFFFF) {
            last_next_knot_idx = *(ushort*)((int)SphereGrid.lpamng + 0x11632);
        }

        if (freeze_move) {
            *(float*)((int)SphereGrid.lpamng + 0x11620) = last_last_t;
        } else if (skip_moves) {
            *(float*)((int)SphereGrid.lpamng + 0x11620) = 1.0f;
        }

        last_speed = *(float*)((int)SphereGrid.lpamng + 0x11624);
        last_t = *(float*)((int)SphereGrid.lpamng + 0x11620);


        if (last_t < last_last_t) {
            knots_counted += 1;

            on_move_knot(*(int*)((int)SphereGrid.lpamng + 0x115bc), (short)last_next_knot_idx);
            last_next_knot_idx = *(ushort*)((int)SphereGrid.lpamng + 0x11632);
        }

        float speed_mult = FhUtil.get_at<int>(0x8e82a4) switch {
            0 => 1.0f,
            1 => 2.0f,
            2 => 4.0f,
            _ => 1.0f,
        };

        *(float*)((int)SphereGrid.lpamng + 0x11624) = 0.1f * speed_mult;
    }

    internal bool can_activate(SphereGridNode* node) {
        return (node->node_type < NodeType.LUCK_1 || node->node_type > NodeType.LUCK_4)
            && (
                node->node_type.is_attribute_node()
             || node->node_type.is_skill_node()
             || node->node_type.is_special_node()
             || node->node_type.is_white_magic()
             || node->node_type.is_black_magic()
            );
    }


    internal static HashSet<short> activated_nodes = [ ];
    internal void on_move_knot(int chr_id, short node_idx) {
        if (node_idx < SphereGrid.lpamng->node_count) {
            SphereGridNode* node = &SphereGrid.lpamng->nodes[node_idx];

            bool activated_some = false;

            if (!node->activated_by.get_bit(chr_id) && can_activate(node)) {
                //_abmap_get_panel(chr_id, node_idx);
                node->activated_by.set_bit(chr_id, true);
                activated_some = true;
                activated_nodes.Add(node_idx);
            }

            for (int i = 0; i < 5; i++) {
                SphereGridLink* link = node->get_link(i);
                if (link == null) continue;

                short other_idx = link->node_a_idx == node_idx ? link->node_b_idx : link->node_a_idx;
                SphereGridNode* other = &SphereGrid.lpamng->nodes[other_idx];

                if (!other->activated_by.get_bit(chr_id) && can_activate(other)) {
                    //_abmap_get_panel(chr_id, other_idx);
                    other->activated_by.set_bit(chr_id, true);
                    activated_some = true;
                    activated_nodes.Add(other_idx);
                }
            }

            if (activated_some) {
                SphereGrid.lpamng->should_update = 1;
                SphereGrid.lpamng->should_update_node = -1;
            }
        }
    }

    public void h_move_confirm() {
        var lpamng = SphereGrid.lpamng;

        if (*(byte*)((int)lpamng + 0x115cd) == 0 && *(int*)((int)lpamng + 0x115b0) == 0) {
            short pad = *(short*)((int)lpamng + 0x1166e);

            if ((pad & 0x60) != 0) {
                knots_counted = 0;
                last_next_knot_idx = 0xFFFF;

                _logger.Info("Got pad input!");
                _logger.Info($"  Confirm: {(pad & 0x20) != 0}");
                _logger.Info($"   Cancel: {(pad & 0x40) != 0}");
            }

            // If we cancelled it, also deactivate all activated nodes
            if ((pad & 0x40) != 0 && activated_nodes.Count > 0) {
                int chr_id = *(int*)((int)lpamng + 0x115bc);

                foreach (short node_idx in activated_nodes) {
                    lpamng->nodes[node_idx].activated_by.set_bit(chr_id, false);
                }

                if (activated_nodes.Count > 0) {
                    _SndSepPlaySimple(0x8000006e);
                }

                lpamng->should_update = 1;
                lpamng->should_update_node = -1;
                _logger.Info($"Deactivated {activated_nodes.Count} nodes!");
                activated_nodes.Clear();
            }
        }

        _sphere_grid_confirm_move.orig_fptr();
    }
}
