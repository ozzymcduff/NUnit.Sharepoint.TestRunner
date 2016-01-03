﻿using NUnit.Framework;
using NUnit.Hosted.Utilities;
using System.IO;
using System;
using System.Collections.Generic;

namespace NUnit.Hosted.AspNet.Tests
{
    [TestFixture]
    public class TeamCityMessageWriterTests
    {
        private string Render(IEnumerable<IMessage> ms)
        {
            using (var tc_output = new MemoryStream())
            using (var tc_writer = new StreamWriter(tc_output))
            {
                var writer = new TeamCityMessageWriter(tc_writer);
                foreach (var m in ms)
                {
                    writer.OnMessage(m);
                }
                tc_writer.Flush();
                tc_output.Seek(0, SeekOrigin.Begin);
                return new StreamReader(tc_output).ReadToEnd();
            }
        }

        [Test]
        public void RegressionTest()
        {
            var teamCityOutPut = Render(new RecordedMessages().GetMessages());

            Assert.AreEqual(@"##teamcity[testSuiteStarted name='TestsInWebContext.dll' flowId='1-1003']
##teamcity[flowStarted flowId='1-1001' parent='1-1003']
##teamcity[testStarted name='TestsInWebContext.Sample_fixture.A_failing_test' captureStandardOutput='false' flowId='1-1001']
##teamcity[testFailed name='TestsInWebContext.Sample_fixture.A_failing_test' message='' details='at TestsInWebContext.Sample_fixture.A_failing_test() in c:\src\NUnit.Sharepoint.TestRunner\nunit3\TestsInWebContext\Sample.cs:line 12|r|n' flowId='1-1001']
##teamcity[testFinished name='TestsInWebContext.Sample_fixture.A_failing_test' duration='57' flowId='1-1001']
##teamcity[flowFinished flowId='1-1001']
##teamcity[flowStarted flowId='1-1002' parent='1-1003']
##teamcity[testStarted name='TestsInWebContext.Sample_fixture.A_successful_test_with_output' captureStandardOutput='false' flowId='1-1002']
##teamcity[testStdOut name='TestsInWebContext.Sample_fixture.A_successful_test_with_output' out='output from console|r|noutput from error console|r|n' flowId='1-1002']
##teamcity[testFinished name='TestsInWebContext.Sample_fixture.A_successful_test_with_output' duration='3' flowId='1-1002']
##teamcity[flowFinished flowId='1-1002']
##teamcity[testSuiteFinished name='TestsInWebContext.dll' flowId='1-1003']
", teamCityOutPut);
        }

        [Test]
        public void Should_not_render_flows_when_missing_flow_id()
        {
            var teamCityOutPut = Render(WithoutFlow(new RecordedMessages().GetMessages()));

            Assert.AreEqual(@"##teamcity[testSuiteStarted name='TestsInWebContext.dll']
##teamcity[testStarted name='TestsInWebContext.Sample_fixture.A_failing_test' captureStandardOutput='false']
##teamcity[testFailed name='TestsInWebContext.Sample_fixture.A_failing_test' message='' details='at TestsInWebContext.Sample_fixture.A_failing_test() in c:\src\NUnit.Sharepoint.TestRunner\nunit3\TestsInWebContext\Sample.cs:line 12|r|n']
##teamcity[testFinished name='TestsInWebContext.Sample_fixture.A_failing_test' duration='57']
##teamcity[testStarted name='TestsInWebContext.Sample_fixture.A_successful_test_with_output' captureStandardOutput='false']
##teamcity[testStdOut name='TestsInWebContext.Sample_fixture.A_successful_test_with_output' out='output from console|r|noutput from error console|r|n']
##teamcity[testFinished name='TestsInWebContext.Sample_fixture.A_successful_test_with_output' duration='3']
##teamcity[testSuiteFinished name='TestsInWebContext.dll']
", teamCityOutPut);
        }

        private IEnumerable<IMessage> WithoutFlow(IEnumerable<IMessage> enumerable)
        {
            throw new NotImplementedException();
        }
    }
}
