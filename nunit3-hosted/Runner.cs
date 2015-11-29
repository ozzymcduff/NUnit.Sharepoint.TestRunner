// ***********************************************************************
// Copyright (c) 2014 Charlie Poole
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// ***********************************************************************


using System;
using NUnit.Engine;
using System.IO;
using NUnit.Hosted.Utilities;
using System.Xml;

namespace NUnit.Hosted
{
    public class Runner
    {
        private ITestEngine _engine;
        private HostedOptions _options;
        private IResultService _resultService;
        private ITestFilterService _filterService;

        public Runner(ITestEngine engine, HostedOptions options)
        {
            _engine = engine;
            _options = options;
            _resultService = _engine.Services.GetService<IResultService>();
            _filterService = _engine.Services.GetService<ITestFilterService>();

        }

        private TestResult RunTests(TestPackage package, TestFilter filter)
        {
            XmlNode result;

            using (new SaveConsoleOutput())
            using (ITestRunner runner = _engine.GetRunner(package))
            using (var ms = new MemoryStream())
            using (var output = CreateOutputWriter(ms))
            {
                try
                {
                    var labels = !string.IsNullOrEmpty( _options.DisplayTestLabels)
                       ? _options.DisplayTestLabels.ToUpperInvariant()
                       : "ON";

                    var eventHandler = new TestEventHandler(output, labels, _options.TeamCity);

                    result = runner.Run(eventHandler, filter);
                    var reporter = new ResultReporter(result, output, _options);
                    reporter.ReportResults();

                    output.Flush();
                    if (reporter.Summary.UnexpectedError)
                        return new TestResult(TestResult.UNEXPECTED_ERROR, GetResultText(ms));

                    return new TestResult(reporter.Summary.InvalidAssemblies > 0
                            ? TestResult.INVALID_ASSEMBLY
                            : reporter.Summary.FailureCount + reporter.Summary.ErrorCount + reporter.Summary.InvalidCount,
                            GetResultText(ms));

                }
                catch (NUnitEngineException ex)
                {
                    output.WriteLine(ex.Message);
                    return new TestResult(TestResult.INVALID_ARG, GetResultText(ms));
                }
                catch (FileNotFoundException ex)
                {
                    output.WriteLine(ex.Message);
                    return new TestResult(TestResult.INVALID_ASSEMBLY, GetResultText(ms));
                }
                catch (DirectoryNotFoundException ex)
                {
                    output.WriteLine(ex.Message);
                    return new TestResult(TestResult.INVALID_ASSEMBLY, GetResultText(ms));
                }
                catch (Exception ex)
                {
                    output.WriteLine(ex.ToString());
                    return new TestResult(TestResult.UNEXPECTED_ERROR, GetResultText(ms));
                }
            }
        }

        private string GetResultText(MemoryStream output)
        {
            output.Seek(0, SeekOrigin.Begin);
            using (var reader = new StreamReader(output))
                return reader.ReadToEnd();
        }

        private TextWriter CreateOutputWriter(MemoryStream output)
        {
            return new StreamWriter(output);
        }

        public TestResult Execute()
        {
            var package = MakeTestPackage(this._options);
            TestFilter filter = CreateTestFilter(_options);

            return RunTests(package, filter);
        }

        private TestFilter CreateTestFilter(HostedOptions _options)
        {
            ITestFilterBuilder builder = _filterService.GetTestFilterBuilder();

            foreach (string testName in _options.TestList)
                builder.AddTest(testName);

            if (_options.WhereClauseSpecified)
                builder.SelectWhere(_options.WhereClause);

            return builder.GetFilter();
        }

        public static TestPackage MakeTestPackage(HostedOptions options)
        {
            TestPackage package = new TestPackage(options.InputFiles);
            return package;
        }

        public static TestResult Run(HostedOptions options)
        {
            using (ITestEngine engine = TestEngineActivator.CreateInstance())
            {
                if (options.WorkDirectory != null)
                    engine.WorkDirectory = options.WorkDirectory;

                if (options.InternalTraceLevel != null)
                    engine.InternalTraceLevel = (InternalTraceLevel)Enum.Parse(typeof(InternalTraceLevel), options.InternalTraceLevel);
                return new Runner(engine, options).Execute();
            }
        }

    }
}

