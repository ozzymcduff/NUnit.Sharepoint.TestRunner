using System;
using System.Globalization;
using System.IO;

namespace NUnit.Hosted.Utilities
{
    public class TeamCityMessageWriter:Messages.ISubscriber
    {
        private Messages.ISubscriber subscriber;
        public TeamCityMessageWriter(TextWriter outWriter)
        {
            subscriber = new Messages.HandleAllSubscriber(new HandleAll(outWriter));
        }

        public void OnMessage(IMessage message)
        {
            subscriber.OnMessage(message);
        }

        private class HandleAll : Messages.IHandleAll
        {
            private readonly TextWriter _outWriter;

            public HandleAll(TextWriter outWriter)
            {
                if (outWriter == null)
                {
                    throw new ArgumentNullException("outWriter");
                }

                _outWriter = outWriter;
            }

            private void TrySendOutput(string flowId, TestResult message, string fullName)
            {
                if (message == null)
                {
                    throw new ArgumentNullException("message");
                }
                if (string.IsNullOrEmpty(message.Output))
                {
                    return;
                }

                WriteLine("##teamcity[testStdOut name='{0}' out='{1}' flowId='{2}']", fullName, message.Output, flowId);
            }

            public void OnRootSuiteStart(string flowId, string assemblyName)
            {
                assemblyName = Path.GetFileName(assemblyName);
                WriteLine("##teamcity[testSuiteStarted name='{0}' flowId='{1}']", assemblyName, flowId);
            }

            public void OnRootSuiteFinish(string flowId, string assemblyName)
            {
                assemblyName = Path.GetFileName(assemblyName);
                WriteLine("##teamcity[testSuiteFinished name='{0}' flowId='{1}']", assemblyName, flowId);
            }

            public void OnFlowStarted(string flowId, string parentFlowId)
            {
                WriteLine("##teamcity[flowStarted flowId='{0}' parent='{1}']", flowId, parentFlowId);
            }

            public void OnFlowFinished(string flowId)
            {
                WriteLine("##teamcity[flowFinished flowId='{0}']", flowId);
            }

            public void OnTestStart(string flowId, string fullName)
            {
                WriteLine("##teamcity[testStarted name='{0}' captureStandardOutput='false' flowId='{1}']", fullName, flowId);
            }

            public void OnTestFinishedSuccessFully(string flowId, TestResult message, string fullName)
            {
                if (message == null)
                {
                    throw new ArgumentNullException("message");
                }

                var durationMilliseconds = message.DurationMilliseconds;

                TrySendOutput(flowId, message, fullName);
                WriteLine(
                    "##teamcity[testFinished name='{0}' duration='{1}' flowId='{2}']",
                    fullName,
                    durationMilliseconds.ToString(),
                    flowId);
            }

            public void OnTestFailed(string flowId, TestResult message, string fullName)
            {
                if (message == null)
                {
                    throw new ArgumentNullException("message");
                }

                var errorMmessage = message.Failure.Message;
                var stackTrace = message.Failure.StackTrace;
                WriteLine(
                    "##teamcity[testFailed name='{0}' message='{1}' details='{2}' flowId='{3}']",
                    fullName,
                    errorMmessage == null ? string.Empty : errorMmessage,
                    stackTrace == null ? string.Empty : stackTrace,
                    flowId);

                var durationMilliseconds = message.DurationMilliseconds;

                TrySendOutput(flowId, message, fullName);
                WriteLine(
                    "##teamcity[testFinished name='{0}' duration='{1}' flowId='{2}']",
                    fullName,
                    durationMilliseconds.ToString(),
                    flowId);
            }

            public void OnTestSkipped(string flowId, TestResult message, string fullName)
            {
                if (message == null)
                {
                    throw new ArgumentNullException("message");
                }

                TrySendOutput(flowId, message, fullName);
                var reason = message.Reason.Message;
                WriteLine(
                    "##teamcity[testIgnored name='{0}' message='{1}' flowId='{2}']",
                    fullName,
                    reason == null ? string.Empty : reason,
                    flowId);
            }

            public void OnTestInconclusive(string flowId, TestResult message, string fullName)
            {
                if (message == null)
                {
                    throw new ArgumentNullException("message");
                }

                TrySendOutput(flowId, message, fullName);
                WriteLine(
                    "##teamcity[testIgnored name='{0}' message='{1}' flowId='{2}']",
                    fullName,
                    "Inconclusive",
                    flowId);
            }

            private void WriteLine(string format, params string[] arg)
            {
                if (format == null)
                {
                    throw new ArgumentNullException("format");
                }

                if (arg == null)
                {
                    throw new ArgumentNullException("arg");
                }

                var argObjects = new object[arg.Length];
                for (var i = 0; i < arg.Length; i++)
                {
                    var str = arg[i];
                    if (str != null)
                    {
                        str = Escape(str);
                    }

                    argObjects[i] = str;
                }

                var message = string.Format(format, argObjects);
                _outWriter.WriteLine(message);
            }

            private static string Escape(string input)
            {
                return input != null
                    ? input.Replace("|", "||")
                           .Replace("'", "|'")
                           .Replace("\n", "|n")
                           .Replace("\r", "|r")
                           .Replace(char.ConvertFromUtf32(int.Parse("0086", NumberStyles.HexNumber)), "|x")
                           .Replace(char.ConvertFromUtf32(int.Parse("2028", NumberStyles.HexNumber)), "|l")
                           .Replace(char.ConvertFromUtf32(int.Parse("2029", NumberStyles.HexNumber)), "|p")
                           .Replace("[", "|[")
                           .Replace("]", "|]")
                    : null;
            }
        }

    }
}
