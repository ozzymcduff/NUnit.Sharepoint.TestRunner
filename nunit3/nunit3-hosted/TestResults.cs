using System.Text.RegularExpressions;

namespace NUnit.Hosted
{
    public class TestResults
    {
        public enum Code
        {
            Ok = 0,
            InvalidArg = -1,
            InvalidAssembly = -2,
            FixtureNotFound = -3,
            UnexpectedError = -100,
            TestFailure = 100
        }
        private Code code;
        public readonly string Message;
        public readonly ResultSummary Summary;
        public class ErrorCodeC
        {
            private Code code;

            public ErrorCodeC(Code code)
            {
                this.code = code;
            }

            public string Text
            {
                get
                {
                    return Regex.Replace(code.ToString(), "(?<=.)([A-Z])", " $0").ToLower();
                }
            }
            public int Code { get { return (int)code; } }
        }
        public ErrorCodeC ErrorCode
        {
            get
            {
                return new ErrorCodeC(this.code);
            }
        }

        public TestResults(Code code, string message)
        {
            this.code = code;
            this.Message = message;
        }

        public TestResults(Code code, string message, ResultSummary summary) : this(code, message)
        {
            this.Summary = summary;
        }
    }
}