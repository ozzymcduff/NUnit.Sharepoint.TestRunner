﻿// ***********************************************************************
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

namespace NUnit.Hosted
{
    /// <summary>
    /// Summary description for ResultSummary.
    /// </summary>
    public class ResultSummary
    {
        public static void InitializeCounters(ResultSummary s)
        {
            s.TestCount = 0;
            s.PassCount = 0;
            s.FailureCount = 0;
            s.ErrorCount = 0;
            s.InconclusiveCount = 0;
            s.SkipCount = 0;
            s.IgnoreCount = 0;
            s.ExplicitCount = 0;
            s.InvalidCount = 0;
            s.InvalidAssemblies = 0;
        }

        #region Properties

        /// <summary>
        /// Gets the number of test cases for which results
        /// have been summarized. Any tests excluded by use of
        /// Category or Explicit attributes are not counted.
        /// </summary>
        public int TestCount { get; set; }

        /// <summary>
        /// Returns the number of test cases actually run.
        /// </summary>
        public int RunCount
        {
            get { return PassCount + FailureCount + ErrorCount + InconclusiveCount; }
        }

        /// <summary>
        /// Returns the number of test cases not run for any reason.
        /// </summary>
        public int NotRunCount
        {
            get { return IgnoreCount + ExplicitCount + InvalidCount + SkipCount; }
        }

        /// <summary>
        /// Gets the count of passed tests
        /// </summary>
        public int PassCount { get; set; }

        /// <summary>
        /// Gets the count of failed tests, excluding errors and invalid tests
        /// </summary>
        public int FailureCount { get; set; }

        /// <summary>
        /// Returns the number of test cases that had an error.
        /// </summary>
        public int ErrorCount { get; set; }

        /// <summary>
        /// Gets the count of inconclusive tests
        /// </summary>
        public int InconclusiveCount { get; set; }

        /// <summary>
        /// Returns the number of test cases that were not runnable
        /// due to errors in the signature of the class or method.
        /// Such tests are also counted as Errors.
        /// </summary>
        public int InvalidCount { get; set; }

        /// <summary>
        /// Gets the count of skipped tests, excluding ignored and explicit tests
        /// </summary>
        public int SkipCount { get; set; }

        /// <summary>
        /// Gets the count of ignored tests
        /// </summary>
        public int IgnoreCount { get; set; }

        /// <summary>
        /// Gets the count of tests not run because the are Explicit
        /// </summary>
        public int ExplicitCount { get; set; }

        /// <summary>
        /// Gets the count of invalid assemblies
        /// </summary>
        public int InvalidAssemblies { get; set; }

        /// <summary>
        /// An Unexpected error occurred
        /// </summary>
        public bool UnexpectedError { get; set; }

        #endregion


    }
}
