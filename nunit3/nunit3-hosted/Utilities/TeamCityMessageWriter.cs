// ***********************************************************************
// Copyright (c) 2015-2016 Charlie Poole, Oskar Gewalli (based on NUnit code)
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
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
namespace NUnit.Hosted.Utilities
{
    public class TeamCityMessageWriter
    {
        private Messages.OnMessage _onMessage;
        public TeamCityMessageWriter(TextWriter outWriter)
        {
            _onMessage = Messages.HandleAllSubscriber(
                new HandleAll(outWriter)
            );
        }

        public void OnMessage(IMessage message)
        {
            _onMessage.Invoke(message);
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
            private void TcWriteLine(string name, IEnumerable<string> listOfPairs)
            {
                var a = listOfPairs.ToArray();
                var keyValuePairs = new List<KeyValuePair<string, string>>(a.Length / 2);
                for (int i = 0; i < a.Length; i += 2)
                {
                    keyValuePairs.Add(new KeyValuePair<string, string>(a[i], a[i + 1]));
                }
                TcWriteLine(name, keyValuePairs);
            }

            private void TcWriteLine(string name, IEnumerable<KeyValuePair<string, string>> keyValuePairs)
            {
                _outWriter.WriteLine("##teamcity[{0} {1}]", name, string.Join(" ", keyValuePairs
                    .Where(NotEmpyFlowId)
                    .Select(kv => kv.Key + "='" + Escape(kv.Value) + "'")
                    .ToArray()));
            }

            private bool NotEmpyFlowId(KeyValuePair<string, string> arg)
            {
                return !(arg.Key.Equals("flowId") && string.IsNullOrEmpty(arg.Value));
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
                TcWriteLine("testStdOut", new[] { "name", fullName, "out", message.Output, "flowId", flowId });
            }

            public void OnRootSuiteStart(string flowId, string assemblyName)
            {
                assemblyName = Path.GetFileName(assemblyName);
                TcWriteLine("testSuiteStarted", new[] { "name", assemblyName, "flowId", flowId });
            }

            public void OnRootSuiteFinish(string flowId, string assemblyName)
            {
                assemblyName = Path.GetFileName(assemblyName);
                TcWriteLine("testSuiteFinished", new[] { "name", assemblyName, "flowId", flowId });
            }

            public void OnFlowStarted(string flowId, string parentFlowId)
            {
                TcWriteLine("flowStarted", new[] { "flowId", flowId, "parent", parentFlowId });
            }

            public void OnFlowFinished(string flowId)
            {
                TcWriteLine("flowFinished", new[] { "flowId", flowId });
            }

            public void OnTestStart(string flowId, string fullName)
            {
                TcWriteLine("testStarted", new[] { "name", fullName, "captureStandardOutput", "false", "flowId", flowId });
            }

            public void OnTestFinishedSuccessFully(string flowId, TestResult message, string fullName)
            {
                if (message == null)
                {
                    throw new ArgumentNullException("message");
                }

                var durationMilliseconds = message.DurationMilliseconds;

                TrySendOutput(flowId, message, fullName);
                TcWriteLine("testFinished", new[] { "name", fullName, "duration", durationMilliseconds.ToString(), "flowId", flowId });
            }

            public void OnTestFailed(string flowId, TestResult message, string fullName)
            {
                if (message == null)
                {
                    throw new ArgumentNullException("message");
                }

                var errorMmessage = message.Failure.Message;
                var stackTrace = message.Failure.StackTrace;
                TcWriteLine("testFailed", new[] { "name", fullName,
                    "message", errorMmessage == null ? string.Empty : errorMmessage,
                    "details", stackTrace == null ? string.Empty : stackTrace,
                    "flowId", flowId });
                var durationMilliseconds = message.DurationMilliseconds;

                TrySendOutput(flowId, message, fullName);
                TcWriteLine("testFinished", new[] { "name", fullName, "duration", durationMilliseconds.ToString(), "flowId", flowId });
            }

            public void OnTestSkipped(string flowId, TestResult message, string fullName)
            {
                if (message == null)
                {
                    throw new ArgumentNullException("message");
                }

                TrySendOutput(flowId, message, fullName);
                var reason = message.Reason.Message;
                TcWriteLine("testFinished", new[] { "name", fullName, "message", reason == null ? string.Empty : reason, "flowId", flowId });
            }

            public void OnTestInconclusive(string flowId, TestResult message, string fullName)
            {
                if (message == null)
                {
                    throw new ArgumentNullException("message");
                }

                TrySendOutput(flowId, message, fullName);
                TcWriteLine("testIgnored", new[] { "name", fullName, "message", "Inconclusive", "flowId", flowId });
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
