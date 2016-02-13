using Rocket.API;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Freenex.FeexRanks
{
    public class FeexRanksConfiguration : IRocketPluginConfiguration
    {
        public bool EnableRankNotification;
        public bool EnableRankNotificationGlobal;
        public bool EnableRankNotificationOnJoin;
        public string NotificationColor;
        public string NotificationColorGlobal;
        public string NotificationColorPoints;
        public classDatabase FeexRanksDatabase;
        [XmlArrayItem(ElementName = "PointsEvent")]
        public List<classPoints> Points;
        [XmlArrayItem(ElementName = "Rank")]
        public List<classRank> Ranks;

        public void LoadDefaults()
        {
            EnableRankNotification = false;
            EnableRankNotificationGlobal = true;
            EnableRankNotificationOnJoin = true;
            NotificationColor = "green";
            NotificationColorGlobal = "green";
            NotificationColorPoints = "green";

            FeexRanksDatabase = new classDatabase()
            {
                DatabaseAddress = "localhost",
                DatabaseUsername = "unturned",
                DatabasePassword = "password",
                DatabaseName = "unturned",
                DatabaseTableName = "ranks",
                DatabasePort = 3306
            };

            Points = new List<classPoints>()
            {
                new classPoints { EventName = "KILLS_PLAYERS", Notify = true, Points = 60 },
                new classPoints { EventName = "KILLS_ZOMBIES_MEGA", Notify = true, Points = 50 },
                new classPoints { EventName = "KILLS_ZOMBIES_NORMAL", Notify = false, Points = 10 },
            };

            Ranks = new List<classRank>()
            {
                new classRank() { Points = 0, Name = "Pig"},
                new classRank() { Points = 100, Name = "Small Zombie", UconomyReward = true, UconomyNotify = true, UconomyAmount = 100},
                new classRank() { Points = 200, Name = "Zombie", KitReward = true, KitNotify = true, KitName = "zombie"},
                new classRank() { Points = 500, Name = "Giant Zombie", KitReward = true, KitNotify = true, KitName = "giantzombie", UconomyReward = true, UconomyNotify = false, UconomyAmount = 200},
            };
        }
    }

    public class classRank
    {
        public classRank() { }

        public int Points;
        public string Name;
        public bool KitReward = false;
        public bool KitNotify = true;
        public string KitName = string.Empty;
        public bool UconomyReward = false;
        public bool UconomyNotify = true;
        public decimal UconomyAmount = 0;
    }

    public class classPoints
    {
        public classPoints() { }
        public string EventName;
        public bool Notify;
        public int Points;
    }

    public class classDatabase
    {
        public string DatabaseAddress;
        public string DatabaseUsername;
        public string DatabasePassword;
        public string DatabaseName;
        public string DatabaseTableName;
        public int DatabasePort;
    }
}
