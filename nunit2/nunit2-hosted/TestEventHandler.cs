// ***********************************************************************
// Copyright (c) 2007 Charlie Poole
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
using System.IO;
using NUnit.Core;
using NUnit.Util;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace NUnit.Hosted
{
    public class TestEventHandler: MarshalByRefObject, EventListener
    {
        private List<string> unhandledExceptions = new List<string>();
        private int testIgnoreCount;
        private int failureCount;
        private int level;
        private HostedOptions options;
        private TextWriter outWriter;
        private TextWriter errorWriter;
        private StringBuilder messages;
        private string currentTestName;
        private int errorCount;
        private int inconclusiveCount;
        private int invalidCount;
        private int passCount;
        private int skipCount;

        public bool HasExceptions
        {
            get
            {
                return this.unhandledExceptions.Count > 0;
            }
        }

        public TestEventHandler(HostedOptions options, TextWriter outWriter, TextWriter errorWriter)
        {
            this.level = 0;
            this.options = options;
            this.outWriter = outWriter;
            this.errorWriter = errorWriter;
            this.currentTestName = string.Empty;
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(this.OnUnhandledException);
        }

        public void RunStarted(string name, int testCount)
        {
        }

        public void RunFinished(TestResult result)
        {
        }

        public void RunFinished(Exception exception)
        {
        }

        public void TestFinished(TestResult testResult)
        {
            switch (testResult.ResultState)
            {
                case ResultState.Inconclusive:
                    ++this.inconclusiveCount;
                    break;
                case ResultState.Success:
                    ++this.passCount;
                    break;
                case ResultState.NotRunnable:
                    ++this.invalidCount;
                    break;
                case ResultState.Skipped:
                    ++this.skipCount;
                    break;
                case ResultState.Ignored:
                    ++this.testIgnoreCount;
                    break;
                case ResultState.Failure:
                    ++this.failureCount;
                    FormatStackTrace(testResult);
                    break;
                case ResultState.Error:
                    ++this.errorCount;
                    FormatStackTrace(testResult);
                    break;
                case ResultState.Cancelled:
                    ++this.failureCount;
                    FormatStackTrace(testResult);
                    break;
            }
            this.currentTestName = string.Empty;
        }

        private void FormatStackTrace(TestResult testResult)
        {
            this.messages.AppendLine(string.Format("{0}) {1} :", (object)this.failureCount, (object)testResult.Test.TestName.FullName));
            this.messages.AppendLine(testResult.Message.Trim(Environment.NewLine.ToCharArray()));
            string str1 = StackTraceFilter.Filter(testResult.StackTrace);
            if (str1 != null && str1 != string.Empty)
            {
                foreach (string str2 in str1.Split(Environment.NewLine.ToCharArray()))
                {
                    if (str2 != string.Empty)
                        this.messages.AppendLine(string.Format("at\n{0}", (object)Regex.Replace(str2.Trim(), ".* in (.*):line (.*)", "$1($2)")));
                }
            }
        }

        public void TestStarted(TestName testName)
        {
            this.currentTestName = testName.FullName;
            if (this.options.labels)
                this.outWriter.WriteLine("***** {0}", (object)this.currentTestName);
        }

        public void SuiteStarted(TestName testName)
        {
            if (this.level++ != 0)
                return;
            this.messages = new StringBuilder();
            this.testIgnoreCount = 0;
            this.failureCount = 0;
        }

        public void SuiteFinished(TestResult suiteResult)
        {
        }

        private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject.GetType() == typeof(ThreadAbortException))
                return;
            this.UnhandledException((Exception)e.ExceptionObject);
        }

        public void UnhandledException(Exception exception)
        {
            this.unhandledExceptions.Add((this.currentTestName + " : " + exception.ToString()));
        }

        public void TestOutput(TestOutput output)
        {
            switch (output.Type)
            {
                case TestOutputType.Out:
                    this.outWriter.Write(output.Text);
                    break;
                case TestOutputType.Error:
                    this.errorWriter.Write(output.Text);
                    break;
            }
        }

        public override object InitializeLifetimeService()
        {
            return (object)null;
        }

        public ResultSummary GetSummary()
        {
            return new ResultSummary {
                FailureCount=this.failureCount,
                ErrorCount=this.errorCount,
                ExplicitCount=0,
                IgnoreCount=this.testIgnoreCount,
                InconclusiveCount=this.inconclusiveCount,
                InvalidAssemblies=0,
                InvalidCount=this.invalidCount,
                PassCount=this.passCount,
                SkipCount=this.skipCount,
                UnexpectedError=false
            };
        }
    }
}
