using Rocket.API.Collections;
using Rocket.Core.Logging;
using Rocket.Core.Plugins;
using Rocket.Unturned;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Events;
using Rocket.Unturned.Player;
using System;
using System.Linq;
using UnityEngine;

namespace Freenex.FeexRanks
{
    public class FeexRanks : RocketPlugin<FeexRanksConfiguration>
    {
        public DatabaseManager FeexRanksDatabase;
        public static FeexRanks Instance;

        public override TranslationList DefaultTranslations
        {
            get
            {
                return new TranslationList(){
                {"rank_self","Your current rank: {2} with {0} points on position {1}."},
                {"rank_other","{3}'s current rank: {2} with {0} points on position {1}."},
                {"rank_reset_player","Your points have been reseted."},
                {"rank_reset_caller","You have reseted the points of {0}."},
                {"list_1","The top 3 players:"},
                {"list_2","{1}st: [{2}] {3} ({0} points)"},
                {"list_3","{1}nd: [{2}] {3} ({0} points)"},
                {"list_4","{1}th: [{2}] {3} ({0} points)"},
                {"list_search","Position {1}: [{2}] {3} ({0} points)"},
                {"list_search_not_found","Position not found."},
                {"rank_general_onjoin","[{2}] {3} ({0} points, {1} position) connected to the server."},
                {"rank_general_up","You went up: {1} with {0} points."},
                {"rank_general_up_kit","You went up and reveiced the kit {0}."},
                {"rank_general_up_uconomy","You went up and received {0}."},
                {"rank_general_up_global","{2} went up: {1} with {0} points."},
                {"rank_general_not_found","Player not found."},
                {"rank_general_invalid_parameter","Invalid parameter."},
                {"points_ACCURACY","You received {0} points."},
                {"points_ARENA_WINS","You received {0} points."},
                {"points_DEATHS_PLAYERS","You received {0} points."},
                {"points_FOUND_BUILDABLES","You received {0} points."},
                {"points_FOUND_CRAFTS","You received {0} points."},
                {"points_FOUND_EXPERIENCE","You received {0} points."},
                {"points_FOUND_FISHES","You received {0} points."},
                {"points_FOUND_ITEMS","You received {0} points."},
                {"points_FOUND_PLANTS","You received {0} points."},
                {"points_FOUND_RESOURCES","You received {0} points."},
                {"points_FOUND_THROWABLES","You received {0} points."},
                {"points_HEADSHOTS","You received {0} points."},
                {"points_KILLS_ANIMALS","You received {0} points."},
                {"points_KILLS_PLAYERS","You received {0} points."},
                {"points_KILLS_ZOMBIES_MEGA","You received {0} points."},
                {"points_KILLS_ZOMBIES_NORMAL","You received {0} points."},
                {"points_NONE","You received {0} points."},
                {"points_TRAVEL_FOOT","You received {0} points."},
                {"points_TRAVEL_VEHICLE","You received {0} points."}
                };
            }
        }

        protected override void Load()
        {
            Instance = this;
            FeexRanksDatabase = new DatabaseManager();
            FeexRanks.Instance.Configuration.Instance.Ranks = FeexRanks.Instance.Configuration.Instance.Ranks.OrderByDescending(x => x.Points).ToList();
            U.Events.OnPlayerConnected += Events_OnPlayerConnected;
            UnturnedPlayerEvents.OnPlayerUpdateStat += UnturnedPlayerEvents_OnPlayerUpdateStat;
            Logger.Log("Freenex's FeexRanks has been loaded!");
        }

        protected override void Unload()
        {
            U.Events.OnPlayerConnected -= Events_OnPlayerConnected;
            UnturnedPlayerEvents.OnPlayerUpdateStat -= UnturnedPlayerEvents_OnPlayerUpdateStat;
            Logger.Log("Freenex's FeexRanks has been unloaded!");
        }

        private void Events_OnPlayerConnected(UnturnedPlayer player)
        {
            if (FeexRanks.Instance.FeexRanksDatabase.CheckExists(player.CSteamID))
            {
                FeexRanks.Instance.FeexRanksDatabase.UpdateDisplayName(player.CSteamID, player.DisplayName);
            }
            else
            {
                FeexRanks.Instance.FeexRanksDatabase.AddAccount(player);
            }

            if (FeexRanks.Instance.Configuration.Instance.EnableRankNotificationOnJoin)
            {
                string[] rankInfo = FeexRanks.Instance.FeexRanksDatabase.GetAccountBySteamID(player.CSteamID);
                UnturnedChat.Say(player, FeexRanks.Instance.Translations.Instance.Translate("rank_self", rankInfo[0], rankInfo[1], FeexRanks.Instance.GetRankName(Convert.ToInt16(rankInfo[0])).Name), UnturnedChat.GetColorFromName(FeexRanks.Instance.Configuration.Instance.NotificationColor, Color.green));
            }
            if (FeexRanks.Instance.Configuration.Instance.EnableRankNotificationOnJoinGlobal)
            {
                string[] rankInfo = FeexRanks.Instance.FeexRanksDatabase.GetAccountBySteamID(player.CSteamID);
                UnturnedChat.Say(FeexRanks.Instance.Translations.Instance.Translate("rank_general_onjoin", rankInfo[0], rankInfo[1], FeexRanks.Instance.GetRankName(Convert.ToInt16(rankInfo[0])).Name, player.DisplayName), UnturnedChat.GetColorFromName(FeexRanks.Instance.Configuration.Instance.NotificationColorGlobal, Color.green));
            }
        }

        private void UnturnedPlayerEvents_OnPlayerUpdateStat(UnturnedPlayer player, SDG.Unturned.EPlayerStat stat)
        {
            classPoints configPointsEvent = FeexRanks.Instance.Configuration.Instance.Points.Find(x => x.EventName == stat.ToString());
            if (configPointsEvent != null)
            {
                UpdatePoints(player, configPointsEvent.Points);
                if (configPointsEvent.Notify)
                {
                    UnturnedChat.Say(player, Translate("points_" + configPointsEvent.EventName, configPointsEvent.Points), UnturnedChat.GetColorFromName(FeexRanks.Instance.Configuration.Instance.NotificationColor, Color.green));
                }
            }
        }

        public void UpdatePoints (UnturnedPlayer player, int points)
        {
            classRank oldRank = GetRankName(Convert.ToInt16(FeexRanks.Instance.FeexRanksDatabase.GetAccountBySteamID(player.CSteamID)[0]));
            FeexRanks.Instance.FeexRanksDatabase.UpdateAccount(player.CSteamID, points);
            int newPoints = Convert.ToInt16(FeexRanks.Instance.FeexRanksDatabase.GetAccountBySteamID(player.CSteamID)[0]);
            classRank newRank = GetRankName(newPoints);

            if (oldRank.Name != newRank.Name)
            {
                if (FeexRanks.Instance.Configuration.Instance.EnableRankNotification)
                {
                    UnturnedChat.Say(player, Translate("rank_general_up", newPoints, newRank.Name), UnturnedChat.GetColorFromName(FeexRanks.Instance.Configuration.Instance.NotificationColor, Color.green));
                }
                if (FeexRanks.Instance.Configuration.Instance.EnableRankNotificationGlobal)
                {
                    UnturnedChat.Say(Translate("rank_general_up_global", newPoints, newRank.Name, player.DisplayName), UnturnedChat.GetColorFromName(FeexRanks.Instance.Configuration.Instance.NotificationColorGlobal, Color.green));
                }

                if (newRank.KitReward)
                {
                    KitReward(newRank, player);
                }
                if (newRank.UconomyReward)
                {
                    UconomyReward(newRank, player);
                }
            }
        }

        public classRank GetRankName (int points)
        {
            foreach (classRank configPointsEvent in FeexRanks.Instance.Configuration.Instance.Ranks)
            {
                if (points >= configPointsEvent.Points)
                {
                    return configPointsEvent;
                }
            }
            return null;
        }

        private void KitReward(classRank configRank, UnturnedPlayer player)
        {
            fr34kyn01535.Kits.Kit kit = fr34kyn01535.Kits.Kits.Instance.Configuration.Instance.Kits.Where(k => k.Name.ToLower() == configRank.KitName.ToLower()).FirstOrDefault();
            if (kit == null)
            {
                Logger.LogWarning(fr34kyn01535.Kits.Kits.Instance.Translations.Instance.Translate("command_kit_not_found"));
                return;
            }
            foreach (fr34kyn01535.Kits.KitItem item in kit.Items)
            {
                if (!player.GiveItem(item.ItemId, item.Amount))
                {
                    Logger.Log(fr34kyn01535.Kits.Kits.Instance.Translations.Instance.Translate("command_kit_failed_giving_item", player.CharacterName, item.ItemId, item.Amount));
                }
            }
            player.Experience += kit.XP;

            if (configRank.KitNotify)
            {
                UnturnedChat.Say(player, Translate("rank_general_up_kit", configRank.KitName), UnturnedChat.GetColorFromName(FeexRanks.Instance.Configuration.Instance.NotificationColor, Color.green));
            }
        }

        private void UconomyReward(classRank configRank, UnturnedPlayer player)
        {
            fr34kyn01535.Uconomy.Uconomy.Instance.Database.IncreaseBalance(player.Id, configRank.UconomyAmount);
            if (configRank.UconomyNotify)
            {
                UnturnedChat.Say(player, Translate("rank_general_up_uconomy", configRank.UconomyAmount + fr34kyn01535.Uconomy.Uconomy.Instance.Configuration.Instance.MoneyName), UnturnedChat.GetColorFromName(FeexRanks.Instance.Configuration.Instance.NotificationColor, Color.green));
            }
        }

    }
}
