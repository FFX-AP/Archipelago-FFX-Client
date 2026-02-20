// SPDX-License-Identifier: MIT

using Fahrenheit.Core;
using Fahrenheit.Core.FFX;
using Fahrenheit.Modules.ArchipelagoFFX.GUI;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using TerraFX.Interop.Windows;
using static Fahrenheit.Modules.ArchipelagoFFX.delegates;

namespace Fahrenheit.Modules.ArchipelagoFFX;

public unsafe partial class ArchipelagoFFXModule {

    // Customization
    public static FhMethodHandle<PrepareMenuList> _PrepareMenuList;
    public static FhMethodHandle<UpdateCustomizationMenuState> _UpdateCustomizationMenuState;
    public static FhMethodHandle<DrawCustomizationMenu> _DrawCustomizationMenu;
    public static MsGetRomKaizou _MsGetRomKaizou;
    public static MsGetRomAbility _MsGetRomAbility;

    public static FUN_008c1c70 _FUN_008c1c70;
    public static TODrawMenuPlateXYWHType _TODrawMenuPlateXYWHType;
    public static FUN_008f8bb0 _FUN_008f8bb0;
    public static TODrawScissorXYWH _TODrawScissorXYWH;
    public static FUN_008d5d20 _FUN_008d5d20;
    public static FUN_008c0f40 _FUN_008c0f40;
    public static FUN_008c1350_DrawScissor512x416 _FUN_008c1350_DrawScissor512x416;
    public static FUN_008d5dc0 _FUN_008d5dc0;
    public static FUN_008e6cc0 _FUN_008e6cc0;
    public static FUN_008d6630 _FUN_008d6630;




    // Customization-related
    private ushort[] original_kaizou_costs;
    public void h_PrepareMenuList(MenuListEnum menu_list_id, Equipment* gear) {
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
                    logger.Debug($"Free customization: {ArchipelagoData.other_item_names[i]}");
                    customizations[i].item_cost = 0;
                }
            }
        }

        // TODO: Fix Customizations being skipped if the player has 0 of the required item

        uint[] ability_international_bonuses = new uint[4];
        uint[] ability_group_idxs = new uint[4];
        uint[] ability_group_levels = new uint[4];
        if (menu_list_id == MenuListEnum.GEAR_CUSTOMIZATION) {
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
                            CustomizationStatusEnum status = CustomizationStatusEnum.AVAILABLE;
                            if (num_abilities == 0) {
                                if (item_count < customization.item_cost) {
                                    status = CustomizationStatusEnum.NOT_ENOUGH_ITEMS;
                                }
                            }
                            else {
                                for (int i = 0; i < num_abilities; i++) {
                                    if (ability_group_idxs[i] == a_ability->group_idx) {
                                        if (a_ability->group_level < ability_group_levels[i]) {
                                            // Same group, lower level
                                            status = CustomizationStatusEnum.CONFLICTING;
                                        }
                                        if (a_ability->group_level == ability_group_levels[i]) {
                                            status = a_ability->international_bonus_idx != ability_international_bonuses[i] ? CustomizationStatusEnum.CONFLICTING : CustomizationStatusEnum.ALREADY_APPLIED;
                                        }
                                    }
                                    if (has_ribbon && a_ability->international_bonus_idx == 0xFE) {
                                        status = CustomizationStatusEnum.CONFLICTING;
                                    }
                                }
                                if (status == CustomizationStatusEnum.AVAILABLE && item_count < customization.item_cost) {
                                    status = CustomizationStatusEnum.NOT_ENOUGH_ITEMS;
                                }
                            }
                            if (gear->abilities[gear->slot_count - 1] != 0xff && gear->abilities[gear->slot_count - 1] != 0) {
                                status = CustomizationStatusEnum.NO_SLOTS;
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
            uint* _DAT_0186a210 = FhUtil.ptr_at<uint>(0x146A210);
            *_DAT_0186a210 = added;
            *_DAT_0186a20c = added + skipped;
            if (*_DAT_0186a210 == 0) {
                *_DAT_0186a210 = 1;
            }

        }
        else {
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

    public void h_UpdateCustomizationMenuState(int param_1) {
        uint* state = FhUtil.ptr_at<uint>(0x146AA28);
        uint pre_state = *state;

        _UpdateCustomizationMenuState.orig_fptr(param_1);

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
                    if (count > 0) other_inventory[item_id] = count - 1;
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

    public static ManagedCustomString customization_string = new ManagedCustomString($"Free!");
    public void h_DrawCustomizationMenu(uint param_1) {
        //_DrawCustomizationMenu.orig_fptr(param_1);
        DrawCustomizationMenu_reimplement(param_1);
        return;
    }

    public void DrawCustomizationMenu_reimplement(uint param_1) {
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
            _FUN_008e6cc0(pos_1.X, pos_1.Y, pos_2.X, pos_2.Y, iVar2, uVar1, uVar5);
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
