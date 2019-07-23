using System;
using System.Collections.Generic;
using System.Linq;
using fr34kyn01535.Kits;
using fr34kyn01535.Uconomy;
using Freenex.FeexRanks.Configuration;
using Rocket.API;
using Rocket.API.Collections;
using Rocket.Core;
using Rocket.Core.Plugins;
using Rocket.Unturned;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Events;
using Rocket.Unturned.Player;
using SDG.Unturned;
using Steamworks;
using UnityEngine;
using DatabaseManager = Freenex.FeexRanks.Database.DatabaseManager;
using Level = Freenex.FeexRanks.Configuration.Level;
using Logger = Rocket.Core.Logging.Logger;

namespace Freenex.FeexRanks
{
    public class FeexRanks : RocketPlugin<FeexRanksConfiguration>
    {
        public DatabaseManager FeexRanksDatabase;
        public static FeexRanks Instance;
        public static Dictionary<CSteamID, int> DicPoints = new Dictionary<CSteamID, int>();
        public Color configNotificationColor;
        public Color configNotificationColorGlobal;
        public Color configNotificationColorJoinLeaveGlobal;

        public override TranslationList DefaultTranslations =>
            new TranslationList
            {
                {"general_onjoin", "[{2}] {3} ({0} points, rank {1}) connected to the server."},
                {"general_onleave", "[{2}] {3} ({0} points, rank {1}) disconnected from the server."},
                {"general_not_found", "Player not found."},
                {"general_invalid_parameter", "Invalid parameter."},
                {"rank_self", "Your current rank: {1} with {0} points [{2}]"},
                {"rank_other", "{3}'s current rank: {1} with {0} points [{2}]"},
                {"list_1", "The top 3 players:"},
                {"list_2", "{1}st: [{2}] {3} ({0} points)"},
                {"list_3", "{1}nd: [{2}] {3} ({0} points)"},
                {"list_4", "{1}rd: [{2}] {3} ({0} points)"},
                {"list_search", "Rank {1}: [{2}] {3} ({0} points)"},
                {"list_search_not_found", "Rank not found."},
                {"points_reset_player", "Your points have been reset."},
                {"points_reset_caller", "{0}'s points have been reset."},
                {"points_set_player", "Your points have been set to {0}."},
                {"points_set_caller", "{1}'s points have been set to {0}."},
                {"points_add_player", "You received {0} points."},
                {"points_add_caller", "You sent {0} points to {1}."},
                {"points_remove_player", "You lost {0} points."},
                {"points_remove_caller", "You removed {0} points from {1}."},
                {"level_up", "You went up: {1} with {0} points."},
                {"level_up_kit", "You went up and received the kit {0}."},
                {"level_up_rank", "You went up and recieved the permission rank {0}."},
                {"level_up_uconomy", "You went up and received {0}."},
                {"level_up_global", "{2} went up: {1} with {0} points."},
                {"event_ACCURACY", "You received {0} points. ({1} points)"},
                {"event_ARENA_WINS", "You received {0} points. ({1} points)"},
                {"event_DEATHS_PLAYERS", "You received {0} points. ({1} points)"},
                {"event_FOUND_BUILDABLES", "You received {0} points. ({1} points)"},
                {"event_FOUND_CRAFTS", "You received {0} points. ({1} points)"},
                {"event_FOUND_EXPERIENCE", "You received {0} points. ({1} points)"},
                {"event_FOUND_FISHES", "You received {0} points. ({1} points)"},
                {"event_FOUND_ITEMS", "You received {0} points. ({1} points)"},
                {"event_FOUND_PLANTS", "You received {0} points. ({1} points)"},
                {"event_FOUND_RESOURCES", "You received {0} points. ({1} points)"},
                {"event_FOUND_THROWABLES", "You received {0} points. ({1} points)"},
                {"event_HEADSHOTS", "You received {0} points. ({1} points)"},
                {"event_KILLS_ANIMALS", "You received {0} points. ({1} points)"},
                {"event_KILLS_PLAYERS", "You received {0} points. ({1} points)"},
                {"event_KILLS_ZOMBIES_MEGA", "You received {0} points. ({1} points)"},
                {"event_KILLS_ZOMBIES_NORMAL", "You received {0} points. ({1} points)"},
                {"event_NONE", "You received {0} points. ({1} points)"},
                {"event_TRAVEL_FOOT", "You received {0} points. ({1} points)"},
                {"event_TRAVEL_VEHICLE", "You received {0} points. ({1} points)"}
            };

        protected override void Load()
        {
            Instance = this;
            FeexRanksDatabase = new DatabaseManager();

            Instance.Configuration.Instance.Level =
                Instance.Configuration.Instance.Level.OrderByDescending(x => x.Points).ToList();
            configNotificationColor =
                UnturnedChat.GetColorFromName(Instance.Configuration.Instance.NotificationColor, Color.green);
            configNotificationColorGlobal =
                UnturnedChat.GetColorFromName(Instance.Configuration.Instance.NotificationColorGlobal, Color.green);
            configNotificationColorJoinLeaveGlobal =
                UnturnedChat.GetColorFromName(Instance.Configuration.Instance.NotificationColorJoinLeaveGlobal,
                    Color.green);

            U.Events.OnPlayerConnected += Events_OnPlayerConnected;
            U.Events.OnPlayerDisconnected += Events_OnPlayerDisconnected;
            UnturnedPlayerEvents.OnPlayerUpdateStat += UnturnedPlayerEvents_OnPlayerUpdateStat;

            Logger.Log("Freenex's FeexRanks has been loaded!");
        }

        protected override void Unload()
        {
            DicPoints.Clear();

            U.Events.OnPlayerConnected -= Events_OnPlayerConnected;
            U.Events.OnPlayerDisconnected -= Events_OnPlayerDisconnected;
            UnturnedPlayerEvents.OnPlayerUpdateStat -= UnturnedPlayerEvents_OnPlayerUpdateStat;

            Logger.Log("Freenex's FeexRanks has been unloaded!");
        }

        private void Events_OnPlayerConnected(UnturnedPlayer player)
        {
            Instance.FeexRanksDatabase.AddUpdatePlayer(player.CSteamID.ToString(), player.DisplayName);
            var rankInfo = Instance.FeexRanksDatabase.GetAccountBySteamId(player.CSteamID.ToString());
            DicPoints.Add(player.CSteamID, int.Parse(rankInfo.Points));

            if (Instance.Configuration.Instance.EnableRankNotificationOnJoin)
                UnturnedChat.Say(player,
                    Instance.Translations.Instance.Translate("rank_self", rankInfo.Points, rankInfo.CurrentRank,
                        Instance.GetLevel(int.Parse(rankInfo.Points)).Name), configNotificationColor);
            if (Instance.Configuration.Instance.EnableRankNotificationOnJoinGlobal)
                UnturnedChat.Say(
                    Instance.Translations.Instance.Translate("general_onjoin", rankInfo.Points, rankInfo.CurrentRank,
                        Instance.GetLevel(int.Parse(rankInfo.Points)).Name, player.DisplayName),
                    configNotificationColorJoinLeaveGlobal);
        }

        private void Events_OnPlayerDisconnected(UnturnedPlayer player)
        {
            var playerExists = DicPoints.TryGetValue(player.CSteamID, out var playerPoints);
            if (playerExists) DicPoints.Remove(player.CSteamID);

            if (Instance.Configuration.Instance.EnableRankNotificationOnLeaveGlobal)
                UnturnedChat.Say(
                    Instance.Translations.Instance.Translate("general_onleave", playerPoints,
                        Instance.FeexRanksDatabase.GetRankBySteamId(player.CSteamID.ToString()),
                        Instance.GetLevel(playerPoints).Name, player.DisplayName),
                    configNotificationColorJoinLeaveGlobal);
        }

        private void UnturnedPlayerEvents_OnPlayerUpdateStat(UnturnedPlayer player, EPlayerStat stat)
        {
            var configEvent = Instance.Configuration.Instance.Events.Find(x => x.EventName == stat.ToString());
            if (configEvent == null) return;

            if (configEvent.Notify)
            {
                var playerExists = DicPoints.TryGetValue(player.CSteamID, out var oldPoints);
                if (playerExists)
                    UnturnedChat.Say(player,
                        Translate("event_" + configEvent.EventName, configEvent.Points,
                            oldPoints + configEvent.Points), configNotificationColor);
            }

            UpdatePoints(player, configEvent.Points);
        }

        public void UpdatePoints(UnturnedPlayer player, int points)
        {
            var playerExists = DicPoints.TryGetValue(player.CSteamID, out var oldPoints);

            if (!playerExists) return;

            Instance.FeexRanksDatabase.AddPoints(player.CSteamID.ToString(), points);
            DicPoints[player.CSteamID] += points;

            var newPoints = oldPoints + points;
            var configLevelOld = GetLevel(oldPoints);
            var configLevelNew = GetLevel(newPoints);

            if (configLevelOld.Name == configLevelNew.Name) return;

            if (Instance.Configuration.Instance.EnableLevelUpNotification)
                UnturnedChat.Say(player, Translate("level_up", newPoints, configLevelNew.Name),
                    configNotificationColor);
            if (Instance.Configuration.Instance.EnableLevelUpNotificationGlobal)
                UnturnedChat.Say(
                    Translate("level_up_global", newPoints, configLevelNew.Name, player.DisplayName),
                    configNotificationColorGlobal);

            if (configLevelNew.KitReward)
                try
                {
                    KitReward(configLevelNew, player);
                }
                catch (Exception ex)
                {
                    Logger.LogError($"Issue occured while giving a kit reward: {ex.Message}");
                }

            if (configLevelNew.PermissionGroupReward)
                try
                {
                    PermissionGroupReward(configLevelNew, player);
                }
                catch (Exception ex)
                {
                    Logger.LogError($"Issue occured while giving a permission group reward: {ex.Message}");
                }

            if (!configLevelNew.UconomyReward) return;

            try
            {
                UconomyReward(configLevelNew, player);
            }
            catch (Exception ex)
            {
                Logger.LogError($"Issue occured while giving a uconomy reward: {ex.Message}");
            }
        }

        public Level GetLevel(int points)
        {
            foreach (var configLevel in Instance.Configuration.Instance.Level)
                if (points >= configLevel.Points)
                    return configLevel;

            return null;
        }

        private void KitReward(Level level, UnturnedPlayer player)
        {
            var rewardKit = Kits.Instance.Configuration.Instance.Kits.FirstOrDefault(k =>
                string.Equals(k.Name, level.KitName, StringComparison.CurrentCultureIgnoreCase));
            if (rewardKit == null)
            {
                Logger.LogWarning("Kit " + level.KitName + " not found.");
                return;
            }

            foreach (var item in rewardKit.Items)
                if (!player.GiveItem(item.ItemId, item.Amount))
                    Logger.Log($"Failed giving a item to {player.CharacterName} ({item.ItemId}, {item.Amount})");
            player.Experience += rewardKit.XP.Value;

            if (level.KitNotify)
                UnturnedChat.Say(player, Translate("level_up_kit", level.KitName), configNotificationColor);
        }

        private void PermissionGroupReward(Level level, IRocketPlayer player)
        {
            var result = R.Permissions.AddPlayerToGroup(level.PermissionGroupName, player);

            switch (result)
            {
                case RocketPermissionsProviderResult.GroupNotFound:
                    Logger.LogWarning(
                        $"Group {level.PermissionGroupName} does not exist. Group was not given to player.");
                    break;
                case RocketPermissionsProviderResult.Success:
                    if (level.PermissionGroupNotify)
                        UnturnedChat.Say(player, Translate("level_up_rank", level.PermissionGroupName),
                            configNotificationColor);
                    break;
            }
        }

        private void UconomyReward(Level level, IRocketPlayer player)
        {
            Uconomy.Instance.Database.IncreaseBalance(player.Id, level.UconomyAmount);
            if (level.UconomyNotify)
                UnturnedChat.Say(player,
                    Translate("level_up_uconomy",
                        level.UconomyAmount +
                        Uconomy.Instance.Configuration.Instance.MoneyName),
                    configNotificationColor);
        }
    }
}