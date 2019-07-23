using System.Collections.Generic;
using Rocket.API;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;

namespace Freenex.FeexRanks
{
    public class CommandPoints : IRocketCommand
    {
        public string Name => "points";

        public string Help => "Reset, set, add or remove points";

        public string Syntax => "[reset/set/add/remove] [<player>] [<points>]";

        public List<string> Aliases => new List<string>();

        public AllowedCaller AllowedCaller => AllowedCaller.Both;

        public List<string> Permissions =>
            new List<string>
            {
                "points",
                "points.reset",
                "points.set",
                "points.add",
                "points.remove"
            };

        public void Execute(IRocketPlayer caller, params string[] command)
        {
            if (command.Length == 2 && caller.HasPermission("points.reset") && command[0] == "reset")
            {
                var otherPlayer = UnturnedPlayer.FromName(command[1]);
                if (otherPlayer == null)
                {
                    UnturnedChat.Say(caller, FeexRanks.Instance.Translate("general_not_found"),
                        FeexRanks.Instance.configNotificationColor);
                }
                else
                {
                    FeexRanks.Instance.FeexRanksDatabase.SetPoints(otherPlayer.CSteamID.ToString(), 0);
                    FeexRanks.DicPoints[otherPlayer.CSteamID] = 0;
                    UnturnedChat.Say(otherPlayer, FeexRanks.Instance.Translate("points_reset_player"),
                        FeexRanks.Instance.configNotificationColor);
                    UnturnedChat.Say(caller,
                        FeexRanks.Instance.Translate("points_reset_caller", otherPlayer.DisplayName),
                        FeexRanks.Instance.configNotificationColor);
                }
            }
            else if (command.Length == 3 && caller.HasPermission("points.set") && command[0] == "set")
            {
                var otherPlayer = UnturnedPlayer.FromName(command[1]);
                if (otherPlayer == null)
                {
                    UnturnedChat.Say(caller, FeexRanks.Instance.Translate("general_not_found"),
                        FeexRanks.Instance.configNotificationColor);
                }
                else
                {
                    var isNumeric = int.TryParse(command[2], out var points);
                    if (!isNumeric)
                    {
                        UnturnedChat.Say(caller, FeexRanks.Instance.Translate("general_invalid_parameter"),
                            FeexRanks.Instance.configNotificationColor);
                        return;
                    }

                    FeexRanks.Instance.FeexRanksDatabase.SetPoints(otherPlayer.CSteamID.ToString(), points);
                    FeexRanks.DicPoints[otherPlayer.CSteamID] = points;
                    UnturnedChat.Say(otherPlayer,
                        FeexRanks.Instance.Translate("points_set_player", points.ToString()),
                        FeexRanks.Instance.configNotificationColor);
                    UnturnedChat.Say(caller,
                        FeexRanks.Instance.Translate("points_set_caller", points.ToString(), otherPlayer.DisplayName),
                        FeexRanks.Instance.configNotificationColor);
                }
            }
            else if (command.Length == 3 && caller.HasPermission("points.add") && command[0] == "add")
            {
                var otherPlayer = UnturnedPlayer.FromName(command[1]);
                if (otherPlayer == null)
                {
                    UnturnedChat.Say(caller, FeexRanks.Instance.Translate("general_not_found"),
                        FeexRanks.Instance.configNotificationColor);
                }
                else
                {
                    var isNumeric = int.TryParse(command[2], out var points);
                    if (!isNumeric)
                    {
                        UnturnedChat.Say(caller, FeexRanks.Instance.Translate("general_invalid_parameter"),
                            FeexRanks.Instance.configNotificationColor);
                        return;
                    }

                    FeexRanks.Instance.UpdatePoints(otherPlayer, points);
                    UnturnedChat.Say(otherPlayer,
                        FeexRanks.Instance.Translate("points_add_player", points.ToString()),
                        FeexRanks.Instance.configNotificationColor);
                    UnturnedChat.Say(caller,
                        FeexRanks.Instance.Translate("points_add_caller", points.ToString(), otherPlayer.DisplayName),
                        FeexRanks.Instance.configNotificationColor);
                }
            }
            else if (command.Length == 3 && caller.HasPermission("points.remove") && command[0] == "remove")
            {
                var otherPlayer = UnturnedPlayer.FromName(command[1]);
                if (otherPlayer == null)
                {
                    UnturnedChat.Say(caller, FeexRanks.Instance.Translate("general_not_found"),
                        FeexRanks.Instance.configNotificationColor);
                }
                else
                {
                    var isNumeric = int.TryParse(command[2], out var points);
                    if (!isNumeric)
                    {
                        UnturnedChat.Say(caller, FeexRanks.Instance.Translate("general_invalid_parameter"),
                            FeexRanks.Instance.configNotificationColor);
                        return;
                    }

                    var playerExists = FeexRanks.DicPoints.TryGetValue(otherPlayer.CSteamID, out _);
                    if (playerExists)
                    {
                        if (FeexRanks.DicPoints[otherPlayer.CSteamID] - points >= 0)
                        {
                            FeexRanks.Instance.FeexRanksDatabase.AddPoints(otherPlayer.CSteamID.ToString(), -points);
                            FeexRanks.DicPoints[otherPlayer.CSteamID] -= points;
                        }
                        else
                        {
                            FeexRanks.Instance.FeexRanksDatabase.SetPoints(otherPlayer.CSteamID.ToString(), 0);
                            points = FeexRanks.DicPoints[otherPlayer.CSteamID];
                            FeexRanks.DicPoints[otherPlayer.CSteamID] = 0;
                        }
                    }

                    UnturnedChat.Say(otherPlayer,
                        FeexRanks.Instance.Translate("points_remove_player", points.ToString()),
                        FeexRanks.Instance.configNotificationColor);
                    UnturnedChat.Say(caller,
                        FeexRanks.Instance.Translate("points_remove_caller", points.ToString(),
                            otherPlayer.DisplayName), FeexRanks.Instance.configNotificationColor);
                }
            }
            else
            {
                UnturnedChat.Say(caller, FeexRanks.Instance.Translate("general_invalid_parameter"),
                    FeexRanks.Instance.configNotificationColor);
            }
        }
    }
}