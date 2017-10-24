// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.TestCommon;

namespace System.Web.Mvc.Test
{
    public class SelectListTest
    {
        [Fact]
        public void Constructor1SetsProperties()
        {
            // Arrange
            IEnumerable items = new object[0];

            // Act
            SelectList selectList = new SelectList(items);

            // Assert
            Assert.Same(items, selectList.Items);
            Assert.Null(selectList.DataValueField);
            Assert.Null(selectList.DataTextField);
            Assert.Null(selectList.SelectedValues);
            Assert.Null(selectList.SelectedValue);
        }

        [Fact]
        public void Constructor2SetsProperties_Items_SelectedValue()
        {
            // Arrange
            IEnumerable items = new object[0];
            object selectedValue = new object();

            // Act
            SelectList selectList = new SelectList(items, selectedValue);
            List<object> selectedValues = selectList.SelectedValues.Cast<object>().ToList();

            // Assert
            Assert.Same(items, selectList.Items);
            Assert.Null(selectList.DataValueField);
            Assert.Null(selectList.DataTextField);
            Assert.Same(selectedValue, selectList.SelectedValue);
            object resultValue = Assert.Single(selectedValues);
            Assert.Same(selectedValue, resultValue);
        }

        [Fact]
        public void Constructor3SetsProperties_SelectedValue_DisabledValues()
        {
            // Arrange
            IEnumerable items = new[] { "A", "B", "C" };
            IEnumerable selectedValues = "A";
            IEnumerable disabledValues = new[] { "B", "C" };

            // Act
            SelectList multiSelect = new SelectList(items, selectedValues, disabledValues);

            // Assert
            Assert.Same(items, multiSelect.Items);
            Assert.Equal(new object[] { selectedValues }, multiSelect.SelectedValues);
            Assert.Equal(disabledValues, multiSelect.DisabledValues);
            Assert.Null(multiSelect.DataTextField);
            Assert.Null(multiSelect.DataValueField);
        }

        [Fact]
        public void Constructor3SetsProperties_Value_Text()
        {
            // Arrange
            IEnumerable items = new object[0];

            // Act
            SelectList selectList = new SelectList(items, "SomeValueField", "SomeTextField");

            // Assert
            Assert.Same(items, selectList.Items);
            Assert.Equal("SomeValueField", selectList.DataValueField);
            Assert.Equal("SomeTextField", selectList.DataTextField);
            Assert.Null(selectList.SelectedValues);
            Assert.Null(selectList.SelectedValue);
        }

        [Fact]
        public void Constructor4SetsProperties()
        {
            // Arrange
            IEnumerable items = new object[0];
            object selectedValue = new object();

            // Act
            SelectList selectList = new SelectList(items, "SomeValueField", "SomeTextField", selectedValue);
            List<object> selectedValues = selectList.SelectedValues.Cast<object>().ToList();

            // Assert
            Assert.Same(items, selectList.Items);
            Assert.Equal("SomeValueField", selectList.DataValueField);
            Assert.Equal("SomeTextField", selectList.DataTextField);
            Assert.Same(selectedValue, selectList.SelectedValue);
            object resultValue = Assert.Single(selectedValues);
            Assert.Same(selectedValue, resultValue);
        }

        [Fact]
        public void Constructor_SetsProperties_Items_Value_Text_SelectedValue_DisabledValues()
        {
            // Arrange
            IEnumerable items = new[]
            {
                new { Value = "A", Text = "Alice" },
                new { Value = "B", Text = "Bravo" },
                new { Value = "C", Text = "Charlie" },
            };
            object selectedValue = "A";
            IEnumerable disabledValues = new[] { "B", "C" };

            // Act
            SelectList selectList = new SelectList(items,
                "Value",
                "Text",
                selectedValue,
                disabledValues);
            List<object> selectedValues = selectList.SelectedValues.Cast<object>().ToList();

            // Assert
            Assert.Same(items, selectList.Items);
            Assert.Equal("Value", selectList.DataValueField);
            Assert.Equal("Text", selectList.DataTextField);
            Assert.Same(selectedValue, selectList.SelectedValue);
            object resultValue = Assert.Single(selectedValues);
            Assert.Same(selectedValue, resultValue);
            Assert.Same(disabledValues, selectList.DisabledValues);
            Assert.Equal(disabledValues, selectList.DisabledValues);
            Assert.Null(selectList.DataGroupField);
            Assert.Null(selectList.DisabledGroups);
        }

        [Fact]
        public void Constructor_SetsProperties_Items_Value_Text_SelectedValue_Group()
        {
            // Arrange
            IEnumerable items = new[]
            {
                new { Value = "A", Text = "Alice", Group = "AB" },
                new { Value = "B", Text = "Bravo", Group = "AB" },
                new { Value = "C", Text = "Charlie", Group = "C" },
            };
            object selectedValue = "A";

            // Act
            SelectList selectList = new SelectList(items,
                "Value",
                "Text",
                "Group",
                selectedValue);
            List<object> selectedValues = selectList.SelectedValues.Cast<object>().ToList();

            // Assert
            Assert.Same(items, selectList.Items);
            Assert.Equal("Value", selectList.DataValueField);
            Assert.Equal("Text", selectList.DataTextField);
            Assert.Same(selectedValue, selectList.SelectedValue);
            object resultValue = Assert.Single(selectedValues);
            Assert.Same(selectedValue, resultValue);
            Assert.Equal("Group", selectList.DataGroupField);
        }

        [Fact]
        public void Constructor_SetsProperties_Items_Value_Text_SelectedValue_Group_DisabledGroups()
        {
            // Arrange
            IEnumerable items = new[]
            {
                new { Value = "A", Text = "Alice", Group = "AB" },
                new { Value = "B", Text = "Bravo", Group = "AB" },
                new { Value = "C", Text = "Charlie", Group = "C" },
            };
            object selectedValue = "A";
            IEnumerable disabledGroups = new[] { "AB" };

            // Act
            SelectList selectList = new SelectList(items,
                "Value",
                "Text",
                "Group",
                selectedValue,
                null,
                disabledGroups);
            List<object> selectedValues = selectList.SelectedValues.Cast<object>().ToList();

            // Assert
            Assert.Same(items, selectList.Items);
            Assert.Equal("Value", selectList.DataValueField);
            Assert.Equal("Text", selectList.DataTextField);
            Assert.Same(selectedValue, selectList.SelectedValue);
            object resultValue = Assert.Single(selectedValues);
            Assert.Same(selectedValue, resultValue);
            Assert.Equal("Group", selectList.DataGroupField);
            Assert.Equal(disabledGroups, selectList.DisabledGroups);
        }

        [Fact]
        public void Constructor_SetsProperties_Items_Value_Text_SelectedValue_DisabledValues_Group()
        {
            // Arrange
            IEnumerable items = new[]
            {
                new { Value = "A", Text = "Alice", Group = "AB" },
                new { Value = "B", Text = "Bravo", Group = "AB" },
                new { Value = "C", Text = "Charlie", Group = "C" },
            };
            object selectedValue = "A";
            IEnumerable disabledValues = new[] { "A", "C" };

            // Act
            SelectList selectList = new SelectList(items,
                "Value",
                "Text",
                "Group",
                selectedValue,
                disabledValues);
            List<object> selectedValues = selectList.SelectedValues.Cast<object>().ToList();

            // Assert
            Assert.Same(items, selectList.Items);
            Assert.Equal("Value", selectList.DataValueField);
            Assert.Equal("Text", selectList.DataTextField);
            Assert.Same(selectedValue, selectList.SelectedValue);
            object resultValue = Assert.Single(selectedValues);
            Assert.Same(selectedValue, resultValue);
            Assert.Equal("Group", selectList.DataGroupField);
            Assert.Same(disabledValues, selectList.DisabledValues);
        }

        [Fact]
        public void DataGroupFieldSetByCtor()
        {
            // Arrange
            IEnumerable items = new object[0];
            object selectedValue = new object();

            // Act
            SelectList selectList = new SelectList(items, "SomeValueField", "SomeTextField", "SomeGroupField",
                selectedValue);
            IEnumerable selectedValues = selectList.SelectedValues;

            // Assert
            Assert.Same(items, selectList.Items);
            Assert.Equal("SomeValueField", selectList.DataValueField);
            Assert.Equal("SomeTextField", selectList.DataTextField);
            Assert.Equal("SomeGroupField", selectList.DataGroupField);
            Assert.Same(selectedValue, selectList.SelectedValue);
            Assert.Single(selectedValues, selectedValue);
            Assert.Null(selectList.DisabledValues);
            Assert.Null(selectList.DisabledGroups);
        }
    }
}
