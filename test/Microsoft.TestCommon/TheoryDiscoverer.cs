// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Microsoft.TestCommon
{
    /// <summary>
    /// An override of <see cref="Xunit.Sdk.TheoryDiscoverer"/> that provides extended capabilities.
    /// </summary>
    public class TheoryDiscoverer : Xunit.Sdk.TheoryDiscoverer
    {
        private readonly IMessageSink _diagnosticMessageSink;

        /// <summary>
        /// Instantiates a new <see cref="TheoryDiscoverer"/> instance.
        /// </summary>
        /// <param name="diagnosticMessageSink">The <see cref="IMessageSink"/> used to send diagnostic messages.</param>
        public TheoryDiscoverer(IMessageSink diagnosticMessageSink)
            : base(diagnosticMessageSink)
        {
            _diagnosticMessageSink = diagnosticMessageSink;
        }

        /// <summary>
        /// Gets the platform that the unit test is currently running on.
        /// </summary>
        protected Platform Platform
        {
            get { return PlatformInfo.Platform; }
        }

        /// <inheritdoc/>
        public override IEnumerable<IXunitTestCase> Discover(
            ITestFrameworkDiscoveryOptions discoveryOptions,
            ITestMethod testMethod,
            IAttributeInfo theoryAttribute)
        {
            var baseCases = base.Discover(discoveryOptions, testMethod, theoryAttribute);
            if (!String.IsNullOrEmpty(theoryAttribute.GetNamedArgument<string>("Skip")))
            {
                // No need to change skipped tests.
                return baseCases;
            }

            var platforms = theoryAttribute.GetNamedArgument<Platform>("Platforms");
            if ((platforms & Platform) != 0)
            {
                // No need to change tests that should run on the current platform.
                return baseCases;
            }

            // Update the individual test cases as needed: Skip test cases that would otherwise run.
            var testCases = new List<IXunitTestCase>();
            var platformJustification = theoryAttribute.GetNamedArgument<string>("PlatformJustification");
            var skipReason = String.Format(platformJustification, platforms.ToString().Replace(", ", " | "), Platform);
            foreach (var baseCase in baseCases)
            {
                if (baseCase is ExecutionErrorTestCase)
                {
                    // No need to change an erroneous test. Covered to protect against changes in the base class.
                    testCases.Add(baseCase);
                    continue;
                }

                if (!String.IsNullOrEmpty(baseCase.SkipReason))
                {
                    // No need to change a skipped test. Likely to hit this case only after xUnit.net has been updated
                    // to 2.2.0+, where [Data] also has a Skip property.
                    testCases.Add(baseCase);
                    continue;
                }

                var testCase = new SkippedXunitTestCase(
                    _diagnosticMessageSink,
                    discoveryOptions.MethodDisplayOrDefault(),
                    TestMethodDisplayOptions.None,
                    skipReason,
                    baseCase.TestMethod,
                    baseCase.TestMethodArguments);
                testCases.Add(testCase);
            }

            return testCases;
        }
    }
}