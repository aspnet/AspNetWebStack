// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.TestCommon;

namespace System.Web.Http.Validation
{
    public class ReferenceEqualityComparerTest
    {
        [Fact]
        public void Equals_ReturnsTrue_ForSameObject()
        {
            object o = new object();
            Assert.True(ReferenceEqualityComparer.Instance.Equals(o, o));
        }

        [Fact]
        public void Equals_ReturnsFalse_ForDifferentObject()
        {
            Object o1 = new Object();
            Object o2 = new Object();

            Assert.False(ReferenceEqualityComparer.Instance.Equals(o1, o2));
        }

        [Fact]
        public void Equals_DoesntCall_OverriddenEqualsOnTheType()
        {
            TypeThatOverridesEquals t1 = new TypeThatOverridesEquals();
            TypeThatOverridesEquals t2 = new TypeThatOverridesEquals();

            Assert.DoesNotThrow(() => ReferenceEqualityComparer.Instance.Equals(t1, t2));
        }

        [Fact]
        public void Equals_ReturnsFalse_ValueType()
        {
            Assert.False(ReferenceEqualityComparer.Instance.Equals(42, 42));
        }

        [Fact]
        public void Equals_NullEqualsNull()
        {
            var comparer = ReferenceEqualityComparer.Instance;
            Assert.True(comparer.Equals(null, null));
        }

        [Fact]
        public void GetHashCode_ReturnsSameValueForSameObject()
        {
            object o = new object();
            var comparer = ReferenceEqualityComparer.Instance;
            Assert.Equal(comparer.GetHashCode(o), comparer.GetHashCode(o));
        }

        [Fact]
        public void GetHashCode_DoesNotThrowForNull()
        {
            var comparer = ReferenceEqualityComparer.Instance;
            Assert.DoesNotThrow(() => comparer.GetHashCode(null));
        }

        private class TypeThatOverridesEquals
        {
            public override bool Equals(object obj)
            {
                throw new InvalidOperationException();
            }

            public override int GetHashCode()
            {
                throw new InvalidOperationException();
            }
        }
    }
}
