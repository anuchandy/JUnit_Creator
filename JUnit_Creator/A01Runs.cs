using Npgsql;
using System;
using System.Collections.Generic;

namespace JUnit_Creator
{
    public class A01Runs
    {
        public A01Runs(Config.DBConfig dBConfig)
        {
            this.ConnectionString = $"Host={dBConfig.Host};Username={dBConfig.UserName};Password={dBConfig.Password};Database={dBConfig.Database}";
        }

        public string ConnectionString { get; }

        public IEnumerable<A01Run> RetrieveCompletedRuns()
        {
            using (var connection = new NpgsqlConnection(this.ConnectionString))
            {
                connection.Open();
                // Query all Completed Java Official Runs.
                //
                string runsQuery = $@"SELECT {A01Run.FIELDS_TO_SELECT}
                    FROM run
                    WHERE (settings::json->> 'a01.reserved.remark')::VARCHAR = 'Official'
                    AND (details::json->> 'a01.reserved.product')::VARCHAR = 'javasdk'
                    AND status = 'Completed'";
                using (var runCmd = new NpgsqlCommand(runsQuery, connection))
                {
                    using (var dataReader = runCmd.ExecuteReader())
                    {
                        while (dataReader.Read())
                        {
                            yield return new A01Run(a01Runs: this, dataReader: dataReader);
                        }
                    }
                }
            }
        }

        public IEnumerable<A01Run> RetrieveCompletedRuns(IEnumerable<int> runIds)
        {
            string runIdsRange = $"'{{{string.Join(",", runIds)}}}'::int[]";
            using (var connection = new NpgsqlConnection(this.ConnectionString))
            {
                connection.Open();
                // Query Completed Java Official Runs with the given ids.
                //
                string runsQuery = $@"SELECT {A01Run.FIELDS_TO_SELECT}
                    FROM run
                    WHERE (settings::json->> 'a01.reserved.remark')::VARCHAR = 'Official'
                    AND (details::json->> 'a01.reserved.product')::VARCHAR = 'javasdk'
                    AND status = 'Completed'
                    AND id = ANY ({runIdsRange})";
                using (var runCmd = new NpgsqlCommand(runsQuery, connection))
                {
                    using (var runReader = runCmd.ExecuteReader())
                    {
                        while (runReader.Read())
                        {
                            yield return new A01Run(a01Runs: this, dataReader: runReader);
                        }
                    }
                }
            }
        }

        public IEnumerable<A01Run> RetrieveCompletedRuns(DateTime utcStartTime, DateTime utcEndTime)
        {
            if (utcStartTime > utcEndTime)
            {
                throw new ArgumentException("startDateTime must be before endDateTime");
            }
            //
            string startTimeStamp = $"timestamp '{utcStartTime.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'")}'";
            string endTimeStamp =   $"timestamp '{utcEndTime.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'")}'";
            //
            using (var connection = new NpgsqlConnection(this.ConnectionString))
            {
                connection.Open();
                // Query all Completed Java Official Runs between provided dates.
                //
                string runsQuery = $@"SELECT {A01Run.FIELDS_TO_SELECT}
                    FROM run
                    WHERE (settings::json->> 'a01.reserved.remark')::VARCHAR = 'Official'
                    AND (details::json->> 'a01.reserved.product')::VARCHAR = 'javasdk'
                    AND status = 'Completed'
                    AND creation >= {startTimeStamp} AND creation <= {endTimeStamp}";
                //
                using (var runCmd = new NpgsqlCommand(runsQuery, connection))
                {
                    using (var dataReader = runCmd.ExecuteReader())
                    {
                        while (dataReader.Read())
                        {
                            yield return new A01Run(a01Runs: this, dataReader: dataReader);
                        }
                    }
                }
            }
        }
    }
}
