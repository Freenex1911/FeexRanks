using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Freenex.FeexRanks.Configuration;
using I18N.West;
using MySql.Data.MySqlClient;
using Rocket.Core.Logging;

namespace Freenex.FeexRanks.Database
{
    public class DatabaseManager
    {
        internal DatabaseManager()
        {
            new CP1250();

            CreateCheckSchema();
        }

        private MySqlConnection CreateConnection()
        {
            MySqlConnection connection = null;
            try
            {
                if (FeexRanks.Instance.Configuration.Instance.FeexRanksDatabaseConfig.DatabasePort == 0)
                    FeexRanks.Instance.Configuration.Instance.FeexRanksDatabaseConfig.DatabasePort = 3306;
                connection = new MySqlConnection(
                    $"SERVER={FeexRanks.Instance.Configuration.Instance.FeexRanksDatabaseConfig.DatabaseAddress};DATABASE={FeexRanks.Instance.Configuration.Instance.FeexRanksDatabaseConfig.DatabaseName};UID={FeexRanks.Instance.Configuration.Instance.FeexRanksDatabaseConfig.DatabaseUsername};PASSWORD={FeexRanks.Instance.Configuration.Instance.FeexRanksDatabaseConfig.DatabasePassword};PORT={FeexRanks.Instance.Configuration.Instance.FeexRanksDatabaseConfig.DatabasePort};");
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }

            return connection;
        }

        public void AddUpdatePlayer(string steamId, string lastDisplayName)
        {
            try
            {
                ThreadPool.QueueUserWorkItem(yes => AddUpdatePlayerThread(steamId, lastDisplayName));
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
        }

        private void AddUpdatePlayerThread(string steamId, string lastDisplayName)
        {
            ExecuteQuery(EQueryType.NonQuery,
                $"INSERT INTO `{FeexRanks.Instance.Configuration.Instance.FeexRanksDatabaseConfig.DatabaseTableName}` (`steamId`,`lastDisplayName`) VALUES (@steamId,@lastDisplayName) ON DUPLICATE KEY UPDATE lastDisplayName = @lastDisplayName;",
                new MySqlParameter("@lastDisplayName", lastDisplayName), new MySqlParameter("@steamId", steamId));
        }

        public void AddPoints(string steamId, int points)
        {
            try
            {
                ThreadPool.QueueUserWorkItem(yes => AddPointsThread(steamId, points));
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
        }

        private void AddPointsThread(string steamId, int points)
        {
            ExecuteQuery(EQueryType.NonQuery,
                $"UPDATE `{FeexRanks.Instance.Configuration.Instance.FeexRanksDatabaseConfig.DatabaseTableName}` SET `points`=`points`+{points} WHERE `steamId`=@steamId;",
                new MySqlParameter("@steamId", steamId));
        }

        public void SetPoints(string steamId, int points)
        {
            try
            {
                ThreadPool.QueueUserWorkItem(yes => SetPointsThread(steamId, points));
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
        }

        private void SetPointsThread(string steamId, int points)
        {
            ExecuteQuery(EQueryType.NonQuery,
                $"UPDATE `{FeexRanks.Instance.Configuration.Instance.FeexRanksDatabaseConfig.DatabaseTableName}` SET `points`={points} WHERE `steamId`=@steamId;",
                new MySqlParameter("@steamId", steamId));
        }

        public PlayerRank GetAccountBySteamId(string steamId)
        {
            var readerResult = (List<Row>) ExecuteQuery(EQueryType.Reader,
                $"SELECT * FROM (SELECT t.steamId, t.points, t.lastDisplayName, @`rownum` := @`rownum` + 1 AS currentRank FROM `{FeexRanks.Instance.Configuration.Instance.FeexRanksDatabaseConfig.DatabaseTableName}` t JOIN (SELECT @`rownum` := 0) r ORDER BY t.points DESC) x WHERE x.steamId = @steamId;",
                new MySqlParameter("@steamId", steamId));

            return readerResult?.Select(k => new PlayerRank
            {
                Points = k.Values["points"].ToString(), CurrentRank = k.Values["currentRank"].ToString(),
                LastDisplayName = k.Values["lastDisplayName"].ToString()
            }).First();
        }

        public PlayerRank GetAccountByRank(int rank)
        {
            var readerResult = (List<Row>) ExecuteQuery(EQueryType.Reader,
                $"SELECT * FROM (SELECT t.steamId, t.points, t.lastDisplayName, @`rownum` := @`rownum` + 1 AS currentRank FROM `{FeexRanks.Instance.Configuration.Instance.FeexRanksDatabaseConfig.DatabaseTableName}` t JOIN (SELECT @`rownum` := 0) r ORDER BY t.points DESC) x WHERE x.currentRank = {rank};");

            return readerResult?.Select(k => new PlayerRank
            {
                Points = k.Values["points"].ToString(), CurrentRank = k.Values["currentRank"].ToString(),
                LastDisplayName = k.Values["lastDisplayName"].ToString()
            }).First();
        }

        public int GetRankBySteamId(string steamId)
        {
            var output = 0;
            var result = ExecuteQuery(EQueryType.Scalar,
                $"SELECT `currentRank` FROM (SELECT t.steamId, t.points, @`rownum` := @`rownum` + 1 AS currentRank FROM `{FeexRanks.Instance.Configuration.Instance.FeexRanksDatabaseConfig.DatabaseTableName}` t JOIN (SELECT @`rownum` := 0) r ORDER BY t.points DESC) x WHERE x.steamId = @steamId;",
                new MySqlParameter("@steamId", steamId));

            if (result != null) int.TryParse(result.ToString(), out output);

            return output;
        }

        public IEnumerable<PlayerRank> GetTopRanks(int limit)
        {
            var readerResult = (List<Row>) ExecuteQuery(EQueryType.Reader,
                $"SELECT * FROM (SELECT t.steamId, t.points, t.lastDisplayName, @`rownum` := @`rownum` + 1 AS currentRank FROM `{FeexRanks.Instance.Configuration.Instance.FeexRanksDatabaseConfig.DatabaseTableName}` t JOIN (SELECT @`rownum` := 0) r ORDER BY t.points DESC) x LIMIT 0,{limit};");

            return readerResult?.Select(row => new PlayerRank
            {
                Points = row.Values["points"].ToString(), CurrentRank = row.Values["currentRank"].ToString(),
                LastDisplayName = row.Values["lastDisplayName"].ToString()
            });
        }

        internal void CreateCheckSchema()
        {
            var test = ExecuteQuery(EQueryType.Scalar,
                $"SHOW TABLES LIKE '{FeexRanks.Instance.Configuration.Instance.FeexRanksDatabaseConfig.DatabaseTableName}';");

            if (test == null)
                ExecuteQuery(EQueryType.NonQuery,
                    $"CREATE TABLE `{FeexRanks.Instance.Configuration.Instance.FeexRanksDatabaseConfig.DatabaseTableName}` (`steamId` VARCHAR(32) NOT NULL, `points` INT(32) NOT NULL DEFAULT '0', `lastDisplayName` varchar(32) NOT NULL, `lastUpdated` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP, PRIMARY KEY (`steamId`));");
        }

        private object ExecuteQuery(EQueryType queryType, string query, params MySqlParameter[] parameters)
        {
            object result = null;
            MySqlDataReader reader = null;

            using (var connection = CreateConnection())
            {
                try
                {
                    var command = connection.CreateCommand();
                    command.CommandText = query;

                    foreach (var parameter in parameters)
                        command.Parameters.Add(parameter);

                    connection.Open();
                    switch (queryType)
                    {
                        case EQueryType.Reader:
                            var readerResult = new List<Row>();

                            reader = command.ExecuteReader();
                            while (reader.Read())
                                try
                                {
                                    var values = new Dictionary<string, object>();

                                    for (var i = 0; i < reader.FieldCount; i++)
                                    {
                                        var columnName = reader.GetName(i);
                                        values.Add(columnName, reader[columnName]);
                                    }

                                    readerResult.Add(new Row {Values = values});
                                }
                                catch (Exception ex)
                                {
                                    Logger.LogError(
                                        $"The following query threw an error during reader execution:\nQuery: \"{query}\"\nError: {ex.Message}");
                                }

                            result = readerResult;
                            break;
                        case EQueryType.Scalar:
                            result = command.ExecuteScalar();
                            break;
                        case EQueryType.NonQuery:
                            result = command.ExecuteNonQuery();
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogException(ex);
                }
                finally
                {
                    reader?.Close();
                    connection.Close();
                }
            }

            return result;
        }
    }
}