// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.TestCommon;

namespace System.Collections.ObjectModel
{
    public class ListWrapperCollectionTests
    {
        [Fact]
        public void ListWrapperCollection_ItemsList_HasSameContents()
        {
            // Arrange
            ListWrapperCollection<object> listWrapper = new ListWrapperCollection<object>();

            // Act
            listWrapper.Add(new object());
            listWrapper.Add(new object());

            // Assert
            Assert.Equal(listWrapper, listWrapper.ItemsList);
        }

        [Fact]
        public void ListWrapperCollection_ItemsList_IsPassedInList()
        {
            // Arrange
            List<object> list = new List<object>() { new object(), new object() };
            ListWrapperCollection<object> listWrapper = new ListWrapperCollection<object>(list);

            // Act & Assert
            Assert.Same(list, listWrapper.ItemsList);
        }
    }
}
