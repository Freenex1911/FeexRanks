using MySql.Data.MySqlClient;
using Rocket.Core.Logging;
using System;
using System.Collections.Generic;
using System.Threading;

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

                CreateCheckSchema();
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
                FeexRanks.Instance.ForceUnload();
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

        #region AddUpdatePlayerAsync
        public void AddUpdatePlayer(string steamId, string lastDisplayName)
        {
            try
            {
                AddUpdatePlayerParameter classAddUpdatePlayerParameter = new AddUpdatePlayerParameter(steamId, lastDisplayName);
                ThreadPool.QueueUserWorkItem
                    (new WaitCallback(AddUpdatePlayerThread), classAddUpdatePlayerParameter);
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
        }

        private void AddUpdatePlayerThread(object objectAddUpdatePlayerParameter)
        {
            try
            {
                AddUpdatePlayerParameter classAddUpdatePlayerParameter = (AddUpdatePlayerParameter)objectAddUpdatePlayerParameter;
                MySqlConnection connection = CreateConnection();
                MySqlCommand command = connection.CreateCommand();
                command.CommandText = "INSERT INTO `" + FeexRanks.Instance.Configuration.Instance.FeexRanksDatabase.DatabaseTableName + "` (`steamId`,`lastDisplayName`) VALUES (@steamId,@lastDisplayName) ON DUPLICATE KEY UPDATE lastDisplayName = @lastDisplayName";
                command.Parameters.AddWithValue("@steamId", classAddUpdatePlayerParameter.steamId);
                command.Parameters.AddWithValue("@lastDisplayName", classAddUpdatePlayerParameter.lastDisplayName);
                connection.Open();
                command.ExecuteNonQuery();
                connection.Dispose();
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
        }

        private class AddUpdatePlayerParameter
        {
            public string steamId;
            public string lastDisplayName;

            public AddUpdatePlayerParameter(string steamId, string lastDisplayName)
            {
                this.steamId = steamId;
                this.lastDisplayName = lastDisplayName;
            }
        }
        #endregion

        #region AddPointsAsync
        public void AddPoints(string steamId, int points)
        {
            try
            {
                AddSetPointsParameter classAddSetPointsParameter = new AddSetPointsParameter(steamId, points);
                ThreadPool.QueueUserWorkItem
                    (new WaitCallback(AddPointsThread), classAddSetPointsParameter);
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
        }

        private void AddPointsThread(object objectAddSetPointsParameter)
        {
            try
            {
                AddSetPointsParameter classAddSetPointsParameter = (AddSetPointsParameter)objectAddSetPointsParameter;
                MySqlConnection connection = CreateConnection();
                MySqlCommand command = connection.CreateCommand();
                command.CommandText = "UPDATE `" + FeexRanks.Instance.Configuration.Instance.FeexRanksDatabase.DatabaseTableName + "` SET `points`=`points`+" + classAddSetPointsParameter.points.ToString() + " WHERE `steamId`=@steamId";
                command.Parameters.AddWithValue("@steamId", classAddSetPointsParameter.steamId);
                connection.Open();
                command.ExecuteNonQuery();
                connection.Dispose();
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
        }
        #endregion

        #region SetPointsAsync
        public void SetPoints(string steamId, int points)
        {
            try
            {
                AddSetPointsParameter classAddSetPointsParameter = new AddSetPointsParameter(steamId, points);
                ThreadPool.QueueUserWorkItem
                    (new WaitCallback(SetPointsThread), classAddSetPointsParameter);
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
        }

        private void SetPointsThread(object objectAddSetPointsParameter)
        {
            try
            {
                AddSetPointsParameter classAddSetPointsParameter = (AddSetPointsParameter)objectAddSetPointsParameter;
                MySqlConnection connection = CreateConnection();
                MySqlCommand command = connection.CreateCommand();
                command.CommandText = "UPDATE `" + FeexRanks.Instance.Configuration.Instance.FeexRanksDatabase.DatabaseTableName + "` SET `points`=" + classAddSetPointsParameter.points.ToString() + " WHERE `steamId`=@steamId";
                command.Parameters.AddWithValue("@steamId", classAddSetPointsParameter.steamId);
                connection.Open();
                command.ExecuteNonQuery();
                connection.Dispose();
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
        }
        #endregion

        private class AddSetPointsParameter
        {
            public string steamId;
            public int points;

            public AddSetPointsParameter(string steamId, int points)
            {
                this.steamId = steamId;
                this.points = points;
            }
        }

        public string[] GetAccountBySteamID(string steamId)
        {
            string[] output = new string[3];
            try
            {
                MySqlConnection connection = CreateConnection();
                MySqlCommand command = new MySqlCommand("SELECT * FROM (SELECT t.steamId, t.points, t.lastDisplayName, @`rownum` := @`rownum` + 1 AS currentRank FROM `" + FeexRanks.Instance.Configuration.Instance.FeexRanksDatabase.DatabaseTableName + "` t JOIN (SELECT @`rownum` := 0) r ORDER BY t.points DESC) x WHERE x.steamId = @steamId;", connection);
                command.Parameters.AddWithValue("@steamId", steamId);
                connection.Open();
                MySqlDataReader dataReader = command.ExecuteReader(System.Data.CommandBehavior.SingleRow);
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

        public string[] GetAccountByRank(int rank)
        {
            string[] output = new string[3];
            try
            {
                MySqlConnection connection = CreateConnection();
                MySqlCommand command = new MySqlCommand("SELECT * FROM (SELECT t.steamId, t.points, t.lastDisplayName, @`rownum` := @`rownum` + 1 AS currentRank FROM `" + FeexRanks.Instance.Configuration.Instance.FeexRanksDatabase.DatabaseTableName + "` t JOIN (SELECT @`rownum` := 0) r ORDER BY t.points DESC) x WHERE x.currentRank = " + rank.ToString() + ";", connection);
                connection.Open();
                MySqlDataReader dataReader = command.ExecuteReader(System.Data.CommandBehavior.SingleRow);
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

        public int GetRankBySteamID(string steamId)
        {
            int output = 0;
            try
            {
                MySqlConnection connection = CreateConnection();
                MySqlCommand command = connection.CreateCommand();
                command.CommandText = "SELECT `currentRank` FROM (SELECT t.steamId, t.points, @`rownum` := @`rownum` + 1 AS currentRank FROM `" + FeexRanks.Instance.Configuration.Instance.FeexRanksDatabase.DatabaseTableName + "` t JOIN (SELECT @`rownum` := 0) r ORDER BY t.points DESC) x WHERE x.steamId = @steamId;";
                command.Parameters.AddWithValue("@steamId", steamId);
                connection.Open();
                object result = command.ExecuteScalar();
                if (result != null) int.TryParse(result.ToString(), out output);
                connection.Close();
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
            return output;
        }

        public string[] GetTopRanks(int limit)
        {
            string[] output;
            List<string> listOutput = new List<string>();
            try
            {
                MySqlConnection connection = CreateConnection();
                MySqlCommand command = new MySqlCommand("SELECT * FROM (SELECT t.steamId, t.points, t.lastDisplayName, @`rownum` := @`rownum` + 1 AS currentRank FROM `" + FeexRanks.Instance.Configuration.Instance.FeexRanksDatabase.DatabaseTableName + "` t JOIN (SELECT @`rownum` := 0) r ORDER BY t.points DESC) x LIMIT 0," + limit.ToString() + ";", connection);
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

        internal void CreateCheckSchema()
        {
            try
            {
                MySqlConnection connection = CreateConnection();
                MySqlCommand command = connection.CreateCommand();
                connection.Open();
                command.CommandText = "SHOW TABLES LIKE '" + FeexRanks.Instance.Configuration.Instance.FeexRanksDatabase.DatabaseTableName + "';";
                object test = command.ExecuteScalar();
                if (test == null)
                {
                    command.CommandText = "CREATE TABLE `" + FeexRanks.Instance.Configuration.Instance.FeexRanksDatabase.DatabaseTableName + "` (`steamId` VARCHAR(32) NOT NULL, `points` INT(32) NOT NULL DEFAULT '0', `lastDisplayName` varchar(32) NOT NULL, `lastUpdated` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP, PRIMARY KEY (`steamId`));";
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
