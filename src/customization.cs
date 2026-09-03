// SPDX-License-Identifier: MIT

using Fahrenheit;
using Fahrenheit.FFX;
using Fahrenheit.FFX.Ids;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using FhXCall = Fahrenheit.FFX.FhCall;

namespace ArchipelagoFFX;

public unsafe partial class ArchipelagoFFXModule {
    public enum CustomizationStatusEnum : byte {
        NONE                          = 0x0,
        AEON_AVAILABLE                = 0x4,
        AEON_ALREADY_LEARNED          = 0x5,
        AEON_CANNOT_LEARN_WITHOUT_KEY = 0x6,
        AEON_NOT_ENOUGH_ITEMS         = 0x7,

        GEAR_AVAILABLE        = 0xb,
        GEAR_ALREADY_APPLIED  = 0xc,
        GEAR_NOT_ENOUGH_ITEMS = 0xe,
        GEAR_CONFLICTING      = 0xf, // (same group but lower level) or (same group, same level, different international bonus) or (international bonus is 0xfe AND gear has any ability with 0xff international bonus)
        GEAR_NO_SLOTS         = 0x10,
        //NONE             = 0x11
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct CustomizationMenuList {
        public ushort                  a_ability_id;
        public CustomizationStatusEnum status;
        public byte                    customization_id;
    }

    // Customization-related
    private static int selected_gear_slot = 0;
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

    public void PrepareMenuList(TkMenuItemListId menu_list_id, Equipment* gear) {
        //MenuList menu_list_id = (MenuList)menu_list_id_raw;
        _logger.Info($"{menu_list_id}");

        uint[] ability_international_bonuses = new uint[4];
        uint[] ability_group_idxs = new uint[4];
        uint[] ability_group_levels = new uint[4];

        if (menu_list_id == TkMenuItemListId.GEAR_CUSTOMIZATION) {
            // Modify kaizou.bin
            int num_customizations;
            CustomizationRecipe* customizations = FhXCall.MsGetRomKaizou.fnptr!(&num_customizations);
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
                        _logger.Debug($"Free customization: {get_other_item_name(item_id)}");
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
                //if (selected_gear_slot == i) continue; // Skip selected slot
                ushort ability_id = gear->abilities[i];
                if (ability_id != 0 && ability_id != 0xFF) {
                    int a_ability_id;
                    AutoAbility* a_ability = FhXCall.MsGetRomAbility.fnptr!(ability_id, &a_ability_id);

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
                        AutoAbility* a_ability = FhXCall.MsGetRomAbility.fnptr!(a_ability_id, &local_68);
                        uint item_count = Globals.save_data->get_item_count(customization.item);

                        if (item_count == 0 && customization.item_cost != 0) {
                            skipped++;
                            continue;
                        } else {
                            CustomizationStatusEnum status = CustomizationStatusEnum.GEAR_AVAILABLE;
                            if (num_abilities == 0) {
                                if (item_count < customization.item_cost) {
                                    status = CustomizationStatusEnum.GEAR_NOT_ENOUGH_ITEMS;
                                }
                            } else {
                                for (int i = 0; i < num_abilities; i++) {
                                    if (ability_group_idxs[i] == a_ability->group_idx) {
                                        if (a_ability->group_level < ability_group_levels[i]) {
                                            // Same group, lower level
                                            status = CustomizationStatusEnum.GEAR_CONFLICTING;
                                        }

                                        //if (a_ability->group_level == ability_group_levels[i]) {
                                        //    status = a_ability->international_bonus_idx != ability_international_bonuses[i] ? CustomizationStatusEnum.GEAR_CONFLICTING : CustomizationStatusEnum.GEAR_ALREADY_APPLIED;
                                        //}
                                        if (a_ability->group_level == ability_group_levels[i]) {
                                            if (a_ability->international_bonus_idx == ability_international_bonuses[i]) {
                                                status = CustomizationStatusEnum.GEAR_ALREADY_APPLIED;
                                            } else if (selected_gear_slot != i) {
                                                status = CustomizationStatusEnum.GEAR_CONFLICTING;
                                            }
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

                            //if (gear->abilities[gear->slot_count - 1] != 0xff && gear->abilities[gear->slot_count - 1] != 0) {
                            //    status = CustomizationStatusEnum.GEAR_NO_SLOTS;
                            //}
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
        } else if (menu_list_id == TkMenuItemListId.AEON_ABILITIES) {
            // TODO: Modify sum_grow.bin
            int num_customizations;
            AeonAbilityRecipe* customizations = FhXCall.MsGetRomSummonGrow.fnptr!(&num_customizations);
            num_customizations = FhXCall.TkMn2GetSummonGrowMax.fnptr!();
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
                        _logger.Debug($"Free customization: {get_other_item_name(item_id)}");
                        customizations[i].item_cost = 0;
                    }
                }
            }

            // Init list
            PrepareMenuList_InitList();
            CustomizationMenuList* menu_list_iter = FhUtil.ptr_at<CustomizationMenuList>(0x1197730);
            uint* _DAT_0186a20c = FhUtil.ptr_at<uint>(0x146A20C);

            byte current_summon = FhXCall.TkMenuGetCurrentSummon.fnptr!();
            bool has_key_item = Globals.save_data->key_items.get(0xa022);

            uint added = 0;
            if (0 < num_customizations) {
                for (byte customization_id = 0; customization_id < num_customizations; customization_id++) {
                    AeonAbilityRecipe customization = customizations[customization_id];
                    short auto_ability_id = customization.command;
                    menu_list_iter->customization_id = 0xff;
                    bool has_command = FhXCall.MsGetSaveCommand.fnptr!(current_summon, (uint)auto_ability_id) != 0;

                    if (!has_command) {
                        uint item_count = Globals.save_data->get_item_count(customization.item);
                        if (item_count == 0 && customization.item_cost != 0) {
                            continue;
                        } else {
                            menu_list_iter->a_ability_id = (ushort)auto_ability_id;
                            menu_list_iter->customization_id = customization_id;
                            if (item_count < customization.item_cost) {
                                menu_list_iter->status = CustomizationStatusEnum.AEON_NOT_ENOUGH_ITEMS;
                            } else if ((0x7F & (1 << (current_summon - 8))) == 0 && !has_key_item) {
                                // Never reached because both conditions are always false: Bit is set for all Aeons (always 0x7F) and key item is unused.
                                menu_list_iter->status = CustomizationStatusEnum.AEON_CANNOT_LEARN_WITHOUT_KEY;
                            } else {
                                menu_list_iter->status = CustomizationStatusEnum.AEON_AVAILABLE;
                            }
                        }
                    } else {
                        menu_list_iter->a_ability_id = (ushort)auto_ability_id;
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
            FhXCall.FUN_008c2370.chain_from(PrepareMenuList).fnptr!(menu_list_id, gear);
        }


        //if (menu_list_id == MenuList.GEAR_CUSTOMIZATION) {
        //    CustomizationMenuList* menu_list = FhUtil.ptr_at<CustomizationMenuList>(0x1197730);
        //    int i = 0;
        //    while (menu_list->status != CustomizationStatusEnum.NONE) {
        //        //logger.Debug($"{i}: status={menu_list->status}, customization=\"{customization_names[menu_list->customization_id]}\" ({menu_list->customization_id}), a_ability={menu_list->a_ability_id}, cost={customizations[menu_list->customization_id].item_cost}");
        //        menu_list++; i++;
        //    }
        //}
    }

    /// <summary>
    ///     Known states:
    ///         2: Get selected weapon. Goes to 3 if it doesn't exist, otherwise goes to 5
    ///         3: Goes to 4 and returns
    ///         4: Goes to 1
    ///         5: Calls TMn2SetStatusBGDiff, then prepares ability menu list and goes to 7
    ///         6: Calls TkMenuRestartSelFileWindow, then ??? and goes to 7
    /// </summary>
    /// <param name="window"></param>
    public void UpdateGearCustomizationMenuState(TkWindow* window) {
        uint* state = FhUtil.ptr_at<uint>(0x146AA28);
        uint pre_state = *state;

        byte* gear_name;
        Equipment* gear;
        byte* p_DAT_0186a9f8 = (byte*)FhUtil.get_at<uint>(0x146A9F8);

        bool break_loop = false;
        while (!break_loop) {
            switch (*state) {
                case 2:
                    FhXCall.FUN_008b4460.fnptr!(window);
                    if (window->exit_value < 1) {
                        return;
                    }

                    ushort weapon_index = *(ushort*)(p_DAT_0186a9f8 + window->selected_index * 2);
                    gear = FhXCall.MsGetSaveWeapon.fnptr!(weapon_index, 0);
                    bool can_customize = false;
                    if (gear->exists && !gear->is_hidden && gear->slot_count > 0) {
                        if (!gear->is_celestial && !gear->is_brotherhood) {
                            //if (gear->abilities[gear->slot_count-1] == 0xff || gear->abilities[gear->slot_count - 1] == 0)
                            can_customize = true;
                        }
                    }

                    if (!can_customize) {
                        goto default;
                    } else {
                        FhXCall.SndSepPlaySimple.fnptr!(0x80000001);
                        {
                            int slot = 0;
                            for (; slot < gear->slot_count; slot++) {
                                if (gear->abilities[slot] is 0 or 0xff) {
                                    break;
                                }
                            }

                            if (slot < 4 && slot < gear->slot_count) {
                                selected_gear_slot = slot;
                                *state = 0x5;
                            } else {
                                CreateMyWindow(gear);
                                *state = 0xe;
                            }
                        }
                    }

                    break;

                case 6: {
                    TkWindow* _GearSelectionWindow = (TkWindow*)FhUtil.get_at<uint>(0x146A9F0); // DAT_0186a9f0
                    gear = FhXCall.MsGetSaveWeapon.fnptr!(
                        *(ushort*)(p_DAT_0186a9f8 + _GearSelectionWindow->selected_index * 2),
                        (nint)(&gear_name)
                    );
                    int slot = 0;
                    for (; slot < gear->slot_count; slot++) {
                        if (gear->abilities[slot] is 0 or 0xff) {
                            break;
                        }
                    }

                    if (slot < 4 && slot < gear->slot_count) {
                        selected_gear_slot = slot;
                        goto default;
                    } else {
                        *state = 8;
                    }
                }
                    break;

                case 8:
                    if (MyWindow != null) {
                        MyWindow->should_destroy = true;
                        MyWindow = null;
                    }

                    goto default;

                case 0xc: {
                    TkWindow* DAT_023cc120 = (TkWindow*)FhUtil.get_at<uint>(0x1FCC120);
                    if (DAT_023cc120->exit_value < 0) {
                        FhXCall.FUN_008e2de0.fnptr!();
                        *state = 6;
                        break_loop = true;
                        break;
                    }

                    if (DAT_023cc120->exit_value == 0) {
                        break_loop = true;
                        break;
                    }

                    FhXCall.FUN_008e2de0.fnptr!();
                    if (DAT_023cc120->selected_index != 0) {
                        FhXCall.SndSepPlaySimple.fnptr!(0x80000001);
                        *state = 6;
                        break_loop = true;
                        break;
                    }
                }
                    *state = 0xd;
                    break;

                case 0xd:
                    TkWindow* AbilitySelectionWindow = (TkWindow*)FhUtil.get_at<uint>(0x146A9F4); // PTR_0186a9f4
                    TkWindow* GearSelectionWindow    = (TkWindow*)FhUtil.get_at<uint>(0x146A9F0); // DAT_0186a9f0
                    CustomizationMenuList* menu_list = FhUtil.ptr_at<CustomizationMenuList>(0x1197730);


                    short selected_ability = AbilitySelectionWindow->selected_index;
                    int num_customizations;
                    CustomizationRecipe* customizations = FhXCall.MsGetRomKaizou.fnptr!(&num_customizations);
                    byte customization_id = menu_list[selected_ability].customization_id;
                    gear = FhXCall.MsGetSaveWeapon.fnptr!(
                        *(ushort*)(p_DAT_0186a9f8 + GearSelectionWindow->selected_index * 2),
                        (nint)(&gear_name)
                    );

                    byte* p_DAT_0186aa30 = FhUtil.ptr_at<byte>(0x146AA30);
                    int i = -1;
                    do {
                        i++;
                        p_DAT_0186aa30[i] = gear_name[i];
                    } while (gear_name[i] != 0);

                    //_FUN_008d5650((int)gear, menu_list[selected_ability].a_ability_id);
                    if (gear->slot_count != 0) {
                        gear->abilities[selected_gear_slot] = menu_list[selected_ability].a_ability_id;
                    }

                    FhXCall.MsSetSaveParamAll.fnptr!();

                    FhXCall.MsSetWeaponName.fnptr!(gear);
                    FhXCall.MsSaveItemUse.fnptr!(customizations[customization_id].item, -customizations[customization_id].item_cost);
                    FhXCall.SndSepPlaySimple.fnptr!(0x80000063);
                    FhXCall.MsGetSaveWeapon.fnptr!((uint)*(ushort*)(p_DAT_0186a9f8 + GearSelectionWindow->selected_index * 2), (nint)(&gear_name));

                    byte* p_DAT_0186aa70 = FhUtil.ptr_at<byte>(0x146AA70);
                    i = -1;
                    do {
                        i++;
                        p_DAT_0186aa70[i] = gear_name[i];
                    } while (gear_name[i] != 0);

                    byte uVar9 = 0;
                    byte prev;
                    byte curr;
                    i = -1;
                    do {
                        i++;
                        prev = p_DAT_0186aa30[i];
                        curr = p_DAT_0186aa70[i];

                        int is_lower = curr < prev ? 1 : 0;
                        if (curr != prev) {
                            uVar9 = (byte)(-is_lower | 1);
                            break;
                        }
                    } while (curr != 0);

                    if (uVar9 == 0) {
                        uVar9 = 0x1e;
                    } else {
                        uVar9 = 0x1f;
                    }

                    byte* pbVar8 = FhXCall.FUN_008bee80.fnptr!(uVar9);
                    FhXCall.FUN_008c2c40.fnptr!(0, 0, p_DAT_0186aa30);
                    FhXCall.FUN_008c2c40.fnptr!(2, 0, p_DAT_0186aa70);
                    FhXCall.FUN_008e33a0.fnptr!(pbVar8, (byte*)0, (byte*)0);
                {
                    TkWindow* DAT_023cc120 = (TkWindow*)FhUtil.get_at<uint>(0x1FCC120);
                    ;
                    DAT_023cc120->render_priority = 4;
                }
                    *state = 10;
                    break_loop = true;
                    break;

                case 0xe:
                    // Custom state for selecting slot to overwrite
                    if (MyWindow->exit_value == 0) {
                        break_loop = true;
                    } else {
                        _logger.Info($"exit_value={MyWindow->exit_value}");
                        if (MyWindow->exit_value > 0) {
                            selected_gear_slot = MyWindow->selected_index;
                            _logger.Info($"selected_gear_slot={selected_gear_slot}");
                            *state = 5;
                        } else {
                            *state = 8;
                        }

                        break_loop = true;
                    }

                    break;

                default:
                    FhXCall.UpdateGearCustomizationMenuState.chain_from(UpdateGearCustomizationMenuState).fnptr!(window);
                    break_loop = true;
                    break;
            }
        }


        if (*state != pre_state) {
            _logger.Info($"{pre_state} -> {*state}");

            if (pre_state == 0xc && *state == 0xa) {
                // Applied customization

                TkWindow* DAT_0186a9f4 = (TkWindow*)FhUtil.get_at<uint>(0x0146A9F4);
                short selected_idx = DAT_0186a9f4->selected_index;
                CustomizationMenuList* menu_list = FhUtil.ptr_at<CustomizationMenuList>(0x1197730);
                int num_customizations;
                CustomizationRecipe* customizations = FhXCall.MsGetRomKaizou.fnptr!(&num_customizations);
                byte customization_id = menu_list[selected_idx].customization_id;
                if (customizations[customization_id].item_cost != original_kaizou_costs[customization_id]) {
                    uint item_id = (uint)(0xC000 | customization_id);
                    other_inventory.TryGetValue(item_id, out int count);
                    if (--count <= 0) {
                        other_inventory.Remove(item_id);
                        customizations[customization_id].item_cost = original_kaizou_costs[customization_id];
                    } else other_inventory[item_id] = count;
                }
            }
        }

        if (pre_state == 1) {
            // Reset kaizou.bin
            if (original_kaizou_costs != null) {
                int num_customizations;
                CustomizationRecipe* customizations = FhXCall.MsGetRomKaizou.fnptr!(&num_customizations);
                for (int i = 0; i < num_customizations; i++) {
                    customizations[i].item_cost = original_kaizou_costs[i];
                }
            }
        }
    }

    // param_1 is TkMenu*
    public void TkMenuCtrlSummon(TkMenu* menu, int param_2) {
        uint* state = (uint*)(menu->state);
        uint pre_state = *state;


        FhXCall.TkMenuCtrlSummon.chain_from(TkMenuCtrlSummon).fnptr!(menu, param_2);

        if (*state != pre_state) {
            _logger.Debug($"{pre_state} -> {*state}");

            if (pre_state == 0x15) {
                TkWindow* DAT_0186a568 = (TkWindow*)FhUtil.get_at<uint>(0x0146a568);
                short selected_idx = DAT_0186a568->selected_index;
                CustomizationMenuList* menu_list = FhUtil.ptr_at<CustomizationMenuList>(0x1197730);
                byte customization_id = menu_list[selected_idx].customization_id;
                int num_customizations;
                AeonAbilityRecipe* customizations = FhXCall.MsGetRomSummonGrow.fnptr!(&num_customizations);
                _logger.Debug($"Applied customization {get_other_item_name((uint)(0xC07D + customization_id))}");
                if (customizations[customization_id].item_cost != original_sum_grow_costs[customization_id]) {
                    uint item_id = (uint)(0xC07D + customization_id);
                    other_inventory.TryGetValue(item_id, out int count);
                    if (--count <= 0) {
                        other_inventory.Remove(item_id);
                        customizations[customization_id].item_cost = (byte)original_sum_grow_costs[customization_id];
                    } else other_inventory[item_id] = count;
                }
            }

            // 1D -> 1E Applied attribute customization
        }
    }

    public static ManagedCustomString customization_string = new("Free!");

    public void DrawGearCustomizationMenu(TkWindow* window) {
        //FhXCall.DrawGearCustomizationMenu.chain_from(DrawGearCustomizationMenu).fnptr!(param_1);
        DrawGearCustomizationMenu_reimplement(window);
        return;
    }

    public void DrawAeonCustomizationMenu(TkWindow* window) {
        //FhXCall.DrawAeonCustomizationMenu.chain_from(DrawAeonCustomizationMenu).fnptr!(param_1);
        DrawAeonCustomizationMenu_reimplement(window);
    }

    public void DrawAeonCustomizationMenu_reimplement(TkWindow* window) {
        FhXCall.TkVU1SyncPath.fnptr!();
        FhXCall.FUN_008e71d0.fnptr!(7);
        uint current_summon = FhXCall.TkMenuGetCurrentSummon.fnptr!();


        CustomizationMenuList* menu_list = FhUtil.ptr_at<CustomizationMenuList>(0x1197730);
        short selected_idx = window->selected_index;
        Vector2 pos_1;
        Vector2 pos_2;

        uint item_id;
        if (menu_list[selected_idx].status == 0) {
            item_id = 0xFFFFFFF;
        } else {
            byte customization_id = menu_list[selected_idx].customization_id;
            int num_customizations;
            AeonAbilityRecipe* customizations = FhXCall.MsGetRomSummonGrow.fnptr!(&num_customizations);
            item_id = customizations[customization_id].item;
            int item_cost = customizations[customization_id].item_cost;


            // Draw cost section
            if (item_cost == 0) {
                fixed (byte* text = customization_string.encoded) {
                    Vector2 pos = new Vector2(1260f, 381f).game_remap_1080p();
                    FhXCall.ToMakeBtlEasyFont.fnptr!(text, pos.X, pos.Y, 0, 2f);
                }
            } else {
                pos_1 = new Vector2(970f, 380f).game_remap_1080p();
                FhXCall.FUN_008c1c70.fnptr!((int)pos_1.X, (int)pos_1.Y, item_id, item_cost);
            }
        }

        pos_1 = new Vector2(210f, 226f).game_remap_1080p();
        FhXCall.FUN_008ff490.fnptr!(current_summon, pos_1.X, pos_1.Y);

        pos_1 = new Vector2(210f, 312f).game_remap_1080p();
        pos_2 = new Vector2(740f,  48f).game_remap_1080p();
        FhXCall.TODrawMenuPlateXYWHType.fnptr!(pos_1.X, pos_1.Y, pos_2.X, pos_2.Y, 2);

        // Header text
        pos_1 = new Vector2(365f, 320f).game_remap_1080p();
        pos_2 = new Vector2(430f,  36f).game_remap_1080p();
        FhXCall.FUN_008f8bb0.fnptr!(0xf, pos_1.X, pos_1.Y, pos_2.X, pos_2.Y);

        if (-1 < (int)item_id) {
            pos_1 = new Vector2(970f, 312f).game_remap_1080p();
            pos_2 = new Vector2(740f, 48f).game_remap_1080p();
            FhXCall.TODrawMenuPlateXYWHType.fnptr!(pos_1.X, pos_1.Y, pos_2.X, pos_2.Y, 2);

            // Header text ("Item cost")?
            pos_1 = new Vector2(1125f, 318f).game_remap_1080p();
            pos_2 = new Vector2(430f,  36f).game_remap_1080p();
            FhXCall.FUN_008f8bb0.fnptr!(0x10, pos_1.X, pos_1.Y, pos_2.X, pos_2.Y);
        }

        int iVar2 = (int)new Vector2(0, 365f).game_remap_1080p().Y;
        int iVar6, sVar1;
        float fVar10;
        if (window->visible_item_offset == window->scroll_offset) {
            // Draw ability list
            FhXCall.FUN_008c0f40.fnptr!(
                iVar2,
                (int)(new Vector2(0, 70f).game_remap_1080p().Y * 9.0),
                0,
                window->scroll_delta
            );
            sVar1 = window->visible_item_offset;
            fVar10 = 0.0f;
            iVar6 = 0;
        } else {
            // Draw ability list when quick scrolling (L2/R2)
            FhXCall.FUN_008c0f40.fnptr!(
                iVar2,
                (int)(new Vector2(0, 70f).game_remap_1080p()
                                         .Y * 9.0),
                1,
                window->scroll_delta
            );
            FUN_008cd960_Extra(window, 1, window->visible_item_offset, 0.0f, 0.0f);

            FhXCall.FUN_008c0f40.fnptr!(
                iVar2,
                (int)(new Vector2(0, 70f).game_remap_1080p()
                                         .Y * 9.0),
                2,
                window->scroll_delta
            );

            fVar10 = (int)(new Vector2(0, 70f).game_remap_1080p()
                                              .Y * 9.0 * window->scroll_delta * -0.00024414063);
            sVar1 = window->scroll_offset;
            iVar6 = 2;
        }

        FUN_008cd960_Extra(window, iVar6, sVar1, 0.0f, fVar10);
        FhXCall.FUN_008c1350_DrawScissor512x416.fnptr!();

        ushort _DAT_0186a5a4 = FhUtil.get_at<ushort>(0x0146a5a4);
        ushort _DAT_0186a5a6 = FhUtil.get_at<ushort>(0x0146a5a6);
        FhXCall.FUN_008cd9f0.fnptr!(window, _DAT_0186a5a4 + 0xc, _DAT_0186a5a6 + 1);

        float local_8;
        FhXCall.ToGetCrossExtMesFontWidth.fnptr!(0, FhXCall.FUN_008bee80.fnptr!(5), &local_8, 0.78f, 1.0f);

        fVar10 = (float)(double)(new Vector2(80f, 0).game_remap_1080p()
                                                    .X + local_8);
        local_8 = fVar10;


        float fVar11 = new Vector2(660f, 0).game_remap_1080p()
                                           .X;
        if (fVar11 < (float)fVar10 == (float.IsNaN(fVar11) || float.IsNaN(fVar10))) {
            fVar10 = new Vector2(740f, 0).game_remap_1080p()
                                         .X;
        } else {
            fVar10 = (float)(new Vector2(80f, 0).game_remap_1080p()
                                                .X + local_8);
        }

        // graphicUiRemapX2(1806.0);
        fVar11 = new Vector2(1806f, 0).game_remap_1080p()
                                      .X - fVar10;
        float fVar12 = (float)((fVar10 - local_8) * 0.5 + fVar11);

        pos_1 = new Vector2(955f, 370f).game_remap_1080p();
        pos_2 = new Vector2(  8f, 630f).game_remap_1080p();
        FhXCall.DrawCrossMenuScrollParts.fnptr!(pos_1.X, pos_1.Y, pos_2.X, pos_2.Y, window->visible_item_offset, window->max_visible_items, window->num_items);

        FhXCall.TODrawMenuPlateXYWHType.fnptr!(
            fVar11,
            new Vector2(0, 925f).game_remap_1080p()
                                .Y,
            fVar10,
            new Vector2(0, 48f).game_remap_1080p()
                               .Y,
            2
        );

        float uv_y2 = 0.3017578f;
        float uv_x2 = 0.99902344f;
        float uv_y1 = 0.22851563f;
        float uv_x1 = 0.9267578f;
        pos_1 = new Vector2(0f, 920f).game_remap_1080p();
        pos_2 = new Vector2(64f,  64f).game_remap_1080p();
        FhXCall.TOMkpShapeXYWHUV.fnptr!(0xf3, fVar12, pos_1.Y, pos_2.X, pos_2.Y, uv_x1, uv_y1, uv_x2, uv_y2);

        pos_1 = new Vector2(80f, 928f).game_remap_1080p();
        FhXCall.TOMkpCrossExtMesFontLClut.fnptr!(0, FhXCall.FUN_008bee80.fnptr!(5), pos_1.X + fVar12, pos_1.Y, 0, 0.78f, 1.0f);

        item_id = FhXCall.FUN_008d48e0.fnptr!();
        FhXCall.FUN_008d4140.fnptr!(item_id, 1);
        FhXCall.TkMn2DrawKickSyncPacket.fnptr!();
    }

    private void FUN_008cd960_Extra(TkWindow* window, int param_2, int menu_offset, float x, float y) {
        FhXCall.FUN_008cd960.fnptr!(window, param_2, menu_offset, x, y);

        Vector2 pos = new(x, y);
        pos += new Vector2(209f + 50f, 306f).game_remap_1080p();

        if (param_2 == 0) {
            pos.Y -= (float)(new Vector2(0, 70f).game_remap_1080p()
                                                .Y * window->scroll_delta * 0.00024414063); // Scroll offset
        }

        CustomizationMenuList* menu_list = FhUtil.ptr_at<CustomizationMenuList>(0x1197730);
        short menu_length = window->num_items;
        for (int i = -1; i < 10; i++) {
            int curr_index = menu_offset + i;
            if (0 <= curr_index && curr_index < menu_length) {
                if (menu_list[curr_index].customization_id != 0xFF) {
                    int num_customizations;
                    AeonAbilityRecipe* customizations = FhXCall.MsGetRomSummonGrow.fnptr!(&num_customizations);

                    uint item_id   = customizations[menu_list[curr_index].customization_id].item;
                    int item_cost = customizations[menu_list[curr_index].customization_id].item_cost;
                    if (item_cost == 0) {
                        fixed (byte* text = customization_string.encoded) {
                            FhXCall.ToMakeBtlEasyFont.fnptr!(text, pos.X, pos.Y, 0, 0.78f);
                        }
                    }
                }
            }

            pos += new Vector2(0, 70f).game_remap_1080p();
        }
    }

    public void DrawGearCustomizationMenu_reimplement(TkWindow* window) {
        CustomizationMenuList* menu_list = FhUtil.ptr_at<CustomizationMenuList>(0x1197730);
        short selected_idx = window->selected_index;
        Vector2 pos_1;
        Vector2 pos_2;
        if (menu_list[selected_idx].customization_id != 0xFF) {
            int num_customizations;
            CustomizationRecipe* customizations = FhXCall.MsGetRomKaizou.fnptr!(&num_customizations);

            uint item_id   = customizations[menu_list[selected_idx].customization_id].item;
            int item_cost = customizations[menu_list[selected_idx].customization_id].item_cost;

            // Draw cost section
            if (item_cost == 0) {
                fixed (byte* text = customization_string.encoded) {
                    Vector2 pos = new Vector2(1260f, 320f).game_remap_1080p();
                    FhXCall.ToMakeBtlEasyFont.fnptr!(text, pos.X, pos.Y, 0, 2f);
                }
            } else {
                pos_1 = new Vector2(970f, 319f).game_remap_1080p();
                FhXCall.FUN_008c1c70.fnptr!((int)pos_1.X, (int)pos_1.Y, item_id, item_cost);
            }

            // Header background
            pos_1 = new Vector2(970f, 252f).game_remap_1080p();
            pos_2 = new Vector2(740f, 60f).game_remap_1080p();
            FhXCall.TODrawMenuPlateXYWHType.fnptr!(pos_1.X, pos_1.Y, pos_2.X, pos_2.Y, 2);

            // Header ("Item cost")
            pos_1 = new Vector2(1125f, 264f).game_remap_1080p();
            pos_2 = new Vector2(430f, 36f).game_remap_1080p();
            FhXCall.FUN_008f8bb0.fnptr!(0x10, pos_1.X, pos_1.Y, pos_2.X, pos_2.Y);
        }
        //_TkMenuGetCurrentPlayer(); // Result is unused?

        // Abilities Header background
        pos_1 = new Vector2(211f, 252f).game_remap_1080p();
        pos_2 = new Vector2(740f, 60f).game_remap_1080p();
        FhXCall.TODrawMenuPlateXYWHType.fnptr!(pos_1.X, pos_1.Y, pos_2.X, pos_2.Y, 2);

        // Header ("Abilities")
        pos_1 = new Vector2(366f, 264f).game_remap_1080p();
        pos_2 = new Vector2(430f, 36f).game_remap_1080p();
        FhXCall.FUN_008f8bb0.fnptr!(7, pos_1.X, pos_1.Y, pos_2.X, pos_2.Y);

        if (window->visible_item_offset == window->scroll_offset) {
            // Draw ability list
            FhXCall.TODrawScissorXYWH.fnptr!(
                0,
                (int)(new Vector2(0, 315f).game_remap_1080p()
                                          .Y),
                0x200,
                (int)(new Vector2(0, 680f).game_remap_1080p()
                                          .Y)
            );
            FUN_008d5d20_Extra(window, 0, window->visible_item_offset, 0, 0);
        } else {
            // Draw ability list when quick scrolling (L2/R2)
            short uVar5 = window->scroll_delta; // Scroll offset

            FhXCall.FUN_008c0f40.fnptr!(
                (int)(new Vector2(0, 315f).game_remap_1080p()
                                          .Y),
                (int)(new Vector2(0, 680f).game_remap_1080p()
                                          .Y),
                1,
                uVar5
            );
            FUN_008d5d20_Extra(window, 1, window->visible_item_offset, 0, 0);

            FhXCall.FUN_008c0f40.fnptr!(
                (int)(new Vector2(0, 315f).game_remap_1080p()
                                          .Y),
                (int)(new Vector2(0, 680f).game_remap_1080p()
                                          .Y),
                2,
                uVar5
            );

            int iVar2 = (int)(new Vector2(0, 675f).game_remap_1080p()
                                                  .Y * uVar5 * -0.00024414063);
            FUN_008d5d20_Extra(window, 2, window->scroll_offset, 0, iVar2);
        }

        FhXCall.FUN_008c1350_DrawScissor512x416.fnptr!();

        pos_1 = new Vector2(389f, 325f).game_remap_1080p();
        FhXCall.FUN_008d5dc0.fnptr!(window, (int)pos_1.X, (int)pos_1.Y);

        {
            int uVar5 = window->num_items;
            int uVar1 = window->max_visible_items;
            int iVar2 = window->visible_item_offset;
            pos_1 = new Vector2(955f, 319f).game_remap_1080p();
            pos_2 = new Vector2(8f, 675f).game_remap_1080p();
            FhXCall.DrawCrossMenuScrollParts.fnptr!(pos_1.X, pos_1.Y, pos_2.X, pos_2.Y, iVar2, uVar1, uVar5);
        }

        {
            // Draw gear name
            uint _DAT_0186a9f0 = FhUtil.get_at<uint>(0x146A9F0);
            int iVar2 = *(short*)(_DAT_0186a9f0 + 0x48);
            pos_1 = new Vector2(970f, 659f).game_remap_1080p();
            FhXCall.FUN_008d6630.fnptr!((int)pos_1.X, (int)pos_1.Y, iVar2);
        }
    }

    private void FUN_008d5d20_Extra(TkWindow* window, int param_2, int menu_offset, int x, int y) {
        FhXCall.FUN_008d5d20.fnptr!(window, param_2, menu_offset, x, y);

        Vector2 pos = new(x, y);
        pos += new Vector2(209f + 50f, 255f).game_remap_1080p();

        if (param_2 == 0) {
            pos.Y -= (float)(new Vector2(0, 75f).game_remap_1080p()
                                                .Y * window->scroll_delta * 0.00024414063); // Scroll offset
        }

        CustomizationMenuList* menu_list = FhUtil.ptr_at<CustomizationMenuList>(0x1197730);
        short menu_length = window->num_items;
        for (int i = -1; i < 10; i++) {
            int curr_index = menu_offset + i;
            if (0 <= curr_index && curr_index < menu_length) {
                if (menu_list[curr_index].customization_id != 0xFF) {
                    int num_customizations;
                    CustomizationRecipe* customizations = FhXCall.MsGetRomKaizou.fnptr!(&num_customizations);

                    uint item_id   = customizations[menu_list[curr_index].customization_id].item;
                    int item_cost = customizations[menu_list[curr_index].customization_id].item_cost;
                    if (item_cost == 0) {
                        fixed (byte* text = customization_string.encoded) {
                            FhXCall.ToMakeBtlEasyFont.fnptr!(text, pos.X, pos.Y, 0, 0.78f);
                        }
                    }
                }
            }

            pos += new Vector2(0, 75f).game_remap_1080p();
        }
    }

    public static TkWindow* MyWindow;

    public static void CreateMyWindow(Equipment* gear) {
        MyWindow = FhXCall.TkMenuMainAllocWindow.fnptr!();
        MyWindow->num_items         = gear->slot_count;
        MyWindow->max_visible_items = 4;
        MyWindow->selected_index    = 0;
        MyWindow->render_priority   = 2;
        MyWindow->fn_init           = &MyWindow_Init;
        MyWindow->fn_state          = &MyWindow_State;
        MyWindow->fn_render         = &MyWindow_Render;
        FhXCall.TkMenuMainRegistWindow.fnptr!(MyWindow);
    }

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
    public static void MyWindow_Init(TkWindow* window) {
        window->current_state = 0;
    }

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
    public static void MyWindow_State(TkWindow* window) {
        while (true) {
            switch (window->current_state) {
                case 0:
                    window->exit_value = 0;
                    window->selected_index = 0;
                    window->current_state = 1;
                    return;

                case 1:
                    if (Globals.Input.up.is_pressed && 0 < window->selected_index) {
                        FhXCall.SndSepPlaySimple.fnptr!(0x80000001);
                        window->selected_index--;
                    } else if (Globals.Input.down.is_pressed && window->selected_index < window->num_items - 1) {
                        FhXCall.SndSepPlaySimple.fnptr!(0x80000001);
                        window->selected_index++;
                    } else if (Globals.Input.confirm.is_pressed) {
                        FhXCall.SndSepPlaySimple.fnptr!(0x80000001);
                        window->current_state = 2;
                    } else if (Globals.Input.cancel.is_pressed) {
                        FhXCall.SndSepPlaySimple.fnptr!(0x80000004);
                        window->current_state = 3;
                    }

                    return;

                case 2:
                    window->exit_value = 1;
                    window->current_state = 5;
                    break;

                case 3:
                    window->exit_value = -1;
                    window->current_state = 7;
                    break;

                default:
                    return;
            }
        }
    }

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
    public static void MyWindow_Render(TkWindow* window) {
        Vector2 pos = new Vector2(970 + 150, 735 + 12 + 68 * window->selected_index).game_remap_1080p();
        FhXCall.TkMn2DrawCrossCursor.fnptr!(pos.X, pos.Y, 0);
    }

    public static bool FUN_008d5720(uint gear_id, int param_2) {
        Equipment* gear = FhXCall.MsGetSaveWeapon.fnptr!(gear_id, 0);

        bool can_customize = false;
        if (gear->exists && !gear->is_hidden && gear->slot_count > 0) {
            if (param_2 != 0 || (!gear->is_celestial && !gear->is_brotherhood)) {
                //if (gear->abilities[gear->slot_count-1] == 0xff || gear->abilities[gear->slot_count - 1] == 0)
                can_customize = true;
            }
        }

        return can_customize;
    }
}
