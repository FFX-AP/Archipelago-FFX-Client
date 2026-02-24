using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Helpers;
using Archipelago.MultiClient.Net.MessageLog.Messages;
using Archipelago.MultiClient.Net.MessageLog.Parts;
using Archipelago.MultiClient.Net.Models;
using Fahrenheit.Core;
using Fahrenheit.Core.FFX;
using Fahrenheit.Modules.ArchipelagoFFX.GUI;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using TerraFX.Interop.Windows;
using static Fahrenheit.Core.FFX.Globals;
using static Fahrenheit.Modules.ArchipelagoFFX.delegates;

namespace Fahrenheit.Modules.ArchipelagoFFX.Client;
public static class FFXArchipelagoClient {
    public static readonly System.Threading.Lock client_lock = new();
    public static          ArchipelagoSession?   current_session;
    public static          int                   received_items = 0;
    public static readonly HashSet<long>         local_checked_locations = [];
    public static          bool                  local_locations_updated = false;
    public static          bool                  remote_locations_updated = false;
    public static          string?               SeedId = null;
    
    public static PlayerInfo? active_player => current_session?.Players.ActivePlayer;
    private static bool is_disconnecting = false;
    public static bool is_connected => current_session is not null & !is_disconnecting;

    public static async Task Connect(string server, string user, string password) {
        ArchipelagoFFXModule.logger.Debug("Connect");
        LoginResult? login_result = new LoginFailure("");
        ArchipelagoSession? session = null;
        if (is_disconnecting) return;

        try {
            session = ArchipelagoSessionFactory.CreateSession(server);
            connectHandlers(session);
            var roomInfoPacket = await session.ConnectAsync();
            
            login_result = await session.LoginAsync("Final Fantasy X", user, ItemsHandlingFlags.RemoteItems, Version.Parse("0.6.0"), password: password, requestSlotData: true);
        }
        catch (Exception e) {
            login_result = new LoginFailure(e.GetBaseException().Message);
        }

        if (!login_result.Successful) {
            LoginFailure failure = (LoginFailure)login_result;
            string errorMessage = $"Failed to Connect to {server} as {user}:";
            foreach (string error in failure.Errors) {
                errorMessage += $"\n    {error}";
            }
            foreach (ConnectionRefusedError error in failure.ErrorCodes) {
                errorMessage += $"\n    {error}";
            }
            current_session = null;
            ArchipelagoFFXModule.logger.Error(errorMessage);
            return; // Did not connect, show the user the contents of `errorMessage`
        }
        var loginSuccess = (LoginSuccessful)login_result;

        if (ArchipelagoFFXModule.seed.Options.SeedId is not null) {
            if (ArchipelagoFFXModule.seed.Options.SeedId != (string)loginSuccess.SlotData["SeedId"]) {
                string message = "Loaded seed doesn't match connected slot";
                ArchipelagoGUI.add_log_message([(message, Color.Red)]);
                ArchipelagoFFXModule.logger.Error(message);
                disconnect(session);
                return;
            }
        } else {
            SeedId = (string)loginSuccess.SlotData["SeedId"];
        }
        current_session = session;
    }

    public static void disconnect(ArchipelagoSession? session = null) {
        ArchipelagoFFXModule.logger.Debug("disconnect");
        session ??= current_session;
        if (session is null || is_disconnecting) return;
        is_disconnecting = true;
        session.Socket.DisconnectAsync();
    }

    private static void connectHandlers(ArchipelagoSession session) {
        ArchipelagoFFXModule.logger.Debug("connectHandlers");
        session.MessageLog.OnMessageReceived += MessageLog_OnMessageReceived;
        session.Socket.ErrorReceived += Socket_ErrorReceived;
        session.Socket.SocketOpened += Socket_SocketOpened;
        session.Socket.SocketClosed += Socket_SocketClosed;
        session.Locations.CheckedLocationsUpdated += Locations_CheckedLocationsUpdated;
    }

    private static void Locations_CheckedLocationsUpdated(System.Collections.ObjectModel.ReadOnlyCollection<long> newCheckedLocations) {
        lock (client_lock) {
            remote_locations_updated = true;
        }
    }

    private static void Socket_ErrorReceived(Exception e, string message) {
        ArchipelagoFFXModule.logger.Debug($"Socket Error: {message}");
        ArchipelagoFFXModule.logger.Debug($"Socket Exception: {e.Message}");

        if (e.StackTrace != null)
            foreach (var line in e.StackTrace.Split('\n'))
                ArchipelagoFFXModule.logger.Debug($"    {line}");
        else
            ArchipelagoFFXModule.logger.Debug($"    No stacktrace provided");
    }

    private static void Socket_SocketOpened() {
        ArchipelagoFFXModule.logger.Debug($"Socket Opened: \"{current_session?.Socket.Uri}\"");
    }

    private static void Socket_SocketClosed(string reason) {
        ArchipelagoFFXModule.logger.Debug($"Socket Closed: \"{reason}\"");
        ArchipelagoGUI.add_log_message([($"Disconnected from server ({reason})", Color.Red)]);
        SeedId = null;
        current_session = null;
        is_disconnecting = false;
    }

    public unsafe static void update() {
        /*
        foreach (ItemInfo item in session.Items.AllItemsReceived) {
            ArchipelagoFFXModule.logger.Debug($"received_item: {item.ItemName}");
        }
         */
        if (current_session == null) {
            return;
        }

        // TODO: Check for post-battle/other menu?
        if (ArchipelagoFFXModule.seed.Options.SeedId   == null ||
            Globals.save_data->current_room_id ==  23  || // Main Menu
            Globals.save_data->current_room_id ==   0  || // Tutorial room
            Globals.save_data->current_room_id == 348  || // Intro
            Globals.Battle.btl->battle_state   !=   0)  { // In battle
            return;
        }

        if (current_session.Items.AllItemsReceived.Count > received_items) {
            ArchipelagoFFXModule.logger.Debug("New items received");
            foreach (ItemInfo item in current_session.Items.AllItemsReceived.Skip(received_items)) {
                ArchipelagoFFXModule.logger.Debug($"received_item: {item.ItemName}");
                ArchipelagoFFXModule.obtain_item((uint)item.ItemId);
                received_items++;
            }
        }

        // Possibly unnecessary
        lock (client_lock) {
            if (local_locations_updated) {
                var local_only = local_checked_locations.Except(current_session.Locations.AllLocationsChecked);
                if (local_only.Any()) {
                    current_session.Locations.CompleteLocationChecks(local_only.ToArray());
                    ArchipelagoFFXModule.logger.Debug($"Sent: {string.Join(",", local_only)}");
                }
                local_locations_updated = false;
            }
        }

        lock (client_lock) {
            if (remote_locations_updated) {
                var remote_only = current_session.Locations.AllLocationsChecked.Except(local_checked_locations);
                foreach (long location in remote_only) {
                    if (ArchipelagoFFXModule.item_locations.location_to_item((int)location, out var item)) {
                        ArchipelagoFFXModule.logger.Debug($"Synced remote location: location:{location}, item:{item.name}, player:{item.player}");
                        ArchipelagoFFXModule.obtain_item(item.id);
                    }
                }
                local_checked_locations.UnionWith(remote_only);
                remote_locations_updated = false;
            }
        }

        // TODO: Implement alternate goals
        if (local_checked_locations.Contains(42 | (long)ArchipelagoLocationType.Boss)) {
            // Yu Yevon defeated
            current_session.SetGoalAchieved();
        }

        /*
        var remote_only = session.Locations.AllLocationsChecked.Except(local_checked_locations);
        if (remote_only.Any()) {
            ArchipelagoFFXModule.logger.Debug($"Received Remote Locations: {string.Join(",", remote_only)}");
            foreach (var location in remote_only) {
                // TODO: Distinguish between location types
                var treasure_id = (location - 0x14)/4;
                ArchipelagoFFXModule.logger.Debug($"Location id: {Convert.ToInt32(treasure_id)}");
                ArchipelagoModule.receive_treasure(Convert.ToInt32(treasure_id));
                local_checked_locations.Add(location);
            }
        }
         */


    }

    private static void MessageLog_OnMessageReceived(LogMessage message) {
        var parts = message.Parts;
        List<(string, Color)> messageParts = parts.Select((part) => {
            Color color = part.Color;
            if (part.IsBackgroundColor) {
                color = Color.White;
            }
            else if (part.Color == Color.Black) {
                color = new(128, 128, 128);
            }
            return (part.Text, color);
            }).ToList();
        ArchipelagoGUI.add_log_message(messageParts);
    }

    /*
public unsafe static void connectHandlers() {
   session.Locations.CheckedLocationsUpdated += (newCheckedLocations) => {
       if (Globals.save_data->current_room_id == 23) return;

       var remote_only = newCheckedLocations.Except(local_checked_locations);
       if (remote_only.Any()) {
           ArchipelagoFFXModule.logger.Debug($"Received Remote Locations: {string.Join(",", remote_only)}");
           foreach (var location in remote_only) {
               ArchipelagoFFXModule.logger.Debug($"Location id: {Convert.ToInt32(location)}");
               ArchipelagoModule.receive_treasure(Convert.ToInt32(location));
               local_checked_locations.Add(location);
           }
       }
   };
}
*/

    public enum ArchipelagoLocationType: int {
        Treasure      = 0x1000,
        Boss          = 0x2000,
        PartyMember   = 0x3000,
        Overdrive     = 0x4000,
        OverdriveMode = 0x5000,
        Other         = 0x6000,
        Recruit       = 0x7000,
        SphereGrid    = 0x8000,
        Capture       = 0x9000,
    }

    public static bool sendLocation(long locationId, ArchipelagoLocationType locationType) {
        var absoluteId = locationId | (long)locationType;
        return sendLocation(absoluteId);
    }
    private static bool sendLocation(long locationId) {
        if (!local_checked_locations.Add(locationId)) return false;
        local_locations_updated = true;
        if (is_connected) {
            ArchipelagoFFXModule.logger.Debug(current_session!.Locations.GetLocationNameFromId(locationId) ?? $"Location: {locationId}");
        }
        return true;
    }

}

