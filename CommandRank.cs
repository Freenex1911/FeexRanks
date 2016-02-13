using Rocket.API;
using Rocket.Core.Logging;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Freenex.FeexRanks
{
    public class CommandRank : IRocketCommand
    {
        public string Name
        {
            get { return "rank"; }
        }
        public string Help
        {
            get { return "View or reset rank"; }
        }

        public string Syntax
        {
            get { return "[<player>] [reset]"; }
        }

        public List<string> Aliases
        {
            get { return new List<string>(); }
        }

        public AllowedCaller AllowedCaller
        {
            get { return AllowedCaller.Both; }
        }

        public List<string> Permissions
        {
            get
            {
                return new List<string>()
                {
                    "rank",
                    "rank.other"
                };
            }
        }

        public void Execute(IRocketPlayer caller, params string[] command)
        {
            UnturnedPlayer callerPlayer = null;
            if (caller is ConsolePlayer == false) { callerPlayer = (UnturnedPlayer)caller; }

            if (command.Length == 0 && caller is ConsolePlayer == false)
            {
                string[] rankInfo = FeexRanks.Instance.FeexRanksDatabase.GetAccountBySteamID(callerPlayer.CSteamID);
                UnturnedChat.Say(caller, FeexRanks.Instance.Translations.Instance.Translate("rank_self", rankInfo[0], rankInfo[1], FeexRanks.Instance.GetRankName(Convert.ToInt16(rankInfo[0])).Name), UnturnedChat.GetColorFromName(FeexRanks.Instance.Configuration.Instance.NotificationColor, Color.green));
            }
            else if (command.Length == 1 && (caller is ConsolePlayer || callerPlayer.HasPermission("rank.other")))
            {
                UnturnedPlayer otherPlayer = UnturnedPlayer.FromName(command[0]);
                if (otherPlayer == null)
                {
                    if (caller is ConsolePlayer) { Logger.Log(FeexRanks.Instance.Translations.Instance.Translate("rank_general_not_found")); }
                    else { UnturnedChat.Say(caller, FeexRanks.Instance.Translations.Instance.Translate("rank_general_not_found")); }
                }
                else
                {
                    string[] rankInfo = FeexRanks.Instance.FeexRanksDatabase.GetAccountBySteamID(callerPlayer.CSteamID);
                    if (caller is ConsolePlayer) { Logger.Log(FeexRanks.Instance.Translations.Instance.Translate("rank_other", rankInfo[0], rankInfo[1], FeexRanks.Instance.GetRankName(Convert.ToInt16(rankInfo[0])).Name, otherPlayer.DisplayName)); }
                    else { UnturnedChat.Say(caller, FeexRanks.Instance.Translations.Instance.Translate("rank_other", rankInfo[0], rankInfo[1], FeexRanks.Instance.GetRankName(Convert.ToInt16(rankInfo[0])).Name, otherPlayer.DisplayName), UnturnedChat.GetColorFromName(FeexRanks.Instance.Configuration.Instance.NotificationColor, Color.green)); }
                }
            }
            else if (command.Length == 2 && (caller is ConsolePlayer || callerPlayer.HasPermission("rank.reset") && command[1] == "reset"))
            {
                UnturnedPlayer otherPlayer = UnturnedPlayer.FromName(command[0]);
                if (otherPlayer == null)
                {
                    if (caller is ConsolePlayer) { Logger.Log(FeexRanks.Instance.Translations.Instance.Translate("rank_general_not_found")); }
                    else { UnturnedChat.Say(caller, FeexRanks.Instance.Translations.Instance.Translate("rank_general_not_found")); }
                }
                else
                {
                    FeexRanks.Instance.FeexRanksDatabase.SetAccount(otherPlayer.CSteamID, 0);
                    UnturnedChat.Say(otherPlayer, FeexRanks.Instance.Translations.Instance.Translate("rank_reset_player", otherPlayer.DisplayName), UnturnedChat.GetColorFromName(FeexRanks.Instance.Configuration.Instance.NotificationColor, Color.green));
                    if (caller is ConsolePlayer) { Logger.Log(FeexRanks.Instance.Translations.Instance.Translate("rank_reset_caller", otherPlayer.DisplayName)); }
                    else { UnturnedChat.Say(caller, FeexRanks.Instance.Translations.Instance.Translate("rank_reset_caller", otherPlayer.DisplayName)); }
                }
            }
            else
            {
                UnturnedChat.Say(caller, FeexRanks.Instance.Translations.Instance.Translate("rank_general_invalid_parameter"), UnturnedChat.GetColorFromName(FeexRanks.Instance.Configuration.Instance.NotificationColor, Color.green));
            }
        }

    }
}
