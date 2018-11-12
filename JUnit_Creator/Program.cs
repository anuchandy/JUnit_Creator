using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace JUnit_Creator
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                throw new ArgumentException("Usage: JUnit_Creatator <start_datetime> <end_datetime>"); // JUnit_Creatator 2018-07-14T00:00:00Z 2018-11-11T00:00:00Z
            }
            DateTime startTime = DateTime.Parse(args[0]);
            DateTime endTime = DateTime.Parse(args[1]);
            if (startTime > endTime)
            {
                throw new ArgumentException("start_datetime must be before end_datetime");
            }
            //
            Config config = Config.Create();
            A01Runs a01Runs = new A01Runs(config.DB);
            //
            IEnumerable<A01Run> a01completedRuns = a01Runs.RetrieveCompletedRuns(utcStartTime: startTime, utcEndTime: endTime);
            foreach (A01Run a01completedRun in a01completedRuns)
            {
                string dirPath = $@"{config.Github.RepoLocalPath}\{Canonalize(a01completedRun.CreationTimeInUtc)}";
                System.IO.Directory.CreateDirectory(dirPath);
                //
                IEnumerable<A01RunTask> failedTasks = a01completedRun.RetrieveFailedTasks();
                foreach (A01RunTask failedTask in failedTasks)
                {
                    string fileName = $@"{dirPath}\{failedTask.TestClassName}_{failedTask.TestName}.xml";
                    //
                    // Tuple<errorType, errorDescription>
                    Tuple<string, string> failure = failedTask.Log.DownloadAndExtractFailure();
                    //
                    XmlDocument junitDocument = JUnitXmlDocument.CreateJUnitDocumentForFailure(testSuiteName: failedTask.TestClassName, 
                        testName: failedTask.TestName, 
                        testDuration: failedTask.TestDurationInSec, 
                        creationTimeInUtc: a01completedRun.CreationTimeInUtc, 
                        failure: failure);
                    SerializeToFile(junitDocument, fileName);
                }
                //
                IEnumerable<A01RunTask> succeededTasks = a01completedRun.RetrieveSucceededTasks();
                foreach (A01RunTask succeededTask in succeededTasks)
                {
                    string fileName = $@"{dirPath}\{succeededTask.TestClassName}_{succeededTask.TestName}.xml";
                    //
                    XmlDocument junitDocument = JUnitXmlDocument.CreateJUnitDocumentForSuccess(testSuiteName: succeededTask.TestClassName,
                        testName: succeededTask.TestName,
                        testDuration: succeededTask.TestDurationInSec,
                        creationTimeInUtc: a01completedRun.CreationTimeInUtc);
                    SerializeToFile(junitDocument, fileName);
                }
            }
            Console.ReadKey();
        }

        private static void SerializeToFile(XmlDocument xmlDocument, string filePath)
        {
            using (var memoryStream = new MemoryStream())
            {
                using (XmlTextWriter xmlTextWriter = new XmlTextWriter(memoryStream, Encoding.Unicode))
                {
                    xmlTextWriter.Formatting = Formatting.Indented;
                    xmlDocument.WriteTo(xmlTextWriter);
                    xmlTextWriter.Flush();
                    memoryStream.Flush();
                    memoryStream.Position = 0;
                    using (FileStream file = new FileStream(filePath, System.IO.FileMode.OpenOrCreate, FileAccess.Write))
                    {
                        memoryStream.CopyTo(file);
                    }
                }
            }
        }

        private static string Canonalize(string txt)
        {
            if (txt == null)
            {
                return null;
            }
            else
            {
                return txt.Replace('-', '_').Replace('.', '_').Replace(':', '_');
            }
        }
    }
}
