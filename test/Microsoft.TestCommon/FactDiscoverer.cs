// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Microsoft.TestCommon
{
    /// <summary>
    /// An override of <see cref="Xunit.Sdk.FactDiscoverer"/> that provides extended capabilities.
    /// </summary>
    class FactDiscoverer : Xunit.Sdk.FactDiscoverer
    {
        private readonly IMessageSink _diagnosticMessageSink;

        /// <summary>
        /// Instantiates a new <see cref="FactDiscoverer"/> instance.
        /// </summary>
        /// <param name="diagnosticMessageSink">The <see cref="IMessageSink"/> used to send diagnostic messages.</param>
        public FactDiscoverer(IMessageSink diagnosticMessageSink)
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
            IAttributeInfo factAttribute)
        {
            var baseCases = base.Discover(discoveryOptions, testMethod, factAttribute);
            if (!String.IsNullOrEmpty(factAttribute.GetNamedArgument<string>("Skip")))
            {
                // No need to change skipped tests.
                return baseCases;
            }

            var platforms = factAttribute.GetNamedArgument<Platform>("Platforms");
            if ((platforms & Platform) != 0)
            {
                // No need to change tests that should run on the current platform.
                return baseCases;
            }

            // Base implementation always returns a single test case.
            var baseCase = baseCases.FirstOrDefault();
            Contract.Assert(baseCase != null);
            if (baseCase is ExecutionErrorTestCase)
            {
                // No need to change an erroneous test.
                return baseCases;
            }

            if (!String.IsNullOrEmpty(baseCase.SkipReason))
            {
                // No need to change a skipped test. Covered to protect against changes in the base class.
                return baseCases;
            }

            // Replace test with its skipped equivalent.
            var platformJustification = factAttribute.GetNamedArgument<string>("PlatformJustification");
            var skipReason = String.Format(platformJustification, platforms.ToString().Replace(", ", " | "), Platform);
            var testCase = new SkippedXunitTestCase(
                _diagnosticMessageSink,
                discoveryOptions.MethodDisplayOrDefault(),
                skipReason,
                baseCase.TestMethod,
                baseCase.TestMethodArguments);

            return new[] { testCase };
        }
    }
}