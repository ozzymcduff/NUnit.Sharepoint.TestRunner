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
using System.Text.RegularExpressions;
using System.Threading;
using NUnit.Hosted.Utilities;

namespace NUnit.Hosted
{
    public class TestEventHandler : MarshalByRefObject, EventListener
    {
        private int level;
        private HostedOptions options;
        private TextWriter outWriter;
        private string currentTestName;
        private ResultSummary s;
        private readonly TestMessageSubscriberAdapter _messageSubscribeAdapter;

        public TestEventHandler(HostedOptions options, TextWriter outWriter, Messages.ISubscriber[] subscribers)
        {
            this.level = 0;
            this.s = new ResultSummary();
            this.options = options;
            this.outWriter = outWriter;
            this.currentTestName = string.Empty;
            this._messageSubscribeAdapter = subscribers != null
                ? new TestMessageSubscriberAdapter(new Messages.CombineSubscribers(subscribers))
                : null;
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(this.OnUnhandledException);
        }

        public void RunStarted(string name, int testCount)
        {
            if (this._messageSubscribeAdapter != null)
            {
                this._messageSubscribeAdapter.RunStarted(name, testCount);
            }
        }

        public void RunFinished(NUnit.Core.TestResult result)
        {
            if (this._messageSubscribeAdapter != null)
            {
                this._messageSubscribeAdapter.RunFinished(result);
            }
        }

        public void RunFinished(Exception exception)
        {
            if (this._messageSubscribeAdapter != null)
            {
                this._messageSubscribeAdapter.RunFinished(exception);
            }
        }

        public void TestFinished(NUnit.Core.TestResult testResult)
        {
            ++this.s.TestCount;
            switch (testResult.ResultState)
            {
                case ResultState.Inconclusive:
                    ++this.s.InconclusiveCount;
                    break;
                case ResultState.Success:
                    ++this.s.PassCount;
                    break;
                case ResultState.NotRunnable:
                    ++this.s.InvalidCount;
                    break;
                case ResultState.Skipped:
                    ++this.s.SkipCount;
                    break;
                case ResultState.Ignored:
                    ++this.s.IgnoreCount;
                    break;
                case ResultState.Failure:
                    ++this.s.FailureCount;
                    FormatStackTrace(testResult);
                    break;
                case ResultState.Error:
                    ++this.s.ErrorCount;
                    FormatStackTrace(testResult);
                    break;
                case ResultState.Cancelled:
                    ++this.s.FailureCount;
                    FormatStackTrace(testResult);
                    break;
            }
            if (this._messageSubscribeAdapter != null)
            {
                this._messageSubscribeAdapter.TestFinished(testResult);
            }
            this.currentTestName = string.Empty;
        }

        private void FormatStackTrace(NUnit.Core.TestResult testResult)
        {
            this.outWriter.WriteLine(string.Format("{0}) {1} :", this.s.FailureCount, testResult.Test.TestName.FullName));
            this.outWriter.WriteLine(testResult.Message.Trim(Environment.NewLine.ToCharArray()));
            string filteredStackTrace = StackTraceFilter.Filter(testResult.StackTrace);
            if (filteredStackTrace != null && filteredStackTrace != string.Empty)
            {
                foreach (string stackTraceLines in filteredStackTrace.Split(Environment.NewLine.ToCharArray()))
                {
                    if (stackTraceLines != string.Empty)
                        this.outWriter.WriteLine(string.Format("at\n{0}",
                            Regex.Replace(stackTraceLines.Trim(), ".* in (.*):line (.*)", "$1($2)")));
                }
            }
        }

        public void TestStarted(TestName testName)
        {
            this.currentTestName = testName.FullName;
            if (this._messageSubscribeAdapter != null)
            {
                this._messageSubscribeAdapter.TestStarted(testName);
            }
        }

        public void SuiteStarted(TestName testName)
        {
            if (this.level++ != 0)
                return;
            ResultSummary.InitializeCounters(s);
            if (this._messageSubscribeAdapter != null)
            {
                this._messageSubscribeAdapter.SuiteStarted(testName);
            }
        }

        public void SuiteFinished(NUnit.Core.TestResult suiteResult)
        {
            if (this._messageSubscribeAdapter != null)
            {
                this._messageSubscribeAdapter.SuiteFinished(suiteResult);
            }
        }

        private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject.GetType() == typeof(ThreadAbortException))
                return;
            this.UnhandledException((Exception)e.ExceptionObject);
        }

        public void UnhandledException(Exception exception)
        {
            s.UnexpectedError = true;
            this.outWriter.WriteLine((this.currentTestName + " : " + exception.ToString()));
        }

        public void TestOutput(TestOutput output)
        {
            this.outWriter.Write(output.Text);
        }

        public override object InitializeLifetimeService()
        {
            return (object)null;
        }

        public ResultSummary GetSummary()
        {
            return new ResultSummary
            {
                TestCount = s.TestCount,
                FailureCount = s.FailureCount,
                ErrorCount = s.ErrorCount,
                ExplicitCount = s.ExplicitCount,
                IgnoreCount = s.IgnoreCount,
                InconclusiveCount = s.InconclusiveCount,
                InvalidAssemblies = s.InvalidAssemblies,
                InvalidCount = s.InvalidCount,
                PassCount = s.PassCount,
                SkipCount = s.SkipCount,
                UnexpectedError = s.UnexpectedError
            };
        }
    }
}
