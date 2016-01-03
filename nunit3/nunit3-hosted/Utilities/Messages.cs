using System;

namespace NUnit.Hosted.Utilities
{
    public class Messages
    {
        public delegate void OnMessage(IMessage message);

        public enum Type
        {
            OnTestStart,
            OnRootSuiteStart,
            OnRootSuiteFinish,
            OnFlowStarted,
            OnFlowFinished,
            OnTestFinishedSuccessFully,
            OnTestFailed,
            OnTestSkipped,
            OnTestInconclusive
        }

        public interface IHandleAll
        {
            void OnTestStart(string flowId, string fullName);
            void OnRootSuiteStart(string flowId, string assemblyName);
            void OnRootSuiteFinish(string flowId, string assemblyName);
            void OnFlowStarted(string flowId, string parentFlowId);
            void OnFlowFinished(string flowId);
            void OnTestFinishedSuccessFully(string flowId, TestResult msg, string fullName);
            void OnTestFailed(string flowId, TestResult msg, string fullName);
            void OnTestSkipped(string flowId, TestResult msg, string fullName);
            void OnTestInconclusive(string flowId, TestResult msg, string fullName);
        }

        public class CombineSubscribers 
        {
            private OnMessage[] subscribers;
            public CombineSubscribers(OnMessage[] subscribers)
            {
                this.subscribers = subscribers;
            }

            public void OnMessage(IMessage message)
            {
                foreach (var subscriber in subscribers)
                {
                    subscriber.Invoke(message);
                }
            }
        }

        public class HandleAllSubscriber 
        {
            private readonly IHandleAll handler;
            public HandleAllSubscriber(IHandleAll handleAllMessages)
            {
                handler = handleAllMessages;
            }
            public void OnMessage(IMessage message)
            {
                switch (message.Type)
                {
                    case Type.OnTestStart:
                        {
                            var m = (OnTestStart)message;
                            handler.OnTestStart(m.FlowId, m.FullName);
                        }
                        break;
                    case Type.OnRootSuiteStart:
                        {
                            var m = (OnRootSuiteStart)message;
                            handler.OnRootSuiteStart(m.FlowId, m.AssemblyName);
                        }
                        break;
                    case Type.OnRootSuiteFinish:
                        {
                            var m = (OnRootSuiteFinish)message;
                            handler.OnRootSuiteFinish(m.FlowId, m.AssemblyName);
                        }
                        break;
                    case Type.OnFlowStarted:
                        {
                            var m = (OnFlowStarted)message;
                            handler.OnFlowStarted(m.FlowId, m.ParentFlowId);
                        }
                        break;
                    case Type.OnFlowFinished:
                        {
                            var m = (OnFlowFinished)message;
                            handler.OnFlowFinished(m.FlowId);
                        }
                        break;
                    case Type.OnTestFinishedSuccessFully:
                        {
                            var m = (OnTestSuccess)message;
                            handler.OnTestFinishedSuccessFully(m.FlowId, m.Result, m.FullName);
                        }
                        break;
                    case Type.OnTestFailed:
                        {
                            var m = (OnTestFailed)message;
                            handler.OnTestFailed(m.FlowId, m.Result, m.FullName);
                        }
                        break;
                    case Type.OnTestSkipped:
                        {
                            var m = (OnTestSkipped)message;
                            handler.OnTestSkipped(m.FlowId, m.Result, m.FullName);
                        }
                        break;
                    case Type.OnTestInconclusive:
                        {
                            var m = (OnTestInconclusive)message;
                            handler.OnTestInconclusive(m.FlowId, m.Result, m.FullName);
                        }
                        break;
                    default:
                        throw new Exception("Unknown message type " + message.Type);
                }
            }
        }

        public class OnTestStart : IMessage
        {
            public readonly string FlowId;
            public readonly string FullName;
            public OnTestStart(string flowId, string fullName) { this.FlowId = flowId; this.FullName = fullName; }
            public Type Type { get { return Type.OnTestStart; } }
        }

        public class OnRootSuiteStart : IMessage
        {
            public readonly string FlowId;
            public readonly string AssemblyName;
            public OnRootSuiteStart(string flowId, string assemblyName) { this.FlowId = flowId; this.AssemblyName = assemblyName; }
            public Type Type { get { return Type.OnRootSuiteStart; } }
        }

        public class OnRootSuiteFinish : IMessage
        {
            public readonly string FlowId;
            public readonly string AssemblyName;
            public OnRootSuiteFinish(string flowId, string assemblyName) { this.FlowId = flowId; this.AssemblyName = assemblyName; }
            public Type Type { get { return Type.OnRootSuiteFinish; } }
        }

        public class OnFlowStarted : IMessage
        {
            public readonly string FlowId;
            public readonly string ParentFlowId;
            public OnFlowStarted(string flowId, string parentFlowId) { this.FlowId = flowId; this.ParentFlowId = parentFlowId; }
            public Type Type { get { return Type.OnFlowStarted; } }
        }
        public class OnFlowFinished : IMessage
        {
            public readonly string FlowId;
            public OnFlowFinished(string flowId) { this.FlowId = flowId; }
            public Type Type { get { return Type.OnFlowFinished; } }
        }
        public class OnTestSuccess : IMessage
        {
            public readonly string FlowId;
            public readonly TestResult Result;
            public readonly string FullName;
            public OnTestSuccess(string flowId, TestResult result, string fullName)
            {
                this.FlowId = flowId;
                this.Result = result;
                this.FullName = fullName;
            }
            public Type Type { get { return Type.OnTestFinishedSuccessFully; } }
        }

        public class OnTestFailed : IMessage
        {
            public readonly string FlowId;
            public readonly TestResult Result;
            public readonly string FullName;
            public OnTestFailed(string flowId, TestResult result, string fullName)
            {
                this.FlowId = flowId;
                this.Result = result;
                this.FullName = fullName;
            }
            public Type Type { get { return Type.OnTestFailed; } }
        }

        public class OnTestSkipped : IMessage
        {
            public readonly string FlowId;
            public readonly TestResult Result;
            public readonly string FullName;
            public OnTestSkipped(string flowId, TestResult result, string fullName)
            {
                this.FlowId = flowId;
                this.Result = result;
                this.FullName = fullName;
            }
            public Type Type { get { return Type.OnTestSkipped; } }
        }

        public class OnTestInconclusive : IMessage
        {
            public readonly string FlowId;
            public readonly TestResult Result;
            public readonly string FullName;
            public OnTestInconclusive(string flowId, TestResult result, string fullName)
            {
                this.FlowId = flowId;
                this.Result = result;
                this.FullName = fullName;
            }
            public Type Type { get { return Type.OnTestInconclusive; } }
        }
    }
}
