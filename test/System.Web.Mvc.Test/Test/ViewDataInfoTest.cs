// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.TestCommon;

namespace System.Web.Mvc.Test
{
    public class ViewDataInfoTest
    {
        [Fact]
        public void ViewDataInfoDoesNotCallAccessorUntilValuePropertyAccessed()
        {
            // Arrange
            bool called = false;
            ViewDataInfo vdi = new ViewDataInfo(() =>
            {
                called = true;
                return 21;
            });

            // Act & Assert
            Assert.False(called);
            object result = vdi.Value;
            Assert.True(called);
            Assert.Equal(21, result);
        }

        [Fact]
        public void AccessorIsOnlyCalledOnce()
        {
            // Arrange
            int callCount = 0;
            ViewDataInfo vdi = new ViewDataInfo(() =>
            {
                ++callCount;
                return null;
            });

            // Act & Assert
            Assert.Equal(0, callCount);
            object unused;
            unused = vdi.Value;
            unused = vdi.Value;
            unused = vdi.Value;
            Assert.Equal(1, callCount);
        }

        [Fact]
        public void SettingExplicitValueOverridesAccessorMethod()
        {
            // Arrange
            bool called = false;
            ViewDataInfo vdi = new ViewDataInfo(() =>
            {
                called = true;
                return null;
            });

            // Act & Assert
            Assert.False(called);
            vdi.Value = 42;
            object result = vdi.Value;
            Assert.False(called);
            Assert.Equal(42, result);
        }
    }
}
