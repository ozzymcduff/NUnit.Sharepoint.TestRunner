// ***********************************************************************
// Copyright (c) 2015 Charlie Poole
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

using System.IO;
using System.Xml;
using System.Collections.Generic;
using System.Globalization;
using System;
using NUnit.Hosted.Common;

namespace NUnit.Hosted.Utilities
{
    class TestMessageSubscriberAdapter
    {
        private readonly Messages.OnMessage _onMessage;
        private readonly Dictionary<string, string> _refs = new Dictionary<string, string>();
        private int _blockCounter;
        private string _rootFlowId;

        public TestMessageSubscriberAdapter(Messages.OnMessage sink)
        {
            this._onMessage = sink;
        }

        public void RegisterMessage(XmlNode message)
        {
            if (message == null)
            {
                throw new ArgumentNullException("message");
            }

            var messageName = message.Name;
            if (string.IsNullOrEmpty(messageName))
            {
                return;
            }

            messageName = messageName.ToLowerInvariant();
            if (messageName == "start-run")
            {
                _refs.Clear();
                return;
            }

            var fullName = message.GetAttribute("fullname");
            if (string.IsNullOrEmpty(fullName))
            {
                return;
            }

            var id = message.GetAttribute("id");
            var parentId = message.GetAttribute("parentId");
            string flowId;
            if (parentId != null)
            {
                // NUnit 3 case
                string rootId;
                flowId = TryFindRootId(parentId, out rootId) ? rootId : id;
            }
            else
            {
                // NUnit 2 case
                flowId = _rootFlowId;
            }

            string testFlowId;
            if (id != flowId && parentId != null)
            {
                testFlowId = id;
            }
            else
            {
                testFlowId = flowId;
                if (testFlowId == null)
                {
                    testFlowId = id;
                }
            }

            switch (messageName.ToLowerInvariant())
            {
                case "start-suite":
                    _refs[id] = parentId;
                    // NUnit 3 case
                    if (parentId == string.Empty)
                    {
                        OnRootSuiteStart(flowId, fullName);
                    }

                    // NUnit 2 case
                    if (parentId == null)
                    {
                        if (_blockCounter++ == 0)
                        {
                            _rootFlowId = id;
                            OnRootSuiteStart(id, fullName);
                        }
                    }

                    break;

                case "test-suite":
                    _refs.Remove(id);
                    // NUnit 3 case
                    if (parentId == string.Empty)
                    {
                        OnRootSuiteFinish(flowId, fullName);
                    }

                    // NUnit 2 case
                    if (parentId == null)
                    {
                        if (--_blockCounter == 0)
                        {
                            _rootFlowId = null;
                            OnRootSuiteFinish(id, fullName);
                        }
                    }

                    break;

                case "start-test":
                    _refs[id] = parentId;
                    if (id != flowId && parentId != null)
                    {
                        OnFlowStarted(id, flowId);
                    }

                    OnTestStart(testFlowId, fullName);
                    break;

                case "test-case":
                    try
                    {
                        _refs.Remove(id);
                        var result = message.GetAttribute("result");
                        if (string.IsNullOrEmpty(result))
                        {
                            break;
                        }

                        switch (result.ToLowerInvariant())
                        {
                            case "passed":
                                _onMessage.Invoke(new Messages.OnTestSuccess(testFlowId, ParseTestResult(message), fullName));
                                break;

                            case "inconclusive":
                                OnTestInconclusive(testFlowId, message, fullName);
                                break;

                            case "skipped":
                                OnTestSkipped(testFlowId, message, fullName);
                                break;

                            case "failed":
                                OnTestFailed(testFlowId, message, fullName);
                                break;
                        }
                    }
                    finally
                    {
                        if (id != flowId && parentId != null)
                        {
                            OnFlowFinished(id);
                        }
                    }

                    break;
            }
        }

        private bool TryFindParentId(string id, out string parentId)
        {
            if (id == null)
            {
                throw new ArgumentNullException("id");
            }

            return _refs.TryGetValue(id, out parentId) && !string.IsNullOrEmpty(parentId);
        }

        private bool TryFindRootId(string id, out string rootId)
        {
            if (id == null)
            {
                throw new ArgumentNullException("id");
            }

            while (TryFindParentId(id, out rootId) && id != rootId)
            {
                id = rootId;
            }

            rootId = id;
            return !string.IsNullOrEmpty(id);
        }


        private void OnRootSuiteStart(string flowId, string assemblyName)
        {
            assemblyName = Path.GetFileName(assemblyName);
            _onMessage.Invoke(new Messages.OnRootSuiteStart(flowId, assemblyName));
        }

        private void OnRootSuiteFinish(string flowId, string assemblyName)
        {
            assemblyName = Path.GetFileName(assemblyName);
            _onMessage.Invoke(new Messages.OnRootSuiteFinish(flowId, assemblyName));
        }

        private void OnFlowStarted(string flowId, string parentFlowId)
        {
            _onMessage.Invoke(new Messages.OnFlowStarted(flowId, parentFlowId));
        }

        private void OnFlowFinished(string flowId)
        {
            _onMessage.Invoke(new Messages.OnFlowFinished(flowId));
        }

        private void OnTestStart(string flowId, string fullName)
        {
            _onMessage.Invoke(new Messages.OnTestStart(flowId, fullName));
        }

        private TestResult ParseTestResult(XmlNode message)
        {
            if (message == null)
            {
                throw new ArgumentNullException("message");
            }
            var msg = new TestResult();
            var durationStr = message.GetAttribute("duration");
            double durationDecimal;
            msg.DurationMilliseconds = 0;
            if (durationStr != null && double.TryParse(durationStr, NumberStyles.Any, CultureInfo.InvariantCulture, out durationDecimal))
            {
                msg.DurationMilliseconds = (int)(durationDecimal * 1000d);
            }
            var output = message.SelectSingleNode("output");
            if (output != null)
            {
                msg.Output = output.InnerText;
            }
            return msg;
        }

        private void OnTestFinishedSuccessFully(string flowId, XmlNode message, string fullName)
        {
            if (message == null)
            {
                throw new ArgumentNullException("message");
            }

            _onMessage.Invoke(new Messages.OnTestSuccess(flowId, ParseTestResult(message), fullName));
        }

        private void OnTestFailed(string flowId, XmlNode message, string fullName)
        {
            if (message == null)
            {
                throw new ArgumentNullException("message");
            }
            var msg = ParseTestResult(message);
            var errorMmessage = message.SelectSingleNode("failure/message");
            msg.Failure.Message = errorMmessage.InnerText;
            var stackTrace = message.SelectSingleNode("failure/stack-trace");
            msg.Failure.StackTrace = stackTrace.InnerText;

            _onMessage.Invoke(new Messages.OnTestFailed(flowId, msg, fullName));
            //sink.OnTestFinished(flowId, msg, fullName);
        }

        private void OnTestSkipped(string flowId, XmlNode message, string fullName)
        {
            if (message == null)
            {
                throw new ArgumentNullException("message");
            }

            var msg = new TestResult();
            var reason = message.SelectSingleNode("reason/message");
            msg.Reason.Message = reason.InnerText;
            _onMessage.Invoke(new Messages.OnTestSkipped(flowId, msg, fullName));
        }

        private void OnTestInconclusive(string flowId, XmlNode message, string fullName)
        {
            if (message == null)
            {
                throw new ArgumentNullException("message");
            }

            var msg = new TestResult();
            _onMessage.Invoke(new Messages.OnTestInconclusive(flowId, msg, fullName));
        }

    }
}
