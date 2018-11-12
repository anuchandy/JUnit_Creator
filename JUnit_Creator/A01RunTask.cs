using Npgsql;

namespace JUnit_Creator
{
    public class A01RunTask
    {
        public static readonly string FIELDS_TO_SELECT = "name as testName, duration as testDurationInSec, (result_details::json->> 'a01.reserved.tasklogpath')::VARCHAR as logPath";

        public A01RunTask(A01Run a01Run, NpgsqlDataReader dataReader, bool isSucceededTask)
        {
            this.A01Run = a01Run;
            this.TestDurationInSec = dataReader.GetInt64(1);
            this.TaskLogPath = dataReader.GetString(2);
            this.IsSucceededTask = isSucceededTask;
            //
            string testFQName = dataReader.GetString(0); //Test: com.microsoft.azure.management.AzureTests#listResourceGroups
            testFQName = testFQName.Trim();
            if (testFQName.StartsWith("Test:"))
            {
                testFQName = testFQName.Substring(5);
                testFQName = testFQName.Trim();
            }
            var parts = testFQName.Split('#', 2);
            this.TestClassName = parts[0];
            this.TestName = parts[1];
        }

        public A01Run A01Run
        {
            get;
        }

        public string TestClassName { get; }
        public string TestName { get; }
        public long TestDurationInSec { get; }
        public string TaskLogPath { get; }
        public bool IsSucceededTask { get; }

        public RunTaskLog Log
        {
            get
            {
                return new RunTaskLog(this.TaskLogPath);
            }
        }
    }
}
