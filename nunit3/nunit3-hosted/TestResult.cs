namespace NUnit.Hosted
{
    public class TestResult
    {
        public int DurationMilliseconds;
        public FailureResult Failure = new FailureResult();
        public ReasonResult Reason = new ReasonResult();
        public string Output;

        public class FailureResult
        {
            public string Message;
            public string StackTrace;
        }
        public class ReasonResult
        {
            public string Message;
        }
    }
}
