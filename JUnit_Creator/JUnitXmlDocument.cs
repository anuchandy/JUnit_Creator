using System;
using System.Xml;

namespace JUnit_Creator
{
    // https://stackoverflow.com/questions/4922867/what-is-the-junit-xml-format-specification-that-hudson-supports
    //
    public class JUnitXmlDocument
    {
        public static XmlDocument CreateJUnitDocumentForFailure(string testSuiteName, string testName, long testDuration, string creationTimeInUtc, Tuple<string, string> failure)
        {
            XmlDocument xmlDocument = new XmlDocument();
            XmlDeclaration xmlDeclaration = xmlDocument.CreateXmlDeclaration("1.0", "UTF-8", "yes");
            //
            XmlElement root = xmlDocument.DocumentElement;
            xmlDocument.InsertBefore(xmlDeclaration, root);
            //
            string exceptionTypeName = failure?.Item1;
            string exceptionDetailsTxt = failure?.Item2;
            bool hasExceptionThrown = true;
            bool isAssertingError = false;
            if (exceptionTypeName != null && exceptionTypeName.Equals("java.lang.AssertionError"))
            {
                hasExceptionThrown = false;
                isAssertingError = true;
            }
            // Create testsuite element
            XmlElement testsuiteElement = CreateTestSuiteElement(xmlDocument, testSuiteName, creationTimeInUtc, testDuration, hasExceptionThrown, isAssertingError);
            //
            XmlElement testcaseElement = xmlDocument.CreateElement("testcase");
            testcaseElement.SetAttribute("name", testName);
            testcaseElement.SetAttribute("className", testSuiteName);
            testcaseElement.SetAttribute("time", $"{testDuration}");

            if (exceptionTypeName != null)
            {
                if (exceptionTypeName.Equals("java.lang.AssertionError"))
                {
                    // Indicates that the test failed.via assert*().
                    XmlElement failureElement = xmlDocument.CreateElement("failure");
                    failureElement.SetAttribute("type", "java.lang.AssertionError");
                    //
                    XmlText failureTextNode = xmlDocument.CreateTextNode(exceptionDetailsTxt);
                    failureElement.AppendChild(failureTextNode);
                    //
                    testcaseElement.AppendChild(failureElement);
                }
                else
                {
                    // Indicates that the test errored, i.e. throws exception
                    XmlElement errorElement = xmlDocument.CreateElement("error");
                    errorElement.SetAttribute("type", exceptionTypeName);
                    //
                    XmlText errorTextNode = xmlDocument.CreateTextNode(exceptionDetailsTxt);
                    errorElement.AppendChild(errorTextNode);
                    //
                    testcaseElement.AppendChild(errorElement);
                }
            }
            else
            {
                // Indicates that the test errored, i.e. throwed exception
                XmlElement errorElement = xmlDocument.CreateElement("error");
                //
                XmlText errorTextNode = xmlDocument.CreateTextNode(exceptionDetailsTxt);
                errorElement.AppendChild(errorTextNode);
                //
                testcaseElement.AppendChild(errorElement);
            }

            testsuiteElement.AppendChild(testcaseElement);
            //
            xmlDocument.AppendChild(testsuiteElement);
            return xmlDocument;
        }


        public static XmlDocument CreateJUnitDocumentForSuccess(string testSuiteName, string testName, long testDuration, string creationTimeInUtc)
        {
            XmlDocument xmlDocument = new XmlDocument();
            XmlDeclaration xmlDeclaration = xmlDocument.CreateXmlDeclaration("1.0", "UTF-8", "yes");
            //
            XmlElement root = xmlDocument.DocumentElement;
            xmlDocument.InsertBefore(xmlDeclaration, root);
            //
            // Create testsuite element
            XmlElement testsuiteElement = CreateTestSuiteElement(xmlDocument, testSuiteName, creationTimeInUtc, testDuration, true, true);
            //
            XmlElement testcaseElement = xmlDocument.CreateElement("testcase");
            testcaseElement.SetAttribute("name", testName);
            testcaseElement.SetAttribute("className", testSuiteName);
            testcaseElement.SetAttribute("time", $"{testDuration}");

            testsuiteElement.AppendChild(testcaseElement);
            //
            xmlDocument.AppendChild(testsuiteElement);
            return xmlDocument;
        }

        private static XmlElement CreateTestSuiteElement(XmlDocument xmlDocument, string testSuiteName, string creationTimeUtc, long duration, bool hasExceptionThrown, bool isAssertingError)
        {
            XmlElement testsuiteElement = xmlDocument.CreateElement("testsuite");
            testsuiteElement.SetAttribute("xmlns:xsi", "http://www.w3.org/2001/XMLSchema-instance");
            testsuiteElement.SetAttribute("xsi:noNamespaceSchemaLocation", "https://maven.apache.org/surefire/maven-surefire-plugin/xsd/surefire-test-report.xsd");
            testsuiteElement.SetAttribute("name", $"{testSuiteName}");
            testsuiteElement.SetAttribute("time", $"{duration}");
            testsuiteElement.SetAttribute("tests", "1");
            //
            if (hasExceptionThrown)
            {
                testsuiteElement.SetAttribute("errors", "1");
            }
            else
            {
                testsuiteElement.SetAttribute("errors", "0");
            }
            //
            if (isAssertingError)
            {
                testsuiteElement.SetAttribute("failures", "1");
            }
            else
            {
                testsuiteElement.SetAttribute("failures", "0");
            }
            //
            testsuiteElement.SetAttribute("skipped", "0");
            testsuiteElement.SetAttribute("timestamp", $"{creationTimeUtc}");
            return testsuiteElement;
        }
    }
}
