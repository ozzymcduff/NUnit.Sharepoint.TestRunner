using NUnit.Hosted.Utilities;
using System;
using System.IO;

namespace NUnit.Hosted.AspNet
{
    public partial class Api1 : System.Web.UI.Page
    {
        private TestResults result;
        public int Passed { get { return result.Summary.PassCount; } }
        public int Total { get { return result.Summary.RunCount; } }
        public int Failed { get { return result.Summary.FailureCount + result.Summary.ErrorCount; } }
        public bool Failure { get { return result.Summary.IsFailure; } }
        public string HeaderTitle { get; private set; }
        public string ConsoleOut { get; private set; }
        public string RenderedResults { get; private set; }
        protected void Page_Load(object sender, EventArgs e)
        {
            var context = HttpUtility.Wrap(this.Context);
            using (var tc_output = new MemoryStream())
            using (var tc_writer = new StreamWriter(tc_output))
            using (var renderer_output = new MemoryStream())
            using (var renderer_writer = new StreamWriter(renderer_output))
            {
                result = Runner.Run(new HostedOptions
                {
                    InputFiles = Path.Combine(Path.Combine(context.Request.MapPath("/"), "bin"), "TestsInWebContext.dll"),
                    WorkDirectory = this.GetType().Assembly.Location,
                }, new Messages.ISubscriber[] {
                    new TeamCityMessageWriter(tc_writer),
                    new NUnitRenderer(context, renderer_writer),
                    //new AppendMessagesToFile("C:\\src\\messages.json")
                });
                tc_writer.Flush();
                tc_output.Seek(0, SeekOrigin.Begin);
                ConsoleOut = result.Message + Environment.NewLine
                    + new StreamReader(tc_output).ReadToEnd();
                renderer_writer.Flush();
                renderer_output.Seek(0, SeekOrigin.Begin);
                RenderedResults = new StreamReader(renderer_output).ReadToEnd();
            }
            HeaderTitle = "TestsInWebContext";
        }
    }
}