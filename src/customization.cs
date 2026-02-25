// SPDX-License-Identifier: MIT

using Fahrenheit.Core;
using Fahrenheit.Core.FFX;
using System.Numerics;
using static Fahrenheit.Modules.ArchipelagoFFX.delegates;

namespace Fahrenheit.Modules.ArchipelagoFFX;

public unsafe partial class ArchipelagoFFXModule {

    // Customization
    public static FhMethodHandle<PrepareMenuList> _PrepareMenuList;
    public static FhMethodHandle<UpdateGearCustomizationMenuState> _UpdateGearCustomizationMenuState;
    public static FhMethodHandle<UpdateAeonCustomizationMenuState> _UpdateAeonCustomizationMenuState;
    public static FhMethodHandle<DrawGearCustomizationMenu> _DrawGearCustomizationMenu;
    public static FhMethodHandle<DrawAeonCustomizationMenu> _DrawAeonCustomizationMenu;
    public static MsGetRomKaizou _MsGetRomKaizou;
    public static MsGetRomAbility _MsGetRomAbility;
    public static MsGetRomSummonGrow _MsGetRomSummonGrow;
    public static TkMn2GetSummonGrowMax _TkMn2GetSummonGrowMax;
    public static TkMenuGetCurrentSummon _TkMenuGetCurrentSummon;
    public static MsGetSaveCommand _MsGetSaveCommand;

    public static FUN_008c1c70 _FUN_008c1c70;
    public static TODrawMenuPlateXYWHType _TODrawMenuPlateXYWHType;
    public static FUN_008f8bb0 _FUN_008f8bb0;
    public static TODrawScissorXYWH _TODrawScissorXYWH;
    public static FUN_008d5d20 _FUN_008d5d20;
    public static FUN_008c0f40 _FUN_008c0f40;
    public static FUN_008c1350_DrawScissor512x416 _FUN_008c1350_DrawScissor512x416;
    public static FUN_008d5dc0 _FUN_008d5dc0;
    public static DrawCrossMenuScrollParts _DrawCrossMenuScrollParts;
    public static FUN_008d6630 _FUN_008d6630;

    public static TkVU1SyncPath _TkVU1SyncPath;
    public static FUN_008e71d0 _FUN_008e71d0;
    public static FUN_008ff490 _FUN_008ff490;
    public static FUN_008cd960 _FUN_008cd960;
    public static FUN_008cd9f0 _FUN_008cd9f0;
    public static ToGetCrossExtMesFontWidth _ToGetCrossExtMesFontWidth;
    public static FUN_008bee80 _FUN_008bee80;

    public static TOMkpShapeXYWHUV _TOMkpShapeXYWHUV;
    public static TOMkpCrossExtMesFontLClut _TOMkpCrossExtMesFontLClut;
    public static FUN_008d48e0 _FUN_008d48e0;
    public static FUN_008d4140 _FUN_008d4140;
    public static TkMn2DrawKickSyncPacket _TkMn2DrawKickSyncPacket;





    // Customization-related
    private ushort[] original_kaizou_costs;
    private ushort[] original_sum_grow_costs;
    public void PrepareMenuList_InitList() {
        // Init list
        ushort* _DAT_01597330 = FhUtil.ptr_at<ushort>(0x1197330);
        CustomizationMenuList* menu_list_iter = FhUtil.ptr_at<CustomizationMenuList>(0x1197730);

        for (int i = 0; i < 0x200; i++) {
            menu_list_iter[i].a_ability_id = 0;
            menu_list_iter[i].status = 0;
            menu_list_iter[i].customization_id = 0;
            _DAT_01597330[i] = 0;
        }
        uint* _DAT_0186a20c = FhUtil.ptr_at<uint>(0x146A20C);
        *_DAT_0186a20c = 0;
    }

    public void PrepareMenuList_SetLength(uint added, uint skipped) {
        // Set length
        uint* _DAT_0186a20c = FhUtil.ptr_at<uint>(0x146A20C);
        uint* _DAT_0186a210 = FhUtil.ptr_at<uint>(0x146A210);
        *_DAT_0186a210 = added;
        *_DAT_0186a20c = added + skipped;
        if (*_DAT_0186a210 == 0) {
            *_DAT_0186a210 = 1;
        }
    }
    public void h_PrepareMenuList(MenuListEnum menu_list_id, Equipment* gear) {

        // TODO: Fix Customizations being skipped if the player has 0 of the required item

        uint[] ability_international_bonuses = new uint[4];
        uint[] ability_group_idxs = new uint[4];
        uint[] ability_group_levels = new uint[4];

        if (menu_list_id == MenuListEnum.GEAR_CUSTOMIZATION) {
            // Modify kaizou.bin
            int num_customizations;
            CustomizationRecipe* customizations = _MsGetRomKaizou(&num_customizations);
            if (original_kaizou_costs == null) {
                original_kaizou_costs = new ushort[num_customizations];
                for (int i = 0; i < num_customizations; i++) {
                    original_kaizou_costs[i] = customizations[i].item_cost;
                }
            }
            uint item_id = 0xC000;
            for (int i = 0; i < num_customizations; i++, item_id++) {
                if (other_inventory.TryGetValue(item_id, out int count)) {
                    if (count > 0) {
                        logger.Debug($"Free customization: {get_other_item_name(item_id)}");
                        customizations[i].item_cost = 0;
                    }
                }
            }

            // Init list
            PrepareMenuList_InitList();
            CustomizationMenuList* menu_list_iter = FhUtil.ptr_at<CustomizationMenuList>(0x1197730);
            uint* _DAT_0186a20c = FhUtil.ptr_at<uint>(0x146A20C);


            // Prepare list
            bool has_ribbon = false;
            ability_group_levels[0] = 0xffffffff;
            GearType gear_type = gear->is_weapon ? GearType.WEAPON : GearType.ARMOR;
            ability_group_levels[1] = 0xffffffff;
            ability_group_levels[2] = 0xffffffff;
            ability_group_levels[3] = 0xffffffff;
            ability_group_idxs[0] = 0;
            ability_group_idxs[1] = 0;
            ability_group_idxs[2] = 0;
            ability_group_idxs[3] = 0;

            int num_abilities = 0;

            for (int i = 0; i < 4; i++) {
                ushort ability_id = gear->abilities[i];
                if (ability_id != 0 && ability_id != 0xFF) {
                    int a_ability_id;
                    AutoAbility* a_ability = _MsGetRomAbility(ability_id, &a_ability_id);

                    ability_group_idxs[num_abilities] = (uint)a_ability->group_idx;
                    ability_group_levels[num_abilities] = (uint)a_ability->group_level;
                    ability_international_bonuses[num_abilities] = (uint)a_ability->international_bonus_idx;
                    if (a_ability->international_bonus_idx == 0xff) {
                        has_ribbon = true;
                    }
                    num_abilities++;
                }
            }

            uint added = 0;
            uint skipped = 0;
            if (0 < num_customizations) {
                for (byte customization_id = 0; customization_id < num_customizations; customization_id++) {
                    CustomizationRecipe customization = customizations[customization_id];

                    if (customization.target_gear_type.HasFlag(gear_type)) {
                        ushort a_ability_id = customization.auto_ability;
                        int local_68;
                        AutoAbility* a_ability = _MsGetRomAbility(a_ability_id, &local_68);
                        uint item_count = Globals.save_data->get_item_count(customization.item);

                        if (item_count == 0 && customization.item_cost != 0) {
                            skipped++;
                            continue;
                        }
                        else {
                            CustomizationStatusEnum status = CustomizationStatusEnum.GEAR_AVAILABLE;
                            if (num_abilities == 0) {
                                if (item_count < customization.item_cost) {
                                    status = CustomizationStatusEnum.GEAR_NOT_ENOUGH_ITEMS;
                                }
                            }
                            else {
                                for (int i = 0; i < num_abilities; i++) {
                                    if (ability_group_idxs[i] == a_ability->group_idx) {
                                        if (a_ability->group_level < ability_group_levels[i]) {
                                            // Same group, lower level
                                            status = CustomizationStatusEnum.GEAR_CONFLICTING;
                                        }
                                        if (a_ability->group_level == ability_group_levels[i]) {
                                            status = a_ability->international_bonus_idx != ability_international_bonuses[i] ? CustomizationStatusEnum.GEAR_CONFLICTING : CustomizationStatusEnum.GEAR_ALREADY_APPLIED;
                                        }
                                    }
                                    if (has_ribbon && a_ability->international_bonus_idx == 0xFE) {
                                        status = CustomizationStatusEnum.GEAR_CONFLICTING;
                                    }
                                }
                                if (status == CustomizationStatusEnum.GEAR_AVAILABLE && item_count < customization.item_cost) {
                                    status = CustomizationStatusEnum.GEAR_NOT_ENOUGH_ITEMS;
                                }
                            }
                            if (gear->abilities[gear->slot_count - 1] != 0xff && gear->abilities[gear->slot_count - 1] != 0) {
                                status = CustomizationStatusEnum.GEAR_NO_SLOTS;
                            }
                            menu_list_iter->status = status;
                            menu_list_iter->a_ability_id = a_ability_id;
                            menu_list_iter->customization_id = customization_id;
                            menu_list_iter++;
                            added++;
                        }
                    }
                }
                for (int i = 0; i < skipped; i++) {
                    // ????
                    menu_list_iter->status = (CustomizationStatusEnum)0x11;
                    menu_list_iter->a_ability_id = 0;
                    menu_list_iter->customization_id = 0xFF;
                    menu_list_iter++;
                }
            }


            // Set length
            PrepareMenuList_SetLength(added, skipped);
        }
        else if (menu_list_id == MenuListEnum.AEON_ABILITIES) {
            // TODO: Modify sum_grow.bin
            int num_customizations;
            CustomizationRecipe* customizations = _MsGetRomSummonGrow(&num_customizations);
            num_customizations = _TkMn2GetSummonGrowMax();
            if (original_sum_grow_costs == null) {
                original_sum_grow_costs = new ushort[num_customizations];
                for (int i = 0; i < num_customizations; i++) {
                    original_sum_grow_costs[i] = customizations[i].item_cost;
                }
            }
            uint item_id = 0xC07D;
            for (int i = 0; i < num_customizations; i++, item_id++) {
                if (other_inventory.TryGetValue(item_id, out int count)) {
                    if (count > 0) {
                        logger.Debug($"Free customization: {get_other_item_name(item_id)}");
                        customizations[i].item_cost = 0;
                    }
                }
            }

            // Init list
            PrepareMenuList_InitList();
            CustomizationMenuList* menu_list_iter = FhUtil.ptr_at<CustomizationMenuList>(0x1197730);
            uint* _DAT_0186a20c = FhUtil.ptr_at<uint>(0x146A20C);

            byte current_summon = _TkMenuGetCurrentSummon();
            bool has_key_item = Globals.save_data->key_items.get(0xa022);

            uint added = 0;
            if (0 < num_customizations) {
                for (byte customization_id = 0; customization_id < num_customizations; customization_id++) {
                    CustomizationRecipe customization = customizations[customization_id];
                    ushort auto_ability_id = customization.auto_ability;
                    menu_list_iter->customization_id = 0xff;
                    bool has_command = _MsGetSaveCommand(current_summon, auto_ability_id);

                    if (!has_command) {
                        uint item_count = Globals.save_data->get_item_count(customization.item);
                        if (item_count == 0 && customization.item_cost != 0) {
                            continue;
                        } else {
                            menu_list_iter->a_ability_id = auto_ability_id;
                            menu_list_iter->customization_id = customization_id;
                            if (item_count < customization.item_cost) {
                                menu_list_iter->status = CustomizationStatusEnum.AEON_NOT_ENOUGH_ITEMS;
                            } else if (((int)customization.target_gear_type & (1 << (current_summon - 8))) == 0 && !has_key_item) {
                                // Never reached because both conditions are always false: Bit is set for all Aeons (always 0x7F) and key item is unused.
                                menu_list_iter->status = CustomizationStatusEnum.AEON_CANNOT_LEARN_WITHOUT_KEY;
                            } else {
                                menu_list_iter->status = CustomizationStatusEnum.AEON_AVAILABLE;
                            }
                        }
                    } else {
                        menu_list_iter->a_ability_id = auto_ability_id;
                        menu_list_iter->customization_id = customization_id;
                        menu_list_iter->status = CustomizationStatusEnum.AEON_ALREADY_LEARNED;
                    }
                    menu_list_iter++;
                    added++;
                }
            }

            // Set length
            PrepareMenuList_SetLength(added, 0);

        } else {
            _PrepareMenuList.orig_fptr(menu_list_id, gear);
        }


        //if (menu_list_id == MenuListEnum.GEAR_CUSTOMIZATION) {
        //    CustomizationMenuList* menu_list = FhUtil.ptr_at<CustomizationMenuList>(0x1197730);
        //    int i = 0;
        //    while (menu_list->status != CustomizationStatusEnum.NONE) {
        //        //logger.Debug($"{i}: status={menu_list->status}, customization=\"{customization_names[menu_list->customization_id]}\" ({menu_list->customization_id}), a_ability={menu_list->a_ability_id}, cost={customizations[menu_list->customization_id].item_cost}");
        //        menu_list++; i++;
        //    }
        //}
    }

    public void h_UpdateGearCustomizationMenuState(int param_1) {
        uint* state = FhUtil.ptr_at<uint>(0x146AA28);
        uint pre_state = *state;

        _UpdateGearCustomizationMenuState.orig_fptr(param_1);

        if (*state != pre_state) {
            //logger.Debug($"{pre_state} -> {*state}");

            if (pre_state == 0xc && *state == 0xa) {
                // Applied customization

                uint DAT_0186a9f4 = FhUtil.get_at<uint>(0x0146A9F4);
                short selected_idx = *(short*)(DAT_0186a9f4 + 0x48);
                CustomizationMenuList* menu_list = FhUtil.ptr_at<CustomizationMenuList>(0x1197730);
                int num_customizations;
                CustomizationRecipe* customizations = _MsGetRomKaizou(&num_customizations);
                byte customization_id = menu_list[selected_idx].customization_id;
                if (customizations[customization_id].item_cost != original_kaizou_costs[customization_id]) {
                    uint item_id = (uint)(0xC000 | customization_id);
                    other_inventory.TryGetValue(item_id, out int count);
                    if (--count <= 0) {
                        other_inventory.Remove(item_id);
                        customizations[customization_id].item_cost = original_kaizou_costs[customization_id];
                    }
                    else other_inventory[item_id] = count;
                }
            }
        }

        if (pre_state == 1) {
            // Reset kaizou.bin
            if (original_kaizou_costs != null) {
                int num_customizations;
                CustomizationRecipe* customizations = _MsGetRomKaizou(&num_customizations);
                for (int i = 0; i < num_customizations; i++) {
                    customizations[i].item_cost = original_kaizou_costs[i];
                }
            }
        }
    }

    public void h_UpdateAeonCustomizationMenuState(int param_1) {
        uint* state = (uint *)(param_1 + 0x1c);
        uint pre_state = *state;


        _UpdateAeonCustomizationMenuState.orig_fptr(param_1);

        if (*state != pre_state) {
            logger.Debug($"{pre_state} -> {*state}");

            if (pre_state == 0x15) {
                uint DAT_0186a568 = FhUtil.get_at<uint>(0x0146a568);
                short selected_idx = *(short*)(DAT_0186a568 + 0x48);
                CustomizationMenuList* menu_list = FhUtil.ptr_at<CustomizationMenuList>(0x1197730);
                byte customization_id = menu_list[selected_idx].customization_id;
                int num_customizations;
                CustomizationRecipe* customizations = _MsGetRomSummonGrow(&num_customizations);
                logger.Debug($"Applied customization {get_other_item_name((uint)(0xC07D + customization_id))}");
                if (customizations[customization_id].item_cost != original_sum_grow_costs[customization_id]) {
                    uint item_id = (uint)(0xC07D + customization_id);
                    other_inventory.TryGetValue(item_id, out int count);
                    if (--count <= 0) {
                        other_inventory.Remove(item_id);
                        customizations[customization_id].item_cost = original_sum_grow_costs[customization_id];
                    }
                    else other_inventory[item_id] = count;

                }
            }

            // 1D -> 1E Applied attribute customization
        }
    }

    public static ManagedCustomString customization_string = new ManagedCustomString($"Free!");
    public void h_DrawGearCustomizationMenu(uint param_1) {
        //_DrawGearCustomizationMenu.orig_fptr(param_1);
        DrawGearCustomizationMenu_reimplement(param_1);
        return;
    }

    public void h_DrawAeonCustomizationMenu(uint param_1) {
        //_DrawAeonCustomizationMenu.orig_fptr(param_1);
        DrawAeonCustomizationMenu_reimplement(param_1);
    }

    public void DrawAeonCustomizationMenu_reimplement(uint param_1) {
        _TkVU1SyncPath();
        _FUN_008e71d0(7);
        uint current_summon = _TkMenuGetCurrentSummon();


        CustomizationMenuList* menu_list = FhUtil.ptr_at<CustomizationMenuList>(0x1197730);
        short selected_idx = *(short*)(param_1 + 0x48);
        Vector2 pos_1;
        Vector2 pos_2;

        uint item_id;
        if (menu_list[selected_idx].status == 0) {
            item_id = 0xFFFFFFF;
        } else {
            byte customization_id = menu_list[selected_idx].customization_id;
            int num_customizations;
            CustomizationRecipe* customizations = _MsGetRomSummonGrow(&num_customizations);
            item_id = customizations[customization_id].item;
            int item_cost = customizations[customization_id].item_cost;


            // Draw cost section
            if (item_cost == 0) {
                fixed (byte* text = customization_string.encoded) {
                    Vector2 pos = new Vector2(1260f, 381f).game_remap_1080p();
                    _ToMakeBtlEasyFont(text, pos.X, pos.Y, 0, 2f);
                }
            }
            else {
                pos_1 = new Vector2(970f, 380f).game_remap_1080p();
                _FUN_008c1c70((int)pos_1.X, (int)pos_1.Y, item_id, item_cost);
            }

        }

        pos_1 = new Vector2(210f, 226f).game_remap_1080p();
        _FUN_008ff490(current_summon, pos_1.X, pos_1.Y);

        pos_1 = new Vector2(210f, 312f).game_remap_1080p();
        pos_2 = new Vector2(740f,  48f).game_remap_1080p();
        _TODrawMenuPlateXYWHType(pos_1.X, pos_1.Y, pos_2.X, pos_2.Y, 2);

        // Header text
        pos_1 = new Vector2(365f, 320f).game_remap_1080p();
        pos_2 = new Vector2(430f,  36f).game_remap_1080p();
        _FUN_008f8bb0(0xf, pos_1.X, pos_1.Y, pos_2.X, pos_2.Y);

        if (-1 < (int)item_id) {
            pos_1 = new Vector2(970f, 312f).game_remap_1080p();
            pos_2 = new Vector2(740f, 48f).game_remap_1080p();
            _TODrawMenuPlateXYWHType(pos_1.X, pos_1.Y, pos_2.X, pos_2.Y, 2);

            // Header text ("Item cost")?
            pos_1 = new Vector2(1125f, 318f).game_remap_1080p();
            pos_2 = new Vector2( 430f,  36f).game_remap_1080p();
            _FUN_008f8bb0(0x10, pos_1.X, pos_1.Y, pos_2.X, pos_2.Y);
        }

        int iVar2 = (int)new Vector2(0, 365f).game_remap_1080p().Y;
        int iVar6, sVar1;
        float fVar10;
        if (*(short*)(param_1 + 0x32) == *(short*)(param_1 + 0x34)) {
            // Draw ability list
            item_id = (uint)*(short*)(param_1 + 0x46);
            _FUN_008c0f40(iVar2, (int)(new Vector2(0, 70f).game_remap_1080p().Y * 9.0), 0, (int)item_id);
            sVar1 = *(short*)(param_1 + 0x32);
            fVar10 = 0.0f;
            iVar6 = 0;
        }
        else {
            // Draw ability list when quick scrolling (L2/R2)
            int param_1_0x46 = (int)*(short*)(param_1 + 0x46);
            item_id = (uint)(new Vector2(0, 70f).game_remap_1080p().Y * 9.0);
            _FUN_008c0f40(iVar2, (int)item_id, 1, param_1_0x46);
            FUN_008cd960_Extra(param_1, 1, (int)*(short*)(param_1 + 0x32), 0.0f, 0.0f);

            _FUN_008c0f40(iVar2, (int)item_id, 2, param_1_0x46);

            fVar10 = (int)(new Vector2(0, 70f).game_remap_1080p().Y * 9.0 * (float)param_1_0x46 * -0.00024414063);
            sVar1 = (int)*(short*)(param_1 + 0x34);
            iVar6 = 2;
        }
        FUN_008cd960_Extra(param_1, iVar6, sVar1, 0.0f, fVar10);
        _FUN_008c1350_DrawScissor512x416();

        ushort _DAT_0186a5a4 = FhUtil.get_at<ushort>(0x0146a5a4);
        ushort _DAT_0186a5a6 = FhUtil.get_at<ushort>(0x0146a5a6);
        _FUN_008cd9f0(param_1, _DAT_0186a5a4 + 0xc, _DAT_0186a5a6 + 1);

        float local_8;
        _ToGetCrossExtMesFontWidth(0, _FUN_008bee80(5), &local_8, 0.78f, 1.0f);

        fVar10 = (float)(double)(new Vector2(80f, 0).game_remap_1080p().X + local_8);
        local_8 = fVar10;



        float fVar11 = new Vector2(660f, 0).game_remap_1080p().X;
        if (fVar11 < (float)fVar10 == (float.IsNaN(fVar11) || float.IsNaN(fVar10))) {
            fVar10 = new Vector2(740f, 0).game_remap_1080p().X;
        }
        else {
            fVar10 = (float)(new Vector2(80f, 0).game_remap_1080p().X + local_8);
        }

        // graphicUiRemapX2(1806.0);
        fVar11 = new Vector2(1806f, 0).game_remap_1080p().X - fVar10;
        float fVar12 = (float)((fVar10 - local_8) * 0.5 + fVar11);

        pos_1 = new Vector2(955f, 370f).game_remap_1080p();
        pos_2 = new Vector2(  8f, 630f).game_remap_1080p();
        _DrawCrossMenuScrollParts(pos_1.X, pos_1.Y, pos_2.X, pos_2.Y, *(short*)(param_1 + 0x32), *(ushort*)(param_1 + 0x3a), *(ushort*)(param_1 + 0x30));

        _TODrawMenuPlateXYWHType(fVar11, new Vector2(0, 925f).game_remap_1080p().Y, fVar10, new Vector2(0, 48f).game_remap_1080p().Y, 2);

        float uv_y2 = 0.3017578f;
        float uv_x2 = 0.99902344f;
        float uv_y1 = 0.22851563f;
        float uv_x1 = 0.9267578f;
        pos_1 = new Vector2( 0f, 920f).game_remap_1080p();
        pos_2 = new Vector2(64f,  64f).game_remap_1080p();
        _TOMkpShapeXYWHUV(0xf3, fVar12, pos_1.Y, pos_2.X, pos_2.Y, uv_x1, uv_y1, uv_x2, uv_y2);

        pos_1 = new Vector2(80f, 928f).game_remap_1080p();
        _TOMkpCrossExtMesFontLClut(0, _FUN_008bee80(5), pos_1.X + fVar12, pos_1.Y, 0, 0.78f, 1.0f);

        item_id = _FUN_008d48e0();
        _FUN_008d4140(item_id, 1);
        _TkMn2DrawKickSyncPacket();
    }

    private void FUN_008cd960_Extra(uint param_1, int param_2, int menu_offset, float x, float y) {
        _FUN_008cd960(param_1, param_2, menu_offset, x, y);

        Vector2 pos = new(x, y);
        pos += new Vector2(209f + 50f, 306f).game_remap_1080p();

        if (param_2 == 0) {
            pos.Y -= (float)(new Vector2(0, 70f).game_remap_1080p().Y * *(short*)(param_1 + 0x46) * 0.00024414063); // Scroll offset
        }

        CustomizationMenuList* menu_list = FhUtil.ptr_at<CustomizationMenuList>(0x1197730);
        ushort menu_length = *(ushort*)(param_1 + 0x30);
        for (int i = -1; i < 10; i++) {
            int curr_index = menu_offset + i;
            if (0 <= curr_index && curr_index < menu_length) {
                if (menu_list[curr_index].customization_id != 0xFF) {
                    int num_customizations;
                    CustomizationRecipe* customizations = _MsGetRomSummonGrow(&num_customizations);

                    uint item_id   = customizations[menu_list[curr_index].customization_id].item;
                    int item_cost = customizations[menu_list[curr_index].customization_id].item_cost;
                    if (item_cost == 0) {
                        fixed (byte* text = customization_string.encoded) {
                            _ToMakeBtlEasyFont(text, pos.X, pos.Y, 0, 0.78f);
                        }
                    }
                }
            }
            pos += new Vector2(0, 70f).game_remap_1080p();
        }
    }

    public void DrawGearCustomizationMenu_reimplement(uint param_1) {
        CustomizationMenuList* menu_list = FhUtil.ptr_at<CustomizationMenuList>(0x1197730);
        short selected_idx = *(short*)(param_1 + 0x48);
        Vector2 pos_1;
        Vector2 pos_2;
        if (menu_list[selected_idx].customization_id != 0xFF) {
            int num_customizations;
            CustomizationRecipe* customizations = _MsGetRomKaizou(&num_customizations);

            uint item_id   = customizations[menu_list[selected_idx].customization_id].item;
            int item_cost = customizations[menu_list[selected_idx].customization_id].item_cost;

            // Draw cost section
            if (item_cost == 0) {
                fixed (byte* text = customization_string.encoded) {
                    Vector2 pos = new Vector2(1260f, 320f).game_remap_1080p();
                    _ToMakeBtlEasyFont(text, pos.X, pos.Y, 0, 2f);
                }
            } else {
                pos_1 = new Vector2(970f, 319f).game_remap_1080p();
                _FUN_008c1c70((int)pos_1.X, (int)pos_1.Y, item_id, item_cost);
            }

            // Header background
            pos_1 = new Vector2(970f, 252f).game_remap_1080p();
            pos_2 = new Vector2(740f, 60f).game_remap_1080p();
            _TODrawMenuPlateXYWHType(pos_1.X, pos_1.Y, pos_2.X, pos_2.Y, 2);

            // Header ("Item cost")
            pos_1 = new Vector2(1125f, 264f).game_remap_1080p();
            pos_2 = new Vector2(430f, 36f).game_remap_1080p();
            _FUN_008f8bb0(0x10, pos_1.X, pos_1.Y, pos_2.X, pos_2.Y);
        }
        //_TkMenuGetCurrentPlayer(); // Result is unused?

        // Abilities Header background
        pos_1 = new Vector2(211f, 252f).game_remap_1080p();
        pos_2 = new Vector2(740f, 60f).game_remap_1080p();
        _TODrawMenuPlateXYWHType(pos_1.X, pos_1.Y, pos_2.X, pos_2.Y, 2);

        // Header ("Abilities")
        pos_1 = new Vector2(366f, 264f).game_remap_1080p();
        pos_2 = new Vector2(430f, 36f).game_remap_1080p();
        _FUN_008f8bb0(7, pos_1.X, pos_1.Y, pos_2.X, pos_2.Y);

        if (*(short*)(param_1 + 0x32) == *(short*)(param_1 + 0x34)) {

            // Draw ability list
            _TODrawScissorXYWH(0, (int)(new Vector2(0, 315f).game_remap_1080p().Y), 0x200, (int)(new Vector2(0, 680f).game_remap_1080p().Y));
            FUN_008d5d20_Extra((int)param_1, 0, *(short*)(param_1 + 0x32), 0, 0);

        } else {
            // Draw ability list when quick scrolling (L2/R2)
            short uVar5 = *(short*)(param_1 + 0x46); // Scroll offset

            _FUN_008c0f40((int)(new Vector2(0, 315f).game_remap_1080p().Y), (int)(new Vector2(0, 680f).game_remap_1080p().Y), 1, uVar5);
            FUN_008d5d20_Extra((int)param_1, 1, *(short*)(param_1 + 0x32), 0, 0);

            _FUN_008c0f40((int)(new Vector2(0, 315f).game_remap_1080p().Y), (int)(new Vector2(0, 680f).game_remap_1080p().Y), 2, uVar5);

            int iVar2 = (int)(new Vector2(0, 675f).game_remap_1080p().Y * uVar5 * -0.00024414063);
            FUN_008d5d20_Extra((int)param_1, 2, *(short*)(param_1 + 0x34), 0, iVar2);
        }
        _FUN_008c1350_DrawScissor512x416();

        pos_1 = new Vector2(389f, 325f).game_remap_1080p();
        _FUN_008d5dc0((int)param_1, (int)pos_1.X, (int)pos_1.Y);

        {
            int uVar5 = *(ushort*)(param_1 + 0x30);
            int uVar1 = *(ushort*)(param_1 + 0x3a);
            int iVar2 = *(short*)(param_1 + 0x32);
            pos_1 = new Vector2(955f, 319f).game_remap_1080p();
            pos_2 = new Vector2(8f, 675f).game_remap_1080p();
            _DrawCrossMenuScrollParts(pos_1.X, pos_1.Y, pos_2.X, pos_2.Y, iVar2, uVar1, uVar5);
        }

        {
            // Draw gear name
            uint _DAT_0186a9f0 = FhUtil.get_at<uint>(0x146A9F0);
            int iVar2 = *(short*)(_DAT_0186a9f0 + 0x48);
            pos_1 = new Vector2(970f, 659f).game_remap_1080p();
            _FUN_008d6630((int)pos_1.X, (int)pos_1.Y, iVar2);
        }
        return;
    }


    private void FUN_008d5d20_Extra(int param_1, int param_2, int menu_offset, int x, int y) {
        _FUN_008d5d20(param_1, param_2, menu_offset, x, y);

        Vector2 pos = new(x, y);
        pos += new Vector2(209f + 50f, 255f).game_remap_1080p();

        if (param_2 == 0) {
            pos.Y -= (float)(new Vector2(0, 75f).game_remap_1080p().Y * *(short*)(param_1 + 0x46) * 0.00024414063); // Scroll offset
        }

        CustomizationMenuList* menu_list = FhUtil.ptr_at<CustomizationMenuList>(0x1197730);
        ushort menu_length = *(ushort*)(param_1 + 0x30);
        for (int i = -1; i < 10; i++) {
            int curr_index = menu_offset + i;
            if (0 <= curr_index && curr_index < menu_length) {
                if (menu_list[curr_index].customization_id != 0xFF) {
                    int num_customizations;
                    CustomizationRecipe* customizations = _MsGetRomKaizou(&num_customizations);

                    uint item_id   = customizations[menu_list[curr_index].customization_id].item;
                    int item_cost = customizations[menu_list[curr_index].customization_id].item_cost;
                    if (item_cost == 0) {
                        fixed (byte* text = customization_string.encoded) {
                            _ToMakeBtlEasyFont(text, pos.X, pos.Y, 0, 0.78f);
                        }
                    }
                }
            }
            pos += new Vector2(0, 75f).game_remap_1080p();
        }
    }
}
