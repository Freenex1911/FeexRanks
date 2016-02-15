using MySql.Data.MySqlClient;
using Rocket.Core.Logging;
using Rocket.Unturned.Player;
using Steamworks;
using System;
using System.Collections.Generic;

namespace Freenex.FeexRanks
{
    public class DatabaseManager
    {
        internal DatabaseManager()
        {
            new I18N.West.CP1250();
            MySqlConnection connection = CreateConnection();
            try
            {
                connection.Open();
                connection.Close();

                CreateCheckTable();
                CreateCheckView();
            }
            catch (MySqlException ex)
            {
                if (ex.Number == 1042)
                {
                    Logger.LogError("FeexRanks >> Can't connect to MySQL host.");
                }
                else
                {
                    Logger.LogException(ex);
                }
            }
        }

        private MySqlConnection CreateConnection()
        {
            MySqlConnection connection = null;
            try
            {
                if (FeexRanks.Instance.Configuration.Instance.FeexRanksDatabase.DatabasePort == 0) FeexRanks.Instance.Configuration.Instance.FeexRanksDatabase.DatabasePort = 3306;
                connection = new MySqlConnection(String.Format("SERVER={0};DATABASE={1};UID={2};PASSWORD={3};PORT={4};", FeexRanks.Instance.Configuration.Instance.FeexRanksDatabase.DatabaseAddress, FeexRanks.Instance.Configuration.Instance.FeexRanksDatabase.DatabaseName, FeexRanks.Instance.Configuration.Instance.FeexRanksDatabase.DatabaseUsername, FeexRanks.Instance.Configuration.Instance.FeexRanksDatabase.DatabasePassword, FeexRanks.Instance.Configuration.Instance.FeexRanksDatabase.DatabasePort));
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
            return connection;
        }

        public void AddAccount(UnturnedPlayer player)
        {
            try
            {
                MySqlConnection connection = CreateConnection();
                MySqlCommand command = connection.CreateCommand();
                connection.Open();
                command.CommandText = "INSERT IGNORE INTO `" + FeexRanks.Instance.Configuration.Instance.FeexRanksDatabase.DatabaseTableName + "` (`steamId`,`lastDisplayName`) VALUES ('" + player.CSteamID.ToString() + "','" + player.DisplayName + "')";
                command.ExecuteNonQuery();
                connection.Close();
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
        }

        public void UpdateAccount(CSteamID id, int points)
        {
            try
            {
                MySqlConnection connection = CreateConnection();
                MySqlCommand command = connection.CreateCommand();
                connection.Open();
                command.CommandText = "UPDATE `" + FeexRanks.Instance.Configuration.Instance.FeexRanksDatabase.DatabaseTableName + "` SET `points`=`points`+" + points + " WHERE `steamId`='" + id.ToString() + "'";
                command.ExecuteNonQuery();
                connection.Close();
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
        }

        public void SetAccount(CSteamID id, int points)
        {
            try
            {
                MySqlConnection connection = CreateConnection();
                MySqlCommand command = connection.CreateCommand();
                connection.Open();
                command.CommandText = "UPDATE `" + FeexRanks.Instance.Configuration.Instance.FeexRanksDatabase.DatabaseTableName + "` SET `points`=" + points.ToString() + " WHERE `steamId`='" + id.ToString() + "'";
                command.ExecuteNonQuery();
                connection.Close();
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
        }

        public bool CheckAccount(CSteamID id)
        {
            try
            {
                MySqlConnection connection = CreateConnection();
                MySqlCommand command = connection.CreateCommand();
                int exists = 0;
                connection.Open();
                command.CommandText = "SELECT COUNT(1) FROM `" + FeexRanks.Instance.Configuration.Instance.FeexRanksDatabase.DatabaseTableName + "` WHERE `steamId` = '" + id.ToString() + "'";
                object result = command.ExecuteScalar();
                if (result != null) Int32.TryParse(result.ToString(), out exists);
                connection.Close();

                if (exists == 0) { return false; }
                else { return true; }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
                return false;
            }
        }

        public void UpdateDisplayName(CSteamID id, string lastDisplayName)
        {
            try
            {
                MySqlConnection connection = CreateConnection();
                MySqlCommand command = connection.CreateCommand();
                command.CommandText = "UPDATE `" + FeexRanks.Instance.Configuration.Instance.FeexRanksDatabase.DatabaseTableName + "` SET `lastDisplayName` = '" + lastDisplayName + "' WHERE `steamId` = '" + id.ToString() + "'";
                connection.Open();
                command.ExecuteNonQuery();
                connection.Close();
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
        }

        public string[] GetAccountBySteamID(CSteamID id)
        {
            string[] output = new string[3];
            try
            {
                MySqlConnection connection = CreateConnection();
                MySqlCommand command = new MySqlCommand("SELECT * FROM `" + FeexRanks.Instance.Configuration.Instance.FeexRanksDatabase.DatabaseViewName + "` WHERE `steamId` = '" + id.ToString() + "'", connection);
                connection.Open();
                MySqlDataReader dataReader = command.ExecuteReader();
                while (dataReader.Read())
                {
                    output[0] = Convert.ToString(dataReader["points"]);
                    output[1] = Convert.ToString(dataReader["currentRank"]);
                    output[2] = Convert.ToString(dataReader["lastDisplayName"]);
                }
                dataReader.Close();
                connection.Close();
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
            return output;
        }

        public string[] GetAccountByRank(string rank)
        {
            string[] output = new string[3];
            try
            {
                MySqlConnection connection = CreateConnection();
                MySqlCommand command = new MySqlCommand("SELECT * FROM `" + FeexRanks.Instance.Configuration.Instance.FeexRanksDatabase.DatabaseViewName + "` WHERE `currentRank` = '" + rank + "'", connection);
                connection.Open();
                MySqlDataReader dataReader = command.ExecuteReader();
                while (dataReader.Read())
                {
                    output[0] = Convert.ToString(dataReader["points"]);
                    output[1] = Convert.ToString(dataReader["currentRank"]);
                    output[2] = Convert.ToString(dataReader["lastDisplayName"]);
                }
                dataReader.Close();
                connection.Close();
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
            return output;
        }

        public string[] GetTopRanks(string limit)
        {
            string[] output;
            List<string> listOutput = new List<string>();
            try
            {
                MySqlConnection connection = CreateConnection();
                MySqlCommand command = new MySqlCommand("SELECT * FROM (SELECT * FROM `" + FeexRanks.Instance.Configuration.Instance.FeexRanksDatabase.DatabaseViewName + "` WHERE `currentRank` ORDER BY `currentRank` DESC LIMIT " + limit + ") AS tbl ORDER BY `currentRank` ASC", connection);
                connection.Open();
                MySqlDataReader dataReader = command.ExecuteReader();
                while (dataReader.Read())
                {
                    listOutput.Add(Convert.ToString(dataReader["points"]));
                    listOutput.Add(Convert.ToString(dataReader["currentRank"]));
                    listOutput.Add(Convert.ToString(dataReader["lastDisplayName"]));
                }
                dataReader.Close();
                connection.Close();
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
            return output = listOutput.ToArray();
        }

        internal void CreateCheckTable()
        {
            try
            {
                MySqlConnection connection = CreateConnection();
                MySqlCommand command = connection.CreateCommand();
                connection.Open();
                command.CommandText = "SHOW TABLES LIKE '" + FeexRanks.Instance.Configuration.Instance.FeexRanksDatabase.DatabaseTableName + "'";
                object test = command.ExecuteScalar();
                if (test == null)
                {
                    command.CommandText = "CREATE TABLE `" + FeexRanks.Instance.Configuration.Instance.FeexRanksDatabase.DatabaseTableName + "` (`steamId` VARCHAR(32) NOT NULL, `points` INT(11) NOT NULL DEFAULT '0', `lastDisplayName` varchar(32) NOT NULL, `lastUpdated` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP, PRIMARY KEY (`steamId`))";
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
        }

        internal void CreateCheckView()
        {
            try
            {
                MySqlConnection connection = CreateConnection();
                MySqlCommand command = connection.CreateCommand();
                connection.Open();
                command.CommandText = "SHOW FULL TABLES LIKE '" + FeexRanks.Instance.Configuration.Instance.FeexRanksDatabase.DatabaseViewName + "'";
                object test = command.ExecuteScalar();
                if (test == null)
                {
                    command.CommandText = "CREATE VIEW `" + FeexRanks.Instance.Configuration.Instance.FeexRanksDatabase.DatabaseViewName + "` AS SELECT steamId, points, lastDisplayName, (select count(1) FROM " + FeexRanks.Instance.Configuration.Instance.FeexRanksDatabase.DatabaseTableName + " b WHERE  b.points > a.points) + 1 AS currentRank FROM " + FeexRanks.Instance.Configuration.Instance.FeexRanksDatabase.DatabaseTableName + " AS a";
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
        }

    }
}
