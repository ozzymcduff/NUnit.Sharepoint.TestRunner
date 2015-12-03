﻿using NUnit.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace NUnit.Hosted
{
    public class HostedOptions
    {
        //    Apartment for running tests: MTA (Default), STA
        public ApartmentState apartment = ApartmentState.Unknown;
        /// Project configuration (e.g.: Debug) to load
        public string config;
        /// Set internal trace level: Off, Error, Warning, Info, Verbose
        public InternalTraceLevel trace;
        ///List of categories to include
        public string include;
        ///List of categories to exclude
        public string exclude;
        ///  Framework version to be used for tests
        public string framework;
        ///Process model for tests: Single, Separate, Multiple
        public ProcessModel process;
        ///AppDomain Usage for tests: None, Single, Multiple
        public DomainUsage domain;
        /// Disable shadow copy when running in separate domain
        public bool noshadow;
        /// Disable use of a separate thread for tests
        public bool nothread;
        ///  Base path to be used when loading the assemblies
        public string basepath;
        /// Additional directories to be probed when loading assemblies, separated by semicolons
        public string privatebinpath;
        ///Set timeout for each test case in milliseconds
        public int timeout;
        public string InputFiles { get; set; }
        public string WorkDirectory { get; set; }

    }
}
