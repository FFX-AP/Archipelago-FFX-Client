using Archipelago.MultiClient.Net.Enums;
using ArchipelagoFFX.Client;
using Fahrenheit;
using Fahrenheit.FFX;
using Fahrenheit.FFX.Battle;
using Hexa.NET.ImGui;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using static ArchipelagoFFX.ArchipelagoData;
using static ArchipelagoFFX.ArchipelagoFFXModule;
using static Fahrenheit.FFX.Globals;
using Color = Archipelago.MultiClient.Net.Models.Color;
using FhXCall = Fahrenheit.FFX.FhCall;

namespace ArchipelagoFFX.GUI;

[FhLoad(FhGameId.FFX)]
public unsafe class ArchipelagoGuiModule : FhModule {
    public const ImGuiKey archipelago_gui_key = ImGuiKey.F8;
    public const ImGuiKey experimental_gui_key = ImGuiKey.F9;

    public bool enabled = false;
    public bool experiments_enabled = false;
    private bool show = true;

    public  const  string DEFAULT_CLIENT_ADDRESS = "archipelago.gg:";
    public  string client_input_address  = DEFAULT_CLIENT_ADDRESS;
    public  string client_input_name     = "";
    private string client_input_password = "";

    private static string client_input_command = "";

    public readonly System.Threading.Lock client_log_lock = new();
    private List<List<(string text, Color color)>> client_log = [];
    public bool client_log_updated = false;
    private float previous_scroll = 1;
    private float previous_scroll_max = 1;


    private static readonly Vector2 PANE_BUTTON_SIZE = new Vector2(16f);

    private int grav_mode = 1;
    private int field_mode = 0;
    private int motion_type = 0;

    private int character_model = 0;

    private int[] MsBtlGetPosParams = [0, 0, 0];
    private Vector4 MsBtlGetPosResult = new(0, 0, 0, 0);
    private uint LaunchBattleInput = 0;

    private  int auto_ability_id = 0;
    private  AutoAbility* ability = null;
    private  int chr_id = 0;
    private Equipment* weapon = null;
    private Equipment* armor = null;

    private int clickedNodeIndex = -1;


    public int selected_seed;

    public int font_size = -1;

    private bool show_popup;
    public string popup_content {
        get;
        set {
            field = value;
            show_popup = value != "";
        }
    }

    private FhModuleHandle<ArchipelagoClientModule> _client_handle;
    private ArchipelagoClientModule? _client;

    private FhModuleHandle<ArchipelagoFFXModule> _ffx_interop_handle;
    private ArchipelagoFFXModule? _ffx_interop;

    private FhModuleHandle<DeathLinkModule> _deathlink_handle;
    private DeathLinkModule? _deathlink;

    private FhModuleHandle<HardcoreDreamsEndModule> _hardcore_dreams_end_handle;
    private HardcoreDreamsEndModule? _hardcore_dreams_end;

    public ArchipelagoGuiModule() {
        _client_handle = new(this);
        _ffx_interop_handle = new(this);
        _hardcore_dreams_end_handle = new(this);
        _deathlink_handle = new(this);
    }

    public override bool init(FhModContext mod_context, FileStream global_state_file) {
        shiori_file = mod_context.Paths.ResourcesDir.GetFiles("shiori.png").FirstOrDefault();

        return _client_handle.try_get_module(out _client)
            && _ffx_interop_handle.try_get_module(out _ffx_interop)
            && _hardcore_dreams_end_handle.try_get_module(out _hardcore_dreams_end)
            && _deathlink_handle.try_get_module(out _deathlink);
    }

    public override void render_imgui() {
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

    public  FileInfo?  shiori_file;
    private FhTexture? shiori_image;

    private int voiceline_id;
    public  byte voice_lang = 0xFF;
    public  byte text_lang  = 0xFF;

    private void render_experiments() {
        experiments_enabled ^= ImGui.IsKeyPressed(experimental_gui_key);
        if (!experiments_enabled) return;

        ImGuiStylePtr style = ImGui.GetStyle();
        if (ImGui.Begin("Archipelago###Archipelago.Experiments.GUI")) {

            float frameHeight = ImGui.GetFrameHeight();
            Vector2 windowPos = ImGui.GetWindowPos();
            float windowBorderSize = ImGui.GetStyle().WindowBorderSize;

            if (shiori_image?.try_use(out ImTextureRef texture_ref, out _) ?? false) {
                Vector2 image_tl = windowPos + new Vector2(windowBorderSize);
                Vector2 image_br = windowPos + new Vector2(frameHeight) - new Vector2(windowBorderSize);

                ImGui.GetForegroundDrawList().AddImage(texture_ref, image_tl, image_br);
            }

            if (ImGui.Checkbox("Original soundtrack?", &save_data->soundtrack_type)) {
                FhXCall.h_FUN_008cc120.fnptr!(save_data->soundtrack_type ? 1 : 0);
            }

            ImGui.InputScalarN("frontline? (0x1FC5)", ImGuiDataType.U8, Battle.btl->__0x1FC5, 7);
            ImGui.InputScalarN("frontline? (0x1FCC)", ImGuiDataType.U8, Battle.btl->__0x1FCC, 7);
            ImGui.InputScalarN("backline? (0x1FD3)", ImGuiDataType.U8, Battle.btl->__0x1FD3, 17);



            Vector4* Vector4f_ARRAY_00c86010 = FhUtil.ptr_at<Vector4>(0x886010);

            ImGui.InputFloat4("Tidus ambient(?) color", &Vector4f_ARRAY_00c86010->X);

            //var goalRequirements = Enum.GetNames<GoalRequirement>();
            //int currentRequirement = (int)seed.Options.GoalRequirement;
            //if (ImGui.Combo("Goal Requirement", ref currentRequirement, goalRequirements, goalRequirements.Length)) {
            //    seed.Options.GoalRequirement = (GoalRequirement)currentRequirement;
            //}

            //ImGui.InputInt("Required Party Members", ref seed.Options.RequiredPartyMembers);

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

            ImGui.Text($"Tidus overdrive uses: {save_data->tidus_limit_uses}");

            //for (int i = 0; i < 18; i++) {
            //    string name = Globals.save_data->character_names[i].name;
            //    if (ImGui.InputText($"Name {i}", ref name, 20)) {
            //        Globals.save_data->character_names[i].name = name;
            //    }
            //}

            ImGui.InputInt("Voiceline id", ref voiceline_id);
            if (ImGui.Button("Play voiceline")) {
                _ffx_interop!.queued_voice_lines.Enqueue(voiceline_id);
            }

            int inMenu = FhUtil.get_at<int>(0x01efb4d4);
            ImGui.Text($"Is in menu?: {inMenu}");

            BtlArea* pos_def_ptr = Battle.btl->ptr_pos_def;

            if (pos_def_ptr != null && Battle.btl->battle_state != 0) {
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
            //    //FhXCall.h_FfxFmod_soundInit.fnptr!(*(nint*)(_FmodManager+8));
            //
            //    nint ffxFmod = *(nint*)(_FmodManager + 8);
            //
            //    nint fmodVoice = *(nint*)(ffxFmod + 0x10);
            //
            //    *(byte*)(fmodVoice + 4) = (byte)voice_language;
            //
            //    FhXCall.h_FmodVoice_initList.fnptr!(fmodVoice);
            //
            //    int dataChange_result = FmodVoice_dataChange(fmodVoice, Globals.save_data->current_room_id, *(nint*)(ffxFmod + 4));
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
    }

    private void render_sphere_grid_editor() {
        ImGuiStylePtr style = ImGui.GetStyle();

        if (ImGui.Button("Activate all nodes for all characters")) {
            for (int i = 0; i < SphereGrid.lpamng->node_count; i++) {
                SphereGrid.lpamng->nodes[i].activated_by = 0x7f;
            }
        }

        Matrix4x4* world_matrix = (Matrix4x4*)(*FhUtil.ptr_at<nint>(0x8cb9d8) + 0xd34);

        var mousePos = ImGui.GetMousePos();
        var centeredPos = mousePos - (ImGui.GetWindowViewport().Size * 0.5f);

        float main_x = 2560;
        float x_ratio = main_x / ImGui.GetWindowViewport().Size.X;
        float y_ratio = x_ratio * (3.0f/4.0f);
        var gridPos = new Vector2(centeredPos.X * x_ratio, centeredPos.Y * y_ratio);
        float zoom_mult = SphereGrid.lpamng->zoom_level.get_zoom();
        var absPos = new Vector2(-gridPos.X / zoom_mult + world_matrix->M31, -gridPos.Y / zoom_mult + world_matrix->M32);
        var truePos = new Vector2((absPos.X ) / -3.75f, (absPos.Y ) / -2.8125f);

        // Only bother if flat
        int closestNodeIndex = -1;
        if (SphereGrid.lpamng->tilt_level == SphereGridTilt.FLAT) {
            float shortestDistance = 20;
            for (int i = 0; i < 1024; i++) {
                float distance = (new Vector2(SphereGrid.lpamng->nodes[i].x, SphereGrid.lpamng->nodes[i].y) - truePos).Length();
                if (distance < shortestDistance) {
                    closestNodeIndex = i;
                    shortestDistance = distance;
                }
            }
            if (closestNodeIndex != -1) {
                SphereGridNode closestNode = SphereGrid.lpamng->nodes[closestNodeIndex];

            }
            if (!ImGui.GetIO().WantCaptureMouse && ImGui.IsMouseReleased(ImGuiMouseButton.Left)) {
                clickedNodeIndex = closestNodeIndex;
            }
        }

        if (clickedNodeIndex != -1) {
            SphereGridNode* clickedNode = &SphereGrid.lpamng->nodes[clickedNodeIndex];
            ImGui.Text($"Clicked node: {clickedNodeIndex}, pos: ({clickedNode->x}, {clickedNode->y}), type: {clickedNode->node_type}");
            NodeType[] typeArray = Enum.GetValues<NodeType>();
            if (ImGui.BeginListBox("Node type")) {
                for (int i = 0; i < typeArray.Length - 1; i++) {
                    bool is_selected = (typeArray[i] == clickedNode->node_type);
                    if (ImGui.Selectable($"{typeArray[i]}")) {
                        clickedNode->node_type = typeArray[i];
                        SphereGrid.lpamng->should_update = 1;
                        SphereGrid.lpamng->should_update_node = clickedNodeIndex;
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
                    FhXCall.h_eiAbmParaGet.fnptr!();
                    SphereGrid.lpamng->should_update = 1;
                    // Setting to clickedNodeIndex only turns off light if no character has it activated. Setting to -1 correctly turns on/off node itself, but not surrounding lights (per character).
                    SphereGrid.lpamng->should_update_node = -1;
                }
            }
        }
    }

    private void render_connection() {
        if (seed.Options.SeedId is null && !(_client!.is_connected)) {
            string[] seedNames = [.. loaded_seeds.Select(x => x.Name)];
            if (ImGui.Combo("Selected seed", ref selected_seed, seedNames, seedNames.Length)) {
                ArchipelagoSeed seed = loaded_seeds[selected_seed];
                client_input_name = seed.Options.PlayerName;
                if (SeedToServer.TryGetValue(seed.Options.SeedId, out string? server)) {
                    client_input_address = server;
                } else {
                    client_input_address = DEFAULT_CLIENT_ADDRESS;
                }
            }
        } else {
            ImGui.Text($"Loaded seed: {seed.Name}");
        }
        if (!(_client!.is_connected)) {
            ImGui.InputText("Address", ref client_input_address, 50);
            ImGui.InputText("Name", ref client_input_name, 50);
            ImGui.InputText("Password", ref client_input_password, 50);
            if (ImGui.Button("Connect")) {
                //Task.Run(() => FFXArchipelagoClient.Connect(client_input_address, client_input_name, client_input_password));
                _ = _client!.Connect(client_input_address, client_input_name, client_input_password);
                //FFXArchipelagoClient.Connect(client_input_address, client_input_name, client_input_password);
            }
        } else {
            ImGui.Text($"Connected as {_client!.active_player?.Name}");
            if (ImGui.Button("Disconnect")) {
                _client!.disconnect();
            }
        }
    }

    public void add_log_message(List<(string, Color)> message) {
        lock (client_log_lock) {
            client_log.Add(message);
            client_log_updated = true;
        }
    }

    private LinkedList<string> client_input_history = new();
    private LinkedListNode<string>? client_input_history_current;
    private bool focus_client_input;
    private int client_input_ImGuiInputTextCallback(ImGuiInputTextCallbackData* data) {
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

    private void render_console() {
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
                            } else {
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
            } else {
                //ImGui.SetScrollY(curr_scroll * ImGui.GetScrollMaxY());
            }
            //ImGui.TextWrapped(string.Join("\n", client_log));
        }
        ImGui.EndChild();

        if (focus_client_input) {
            ImGui.SetKeyboardFocusHere();
            focus_client_input = false;
        }

        bool process_input = ImGui.InputText(
            "Input",
            ref client_input_command,
            150,
            ImGuiInputTextFlags.EnterReturnsTrue | ImGuiInputTextFlags.CallbackHistory,
            client_input_ImGuiInputTextCallback
        );

        if (process_input && client_input_command.Length > 0) {
            client_input_history.AddFirst(client_input_command);
            if (client_input_history.Count > 10) {
                client_input_history.RemoveLast();
            }
            client_input_history_current = null;

            focus_client_input = true;

            if (!client_input_command.StartsWith("/")) {
                // Say
                _client!.SayAsync(client_input_command);
            } else {
                // Client-side command
                string[] cmd = client_input_command.Split(" ");
                Action cmd_fn = parse_command(cmd);

                cmd_fn();
                client_log_updated = true;
            }
            client_input_command = "";
        }
    }

    private Action parse_command(string[] command) {
        return command switch {
            ["/resetregion", { } region_name] => () => {
                RegionEnum region = stringToRegion(region_name);
                if (region != RegionEnum.None) {
                    ArchipelagoRegion region_state = region_states[region];
                    region_state.story_progress = region_starting_state[region].story_progress;
                    region_state.room_id        = region_starting_state[region].room_id;
                    region_state.entrance       = region_starting_state[region].entrance;
                    region_starting_state[region].savedata.CopyTo(region_state.savedata);

                    List<(string, Color)> message = [(region.ToString(), Color.Blue), (" has been reset", Color.White)];
                    add_log_message(message);
                }
                else {
                    List<(string, Color)> message = [("invalid region: ", Color.Red), (region_name, Color.Blue)];
                    add_log_message(message);
                }
            },

            ["/resetregion", ..] => () => {
                List<(string, Color)> message = [("Wrong arguments for '/resetregion': Should be ", Color.Red), ("/resetregion regionName", Color.Blue)];
                add_log_message(message);
            },

#if DEBUG
            ["/setdatastorage", { } key, { } value] => () => {
                lock (_client!.client_lock) {
                    if (_client!.is_connected) {
                        _client!.current_session!.DataStorage[Scope.Slot, key] = value;
                    }

                }
            },

            ["/getdatastorage", { } key] => () => {
                lock (_client!.client_lock) {
                    if (_client!.is_connected) {
                        string? message_text = _client!.current_session!.DataStorage[Scope.Slot, key];
                        if (message_text != null) {
                            List<(string, Color)> message = [(key, Color.Blue), (message_text, Color.White)];
                            add_log_message(message);
                        }
                    }
                }
            },

            ["/setregion", { } regionString, { } progressString, { } mapString, { } entranceString] => () => {
                RegionEnum region = stringToRegion(regionString);
                if (region != RegionEnum.None) {
                    if (ushort.TryParse(progressString, out ushort progress)) {
                        if (ushort.TryParse(mapString, out ushort map)) {
                            if (ushort.TryParse(entranceString, out ushort entrance)) {
                                ArchipelagoRegion region_state = region_states[region];
                                region_state.story_progress = progress;
                                region_state.room_id = map;
                                region_state.entrance = entrance;

                                List<(string, Color)> message = [(region.ToString(), Color.Blue), ($"'s state has been set to (story_progress: {progress}, room_id: {map}, entrance: {entrance})", Color.White)];
                                add_log_message(message);
                            } else {
                                List<(string, Color)> message = [("invalid entrance_id: ", Color.Red), (entranceString, Color.Blue)];
                                add_log_message(message);
                            }
                        } else {
                            List<(string, Color)> message = [("invalid map_id: ", Color.Red), (mapString, Color.Blue)];
                            add_log_message(message);
                        }
                    } else {
                        List<(string, Color)> message = [("invalid story_progress: ", Color.Red), (progressString, Color.Blue)];
                        add_log_message(message);
                    }
                } else {
                    List<(string, Color)> message = [("invalid region: ", Color.Red), (regionString, Color.Blue)];
                    add_log_message(message);
                }
            },

            ["/setregion", ..] => () => {
                List<(string, Color)> message = [("Wrong arguments for '/setregion': Should be ", Color.Red), ("/setregion regionName story_progress map_id entrance_id", Color.Blue)];
                add_log_message(message);
            },

            ["/warp", { } map, { } entrance] => () => {
                if (int.TryParse(map, out int map_id)) {
                    if (int.TryParse(entrance, out int entrance_id)) {
                        List<(string, Color)> message = [("Warping to ", Color.White), ($"{map_id} (entrance {entrance_id})", Color.Blue)];
                        add_log_message(message);
                        _ffx_interop!.call_warp_to_map(map_id, entrance_id);
                    } else {
                        List<(string, Color)> message = [("invalid entrance_id: ", Color.Red), (entrance, Color.Blue)];
                        add_log_message(message);
                    }
                } else {
                    List<(string, Color)> message = [("invalid map_id: ", Color.Red), (map, Color.Blue)];
                    add_log_message(message);
                }
            },

            ["/warp", ..] => () => {
                List<(string, Color)> message = [("Wrong arguments for /warp: Should be ", Color.Red), ($"/warp map_id entrance_id", Color.Blue)];
                add_log_message(message);
            },
#endif

            ["/send_checks"] => () => {
                _client!.local_locations_updated = true;

                List<(string, Color)> message = [("Resending local checks", Color.White)];
                add_log_message(message);
            },

            ["/clear"] => () => {
                lock (client_log_lock) {
                    client_log.Clear();
                }
            },

            ["/help"] => () => {
                add_log_message([("Available commands:", Color.White)]);
#if DEBUG
                add_log_message([("/setdatastorage key value", Color.White)]);
                add_log_message([("/getdatastorage key", Color.White)]);
                add_log_message([("/warp map entrance", Color.White)]);
                add_log_message([("/setregion regionName progress map entrance", Color.White)]);
#endif
                add_log_message([("/resetregion regionName", Color.White)]);
                add_log_message([("/send_checks", Color.White)]);
                add_log_message([("/clear", Color.White)]);
            },

            _ => () => {
                List<(string, Color)> message = [("unknown command: ", Color.Red), (client_input_command, Color.Blue)];
                add_log_message(message);
            }
        };
    }

    private void render_debug_tab() {
#if DEBUG
        fixed (int* ap_mult = &ap_multiplier) {
            uint step = 1;
            uint step_fast = 10;
            ImGui.InputScalar("AP multiplier", ImGuiDataType.U32, ap_mult, &step, &step_fast);
        }
#endif

        ImGui.Text($"Current room: {save_data->current_room_id} ({Marshal.PtrToStringAnsi((nint)get_event_name(*(uint*)event_id))!})");
        ImGui.Text($"Current region: {current_region}");
        ImGui.Text($"Current story progress: {save_data->story_progress}");
        if (current_region != RegionEnum.None) {
            foreach (var data in region_states[current_region].savedata) {
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

            foreach (var region in region_states) {
                ImGui.TableNextColumn(); ImGui.Text($"{region.Key}");
                ImGui.TableNextColumn(); ImGui.Text($"{region.Value.story_progress}");
                ImGui.TableNextColumn(); ImGui.Text($"{region.Value.room_id}");
                ImGui.TableNextColumn(); ImGui.Text($"{region.Value.entrance}");
                ImGui.TableNextColumn(); ImGui.Text($"{region.Value.completed_visits}");
            }

            ImGui.EndTable();
        }

        if (Battle.btl->battle_state != 0) {
            ImGui.Text($"Battle Name: {Marshal.PtrToStringAnsi((nint)FhUtil.ptr_at<char>(0xD2C25A))}");
        } else {
#if DEBUG
            fixed (uint* battle_input = &LaunchBattleInput) {
                uint p_step = 1;
                uint p_step_fast = 10;
                ImGui.InputScalar("launchBattleInput", ImGuiDataType.U32, battle_input, &p_step, &p_step_fast, "%x");
            }
            if (ImGui.Button("launchBattleButton")) {
                FhXCall.h_MsBattleLabelExe.fnptr!(LaunchBattleInput, 1, 1);
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
            foreach (var (region, i) in region_is_unlocked.Select((value, i) => (value, i))) {
                ImGui.TableNextColumn();
                Color color = region.Value ? Color.Green : Color.Red;
                ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(color.R / 255f, color.G / 255f, color.B / 255f, 1.0f));
#if !DEBUG
                ImGui.BeginDisabled();
#endif
                bool unlocked = region_is_unlocked[region.Key];
                if (ImGui.Checkbox($"###Archipelago.GUI.Unlocks.{region.Key}", &unlocked)) {
                    region_is_unlocked[region.Key] = unlocked;
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
            foreach (var (character, i) in unlocked_characters.Select((value, i) => (value, i))) {
                ImGui.TableNextColumn();
                Color color = character.Value ? Color.Green : Color.Red;
                ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(color.R / 255f, color.G / 255f, color.B / 255f, 1.0f));
#if !DEBUG
                ImGui.BeginDisabled();
#endif
                bool unlocked = unlocked_characters[character.Key];
                if (ImGui.Checkbox($"###Archipelago.GUI.Unlocks.{id_to_character[character.Key]}", &unlocked)) {
                    unlocked_characters[character.Key] = unlocked;
                }
#if !DEBUG
                ImGui.EndDisabled();
#endif
                ImGui.SameLine();
                Vector2 text_start = ImGui.GetCursorScreenPos();
                ImGui.Text($"{id_to_character[character.Key]}");
                // Strikethrough if locked
                Vector2 text_size = ImGui.CalcTextSize($"{id_to_character[character.Key]}");
                if (locked_characters[character.Key]) ImGui.AddLine(ImGui.GetWindowDrawList(), text_start + new Vector2(0, text_size.Y * 0.75f), text_start + new Vector2(text_size.X, text_size.Y * 0.75f), ImGui.GetColorU32(ImGuiCol.Text));
                ImGui.PopStyleColor();
            }
            ImGui.EndTable();
        }
    }

    private void render_settings() {
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

        ImGui.Checkbox("Show Recent Items", ref RecentItemsModule.show_recent_items);

        if (ImGui.Button("Save settings")) {
            VoiceLanguage = voice_lang != 0xFF ? (FhLangId)voice_lang : null;
            TextLanguage = text_lang != 0xFF ? (FhLangId)text_lang : null;
            _ffx_interop!.save_global_state();
        }

        ImGui.SeparatorText("Save-Local Settings");
        ImGui.Indent();

        if (seed.Options.SeedId is null) {
            ImGui.Text("Please load a save to display these settings.");
            ImGui.Unindent();
            return;
        }

        bool hardcore_contest = _hardcore_dreams_end!.get_enabled();
        ImGui.Checkbox("Enable Hardcore Dream's End", ref hardcore_contest);
        _hardcore_dreams_end!.set_enabled(hardcore_contest);

        bool deathlink = _deathlink!.get_enabled();
        ImGui.Checkbox("Enable Deathlink", ref deathlink);
        _deathlink!.set_enabled(deathlink);

        ImGui.Text($"Deathlinks Queued: {_deathlink!.get_deathlinks_queued()}");

        string deathlink_send_type = _deathlink!.get_send_type();
        if (ImGui.BeginCombo("Deathlink Send Type", deathlink_send_type)) {
            foreach (DeathLinkModule.DeathLinkSendType type in Enum.GetValues<DeathLinkModule.DeathLinkSendType>()) {
                string type_name = _deathlink!.get_send_type_name(type);
                if (ImGui.Selectable(type_name, type_name == deathlink_send_type)) {
                    deathlink_send_type = type_name;
                }
            }

            ImGui.EndCombo();
        }
        _deathlink!.set_send_type(deathlink_send_type);

        string deathlink_receive_type = _deathlink!.get_receive_type();
        if (ImGui.BeginCombo("Deathlink Receive Type", deathlink_receive_type)) {
            foreach (DeathLinkModule.DeathLinkReceiveType type in Enum.GetValues<DeathLinkModule.DeathLinkReceiveType>()) {
                string type_name = _deathlink!.get_receive_type_name(type);
                if (ImGui.Selectable(type_name, type_name == deathlink_receive_type)) {
                    deathlink_receive_type = type_name;
                }
            }

            ImGui.EndCombo();
        }
        _deathlink!.set_receive_type(deathlink_receive_type);

#if DEBUG
        if (ImGui.Button("Receive Debug Deathlink")) {
            _deathlink!.debug_add_queued();
        }

        if (ImGui.Button("Apply Deathlink")) {
            _deathlink!.debug_apply_deathlink();
        }
#endif

        ImGui.Unindent();
    }

    private void render_client() {
        enabled ^= ImGui.IsKeyPressed(archipelago_gui_key);
        if (!enabled) return;
        ImGuiStylePtr style = ImGui.GetStyle();

        if (ImGui.Begin("Archipelago###Archipelago.GUI")) {
            if (shiori_file != null) {
                if (shiori_image == null) {
                    shiori_image = new(shiori_file!.FullName, FhTextureType.PNG);
                }

                FhApi.Resources.load_texture_from_disk(shiori_image);

                if (shiori_image.try_use(out ImTextureRef texture_ref, out _)) {
                    ImGui.GetWindowDrawList().AddImage(texture_ref, ImGui.GetWindowPos(), ImGui.GetWindowPos() + ImGui.GetWindowSize(), 0x55555555);
                }
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
                            string item_name = _ffx_interop!.get_item_name(item_id);
                            ImGui.Text($"{item_name}: {amount}");
                        }
                    }
                    ImGui.SeparatorText("Other");
                    if (other_inventory.Count == 0) {
                        ImGui.Text("Empty");
                    } else {
                        foreach ((uint item_id, int amount) in other_inventory) {
                            string item_name = _ffx_interop!.get_other_item_name(item_id);
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
                if (SphereGrid.lpamng != null && *SphereGrid.is_open && ImGui.BeginTabItem("Sphere Grid###Archipelago.GUI.TabBar.SphereGrid")) {
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
