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
        public Color configNotificationColor;
        public Color configNotificationColorGlobal;

        public override TranslationList DefaultTranslations
        {
            get
            {
                return new TranslationList(){
                {"rank_self","Your current rank: {1} with {0} points [{2}]"},
                {"rank_other","{3}'s current rank: {1} with {0} points [{2}]"},
                {"rank_reset_player","Your points have been reseted."},
                {"rank_reset_caller","You have reseted the points of {0}."},
                {"list_1","The top 3 players:"},
                {"list_2","{1}st: [{2}] {3} ({0} points)"},
                {"list_3","{1}nd: [{2}] {3} ({0} points)"},
                {"list_4","{1}th: [{2}] {3} ({0} points)"},
                {"list_search","Rank {1}: [{2}] {3} ({0} points)"},
                {"list_search_not_found","Rank not found."},
                {"level_up","You went up: {1} with {0} points."},
                {"level_up_kit","You went up and reveiced the kit {0}."},
                {"level_up_uconomy","You went up and received {0}."},
                {"level_up_global","{2} went up: {1} with {0} points."},
                {"general_onjoin","[{2}] {3} ({0} points, rank {1}) connected to the server."},
                {"general_not_found","Player not found."},
                {"general_invalid_parameter","Invalid parameter."},
                {"event_ACCURACY","You received {0} points."},
                {"event_ARENA_WINS","You received {0} points."},
                {"event_DEATHS_PLAYERS","You received {0} points."},
                {"event_FOUND_BUILDABLES","You received {0} points."},
                {"event_FOUND_CRAFTS","You received {0} points."},
                {"event_FOUND_EXPERIENCE","You received {0} points."},
                {"event_FOUND_FISHES","You received {0} points."},
                {"event_FOUND_ITEMS","You received {0} points."},
                {"event_FOUND_PLANTS","You received {0} points."},
                {"event_FOUND_RESOURCES","You received {0} points."},
                {"event_FOUND_THROWABLES","You received {0} points."},
                {"event_HEADSHOTS","You received {0} points."},
                {"event_KILLS_ANIMALS","You received {0} points."},
                {"event_KILLS_PLAYERS","You received {0} points."},
                {"event_KILLS_ZOMBIES_MEGA","You received {0} points."},
                {"event_KILLS_ZOMBIES_NORMAL","You received {0} points."},
                {"event_NONE","You received {0} points."},
                {"event_TRAVEL_FOOT","You received {0} points."},
                {"event_TRAVEL_VEHICLE","You received {0} points."}
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
            if (FeexRanks.Instance.FeexRanksDatabase.CheckAccount(player.CSteamID))
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
                UnturnedChat.Say(player, FeexRanks.Instance.Translations.Instance.Translate("rank_self", rankInfo[0], rankInfo[1], FeexRanks.Instance.GetLevel(Convert.ToInt16(rankInfo[0])).Name), configNotificationColor);
            }
            if (FeexRanks.Instance.Configuration.Instance.EnableRankNotificationOnJoinGlobal)
            {
                string[] rankInfo = FeexRanks.Instance.FeexRanksDatabase.GetAccountBySteamID(player.CSteamID);
                UnturnedChat.Say(FeexRanks.Instance.Translations.Instance.Translate("general_onjoin", rankInfo[0], rankInfo[1], FeexRanks.Instance.GetLevel(Convert.ToInt16(rankInfo[0])).Name, player.DisplayName), configNotificationColorGlobal);
            }
        }

        private void UnturnedPlayerEvents_OnPlayerUpdateStat(UnturnedPlayer player, SDG.Unturned.EPlayerStat stat)
        {
            classEvent configEvent = FeexRanks.Instance.Configuration.Instance.Events.Find(x => x.EventName == stat.ToString());
            if (configEvent != null)
            {
                UpdatePoints(player, configEvent.Points);
                if (configEvent.Notify)
                {
                    UnturnedChat.Say(player, Translate("event_" + configEvent.EventName, configEvent.Points), configNotificationColor);
                }
            }
        }

        public void UpdatePoints (UnturnedPlayer player, int points)
        {
            classLevel configLevelOld = GetLevel(Convert.ToInt16(FeexRanks.Instance.FeexRanksDatabase.GetAccountBySteamID(player.CSteamID)[0]));
            FeexRanks.Instance.FeexRanksDatabase.UpdateAccount(player.CSteamID, points);
            int newPoints = Convert.ToInt16(FeexRanks.Instance.FeexRanksDatabase.GetAccountBySteamID(player.CSteamID)[0]);
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
                Logger.LogWarning("Kit " + configLevel.KitName + " not found.");
                return;
            }
            foreach (fr34kyn01535.Kits.KitItem item in rewardKit.Items)
            {
                if (!player.GiveItem(item.ItemId, item.Amount))
                {
                    Logger.Log(string.Format("Failed giving a item to {0} ({1}, {2})", player.CharacterName, item.ItemId, item.Amount));
                }
            }
            player.Experience += rewardKit.XP;

            if (configLevel.KitNotify)
            {
                UnturnedChat.Say(player, Translate("level_up_kit", configLevel.KitName), configNotificationColor);
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
