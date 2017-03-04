using Rocket.API.Collections;
using Rocket.Core.Plugins;
using Rocket.Unturned;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Events;
using Rocket.Unturned.Player;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Logger = Rocket.Core.Logging.Logger;

namespace Freenex.FeexRanks
{
    public class FeexRanks : RocketPlugin<FeexRanksConfiguration>
    {
        public DatabaseManager FeexRanksDatabase;
        public static FeexRanks Instance;
        public static Dictionary<Steamworks.CSteamID, int> dicPoints = new Dictionary<Steamworks.CSteamID, int>();
        public Color configNotificationColor;
        public Color configNotificationColorGlobal;
        public Color configNotificationColorJoinLeaveGlobal;

        private DateTime? lastQuery = DateTime.Now;

        public override TranslationList DefaultTranslations
        {
            get
            {
                return new TranslationList(){
                {"general_onjoin","[{2}] {3} ({0} points, rank {1}) connected to the server."},
                {"general_onleave","[{2}] {3} ({0} points, rank {1}) disconnected from the server."},
                {"general_not_found","Player not found."},
                {"general_invalid_parameter","Invalid parameter."},
                {"rank_self","Your current rank: {1} with {0} points [{2}]"},
                {"rank_other","{3}'s current rank: {1} with {0} points [{2}]"},
                {"list_1","The top 3 players:"},
                {"list_2","{1}st: [{2}] {3} ({0} points)"},
                {"list_3","{1}nd: [{2}] {3} ({0} points)"},
                {"list_4","{1}rd: [{2}] {3} ({0} points)"},
                {"list_search","Rank {1}: [{2}] {3} ({0} points)"},
                {"list_search_not_found","Rank not found."},
                {"points_reset_player","Your points have been reset."},
                {"points_reset_caller","{0}'s points have been reset."},
                {"points_set_player","Your points have been set to {0}."},
                {"points_set_caller","{1}'s points have been set to {0}."},
                {"points_add_player","You received {0} points."},
                {"points_add_caller","You sent {0} points to {1}."},
                {"points_remove_player","You lost {0} points."},
                {"points_remove_caller","You removed {0} points from {1}."},
                {"level_up","You went up: {1} with {0} points."},
                {"level_up_kit","You went up and received the kit {0}."},
                {"level_up_rank","You went up and recieved the permission rank {0}." },
                {"level_up_uconomy","You went up and received {0}."},
                {"level_up_global","{2} went up: {1} with {0} points."},
                {"event_ACCURACY","You received {0} points. ({1} points)"},
                {"event_ARENA_WINS","You received {0} points. ({1} points)"},
                {"event_DEATHS_PLAYERS","You received {0} points. ({1} points)"},
                {"event_FOUND_BUILDABLES","You received {0} points. ({1} points)"},
                {"event_FOUND_CRAFTS","You received {0} points. ({1} points)"},
                {"event_FOUND_EXPERIENCE","You received {0} points. ({1} points)"},
                {"event_FOUND_FISHES","You received {0} points. ({1} points)"},
                {"event_FOUND_ITEMS","You received {0} points. ({1} points)"},
                {"event_FOUND_PLANTS","You received {0} points. ({1} points)"},
                {"event_FOUND_RESOURCES","You received {0} points. ({1} points)"},
                {"event_FOUND_THROWABLES","You received {0} points. ({1} points)"},
                {"event_HEADSHOTS","You received {0} points. ({1} points)"},
                {"event_KILLS_ANIMALS","You received {0} points. ({1} points)"},
                {"event_KILLS_PLAYERS","You received {0} points. ({1} points)"},
                {"event_KILLS_ZOMBIES_MEGA","You received {0} points. ({1} points)"},
                {"event_KILLS_ZOMBIES_NORMAL","You received {0} points. ({1} points)"},
                {"event_NONE","You received {0} points. ({1} points)"},
                {"event_TRAVEL_FOOT","You received {0} points. ({1} points)"},
                {"event_TRAVEL_VEHICLE","You received {0} points. ({1} points)"}
                };
            }
        }

        protected override void Load()
        {
            Instance = this;
            FeexRanksDatabase = new DatabaseManager();

            FeexRanks.Instance.Configuration.Instance.Level = FeexRanks.Instance.Configuration.Instance.Level.OrderByDescending(x => x.Points).ToList();
            configNotificationColor = UnturnedChat.GetColorFromName(FeexRanks.Instance.Configuration.Instance.NotificationColor, Color.green);
            configNotificationColorGlobal = UnturnedChat.GetColorFromName(FeexRanks.Instance.Configuration.Instance.NotificationColorGlobal, Color.green);
            configNotificationColorJoinLeaveGlobal = UnturnedChat.GetColorFromName(FeexRanks.Instance.Configuration.Instance.NotificationColorJoinLeaveGlobal, Color.green);
            
            U.Events.OnPlayerConnected += Events_OnPlayerConnected;
            U.Events.OnPlayerDisconnected += Events_OnPlayerDisconnected;
            UnturnedPlayerEvents.OnPlayerUpdateStat += UnturnedPlayerEvents_OnPlayerUpdateStat;

            Rocket.Core.Logging.Logger.Log("Freenex's FeexRanks has been loaded!");
        }

        protected override void Unload()
        {
            dicPoints.Clear();

            U.Events.OnPlayerConnected -= Events_OnPlayerConnected;
            U.Events.OnPlayerDisconnected -= Events_OnPlayerDisconnected;
            UnturnedPlayerEvents.OnPlayerUpdateStat -= UnturnedPlayerEvents_OnPlayerUpdateStat;

            Rocket.Core.Logging.Logger.Log("Freenex's FeexRanks has been unloaded!");
        }

        private void Events_OnPlayerConnected(UnturnedPlayer player)
        {
            FeexRanks.Instance.FeexRanksDatabase.AddUpdatePlayer(player.CSteamID.ToString(), player.DisplayName);
            string[] rankInfo = FeexRanks.Instance.FeexRanksDatabase.GetAccountBySteamID(player.CSteamID.ToString());
            dicPoints.Add(player.CSteamID, Convert.ToInt32(rankInfo[0]));

            if (FeexRanks.Instance.Configuration.Instance.EnableRankNotificationOnJoin)
            {
                UnturnedChat.Say(player, FeexRanks.Instance.Translations.Instance.Translate("rank_self", rankInfo[0], rankInfo[1], FeexRanks.Instance.GetLevel(Convert.ToInt32(rankInfo[0])).Name), configNotificationColor);
            }
            if (FeexRanks.Instance.Configuration.Instance.EnableRankNotificationOnJoinGlobal)
            {
                UnturnedChat.Say(FeexRanks.Instance.Translations.Instance.Translate("general_onjoin", rankInfo[0], rankInfo[1], FeexRanks.Instance.GetLevel(Convert.ToInt32(rankInfo[0])).Name, player.DisplayName), configNotificationColorJoinLeaveGlobal);
            }
        }

        private void Events_OnPlayerDisconnected(UnturnedPlayer player)
        {
            int playerPoints;
            bool playerExists = dicPoints.TryGetValue(player.CSteamID, out playerPoints);
            if (playerExists)
            {
                dicPoints.Remove(player.CSteamID);
            }

            if (FeexRanks.Instance.Configuration.Instance.EnableRankNotificationOnLeaveGlobal)
            {
                UnturnedChat.Say(FeexRanks.Instance.Translations.Instance.Translate("general_onleave", playerPoints, FeexRanks.Instance.FeexRanksDatabase.GetRankBySteamID(player.CSteamID.ToString()), FeexRanks.Instance.GetLevel(playerPoints).Name, player.DisplayName), configNotificationColorJoinLeaveGlobal);
            }
        }

        private void UnturnedPlayerEvents_OnPlayerUpdateStat(UnturnedPlayer player, SDG.Unturned.EPlayerStat stat)
        {
            classEvent configEvent = FeexRanks.Instance.Configuration.Instance.Events.Find(x => x.EventName == stat.ToString());
            if (configEvent != null)
            {
                if (configEvent.Notify)
                {
                    int oldPoints;
                    bool playerExists = dicPoints.TryGetValue(player.CSteamID, out oldPoints);
                    if (playerExists)
                    {
                        UnturnedChat.Say(player, Translate("event_" + configEvent.EventName, configEvent.Points, oldPoints + configEvent.Points), configNotificationColor);
                    }
                }
                UpdatePoints(player, configEvent.Points);
            }
        }

        public void UpdatePoints(UnturnedPlayer player, int points)
        {
            int oldPoints;
            bool playerExists = dicPoints.TryGetValue(player.CSteamID, out oldPoints);

            if (playerExists)
            {
                FeexRanks.Instance.FeexRanksDatabase.AddPoints(player.CSteamID.ToString(), points);
                dicPoints[player.CSteamID] += points;

                int newPoints = oldPoints + points;
                classLevel configLevelOld = GetLevel(oldPoints);
                classLevel configLevelNew = GetLevel(newPoints);

                if (configLevelOld.Name != configLevelNew.Name)
                {
                    if (FeexRanks.Instance.Configuration.Instance.EnableLevelUpNotification)
                    {
                        UnturnedChat.Say(player, Translate("level_up", newPoints, configLevelNew.Name), configNotificationColor);
                    }
                    if (FeexRanks.Instance.Configuration.Instance.EnableLevelUpNotificationGlobal)
                    {
                        UnturnedChat.Say(Translate("level_up_global", newPoints, configLevelNew.Name, player.DisplayName), configNotificationColorGlobal);
                    }

                    if (configLevelNew.KitReward)
                    {
                        try
                        {
                            KitReward(configLevelNew, player);
                        }
                        catch { }
                    }
                    if (configLevelNew.PermissionGroupReward)
                    {
                        try
                        {
                            PermissionGroupReward(configLevelNew, player);
                        }
                        catch { }
                    }
                    if (configLevelNew.UconomyReward)
                    {
                        try
                        {
                            UconomyReward(configLevelNew, player);
                        }
                        catch { }
                    }
                }
            }
        }

        public classLevel GetLevel (int points)
        {
            foreach (classLevel configLevel in FeexRanks.Instance.Configuration.Instance.Level)
            {
                if (points >= configLevel.Points)
                {
                    return configLevel;
                }
            }
            return null;
        }

        private void KitReward(classLevel configLevel, UnturnedPlayer player)
        {
            fr34kyn01535.Kits.Kit rewardKit = fr34kyn01535.Kits.Kits.Instance.Configuration.Instance.Kits.Where(k => k.Name.ToLower() == configLevel.KitName.ToLower()).FirstOrDefault();
            if (rewardKit == null)
            {
                Rocket.Core.Logging.Logger.LogWarning("Kit " + configLevel.KitName + " not found.");
                return;
            }
            foreach (fr34kyn01535.Kits.KitItem item in rewardKit.Items)
            {
                if (!player.GiveItem(item.ItemId, item.Amount))
                {
                    Rocket.Core.Logging.Logger.Log(string.Format("Failed giving a item to {0} ({1}, {2})", player.CharacterName, item.ItemId, item.Amount));
                }
            }
            player.Experience += rewardKit.XP.Value;

            if (configLevel.KitNotify)
            {
                UnturnedChat.Say(player, Translate("level_up_kit", configLevel.KitName), configNotificationColor);
            }
        }

        private void PermissionGroupReward(classLevel configLevel, UnturnedPlayer player)
        {
            Rocket.Core.Permissions.RocketPermissionsManager a = Rocket.Core.R.Instance.GetComponent<Rocket.Core.Permissions.RocketPermissionsManager>();
            try
            {
                a.GetGroup(configLevel.PermissionGroupName);
            }
            catch (Exception)
            {
                Logger.LogWarning("Group " + configLevel.PermissionGroupName + " does not exist. Group was not given to player.");
                return;
            }
            a.AddPlayerToGroup(configLevel.PermissionGroupName, player);
            if (configLevel.PermissionGroupNotify)
            {
                UnturnedChat.Say(player, Translate("level_up_rank", configLevel.PermissionGroupName), configNotificationColor);
            }
        }

        private void UconomyReward(classLevel configLevel, UnturnedPlayer player)
        {
            fr34kyn01535.Uconomy.Uconomy.Instance.Database.IncreaseBalance(player.Id, configLevel.UconomyAmount);
            if (configLevel.UconomyNotify)
            {
                UnturnedChat.Say(player, Translate("level_up_uconomy", configLevel.UconomyAmount + fr34kyn01535.Uconomy.Uconomy.Instance.Configuration.Instance.MoneyName), configNotificationColor);
            }
        }
    }
}
