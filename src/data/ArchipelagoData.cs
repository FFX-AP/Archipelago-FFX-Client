using Fahrenheit.Core;
using Fahrenheit.Core.FFX;
using Fahrenheit.Core.FFX.Ids;
using Fahrenheit.Modules.ArchipelagoFFX.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using Archipelago.MultiClient.Net.Enums;

namespace Fahrenheit.Modules.ArchipelagoFFX;

public static class ArchipelagoData {
    public static string[] id_to_character => [
        "TIDUS",
        "YUNA",
        "AURON",
        "KIMAHRI",
        "WAKKA",
        "LULU",
        "RIKKU",
        "SEYMOUR",
        "VALEFOR",
        "IFRIT",
        "IXION",
        "SHIVA",
        "BAHAMUT",
        "ANIMA",
        "YOJIMBO",
        "MAGUS1",
        "MAGUS2",
        "MAGUS3",
        "DUMMY",
        "DUMMY2",
    ];

    public enum RegionEnum {
        None = 0,
        DreamZanarkand,
        BaajTemple,
        Besaid,
        Kilika,
        Luca,
        MiihenHighroad,
        MushroomRockRoad,
        Djose,
        Moonflow,
        Guadosalam,
        ThunderPlains,
        Macalania,
        Bikanel,
        Airship,
        Bevelle,
        CalmLands,
        CavernOfTheStolenFayth,
        MtGagazet,
        ZanarkandRuins,
        Sin,
        OmegaRuins,
        MonsterArena,
    }

    public enum GoalRequirement {
        None = 0,
        PartyMembers,
        Pilgrimage,
        PartyMembersAndAeons,
        Nemesis
    }

    public static RegionEnum[] pilgrimageRegions = [
        RegionEnum.Besaid,
        RegionEnum.Kilika,
        RegionEnum.Djose,
        RegionEnum.Macalania,
        RegionEnum.Bevelle,
        RegionEnum.ZanarkandRuins,
    ];

    public static ushort[] pilgrimageStoryChecks = [
        182,    // Valefor
        348,    // Ifrit
        1010,   // Ixion
        1545,   // Shiva
        2220,   // Bahamut
        2850    // Yunalesca
    ];

    public static Dictionary<RegionEnum, List<int>> region_to_ids = new(){
        { RegionEnum.DreamZanarkand, [
            132,
            368,
            376,
            371,
            370,
            366,
            389,
            367,
            384,
            385,
            ] },
        { RegionEnum.BaajTemple, [
            48,
            49,
            50,
            63,
            52,
            74,
            295,
            196,
            298,
            51,
            71,
            288,
            64,
            380,
            ] },
        { RegionEnum.Besaid, [
            70,
            20,
            41,
            67,
            69,
            133,
            17,
            142,
            144,
            145,
            60,
            143,
            84,
            42,
            147,
            146,
            122,
            103,
            83,
            100,
            252,
            68,
            21,
            22,
            19,
            117,
            301,
            148,
            149,
            150,
            154,
            61,
            282,
            220,
            139,
            191, // 
            336, // Jecht Sphere
            337, // Jecht Sphere
            ] },
        { RegionEnum.Kilika, [
            43,
            53,
            152,
            46,
            16,
            151,
            293,
            18,
            65,
            78,
            155,
            156,
            96,
            44,
            108,
            45,
            98,
            47,
            118,
            167,
            237,
            168,
            94,
            169,
            191,
            297,
            185,
            294, // Maybe
            302, // Maybe
            ] },
        { RegionEnum.Luca, [
            267,
            377,
            268,
            355,
            72,
            123,
            73,
            85,
            86,
            87,
            88,
            89,
            77,
            157,
            170,
            158,
            184,
            104,
            107,
            159,
            57,
            121,
            299,
            113,
            124,
            62,
            212,
            347,
            250,
            125,
            55,
            335, // Jecht Sphere
            ] },
        { RegionEnum.MiihenHighroad, [
            95,
            120,
            127,
            58,
            171,
            112,
            115,
            116,
            59,
            334, // Jecht Sphere
            ] },
        { RegionEnum.MushroomRockRoad, [
            79,
            92,
            128,
            119,
            247,
            254,
            218,
            341,
            134,
            131,
            253, // Jecht Sphere?
            289, // Maybe
            ] },
        { RegionEnum.Djose, [
            93,
            76,
            82,
            210,
            81,
            161,
            160,
            214,
            90,
            91,
            245,
            ] },
        { RegionEnum.Moonflow, [
            75,
            105,
            187,
            188,
            235,
            99,
            291,
            236,
            190,
            189,
            109,
            97,
            234, // Maybe
            333, // Jecht Sphere
            ] },
        { RegionEnum.Guadosalam, [
            135,
            243,
            174,
            173,
            172,
            163,
            141,
            197,
            217,
            175,
            257,
            193,
            364,
            134,
            213, // Maybe?
            ] },
        { RegionEnum.ThunderPlains, [
            140,
            256,
            264,
            263,
            262,
            162,
            332, // Jecht Sphere
            ] },
        { RegionEnum.Macalania, [
            110,
            241,
            242,
            221,
            248,
            164,
            215,
            102,
            192,
            153,
            106,
            178,
            179,
            239,
            80,
            54,
            284,
            330,
            331,
            332,
            191,
            260, // Maybe
            365, // Maybe
            391, // Luca flashback
            338, // Jecht Sphere
            ] },
        { RegionEnum.Bikanel, [
            129,
            136,
            137,
            138,
            130,
            276,
            280,
            286,
            275,
            219,
            303,
            261,
            391, // Luca flashback
            ] },
        { RegionEnum.Airship, [
            194,
            265,
            351,
            211,
            277,
            255,
            392,
            205,
            199,
            200,
            201,
            374, // Unsure
            375, // Unsure
            202, // Unsure
            ] },
        { RegionEnum.Bevelle, [
            205,
            180,
            181,
            182,
            306,
            226,
            198,
            209,
            208,
            183,
            274,
            206,
            177,
            176,
            329,
            227,
            305,
            238, // Maybe
            269, // Maybe
            273, // Maybe
            287, // Maybe
            339, // Jecht Sphere
            195,
            207,
            ] },
        { RegionEnum.CalmLands, [
            223,
            290,
            308,
            279,
            372,
            ] },
        { RegionEnum.CavernOfTheStolenFayth, [
            266,
            56,
            283,
            ] },
        { RegionEnum.MtGagazet, [
            259,
            244,
            285,
            309,
            134,
            165,
            249,
            272,
            310,
            311,
            312,
            361,
            381,
            383, // Braska Sphere
            362,
            ] },
        { RegionEnum.ZanarkandRuins, [
            132,
            363,
            225,
            314,
            222,
            316,
            320,
            317,
            318,
            224,
            270,
            319,
            315,
            313,
            ] },
        { RegionEnum.Sin, [
            322,
            203,
            296,
            204,
            327,
            324, // Point of no return : 3250
            325,
            326,
            386, // Ending
            387, // Ending
            388, // Ending
            390, // Ending
            ] },
        { RegionEnum.OmegaRuins, [
            258,
            271,
            ] },
        { RegionEnum.MonsterArena, [
            307,
            ] },
    };

    public static readonly Lookup<int, RegionEnum> id_to_regions = (Lookup<int, RegionEnum>)region_to_ids.SelectMany(region => region.Value, (region, id) => new{region.Key, id}).ToLookup(pair => pair.id, pair => pair.Key);

    public delegate void ArchipelagoStoryCheckDelegate(ArchipelagoRegion region);

    public class ArchipelagoStoryCheck {
        public ushort? next_story_progress;
        public ushort? next_room_id;
        public ushort? next_entrance;
        public bool    visit_complete    = false;
        public bool    pilgrimage        = false;
        public bool    return_to_airship = false;
        public ArchipelagoStoryCheckDelegate? check_delegate;
    }
    public enum SaveDataType {
        Byte,
        Short,
        Int,
    }
    public struct ArchipelagoRegionSaveData(uint offset, int size) {
        [JsonInclude]
        public uint   offset = offset;
        [JsonInclude]
        public int    size   = size;
        [JsonInclude]
        public byte[] bytes  = new byte[size];
        public ArchipelagoRegionSaveData(uint offset, int size, byte[] value) : this(offset, size) {
            this.bytes = value;
        }
    }
    public unsafe class ArchipelagoRegion {
        public Dictionary<ushort, ArchipelagoStoryCheck> story_checks = [];

        [JsonInclude]
        public int completed_visits;
        [JsonInclude]
        public ushort story_progress;
        [JsonInclude]
        public ushort room_id;
        [JsonInclude]
        public ushort entrance;
        public bool pilgrimage_completed { get; set; }
        public uint airship_destination_index;

        [JsonInclude]
        public ArchipelagoRegionSaveData[] savedata = [];

    }

    public static unsafe Dictionary<RegionEnum, ArchipelagoRegion> region_starting_state => new(){
        {RegionEnum.DreamZanarkand, new(){ story_progress = 0, room_id = 132, entrance = 0, airship_destination_index = 99,
            story_checks = {
                // Dream Zanarkand region does not exist in Apworld, so no place to put Tidus location. Revisit when starting party member rando is added.
                //{ 4,  new() {check_delegate = (r) => {
                //    // Tidus
                //    int partyMember_id = 0;
                //    if (!FFXArchipelagoClient.local_checked_locations.Contains(partyMember_id | (long)FFXArchipelagoClient.ArchipelagoLocationType.PartyMember)) {
                //        if (ArchipelagoFFXModule.item_locations.party_member.TryGetValue(partyMember_id, out var item)) {
                //            if (FFXArchipelagoClient.sendLocation(partyMember_id, FFXArchipelagoClient.ArchipelagoLocationType.PartyMember)) {
                //                ArchipelagoFFXModule.obtain_item(item.id);
                //            }
                //        }
                //    }
                //} } },
                { 5, new() {check_delegate = (r) => {
                    ArchipelagoFFXModule.logger.Info("Dream Zanarkand complete");
                    ArchipelagoFFXModule.save_party();
                    ArchipelagoFFXModule.reset_party();
                    ArchipelagoFFXModule.call_warp_to_map(382, 0);
                }} }
            } } },
        {RegionEnum.BaajTemple, new(){ story_progress = 30, room_id = 48, entrance = 0, airship_destination_index = 1, 
            story_checks = {
                { 60, new() {check_delegate = (r) => {ArchipelagoFFXModule.logger.Info("Baaj Temple visit 1 complete"); } } },
                { 110, new() {visit_complete = true, next_story_progress = 3210, next_room_id = 49, next_entrance = 2, return_to_airship = true, check_delegate = (r) => {ArchipelagoFFXModule.logger.Info("Al Bhed Ship complete"); } } },
            } } },
        {RegionEnum.Besaid, new(){ story_progress = 111, room_id = 70, entrance = 0, airship_destination_index = 2,
            story_checks = {
                { 119, new() {check_delegate = (r) => {
                    // Wakka
                    int partyMember_id = 4;
                    if (!FFXArchipelagoClient.local_checked_locations.Contains(partyMember_id | (long)FFXArchipelagoClient.ArchipelagoLocationType.PartyMember)) {
                        if (ArchipelagoFFXModule.item_locations.party_member.TryGetValue(partyMember_id, out var item)) {
                            if (FFXArchipelagoClient.sendLocation(partyMember_id, FFXArchipelagoClient.ArchipelagoLocationType.PartyMember)) {
                                ArchipelagoFFXModule.obtain_item(item.id);
                            }
                        }
                    }
                } } },
                { 182, new() {pilgrimage = true, check_delegate = (r) => {
                    // Valefor
                    int partyMember_id = 8;
                    if (!FFXArchipelagoClient.local_checked_locations.Contains(partyMember_id | (long)FFXArchipelagoClient.ArchipelagoLocationType.PartyMember)) {
                        if (ArchipelagoFFXModule.item_locations.party_member.TryGetValue(partyMember_id, out var item)) {
                            if (FFXArchipelagoClient.sendLocation(partyMember_id, FFXArchipelagoClient.ArchipelagoLocationType.PartyMember)) {
                                ArchipelagoFFXModule.obtain_item(item.id);
                            }
                        }
                    }
                } } },
                { 210, new() {check_delegate = (r) => {
                    // Send and obtain Map and Brotherhood locations if skipped by CSR
                    int treasure_id = 459;
                    if (!FFXArchipelagoClient.local_checked_locations.Contains(treasure_id | (long)FFXArchipelagoClient.ArchipelagoLocationType.Treasure)) {
                        if (ArchipelagoFFXModule.item_locations.treasure.TryGetValue(treasure_id, out var item)) {
                            if (FFXArchipelagoClient.sendLocation(treasure_id, FFXArchipelagoClient.ArchipelagoLocationType.Treasure)) {
                                ArchipelagoFFXModule.obtain_item(item.id);
                            }
                        }
                    }
                    int other_id = 0;
                    if (!FFXArchipelagoClient.local_checked_locations.Contains(other_id | (long)FFXArchipelagoClient.ArchipelagoLocationType.Other)) {
                        if (ArchipelagoFFXModule.item_locations.other.TryGetValue(other_id, out var item)) {
                            if (FFXArchipelagoClient.sendLocation(other_id, FFXArchipelagoClient.ArchipelagoLocationType.Other)) {
                                ArchipelagoFFXModule.obtain_item(item.id);
                            }
                        }
                    }
                } } },
                { 228, new() {check_delegate = (r) => {ArchipelagoFFXModule.logger.Info("Besaid visit 1 complete"); } } },
                { 248, new() {check_delegate = (r) => {
                    int partyMember_id = 3; // Kimahri
                    if (!FFXArchipelagoClient.local_checked_locations.Contains(partyMember_id | (long)FFXArchipelagoClient.ArchipelagoLocationType.PartyMember)) {
                        if (ArchipelagoFFXModule.item_locations.party_member.TryGetValue(partyMember_id, out var item)) {
                            if (FFXArchipelagoClient.sendLocation(partyMember_id, FFXArchipelagoClient.ArchipelagoLocationType.PartyMember)) {
                                ArchipelagoFFXModule.obtain_item(item.id);
                            }
                        }
                    }
                } } },
                { 290, new() {visit_complete = true, next_story_progress = 3210, next_room_id = 19, next_entrance = 1, return_to_airship = true, check_delegate = (r) => {ArchipelagoFFXModule.logger.Info("S.S Liki visit complete"); } } },
            } } },
        {RegionEnum.Kilika, new(){ story_progress = 304, room_id = 16, entrance = 0, airship_destination_index = 3,
            savedata = [
                new ArchipelagoRegionSaveData(0x03AC, 1, [0]),
            ],
            story_checks = {
                { 348,  new() {pilgrimage = true, check_delegate = (r) => {
                    // Ifrit
                    int partyMember_id = 9;
                    if (!FFXArchipelagoClient.local_checked_locations.Contains(partyMember_id | (long)FFXArchipelagoClient.ArchipelagoLocationType.PartyMember)) {
                        if (ArchipelagoFFXModule.item_locations.party_member.TryGetValue(partyMember_id, out var item)) {
                            if (FFXArchipelagoClient.sendLocation(partyMember_id, FFXArchipelagoClient.ArchipelagoLocationType.PartyMember)) {
                                ArchipelagoFFXModule.obtain_item(item.id);
                            }
                        }
                    }
                } } },
                { 372, new() {check_delegate = (r) => {ArchipelagoFFXModule.logger.Info("Kilika visit 1 complete"); } } },
                { 400, new() {visit_complete = true, next_story_progress = 3210, next_room_id = 98, next_entrance = 3, return_to_airship = true, check_delegate = (r) => {ArchipelagoFFXModule.logger.Info("S.S Winno visit complete"); } } },
                { 402, new() {visit_complete = true, next_story_progress = 3210, next_room_id = 98, next_entrance = 3, return_to_airship = true, check_delegate = (r) => {ArchipelagoFFXModule.logger.Info("S.S Winno visit complete"); } } },
            } } },
        {RegionEnum.Luca, new(){ story_progress = 402, room_id = 267, entrance = 0, airship_destination_index = 4,
            story_checks = {
                { 600, new() {check_delegate = (r) => {
                    // Auron
                    int partyMember_id = 2;
                    if (!FFXArchipelagoClient.local_checked_locations.Contains(partyMember_id | (long)FFXArchipelagoClient.ArchipelagoLocationType.PartyMember)) {
                        if (ArchipelagoFFXModule.item_locations.party_member.TryGetValue(partyMember_id, out var item)) {
                            if (FFXArchipelagoClient.sendLocation(partyMember_id, FFXArchipelagoClient.ArchipelagoLocationType.PartyMember)) {
                                ArchipelagoFFXModule.obtain_item(item.id);
                            }
                        }
                    }
                } } },
                { 730, new() {visit_complete = true, next_story_progress = 3210, next_room_id = 123, next_entrance = 6, return_to_airship = true, 
                    check_delegate = (r) => {
                        ArchipelagoFFXModule.logger.Info("Luca visit complete"); 
                        // CSR workaround
                        Globals.save_data->current_room_id = 123;
                        Globals.save_data->current_spawnpoint = 6;
                    } } },
            } } },
        {RegionEnum.MiihenHighroad, new(){ story_progress = 730, room_id = 95, entrance = 0, airship_destination_index = 5,
            savedata = [
                new ArchipelagoRegionSaveData(0x0285, 1),
                new ArchipelagoRegionSaveData(0x0C3C, 4),
                new ArchipelagoRegionSaveData(0x0C40, 4),
                new ArchipelagoRegionSaveData(0x0C44, 4),
                new ArchipelagoRegionSaveData(0x0C48, 4),
                ],
            story_checks = {
                { 787, new() {visit_complete = true, next_story_progress = 3210, next_room_id = 171, next_entrance = 1, return_to_airship = true, check_delegate = (r) => {ArchipelagoFFXModule.logger.Info("Mi'ihen Highroad visit complete"); } } },
            } } },
        {RegionEnum.MushroomRockRoad, new(){ story_progress = 787, room_id = 79, entrance = 0, airship_destination_index = 6,
            story_checks = {
                { 960, new() {visit_complete = true, next_story_progress = 3210, next_room_id = 131, next_entrance = 3, return_to_airship = true, check_delegate = (r) => {ArchipelagoFFXModule.logger.Info("Mushroom Rock Road visit complete"); } } },
            } } },
        {RegionEnum.Djose, new(){ story_progress = 960, room_id = 93, entrance = 0, airship_destination_index = 7,
            story_checks = {
                { 1010, new() {pilgrimage = true, check_delegate = (r) => {
                    // Ixion
                    int partyMember_id = 10;
                    if (!FFXArchipelagoClient.local_checked_locations.Contains(partyMember_id | (long)FFXArchipelagoClient.ArchipelagoLocationType.PartyMember)) {
                        if (ArchipelagoFFXModule.item_locations.party_member.TryGetValue(partyMember_id, out var item)) {
                            if (FFXArchipelagoClient.sendLocation(partyMember_id, FFXArchipelagoClient.ArchipelagoLocationType.PartyMember)) {
                                ArchipelagoFFXModule.obtain_item(item.id);
                            }
                        }
                    }
                } } },
                { 1030, new() {visit_complete = true, next_story_progress = 3210, next_room_id = 82, next_entrance = 0, return_to_airship = true, check_delegate = (r) => {ArchipelagoFFXModule.logger.Info("Djose visit 1 complete"); } } },
            } } },
        {RegionEnum.Moonflow, new(){ story_progress = 1030, room_id = 75, entrance = 0, airship_destination_index = 8,
            story_checks = {
                { 1085, new() {visit_complete = true, next_story_progress = 3210, next_room_id = 235, next_entrance = 1, return_to_airship = true, 
                    check_delegate = (r) => {
                    // Rikku
                    int partyMember_id = 6;
                    if (!FFXArchipelagoClient.local_checked_locations.Contains(partyMember_id | (long)FFXArchipelagoClient.ArchipelagoLocationType.PartyMember)) {
                        if (ArchipelagoFFXModule.item_locations.party_member.TryGetValue(partyMember_id, out var item)) {
                            if (FFXArchipelagoClient.sendLocation(partyMember_id, FFXArchipelagoClient.ArchipelagoLocationType.PartyMember)) {
                                ArchipelagoFFXModule.obtain_item(item.id);
                            }
                        }
                    }
                    ArchipelagoFFXModule.logger.Info("Moonflow visit complete");
                } } },
            } } },
        {RegionEnum.Guadosalam, new(){ story_progress = 1085, room_id = 135, entrance = 0, airship_destination_index = 9,
            story_checks = {
                { 1170, new() {check_delegate = (r) => {
                    // Send and obtain Brotherhood upgrade location if skipped by CSR
                    int other_id = 37;
                    if (!FFXArchipelagoClient.local_checked_locations.Contains(other_id | (long)FFXArchipelagoClient.ArchipelagoLocationType.Other)) {
                        if (ArchipelagoFFXModule.item_locations.other.TryGetValue(other_id, out var item)) {
                            if (FFXArchipelagoClient.sendLocation(other_id, FFXArchipelagoClient.ArchipelagoLocationType.Other)) {
                                ArchipelagoFFXModule.obtain_item(item.id);
                            }
                        }
                    }
                } } },
                { 1210, new() {visit_complete = true, next_story_progress = 3210, next_room_id = 243, next_entrance = 1, return_to_airship = true, check_delegate = (r) => {ArchipelagoFFXModule.logger.Info("Guadosalam visit complete"); } } },
            } } },
        {RegionEnum.ThunderPlains, new(){ story_progress = 1210, room_id = 140, entrance = 0, airship_destination_index = 10,
            story_checks = {
                { 1375, new() {visit_complete = true, next_story_progress = 3210, next_room_id = 263, next_entrance = 2, return_to_airship = true, check_delegate = (r) => {ArchipelagoFFXModule.logger.Info("Thunder Plains visit complete"); } } },
            } } },
        {RegionEnum.Macalania, new(){ story_progress = 1400, room_id = 110, entrance = 0, airship_destination_index = 11,
            story_checks = {
                { 1430, new() {check_delegate = (r) => {
                    // Send and obtain Jecht Sphere location if skipped by CSR
                    int treasure_id = 177;
                    if (!FFXArchipelagoClient.local_checked_locations.Contains(treasure_id | (long)FFXArchipelagoClient.ArchipelagoLocationType.Treasure)) {
                        if (ArchipelagoFFXModule.item_locations.treasure.TryGetValue(treasure_id, out var item)) {
                            if (FFXArchipelagoClient.sendLocation(treasure_id, FFXArchipelagoClient.ArchipelagoLocationType.Treasure)) {
                                ArchipelagoFFXModule.obtain_item(item.id);
                            }
                        }
                    }
                } } },
                { 1470, new() {check_delegate = (r) => {ArchipelagoFFXModule.logger.Info("Macalania Woods visit complete"); } } },
                { 1530, new() {check_delegate = (r) => {
                    int treasure_id = 85;
                    if (!FFXArchipelagoClient.local_checked_locations.Contains(treasure_id | (long)FFXArchipelagoClient.ArchipelagoLocationType.Treasure)) {
                        if (ArchipelagoFFXModule.item_locations.treasure.TryGetValue(treasure_id, out var item)) {
                            if (FFXArchipelagoClient.sendLocation(treasure_id, FFXArchipelagoClient.ArchipelagoLocationType.Treasure)) {
                                ArchipelagoFFXModule.obtain_item(item.id);
                            }
                        }
                    } } } },
                { 1545, new() {pilgrimage = true, check_delegate = (r) => {
                    // Shiva
                    int partyMember_id = 11;
                    if (!FFXArchipelagoClient.local_checked_locations.Contains(partyMember_id | (long)FFXArchipelagoClient.ArchipelagoLocationType.PartyMember)) {
                        if (ArchipelagoFFXModule.item_locations.party_member.TryGetValue(partyMember_id, out var item)) {
                            if (FFXArchipelagoClient.sendLocation(partyMember_id, FFXArchipelagoClient.ArchipelagoLocationType.PartyMember)) {
                                ArchipelagoFFXModule.obtain_item(item.id);
                            }
                        }
                    }
                } } },
                { 1704, new() {visit_complete = true, next_story_progress = 3210, next_room_id = 215, next_entrance = 1, return_to_airship = true, check_delegate = (r) => {ArchipelagoFFXModule.logger.Info("Lake Macalania visit 1 complete"); } } },
            } } },
        {RegionEnum.Bikanel, new(){ story_progress = 1704, room_id = 129, entrance = 0, airship_destination_index = 12,
            story_checks = {
                { 1940, new() {visit_complete = true, next_story_progress = 3210, next_room_id = 129, next_entrance = 2, return_to_airship = true, check_delegate = (r) => {
                    ArchipelagoFFXModule.logger.Info("Bikanel visit complete");
                    int[] treasure_ids = [362, 363, 364];
                    foreach (int treasure_id in treasure_ids) {
                        if (!FFXArchipelagoClient.local_checked_locations.Contains(treasure_id | (long)FFXArchipelagoClient.ArchipelagoLocationType.Treasure)) {
                            if (ArchipelagoFFXModule.item_locations.treasure.TryGetValue(treasure_id, out var item)) {
                                if (FFXArchipelagoClient.sendLocation(treasure_id, FFXArchipelagoClient.ArchipelagoLocationType.Treasure)) {
                                    ArchipelagoFFXModule.obtain_item(item.id);
                                }
                            }
                        }
                    }
                } } },
                { 1950, new() {visit_complete = true, next_story_progress = 3210, next_room_id = 129, next_entrance = 2, return_to_airship = true, check_delegate = (r) => {
                    ArchipelagoFFXModule.logger.Info("Bikanel visit complete");
                    int[] treasure_ids = [362, 363, 364];
                    foreach (int treasure_id in treasure_ids) {
                        if (!FFXArchipelagoClient.local_checked_locations.Contains(treasure_id | (long)FFXArchipelagoClient.ArchipelagoLocationType.Treasure)) {
                            if (ArchipelagoFFXModule.item_locations.treasure.TryGetValue(treasure_id, out var item)) {
                                if (FFXArchipelagoClient.sendLocation(treasure_id, FFXArchipelagoClient.ArchipelagoLocationType.Treasure)) {
                                    ArchipelagoFFXModule.obtain_item(item.id);
                                }
                            }
                        }
                    }
                } } },
            } } },
        {RegionEnum.Airship, new(){ story_progress = 1950, room_id = 194, entrance = 1, airship_destination_index = 13,
            story_checks = {
                { 2075, new() {visit_complete = true, next_story_progress = 2970, next_room_id = 255, next_entrance = 0, return_to_airship = true, check_delegate = (r) => {ArchipelagoFFXModule.logger.Info("Airship visit 1 complete"); } } },
                //{ 3135, new() {next_story_progress = 3210, next_room_id = 255, next_entrance = 0, return_if_locked = RegionEnum.Sin, check_delegate = (r) => {ArchipelagoFFXModule.logger.Info("Airship visit 2 complete"); } } },
            } } },
        {RegionEnum.Bevelle, new(){ story_progress = 2040, room_id = 205, entrance = 0, airship_destination_index = 14, // Destination 12 doesn't work (12 = Bevelle but doesn't have destination, 18 = Highbridge) 
            story_checks = {
                { 2220, new() {pilgrimage = true, check_delegate = (r) => {
                    // Bahamut
                    int partyMember_id = 12;
                    if (!FFXArchipelagoClient.local_checked_locations.Contains(partyMember_id | (long)FFXArchipelagoClient.ArchipelagoLocationType.PartyMember)) {
                        if (ArchipelagoFFXModule.item_locations.party_member.TryGetValue(partyMember_id, out var item)) {
                            if (FFXArchipelagoClient.sendLocation(partyMember_id, FFXArchipelagoClient.ArchipelagoLocationType.PartyMember)) {
                                ArchipelagoFFXModule.obtain_item(item.id);
                            }
                        }
                    }
                } } },
                { 2385, new() {visit_complete = true, next_story_progress = 2920, next_room_id = 208, next_entrance = 1, return_to_airship = true, check_delegate = (r) => {ArchipelagoFFXModule.logger.Info("Bevelle visit 1 complete"); } } },
                { 2945, new() {visit_complete = true, next_story_progress = 3210, next_room_id = 208, next_entrance = 1, check_delegate = (r) => {ArchipelagoFFXModule.logger.Info("Bevelle visit 2 complete"); } } },
            } } }, 
        {RegionEnum.CalmLands, new(){ story_progress = 2385, room_id = 223, entrance = 0, airship_destination_index = 15,
            savedata = [
                new ArchipelagoRegionSaveData(0x0285, 1),
                new ArchipelagoRegionSaveData(0x0C3C, 4),
                new ArchipelagoRegionSaveData(0x0C40, 4),
                new ArchipelagoRegionSaveData(0x0C44, 4),
                new ArchipelagoRegionSaveData(0x0C48, 4),
                ],
            story_checks = {
                { 2440, new() {visit_complete = true, next_story_progress = 3210, next_room_id = 223, next_entrance = 4, return_to_airship = true, check_delegate = (r) => {ArchipelagoFFXModule.logger.Info("Calm Lands complete"); } } }, // Normally ends at 2440, but CSR skips from 2420 to 2510
                { 2510, new() {visit_complete = true, next_story_progress = 3210, next_room_id = 223, next_entrance = 4, return_to_airship = true, check_delegate = (r) => {ArchipelagoFFXModule.logger.Info("Calm Lands complete"); } } }, // Normally ends at 2440, but CSR skips from 2420 to 2510
            } } },
        {RegionEnum.CavernOfTheStolenFayth, new(){ story_progress = 2420, room_id = 266, entrance = 0, airship_destination_index = 16 } },
        {RegionEnum.MtGagazet, new(){ story_progress = 2440, room_id = 259, entrance = 0, airship_destination_index = 18,
            story_checks = {
                { 2680, new() {visit_complete = true, next_story_progress = 3210, next_room_id = 259, next_entrance = 2, return_to_airship = true, check_delegate = (r) => {ArchipelagoFFXModule.logger.Info("Mt. Gagazet complete"); } } },
            } } },
        {RegionEnum.ZanarkandRuins, new(){ story_progress = 2680, room_id = 132, entrance = 0, airship_destination_index = 19,
            story_checks = {
                { 2850, new() {pilgrimage = true } },
                { 2875, new() {visit_complete = true, next_story_progress = 3210, next_room_id = 313, next_entrance = 3, return_to_airship = true, check_delegate = (r) => {ArchipelagoFFXModule.logger.Info("Zanarkand Ruins complete"); } } },
                { 2900, new() {visit_complete = true, next_story_progress = 3210, next_room_id = 313, next_entrance = 3, return_to_airship = true, check_delegate = (r) => {ArchipelagoFFXModule.logger.Info("Zanarkand Ruins complete"); } } },
            } } },
        {RegionEnum.Sin, new(){ story_progress = 3125, room_id = 322, entrance = 2, airship_destination_index = 20,
            story_checks = {
                { 3400, new() {visit_complete = true, next_room_id = 322, next_entrance = 2, check_delegate = (r) => {
                    ArchipelagoFFXModule.logger.Info("Game Complete");
                    foreach (var character in ArchipelagoFFXModule.locked_characters) {
                        ArchipelagoFFXModule.locked_characters[character.Key] = false;
                    }
                } } },
                { 11000, new() {next_story_progress = 3210, next_room_id = 327, next_entrance = 0, check_delegate = (r) => {
                    ArchipelagoFFXModule.call_warp_to_map(382, 0);
                } } },
            } } },
        {RegionEnum.OmegaRuins, new(){ story_progress = 3210, room_id = 258, entrance = 2, airship_destination_index = 21 } }, // Story_progress?
        {RegionEnum.MonsterArena, new(){ story_progress = 3210, room_id = 307, entrance = 0, airship_destination_index = 17 } },
    };

    public static uint[] airship_destination_addresses = [
        0x3CC1,
        0x3CF2,
        0x3D62,
        0x3D93,
        0x3DC4,
        0x3E34,
        0x3E65,
        0x3E96,
        0x3EC7,
        0x3EF8,
        0x3F29,
        0x426F, // 12: doesn't exist in vanilla so pointed at 26 instead
        0x3F99,
        0x3FCA,
        0x3FFB,
        0x4041, // 16: Requires override of GameMoment check
        0x40E5,
        0x4134, // 18: Requires override of GameMoment check
        0x4173,
        0x41B2,
        0x41F1,
        0x3D31,
        0x3E03,
        0x4230,
        0x3F68,
        //0x426F,
    ];

    public static ArchipelagoFFXModule.ManagedCustomString[] airship_destination_names = [
        new(""u8),
        new("Baaj Temple"u8),
        new("Besaid Island"u8),
        new("Kilika Port"u8),
        new("Luca"u8),
        new("Mi'ihen Highroad"u8),
        new("Mushroom Rock Road"u8),
        new("Djose"u8),
        new("Moonflow"u8),
        new("Guadosalam"u8),
        new("Thunder Plains"u8),
        new("Macalania"u8),
        new("Bikanel Desert"u8),
        new("Airship"u8),
        new("Bevelle"u8),
        new("Calm Lands"u8),
        new("Cavern of the Stolen Fayth"u8),
        new("Monster Arena"u8),
        new("Mt. Gagazet"u8),
        new("Zanarkand Ruins"u8),
        new("Sin"u8),
        new("Omega Ruins"u8),
    ];

    // For battles that don't push/pop but should
    public static Dictionary<string, List<int>> encounterToPartyDict => new(){
        {"bjyt02_00", [PlySaveId.PC_TIDUS,
            PlySaveId.PC_WAKKA,
            PlySaveId.PC_RIKKU,
        ]},
        {"bjyt02_01", [PlySaveId.PC_TIDUS,
            PlySaveId.PC_WAKKA,
            PlySaveId.PC_RIKKU,
        ]},
        {"lchb07_00", [ // Auron solo
        ]},
        {"lchb08_00", [ // Luca post-blitzball sahagins
        ]},
    };

    public static Dictionary<string, Action> encounterVictoryActions => new() {
        // Evrae airship battle
        //{"hiku15_00", () => {
        //    ArchipelagoFFXModule.logger.Info("Defeated Evrae. Airship visit 1 complete");
        //    ArchipelagoFFXModule.region_states[RegionEnum.Airship].story_progress = 2970;
        //    ArchipelagoFFXModule.region_states[RegionEnum.Airship].room_id = 255;
        //    ArchipelagoFFXModule.region_states[RegionEnum.Airship].entrance = 0;
        //    ArchipelagoFFXModule.skip_state_updates = true;
        //    }
        //},
        // Overdrive Sin
        {
            "ssbt03_00", () => {
            ArchipelagoFFXModule.logger.Info("Defeated Overdrive Sin. Airship visit 2 complete");
            ArchipelagoFFXModule.region_states[RegionEnum.Airship].story_progress = 3210;
            ArchipelagoFFXModule.region_states[RegionEnum.Airship].room_id = 374;
            ArchipelagoFFXModule.region_states[RegionEnum.Airship].entrance = 1;
            ArchipelagoFFXModule.skip_state_updates = true;
            }
        },
        // Yojimbo (Maybe on recruiting Yojimbo instead?)
        //{"nagi05_10", () => {
        //    ArchipelagoFFXModule.logger.Info("Defeated Yojimbo. Cavern of the Stolen Fayth visit 1 complete");
        //    ArchipelagoFFXModule.region_states[RegionEnum.CavernOfTheStolenFayth].story_progress = 3210;
        //    ArchipelagoFFXModule.region_states[RegionEnum.CavernOfTheStolenFayth].room_id = 56;
        //    ArchipelagoFFXModule.region_states[RegionEnum.CavernOfTheStolenFayth].entrance = 0;
        //    ArchipelagoFFXModule.skip_state_updates = true;
        //    }
        //},
        // Yu Yevon
        //{
        //    "sins07_10", () => {
        //
        //    }
        //}

        // Bikanel forced Zu fight
        {"bika00_10", () =>  FFXArchipelagoClient.current_session?.DataStorage[Scope.Slot, "FFX_LOGIC_ZU"] = true },
    };

    public static Dictionary<string, Action> encounterEscapeActions => new() {
        // Bikanel forced Zu fight
        {"bika00_10", () =>  FFXArchipelagoClient.current_session?.DataStorage[Scope.Slot, "FFX_LOGIC_ZU"] = true },
        {"mihn02_00", () => {
            int boss_id = 8; // Chocobo Eater
            if (!FFXArchipelagoClient.local_checked_locations.Contains(boss_id | (long)FFXArchipelagoClient.ArchipelagoLocationType.Boss)) {
                if (ArchipelagoFFXModule.item_locations.party_member.TryGetValue(boss_id, out var item)) {
                    if (FFXArchipelagoClient.sendLocation(boss_id, FFXArchipelagoClient.ArchipelagoLocationType.Boss)) {
                        ArchipelagoFFXModule.obtain_item(item.id);
                    }
                }
            }
        } },
    };

    public static Dictionary<string, Action> encounterToActionDict => new(){
        //{"bjyt04_00", ArchipelagoFFXModule.reset_party }, // Klikk (solo Tidus)
        //{"bjyt04_01", ArchipelagoFFXModule.reset_party }, // Klikk (Tidus + Rikku)
        
        // Auron solo
        {"lchb07_00", () => ArchipelagoFFXModule.set_party([PlySaveId.PC_AURON], true, false) },
        // Yenke and Biran. Can only target Kimahri and scale on his stats.
        {"mtgz01_10", () => ArchipelagoFFXModule.set_party([PlySaveId.PC_KIMAHRI], true, false) },

        // Tutorials
        // Will softlock without Lulu
        {"bsil07_51", () => {
            int partyMember_id = 5; // Lulu
            if (!FFXArchipelagoClient.local_checked_locations.Contains(partyMember_id | (long)FFXArchipelagoClient.ArchipelagoLocationType.PartyMember)) {
                if (ArchipelagoFFXModule.item_locations.party_member.TryGetValue(partyMember_id, out var item)) {
                    if (FFXArchipelagoClient.sendLocation(partyMember_id, FFXArchipelagoClient.ArchipelagoLocationType.PartyMember)) {
                        ArchipelagoFFXModule.obtain_item(item.id);
                    }
                }
            }
            ArchipelagoFFXModule.set_party([PlySaveId.PC_TIDUS, PlySaveId.PC_WAKKA, PlySaveId.PC_LULU], true, false);
        } },
        // Yuna is unable to act without an aeon, causing a softlock.
        {"bsil05_50", () => {
            int partyMember_id = 1; // Yuna
            if (!FFXArchipelagoClient.local_checked_locations.Contains(partyMember_id | (long)FFXArchipelagoClient.ArchipelagoLocationType.PartyMember)) {
                if (ArchipelagoFFXModule.item_locations.party_member.TryGetValue(partyMember_id, out var item)) {
                    if (FFXArchipelagoClient.sendLocation(partyMember_id, FFXArchipelagoClient.ArchipelagoLocationType.PartyMember)) {
                        ArchipelagoFFXModule.obtain_item(item.id);
                    }
                }
            }
            ArchipelagoFFXModule.set_party([PlySaveId.PC_LULU, PlySaveId.PC_TIDUS, PlySaveId.PC_WAKKA, PlySaveId.PC_YUNA, PlySaveId.PC_VALEFOR], true, false);
        } },

        // Gui 2
        {"kino03_10", () => {
            int partyMember_id = 7; // Seymour
            if (!FFXArchipelagoClient.local_checked_locations.Contains(partyMember_id | (long)FFXArchipelagoClient.ArchipelagoLocationType.PartyMember)) {
                if (ArchipelagoFFXModule.item_locations.party_member.TryGetValue(partyMember_id, out var item)) {
                    if (FFXArchipelagoClient.sendLocation(partyMember_id, FFXArchipelagoClient.ArchipelagoLocationType.PartyMember)) {
                        ArchipelagoFFXModule.obtain_item(item.id);
                    }
                }
            }
        } },

        // Tidus only gets 1 turn?
        {"klyt00_50", () => ArchipelagoFFXModule.set_party([PlySaveId.PC_TIDUS, PlySaveId.PC_KIMAHRI, PlySaveId.PC_LULU], true, false) },

        // Piercing tutorial. Tidus only gets 1 turn. Enemy only attacks 3rd character?
        {"mihn00_50", () => ArchipelagoFFXModule.set_party([PlySaveId.PC_TIDUS, PlySaveId.PC_AURON, PlySaveId.PC_WAKKA], true, false) },

        // Rikku tutorial
        {"genk16_50", () => ArchipelagoFFXModule.set_party([PlySaveId.PC_RIKKU], true, false) },

        // Summon fights
        // Belgemine
        {"genk00_40",  () => ArchipelagoFFXModule.set_summon_party()},
        {"kino04_40",  () => ArchipelagoFFXModule.set_summon_party()},
        {"lmyt01_00",  () => ArchipelagoFFXModule.set_summon_party()},
        {"lmyt01_01",  () => ArchipelagoFFXModule.set_summon_party()},
        {"lmyt01_02",  () => ArchipelagoFFXModule.set_summon_party()},
        {"lmyt01_03",  () => ArchipelagoFFXModule.set_summon_party()},
        {"lmyt01_04",  () => ArchipelagoFFXModule.set_summon_party()},
        {"lmyt01_05",  () => ArchipelagoFFXModule.set_summon_party()},
        {"lmyt01_06",  () => ArchipelagoFFXModule.set_summon_party()},
        {"lmyt01_07",  () => ArchipelagoFFXModule.set_summon_party()},
        {"mihn00_60",  () => ArchipelagoFFXModule.set_summon_party()},
        {"nagi00_40",  () => ArchipelagoFFXModule.set_summon_party()},
        {"zzzz00_250", () => ArchipelagoFFXModule.set_summon_party()},
        {"zzzz02_85",  () => ArchipelagoFFXModule.set_summon_party()},
        // Isaaru
        {"bvyt09_10",  () => ArchipelagoFFXModule.set_summon_party()},
        {"bvyt09_11",  () => ArchipelagoFFXModule.set_summon_party()},
        {"bvyt09_12",  () => ArchipelagoFFXModule.set_summon_party()},
        {"zzzz00_248", () => ArchipelagoFFXModule.set_summon_party()},


        // Underwater fights
        // Luca post-blitzball underwater fight
        {"lchb08_00", () => {
            ArchipelagoFFXModule.set_party([PlySaveId.PC_TIDUS, PlySaveId.PC_WAKKA]); // Only 2. Third character stays in original spot when battle moves forward
        } },
        // Extractor. May not work correctly without Yuna?
        {"genk09_00", () => {
            ArchipelagoFFXModule.set_party([PlySaveId.PC_TIDUS, PlySaveId.PC_WAKKA, PlySaveId.PC_YUNA, PlySaveId.PC_RIKKU]);
        } },
        // Baaj. Should work with 3
        {"bjyt02_00", () => ArchipelagoFFXModule.set_underwater_party()},
        {"bjyt02_01", () => ArchipelagoFFXModule.set_underwater_party()},
        {"bjyt02_02", () => ArchipelagoFFXModule.set_underwater_party()},
        // Al Bhed Ship. May only work with 2
        {"cdsp00_00", () => ArchipelagoFFXModule.set_underwater_party()},
        {"cdsp00_01", () => ArchipelagoFFXModule.set_underwater_party()},
        {"cdsp00_02", () => ArchipelagoFFXModule.set_underwater_party()},
        {"cdsp01_00", () => ArchipelagoFFXModule.set_underwater_party()},
        {"cdsp01_01", () => ArchipelagoFFXModule.set_underwater_party()},
        {"cdsp01_02", () => ArchipelagoFFXModule.set_underwater_party()}, // Not confirmed to exist but cdsp01_XX are probably copies of cdsp00_XX
        {"cdsp07_00", () => ArchipelagoFFXModule.set_underwater_party()},
        {"cdsp07_01", () => ArchipelagoFFXModule.set_underwater_party()},
        // Gagazet. Probably the underwater fights
        {"mtgz07_00", () => ArchipelagoFFXModule.set_underwater_party()},
        {"mtgz07_01", () => ArchipelagoFFXModule.set_underwater_party()},
        {"mtgz07_02", () => ArchipelagoFFXModule.set_underwater_party()},
        {"mtgz07_03", () => ArchipelagoFFXModule.set_underwater_party()},
        // Via Purifico
        {"stbv00_00", () => ArchipelagoFFXModule.set_underwater_party()},
        {"stbv00_01", () => ArchipelagoFFXModule.set_underwater_party()},
        {"stbv00_02", () => ArchipelagoFFXModule.set_underwater_party()},
        {"stbv00_03", () => ArchipelagoFFXModule.set_underwater_party()},
        {"stbv00_04", () => ArchipelagoFFXModule.set_underwater_party()},
        {"stbv00_10", () => ArchipelagoFFXModule.set_underwater_party()},
        {"stbv00_11", () => ArchipelagoFFXModule.set_underwater_party()},
        {"stbv00_12", () => ArchipelagoFFXModule.set_underwater_party()},
        // Besaid with Wakka
        {"bsil03_00", () => ArchipelagoFFXModule.set_underwater_party()},
        {"bsil03_01", () => ArchipelagoFFXModule.set_underwater_party()},
        {"bsil03_02", () => ArchipelagoFFXModule.set_underwater_party()},
        {"bsil03_03", () => ArchipelagoFFXModule.set_underwater_party()},
        // S.S Liki
        {"slik02_01", () => ArchipelagoFFXModule.set_underwater_party()},
        
        // Monster Arena
        {"zzzz00_52", () => ArchipelagoFFXModule.set_underwater_party()},
        {"zzzz00_54", () => ArchipelagoFFXModule.set_underwater_party()},
        {"zzzz00_73", () => ArchipelagoFFXModule.set_underwater_party()},
        {"zzzz02_83", () => ArchipelagoFFXModule.set_underwater_party()},
    };


    public static Dictionary<string, int[]> encounterToLocationDict = new(){
        {"bjyt04_01",   [0]}, // Baaj Temple: Klikk Defeated
        {"cdsp07_00",   [1]}, // Al Bhed Ship: Tros Defeated
        {"bsil07_70",   [2]}, // Besaid: Dark Valefor
        {"slik02_00",   [3]}, // S.S. Liki: Sin Fin
        {"slik02_01",   [4]}, // S.S. Liki: Sinspawn Echuilles
        {"klyt00_00",   [5]}, // Kilika: Lord Ochu
        {"klyt01_00",   [6]}, // Kilika: Sinspawn Geneaux
        {"cdsp02_00",   [7]}, // Luca: Oblitzerator defeated
        {"mihn02_00",   [8]}, // Mi'Hen Highroad: Chocobo Eater
        {"kino02_00",   [9]}, // Mushroom Rock Road: Sinspawn Gui 2
        {"kino03_10",  [10]}, // Mushroom Rock Road: Sinspawn Gui
        {"genk09_00",  [12]}, // Moonflow: Extractor
        {"kami03_71",  [13]}, // Thunder Plains: Dark Ixion
        {"mcfr03_00",  [14]}, // Macalania Woods: Spherimorph
        {"maca02_00",  [15]}, // Lake Macalania: Crawler
        {"mcyt06_00",  [16]}, // Lake Macalania: Seymour/Anima
        {"maca02_01",  [17]}, // Lake Macalania: Wendigo
        {"mcyt00_70",  [18]}, // Lake Macalania: Dark Shiva
        {"bika03_70",  [19]}, // Bikanel: Dark Ifrit
        {"hiku15_00",  [20]}, // Airship: Evrae
        {"ssbt00_00",  [21]}, // Airship: Sin Left Fin
        {"ssbt01_00",  [22]}, // Airship: Sin Right Fin
        {"ssbt02_00",  [23]}, // Airship: Sinspawn Genais and Core
        {"ssbt03_00",  [24]}, // Airship: Overdrive Sin
        {"hiku15_70",  [25]}, // Airship: Penance
        {"bvyt09_12",  [26]}, // Bevelle: Isaaru (probably?)
        {"stbv00_10",  [27]}, // Bevelle: Evrae Altana
        {"stbv01_10",  [28]}, // Bevelle: Seymour Natus
        {"nagi01_00",  [29]}, // Calm Lands: Defender X
        {"nagi05_74",  [31]}, // Cavern of the Stolen Fayth: Dark Yojimbo
        {"mtgz01_10",  [32]}, // Gagazet (Outside): Biran and Yenke
        {"mtgz02_00",  [33]}, // Gagazet (Outside): Seymour Flux
        {"mtgz01_70",  [34]}, // Gagazet (Outside): Dark Anima
        {"mtgz08_00",  [35]}, // Gagazet: Sanctuary Keeper
        {"dome02_00",  [36]}, // Zanarkand: Spectral Keeper
        {"dome06_00",  [37]}, // Zanarkand: Yunalesca
        {"dome06_70",  [38]}, // Zanarkand: Dark Bahamut
        {"sins03_00",  [39]}, // Sin: Seymour Omnis
        {"sins06_00",  [40]}, // Sin: Braska's Final Aeon
        {"sins07_0x",  [41]}, // Sin: Contest of Aeons
        {"sins07_10",  [42]}, // Sin: Yu Yevon
        {"omeg00_10",  [43]}, // Omega Ruins: Ultima Weapon
        {"omeg01_10",  [44]}, // Omega Ruins: Omega Weapon
        {"kino00_70",  [45, 46, 47]}, // Dark Mindy, Dark Sandy, Dark Cindy
        {"kino01_70",  [45, 46, 47]}, // Dark Mindy, Dark Sandy, Dark Cindy
        {"kino01_72",  [45, 46]}, // Dark Mindy, Dark Sandy
        {"kino05_71",  [45]}, // Dark Mindy
        {"kino05_70",  [46]}, // Dark Sandy
        {"kino01_71",  [47]}, // Dark Cindy
        {"bjyt02_02",  [48]}, // Geosgaeno
        {"zzzz03_00",  [49]}, // Stratoavis
        {"zzzz03_04",  [50]}, // Malboro Menace
        {"zzzz03_05",  [51]}, // Kottos
        {"zzzz03_09",  [52]}, // Coeurlregina
        {"zzzz03_11",  [53]}, // Jormungand
        {"zzzz03_14",  [54]}, // Cactuar King
        {"zzzz03_18",  [55]}, // Espada
        {"zzzz03_01",  [56]}, // Abyss Worm
        {"zzzz03_08",  [57]}, // Chimerageist
        {"zzzz03_17",  [58]}, // Don Tonberry
        {"zzzz03_07",  [59]}, // Catoblepas
        {"zzzz03_12",  [60]}, // Abaddon
        {"zzzz03_15",  [61]}, // Vorban
        {"zzzz02_94",  [62]}, // Fenrir
        {"zzzz02_96",  [63]}, // Ornitholestes
        {"zzzz02_97",  [64]}, // Pteryx
        {"zzzz02_98",  [65]}, // Hornet
        {"zzzz02_93",  [66]}, // Vidatu
        {"zzzz02_99",  [67]}, // One-Eye
        {"zzzz02_95",  [68]}, // Jumbo Flan
        {"zzzz03_06",  [69]}, // Nega Elemental
        {"zzzz02_92",  [70]}, // Tanket
        {"zzzz03_03",  [71]}, // Fafnir
        {"zzzz03_16",  [72]}, // Sleep Sprout
        {"zzzz03_13",  [73]}, // Bomb King
        {"zzzz03_02",  [74]}, // Juggernaut
        {"zzzz03_10",  [75]}, // Ironclad
        {"zzzz02_79",  [76]}, // Earth Eater
        {"zzzz02_81",  [77]}, // Greater Sphere
        {"zzzz02_77",  [78]}, // Catastrophe
        {"zzzz02_82",  [79]}, // Th'uban
        {"zzzz00_105", [80]}, // Neslug
        {"zzzz02_80",  [81]}, // Ultima Buster
        {"zzzz02_83",  [82]}, // Shinryu
        {"zzzz02_76",  [83]}, // Monster Arena: Nemesis


        //{"kino00_70", 45}, {"kino01_70", 45}, {"kino01_72", 45}, {"kino05_71", 45}, // Dark Mindy
        //{"kino00_70", 46}, {"kino01_70", 46}, {"kino01_72", 46}, {"kino05_70", 46}, // Dark Sandy
        //{"kino00_70", 47}, {"kino01_70", 47}, {"kino01_71", 47},                    // Dark Cindy
    };

    public static int[] voicelines = [
        0x0827A1C2, // ffx_us_voice03: "Stay away from the summoner"
        0x0827B1C3, // ffx_us_voice03: "You're a bad man"
        0x12A29504, // ffx_us_voice07: *Tidus laugh*
        0x12A2A504, // ffx_us_voice07: *Tidus laugh*
        0x12A2C504, // ffx_us_voice07: *Yuna and Tidus laugh*
        0x1AF441C2, // ffx_us_voice11: "Hornless! Hornless!"
        0x1B45F5CB, // ffx_us_voice11: "Those sand-blasted grease monkeys!"
        0x1B1991C1, // ffx_us_voice11: "Ride ze shoopuf?"
        0x1E5911C1, // ffx_us_voice12: "then burn me, boil me, it matters not."
        0x3172248B, // ffx_us_voice20: "Sometimes, when I got a lot on my mind"
        0x4BC8C5CB, // ffx_us_voice_btl: "Who did that!?"
        0x57EA548B, // ffx_us_voice_btl: "Guardians should fight, not think." + Rikku response
        ];

    public static string[] other_item_names = [
        "Customization: Sensor",
        "Customization: Piercing",
        "Customization: First Strike",
        "Customization: Initiative",
        "Customization: Firestrike",
        "Customization: Fire Ward",
        "Customization: Fireproof",
        "Customization: Fire Eater",
        "Customization: Lightningstrike",
        "Customization: Lightning Ward",
        "Customization: Lightningproof",
        "Customization: Lightning Eater",
        "Customization: Waterstrike",
        "Customization: Water Ward",
        "Customization: Waterproof",
        "Customization: Water Eater",
        "Customization: Icestrike",
        "Customization: Ice Ward",
        "Customization: Iceproof",
        "Customization: Ice Eater",
        "Customization: Darktouch",
        "Customization: Darkstrike",
        "Customization: Dark Ward",
        "Customization: Darkproof",
        "Customization: Silencetouch",
        "Customization: Silencestrike",
        "Customization: Silence Ward",
        "Customization: Silenceproof",
        "Customization: Sleeptouch",
        "Customization: Sleepstrike",
        "Customization: Sleep Ward",
        "Customization: Sleepproof",
        "Customization: Poisontouch",
        "Customization: Poisonstrike",
        "Customization: Poison Ward",
        "Customization: Poisonproof",
        "Customization: Stonetouch",
        "Customization: Stonestrike",
        "Customization: Stone Ward",
        "Customization: Stoneproof",
        "Customization: Deathtouch",
        "Customization: Deathstrike",
        "Customization: Death Ward",
        "Customization: Deathproof",
        "Customization: Zombietouch",
        "Customization: Zombiestrike",
        "Customization: Zombie Ward",
        "Customization: Zombieproof",
        "Customization: Slowtouch",
        "Customization: Slowstrike",
        "Customization: Slow Ward",
        "Customization: Slowproof",
        "Customization: Confuse Ward",
        "Customization: Confuseproof",
        "Customization: Berserk Ward",
        "Customization: Berserkproof",
        "Customization: Curseproof",
        "Customization: Distill Power",
        "Customization: Distill Mana",
        "Customization: Distill Speed",
        "Customization: Distill Ability",
        "Customization: SOS NulBlaze",
        "Customization: SOS NulShock",
        "Customization: SOS NulTide",
        "Customization: SOS NulFrost",
        "Customization: SOS Shell",
        "Customization: SOS Protect",
        "Customization: SOS Reflect",
        "Customization: SOS Haste",
        "Customization: SOS Regen",
        "Customization: Auto-Shell",
        "Customization: Auto-Protect",
        "Customization: Auto-Reflect",
        "Customization: Auto-Haste",
        "Customization: Auto-Regen",
        "Customization: Strength +3%",
        "Customization: Strength +5%",
        "Customization: Strength +10%",
        "Customization: Strength +20%",
        "Customization: Magic +3%",
        "Customization: Magic +5%",
        "Customization: Magic +10%",
        "Customization: Magic +20%",
        "Customization: Defense +3%",
        "Customization: Defense +5%",
        "Customization: Defense +10%",
        "Customization: Defense +20%",
        "Customization: Magic Def +3%",
        "Customization: Magic Def +5%",
        "Customization: Magic Def +10%",
        "Customization: Magic Def +20%",
        "Customization: HP +5%",
        "Customization: HP +10%",
        "Customization: HP +20%",
        "Customization: HP +30%",
        "Customization: MP +5%",
        "Customization: MP +10%",
        "Customization: MP +20%",
        "Customization: MP +30%",
        "Customization: Counterattack",
        "Customization: Evade & Counter",
        "Customization: Magic Counter",
        "Customization: Magic Booster",
        "Customization: Alchemy",
        "Customization: Auto-Potion",
        "Customization: Auto-Med",
        "Customization: Auto-Phoenix",
        "Customization: Half MP Cost",
        "Customization: One MP Cost",
        "Customization: Double AP",
        "Customization: Triple AP",
        "Customization: Overdrive to AP",
        "Customization: SOS Overdrive",
        "Customization: Double Overdrive",
        "Customization: Triple Overdrive",
        "Customization: Gillionaire",
        "Customization: Pickpocket",
        "Customization: Master Thief",
        "Customization: HP Stroll",
        "Customization: MP Stroll",
        "Customization: No Encounters",
        "Customization: Break MP Limit",
        "Customization: Break HP Limit",
        "Customization: Break Damage Limit",
        "Customization: Ribbon",
        ];
}
