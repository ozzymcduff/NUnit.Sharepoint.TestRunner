using NUnit.Framework;
using NUnit.Hosted.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using With;

namespace NUnit.Hosted.AspNet.Tests
{
    [TestFixture]
    public class AppendMessagesToFileTests
    {
        private List<string> dbs = new List<string>();
        private IEnumerable<IMessage> GetMessages()
        {
            return new IMessage[]{
                new Messages.OnTestStart("1","name"),
                new Messages.OnTestFailed("1", new TestResult(), "name"),
                new Messages.OnTestSuccess("2", new TestResult(), "name_2")
            };
        }

        [TearDown]
        public void TearDown()
        {
            foreach (var db in dbs)
            {
                File.WriteAllText(db, string.Empty);
            }
        }

        [Test]
        public void Read_items()
        {
            var messsages = GetMessages().ToArray();
            var _persist = new AppendMessagesToFile("Json1.db".Tap(db => dbs.Add(db)));
            foreach (var message in messsages)
            {
                _persist.OnMessage(message);
            }
            Assert.That(_persist.ReadAll().Select(c => c.GetType()).ToArray(), Is.EquivalentTo(messsages.Select(c => c.GetType()).ToArray()));
        }
    }
}
