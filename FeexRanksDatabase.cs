using MySql.Data.MySqlClient;
using Rocket.Core.Logging;
using System;

namespace Freenex.FeexRanks
{
    public class DatabaseManager
    {
        internal DatabaseManager()
        {
            new I18N.West.CP1250();
            CreateCheckSchema();
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

        public void AddAccount(Steamworks.CSteamID id)
        {
            try
            {
                MySqlConnection connection = CreateConnection();
                MySqlCommand command = connection.CreateCommand();
                connection.Open();
                command.CommandText = "INSERT IGNORE INTO `" + FeexRanks.Instance.Configuration.Instance.FeexRanksDatabase.DatabaseTableName + "` (`steamId`) VALUES ('" + id + "')";
                command.ExecuteNonQuery();
                connection.Close();
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
        }

        public void SetAccount(Steamworks.CSteamID id, int points)
        {
            try
            {
                MySqlConnection connection = CreateConnection();
                MySqlCommand command = connection.CreateCommand();
                connection.Open();
                command.CommandText = "UPDATE `" + FeexRanks.Instance.Configuration.Instance.FeexRanksDatabase.DatabaseTableName + "` SET `points`=" + points + " WHERE `steamId`='" + id + "'";
                command.ExecuteNonQuery();
                connection.Close();
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
        }

        public void UpdateAccount(Steamworks.CSteamID id, int points)
        {
            try
            {
                MySqlConnection connection = CreateConnection();
                MySqlCommand command = connection.CreateCommand();
                connection.Open();
                command.CommandText = "UPDATE `" + FeexRanks.Instance.Configuration.Instance.FeexRanksDatabase.DatabaseTableName + "` SET `points`=`points`+" + points + " WHERE `steamId`='" + id + "'";
                command.ExecuteNonQuery();
                connection.Close();
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
        }

        public bool CheckExists(Steamworks.CSteamID id)
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

        public int GetPoints(Steamworks.CSteamID id)
        {
            int output = 0;
            try
            {
                MySqlConnection connection = CreateConnection();
                MySqlCommand command = new MySqlCommand("SELECT * FROM `" + FeexRanks.Instance.Configuration.Instance.FeexRanksDatabase.DatabaseTableName + "` WHERE `steamId` = '" + id.ToString() + "'", connection);
                connection.Open();
                MySqlDataReader dataReader = command.ExecuteReader();
                while (dataReader.Read())
                {
                    output = Convert.ToInt16(dataReader["points"]);
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

        internal void CreateCheckSchema()
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
                    command.CommandText = "CREATE TABLE `" + FeexRanks.Instance.Configuration.Instance.FeexRanksDatabase.DatabaseTableName + "` (`steamId` VARCHAR(32) NOT NULL, `points` INT(11) NOT NULL DEFAULT '0', `lastUpdated` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP, PRIMARY KEY (`steamId`))";
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
