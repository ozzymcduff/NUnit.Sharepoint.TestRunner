using System.Collections.Generic;

namespace NUnit.Hosted
{
    public class HostedOptions
    {
        public string InputFiles { get; internal set; }
        public string InternalTraceLevel { get; internal set; }
        public bool StopOnError { get; internal set; }
        public bool TeamCity { get; internal set; }
        public IEnumerable<string> TestList { get; internal set; }
        public string WhereClause { get; internal set; }
        public bool WhereClauseSpecified { get { return !string.IsNullOrEmpty(WhereClause); } }
        public string WorkDirectory { get; internal set; }
    }
}