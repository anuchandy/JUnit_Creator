using System;
using System.Net;
using System.Text.RegularExpressions;

namespace JUnit_Creator
{
    public class RunTaskLog
    {
        private readonly string logPath;
        private static readonly Regex exceptionRegEx = new Regex(@"^(([[a-zA-Z0-9]+[\.]?)+Exception)", RegexOptions.Compiled);

        public RunTaskLog(string logPath)
        {
            this.logPath = logPath;
        }

        public Tuple<string, string> DownloadAndExtractFailure()
        {
            string logTxt = new WebClient().DownloadString(this.logPath);
            return TryExtractFailure(logTxt);
        }

        private static Tuple<string, string> TryExtractFailure(string txtLog)
        {
            string errorTxt;
            int failureStartIndex = txtLog.LastIndexOf("<<< FAILURE!");
            if (failureStartIndex != -1)
            {
                int errorStartIndex = txtLog.IndexOf("<<< ERROR!", failureStartIndex);
                if (errorStartIndex != -1)
                {
                    int errorEndIndex = txtLog.IndexOf("[ERROR]", errorStartIndex);
                    if (errorEndIndex != -1)
                    {
                        int exceptionStartIndex = errorStartIndex + "<<< ERROR!".Length;
                        errorTxt = txtLog.Substring(exceptionStartIndex, errorEndIndex - exceptionStartIndex);
                    }
                    else
                    {
                        errorEndIndex = txtLog.IndexOf("[INFO]", errorStartIndex);
                        if (errorEndIndex != -1)
                        {
                            int exceptionStartIndex = errorStartIndex + "<<< ERROR!".Length;
                            errorTxt = txtLog.Substring(exceptionStartIndex, errorEndIndex - exceptionStartIndex);
                        }
                        else
                        {
                            int exceptionStartIndex = errorStartIndex + "<<< ERROR!".Length;
                            errorTxt = txtLog.Substring(exceptionStartIndex);
                        }
                    }
                }
                else
                {
                    int exceptionEndIndex = txtLog.IndexOf("[INFO]", failureStartIndex);
                    if (exceptionEndIndex != -1)
                    {
                        int exceptionStartIndex = failureStartIndex + "<<< FAILURE!".Length;
                        errorTxt = txtLog.Substring(exceptionStartIndex, exceptionEndIndex - exceptionStartIndex);
                    }
                    else
                    {
                        exceptionEndIndex = txtLog.IndexOf("[ERROR]", failureStartIndex);
                        if (exceptionEndIndex != -1)
                        {
                            int exceptionStartIndex = failureStartIndex + "<<< FAILURE!".Length;
                            errorTxt = txtLog.Substring(exceptionStartIndex, exceptionEndIndex - exceptionStartIndex);
                        }
                        else
                        {
                            int exceptionStartIndex = failureStartIndex + "<<< FAILURE!".Length;
                            errorTxt = txtLog.Substring(exceptionStartIndex);
                        }
                    }
                }
            }
            else
            {
                errorTxt = null;
            }

            if (!string.IsNullOrEmpty(errorTxt))
            {
                errorTxt = errorTxt.Replace("\r", "").Replace("\n", "").Replace("\t", "").Trim();
                if (errorTxt.StartsWith("java.lang.AssertionError"))
                {
                    return new Tuple<string, string>("java.lang.AssertionError", errorTxt);
                }
                else
                {
                    Match match = exceptionRegEx.Match(errorTxt);
                    if (match.Success)
                    {
                        return new Tuple<string, string>(match.Value, errorTxt);
                    }
                    else
                    {
                        return new Tuple<string, string>(null, errorTxt);
                    }
                }
            }
            else
            {
                return null;
            }
        }
    }
}
