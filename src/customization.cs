// SPDX-License-Identifier: MIT

using Fahrenheit.FFX;

using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using Fahrenheit;

using static ArchipelagoFFX.delegates;

namespace ArchipelagoFFX;

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


    public static TkMenuMainAllocWindow _TkMenuMainAllocWindow;
    public static TkMenuMainRegistWindow _TkMenuMainRegistWindow;


    public static FUN_008e33a0 _FUN_008e33a0;
    public static FUN_008b4460 _FUN_008b4460;
    public static FUN_008e2de0 _FUN_008e2de0;
    public static MsSetSaveParamAll _MsSetSaveParamAll;
    public static MsSetWeaponName _MsSetWeaponName;
    public static FUN_008c2c40 _FUN_008c2c40;
    public static TkMn2DrawCrossCursor _TkMn2DrawCrossCursor;

    public static FhMethodHandle<FUN_008d5720> _FUN_008d5720;

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

    public void h_PrepareMenuList(MenuListEnum menu_list_id, Equipment* gear) {
        _logger.Info($"{menu_list_id}");

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
        } else if (menu_list_id == MenuListEnum.AEON_ABILITIES) {
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
                        _logger.Debug($"Free customization: {get_other_item_name(item_id)}");
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

    /// <summary>
    ///     Known states:
    ///         2: Get selected weapon. Goes to 3 if it doesn't exist, otherwise goes to 5
    ///         3: Goes to 4 and returns
    ///         4: Goes to 1
    ///         5: Calls TMn2SetStatusBGDiff, then prepares ability menu list and goes to 7
    ///         6: Calls TkMenuRestartSelFileWindow, then ??? and goes to 7
    /// </summary>
    /// <param name="window"></param>
    public void h_UpdateGearCustomizationMenuState(TkWindow* window) {
        uint* state = FhUtil.ptr_at<uint>(0x146AA28);
        uint pre_state = *state;

        byte* gear_name;
        Equipment* gear;
        byte* p_DAT_0186a9f8 = (byte*)FhUtil.get_at<uint>(0x146A9F8);

        bool break_loop = false;
        while (!break_loop) {
            switch (*state) {
                case 2:
                    _FUN_008b4460(window);
                    if (window->exit_value < 1) {
                        return;
                    }

                    ushort weapon_index = *(ushort*)(p_DAT_0186a9f8 + window->selected_index * 2);
                    gear = _MsGetSaveWeapon(weapon_index, 0);
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
                        _SndSepPlaySimple(0x80000001);
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
                    gear = _MsGetSaveWeapon(
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
                        _FUN_008e2de0();
                        *state = 6;
                        break_loop = true;
                        break;
                    }

                    if (DAT_023cc120->exit_value == 0) {
                        break_loop = true;
                        break;
                    }

                    _FUN_008e2de0();
                    if (DAT_023cc120->selected_index != 0) {
                        _SndSepPlaySimple(0x80000001);
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
                    CustomizationRecipe* customizations = _MsGetRomKaizou(&num_customizations);
                    byte customization_id = menu_list[selected_ability].customization_id;
                    gear = _MsGetSaveWeapon(
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

                    _MsSetSaveParamAll();

                    _MsSetWeaponName(gear);
                    _MsSaveItemUse(customizations[customization_id].item, -customizations[customization_id].item_cost);
                    _SndSepPlaySimple(0x80000063);
                    _MsGetSaveWeapon((uint)*(ushort*)(p_DAT_0186a9f8 + GearSelectionWindow->selected_index * 2), (nint)(&gear_name));

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

                    byte* pbVar8 = _FUN_008bee80(uVar9);
                    _FUN_008c2c40(0, 0, p_DAT_0186aa30);
                    _FUN_008c2c40(2, 0, p_DAT_0186aa70);
                    _FUN_008e33a0(pbVar8, (byte*)0, (byte*)0);
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
                    _UpdateGearCustomizationMenuState.orig_fptr(window);
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
                CustomizationRecipe* customizations = _MsGetRomKaizou(&num_customizations);
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
                CustomizationRecipe* customizations = _MsGetRomKaizou(&num_customizations);
                for (int i = 0; i < num_customizations; i++) {
                    customizations[i].item_cost = original_kaizou_costs[i];
                }
            }
        }
    }

    // param_1 is TkMenu*
    public void h_UpdateAeonCustomizationMenuState(uint param_1, uint param_2) {
        uint* state = (uint*)(param_1 + 0x1c);
        uint pre_state = *state;


        _UpdateAeonCustomizationMenuState.orig_fptr(param_1, param_2);

        if (*state != pre_state) {
            _logger.Debug($"{pre_state} -> {*state}");

            if (pre_state == 0x15) {
                TkWindow* DAT_0186a568 = (TkWindow*)FhUtil.get_at<uint>(0x0146a568);
                short selected_idx = DAT_0186a568->selected_index;
                CustomizationMenuList* menu_list = FhUtil.ptr_at<CustomizationMenuList>(0x1197730);
                byte customization_id = menu_list[selected_idx].customization_id;
                int num_customizations;
                CustomizationRecipe* customizations = _MsGetRomSummonGrow(&num_customizations);
                _logger.Debug($"Applied customization {get_other_item_name((uint)(0xC07D + customization_id))}");
                if (customizations[customization_id].item_cost != original_sum_grow_costs[customization_id]) {
                    uint item_id = (uint)(0xC07D + customization_id);
                    other_inventory.TryGetValue(item_id, out int count);
                    if (--count <= 0) {
                        other_inventory.Remove(item_id);
                        customizations[customization_id].item_cost = original_sum_grow_costs[customization_id];
                    } else other_inventory[item_id] = count;
                }
            }

            // 1D -> 1E Applied attribute customization
        }
    }

    public static ManagedCustomString customization_string = new("Free!");

    public void h_DrawGearCustomizationMenu(TkWindow* window) {
        //_DrawGearCustomizationMenu.orig_fptr(param_1);
        DrawGearCustomizationMenu_reimplement(window);
        return;
    }

    public void h_DrawAeonCustomizationMenu(TkWindow* window) {
        //_DrawAeonCustomizationMenu.orig_fptr(param_1);
        DrawAeonCustomizationMenu_reimplement(window);
    }

    public void DrawAeonCustomizationMenu_reimplement(TkWindow* window) {
        _TkVU1SyncPath();
        _FUN_008e71d0(7);
        uint current_summon = _TkMenuGetCurrentSummon();


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
            CustomizationRecipe* customizations = _MsGetRomSummonGrow(&num_customizations);
            item_id = customizations[customization_id].item;
            int item_cost = customizations[customization_id].item_cost;


            // Draw cost section
            if (item_cost == 0) {
                fixed (byte* text = customization_string.encoded) {
                    Vector2 pos = new Vector2(1260f, 381f).game_remap_1080p();
                    _ToMakeBtlEasyFont(text, pos.X, pos.Y, 0, 2f);
                }
            } else {
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
            pos_2 = new Vector2(430f,  36f).game_remap_1080p();
            _FUN_008f8bb0(0x10, pos_1.X, pos_1.Y, pos_2.X, pos_2.Y);
        }

        int iVar2 = (int)new Vector2(0, 365f).game_remap_1080p()
                                             .Y;
        int iVar6, sVar1;
        float fVar10;
        if (window->visible_item_offset == window->scroll_offset) {
            // Draw ability list
            _FUN_008c0f40(
                iVar2,
                (int)(new Vector2(0, 70f).game_remap_1080p()
                                         .Y * 9.0),
                0,
                window->scroll_delta
            );
            sVar1 = window->visible_item_offset;
            fVar10 = 0.0f;
            iVar6 = 0;
        } else {
            // Draw ability list when quick scrolling (L2/R2)
            _FUN_008c0f40(
                iVar2,
                (int)(new Vector2(0, 70f).game_remap_1080p()
                                         .Y * 9.0),
                1,
                window->scroll_delta
            );
            FUN_008cd960_Extra(window, 1, window->visible_item_offset, 0.0f, 0.0f);

            _FUN_008c0f40(
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
        _FUN_008c1350_DrawScissor512x416();

        ushort _DAT_0186a5a4 = FhUtil.get_at<ushort>(0x0146a5a4);
        ushort _DAT_0186a5a6 = FhUtil.get_at<ushort>(0x0146a5a6);
        _FUN_008cd9f0(window, _DAT_0186a5a4 + 0xc, _DAT_0186a5a6 + 1);

        float local_8;
        _ToGetCrossExtMesFontWidth(0, _FUN_008bee80(5), &local_8, 0.78f, 1.0f);

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
        _DrawCrossMenuScrollParts(pos_1.X, pos_1.Y, pos_2.X, pos_2.Y, window->visible_item_offset, window->max_visible_items, window->num_items);

        _TODrawMenuPlateXYWHType(
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
        _TOMkpShapeXYWHUV(0xf3, fVar12, pos_1.Y, pos_2.X, pos_2.Y, uv_x1, uv_y1, uv_x2, uv_y2);

        pos_1 = new Vector2(80f, 928f).game_remap_1080p();
        _TOMkpCrossExtMesFontLClut(0, _FUN_008bee80(5), pos_1.X + fVar12, pos_1.Y, 0, 0.78f, 1.0f);

        item_id = _FUN_008d48e0();
        _FUN_008d4140(item_id, 1);
        _TkMn2DrawKickSyncPacket();
    }

    private void FUN_008cd960_Extra(TkWindow* window, int param_2, int menu_offset, float x, float y) {
        _FUN_008cd960(window, param_2, menu_offset, x, y);

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

    public void DrawGearCustomizationMenu_reimplement(TkWindow* window) {
        CustomizationMenuList* menu_list = FhUtil.ptr_at<CustomizationMenuList>(0x1197730);
        short selected_idx = window->selected_index;
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

        if (window->visible_item_offset == window->scroll_offset) {
            // Draw ability list
            _TODrawScissorXYWH(
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

            _FUN_008c0f40(
                (int)(new Vector2(0, 315f).game_remap_1080p()
                                          .Y),
                (int)(new Vector2(0, 680f).game_remap_1080p()
                                          .Y),
                1,
                uVar5
            );
            FUN_008d5d20_Extra(window, 1, window->visible_item_offset, 0, 0);

            _FUN_008c0f40(
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

        _FUN_008c1350_DrawScissor512x416();

        pos_1 = new Vector2(389f, 325f).game_remap_1080p();
        _FUN_008d5dc0(window, (int)pos_1.X, (int)pos_1.Y);

        {
            int uVar5 = window->num_items;
            int uVar1 = window->max_visible_items;
            int iVar2 = window->visible_item_offset;
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
    }

    private void FUN_008d5d20_Extra(TkWindow* window, int param_2, int menu_offset, int x, int y) {
        _FUN_008d5d20(window, param_2, menu_offset, x, y);

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

    public static TkWindow* MyWindow;

    public static void CreateMyWindow(Equipment* gear) {
        MyWindow = _TkMenuMainAllocWindow();
        MyWindow->num_items         = gear->slot_count;
        MyWindow->max_visible_items = 4;
        MyWindow->selected_index    = 0;
        MyWindow->render_priority   = 2;
        MyWindow->fn_init           = &MyWindow_Init;
        MyWindow->fn_state          = &MyWindow_State;
        MyWindow->fn_render         = &MyWindow_Render;
        _TkMenuMainRegistWindow(MyWindow);
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
                    if (Globals.Input.up.just_pressed && 0 < window->selected_index) {
                        _SndSepPlaySimple(0x80000001);
                        window->selected_index--;
                    } else if (Globals.Input.down.just_pressed && window->selected_index < window->num_items - 1) {
                        _SndSepPlaySimple(0x80000001);
                        window->selected_index++;
                    } else if (Globals.Input.confirm.just_pressed) {
                        _SndSepPlaySimple(0x80000001);
                        window->current_state = 2;
                    } else if (Globals.Input.cancel.just_pressed) {
                        _SndSepPlaySimple(0x80000004);
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
        _TkMn2DrawCrossCursor(pos.X, pos.Y, 0);
    }

    public static bool h_FUN_008d5720(uint gear_id, int param_2) {
        Equipment* gear = _MsGetSaveWeapon(gear_id, 0);

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
