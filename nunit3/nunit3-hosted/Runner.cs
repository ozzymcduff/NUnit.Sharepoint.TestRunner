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
using NUnit.Common;

namespace NUnit.Hosted
{
    public class Runner
    {
        private readonly ITestEngine _engine;
        private readonly HostedOptions _options;
        private readonly IResultService _resultService;
        private readonly ITestFilterService _filterService;

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
                    var labels = "ON";

                    var eventHandler = new TestEventHandler(output, labels);

                    result = runner.Run(eventHandler, filter);
                    var reporter = new ResultReporter(result, output, _options);
                    reporter.ReportResults();

                    output.Flush();
                    if (reporter.Summary.UnexpectedError)
                        return new TestResult(TestResult.Code.UnexpectedError, GetResultText(ms), reporter.Summary);

                    return new TestResult(reporter.Summary.InvalidAssemblies > 0
                            ? TestResult.Code.InvalidAssembly
                            : GetCode( reporter.Summary.FailureCount + reporter.Summary.ErrorCount + reporter.Summary.InvalidCount),
                            GetResultText(ms), reporter.Summary);
                }
                catch (NUnitEngineException ex)
                {
                    output.WriteLine(ex.Message);
                    output.Flush();
                    return new TestResult(TestResult.Code.InvalidArg, GetResultText(ms));
                }
                catch (FileNotFoundException ex)
                {
                    output.WriteLine(ex.Message);
                    output.Flush();
                    return new TestResult(TestResult.Code.InvalidAssembly, GetResultText(ms));
                }
                catch (DirectoryNotFoundException ex)
                {
                    output.WriteLine(ex.Message);
                    output.Flush();
                    return new TestResult(TestResult.Code.InvalidAssembly, GetResultText(ms));
                }
                catch (Exception ex)
                {
                    output.WriteLine(ex.ToString());
                    output.Flush();
                    return new TestResult(TestResult.Code.UnexpectedError, GetResultText(ms));
                }
            }
        }

        private TestResult.Code GetCode(int v)
        {
            if (v == 0) { return TestResult.Code.Ok; }
            return TestResult.Code.TestFailure;
        }

        private string GetResultText(MemoryStream output)
        {
            output.Seek(0, SeekOrigin.Begin);
            var reader = new StreamReader(output);
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
            var builder = _filterService.GetTestFilterBuilder();
            return builder.GetFilter();
        }

        public static TestPackage MakeTestPackage(HostedOptions options)
        {
            TestPackage package = new TestPackage(options.InputFiles);
            package.AddSetting(PackageSettings.RuntimeFramework, "v4.0");

            return package;
        }

        public static TestResult Run(HostedOptions options)
        {
            using (ITestEngine engine = TestEngineActivator.CreateInstance())
            {
                if (options.WorkDirectory != null)
                    engine.WorkDirectory = options.WorkDirectory;

                return new Runner(engine, options).Execute();
            }
        }

    }
}

