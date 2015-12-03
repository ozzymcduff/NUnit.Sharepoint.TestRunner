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
using System.IO;
using NUnit.Hosted.Utilities;
using NUnit.Core;
using System.Threading;
using NUnit.Util;
using NUnit.Core.Filters;

namespace NUnit.Hosted
{
    using TestResult2 = NUnit.Hosted.TestResult;
    public class Runner
    {
        private readonly HostedOptions _options;

        public Runner(HostedOptions options)
        {
            _options = options;
        }

        private TestResult2 RunTests(TestPackage package, TestFilter filter)
        {
            NUnit.Core.TestResult result;
            ProcessModel processModel = package.Settings.Contains("ProcessModel") ? (ProcessModel)package.Settings["ProcessModel"] : ProcessModel.Default;
            DomainUsage domainUsage = package.Settings.Contains("DomainUsage") ? (DomainUsage)package.Settings[(object)"DomainUsage"] : DomainUsage.Default;
            RuntimeFramework runtimeFramework = package.Settings.Contains("RuntimeFramework") ? (RuntimeFramework)package.Settings["RuntimeFramework"] : RuntimeFramework.CurrentFramework;

            using (new SaveConsoleOutput())
            using (TestRunner testRunner = new DefaultTestRunnerFactory().MakeTestRunner(package))
            using (var ms = new MemoryStream())
            using (var output = CreateOutputWriter(ms))
            {
                try
                {
                    TestEventHandler eventCollector = new TestEventHandler(_options, output);
                    testRunner.Load(package);
                    if (testRunner.Test == null)
                    {
                        testRunner.Unload();
                        return new TestResult2(TestResult2.Code.FixtureNotFound, "Unable to locate fixture");
                    }
                    result = testRunner.Run(eventCollector, filter, false, LoggingThreshold.All);
                    var summary = eventCollector.GetSummary();

                    output.Flush();
                    if (summary.UnexpectedError)
                        return new TestResult2(TestResult2.Code.UnexpectedError, GetResultText(ms), summary);

                    return new TestResult2(summary.InvalidAssemblies > 0
                            ? TestResult2.Code.InvalidAssembly
                            : GetCode(summary.FailureCount + summary.ErrorCount + summary.InvalidCount),
                            GetResultText(ms), summary);
                }
                catch (FileNotFoundException ex)
                {
                    output.WriteLine(ex.Message);
                    output.Flush();
                    return new TestResult2(TestResult2.Code.InvalidAssembly, GetResultText(ms));
                }
                catch (DirectoryNotFoundException ex)
                {
                    output.WriteLine(ex.Message);
                    output.Flush();
                    return new TestResult2(TestResult2.Code.InvalidAssembly, GetResultText(ms));
                }
                catch (Exception ex)
                {
                    output.WriteLine(ex.ToString());
                    output.Flush();
                    return new TestResult2(TestResult2.Code.UnexpectedError, GetResultText(ms));
                }
            }
        }

        private TestResult2.Code GetCode(int v)
        {
            if (v == 0) { return TestResult2.Code.Ok; }
            return TestResult2.Code.TestFailure;
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

        public TestResult2 Execute()
        {
            var package = MakeTestPackage(this._options);
            TestFilter testFilter;
            if (!CreateTestFilter(_options, out testFilter))
                return new TestResult2(TestResult2.Code.InvalidArg, "");

            return RunTests(package, testFilter);
        }

        private bool CreateTestFilter(HostedOptions options, out TestFilter testFilter)
        {
            testFilter = TestFilter.Empty;
            SimpleNameFilter simpleNameFilter = new SimpleNameFilter();

            if (testFilter is NotFilter)
            {
                ((NotFilter)testFilter).TopLevel = true;
            }
            return true;
        }

        public static TestPackage MakeTestPackage(HostedOptions options)
        {
            ProcessModel processModel = ProcessModel.Default;
            RuntimeFramework runtimeFramework = (RuntimeFramework)null;
            TestPackage testPackage;
            DomainUsage domainUsage;
            NUnitProject nunitProject = Services.ProjectService.LoadProject(options.InputFiles);
            testPackage = nunitProject.ActiveConfig.MakeTestPackage();
            processModel = nunitProject.ProcessModel;
            domainUsage = nunitProject.DomainUsage;
            runtimeFramework = nunitProject.ActiveConfig.RuntimeFramework;

            if (!string.IsNullOrEmpty( options.WorkDirectory ))
                testPackage.BasePath = options.WorkDirectory;
            
            testPackage.TestName = null;
            testPackage.Settings["ProcessModel"] = processModel;
            testPackage.Settings["DomainUsage"] = domainUsage;
            if (runtimeFramework != null)
                testPackage.Settings["RuntimeFramework"] = runtimeFramework;
            // if (domainUsage == DomainUsage.None)
            //   CoreExtensions.Host.AddinRegistry = Services.AddinRegistry;
            testPackage.Settings["ShadowCopyFiles"] = !false;
            testPackage.Settings["UseThreadedRunner"] = !false;
            testPackage.Settings["DefaultTimeout"] = 0;
            testPackage.Settings["WorkDirectory"] = options.WorkDirectory;
            testPackage.Settings["StopOnError"] = false;
            //if (options.apartment != ApartmentState.Unknown)
            //    testPackage.Settings["ApartmentState"] = options.apartment;
            return testPackage;
        }
        private static bool init = false;
        public static TestResult2 Run(HostedOptions options)
        {
            if (!init)
            {
                ServiceManager.Services.ClearServices();
                ServiceManager.Services.AddService(new SettingsService());
                ServiceManager.Services.AddService(new DomainManager());
                ServiceManager.Services.AddService(new ProjectService());
                ServiceManager.Services.AddService(new AddinRegistry());
                ServiceManager.Services.AddService(new AddinManager());
                ServiceManager.Services.AddService(new TestAgency());
                ServiceManager.Services.InitializeServices();
                init = true;
            }
            return new Runner(options).Execute();
        }

    }
}

