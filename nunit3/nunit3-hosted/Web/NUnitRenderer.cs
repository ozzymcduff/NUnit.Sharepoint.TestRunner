using MvcTagBuilder;
using NUnit.Hosted.Utilities;
using System;
using System.IO;
using System.Linq;

namespace NUnit.Hosted.AspNet
{
    /// <summary>
    /// 
    /// </summary>
    public class NUnitRenderer 
    {
        private readonly IHttpContext context;
        private TextWriter _out;
        public NUnitRenderer(IHttpContext context, TextWriter @out)
        {
            this.context = context;
            _out = @out;
        }

        string AppendTest(string name, string testId, string moduleName, TestResult details, bool isFailure)
        {
            var title = new TagBuilder("strong");
            title.InnerHtml = GetNameHtml(name, moduleName);

            var testBlock = new TagBuilder("li");
            testBlock.InnerHtml = title.ToString();
            testBlock.GenerateId("nunit-test-output-" + testId);
            testBlock.AddCssClass(!isFailure ? "pass" : "fail");

            var assertList = new TagBuilder("ol");
            assertList.AddCssClass("nunit-assert-list");

            assertList.InnerHtml += Environment.NewLine + Log(details, isFailure);
            testBlock.InnerHtml += Environment.NewLine + assertList.ToString();

            return testBlock.ToString();
        }
        /// <summary>
        /// append to TagBuilder assertList, 
        /// </summary>
        string Log(TestResult details, bool isFailure)
        {
            var message = HasContent(details)
                ? EscapeText(string.Join(Environment.NewLine, GetContent(details)))
                : ("failed");
            message = "<span class='test-message'>" + message + "</span>";

            var assertLi = new TagBuilder("li");
            assertLi.AddCssClass(!isFailure ? "pass" : "fail");
            assertLi.InnerHtml = message;
            return assertLi.ToString();
        }

        private string[] GetContent(TestResult details)
        {
            return new[] { details.Output, details.Reason.Message, details.Failure.Message, details.Failure.StackTrace }
                .Where(s => !string.IsNullOrEmpty(s)).ToArray();
        }

        private bool HasContent(TestResult details)
        {
            return GetContent(details).Any();
        }

        private string GetNameHtml(string name, string module)
        {
            var nameHtml = "";

            if (!string.IsNullOrEmpty(module))
            {
                nameHtml = "<span class='module-name'>" + EscapeText(module) + "</span>: ";
            }

            nameHtml += "<span class='test-name'>" + EscapeText(name) + "</span>";

            return nameHtml;
        }

        private string EscapeText(string module)
        {
            return HttpUtility.HtmlEncode(module);
        }

        private void OnTestFinishedSuccessFully(string flowId, TestResult msg, string fullName)
        {
            _out.WriteLine(AppendTest(fullName, flowId, "module", msg, isFailure: false));
        }

        private void OnTestFailed(string flowId, TestResult msg, string fullName)
        {
            _out.WriteLine(AppendTest(fullName, flowId, "module", msg, isFailure: true));
        }

        public void OnMessage(IMessage message)
        {
            switch (message.Type)
            {
                case Messages.Type.OnTestFinishedSuccessFully:
                    {
                        var m = (Messages.OnTestSuccess)message;
                        OnTestFinishedSuccessFully(m.FlowId, m.Result, m.FullName);
                    }
                    break;
                case Messages.Type.OnTestFailed:
                    {
                        var m = (Messages.OnTestFailed)message;
                        OnTestFailed(m.FlowId, m.Result, m.FullName);
                    }
                    break;
                default:
                    break;
            }
        }
    }
}