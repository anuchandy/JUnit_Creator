using Npgsql;
using System;
using System.Collections.Generic;

namespace JUnit_Creator
{
    public class A01Run
    {
        public static readonly string FIELDS_TO_SELECT = "id, creation";

        public A01Run(A01Runs a01Runs, NpgsqlDataReader dataReader)
        {
            this.A01Runs = a01Runs;
            this.RunId = dataReader.GetInt64(0);
            this.CreationTimeInUtc = dataReader.GetTimeStamp(1)
                .ToDateTime()
                .ToUniversalTime()
                .ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'");
        }

        A01Runs A01Runs
        {
            get;
        }

        public long RunId
        {
            get; private set;
        }

        public string CreationTimeInUtc
        {
            get; private set;
        }

        public IEnumerable<A01RunTask> RetrieveFailedTasks()
        {
            using (var conn2 = new NpgsqlConnection(this.A01Runs.ConnectionString))
            {
                conn2.Open();
                // Query all Failed tasks in this run
                //
                string failedTasksQuery = $@"SELECT {A01RunTask.FIELDS_TO_SELECT}
                                FROM task
                                WHERE run_id = {this.RunId}
                                AND result = 'Failed'
                                AND (result_details::json->> 'a01.reserved.tasklogpath')::VARCHAR IS NOT NULL";

                using (var taskCmd = new NpgsqlCommand(failedTasksQuery, conn2))
                {
                    using (var dataReader = taskCmd.ExecuteReader())
                    {
                        while (dataReader.Read())
                        {
                            yield return new A01RunTask(a01Run: this, dataReader: dataReader, isSucceededTask: false);
                        }
                    }
                }
            }
        }

        public IEnumerable<A01RunTask> RetrieveSucceededTasks()
        {
            using (var conn2 = new NpgsqlConnection(this.A01Runs.ConnectionString))
            {
                conn2.Open();
                // Query all Passed tasks in this run
                //
                string failedTasksQuery = $@"SELECT {A01RunTask.FIELDS_TO_SELECT}
                                FROM task
                                WHERE run_id = {this.RunId}
                                AND result = 'Passed'
                                AND (result_details::json->> 'a01.reserved.tasklogpath')::VARCHAR IS NOT NULL";

                using (var taskCmd = new NpgsqlCommand(failedTasksQuery, conn2))
                {
                    using (var dataReader = taskCmd.ExecuteReader())
                    {
                        while (dataReader.Read())
                        {
                            yield return new A01RunTask(a01Run: this, dataReader: dataReader, isSucceededTask: true);
                        }
                    }
                }
            }
        }
    }
}
