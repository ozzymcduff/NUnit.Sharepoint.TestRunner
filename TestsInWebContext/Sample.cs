using NUnit.Framework;

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
    }
}
