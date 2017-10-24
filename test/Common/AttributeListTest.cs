// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using Microsoft.TestCommon;

namespace System.ComponentModel
{
    public class AttributeListTest
    {
        private readonly Attribute[] _testAttributes;
        private readonly AttributeCollection _collection;
        private readonly AttributeList _list;

        public AttributeListTest()
        {
            _testAttributes = new Attribute[] { new TestAttribute(), new DerivedAttribute(), new DerivedDerivedAttribute() };
            _collection = new AttributeCollection(_testAttributes);
            _list = new AttributeList(_collection);
        }

        [Fact]
        public void AttributeListCountMatchesWrapped()
        {
            Assert.Equal(_collection.Count, _list.Count);
        }

        [Fact]
        public void AttributeListIsReadOnlyTrue()
        {
            Assert.True(_list.IsReadOnly);
        }

        [Fact]
        public void AttributeListIndexerMatchesWrapped()
        {
            Assert.Equal(_collection[1], _list[1]);
        }

        [Fact]
        public void AttributeListAddThrows()
        {
            Assert.Throws<NotSupportedException>(() => _list.Add(null));
        }

        [Fact]
        public void AttributeListClearThrows()
        {
            Assert.Throws<NotSupportedException>(() => _list.Clear());
        }

        [Fact]
        public void AttributeListContainsWrappedTrue()
        {
            Attribute presentAttribute = _collection[2];
            Assert.Contains(presentAttribute, _list);
        }

        [Fact]
        public void AttributeListContainsMissingFalse()
        {
            Attribute missingAttribute = new MissingAttribute();
            Assert.DoesNotContain(missingAttribute, _list);
        }

        [Fact]
        public void AttributeListCopyToResultsEqual()
        {
            Attribute[] arrayCopy = new Attribute[3];
            _list.CopyTo(arrayCopy, 0);
            Assert.Equal(_list, arrayCopy);
        }

        [Fact]
        public void AttributeListIndexOfMatchesIndexer()
        {
            Assert.Equal(1, _list.IndexOf(_list[1]));
        }

        [Fact]
        public void AttributeListRemoveAtThrows()
        {
            Assert.Throws<NotSupportedException>(() => _list.RemoveAt(0));
            Assert.Throws<NotSupportedException>(() => ((ICollection<Attribute>)_list).Remove(_list[0]));
        }

        [Fact]
        public void AttributeListEnumerationMatchesWrapped()
        {
            int i = 0;
            foreach (Attribute attribute in _list)
            {
                Assert.Equal(_collection[i], attribute);
                i++;
            }
            Assert.Equal(_collection.Count, i);

            i = 0;
            IEnumerable asEumerable = _list as IEnumerable;
            foreach (Attribute attribute in asEumerable)
            {
                Assert.Equal(_collection[i], attribute);
                i++;
            }
            Assert.Equal(_collection.Count, i);
        }

        private class TestAttribute : Attribute
        {
            public TestAttribute() { }
        }

        private class DerivedAttribute: TestAttribute
        {
            public DerivedAttribute() { }
        }

        private class DerivedDerivedAttribute : DerivedAttribute
        {
            public DerivedDerivedAttribute() { }
        }

        private class MissingAttribute : Attribute
        {
            public MissingAttribute() { }
        }
    }
}
