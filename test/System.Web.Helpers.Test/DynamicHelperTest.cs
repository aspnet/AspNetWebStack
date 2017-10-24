// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Dynamic;
using Microsoft.Internal.Web.Utils;
using Microsoft.TestCommon;

namespace System.Web.Helpers.Test
{
    public class DynamicHelperTest
    {
        [Fact]
        public void TryGetMemberValueReturnsValueIfBinderIsNotCSharp()
        {
            // Arrange
            var mockMemberBinder = new MockMemberBinder("Foo");
            var dynamic = new DynamicWrapper(new { Foo = "Bar" });

            // Act
            object value;
            bool result = DynamicHelper.TryGetMemberValue(dynamic, mockMemberBinder, out value);

            // Assert
            Assert.Equal("Bar", value);
        }

        private class MockMemberBinder : GetMemberBinder
        {
            public MockMemberBinder(string name)
                : base(name, false)
            {
            }

            public override DynamicMetaObject FallbackGetMember(DynamicMetaObject target, DynamicMetaObject errorSuggestion)
            {
                throw new NotImplementedException();
            }
        }
    }
}
