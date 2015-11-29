using NUnit.HostedRunner;
using System;
using System.Collections.Generic;
using System.Text;
using NUnit.ConsoleRunner;
using NUnit.Engine;

namespace nunit3_hosted
{
    public class Run
    {
        public HostedOptions Options { get; private set; }
        public object OutWriter { get; private set; }

        public TestResult RunRun()
        {
            using (ITestEngine engine = TestEngineActivator.CreateInstance())
            {
                if (Options.WorkDirectory != null)
                    engine.WorkDirectory = Options.WorkDirectory;

                if (Options.InternalTraceLevel != null)
                    engine.InternalTraceLevel = (InternalTraceLevel)Enum.Parse(typeof(InternalTraceLevel), Options.InternalTraceLevel);

                return new Runner(engine, Options).Execute();
            }
        }
    }
}
