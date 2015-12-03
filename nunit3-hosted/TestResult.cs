namespace NUnit.Hosted
{
    public class TestResult
    {
        public const int OK = 0;
        public const int INVALID_ARG = -1;
        public const int INVALID_ASSEMBLY = -2;
        public const int FIXTURE_NOT_FOUND = -3;
        public const int UNEXPECTED_ERROR = -100;

        private int code;
        public readonly string Text;
        public string ErrorCode
        {
            get
            {
                switch (code)
                {
                    case OK: return "OK";
                    case INVALID_ARG: return "INVALID_ARG";
                    case INVALID_ASSEMBLY: return "INVALID_ASSEMBLY";
                    case FIXTURE_NOT_FOUND: return "FIXTURE_NOT_FOUND";
                    case UNEXPECTED_ERROR: return "UNEXPECTED_ERROR";
                    default:
                        return "UNKNOWN ERROR CODE";
                }
            }
        }

        public TestResult(int code, string text)
        {
            this.code = code;
            this.Text = text;
        }
    }
}