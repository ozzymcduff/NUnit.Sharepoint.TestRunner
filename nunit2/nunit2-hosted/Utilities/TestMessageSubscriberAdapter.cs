using System;
using System.Collections.Generic;
using NUnit.Core;

namespace NUnit.Hosted.Utilities
{
    class TestMessageSubscriberAdapter
    {
        private Messages.ISubscriber subscriber;
        private readonly Dictionary<string, string> _refs = new Dictionary<string, string>();
        private int _blockCounter;
        private string _rootFlowId;
        private readonly string assemblyName;

        public TestMessageSubscriberAdapter(Messages.ISubscriber subscriber)
        {
            this.subscriber = subscriber;
            this.assemblyName = "<>";
        }

        public void TestFinished(Core.TestResult testResult)
        {
            switch (testResult.ResultState)
            {
                case Core.ResultState.Inconclusive:
                    this.subscriber.OnMessage(new Messages.OnTestInconclusive(testResult.Test.TestName.TestID.ToString(),
                        GetTestResult(testResult), testResult.Test.TestName.FullName));
                    break;
                case Core.ResultState.NotRunnable:
                    this.subscriber.OnMessage(new Messages.OnTestInconclusive(testResult.Test.TestName.TestID.ToString(),
                        GetTestResult(testResult), testResult.Test.TestName.FullName));
                    break;
                case Core.ResultState.Skipped:
                    this.subscriber.OnMessage(new Messages.OnTestSkipped(testResult.Test.TestName.TestID.ToString(),
                        GetTestResult(testResult), testResult.Test.TestName.FullName));
                    break;
                case Core.ResultState.Ignored:
                    this.subscriber.OnMessage(new Messages.OnTestSkipped(testResult.Test.TestName.TestID.ToString(),
                        GetTestResult(testResult), testResult.Test.TestName.FullName));
                    break;
                case Core.ResultState.Success:
                    this.subscriber.OnMessage(new Messages.OnTestSuccess(testResult.Test.TestName.TestID.ToString(),
                        GetTestResult(testResult), testResult.Test.TestName.FullName));
                    break;
                case Core.ResultState.Failure:
                    this.subscriber.OnMessage(new Messages.OnTestFailed(testResult.Test.TestName.TestID.ToString(),
                        GetTestResult(testResult), testResult.Test.TestName.FullName));
                    break;
                case Core.ResultState.Error:
                    this.subscriber.OnMessage(new Messages.OnTestFailed(testResult.Test.TestName.TestID.ToString(),
                        GetTestResult(testResult), testResult.Test.TestName.FullName));
                    break;
                case Core.ResultState.Cancelled:
                    this.subscriber.OnMessage(new Messages.OnTestInconclusive(testResult.Test.TestName.TestID.ToString(),
                        GetTestResult(testResult), testResult.Test.TestName.FullName));
                    break;
                default:
                    break;
            }
        }

        private TestResult GetTestResult(Core.TestResult from)
        {
            var to = new TestResult();
            to.DurationMilliseconds = (int)(from.Time * 1000d);
            to.Failure.Message = from.Message;
            to.Failure.StackTrace = from.StackTrace;
            // res.Output = "";
            // res.reason
            return to;
        }

        public void RunStarted(string name, int testCount)
        {
            this.subscriber.OnMessage(new Messages.OnRootSuiteStart("<>", assemblyName));
        }

        public void RunFinished(Core.TestResult result)
        {
            this.subscriber.OnMessage(new Messages.OnRootSuiteFinish("<>", assemblyName));
        }

        public void RunFinished(Exception exception)
        {
            this.subscriber.OnMessage(new Messages.OnRootSuiteFinish("<>", assemblyName));
        }

        public void TestStarted(TestName testName)
        {
            this.subscriber.OnMessage(new Messages.OnTestStart(testName.TestID.ToString(), testName.FullName));
        }

        public void SuiteStarted(TestName testName)
        {
        }

        public void SuiteFinished(Core.TestResult suiteResult)
        {
        }
    }
}
