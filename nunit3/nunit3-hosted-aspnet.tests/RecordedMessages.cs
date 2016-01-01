using NUnit.Hosted.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NUnit.Hosted.AspNet.Tests
{
    class RecordedMessages
    {
        private JsonConvertMessages jsonConvert=new JsonConvertMessages();
        private const string messages = @"{""$type"":""OnRootSuiteStart"",""FlowId"":""1-1003"",""AssemblyName"":""TestsInWebContext.dll"",""Type"":1}
{""$type"":""OnFlowStarted"",""FlowId"":""1-1001"",""ParentFlowId"":""1-1003"",""Type"":3}
{""$type"":""OnTestStart"",""FlowId"":""1-1001"",""FullName"":""TestsInWebContext.Sample_fixture.A_failing_test"",""Type"":0}
{""$type"":""OnTestFailed"",""FlowId"":""1-1001"",""Result"":{""$type"":""NUnit.Hosted.TestResult, NUnit3.HostedRunner"",""DurationMilliseconds"":57,""Failure"":{""$type"":""NUnit.Hosted.TestResult+FailureResult, NUnit3.HostedRunner"",""Message"":"""",""StackTrace"":""at TestsInWebContext.Sample_fixture.A_failing_test() in c:\\src\\NUnit.Sharepoint.TestRunner\\nunit3\\TestsInWebContext\\Sample.cs:line 12\r\n""},""Reason"":{""$type"":""NUnit.Hosted.TestResult+ReasonResult, NUnit3.HostedRunner"",""Message"":null},""Output"":null},""FullName"":""TestsInWebContext.Sample_fixture.A_failing_test"",""Type"":6}
{""$type"":""OnFlowFinished"",""FlowId"":""1-1001"",""Type"":4}
{""$type"":""OnFlowStarted"",""FlowId"":""1-1002"",""ParentFlowId"":""1-1003"",""Type"":3}
{""$type"":""OnTestStart"",""FlowId"":""1-1002"",""FullName"":""TestsInWebContext.Sample_fixture.A_successful_test_with_output"",""Type"":0}
{""$type"":""OnTestSuccess"",""FlowId"":""1-1002"",""Result"":{""$type"":""NUnit.Hosted.TestResult, NUnit3.HostedRunner"",""DurationMilliseconds"":3,""Failure"":{""$type"":""NUnit.Hosted.TestResult+FailureResult, NUnit3.HostedRunner"",""Message"":null,""StackTrace"":null},""Reason"":{""$type"":""NUnit.Hosted.TestResult+ReasonResult, NUnit3.HostedRunner"",""Message"":null},""Output"":""output from console\r\noutput from error console\r\n""},""FullName"":""TestsInWebContext.Sample_fixture.A_successful_test_with_output"",""Type"":5}
{""$type"":""OnFlowFinished"",""FlowId"":""1-1002"",""Type"":4}
{""$type"":""OnRootSuiteFinish"",""FlowId"":""1-1003"",""AssemblyName"":""TestsInWebContext.dll"",""Type"":2}
";
        public IEnumerable<IMessage> GetMessages()
        {
            using (var ms = new MemoryStream())
            using (var w = new StreamWriter(ms))
            {
                w.Write(messages);
                w.Flush();
                ms.Seek(0, SeekOrigin.Begin);
                return jsonConvert.DeserializeStream(ms);
            }
        }
    }
}
