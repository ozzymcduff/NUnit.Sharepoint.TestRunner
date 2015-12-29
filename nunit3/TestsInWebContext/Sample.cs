using NUnit.Framework;
using System;

namespace TestsInWebContext
{
    [TestFixture]
    public class Sample_fixture
    {
        [Test]
        public void A_failing_test()
        {
            Assert.Fail();
        }

        [Test]
        public void A_successful_test_with_output()
        {
            Console.WriteLine("output from console");
            Console.Error.WriteLine("output from error console");
        }
    }
}
