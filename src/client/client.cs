using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Helpers;
using Archipelago.MultiClient.Net.MessageLog.Messages;
using Archipelago.MultiClient.Net.Models;
using Archipelago.MultiClient.Net.Packets;
using Fahrenheit.FFX;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Archipelago.MultiClient.Net.BounceFeatures.DeathLink;

using ArchipelagoFFX.GUI;

using Fahrenheit;

namespace ArchipelagoFFX.Client;

[FhLoad(FhGameId.FFX)]
public class ArchipelagoClientModule : FhModule {
    public readonly System.Threading.Lock client_lock = new();
    public          ArchipelagoSession?   current_session;
    public          string?               current_server;
    public          int                   received_items = 0;
    public readonly HashSet<long>         local_checked_locations = [];
    public          bool                  local_locations_updated = false;
    public          bool                  remote_locations_updated = false;
    public          string?               SeedId = null;

    public DeathLinkService? current_death_link_service;

    public PlayerInfo? active_player => current_session?.Players.ActivePlayer;
    private bool is_disconnecting = false;
    public bool is_connected => current_session is not null && !is_disconnecting;

    private FhModuleHandle<ArchipelagoFFXModule> _ffx_interop_handle;
    private ArchipelagoFFXModule? _ffx_interop;

    private FhModuleHandle<ArchipelagoGuiModule> _gui_handle;
    private ArchipelagoGuiModule? _gui;

    private FhModuleHandle<RecentItemsModule> _recent_items_handle;
    private RecentItemsModule? _recent_items;

    private FhModuleHandle<DeathLinkModule> _death_link_handle;
    private DeathLinkModule? _death_link;

    public ArchipelagoClientModule() {
        _ffx_interop_handle = new(this);
        _gui_handle = new(this);
        _recent_items_handle = new(this);
        _death_link_handle = new(this);
    }

    public override bool init(FhModContext mod_context, FileStream global_state_file) {
        return _ffx_interop_handle.try_get_module(out _ffx_interop)
            && _gui_handle.try_get_module(out _gui)
            && _recent_items_handle.try_get_module(out _recent_items)
            && _death_link_handle.try_get_module(out _death_link);
    }

    public async Task Connect(string server, string user, string password) {
        _logger.Debug("Connect");
        LoginResult? login_result = new LoginFailure("");
        ArchipelagoSession? session = null;
        DeathLinkService? death_link = null;

        if (is_disconnecting) return;

        try {
            session = ArchipelagoSessionFactory.CreateSession(server);
            death_link = session.CreateDeathLinkService();
            connectHandlers(session, death_link);
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
            _logger.Error(errorMessage);
            return; // Did not connect, show the user the contents of `errorMessage`
        }
        var loginSuccess = (LoginSuccessful)login_result;

        if (ArchipelagoFFXModule.seed.Options.SeedId is not null) {
            if (ArchipelagoFFXModule.seed.Options.SeedId != (string)loginSuccess.SlotData["SeedId"]) {
                string message = "Loaded seed doesn't match connected slot";
                _gui!.add_log_message([(message, Color.Red)]);
                _logger.Error(message);
                disconnect(session);
                return;
            }
            ArchipelagoFFXModule.SeedToServer[ArchipelagoFFXModule.seed.Options.SeedId] = server;
            _ffx_interop!.save_global_state();
        } else {
            SeedId = (string)loginSuccess.SlotData["SeedId"];
            int selected_seed = ArchipelagoFFXModule.loaded_seeds.FindIndex(x => x.Options.SeedId == SeedId);
            if (selected_seed != -1) {
                _gui!.selected_seed = selected_seed;
                ArchipelagoFFXModule.SeedToServer[SeedId] = server;
                _ffx_interop!.save_global_state();
            }
        }
        current_server = server;
        current_session = session;
        current_death_link_service = death_link;
    }

    public void disconnect(ArchipelagoSession? session = null) {
        _logger.Debug("disconnect");
        lock (client_lock) {
            session ??= current_session;
            if (session is null || is_disconnecting) return;
            is_disconnecting = true;
            disconnectHandlers(session, current_death_link_service);
            session.Socket.DisconnectAsync();
        }
    }

    private void connectHandlers(ArchipelagoSession session, DeathLinkService death_link) {
        _logger.Debug("connectHandlers");

        session.MessageLog.OnMessageReceived += MessageLog_OnMessageReceived;
        session.Socket.ErrorReceived += Socket_ErrorReceived;
        session.Socket.SocketOpened += Socket_SocketOpened;
        session.Socket.SocketClosed += Socket_SocketClosed;
        session.Locations.CheckedLocationsUpdated += Locations_CheckedLocationsUpdated;

        session.MessageLog.OnMessageReceived += RecentItemsModule.post_item_message;

        death_link.OnDeathLinkReceived += _death_link!.post_deathlink;
    }

    private void disconnectHandlers(ArchipelagoSession? session, DeathLinkService? death_link) {
        _logger.Debug("disconnectHandlers");

        session?.MessageLog.OnMessageReceived -= MessageLog_OnMessageReceived;
        session?.Socket.ErrorReceived -= Socket_ErrorReceived;
        session?.Socket.SocketOpened -= Socket_SocketOpened;
        session?.Socket.SocketClosed -= Socket_SocketClosed;
        session?.Locations.CheckedLocationsUpdated -= Locations_CheckedLocationsUpdated;

        session?.MessageLog.OnMessageReceived -= RecentItemsModule.post_item_message;

        death_link?.OnDeathLinkReceived -= _death_link!.post_deathlink;
    }

    private void Locations_CheckedLocationsUpdated(System.Collections.ObjectModel.ReadOnlyCollection<long> newCheckedLocations) {
        lock (client_lock) {
            remote_locations_updated = true;
        }
    }

    private void Socket_ErrorReceived(Exception e, string message) {
        _logger.Debug($"Socket Error: {message}");
        _logger.Debug($"Socket Exception: {e.Message}");

        if (e.StackTrace != null)
            foreach (var line in e.StackTrace.Split('\n'))
                _logger.Debug($"    {line}");
        else
            _logger.Debug("    No stacktrace provided");
    }

    private void Socket_SocketOpened() {
        _logger.Debug($"Socket Opened: \"{current_session?.Socket.Uri}\"");
    }

    private void Socket_SocketClosed(string reason) {
        _logger.Debug($"Socket Closed: \"{reason}\"");
        _gui!.add_log_message([($"Disconnected from server ({reason})", Color.Red)]);
        lock (client_lock) {
            current_session = null;
            current_death_link_service = null;
            SeedId = null;
            current_server = null;
            is_disconnecting = false;
        }
    }

    public unsafe void update() {
        lock (client_lock) {
            // TODO: Check for post-battle/other menu?
            if (   !is_connected
                || ArchipelagoFFXModule.seed.Options.SeedId == null
                || Globals.save_data->current_room_id ==  23 // Main Menu
                || Globals.save_data->current_room_id ==   0 // Tutorial room
                || Globals.save_data->current_room_id == 348 // Intro
                || Globals.Battle.btl->battle_state   !=   0)  { // In battle
                return;
            }

            if (current_session!.Items.AllItemsReceived.Count > received_items) {
                _logger.Debug("New items received");
                foreach (ItemInfo item in current_session.Items.AllItemsReceived.Skip(received_items)) {
                    _logger.Debug($"received_item: {item.ItemName}");
                    _ffx_interop!.obtain_item((uint)item.ItemId);
                    received_items++;
                }
            }

            if (local_locations_updated) {
                var local_only = local_checked_locations.Except(current_session.Locations.AllLocationsChecked);
                if (local_only.Any()) {
                    current_session.Locations.CompleteLocationChecksAsync(local_only.ToArray());
                    _logger.Debug($"Sent: {string.Join(",", local_only)}");
                }
                local_locations_updated = false;

                current_session.DataStorage.GetClientStatusAsync().ContinueWith(status => {
                    lock (client_lock) {
                        if (status.Result != ArchipelagoClientState.ClientGoal && is_connected) {
                            bool has_goaled = ArchipelagoFFXModule.seed.Options.Goal switch {
                                ArchipelagoData.Goal.YuYevon => local_checked_locations.Contains(42 | (long)ArchipelagoLocationType.Boss),
                                ArchipelagoData.Goal.Nemesis => local_checked_locations.Contains(83 | (long)ArchipelagoLocationType.Boss),
                                _ => false,
                            };
                            if (has_goaled) {
                                current_session.SetGoalAchieved();
                            }
                        }
                    }
                });
            }

            if (remote_locations_updated) {
                var remote_only = current_session.Locations.AllLocationsChecked.Except(local_checked_locations);
                foreach (long location in remote_only) {
                    if (ArchipelagoFFXModule.item_locations.location_to_item((int)location, out var item)) {
                        _logger.Debug($"Synced remote location: location:{location}, item:{item.name}, player:{item.player}");
                        _ffx_interop!.obtain_item(item.id);
                    }
                }
                local_checked_locations.UnionWith(remote_only);
                remote_locations_updated = false;
            }
        }
    }

    private void MessageLog_OnMessageReceived(LogMessage message) {
        var parts = message.Parts;
        List<(string, Color)> messageParts = parts.Select(part => {
            Color color = part.Color;
            if (part.IsBackgroundColor) {
                color = Color.White;
            }
            else if (part.Color == Color.Black) {
                color = new(128, 128, 128);
            }
            return (part.Text, color);
            }).ToList();
        _gui!.add_log_message(messageParts);
    }

    public void SayAsync(string message)
    {
        lock (client_lock) {
            if (is_connected) {
                current_session!.Socket.SendPacketAsync(new SayPacket { Text = message });
            }
        }
    }

    public enum ArchipelagoLocationType {
        Treasure      = 0x1000,
        Boss          = 0x2000,
        Overdrive     = 0x3000,
        OverdriveMode = 0x5000,
        Other         = 0x6000,
        Recruit       = 0x7000,
        SphereGrid    = 0x8000,
        Capture       = 0x9000,
        PartyMember   = 0xF000,
    }

    public bool sendLocation(long locationId, ArchipelagoLocationType locationType) {
        var absoluteId = locationId | (long)locationType;
        return sendLocation(absoluteId);
    }

    private bool sendLocation(long locationId) {
        if (!local_checked_locations.Add(locationId)) return false;
        local_locations_updated = true;
        lock (client_lock) {
            if (is_connected) {
                _logger.Debug(current_session!.Locations.GetLocationNameFromId(locationId) ?? $"Location: {locationId}");
            }
        }
        return true;
    }
}
