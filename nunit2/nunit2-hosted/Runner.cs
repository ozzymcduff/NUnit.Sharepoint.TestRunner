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
    public class Runner
    {
        private readonly HostedOptions _options;

        public Runner(HostedOptions options)
        {
            _options = options;
        }

        private TestResult2 RunTests(TestPackage package, TestFilter filter)
        {
            TestResult result;
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
                    TestEventHandler eventCollector = new TestEventHandler(_options, output, output);
                    testRunner.Load(package);
                    if (testRunner.Test == null)
                    {
                        testRunner.Unload();
                        return new TestResult2(TestResult2.Code.FixtureNotFound, "Unable to locate fixture");
                    }
                    var labels = _options.labels;

                    var eventHandler = new TestEventHandler(_options, output, output);

                    result = testRunner.Run(eventHandler, filter, false, LoggingThreshold.All);
                    var summary = eventHandler.GetSummary();

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

            if (options.include != null && options.include != string.Empty)
            {
                TestFilter filter = new CategoryExpression(options.include).Filter;
                Console.WriteLine("Included categories: " + filter.ToString());
                if (testFilter.IsEmpty)
                    testFilter = filter;
                else
                    testFilter = new AndFilter(new ITestFilter[2]
                    {
             testFilter,
             filter
                    });
            }
            if (options.exclude != null && options.exclude != string.Empty)
            {
                TestFilter testFilter1 = new NotFilter(new CategoryExpression(options.exclude).Filter);
                Console.WriteLine("Excluded categories: " + testFilter1.ToString());
                if (testFilter.IsEmpty)
                    testFilter = testFilter1;
                else if (testFilter is AndFilter)
                    ((AndFilter)testFilter).Add(testFilter1);
                else
                    testFilter = new AndFilter(new ITestFilter[2]
                    {
             testFilter,
             testFilter1
                    });
            }
            if (testFilter is NotFilter)
                ((NotFilter)testFilter).TopLevel = true;
            return true;
        }

        public static TestPackage MakeTestPackage(HostedOptions options)
        {
            ProcessModel processModel = ProcessModel.Default;
            RuntimeFramework runtimeFramework = (RuntimeFramework)null;
            TestPackage testPackage;
            DomainUsage domainUsage;
            NUnitProject nunitProject = Services.ProjectService.LoadProject(options.InputFiles);
            string name = options.config;
            if (name != null)
                nunitProject.SetActiveConfig(name);
            testPackage = nunitProject.ActiveConfig.MakeTestPackage();
            processModel = nunitProject.ProcessModel;
            domainUsage = nunitProject.DomainUsage;
            runtimeFramework = nunitProject.ActiveConfig.RuntimeFramework;

            if (options.basepath != null && options.basepath != string.Empty)
                testPackage.BasePath = options.basepath;
            if (options.privatebinpath != null && options.privatebinpath != string.Empty)
            {
                testPackage.AutoBinPath = false;
                testPackage.PrivateBinPath = options.privatebinpath;
            }
            if (options.framework != null)
                runtimeFramework = RuntimeFramework.Parse(options.framework);
            if (options.process != ProcessModel.Default)
                processModel = options.process;
            if (options.domain != DomainUsage.Default)
                domainUsage = options.domain;
            testPackage.TestName = null;
            testPackage.Settings["ProcessModel"] = processModel;
            testPackage.Settings["DomainUsage"] = domainUsage;
            if (runtimeFramework != null)
                testPackage.Settings["RuntimeFramework"] = runtimeFramework;
            // if (domainUsage == DomainUsage.None)
            //   CoreExtensions.Host.AddinRegistry = Services.AddinRegistry;
            testPackage.Settings["ShadowCopyFiles"] = !options.noshadow;
            testPackage.Settings["UseThreadedRunner"] = !options.nothread;
            testPackage.Settings["DefaultTimeout"] = options.timeout;
            testPackage.Settings["WorkDirectory"] = options.WorkDirectory;
            testPackage.Settings["StopOnError"] = false;
            if (options.apartment != ApartmentState.Unknown)
                testPackage.Settings["ApartmentState"] = options.apartment;
            return testPackage;
        }
        private static bool init = false;
        public static TestResult2 Run(HostedOptions options)
        {
            if (!init)
            {
                SettingsService settingsService = new SettingsService();
                ServiceManager.Services.AddService((IService)settingsService);
                ServiceManager.Services.AddService((IService)new DomainManager());
                ServiceManager.Services.AddService((IService)new ProjectService());
                ServiceManager.Services.AddService((IService)new AddinRegistry());
                ServiceManager.Services.AddService((IService)new AddinManager());
                ServiceManager.Services.AddService((IService)new TestAgency());
                ServiceManager.Services.InitializeServices();
                init = true;
            }
            return new Runner(options).Execute();
        }

    }
}

