﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.TestCommon
{
    // This extends xUnit.net's Assert class, and makes it partial so that we can
    // organize the extension points by logical functionality (rather than dumping them
    // all into this single file).
    //
    // See files named XxxAssertions for root extensions to Assert.
    public partial class Assert : Xunit.Assert
    {
        public static readonly ReflectionAssert Reflection = new ReflectionAssert();

        public static readonly TypeAssert Type = new TypeAssert();

        public static readonly HttpAssert Http = new HttpAssert();

        public static readonly MediaTypeAssert MediaType = new MediaTypeAssert();

        public static readonly GenericTypeAssert GenericType = new GenericTypeAssert();

        public static readonly SerializerAssert Serializer = new SerializerAssert();

        public static readonly StreamAssert Stream = new StreamAssert();

        public static readonly TaskAssert Task = new TaskAssert();

        public static readonly XmlAssert Xml = new XmlAssert();

        // Method has been marked [Obsolete] in xUnit.net v2.0.0+.
        public static new void ReferenceEquals(object a, object b)
        {
            Same(a, b);
        }
    }
}
