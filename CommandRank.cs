using System.Collections.Generic;
using Rocket.API;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using Steamworks;

namespace Freenex.FeexRanks
{
    public class CommandRank : IRocketCommand
    {
        public string Name => "rank";

        public string Help => "Display current rank or get user by name";

        public string Syntax => "[<player>]";

        public List<string> Aliases => new List<string>();

        public AllowedCaller AllowedCaller => AllowedCaller.Both;

        public List<string> Permissions =>
            new List<string>
            {
                "rank",
                "rank.other"
            };

        public void Execute(IRocketPlayer caller, params string[] command)
        {
            switch (command.Length)
            {
                case 0:
                {
                    if (FeexRanks.DicPoints.TryGetValue(new CSteamID(ulong.Parse(caller.Id)), out var playerPoints))
                        UnturnedChat.Say(caller,
                            FeexRanks.Instance.Translations.Instance.Translate("rank_self", playerPoints,
                                FeexRanks.Instance.FeexRanksDatabase.GetRankBySteamId(caller.Id),
                                FeexRanks.Instance.GetLevel(playerPoints).Name),
                            FeexRanks.Instance.configNotificationColor);
                    break;
                }

                case 1:
                {
                    if (!caller.HasPermission("rank.other"))
                    {
                        UnturnedChat.Say(caller, "No Perms");
                        return;
                    }

                    var otherPlayer = UnturnedPlayer.FromName(command[0]);
                    if (otherPlayer == null)
                    {
                        UnturnedChat.Say(caller, FeexRanks.Instance.Translate("general_not_found"),
                            FeexRanks.Instance.configNotificationColor);
                    }
                    else
                    {
                        if (FeexRanks.DicPoints.TryGetValue(otherPlayer.CSteamID, out var playerPoints))
                            UnturnedChat.Say(caller,
                                FeexRanks.Instance.Translate("rank_other", playerPoints,
                                    FeexRanks.Instance.FeexRanksDatabase.GetRankBySteamId(
                                        otherPlayer.CSteamID.ToString()),
                                    FeexRanks.Instance.GetLevel(playerPoints).Name, otherPlayer.DisplayName),
                                FeexRanks.Instance.configNotificationColor);
                    }

                    break;
                }

                default:
                {
                    UnturnedChat.Say(caller, FeexRanks.Instance.Translate("general_invalid_parameter"),
                        FeexRanks.Instance.configNotificationColor);
                    break;
                }
            }
        }
    }
}