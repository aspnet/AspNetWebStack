// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net.Http.Headers;
using Microsoft.TestCommon;
using ROOT_PROJECT_NAMESPACE.Areas.HelpPage;

namespace WebApiHelpPageWebHost.UnitTest
{
    public class HelpPageSampleKeyTest
    {
        [Fact]
        public void Constructor_TwoParameters()
        {
            HelpPageSampleKey key = new HelpPageSampleKey(new MediaTypeHeaderValue("application/xml"), typeof(Tuple<int, string>));
            Assert.Equal(new MediaTypeHeaderValue("application/xml"), key.MediaType);
            Assert.Equal(typeof(Tuple<int, string>), key.ParameterType);
            Assert.Null(key.SampleDirection);
            Assert.Equal(String.Empty, key.ControllerName);
            Assert.Equal(String.Empty, key.ActionName);
            Assert.Empty(key.ParameterNames);
        }

        [Fact]
        public void Constructor_FourParameters()
        {
            HelpPageSampleKey key = new HelpPageSampleKey(SampleDirection.Request, "myController", "myAction", new[] { "id", "name" });
            Assert.Null(key.MediaType);
            Assert.Null(key.ParameterType);
            Assert.Equal(SampleDirection.Request, key.SampleDirection.Value);
            Assert.Equal("myController", key.ControllerName);
            Assert.Equal("myAction", key.ActionName);
            Assert.NotEmpty(key.ParameterNames);
            Assert.True(key.ParameterNames.SetEquals(new[] { "name", "id" }));
        }

        [Fact]
        public void Constructor_FiveParameters()
        {
            HelpPageSampleKey key = new HelpPageSampleKey(new MediaTypeHeaderValue("application/xml"), SampleDirection.Request, "myController", "myAction", new[] { "id", "name" });
            Assert.Equal(new MediaTypeHeaderValue("application/xml"), key.MediaType);
            Assert.Null(key.ParameterType);
            Assert.Equal(SampleDirection.Request, key.SampleDirection.Value);
            Assert.Equal("myController", key.ControllerName);
            Assert.Equal("myAction", key.ActionName);
            Assert.NotEmpty(key.ParameterNames);
            Assert.True(key.ParameterNames.SetEquals(new[] { "name", "id" }));
        }

        public static IEnumerable<object[]> Constructor_ThrowsArgumentNullException_PropertyData
        {
            get
            {
                yield return new Func<object>[] { () => new HelpPageSampleKey(null, typeof(Tuple<int, string>)) };
                yield return new Func<object>[] { () => new HelpPageSampleKey(new MediaTypeHeaderValue("application/xml"), null) };

                yield return new Func<object>[] { () => new HelpPageSampleKey(SampleDirection.Request, "c", "a", null) };
                yield return new Func<object>[] { () => new HelpPageSampleKey(SampleDirection.Request, null, "a", new string[0]) };
                yield return new Func<object>[] { () => new HelpPageSampleKey(SampleDirection.Request, "c", null, new string[0]) };

                yield return new Func<object>[] { () => new HelpPageSampleKey(null, SampleDirection.Request, "c", "a", new string[0]) };
                yield return new Func<object>[] { () => new HelpPageSampleKey(new MediaTypeHeaderValue("application/xml"), SampleDirection.Request, "c", "a", null) };
                yield return new Func<object>[] { () => new HelpPageSampleKey(new MediaTypeHeaderValue("application/xml"), SampleDirection.Request, null, "a", new string[0]) };
                yield return new Func<object>[] { () => new HelpPageSampleKey(new MediaTypeHeaderValue("application/xml"), SampleDirection.Request, "c", null, new string[0]) };
            }
        }

        [Theory]
        [PropertyData("Constructor_ThrowsArgumentNullException_PropertyData")]
        public void Constructor_ThrowsArgumentNullException(Func<object> constructorDelegate)
        {
            Assert.Throws(typeof(ArgumentNullException), constructorDelegate);
        }

        public static IEnumerable<object[]> Constructor_ThrowsArgumentException_PropertyData
        {
            get
            {
                yield return new Func<object>[] { () => new HelpPageSampleKey(SampleDirection.Request, null, "a", new string[0])};
                yield return new Func<object>[] { () => new HelpPageSampleKey(SampleDirection.Request, "c", null, new string[0]) };

                yield return new Func<object>[] { () => new HelpPageSampleKey(new MediaTypeHeaderValue("application/xml"), SampleDirection.Request, null, "a", new string[0]) };
                yield return new Func<object>[] { () => new HelpPageSampleKey(new MediaTypeHeaderValue("application/xml"), SampleDirection.Request, "c", null, new string[0]) };
            }
        }

        public static IEnumerable<object[]> Constructor_ThrowsInvalidEnumArgumentException_PropertyData
        {
            get
            {
                yield return new Func<object>[] { () => new HelpPageSampleKey((SampleDirection)10, "c", "a", new string[0]) };
                yield return new Func<object>[] { () => new HelpPageSampleKey(new MediaTypeHeaderValue("application/xml"), (SampleDirection)9, "c", "a", new string[0]) };
            }
        }

        [Theory]
        [PropertyData("Constructor_ThrowsInvalidEnumArgumentException_PropertyData")]
        public void Constructor_ThrowsInvalidEnumArgumentException(Func<object> constructorDelegate)
        {
            Assert.Throws(typeof(InvalidEnumArgumentException), constructorDelegate);
        }

        public static IEnumerable<object[]> Equals_ReturnsTrue_PropertyData
        {
            get
            {
                HelpPageSampleKey key1 = new HelpPageSampleKey(SampleDirection.Request, "myController", "myAction", new[] { "id", "name" });
                HelpPageSampleKey key2 = new HelpPageSampleKey(SampleDirection.Request, "MyController", "myAction", new[] { "ID", "name" });
                yield return new object[] { key1, key2 };

                key1 = new HelpPageSampleKey(SampleDirection.Request, "myController", "myAction", new[] { "id", "name" });
                key2 = new HelpPageSampleKey((SampleDirection)0, "MyController", "myAction", new[] { "ID", "name" });
                yield return new object[] { key1, key2 };

                key1 = new HelpPageSampleKey(new MediaTypeHeaderValue("text/custom"), typeof(Tuple<int, string>));
                key2 = new HelpPageSampleKey(new MediaTypeHeaderValue("TEXT/custom"), typeof(Tuple<int, string>));
                yield return new object[] { key1, key2 };

                key1 = new HelpPageSampleKey(new MediaTypeHeaderValue("text/custom"), SampleDirection.Request, "controller", "myAction", new string[0]);
                key2 = new HelpPageSampleKey(new MediaTypeHeaderValue("text/custom"), SampleDirection.Request, "CONTROLLER", "MYACTION", new string[0]);
                yield return new object[] { key1, key2 };

                key1 = new HelpPageSampleKey(new MediaTypeHeaderValue("text/custom"), SampleDirection.Response, "controller", "myAction", new[] { "ID", "NAME" });
                key2 = new HelpPageSampleKey(new MediaTypeHeaderValue("text/custom"), SampleDirection.Response, "CONTROLLER", "MYACTION", new[] { "id", "name" });
                yield return new object[] { key1, key2 };

                key1 = new HelpPageSampleKey(new MediaTypeHeaderValue("text/custom"), SampleDirection.Request, "controller", "action", new string[0]);
                key2 = new HelpPageSampleKey(new MediaTypeHeaderValue("text/custom"), SampleDirection.Request, "controller", "action", new string[0]);
                yield return new object[] { key1, key2 };

                key1 = new HelpPageSampleKey(new MediaTypeHeaderValue("text/custom"), SampleDirection.Response, "controller", "action", new string[0]);
                key2 = new HelpPageSampleKey(new MediaTypeHeaderValue("text/custom"), (SampleDirection)1, "controller", "action", new string[0]);
                yield return new object[] { key1, key2 };
            }
        }

        [Theory]
        [PropertyData("Equals_ReturnsTrue_PropertyData")]
        public void Equals_ReturnsTrue(HelpPageSampleKey key1, HelpPageSampleKey key2)
        {
            Assert.True(key1.Equals(key2));
        }

        public static IEnumerable<object[]> Equals_ReturnsFalse_PropertyData
        {
            get
            {
                HelpPageSampleKey key1 = new HelpPageSampleKey(SampleDirection.Request, "myController", "myAction", new[] { "id", "name" });
                HelpPageSampleKey key2 = new HelpPageSampleKey(SampleDirection.Request, "MyController", "myAction", new string[0]);
                yield return new object[] { key1, key2 };

                key1 = new HelpPageSampleKey(new MediaTypeHeaderValue("text/custom"), typeof(Tuple<int, int>));
                key2 = new HelpPageSampleKey(new MediaTypeHeaderValue("TEXT/custom"), typeof(Tuple<int, string>));
                yield return new object[] { key1, key2 };

                key1 = new HelpPageSampleKey(new MediaTypeHeaderValue("text/custom"), SampleDirection.Request, "controller", "action", new string[0]);
                key2 = new HelpPageSampleKey(new MediaTypeHeaderValue("TEXT/custom"), SampleDirection.Response, "controller", "action", new string[0]);
                yield return new object[] { key1, key2 };

                key1 = new HelpPageSampleKey(new MediaTypeHeaderValue("text/custom"), (SampleDirection)0, "controller", "action", new string[0]);
                key2 = new HelpPageSampleKey(new MediaTypeHeaderValue("TEXT/custom"), (SampleDirection)1, "controller", "action", new string[0]);
                yield return new object[] { key1, key2 };

                key1 = new HelpPageSampleKey(new MediaTypeHeaderValue("text/custom"), SampleDirection.Request, "controller", "action", new string[] { "" });
                key2 = new HelpPageSampleKey(new MediaTypeHeaderValue("text/custom"), SampleDirection.Request, "controller", "action", new string[0]);
                yield return new object[] { key1, key2 };

                key1 = new HelpPageSampleKey(new MediaTypeHeaderValue("text/custom"), typeof(Tuple<int, int>));
                key2 = new HelpPageSampleKey(new MediaTypeHeaderValue("text/custom"), SampleDirection.Request, "controller", "action", new string[0]);
                yield return new object[] { key1, key2 };
            }
        }

        [Theory]
        [PropertyData("Equals_ReturnsFalse_PropertyData")]
        public void Equals_ReturnsFalse(HelpPageSampleKey key1, HelpPageSampleKey key2)
        {
            Assert.False(key1.Equals(key2));
        }

        [Theory]
        [PropertyData("Equals_ReturnsTrue_PropertyData")]
        public void GetHashCode_ReturnsSame(HelpPageSampleKey key1, HelpPageSampleKey key2)
        {
            Assert.Equal(key1.GetHashCode(), key2.GetHashCode());
        }

        [Theory]
        [PropertyData("Equals_ReturnsFalse_PropertyData")]
        public void GetHashCode_ReturnsDifferent(HelpPageSampleKey key1, HelpPageSampleKey key2)
        {
            Assert.NotEqual(key1.GetHashCode(), key2.GetHashCode());
        }
    }
}
