using Archipelago.MultiClient.Net.Enums;
using Fahrenheit.Core;
using Fahrenheit.Core.FFX;
using Fahrenheit.Core.FFX.Battle;
using Fahrenheit.Modules.ArchipelagoFFX.Client;
using Hexa.NET.ImGui;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using TerraFX.Interop.DirectX;
using static Fahrenheit.Core.FFX.Globals;
using static Fahrenheit.Modules.ArchipelagoFFX.ArchipelagoData;
using static Fahrenheit.Modules.ArchipelagoFFX.ArchipelagoFFXModule;
using static Fahrenheit.Modules.ArchipelagoFFX.delegates;
using Color = Archipelago.MultiClient.Net.Models.Color;

namespace Fahrenheit.Modules.ArchipelagoFFX.GUI;
public unsafe static class ArchipelagoGUI {
    public delegate void OnRenderDelegate();

    public const ImGuiKey archipelago_gui_key = ImGuiKey.F8;
    public const ImGuiKey experimental_gui_key = ImGuiKey.F9;

    public static bool enabled = false;
    public static bool experiments_enabled = false;
    private static bool show = true;

    private static string client_input_address = "archipelago.gg:";
    private static string client_input_name = "";
    private static string client_input_password = "";

    private static string client_input_command = "";

    public static readonly System.Threading.Lock client_log_lock = new();
    private static List<List<(string text, Color color)>> client_log = [];
    public static bool client_log_updated = false;
    private static float previous_scroll = 1;
    private static float previous_scroll_max = 1;


    private static readonly Vector2 PANE_BUTTON_SIZE = new Vector2(16f);

    private static int grav_mode = 1;
    private static int field_mode = 0;
    private static int motion_type = 0;

    private static int character_model = 0;

    private static int[] MsBtlGetPosParams = [0, 0, 0];
    private static Vector4 MsBtlGetPosResult = new(0, 0, 0, 0);
    private static uint LaunchBattleInput = 0;

    private static int auto_ability_id = 0;
    private static AutoAbility* ability = null;
    private static int chr_id = 0;
    private static Equipment* weapon = null;
    private static Equipment* armor = null;

    private static int clickedNodeIndex = -1;


    public static int selected_seed;

    public static int font_size = -1;

    private static bool show_popup;
    public static string popup_content { 
        get;
        set {
            field = value;
            show_popup = value != "";
        }
    }

    public static void render() {
        //ImGui.ShowDebugLogWindow();
        //ImGui.ShowStyleEditor();


        // Setup windows style
        //float pane_width = ImGui.GetIO().DisplaySize.X * 0.4f;
        //ImGui.PushStyleColor(ImGuiCol.WindowBg, new Vector4 { X = 0.5f, Y = 0.5f, Z = 0.5f });
        //ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 0f);
        //ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 0f);
        if (font_size == -1) font_size = (int)ImGui.GetFontSize();
        ImGui.PushFont(null, font_size);

        if (show_popup) { 
            ImGui.OpenPopup("Archipelago.GUI.Popup");
            show_popup = false;
        }
        ImGui.SetNextWindowPos(ImGui.GetCenter(ImGui.GetMainViewport()), ImGuiCond.Appearing, new(0.5f, 0.5f));
        if (ImGui.BeginPopup("Archipelago.GUI.Popup")) {
            ImGui.Text(popup_content);
            ImGui.EndPopup();
        }

        render_client();
#if DEBUG
        render_experiments();
        //render_clusters();
#endif
        ImGui.PopFont();
        //render_pane(pane_width);

        // Reset style
        //ImGui.PopStyleVar(2);
        //ImGui.PopStyleColor();
    }

    private static string cluster_file_name = "/FFX_Data/GameData/PS3Data/map/hiku/hiku22/2d/tex/D3D11/0_11_132_16_12.dds.phyre";
    private static PTexture2DBase* loaded_texture2d = null;
    private static ImTextureRef? loaded_image;

    private static void render_clusters() {
        if (ImGui.Begin("Archipelago###Archipelago.Clusters.GUI")) {
            ImGui.InputText("fileName", ref cluster_file_name, 256);
            if (ImGui.Button("Load cluster by name")) {
                PCluster* cluster = ArchipelagoFFXModule.loadCluster(cluster_file_name);
                if (cluster != null) {
                    loaded_texture2d = getTextureFromCluster(cluster);
                    if (loaded_image != null) {
                        ((ID3D11ShaderResourceView*)loaded_image.Value.TexID.Handle)->Release();
                        loaded_image = null;
                    }
                }
            }
            if (ImGui.Button("Get cluster by name")) {
                PCluster* cluster = ArchipelagoFFXModule.getCluster(cluster_file_name);
                if (cluster != null) {
                    loaded_texture2d = getTextureFromCluster(cluster);
                    if (loaded_image != null) {
                        ((ID3D11ShaderResourceView*)loaded_image.Value.TexID.Handle)->Release();
                        loaded_image = null;
                    }
                }
            }
            if (ImGui.Button("Unload cluster by name")) {
                PCluster* cluster = getCluster(cluster_file_name);
                if (cluster != null) {
                    loaded_clusters.Remove((nint)cluster);
                    loaded_texture2d = null;
                    if (loaded_image != null) {
                        ((ID3D11ShaderResourceView*)loaded_image.Value.TexID.Handle)->Release();
                        loaded_image = null;
                    }
                    releaseCluster(cluster);
                }
            }
            if (ImGui.Button("Unload all clusters")) {
                loaded_texture2d = null;
                if (loaded_image != null) {
                    ((ID3D11ShaderResourceView*)loaded_image.Value.TexID.Handle)->Release();
                    loaded_image = null;
                }
                foreach (nint cluster in loaded_clusters) {
                    releaseCluster((PCluster*)cluster);
                }
                loaded_clusters.Clear();
            }

            //if (loaded_texture2d != null && loaded_image == null) {
            //    FhApi.Resources.create_srv(loaded_texture2d->buffer, null, out loaded_image);
            //}
            if (loaded_image.HasValue) {
                ImGui.Image(loaded_image.Value, new(loaded_texture2d->width, loaded_texture2d->height), new(0, 1), new(1, 0));
            }
        }
        ImGui.End();
    }

    public static FileInfo?  shiori_file;
    private static FhTexture? shiori_image;
    private static FhTexture? bevelle_image;

    private static int voiceline_id;
    public  static byte voice_lang = 0xFF;
    public  static byte text_lang  = 0xFF;

    private static void render_experiments() {
        experiments_enabled ^= ImGui.IsKeyPressed(experimental_gui_key);
        if (!experiments_enabled) return;

        ImGuiStylePtr style = ImGui.GetStyle();


        if (ImGui.Begin("Archipelago###Archipelago.Experiments.GUI")) {

            float frameHeight = ImGui.GetFrameHeight();
            Vector2 windowPos = ImGui.GetWindowPos();
            float windowBorderSize = ImGui.GetStyle().WindowBorderSize;
            if (shiori_image != null) ImGui.GetForegroundDrawList().AddImage(shiori_image.TextureRef, windowPos + new Vector2(windowBorderSize), new(windowPos.X + frameHeight - windowBorderSize, windowPos.Y + frameHeight - windowBorderSize));

            //Span<byte> tidus_name = new Span<byte>(Globals.save_data->character_names[0].raw, 20);
            //byte[] tidus_decoded = new byte[FhEncoding.compute_decode_buffer_size(tidus_name)];
            //
            //int decoded_length = FhEncoding.decode(tidus_name, tidus_decoded);
            //string tidus_string = Encoding.UTF8.GetString(tidus_decoded, 0, decoded_length);
            //fixed (byte* name_string = tidus_decoded) {
            //    if (ImGui.InputText("Tidus Name", ref tidus_string, 19, ImGuiInputTextFlags.EnterReturnsTrue)) {
            //        string final_string = tidus_string + "{END}";
            //        FhEncoding.encode(Encoding.UTF8.GetBytes(final_string), tidus_name);
            //    }
            //}
            if (ImGui.Checkbox($"Original soundtrack?", &save_data->soundtrack_type)) {
                var soundtrack_callback = FhUtil.get_fptr<FUN_008cc120>(__addr_FUN_008cc120);
                soundtrack_callback(save_data->soundtrack_type ? 1 : 0);
            }

            ImGui.InputScalarN("frontline? (0x1FC5)", ImGuiDataType.U8, Globals.Battle.btl->__0x1FC5,  7);
            ImGui.InputScalarN("frontline? (0x1FCC)", ImGuiDataType.U8, Globals.Battle.btl->__0x1FCC,  7);
            ImGui.InputScalarN("backline? (0x1FD3)" , ImGuiDataType.U8, Globals.Battle.btl->__0x1FD3, 17);



            Vector4* Vector4f_ARRAY_00c86010 = FhUtil.ptr_at<Vector4>(0x886010);

            ImGui.InputFloat4("Tidus ambient(?) color", &Vector4f_ARRAY_00c86010->X);

            var goalRequirements = Enum.GetNames<GoalRequirement>();
            int currentRequirement = (int)seed.GoalRequirement;
            if (ImGui.Combo("Goal Requirement", ref currentRequirement, goalRequirements, goalRequirements.Length)) {
                seed.GoalRequirement = (GoalRequirement)currentRequirement;
            }

            ImGui.InputInt("Required Party Members", ref seed.RequiredPartyMembers);

            //if (ImGui.BeginCombo("Text", text_lang == 0xFF ? "Default" : ((FhLangId)text_lang).ToString())) {
            //    if (ImGui.Selectable("Default", text_lang == 0xFF)) {
            //        text_lang = 0xFF;
            //    }
            //    foreach (FhLangId lang in Enum.GetValues<FhLangId>()) {
            //        if (ImGui.Selectable($"{lang}", text_lang == (byte)lang)) {
            //            text_lang = (byte)lang;
            //        }
            //    }
            //    ImGui.EndCombo();
            //}

            //if (shiori_image == null || bevelle_image == null) {
            //    var resources = ArchipelagoFFXModule.mod_context.Paths.ResourcesDir.GetFiles();
            //    var shiori_file = Array.Find(resources, file => file.Name == "shiori.png");
            //    if (shiori_file != null) {
            //        FhApi.ResourceLoader.load_png_from_disk(shiori_file.FullName, out shiori_image);
            //    }
            //}
            //if (shiori_image != null) {
            //    ImGui.Image(shiori_image.TextureRef, new(shiori_image.Metadata.width, shiori_image.Metadata.height));
            //}

            ImGui.Text($"Tidus overdrive uses: {Globals.save_data->tidus_limit_uses}");

            //for (int i = 0; i < 18; i++) {
            //    string name = Globals.save_data->character_names[i].name;
            //    if (ImGui.InputText($"Name {i}", ref name, 20)) {
            //        Globals.save_data->character_names[i].name = name;
            //    }
            //}

            ImGui.InputInt("Voiceline id", ref voiceline_id);
            if (ImGui.Button("Play voiceline")) {
                queued_voice_lines.Enqueue(voiceline_id);
            }

            int inMenu = FhUtil.get_at<int>(0x01efb4d4);
            ImGui.Text($"Is in menu?: {inMenu}");


            BtlArea* pos_def_ptr = Globals.Battle.btl->ptr_pos_def;

            if (pos_def_ptr != null && Globals.Battle.btl->battle_state != 0) {
                BtlAreasHelper areas = new(pos_def_ptr);
                //BtlAreas areas = *pos_def_ptr;

                foreach (BtlAreaHelper area in areas.areas) {

                    ImGui.Text("PARTY_POS");
                    if (ImGui.BeginTable("PARTY_POS", 4, ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg)) {

                        foreach (Vector4 pos in area.party_pos) {
                            ImGui.TableNextRow();
                            ImGui.TableNextColumn();
                            ImGui.Text($"{pos.X}");
                            ImGui.TableNextColumn();
                            ImGui.Text($"{pos.Y}");
                            ImGui.TableNextColumn();
                            ImGui.Text($"{pos.Z}");
                            ImGui.TableNextColumn();
                            ImGui.Text($"{pos.W}");
                        }

                        ImGui.EndTable();
                    }

                    ImGui.Text("PARTY_RUN_POS");
                    if (ImGui.BeginTable("PARTY_RUN_POS", 4, ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg)) {

                        foreach (Vector4 pos in area.party_run_pos) {
                            ImGui.TableNextRow();
                            ImGui.TableNextColumn();
                            ImGui.Text($"{pos.X}");
                            ImGui.TableNextColumn();
                            ImGui.Text($"{pos.Y}");
                            ImGui.TableNextColumn();
                            ImGui.Text($"{pos.Z}");
                            ImGui.TableNextColumn();
                            ImGui.Text($"{pos.W}");
                        }

                        ImGui.EndTable();
                    }

                    ImGui.Text("AEON_POS");
                    if (ImGui.BeginTable("AEON_POS", 4, ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg)) {

                        foreach (Vector4 pos in area.aeon_pos) {
                            ImGui.TableNextRow();
                            ImGui.TableNextColumn();
                            ImGui.Text($"{pos.X}");
                            ImGui.TableNextColumn();
                            ImGui.Text($"{pos.Y}");
                            ImGui.TableNextColumn();
                            ImGui.Text($"{pos.Z}");
                            ImGui.TableNextColumn();
                            ImGui.Text($"{pos.W}");
                        }

                        ImGui.EndTable();
                    }

                    ImGui.Text("AEON_RUN_POS");
                    if (ImGui.BeginTable("AEON_RUN_POS", 4, ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg)) {

                        foreach (Vector4 pos in area.aeon_run_pos) {
                            ImGui.TableNextRow();
                            ImGui.TableNextColumn();
                            ImGui.Text($"{pos.X}");
                            ImGui.TableNextColumn();
                            ImGui.Text($"{pos.Y}");
                            ImGui.TableNextColumn();
                            ImGui.Text($"{pos.Z}");
                            ImGui.TableNextColumn();
                            ImGui.Text($"{pos.W}");
                        }

                        ImGui.EndTable();
                    }

                    ImGui.Text("ENEMY_POS");
                    if (ImGui.BeginTable("ENEMY_POS", 4, ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg)) {

                        foreach (Vector4 pos in area.enemy_pos) {
                            ImGui.TableNextRow();
                            ImGui.TableNextColumn();
                            ImGui.Text($"{pos.X}");
                            ImGui.TableNextColumn();
                            ImGui.Text($"{pos.Y}");
                            ImGui.TableNextColumn();
                            ImGui.Text($"{pos.Z}");
                            ImGui.TableNextColumn();
                            ImGui.Text($"{pos.W}");
                        }

                        ImGui.EndTable();
                    }

                    ImGui.Text("ENEMY_RUN_POS");
                    if (ImGui.BeginTable("ENEMY_RUN_POS", 4, ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg)) {

                        foreach (Vector4 pos in area.enemy_run_pos) {
                            ImGui.TableNextRow();
                            ImGui.TableNextColumn();
                            ImGui.Text($"{pos.X}");
                            ImGui.TableNextColumn();
                            ImGui.Text($"{pos.Y}");
                            ImGui.TableNextColumn();
                            ImGui.Text($"{pos.Z}");
                            ImGui.TableNextColumn();
                            ImGui.Text($"{pos.W}");
                        }

                        ImGui.EndTable();
                    }
                    
                }
            }
            //var localizationManager = _LocalizationManager_GetInstance();
            //
            //int text_language = localizationManager->text;
            //
            //if (ImGui.InputInt("Text language", &text_language)) {
            //    localizationManager->text = text_language;
            //}
            //
            //
            //int voice_language = localizationManager->voice;
            //
            //if (ImGui.InputInt("Voice language", &voice_language)) {
            //    localizationManager->voice = voice_language;
            //
            //    nint _FmodManager = FhUtil.get_at<nint>(0x008E9000);
            //    //_FfxFmod_soundInit(*(nint*)(_FmodManager+8));
            //
            //    nint ffxFmod = *(nint*)(_FmodManager + 8);
            //
            //    nint fmodVoice = *(nint*)(ffxFmod + 0x10);
            //
            //    *(byte*)(fmodVoice + 4) = (byte)voice_language;
            //
            //    _FmodVoice_initList(fmodVoice);
            //
            //    int dataChange_result = h_FmodVoice_dataChange(fmodVoice, Globals.save_data->current_room_id, *(nint*)(ffxFmod + 4));
            //    if (dataChange_result != 0) {
            //        *(nint*)(*(int*)((int)ffxFmod + 0xc) + 0x28) = **(nint**)((int)ffxFmod + 0x10);
            //    }
            //}


            //string tidusName = Globals.save_data->character_names[0].name;
            //if (ImGui.InputText("Name Test", ref tidusName, 20)) {
            //    Globals.save_data->character_names[0].name = tidusName;
            //}

        }
        ImGui.End();

        return;

        if (Globals.SphereGrid.lpamng == null) return;

        if (!ImGui.Begin("Archipelago###Archipelago.Experiments.GUI")) {
            ImGui.End();
            return;
        }

        if (ImGui.Button("Activate all nodes for all characters")) {
            for (int i = 0; i < Globals.SphereGrid.lpamng->node_count; i++) {
                Globals.SphereGrid.lpamng->nodes[i].activated_by = 0x7f;
            }
        }

        //if (ImGui.InputScalar("Selected node:", ImGuiDataType.U32, (nint)(&Globals.SphereGrid.lpamng->selected_node_idx))) {
        //    Globals.SphereGrid.lpamng->cam_desired_pos.X = Globals.SphereGrid.lpamng->nodes[Globals.SphereGrid.lpamng->selected_node_idx].x;
        //    Globals.SphereGrid.lpamng->cam_desired_pos.Y = Globals.SphereGrid.lpamng->nodes[Globals.SphereGrid.lpamng->selected_node_idx].y;
        //}
        var camDesiredPos = Globals.SphereGrid.lpamng->cam_desired_pos;
        //ImGui.Text($"desired pos: {camDesiredPos.X}, {camDesiredPos.Y}, {camDesiredPos.Z}, {camDesiredPos.W}");


        int selectedNodeIndex = Globals.SphereGrid.lpamng->selected_node_idx;
        SphereGridNode selectedNode = Globals.SphereGrid.lpamng->nodes[selectedNodeIndex];

        //ImGui.Text($"Selected node: {selectedNodeIndex}, pos: ({selectedNode.x}, {selectedNode.y}), type: {(NodeType)selectedNode.node_type}");

        Matrix4x4* world_matrix = (Matrix4x4*)(*FhUtil.ptr_at<nint>(0x8cb9d8) + 0xd34);
        //if (ImGui.CollapsingHeader("World Matrix")) {
        //    ImGui.InputScalarN($"x", ImGuiDataType.Float, (nint)(&world_matrix->M11), 4);
        //    ImGui.InputScalarN($"y", ImGuiDataType.Float, (nint)(&world_matrix->M21), 4);
        //    ImGui.InputScalarN($"z", ImGuiDataType.Float, (nint)(&world_matrix->M31), 4);
        //    ImGui.InputScalarN($"w", ImGuiDataType.Float, (nint)(&world_matrix->M41), 4);
        //}
        Vector4* temp = (Vector4*)(&world_matrix->M11);

        var mousePos = ImGui.GetMousePos();
        //ImGui.Text($"Mouse pos: {mousePos.X}, {mousePos.Y}");
        var centeredPos = mousePos - (ImGui.GetWindowViewport().Size * 0.5f);
        //ImGui.Text($"Centered mouse pos: {centeredPos.X}, {centeredPos.Y}");

        // Doesn't work
        //float tiltRatio = Globals.SphereGrid.lpamng->tilt_level switch {
        //    SphereGridTilt.FarTilt => 1.40625f,
        //    SphereGridTilt.SlightTilt => 1.125f,
        //    _ => 1,
        //};
        
        float main_x = 2560;
        float main_y = 1440;
        float x_ratio = main_x / ImGui.GetWindowViewport().Size.X;
        float y_ratio = x_ratio * (3.0f/4.0f);
        var gridPos = new Vector2(centeredPos.X * x_ratio, centeredPos.Y * y_ratio);
        //ImGui.Text($"Grid mouse pos: {gridPos.X}, {gridPos.Y}");
        float zoom_mult = Globals.SphereGrid.lpamng->zoom_level.get_zoom();
        var absPos = new Vector2(-gridPos.X / zoom_mult + world_matrix->M31, -gridPos.Y / zoom_mult + world_matrix->M32);
        //ImGui.Text($"Absolute mouse pos: {absPos.X}, {absPos.Y}");

        // Adjusts the center offset
        //float tiltAdjustment = Globals.SphereGrid.lpamng->tilt_level switch {
        //    SphereGridTilt.FarTilt => 855.801f,
        //    SphereGridTilt.SlightTilt => 455.475f,
        //    _ => 0,
        //};
        var truePos = new Vector2((absPos.X ) / -3.75f, (absPos.Y ) / -2.8125f);
        //ImGui.Text($"True mouse pos: {truePos.X}, {truePos.Y}");

        // Only bother if flat
        int closestNodeIndex = -1;
        if (Globals.SphereGrid.lpamng->tilt_level == SphereGridTilt.Flat) {
            float shortestDistance = 20;
            for (int i = 0; i < 1024; i++) {
                float distance = (new Vector2(Globals.SphereGrid.lpamng->nodes[i].x, Globals.SphereGrid.lpamng->nodes[i].y) - truePos).Length();
                if (distance < shortestDistance){
                    closestNodeIndex = i;
                    shortestDistance = distance;
                }
            }
            if (closestNodeIndex != -1) {
                SphereGridNode closestNode = Globals.SphereGrid.lpamng->nodes[closestNodeIndex];
                //ImGui.Text($"Hovered node: {closestNodeIndex}, pos: ({closestNode.x}, {closestNode.y}), type: {(NodeType)closestNode.node_type}");

            }
            if (!ImGui.GetIO().WantCaptureMouse && ImGui.IsMouseReleased(ImGuiMouseButton.Left)) {
                clickedNodeIndex = closestNodeIndex;
            }
        }

        if (clickedNodeIndex != -1) {
            SphereGridNode* clickedNode = &Globals.SphereGrid.lpamng->nodes[clickedNodeIndex];
            ImGui.Text($"Clicked node: {clickedNodeIndex}, pos: ({clickedNode->x}, {clickedNode->y}), type: {(NodeType)clickedNode->node_type}");
            NodeType[] typeArray = Enum.GetValues<NodeType>();
            //ImGui.ListBox("Node type", clickedNode->node_type, typeArray, typeArray.Length);
            if (ImGui.BeginListBox("Node type")) {
                for (int i = 0; i < typeArray.Length; i++) {
                    bool is_selected = (typeArray[i] == clickedNode->node_type);
                    if (ImGui.Selectable($"{typeArray[i]}")) {
                        clickedNode->node_type = typeArray[i];
                        Globals.SphereGrid.lpamng->should_update = 1;
                        Globals.SphereGrid.lpamng->should_update_node = clickedNodeIndex;
                    }
                    if (is_selected) {
                        ImGui.SetItemDefaultFocus();
                    }
                }
                ImGui.EndListBox();
            }

            for (int i = 0; i < 7; i++) {
                if (ImGui.Button($"{id_to_character[i]}: {(clickedNode->activated_by & (1 << i)) != 0}")) {
                    clickedNode->activated_by ^= (byte)(1 << i);
                    ArchipelagoFFXModule.h_eiAbmParaGet();
                    Globals.SphereGrid.lpamng->should_update = 1;
                    // Setting to clickedNodeIndex only turns off light if no character has it activated. Setting to -1 correctly turns on/off node itself, but not surrounding lights (per character).
                    Globals.SphereGrid.lpamng->should_update_node = -1;
                }
            }
        }




        ImGui.End();
    }

    private static void render_sphere_grid_editor() {
        ImGuiStylePtr style = ImGui.GetStyle();

        if (ImGui.Button("Activate all nodes for all characters")) {
            for (int i = 0; i < Globals.SphereGrid.lpamng->node_count; i++) {
                Globals.SphereGrid.lpamng->nodes[i].activated_by = 0x7f;
            }
        }

        Matrix4x4* world_matrix = (Matrix4x4*)(*FhUtil.ptr_at<nint>(0x8cb9d8) + 0xd34);

        var mousePos = ImGui.GetMousePos();
        var centeredPos = mousePos - (ImGui.GetWindowViewport().Size * 0.5f);

        float main_x = 2560;
        float x_ratio = main_x / ImGui.GetWindowViewport().Size.X;
        float y_ratio = x_ratio * (3.0f/4.0f);
        var gridPos = new Vector2(centeredPos.X * x_ratio, centeredPos.Y * y_ratio);
        float zoom_mult = Globals.SphereGrid.lpamng->zoom_level.get_zoom();
        var absPos = new Vector2(-gridPos.X / zoom_mult + world_matrix->M31, -gridPos.Y / zoom_mult + world_matrix->M32);
        var truePos = new Vector2((absPos.X ) / -3.75f, (absPos.Y ) / -2.8125f);

        // Only bother if flat
        int closestNodeIndex = -1;
        if (Globals.SphereGrid.lpamng->tilt_level == SphereGridTilt.Flat) {
            float shortestDistance = 20;
            for (int i = 0; i < 1024; i++) {
                float distance = (new Vector2(Globals.SphereGrid.lpamng->nodes[i].x, Globals.SphereGrid.lpamng->nodes[i].y) - truePos).Length();
                if (distance < shortestDistance) {
                    closestNodeIndex = i;
                    shortestDistance = distance;
                }
            }
            if (closestNodeIndex != -1) {
                SphereGridNode closestNode = Globals.SphereGrid.lpamng->nodes[closestNodeIndex];

            }
            if (!ImGui.GetIO().WantCaptureMouse && ImGui.IsMouseReleased(ImGuiMouseButton.Left)) {
                clickedNodeIndex = closestNodeIndex;
            }
        }

        if (clickedNodeIndex != -1) {
            SphereGridNode* clickedNode = &Globals.SphereGrid.lpamng->nodes[clickedNodeIndex];
            ImGui.Text($"Clicked node: {clickedNodeIndex}, pos: ({clickedNode->x}, {clickedNode->y}), type: {(NodeType)clickedNode->node_type}");
            NodeType[] typeArray = Enum.GetValues<NodeType>();
            if (ImGui.BeginListBox("Node type")) {
                for (int i = 0; i < typeArray.Length-1; i++) {
                    bool is_selected = (typeArray[i] == clickedNode->node_type);
                    if (ImGui.Selectable($"{typeArray[i]}")) {
                        clickedNode->node_type = typeArray[i];
                        Globals.SphereGrid.lpamng->should_update = 1;
                        Globals.SphereGrid.lpamng->should_update_node = clickedNodeIndex;
                    }
                    if (is_selected) {
                        ImGui.SetItemDefaultFocus();
                    }
                }
                ImGui.EndListBox();
            }

            for (int i = 0; i < 7; i++) {
                if (ImGui.Button($"{id_to_character[i]}: {(clickedNode->activated_by & (1 << i)) != 0}")) {
                    clickedNode->activated_by ^= (byte)(1 << i);
                    ArchipelagoFFXModule.h_eiAbmParaGet();
                    Globals.SphereGrid.lpamng->should_update = 1;
                    // Setting to clickedNodeIndex only turns off light if no character has it activated. Setting to -1 correctly turns on/off node itself, but not surrounding lights (per character).
                    Globals.SphereGrid.lpamng->should_update_node = -1;
                }
            }
        }
    }

    private static void render_connection() {
        if (seed.SeedId is null && !FFXArchipelagoClient.is_connected) {
            string[] seedNames = [.. ArchipelagoFFXModule.loaded_seeds.Select(x => x.SeedId)];
            ImGui.Combo("Selected seed", ref selected_seed, seedNames, seedNames.Length);
        } else {
            ImGui.Text($"Loaded seed: {seed.SeedId}");
        }
        if (!FFXArchipelagoClient.is_connected) {
            ImGui.InputText("Address", ref client_input_address, 50);
            ImGui.InputText("Name", ref client_input_name, 50);
            ImGui.InputText("Password", ref client_input_password, 50);
            if (ImGui.Button("Connect")) {
                //Task.Run(() => FFXArchipelagoClient.Connect(client_input_address, client_input_name, client_input_password));
                _ = FFXArchipelagoClient.Connect(client_input_address, client_input_name, client_input_password);
                //FFXArchipelagoClient.Connect(client_input_address, client_input_name, client_input_password);
            }
        }
        else {
            ImGui.Text($"Connected as {FFXArchipelagoClient.active_player!.Name}");
            if (ImGui.Button("Disconnect")) {
                FFXArchipelagoClient.disconnect();
            }
        }
    }

    public static void add_log_message(List<(string, Color)> message) {
        lock (client_log_lock) {
            client_log.Add(message);
            client_log_updated = true;
        }
    }

    private static LinkedList<string> client_input_history = new();
    private static LinkedListNode<string>? client_input_history_current = null;
    private static ImGuiInputTextCallback _client_input_ImGuiInputTextCallback = client_input_ImGuiInputTextCallback;
    private static bool focus_client_input = false;
    private static int client_input_ImGuiInputTextCallback(ImGuiInputTextCallbackData* data) {
        if (data->EventFlag == ImGuiInputTextFlags.CallbackHistory) {
            if (data->EventKey == ImGuiKey.UpArrow) {
                if (client_input_history_current is null && client_input_history.First is not null) {
                    client_input_history_current = client_input_history.First;
                    data->DeleteChars(0, data->BufTextLen);
                    data->InsertChars(0, client_input_history_current.Value);
                } else if (client_input_history_current?.Next is not null) {
                    client_input_history_current = client_input_history_current.Next;
                    data->DeleteChars(0, data->BufTextLen);
                    data->InsertChars(0, client_input_history_current.Value);
                }
            } else if (data->EventKey == ImGuiKey.DownArrow) {

                client_input_history_current = client_input_history_current?.Previous;
                data->DeleteChars(0, data->BufTextLen);
                if (client_input_history_current is not null) {
                    data->InsertChars(0, client_input_history_current.Value);
                }
            }
        }
        return 0;
    }
    private static void render_console() {
        ImGuiStylePtr style = ImGui.GetStyle();
        if (ImGui.BeginChild("Archipelago.GUI.Log", new(0, ImGui.GetContentRegionAvail().Y - ImGui.GetTextLineHeight() - 3 * style.ItemSpacing.Y), ImGuiChildFlags.Borders, ImGuiWindowFlags.NoMove)) {
            //var curr_scroll = ImGui.GetScrollY() / previous_scroll_max;
            lock (client_log_lock) {
                foreach (var line in client_log) {
                    byte part_counter = 0;
                    int part_length = line.Count;
                    //ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(0,style.ItemSpacing.Y));
                    //ImGui.PushTextWrapPos(ImGui.GetCursorPosX() + ImGui.GetWindowWidth() - style.WindowPadding.X);
                    var wrap_width = ImGui.GetContentRegionAvail().X;
                    var remaining_width = wrap_width;
                    foreach (var part in line) {
                        var color = new Vector4(part.color.R / 255f, part.color.G / 255f, part.color.B / 255f, 1.0f);
                        ImGui.PushStyleColor(ImGuiCol.Text, color);
                        foreach (var word in part.text.Split(" ")) {
                            var word_width = ImGui.CalcTextSize($"{word} ").X;
                            if (part_counter > 0 && word_width < remaining_width) {
                                ImGui.SameLine();
                                ImGui.TextUnformatted(word);
                                remaining_width -= word_width;
                            }
                            else {
                                ImGui.TextUnformatted(word);
                                remaining_width = wrap_width - word_width;
                            }
                            part_counter++;
                        }
                        ImGui.PopStyleColor();
                        //ImGui.TextWrapped(line);
                    }
                    //ImGui.PopTextWrapPos();
                    //ImGui.PopStyleVar();
                }
            }
            //previous_scroll_max = ImGui.GetScrollMaxY();
            if (client_log_updated) {
                ImGui.SetScrollHereY();
                client_log_updated = false;
            }
            else {
                //ImGui.SetScrollY(curr_scroll * ImGui.GetScrollMaxY());
            }
            //ImGui.TextWrapped(string.Join("\n", client_log));
        }
        ImGui.EndChild();

        if (focus_client_input) {
            ImGui.SetKeyboardFocusHere();
            focus_client_input = false;
        }
        bool process_input = ImGui.InputText("Input",
                                             ref client_input_command,
                                             50,
                                             ImGuiInputTextFlags.EnterReturnsTrue | ImGuiInputTextFlags.CallbackHistory,
                                             _client_input_ImGuiInputTextCallback);

        if (process_input) {
            client_input_history.AddFirst(client_input_command);
            if (client_input_history.Count > 10) {
                client_input_history.RemoveLast();
            }
            client_input_history_current = null;

            focus_client_input = true;

            if (!client_input_command.StartsWith("/")) {
                // Say
                if (FFXArchipelagoClient.is_connected) {
                    FFXArchipelagoClient.current_session!.Say(client_input_command);
                }

            }
            else {
                // Client-side command
                string[] cmd = client_input_command.Split(" ");

                Action fn = cmd switch {
                    ["/resetregion", string regionString] => () => {
                        if (Enum.TryParse(regionString, out RegionEnum region)) {
                            ArchipelagoFFXModule.region_states[region].story_progress = region_starting_state[region].story_progress;
                            ArchipelagoFFXModule.region_states[region].room_id        = region_starting_state[region].room_id;
                            ArchipelagoFFXModule.region_states[region].entrance       = region_starting_state[region].entrance;
                            region_starting_state[region].savedata.CopyTo(ArchipelagoFFXModule.region_states[region].savedata);

                            List<(string, Color)> message = [(regionString, Color.Blue), (" has been reset", Color.White)];
                            add_log_message(message);
                        }
                        else {
                            List<(string, Color)> message = [("invalid region: ", Color.Red), (regionString, Color.Blue)];
                            add_log_message(message);
                        }
                    }
                    ,
                    ["/resetregion", ..] => () => {
                        List<(string, Color)> message = [("Wrong arguments for '/resetregion': Should be ", Color.Red), ($"/resetregion regionName", Color.Blue)];
                        add_log_message(message);
                    }
                    ,
#if DEBUG
                    ["/setdatastorage", string key, string value] => () => {
                        if (FFXArchipelagoClient.is_connected) {
                            FFXArchipelagoClient.current_session?.DataStorage[Scope.Slot, "FFX_ROOM"] = value;
                        }
                    }
                    ,
                    ["/getdatastorage", string key] => () => {
                        if (FFXArchipelagoClient.is_connected) {
                            string? message_text = FFXArchipelagoClient.current_session?.DataStorage[Scope.Slot, key];
                            if (message_text != null) {
                                List<(string, Color)> message = [(key, Color.Blue), (message_text, Color.White)];
                                add_log_message(message);
                            }
                        }
                    }
                    ,
                    ["/setregion", string regionString, string progressString, string mapString, string entranceString] => () => {
                        if (Enum.TryParse(regionString, out RegionEnum region)) {
                            if (ushort.TryParse(progressString, out ushort progress)) {
                                if (ushort.TryParse(mapString, out ushort map)) {
                                    if (ushort.TryParse(entranceString, out ushort entrance)) {
                                        ArchipelagoRegion r = ArchipelagoFFXModule.region_states[region];
                                        r.story_progress = progress;
                                        r.room_id = map;
                                        r.entrance = entrance;

                                        List<(string, Color)> message = [(regionString, Color.Blue), ($"'s state has been set to (story_progress: {progress}, room_id: {map}, entrance: {entrance})", Color.White)];
                                        add_log_message(message);
                                    }
                                    else {
                                        List<(string, Color)> message = [("invalid entrance_id: ", Color.Red), (entranceString, Color.Blue)];
                                        add_log_message(message);
                                    }
                                }
                                else{
                                    List<(string, Color)> message = [("invalid map_id: ", Color.Red), (mapString, Color.Blue)];
                                    add_log_message(message);
                                }
                    
                            }
                            else {
                                List<(string, Color)> message = [("invalid story_progress: ", Color.Red), (progressString, Color.Blue)];
                                add_log_message(message);
                            }
                    
                        }
                        else {
                            List<(string, Color)> message = [("invalid region: ", Color.Red), (regionString, Color.Blue)];
                            add_log_message(message);
                        }
                    }
                    ,
                    ["/setregion", ..] => () => {
                        List<(string, Color)> message = [("Wrong arguments for '/setregion': Should be ", Color.Red), ($"/setregion regionName story_progress map_id entrance_id", Color.Blue)];
                        add_log_message(message);
                    }
                    ,
                    ["/warp", string map, string entrance] => () => {
                        if (int.TryParse(map, out int map_id)) {
                            if (int.TryParse(entrance, out int entrance_id)) {
                                List<(string, Color)> message = [("Warping to ", Color.White), ($"{map_id} (entrance {entrance_id})", Color.Blue)];
                                add_log_message(message);
                                ArchipelagoFFXModule.call_warp_to_map(map_id, entrance_id);
                            }
                            else {
                                List<(string, Color)> message = [("invalid entrance_id: ", Color.Red), (entrance, Color.Blue)];
                                add_log_message(message);
                            }
                        }
                        else {
                            List<(string, Color)> message = [("invalid map_id: ", Color.Red), (map, Color.Blue)];
                            add_log_message(message);
                        }
                    }
                    ,
                    ["/warp", ..] => () => {
                        List<(string, Color)> message = [("Wrong arguments for /warp: Should be ", Color.Red), ($"/warp map_id entrance_id", Color.Blue)];
                        add_log_message(message);
                    }
                    ,
#endif
                    ["/send_checks"] => () => {
                        FFXArchipelagoClient.local_locations_updated = true;

                        List<(string, Color)> message = [("Resending local checks", Color.White)];
                        add_log_message(message);
                    }
                    ,
                    ["/clear"] => () => {
                        lock (client_log_lock) {
                            client_log.Clear(); 
                        }
                    }
                    ,
                    ["/help"] => () => {
                        add_log_message([("Available commands:", Color.White)]);
#if DEBUG
                        add_log_message([("/setregion regionName progress map entrance", Color.White)]);
#endif
                        add_log_message([("/resetregion regionName", Color.White)]);
                        add_log_message([("/send_checks", Color.White)]);
                        add_log_message([("/clear", Color.White)]);
                    }
                    ,
                    _ => () => {
                        List<(string, Color)> message = [("unknown command: ", Color.Red), (client_input_command, Color.Blue)];
                        add_log_message(message);
                    }
                };
                fn();
                client_log_updated = true;
            }
            client_input_command = "";
        }
    }
    
    private static void render_debug_tab() {
#if DEBUG
        fixed (int* ap_mult = &ArchipelagoFFXModule.ap_multiplier) {
            uint step = 1;
            uint step_fast = 10;
            ImGui.InputScalar("AP multiplier", ImGuiDataType.U32, ap_mult, &step, &step_fast);
        }
#endif

        ImGui.Text($"Current room: {Globals.save_data->current_room_id} ({Marshal.PtrToStringAnsi((nint)ArchipelagoFFXModule.get_event_name(*(uint*)Globals.event_id))!})");
        ImGui.Text($"Current region: {ArchipelagoFFXModule.current_region}");
        ImGui.Text($"Current story progress: {Globals.save_data->story_progress}");
        if (ArchipelagoFFXModule.current_region != RegionEnum.None) {
            foreach (var data in ArchipelagoFFXModule.region_states[current_region].savedata) {
                ImGui.Text($"{data.offset}: {string.Join(" ", data.bytes.Select(b => b.ToString()).ToArray())}");
            }
        }

        ImGui.SeparatorText("Region states");
        if (ImGui.BeginTable("Region states", 5)) {
            ImGui.TableSetupColumn("Region");
            ImGui.TableSetupColumn("story_progress");
            ImGui.TableSetupColumn("room");
            ImGui.TableSetupColumn("entrance");
            ImGui.TableSetupColumn("completed_visits");
            ImGui.TableHeadersRow();
            foreach (var region in ArchipelagoFFXModule.region_states) {
                ImGui.TableNextColumn();
                ImGui.Text($"{region.Key}");
                ImGui.TableNextColumn();
                ImGui.Text($"{region.Value.story_progress}");
                ImGui.TableNextColumn();
                ImGui.Text($"{region.Value.room_id}");
                ImGui.TableNextColumn();
                ImGui.Text($"{region.Value.entrance}");
                ImGui.TableNextColumn();
                ImGui.Text($"{region.Value.completed_visits}");
            }
            ImGui.EndTable();
        }

        if (Globals.Battle.btl->battle_state != 0) {
            ImGui.Text($"Battle Name: {Marshal.PtrToStringAnsi((nint)FhUtil.ptr_at<char>(0xD2C25A))}");
        }
        else {
#if DEBUG
            fixed (uint* battle_input = &LaunchBattleInput) {
                uint p_step = 1;
                uint p_step_fast = 10;
                ImGui.InputScalar("launchBattleInput", ImGuiDataType.U32, battle_input, &p_step, &p_step_fast, "%x");
            }
            if (ImGui.Button("launchBattleButton")) {
                ArchipelagoFFXModule._MsBattleLabelExe(LaunchBattleInput, 1, 1);
            }
#endif
        }

        /*
        AtelBasicWorker* worker0 = Atel.controllers[0].worker(0);
        Chr * curr_chr = worker0->chr_handle;
        if (curr_chr != null) {
            //ImGui.InputScalar("grav_mode", ImGuiDataType.U8, (nint)(&curr_chr->grav_mode));
            //ImGui.InputScalar("field_mode", ImGuiDataType.U8, (nint)(&curr_chr->field_mode));
            //ImGui.InputScalar("motion_type", ImGuiDataType.U8, (nint)(&curr_chr->motion_type));

            bool load_character = ImGui.InputInt("character_model", ref character_model);

            if (load_character) {
                character_model = Math.Clamp(character_model, 0, 6);
                ArchipelagoModule.set_character_model((PlySaveId)character_model);
            }

            //ImGui.Text($"grav mode: {curr_chr->grav_mode}");
            //ImGui.Text($"field mode: {curr_chr->field_mode}");
            //ImGui.Text($"motion type: {curr_chr->motion_type}");
        }
         */
    }

    private static void render_unlocks() {
        string s = "Unlocked regions:";
        ImGui.SetCursorPosX((ImGui.GetWindowWidth() - ImGui.CalcTextSize(s).X) * 0.5f);
        ImGui.Text(s);
        if (ImGui.BeginTable("Region Unlocks", 3)) {
            foreach (var (region, i) in ArchipelagoFFXModule.region_is_unlocked.Select((value, i) => (value, i))) {
                ImGui.TableNextColumn();
                Color color = region.Value ? Color.Green : Color.Red;
                ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(color.R / 255f, color.G / 255f, color.B / 255f, 1.0f));
#if !DEBUG
                ImGui.BeginDisabled();
#endif
                bool unlocked = ArchipelagoFFXModule.region_is_unlocked[region.Key];
                if (ImGui.Checkbox($"###Archipelago.GUI.Unlocks.{region.Key}", &unlocked)) {
                    ArchipelagoFFXModule.region_is_unlocked[region.Key] = unlocked;
                }
#if !DEBUG
                ImGui.EndDisabled();
#endif
                ImGui.SameLine();
                ImGui.Text($"{region.Key}");
                ImGui.PopStyleColor();
            }
            ImGui.EndTable();
        }

        s = "Unlocked characters:";
        ImGui.SetCursorPosX((ImGui.GetWindowWidth() - ImGui.CalcTextSize(s).X) * 0.5f);
        ImGui.Text(s);
        if (ImGui.BeginTable("Character Unlocks", 3)) {
            foreach (var (character, i) in ArchipelagoFFXModule.unlocked_characters.Select((value, i) => (value, i))) {
                ImGui.TableNextColumn();
                Color color = character.Value ? Color.Green : Color.Red;
                ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(color.R / 255f, color.G / 255f, color.B / 255f, 1.0f));
#if !DEBUG
                ImGui.BeginDisabled();
#endif
                bool unlocked = ArchipelagoFFXModule.unlocked_characters[character.Key];
                if (ImGui.Checkbox($"###Archipelago.GUI.Unlocks.{id_to_character[character.Key]}", &unlocked)) {
                    ArchipelagoFFXModule.unlocked_characters[character.Key] = unlocked;
                }
#if !DEBUG
                ImGui.EndDisabled();
#endif
                ImGui.SameLine();
                ImGui.Text($"{id_to_character[character.Key]}");
                ImGui.PopStyleColor();
            }
            ImGui.EndTable();
        }
    }

    private static void render_settings() {
        ImGui.SliderInt("Font size", ref font_size, 10, 60);

        if (ImGui.BeginCombo("Voice", voice_lang == 0xFF ? "Default" : ((FhLangId)voice_lang).ToString())) {
            if (ImGui.Selectable("Default", voice_lang == 0xFF)) {
                voice_lang = 0xFF;
            }
            if (ImGui.Selectable("Japanese", voice_lang == (byte)FhLangId.Japanese)) {
                voice_lang = (byte)FhLangId.Japanese;
            }
            if (ImGui.Selectable("English", voice_lang == (byte)FhLangId.English)) {
                voice_lang = (byte)FhLangId.English;
            }
            ImGui.EndCombo();
        }

        if (ImGui.BeginCombo("Text", text_lang == 0xFF ? "Default" : ((FhLangId)text_lang).ToString())) {
            if (ImGui.Selectable("Default", text_lang == 0xFF)) {
                text_lang = 0xFF;
            }
            foreach (FhLangId lang in Enum.GetValues<FhLangId>()) {
                if (ImGui.Selectable($"{lang}", text_lang == (byte)lang)) {
                    text_lang = (byte)lang;
                }
            }
            ImGui.EndCombo();
        }

        if (ImGui.Button("Save settings")) {
            ArchipelagoFFXModule.VoiceLanguage = voice_lang != 0xFF ? (FhLangId)voice_lang : null;
            ArchipelagoFFXModule.TextLanguage = text_lang != 0xFF ? (FhLangId)text_lang : null;
            ArchipelagoFFXModule.save_settings();
        }
    }

    private static void render_client() {
        enabled ^= ImGui.IsKeyPressed(archipelago_gui_key);
        if (!enabled) return;
        ImGuiStylePtr style = ImGui.GetStyle();

        if (ImGui.Begin("Archipelago###Archipelago.GUI")) {
            if (shiori_image != null || (shiori_file != null && FhApi.Resources.load_png_from_disk(shiori_file.FullName, out shiori_image))) {
                ImGui.GetWindowDrawList().AddImage(shiori_image.TextureRef, ImGui.GetWindowPos(), ImGui.GetWindowPos() + ImGui.GetWindowSize(), 0x55555555);
            }
            if (ImGui.BeginTabBar("TabBar###Archipelago.GUI.TabBar")) {
                if (ImGui.BeginTabItem("Main###Archipelago.GUI.TabBar.Main")) {
                    render_connection();

                    render_console();
                    ImGui.EndTabItem();
                }
                if (ImGui.BeginTabItem("Inventory###Archipelago.GUI.TabBar.Inventory")) {
                    ImGui.SeparatorText("Excess");
                    if (excess_inventory.Count == 0) {
                        ImGui.Text("Empty");
                    } else { 
                        foreach ((uint item_id, int amount) in excess_inventory) {
                            if (amount == 0) continue;
                            string item_name = get_item_name(item_id);
                            ImGui.Text($"{item_name}: {amount}");
                        }
                    }
                    ImGui.SeparatorText("Other");
                    if (other_inventory.Count == 0) {
                        ImGui.Text("Empty");
                    }
                    else {
                        foreach ((uint item_id, int amount) in other_inventory) {
                            if (amount == 0) continue;
                            string item_name = get_other_item_name(item_id);
                            ImGui.Text($"{item_name}: {amount}");
                        }
                    }
                    ImGui.EndTabItem();
                }
                if (ImGui.BeginTabItem("Unlocks###Archipelago.GUI.TabBar.Unlocks")) {
                    render_unlocks();
                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem("Debug###Archipelago.GUI.TabBar.Debug")) {
                    render_debug_tab();
                    ImGui.EndTabItem();
                }

#if DEBUG
                if (Globals.SphereGrid.lpamng != null && *Globals.SphereGrid.is_open && ImGui.BeginTabItem("Sphere Grid###Archipelago.GUI.TabBar.SphereGrid")) {
                    render_sphere_grid_editor();
                    ImGui.EndTabItem();
                }
#endif
                if (ImGui.BeginTabItem("Settings###Archipelago.GUI.TabBar.Settings")) {
                    render_settings();
                    ImGui.EndTabItem();
                }

                ImGui.EndTabBar();
            }
        }
        ImGui.End();

    }

}
