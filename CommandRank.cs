using Rocket.API;
using Rocket.Core.Logging;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using System.Collections.Generic;

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
            get { return "Display or reset rank"; }
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
                    "rank.other",
                    "rank.reset"
                };
            }
        }

        public void Execute(IRocketPlayer caller, params string[] command)
        {
            UnturnedPlayer callerPlayer = null;
            if (caller is ConsolePlayer == false) { callerPlayer = (UnturnedPlayer)caller; }

            if (command.Length == 0 && caller is ConsolePlayer == false)
            {
                int playerPoints;
                bool playerExists = FeexRanks.dicPoints.TryGetValue(callerPlayer.CSteamID, out playerPoints);
                if (playerExists)
                {
                    UnturnedChat.Say(caller, FeexRanks.Instance.Translations.Instance.Translate("rank_self", playerPoints, FeexRanks.Instance.FeexRanksDatabase.GetRankBySteamID(callerPlayer.CSteamID.ToString()), FeexRanks.Instance.GetLevel(playerPoints).Name), FeexRanks.Instance.configNotificationColor);
                }
            }
            else if (command.Length == 1 && (caller is ConsolePlayer || callerPlayer.HasPermission("rank.other")))
            {
                UnturnedPlayer otherPlayer = UnturnedPlayer.FromName(command[0]);
                if (otherPlayer == null)
                {
                    if (caller is ConsolePlayer) { Logger.Log(FeexRanks.Instance.Translations.Instance.Translate("general_not_found")); }
                    else { UnturnedChat.Say(caller, FeexRanks.Instance.Translations.Instance.Translate("general_not_found"), FeexRanks.Instance.configNotificationColor); }
                }
                else
                {
                    int playerPoints;
                    bool playerExists = FeexRanks.dicPoints.TryGetValue(otherPlayer.CSteamID, out playerPoints);
                    if (playerExists)
                    {
                        if (caller is ConsolePlayer) { Logger.Log(FeexRanks.Instance.Translations.Instance.Translate("rank_other", playerPoints, FeexRanks.Instance.FeexRanksDatabase.GetRankBySteamID(otherPlayer.CSteamID.ToString()), FeexRanks.Instance.GetLevel(playerPoints).Name, otherPlayer.DisplayName)); }
                        else { UnturnedChat.Say(caller, FeexRanks.Instance.Translations.Instance.Translate("rank_other", playerPoints, FeexRanks.Instance.FeexRanksDatabase.GetRankBySteamID(otherPlayer.CSteamID.ToString()), FeexRanks.Instance.GetLevel(playerPoints).Name, otherPlayer.DisplayName), FeexRanks.Instance.configNotificationColor); }
                    }
                }
            }
            else if (command.Length == 2 && (caller is ConsolePlayer || callerPlayer.HasPermission("rank.reset") && command[1] == "reset"))
            {
                UnturnedPlayer otherPlayer = UnturnedPlayer.FromName(command[0]);
                if (otherPlayer == null)
                {
                    if (caller is ConsolePlayer) { Logger.Log(FeexRanks.Instance.Translations.Instance.Translate("general_not_found")); }
                    else { UnturnedChat.Say(caller, FeexRanks.Instance.Translations.Instance.Translate("general_not_found"), FeexRanks.Instance.configNotificationColor); }
                }
                else
                {
                    FeexRanks.Instance.FeexRanksDatabase.SetPoints(otherPlayer.CSteamID.ToString(), 0);
                    UnturnedChat.Say(otherPlayer, FeexRanks.Instance.Translations.Instance.Translate("rank_reset_player", otherPlayer.DisplayName), FeexRanks.Instance.configNotificationColor);
                    if (caller is ConsolePlayer) { Logger.Log(FeexRanks.Instance.Translations.Instance.Translate("rank_reset_caller", otherPlayer.DisplayName)); }
                    else { UnturnedChat.Say(caller, FeexRanks.Instance.Translations.Instance.Translate("rank_reset_caller", otherPlayer.DisplayName), FeexRanks.Instance.configNotificationColor); }
                }
            }
            else
            {
                if (caller is ConsolePlayer) { Logger.Log(FeexRanks.Instance.Translations.Instance.Translate("general_invalid_parameter")); }
                else { UnturnedChat.Say(caller, FeexRanks.Instance.Translations.Instance.Translate("general_invalid_parameter"), FeexRanks.Instance.configNotificationColor); }
            }
        }

    }
}
