namespace NUnit.Hosted
{
    public class TestResult
    {
        public static readonly int OK = 0;
        public static readonly int INVALID_ARG = -1;
        public static readonly int INVALID_ASSEMBLY = -2;
        public static readonly int FIXTURE_NOT_FOUND = -3;
        public static readonly int UNEXPECTED_ERROR = -100;

        private int code;
        private string text;

        public TestResult(int code, string text)
        {
            this.code = code;
            this.text = text;
        }
    }
}