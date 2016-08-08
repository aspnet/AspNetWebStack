// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.TestCommon;

namespace System.Web.Mvc.Test
{
    public class MultiSelectListTest
    {
        [Fact]
        public void Constructor1SetsProperties()
        {
            // Arrange
            IEnumerable items = new object[0];

            // Act
            MultiSelectList multiSelect = new MultiSelectList(items);

            // Assert
            Assert.Same(items, multiSelect.Items);
            Assert.Null(multiSelect.DataValueField);
            Assert.Null(multiSelect.DataTextField);
            Assert.Null(multiSelect.SelectedValues);
        }

        [Fact]
        public void Constructor2SetsProperties()
        {
            // Arrange
            IEnumerable items = new object[0];
            IEnumerable selectedValues = new object[0];

            // Act
            MultiSelectList multiSelect = new MultiSelectList(items, selectedValues);

            // Assert
            Assert.Same(items, multiSelect.Items);
            Assert.Null(multiSelect.DataValueField);
            Assert.Null(multiSelect.DataTextField);
            Assert.Same(selectedValues, multiSelect.SelectedValues);
        }

        [Fact]
        public void Constructor3SetsProperties_SelectedValues_DisabledValues()
        {
            // Arrange
            IEnumerable items = new[] { "A", "B", "C" };
            IEnumerable selectedValues = new[] { "A", "C" };
            IEnumerable disabledValues = new[] { "B", "C" };

            // Act
            MultiSelectList multiSelect = new MultiSelectList(items, selectedValues, disabledValues);

            // Assert
            Assert.Same(items, multiSelect.Items);
            Assert.Equal(selectedValues, multiSelect.SelectedValues);
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
            MultiSelectList multiSelect = new MultiSelectList(items, "SomeValueField", "SomeTextField");

            // Assert
            Assert.Same(items, multiSelect.Items);
            Assert.Equal("SomeValueField", multiSelect.DataValueField);
            Assert.Equal("SomeTextField", multiSelect.DataTextField);
            Assert.Null(multiSelect.SelectedValues);
        }

        [Fact]
        public void Constructor4SetsProperties()
        {
            // Arrange
            IEnumerable items = new object[0];
            IEnumerable selectedValues = new object[0];

            // Act
            MultiSelectList multiSelect = new MultiSelectList(items, "SomeValueField", "SomeTextField", selectedValues);

            // Assert
            Assert.Same(items, multiSelect.Items);
            Assert.Equal("SomeValueField", multiSelect.DataValueField);
            Assert.Equal("SomeTextField", multiSelect.DataTextField);
            Assert.Same(selectedValues, multiSelect.SelectedValues);
        }

        [Fact]
        public void Constructor_SetsProperties_Items_SelectedValues_DisabledValues_Groups()
        {
            // Arrange
            MultiSelectList multiSelect = new MultiSelectList(GetSampleAnonymousObjectsWithGroups(),
                "Letter", "FullWord", "Group",
                selectedValues: new[] { "A", "C", "T" },
                disabledValues: new[] { "C" },
                disabledGroups: null);

            // Act
            IList<SelectListItem> listItems = multiSelect.GetListItems();

            // Assert
            Assert.Equal(3, listItems.Count);

            Assert.Equal("A", listItems[0].Value);
            Assert.Equal("Alpha", listItems[0].Text);
            Assert.True(listItems[0].Selected);
            Assert.False(listItems[0].Disabled);

            Assert.Equal("B", listItems[1].Value);
            Assert.Equal("Bravo", listItems[1].Text);
            Assert.False(listItems[1].Selected);
            Assert.False(listItems[1].Disabled);

            Assert.Equal("C", listItems[2].Value);
            Assert.Equal("Charlie", listItems[2].Text);
            Assert.True(listItems[2].Selected);
            Assert.True(listItems[2].Disabled);

            Assert.Equal("AB", listItems[0].Group.Name);
            Assert.Equal("AB", listItems[1].Group.Name);
            Assert.Equal("C", listItems[2].Group.Name);
        }

        [Fact]
        public void Constructor_SetsProperties_Items_SelectedValues_DisabledValues_Groups_DisabledGroups()
        {
            // Arrange
            object[] disabledValues = { "C" };
            object[] disabledGroups = { "AB" };
            MultiSelectList multiSelect = new MultiSelectList(GetSampleAnonymousObjectsWithGroups(),
                "Letter", "FullWord", "Group",
                selectedValues: new[] { "A", "C", "T" },
                disabledValues: disabledValues,
                disabledGroups: disabledGroups);

            // Act
            IList<SelectListItem> listItems = multiSelect.GetListItems();

            // Assert
            // Count of Items and Groups
            Assert.Equal(3, listItems.Count);

            // Getters
            Assert.Same(disabledValues, multiSelect.DisabledValues);
            Assert.Same(disabledGroups, multiSelect.DisabledGroups);

            // Item A
            Assert.Equal("A", listItems[0].Value);
            Assert.Equal("Alpha", listItems[0].Text);
            Assert.True(listItems[0].Selected);
            Assert.False(listItems[0].Disabled);

            // Item B
            Assert.Equal("B", listItems[1].Value);
            Assert.Equal("Bravo", listItems[1].Text);
            Assert.False(listItems[1].Selected);
            Assert.False(listItems[1].Disabled);

            // Item C
            Assert.Equal("C", listItems[2].Value);
            Assert.Equal("Charlie", listItems[2].Text);
            Assert.True(listItems[2].Selected);
            Assert.True(listItems[2].Disabled);

            // Group AB
            Assert.Equal("AB", listItems[0].Group.Name);
            Assert.Equal("AB", listItems[1].Group.Name);
            Assert.True(listItems[0].Group.Disabled);
            Assert.Same(listItems[0].Group, listItems[1].Group);

            // Group C is disabled.
            Assert.Equal("C", listItems[2].Group.Name);
            Assert.False(listItems[2].Group.Disabled);
        }

        [Fact]
        public void Constructor_SetsProperties_Items_Value_Text_SelectedValues_DisabledValues_Groups()
        {
            // Arrange
            IEnumerable items = new object[0];
            IEnumerable selectedValues = new object[0];
            IEnumerable disabledValues = new object[0];

            // Act
            MultiSelectList multiSelect = new MultiSelectList(items, "SomeValueField", "SomeTextField", "SomeGroupField",
                selectedValues,
                disabledValues);

            // Assert
            Assert.Same(items, multiSelect.Items);
            Assert.Equal("SomeValueField", multiSelect.DataValueField);
            Assert.Equal("SomeTextField", multiSelect.DataTextField);
            Assert.Equal("SomeGroupField", multiSelect.DataGroupField);
            Assert.Same(selectedValues, multiSelect.SelectedValues);
            Assert.Equal(disabledValues, multiSelect.DisabledValues);
        }

        [Fact]
        public void DataGroupFieldSetByCtor()
        {
            // Arrange
            IEnumerable items = new object[0];
            IEnumerable selectedValues = new object[0];

            // Act
            MultiSelectList multiSelect = new MultiSelectList(items, "SomeValueField", "SomeTextField", "SomeGroupField",
                selectedValues);

            // Assert
            Assert.Same(items, multiSelect.Items);
            Assert.Equal("SomeValueField", multiSelect.DataValueField);
            Assert.Equal("SomeTextField", multiSelect.DataTextField);
            Assert.Same(selectedValues, multiSelect.SelectedValues);
            Assert.Equal("SomeGroupField", multiSelect.DataGroupField);
        }

        [Fact]
        public void Constructor_SetsProperties_Items_Value_Text_SelectedValues_DisabledValues()
        {
            // Arrange
            object[] disabledValues = { "C" };
            MultiSelectList multiSelect = new MultiSelectList(GetSampleAnonymousObjectsWithGroups(),
                "Letter", "FullWord",
                new[] { "A", "C", "T" },
                disabledValues);

            // Act
            IList<SelectListItem> listItems = multiSelect.GetListItems();

            // Assert
            // Count of Items
            Assert.Equal(3, listItems.Count);

            // Getters
            Assert.Same(disabledValues, multiSelect.DisabledValues);

            // Item A
            Assert.Equal("A", listItems[0].Value);
            Assert.Equal("Alpha", listItems[0].Text);
            Assert.True(listItems[0].Selected);
            Assert.False(listItems[0].Disabled);

            // Item B
            Assert.Equal("B", listItems[1].Value);
            Assert.Equal("Bravo", listItems[1].Text);
            Assert.False(listItems[1].Selected);
            Assert.False(listItems[1].Disabled);

            // Item C
            Assert.Equal("C", listItems[2].Value);
            Assert.Equal("Charlie", listItems[2].Text);
            Assert.True(listItems[2].Selected);
            Assert.True(listItems[2].Disabled);

            Assert.Equal(disabledValues, multiSelect.DisabledValues);
        }

        [Fact]
        public void ConstructorWithNullItemsThrows()
        {
            Assert.ThrowsArgumentNull(
                delegate { new MultiSelectList(null /* items */, "dataValueField", "dataTextField", null /* selectedValues */); }, "items");
        }

        [Fact]
        public void GetListItemsThrowsOnBindingFailure()
        {
            // Arrange
            MultiSelectList multiSelect = new MultiSelectList(GetSampleFieldObjects(),
                                                              "Text", "Value", new string[] { "A", "C", "T" });

            // Assert
            Assert.ThrowsHttpException(
                delegate { IList<SelectListItem> listItems = multiSelect.GetListItems(); }, "DataBinding: 'System.Web.Mvc.Test.MultiSelectListTest+Item' does not contain a property with the name 'Text'.", 500);
        }

        [Fact]
        public void GetListItemsWithoutValueField()
        {
            // Arrange
            MultiSelectList multiSelect = new MultiSelectList(GetSampleStrings());

            // Act
            IList<SelectListItem> listItems = multiSelect.GetListItems();

            // Assert
            Assert.Equal(3, listItems.Count);
            Assert.Null(listItems[0].Value);
            Assert.Equal("Alpha", listItems[0].Text);
            Assert.False(listItems[0].Selected);
            Assert.Null(listItems[1].Value);
            Assert.Equal("Bravo", listItems[1].Text);
            Assert.False(listItems[1].Selected);
            Assert.Null(listItems[2].Value);
            Assert.Equal("Charlie", listItems[2].Text);
            Assert.False(listItems[2].Selected);
        }

        [Fact]
        public void GetListItemsWithoutValueFieldWithSelections()
        {
            // Arrange
            MultiSelectList multiSelect = new MultiSelectList(GetSampleStrings(), new string[] { "Alpha", "Charlie", "Tango" });

            // Act
            IList<SelectListItem> listItems = multiSelect.GetListItems();

            // Assert
            Assert.Equal(3, listItems.Count);
            Assert.Null(listItems[0].Value);
            Assert.Equal("Alpha", listItems[0].Text);
            Assert.True(listItems[0].Selected);
            Assert.Null(listItems[1].Value);
            Assert.Equal("Bravo", listItems[1].Text);
            Assert.False(listItems[1].Selected);
            Assert.Null(listItems[2].Value);
            Assert.Equal("Charlie", listItems[2].Text);
            Assert.True(listItems[2].Selected);
        }

        [Fact]
        public void GetListItemsWithValueField()
        {
            // Arrange
            MultiSelectList multiSelect = new MultiSelectList(GetSampleAnonymousObjects(), "Letter", "FullWord");

            // Act
            IList<SelectListItem> listItems = multiSelect.GetListItems();

            // Assert
            Assert.Equal(3, listItems.Count);
            Assert.Equal("A", listItems[0].Value);
            Assert.Equal("Alpha", listItems[0].Text);
            Assert.False(listItems[0].Selected);
            Assert.Equal("B", listItems[1].Value);
            Assert.Equal("Bravo", listItems[1].Text);
            Assert.False(listItems[1].Selected);
            Assert.Equal("C", listItems[2].Value);
            Assert.Equal("Charlie", listItems[2].Text);
            Assert.False(listItems[2].Selected);
        }

        [Fact]
        public void GetListItemsWithGroupField()
        {
            // Arrange
            MultiSelectList multiSelect = new MultiSelectList(GetSampleAnonymousObjectsWithGroups(),
                                                              "Letter",
                                                              "FullWord",
                                                              "Group");

            // Act
            IList<SelectListItem> listItems = multiSelect.GetListItems();

            // Assert
            Assert.Equal(3, listItems.Count);
            Assert.Equal("A", listItems[0].Value);
            Assert.Equal("Alpha", listItems[0].Text);
            Assert.False(listItems[0].Selected);
            Assert.Equal("B", listItems[1].Value);
            Assert.Equal("Bravo", listItems[1].Text);
            Assert.False(listItems[1].Selected);
            Assert.Equal("C", listItems[2].Value);
            Assert.Equal("Charlie", listItems[2].Text);
            Assert.False(listItems[2].Selected);
            Assert.Equal("AB", listItems[0].Group.Name);
            Assert.Equal("AB", listItems[1].Group.Name);
            Assert.Equal("C", listItems[2].Group.Name);
        }

        [Fact]
        public void GetListItemsWithValueFieldWithSelections()
        {
            // Arrange
            MultiSelectList multiSelect = new MultiSelectList(GetSampleAnonymousObjects(),
                                                              "Letter", "FullWord", new string[] { "A", "C", "T" });

            // Act
            IList<SelectListItem> listItems = multiSelect.GetListItems();

            // Assert
            Assert.Equal(3, listItems.Count);
            Assert.Equal("A", listItems[0].Value);
            Assert.Equal("Alpha", listItems[0].Text);
            Assert.True(listItems[0].Selected);
            Assert.Equal("B", listItems[1].Value);
            Assert.Equal("Bravo", listItems[1].Text);
            Assert.False(listItems[1].Selected);
            Assert.Equal("C", listItems[2].Value);
            Assert.Equal("Charlie", listItems[2].Text);
            Assert.True(listItems[2].Selected);
        }

        [Fact]
        public void GetListItemsWithValueFieldWithSelectionsWithGroupField()
        {
            // Arrange
            MultiSelectList multiSelect = new MultiSelectList(GetSampleAnonymousObjectsWithGroups(),
                "Letter", "FullWord", "Group", new string[] { "A", "C", "T" });

            // Act
            IList<SelectListItem> listItems = multiSelect.GetListItems();

            // Assert
            Assert.Equal(3, listItems.Count);
            Assert.Equal("A", listItems[0].Value);
            Assert.Equal("Alpha", listItems[0].Text);
            Assert.True(listItems[0].Selected);
            Assert.Equal("B", listItems[1].Value);
            Assert.Equal("Bravo", listItems[1].Text);
            Assert.False(listItems[1].Selected);
            Assert.Equal("C", listItems[2].Value);
            Assert.Equal("Charlie", listItems[2].Text);
            Assert.True(listItems[2].Selected);
            Assert.Equal("AB", listItems[0].Group.Name);
            Assert.Equal("AB", listItems[1].Group.Name);
            Assert.Equal("C", listItems[2].Group.Name);
        }

        [Fact]
        public void IEnumerableWithAnonymousObjectsAndTextValueFields()
        {
            // Arrange
            MultiSelectList multiSelect = new MultiSelectList(GetSampleAnonymousObjects(),
                                                              "Letter", "FullWord", new string[] { "A", "C", "T" });

            // Act
            IEnumerator enumerator = multiSelect.GetEnumerator();
            enumerator.MoveNext();
            SelectListItem firstItem = enumerator.Current as SelectListItem;
            SelectListItem lastItem = null;

            while (enumerator.MoveNext())
            {
                lastItem = enumerator.Current as SelectListItem;
            }

            // Assert
            Assert.True(firstItem != null);
            Assert.Equal("Alpha", firstItem.Text);
            Assert.Equal("A", firstItem.Value);
            Assert.True(firstItem.Selected);

            Assert.True(lastItem != null);
            Assert.Equal("Charlie", lastItem.Text);
            Assert.Equal("C", lastItem.Value);
            Assert.True(lastItem.Selected);
        }

        internal static IEnumerable GetSampleAnonymousObjects()
        {
            return new[]
            {
                new { Letter = 'A', FullWord = "Alpha" },
                new { Letter = 'B', FullWord = "Bravo" },
                new { Letter = 'C', FullWord = "Charlie" }
            };
        }

        internal static IEnumerable GetSampleAnonymousObjectsWithGroups()
        {
            return new[]
            {
                new { Letter = 'A', FullWord = "Alpha", Group = "AB" },
                new { Letter = 'B', FullWord = "Bravo", Group = "AB" },
                new { Letter = 'C', FullWord = "Charlie", Group = "C" },
            };
        }

        internal static IEnumerable GetSampleFieldObjects()
        {
            return new[]
            {
                new Item { Text = "A", Value = "Alpha" },
                new Item { Text = "B", Value = "Bravo" },
                new Item { Text = "C", Value = "Charlie" }
            };
        }

        internal static List<SelectListItem> GetSampleListObjects()
        {
            List<SelectListItem> list = new List<SelectListItem>();
            string selectedSSN = "111111111";

            foreach (Person person in GetSamplePeople())
            {
                list.Add(new SelectListItem
                {
                    Text = person.FirstName,
                    Value = person.SSN,
                    Selected = String.Equals(person.SSN, selectedSSN)
                });
            }
            return list;
        }

        internal static IEnumerable<SelectListItem> GetSampleIEnumerableObjects()
        {
            Person[] people = GetSamplePeople();

            string selectedSSN = "111111111";
            IEnumerable<SelectListItem> list = from person in people
                                               select new SelectListItem
                                               {
                                                   Text = person.FirstName,
                                                   Value = person.SSN,
                                                   Selected = String.Equals(person.SSN, selectedSSN)
                                               };
            return list;
        }

        internal static IEnumerable GetSampleStrings()
        {
            return new string[] { "Alpha", "Bravo", "Charlie" };
        }

        internal static Person[] GetSamplePeople()
        {
            return new Person[]
            {
                new Person
                {
                    FirstName = "John",
                    SSN = "123456789"
                },
                new Person
                {
                    FirstName = "Jane",
                    SSN = "987654321"
                },
                new Person
                {
                    FirstName = "Joe",
                    SSN = "111111111"
                }
            };
        }

        internal class Item
        {
            public string Text;
            public string Value;
        }

        internal class Person
        {
            public string FirstName { get; set; }

            public string SSN { get; set; }
        }
    }
}
