// ***********************************************************************
// Copyright (c) 2014 Charlie Poole
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// ***********************************************************************

using System;
using System.Globalization;
using System.Xml;
using NUnit.Common;

namespace NUnit.Hosted
{
    using Common;
    using System.IO;
    using Utilities;

    public class ResultReporter
    {
        private TextWriter _writer;
        private XmlNode _result;
        private string _overallResult;
        private HostedOptions _options;

        private int _reportIndex = 0;

        public ResultReporter(XmlNode result, TextWriter writer, HostedOptions options)
        {
            _result = result;
            _writer = writer;

            _overallResult = result.GetAttribute("result");
            if (_overallResult == "Skipped")
                _overallResult = "Warning";

            _options = options;

            Summary = ReadResultSummary(result);
        }

        #region Constructor

        private static ResultSummary ReadResultSummary(XmlNode result)
        {
            if (result.Name != "test-run")
                throw new InvalidOperationException("Expected <test-run> as top-level element but was <" + result.Name + ">");
            var s = new ResultSummary();
            ResultSummary.InitializeCounters(s);

            Summarize(s, result);
            return s;
        }

        #endregion
        #region Helper Methods

        private static void Summarize(ResultSummary s, XmlNode node)
        {
            string type = node.GetAttribute("type");
            string status = node.GetAttribute("result");
            string label = node.GetAttribute("label");

            switch (node.Name)
            {
                case "test-case":
                    s.TestCount++;

                    switch (status)
                    {
                        case "Passed":
                            s.PassCount++;
                            break;
                        case "Failed":
                            if (label == null)
                                s.FailureCount++;
                            else if (label == "Invalid")
                                s.InvalidCount++;
                            else
                                s.ErrorCount++;
                            break;
                        case "Inconclusive":
                            s.InconclusiveCount++;
                            break;
                        case "Skipped":
                            if (label == "Ignored")
                                s.IgnoreCount++;
                            else if (label == "Explicit")
                                s.ExplicitCount++;
                            else
                                s.SkipCount++;
                            break;
                        default:
                            s.SkipCount++;
                            break;
                    }
                    break;

                case "test-suite":
                    if (type == "Assembly" && status == "Failed" && label == "Invalid")
                        s.InvalidAssemblies++;
                    if (type == "Assembly" && status == "Failed" && label == "Error")
                    {
                        s.InvalidAssemblies++;
                        s.UnexpectedError = true;
                    }

                    Summarize(s, node.ChildNodes);
                    break;

                case "test-run":
                    Summarize(s, node.ChildNodes);
                    break;
            }
        }

        private static void Summarize(ResultSummary s, XmlNodeList nodes)
        {
            foreach (XmlNode childResult in nodes)
                Summarize(s, childResult);
        }

        #endregion

        public ResultSummary Summary { get; private set; }

        /// <summary>
        /// Reports the results to the console
        /// </summary>
        public void ReportResults()
        {
            _writer.WriteLine();

            if (Summary.ExplicitCount + Summary.SkipCount + Summary.IgnoreCount > 0)
                WriteNotRunReport();

            if (_overallResult == "Failed")
                WriteErrorsAndFailuresReport();

            WriteRunSettingsReport();
        }

        #region

        private void WriteRunSettingsReport()
        {
            var firstSuite = _result.SelectSingleNode("test-suite");
            if (firstSuite != null)
            {
                var settings = firstSuite.SelectNodes("settings/setting");

                if (settings.Count > 0)
                {
                    _writer.WriteLine("Run Settings");

                    foreach (XmlNode node in settings)
                    {
                        string name = node.GetAttribute("name");
                        string val = node.GetAttribute("value");
                        string label = string.Format("    {0}: ", name);
                        _writer.WriteLine(label+ val);
                    }

                    _writer.WriteLine();
                }
            }
        }

        #endregion


        #region Errors and Failures Report

        public void WriteErrorsAndFailuresReport()
        {
            _reportIndex = 0;
            WriteErrorsAndFailures(_result);
        }

        private void WriteErrorsAndFailures(XmlNode result)
        {
            string resultState = result.GetAttribute("result");

            switch (result.Name)
            {
                case "test-case":
                    if (resultState == "Failed")
                        WriteSingleResult(result);
                    return;

                case "test-run":
                    foreach (XmlNode childResult in result.ChildNodes)
                        WriteErrorsAndFailures(childResult);
                    break;

                case "test-suite":
                    if (resultState == "Failed")
                    {
                        if (result.GetAttribute("type") == "Theory")
                        {
                            WriteSingleResult(result);
                        }
                        else
                        {
                            var site = result.GetAttribute("site");
                            if (site != "Parent" && site != "Child")
                                WriteSingleResult(result);
                            if (site == "SetUp") return;
                        }
                    }

                    foreach (XmlNode childResult in result.ChildNodes)
                        WriteErrorsAndFailures(childResult);

                    break;
            }
        }

        #endregion

        #region Not Run Report

        public void WriteNotRunReport()
        {
            _reportIndex = 0;
            _writer.WriteLine("Tests Not Run");
            _writer.WriteLine();
            WriteNotRunResults(_result);
        }

        private void WriteNotRunResults(XmlNode result)
        {
            switch (result.Name)
            {
                case "test-case":
                    string status = result.GetAttribute("result");

                    if (status == "Skipped")
                    {
                        string label = result.GetAttribute("label");
                        
                        WriteSingleResult(result);
                    }

                    break;

                case "test-suite":
                case "test-run":
                    foreach (XmlNode childResult in result.ChildNodes)
                        WriteNotRunResults(childResult);

                    break;
            }
        }

        #endregion

        #region Helper Methods

        private void WriteSummaryCount(string label, int count)
        {
            _writer.WriteLine(label+ count.ToString(CultureInfo.CurrentUICulture));
        }

        private static readonly char[] EOL_CHARS = new char[] { '\r', '\n' };

        private void WriteSingleResult(XmlNode result)
        {
            string status = result.GetAttribute("label");
            if (status == null)
                status = result.GetAttribute("result");

            if (status == "Failed" || status == "Error")
            {
                var site = result.GetAttribute("site");
                if (site == "SetUp" || site == "TearDown")
                    status = site + " " + status;
            }

            string fullName = result.GetAttribute("fullname");

            _writer.WriteLine(
                string.Format("{0}) {1} : {2}", ++_reportIndex, status, fullName));

            XmlNode failureNode = result.SelectSingleNode("failure");
            if (failureNode != null)
            {
                XmlNode message = failureNode.SelectSingleNode("message");
                XmlNode stacktrace = failureNode.SelectSingleNode("stack-trace");

                // In order to control the format, we trim any line-end chars
                // from end of the strings we write and supply them via calls
                // to WriteLine(). Newlines within the strings are retained.

                if (message != null)
                    _writer.WriteLine( message.InnerText.TrimEnd(EOL_CHARS));

                if (stacktrace != null)
                    _writer.WriteLine(stacktrace.InnerText.TrimEnd(EOL_CHARS));
            }

            XmlNode reasonNode = result.SelectSingleNode("reason");
            if (reasonNode != null)
            {
                XmlNode message = reasonNode.SelectSingleNode("message");

                if (message != null)
                    _writer.WriteLine(message.InnerText.TrimEnd(EOL_CHARS));
            }

            _writer.WriteLine(); // Skip after each item
        }

        #endregion
    }
}
