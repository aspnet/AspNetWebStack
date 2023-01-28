// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Microsoft.TestCommon
{
    /// <summary>
    /// An <see cref="XunitTestCase"/> which is unconditionally skipped.
    /// </summary>
    /// <remarks>
    /// Similar to <c>Xunit.Sdk.XunitSkippedDataRowTestCase</c> but with a more general name. Besides, that will not
    /// exist until xUnit.net 2.2.0 is stable.
    /// </remarks>
    public class SkippedXunitTestCase : XunitTestCase
    {
        private readonly String _skipReason;

        /// <summary>
        /// Instantiates a new <see cref="SkippedXunitTestCase"/> instance.
        /// </summary>
        /// <param name="diagnosticMessageSink">The <see cref="IMessageSink"/> used to send diagnostic messages.</param>
        /// <param name="defaultMethodDisplay">Default <see cref="TestMethodDisplay"/>  to use (when not customized).</param>
        /// <param name="skipReason">The reason that this <see cref="SkippedXunitTestCase"/> will be skipped.</param>
        /// <param name="testMethod">The <see cref="ITestMethod"/> this test case belongs to.</param>
        /// <param name="testMethodArguments">The arguments for the test method.</param>
        public SkippedXunitTestCase(
            IMessageSink diagnosticMessageSink,
            TestMethodDisplay defaultMethodDisplay,
            TestMethodDisplayOptions defaultMethodDisplayOptions,
            String skipReason,
            ITestMethod testMethod,
            object[] testMethodArguments = null)
            : base(diagnosticMessageSink, defaultMethodDisplay, defaultMethodDisplayOptions, testMethod, testMethodArguments)
        {
            _skipReason = skipReason;
        }

        /// <inheritdoc/>
        protected override string GetSkipReason(IAttributeInfo factAttribute)
        {
            return _skipReason;
        }
    }
}