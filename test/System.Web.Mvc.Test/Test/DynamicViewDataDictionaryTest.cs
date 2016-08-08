// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Mvc.Test
{
    public class DynamicViewDataDictionaryTest
    {
        [Fact]
        public void Get_OnExistingProperty_ReturnsValue()
        {
            // Arrange
            ViewDataDictionary vd = new ViewDataDictionary()
            {
                { "Prop", "Value" }
            };
            dynamic dynamicVD = new DynamicViewDataDictionary(() => vd);

            // Act
            object value = dynamicVD.Prop;

            // Assert
            Assert.IsType<string>(value);
            Assert.Equal("Value", value);
        }

        [Fact]
        public void Get_OnNonExistentProperty_ReturnsNull()
        {
            // Arrange
            ViewDataDictionary vd = new ViewDataDictionary();
            dynamic dynamicVD = new DynamicViewDataDictionary(() => vd);

            // Act
            object value = dynamicVD.Prop;

            // Assert
            Assert.Null(value);
        }

        [Fact]
        public void Set_OnExistingProperty_OverridesValue()
        {
            // Arrange
            ViewDataDictionary vd = new ViewDataDictionary()
            {
                { "Prop", "Value" }
            };
            dynamic dynamicVD = new DynamicViewDataDictionary(() => vd);

            // Act
            dynamicVD.Prop = "NewValue";

            // Assert
            Assert.Equal("NewValue", dynamicVD.Prop);
            Assert.Equal("NewValue", vd["Prop"]);
        }

        [Fact]
        public void Set_OnNonExistentProperty_SetsValue()
        {
            // Arrange
            ViewDataDictionary vd = new ViewDataDictionary();
            dynamic dynamicVD = new DynamicViewDataDictionary(() => vd);

            // Act
            dynamicVD.Prop = "NewValue";

            // Assert
            Assert.Equal("NewValue", dynamicVD.Prop);
            Assert.Equal("NewValue", vd["Prop"]);
        }

        [Fact]
        public void TryGetMember_OnExistingProperty_ReturnsValueAndSucceeds()
        {
            // Arrange
            object result = null;
            ViewDataDictionary vd = new ViewDataDictionary()
            {
                { "Prop", "Value" }
            };
            DynamicViewDataDictionary dynamicVD = new DynamicViewDataDictionary(() => vd);
            Mock<GetMemberBinder> binderMock = new Mock<GetMemberBinder>("Prop", /* ignoreCase */ false);

            // Act
            bool success = dynamicVD.TryGetMember(binderMock.Object, out result);

            // Assert
            Assert.True(success);
            Assert.Equal("Value", result);
        }

        [Fact]
        public void TryGetMember_OnNonExistentProperty_ReturnsNullAndSucceeds()
        {
            // Arrange
            object result = null;
            ViewDataDictionary vd = new ViewDataDictionary();
            DynamicViewDataDictionary dynamicVD = new DynamicViewDataDictionary(() => vd);
            Mock<GetMemberBinder> binderMock = new Mock<GetMemberBinder>("Prop", /* ignoreCase */ false);

            // Act
            bool success = dynamicVD.TryGetMember(binderMock.Object, out result);

            // Assert
            Assert.True(success);
            Assert.Null(result);
        }

        [Fact]
        public void TrySetMember_OnExistingProperty_OverridesValueAndSucceeds()
        {
            // Arrange
            ViewDataDictionary vd = new ViewDataDictionary()
            {
                { "Prop", "Value" }
            };
            DynamicViewDataDictionary dynamicVD = new DynamicViewDataDictionary(() => vd);
            Mock<SetMemberBinder> binderMock = new Mock<SetMemberBinder>("Prop", /* ignoreCase */ false);

            // Act
            bool success = dynamicVD.TrySetMember(binderMock.Object, "NewValue");

            // Assert
            Assert.True(success);
            Assert.Equal("NewValue", ((dynamic)dynamicVD).Prop);
            Assert.Equal("NewValue", vd["Prop"]);
        }

        [Fact]
        public void TrySetMember_OnNonExistentProperty_SetsValueAndSucceeds()
        {
            // Arrange
            ViewDataDictionary vd = new ViewDataDictionary();
            DynamicViewDataDictionary dynamicVD = new DynamicViewDataDictionary(() => vd);
            Mock<SetMemberBinder> binderMock = new Mock<SetMemberBinder>("Prop", /* ignoreCase */ false);

            // Act
            bool success = dynamicVD.TrySetMember(binderMock.Object, "NewValue");

            // Assert
            Assert.True(success);
            Assert.Equal("NewValue", ((dynamic)dynamicVD).Prop);
            Assert.Equal("NewValue", vd["Prop"]);
        }

        [Fact]
        public void GetDynamicMemberNames_ReturnsEmptyListForEmptyViewDataDictionary()
        {
            // Arrange
            ViewDataDictionary vd = new ViewDataDictionary();
            DynamicViewDataDictionary dvd = new DynamicViewDataDictionary(() => vd);

            // Act
            IEnumerable<string> result = dvd.GetDynamicMemberNames();

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public void GetDynamicMemberNames_ReturnsKeyNamesForFilledViewDataDictionary()
        {
            // Arrange
            ViewDataDictionary vd = new ViewDataDictionary()
            {
                { "Prop1", 1 },
                { "Prop2", 2 }
            };
            DynamicViewDataDictionary dvd = new DynamicViewDataDictionary(() => vd);

            // Act
            var result = dvd.GetDynamicMemberNames();

            // Assert
            Assert.Equal(new[] { "Prop1", "Prop2" }, result.OrderBy(s => s).ToArray());
        }

        [Fact]
        public void GetDynamicMemberNames_ReflectsChangesToUnderlyingViewDataDictionary()
        {
            // Arrange
            ViewDataDictionary vd = new ViewDataDictionary();
            vd["OldProp"] = 123;
            DynamicViewDataDictionary dvd = new DynamicViewDataDictionary(() => vd);
            vd["NewProp"] = "somevalue";

            // Act
            var result = dvd.GetDynamicMemberNames();

            // Assert
            Assert.Equal(new[] { "NewProp", "OldProp" }, result.OrderBy(s => s).ToArray());
        }

        [Fact]
        public void GetDynamicMemberNames_ReflectsChangesToDynamicObject()
        {
            // Arrange
            ViewDataDictionary vd = new ViewDataDictionary();
            vd["OldProp"] = 123;
            DynamicViewDataDictionary dvd = new DynamicViewDataDictionary(() => vd);
            ((dynamic)dvd).NewProp = "foo";

            // Act
            var result = dvd.GetDynamicMemberNames();

            // Assert
            Assert.Equal(new[] { "NewProp", "OldProp" }, result.OrderBy(s => s).ToArray());
        }
    }
}
