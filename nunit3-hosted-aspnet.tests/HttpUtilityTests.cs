using NUnit.Framework;
using NUnit.Hosted.AspNet;
using System;

namespace Tests
{
    [TestFixture]
    public class HttpUtilityTests
    {
        [Test]
        public void Can_get_last_part_of_path()
        {
            var last = HttpUtility.GetLastPath(new Uri( "http://first_path/file.ashx/last_path?query=string"));
            Assert.AreEqual("last_path", last);
        }
    }
}
