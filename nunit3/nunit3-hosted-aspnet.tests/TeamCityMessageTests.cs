using NUnit.Framework;
using NUnit.Hosted.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tests;

namespace NUnit.Hosted.AspNet.Tests
{
    [TestFixture]
    public class TeamCityMessageTests
    {
        private HttpContextFake context;
        private TestResults result;
        private string TeamCityOutPut;

        [TestFixtureSetUp]
        public void OnceBeforeAnyTest()
        {
            context = new HttpContextFake();
            using (var tc_output = new MemoryStream())
            using (var tc_writer = new StreamWriter(tc_output))
            {
                var location = Path.GetDirectoryName( this.GetType().Assembly.Location);
                result = Runner.Run(new HostedOptions
                {
                    InputFiles = Path.Combine(this.GetType().Assembly.Location, "TestsInWebContext.dll"),
                    WorkDirectory = location,
                }, new Messages.ISubscriber[] {
                    new TeamCityMessageWriter(tc_writer),
                });
                tc_writer.Flush();
                tc_output.Seek(0, SeekOrigin.Begin);
                TeamCityOutPut = new StreamReader(tc_output).ReadToEnd();
            }
        }

        [Test]
        public void Test()
        {
            Assert.AreEqual("<>", TeamCityOutPut);
        }
    }
}
