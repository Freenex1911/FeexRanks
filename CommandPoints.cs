using Rocket.API;
using Rocket.Core.Logging;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using System.Collections.Generic;

namespace Freenex.FeexRanks
{
    public class CommandPoints : IRocketCommand
    {
        public string Name
        {
            get { return "points"; }
        }

        public string Help
        {
            get { return "Reset, set, add or remove points"; }
        }

        public string Syntax
        {
            get { return "[reset/set/add/remove] [<player>] [<points>]"; }
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
                    "points",
                    "points.reset",
                    "points.set",
                    "points.add",
                    "points.remove"
                };
            }
        }

        public void Execute(IRocketPlayer caller, params string[] command)
        {
            UnturnedPlayer callerPlayer = null;
            if (caller is ConsolePlayer == false) { callerPlayer = (UnturnedPlayer)caller; }

            if (command.Length == 2 && (caller is ConsolePlayer || callerPlayer.HasPermission("points.reset")) && command[0] == "reset")
            {
                UnturnedPlayer otherPlayer = UnturnedPlayer.FromName(command[1]);
                if (otherPlayer == null)
                {
                    if (caller is ConsolePlayer) { Logger.Log(FeexRanks.Instance.Translations.Instance.Translate("general_not_found")); }
                    else { UnturnedChat.Say(caller, FeexRanks.Instance.Translations.Instance.Translate("general_not_found"), FeexRanks.Instance.configNotificationColor); }
                }
                else
                {
                    FeexRanks.Instance.FeexRanksDatabase.SetPoints(otherPlayer.CSteamID.ToString(), 0);
                    FeexRanks.dicPoints[otherPlayer.CSteamID] = 0;
                    UnturnedChat.Say(otherPlayer, FeexRanks.Instance.Translations.Instance.Translate("points_reset_player"), FeexRanks.Instance.configNotificationColor);
                    if (caller is ConsolePlayer) { Logger.Log(FeexRanks.Instance.Translations.Instance.Translate("points_reset_caller", otherPlayer.DisplayName)); }
                    else { UnturnedChat.Say(caller, FeexRanks.Instance.Translations.Instance.Translate("points_reset_caller", otherPlayer.DisplayName), FeexRanks.Instance.configNotificationColor); }
                }
            }
            else if (command.Length == 3 && (caller is ConsolePlayer || callerPlayer.HasPermission("points.set")) && command[0] == "set")
            {
                UnturnedPlayer otherPlayer = UnturnedPlayer.FromName(command[1]);
                if (otherPlayer == null)
                {
                    if (caller is ConsolePlayer) { Logger.Log(FeexRanks.Instance.Translations.Instance.Translate("general_not_found")); }
                    else { UnturnedChat.Say(caller, FeexRanks.Instance.Translations.Instance.Translate("general_not_found"), FeexRanks.Instance.configNotificationColor); }
                }
                else
                {
                    int points;
                    bool isNumeric = int.TryParse(command[2], out points);
                    if (!isNumeric)
                    {
                        if (caller is ConsolePlayer) { Logger.Log(FeexRanks.Instance.Translations.Instance.Translate("general_invalid_parameter")); }
                        else { UnturnedChat.Say(caller, FeexRanks.Instance.Translations.Instance.Translate("general_invalid_parameter"), FeexRanks.Instance.configNotificationColor); }
                        return;
                    }
                    FeexRanks.Instance.FeexRanksDatabase.SetPoints(otherPlayer.CSteamID.ToString(), points);
                    FeexRanks.dicPoints[otherPlayer.CSteamID] = points;
                    UnturnedChat.Say(otherPlayer, FeexRanks.Instance.Translations.Instance.Translate("points_set_player", points.ToString()), FeexRanks.Instance.configNotificationColor);
                    if (caller is ConsolePlayer) { Logger.Log(FeexRanks.Instance.Translations.Instance.Translate("points_set_caller", points.ToString(), otherPlayer.DisplayName)); }
                    else { UnturnedChat.Say(caller, FeexRanks.Instance.Translations.Instance.Translate("points_set_caller", points.ToString(), otherPlayer.DisplayName), FeexRanks.Instance.configNotificationColor); }
                }
            }
            else if (command.Length == 3 && (caller is ConsolePlayer || callerPlayer.HasPermission("points.add")) && command[0] == "add")
            {
                UnturnedPlayer otherPlayer = UnturnedPlayer.FromName(command[1]);
                if (otherPlayer == null)
                {
                    if (caller is ConsolePlayer) { Logger.Log(FeexRanks.Instance.Translations.Instance.Translate("general_not_found")); }
                    else { UnturnedChat.Say(caller, FeexRanks.Instance.Translations.Instance.Translate("general_not_found"), FeexRanks.Instance.configNotificationColor); }
                }
                else
                {
                    int points;
                    bool isNumeric = int.TryParse(command[2], out points);
                    if (!isNumeric)
                    {
                        if (caller is ConsolePlayer) { Logger.Log(FeexRanks.Instance.Translations.Instance.Translate("general_invalid_parameter")); }
                        else { UnturnedChat.Say(caller, FeexRanks.Instance.Translations.Instance.Translate("general_invalid_parameter"), FeexRanks.Instance.configNotificationColor); }
                        return;
                    }
                    FeexRanks.Instance.UpdatePoints(otherPlayer, points);
                    UnturnedChat.Say(otherPlayer, FeexRanks.Instance.Translations.Instance.Translate("points_add_player", points.ToString()), FeexRanks.Instance.configNotificationColor);
                    if (caller is ConsolePlayer) { Logger.Log(FeexRanks.Instance.Translations.Instance.Translate("points_add_caller", points.ToString(), otherPlayer.DisplayName)); }
                    else { UnturnedChat.Say(caller, FeexRanks.Instance.Translations.Instance.Translate("points_add_caller", points.ToString(), otherPlayer.DisplayName), FeexRanks.Instance.configNotificationColor); }
                }
            }
            else if (command.Length == 3 && (caller is ConsolePlayer || callerPlayer.HasPermission("points.remove")) && command[0] == "remove")
            {
                UnturnedPlayer otherPlayer = UnturnedPlayer.FromName(command[1]);
                if (otherPlayer == null)
                {
                    if (caller is ConsolePlayer) { Logger.Log(FeexRanks.Instance.Translations.Instance.Translate("general_not_found")); }
                    else { UnturnedChat.Say(caller, FeexRanks.Instance.Translations.Instance.Translate("general_not_found"), FeexRanks.Instance.configNotificationColor); }
                }
                else
                {
                    int points;
                    bool isNumeric = int.TryParse(command[2], out points);
                    if (!isNumeric)
                    {
                        if (caller is ConsolePlayer) { Logger.Log(FeexRanks.Instance.Translations.Instance.Translate("general_invalid_parameter")); }
                        else { UnturnedChat.Say(caller, FeexRanks.Instance.Translations.Instance.Translate("general_invalid_parameter"), FeexRanks.Instance.configNotificationColor); }
                        return;
                    }

                    int oldPoints;
                    bool playerExists = FeexRanks.dicPoints.TryGetValue(otherPlayer.CSteamID, out oldPoints);
                    if (playerExists)
                    {
                        if ((FeexRanks.dicPoints[otherPlayer.CSteamID] - points) >= 0)
                        {
                            FeexRanks.Instance.FeexRanksDatabase.AddPoints(otherPlayer.CSteamID.ToString(), -points);
                            FeexRanks.dicPoints[otherPlayer.CSteamID] -= points;
                        }
                        else
                        {
                            FeexRanks.Instance.FeexRanksDatabase.SetPoints(otherPlayer.CSteamID.ToString(), 0);
                            points = FeexRanks.dicPoints[otherPlayer.CSteamID];
                            FeexRanks.dicPoints[otherPlayer.CSteamID] = 0;
                        }
                    }

                    UnturnedChat.Say(otherPlayer, FeexRanks.Instance.Translations.Instance.Translate("points_remove_player", points.ToString()), FeexRanks.Instance.configNotificationColor);
                    if (caller is ConsolePlayer) { Logger.Log(FeexRanks.Instance.Translations.Instance.Translate("points_remove_caller", points.ToString(), otherPlayer.DisplayName)); }
                    else { UnturnedChat.Say(caller, FeexRanks.Instance.Translations.Instance.Translate("points_remove_caller", points.ToString(), otherPlayer.DisplayName), FeexRanks.Instance.configNotificationColor); }
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
