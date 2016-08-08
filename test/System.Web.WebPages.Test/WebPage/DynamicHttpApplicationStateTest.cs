// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Web.WebPages.Resources;
using Microsoft.TestCommon;

namespace System.Web.WebPages.Test
{
    public class DynamicHttpApplicationStateTest
    {
        private static HttpApplicationStateBase CreateAppStateInstance()
        {
            return new HttpApplicationStateWrapper((HttpApplicationState)Activator.CreateInstance(typeof(HttpApplicationState), true));
        }

        [Fact]
        public void DynamicTest()
        {
            HttpApplicationStateBase appState = CreateAppStateInstance();
            dynamic d = new DynamicHttpApplicationState(appState);
            d["x"] = "y";
            Assert.Equal("y", d.x);
            Assert.Equal("y", d[0]);
            d.a = "b";
            Assert.Equal("b", d["a"]);
            d.Foo = "bar";
            Assert.Equal("bar", d.Foo);
            Assert.Null(d.XYZ);
            Assert.Null(d["xyz"]);
            Assert.Throws<ArgumentOutOfRangeException>(() => { var x = d[5]; });
            var a = d.Baz = 42;
            Assert.Equal(42, a);
            var b = d["test"] = 666;
            Assert.Equal(666, b);
        }

        [Fact]
        public void InvalidNumberOfIndexes()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                HttpApplicationStateBase appState = CreateAppStateInstance();
                dynamic d = new DynamicHttpApplicationState(appState);
                d[1, 2] = 3;
            }, WebPageResources.DynamicDictionary_InvalidNumberOfIndexes);

            Assert.Throws<ArgumentException>(() =>
            {
                HttpApplicationStateBase appState = CreateAppStateInstance();
                dynamic d = new DynamicHttpApplicationState(appState);
                var x = d[1, 2];
            }, WebPageResources.DynamicDictionary_InvalidNumberOfIndexes);
        }

        [Fact]
        public void InvalidTypeWhenSetting()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                HttpApplicationStateBase appState = CreateAppStateInstance();
                dynamic d = new DynamicHttpApplicationState(appState);
                d[new object()] = 3;
            }, WebPageResources.DynamicHttpApplicationState_UseOnlyStringToSet);
        }

        [Fact]
        public void InvalidTypeWhenGetting()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                HttpApplicationStateBase appState = CreateAppStateInstance();
                dynamic d = new DynamicHttpApplicationState(appState);
                var x = d[new object()];
            }, WebPageResources.DynamicHttpApplicationState_UseOnlyStringOrIntToGet);
        }
    }
}
