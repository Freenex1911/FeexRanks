using Rocket.API;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Freenex.FeexRanks
{
    public class CommandList : IRocketCommand
    {
        public string Name
        {
            get { return "list"; }
        }
        public string Help
        {
            get { return "View ranks by position"; }
        }

        public string Syntax
        {
            get { return "[<position>]"; }
        }

        public List<string> Aliases
        {
            get { return new List<string>(); }
        }

        public AllowedCaller AllowedCaller
        {
            get { return AllowedCaller.Player; }
        }

        public List<string> Permissions
        {
            get
            {
                return new List<string>()
                {
                    "list",
                    "list.other"
                };
            }
        }

        public void Execute(IRocketPlayer caller, params string[] command)
        {
            UnturnedPlayer callerPlayer = (UnturnedPlayer)caller;

            if (command.Length == 0)
            {
                string[] rankInfo;
                UnturnedChat.Say(caller, FeexRanks.Instance.Translations.Instance.Translate("list_1"), UnturnedChat.GetColorFromName(FeexRanks.Instance.Configuration.Instance.NotificationColor, Color.green));

                rankInfo = FeexRanks.Instance.FeexRanksDatabase.GetAccountByPosition("1");
                if (rankInfo[0] != null)
                {
                    UnturnedChat.Say(caller, FeexRanks.Instance.Translations.Instance.Translate("list_2", rankInfo[0], rankInfo[1], FeexRanks.Instance.GetRankName(Convert.ToInt16(rankInfo[0])).Name, rankInfo[2]), UnturnedChat.GetColorFromName(FeexRanks.Instance.Configuration.Instance.NotificationColor, Color.green));
                }
                rankInfo = FeexRanks.Instance.FeexRanksDatabase.GetAccountByPosition("2");
                if (rankInfo[0] != null)
                {
                    UnturnedChat.Say(caller, FeexRanks.Instance.Translations.Instance.Translate("list_3", rankInfo[0], rankInfo[1], FeexRanks.Instance.GetRankName(Convert.ToInt16(rankInfo[0])).Name, rankInfo[2]), UnturnedChat.GetColorFromName(FeexRanks.Instance.Configuration.Instance.NotificationColor, Color.green));
                }
                rankInfo = FeexRanks.Instance.FeexRanksDatabase.GetAccountByPosition("3");
                if (rankInfo[0] != null)
                {
                    UnturnedChat.Say(caller, FeexRanks.Instance.Translations.Instance.Translate("list_4", rankInfo[0], rankInfo[1], FeexRanks.Instance.GetRankName(Convert.ToInt16(rankInfo[0])).Name, rankInfo[2]), UnturnedChat.GetColorFromName(FeexRanks.Instance.Configuration.Instance.NotificationColor, Color.green));
                }
            }
            else if (command.Length == 1 && callerPlayer.HasPermission("list.other"))
            {
                string[] rankInfo = FeexRanks.Instance.FeexRanksDatabase.GetAccountByPosition(command[0]); ;
                if (rankInfo[0] == null)
                {
                    UnturnedChat.Say(caller, FeexRanks.Instance.Translations.Instance.Translate("list_search_not_found"), UnturnedChat.GetColorFromName(FeexRanks.Instance.Configuration.Instance.NotificationColor, Color.green));
                }
                else
                {
                    UnturnedChat.Say(caller, FeexRanks.Instance.Translations.Instance.Translate("list_search", rankInfo[0], rankInfo[1], FeexRanks.Instance.GetRankName(Convert.ToInt16(rankInfo[0])).Name, rankInfo[2]), UnturnedChat.GetColorFromName(FeexRanks.Instance.Configuration.Instance.NotificationColor, Color.green));
                }
            }
            else
            {
                UnturnedChat.Say(caller, FeexRanks.Instance.Translations.Instance.Translate("rank_general_invalid_parameter"), UnturnedChat.GetColorFromName(FeexRanks.Instance.Configuration.Instance.NotificationColor, Color.green));
            }
        }

    }
}
