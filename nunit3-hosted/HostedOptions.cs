using System.Collections.Generic;

namespace NUnit.Hosted
{
    public class HostedOptions
    {
        public string DisplayTestLabels { get; set; }
        public string InputFiles { get; set; }
        public string InternalTraceLevel { get; set; }
        public bool StopOnError { get; set; }
        public bool TeamCity { get; set; }
        public IEnumerable<string> TestList { get; set; }
        public string WhereClause { get; set; }
        public bool WhereClauseSpecified { get { return !string.IsNullOrEmpty(WhereClause); } }
        public string WorkDirectory { get; set; }
    }
}