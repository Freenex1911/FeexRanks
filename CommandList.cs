using System.Collections.Generic;
using System.Linq;
using Rocket.API;
using Rocket.Unturned.Chat;

namespace Freenex.FeexRanks
{
    public class CommandList : IRocketCommand
    {
        public string Name => "list";

        public string Help => "Display top players or get user by rank";

        public string Syntax => "[<rank>]";

        public List<string> Aliases => new List<string>();

        public AllowedCaller AllowedCaller => AllowedCaller.Both;

        public List<string> Permissions =>
            new List<string>
            {
                "list",
                "list.other"
            };

        public void Execute(IRocketPlayer caller, params string[] command)
        {
            switch (command.Length)
            {
                case 0:
                {
                    var topRanks = FeexRanks.Instance.FeexRanksDatabase.GetTopRanks(3).ToList();
                    UnturnedChat.Say(caller,
                        FeexRanks.Instance.Translate("list_1", FeexRanks.Instance.configNotificationColor));

                    if (topRanks.Count == 0)
                    {
                        UnturnedChat.Say(caller, "No Ranking");
                        return;
                    }

                    for (var i = 0; i < topRanks.Count; i++)
                    {
                        var ranking = topRanks[i];
                        UnturnedChat.Say(caller,
                            FeexRanks.Instance.Translate($"list_{i + 2}", ranking.Points, ranking.CurrentRank,
                                FeexRanks.Instance.GetLevel(int.Parse(ranking.Points)).Name, ranking.LastDisplayName),
                            FeexRanks.Instance.configNotificationColor);
                    }

                    break;
                }

                case 1:
                {
                    if (!caller.HasPermission("list.other"))
                    {
                        UnturnedChat.Say(caller, "No perms");
                        return;
                    }

                    if (!int.TryParse(command[0], out var rank))
                    {
                        UnturnedChat.Say(caller, "NaN");
                        return;
                    }

                    var ranking = FeexRanks.Instance.FeexRanksDatabase.GetAccountByRank(rank);

                    if (ranking == null)
                    {
                        UnturnedChat.Say(caller,
                            FeexRanks.Instance.Translate("list_search_not_found"),
                            FeexRanks.Instance.configNotificationColor);
                        return;
                    }

                    UnturnedChat.Say(caller,
                        FeexRanks.Instance.Translate("list_search", ranking.Points,
                            ranking.CurrentRank, FeexRanks.Instance.GetLevel(int.Parse(ranking.Points)).Name,
                            ranking.LastDisplayName), FeexRanks.Instance.configNotificationColor);
                    break;
                }

                default:
                    UnturnedChat.Say(caller,
                        FeexRanks.Instance.Translate("general_invalid_parameter"),
                        FeexRanks.Instance.configNotificationColor);
                    break;
            }
        }
    }
}