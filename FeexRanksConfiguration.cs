using Rocket.API;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Freenex.FeexRanks
{
    public class FeexRanksConfiguration : IRocketPluginConfiguration
    {
        public bool EnableLevelUpNotification;
        public bool EnableLevelUpNotificationGlobal;
        public bool EnableRankNotificationOnJoin;
        public bool EnableRankNotificationOnJoinGlobal;
        public bool EnableRankNotificationOnLeaveGlobal;
        public string NotificationColor;
        public string NotificationColorGlobal;
        public string NotificationColorJoinLeaveGlobal;
        public classDatabase FeexRanksDatabase;
        [XmlArrayItem(ElementName = "Event")]
        public List<classEvent> Events;
        [XmlArrayItem(ElementName = "Level")]
        public List<classLevel> Level;

        public void LoadDefaults()
        {
            EnableLevelUpNotification = false;
            EnableLevelUpNotificationGlobal = true;
            EnableRankNotificationOnJoin = true;
            EnableRankNotificationOnJoinGlobal = false;
            EnableRankNotificationOnLeaveGlobal = false;
            NotificationColor = "Green";
            NotificationColorGlobal = "Gray";
            NotificationColorJoinLeaveGlobal = "Green";

            FeexRanksDatabase = new classDatabase()
            {
                DatabaseAddress = "localhost",
                DatabaseUsername = "unturned",
                DatabasePassword = "password",
                DatabaseName = "unturned",
                DatabaseTableName = "ranks",
                DatabaseViewName = "ranks_view",
                DatabasePort = 3306,
                QueryInterval = 10
            };

            Events = new List<classEvent>()
            {
                new classEvent { EventName = "KILLS_ZOMBIES_NORMAL", Notify = false, Points = 10 },
                new classEvent { EventName = "KILLS_ZOMBIES_MEGA", Notify = true, Points = 50 },
                new classEvent { EventName = "KILLS_PLAYERS", Notify = true, Points = 60 }
            };

            Level = new List<classLevel>()
            {
                new classLevel() { Points = 0, Name = "Pig"},
                new classLevel() { Points = 100, Name = "Small Zombie", UconomyReward = true, UconomyNotify = true, UconomyAmount = 100},
                new classLevel() { Points = 200, Name = "Zombie", KitReward = true, KitNotify = true, KitName = "Zombie"},
                new classLevel() { Points = 500, Name = "Giant Zombie", KitReward = true, KitNotify = true, KitName = "Giant Zombie", UconomyReward = true, UconomyNotify = false, UconomyAmount = 200}
            };
        }
    }

    public class classDatabase
    {
        public string DatabaseAddress;
        public string DatabaseUsername;
        public string DatabasePassword;
        public string DatabaseName;
        public string DatabaseTableName;
        public string DatabaseViewName;
        public int DatabasePort;
        public int QueryInterval;
    }

    public class classEvent
    {
        public classEvent() { }
        public string EventName;
        public bool Notify;
        public int Points;
    }

    public class classLevel
    {
        public classLevel() { }
        public int Points = 0;
        public string Name = string.Empty;
        public bool KitReward = false;
        public bool KitNotify = true;
        public string KitName = string.Empty;
        public bool UconomyReward = false;
        public bool UconomyNotify = true;
        public decimal UconomyAmount = 0;
    }
}
