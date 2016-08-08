// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.TestCommon;

namespace System.Web.Http.Tracing
{
    public class TraceRecordTest
    {
        [Fact]
        public void TraceRecord_TraceKind_RoundTrips()
        {
            Assert.Reflection.EnumProperty(
                new TraceRecord(request: null, category: null, level: TraceLevel.Info),
                r => r.Kind,
                expectedDefaultValue: TraceKind.Trace,
                illegalValue: (TraceKind)999,
                roundTripTestValue: TraceKind.End);
        }

        [Fact]
        public void TraceRecord_TraceLevel_RoundTrips()
        {
            Assert.Reflection.EnumProperty(
                new TraceRecord(request: null, category: null, level: TraceLevel.Info),
                r => r.Level,
                expectedDefaultValue: TraceLevel.Info,
                illegalValue: (TraceLevel)999,
                roundTripTestValue: TraceLevel.Fatal);
        }
    }
}
