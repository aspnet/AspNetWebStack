// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Specialized;
using Microsoft.TestCommon;

namespace System.Net.Http.Headers
{
    public class CookieStateTest
    {
        public static TheoryDataSet<string> InvalidCookieNames
        {
            get
            {
                return new TheoryDataSet<string>
                {
                    "<acb>",
                    "{acb}",
                    "[acb]",
                    "\"acb\"",
                    "a,b",
                    "a;b",
                    "a\\b",
                };
            }
        }

        public static TheoryDataSet<string, string> EncodedCookieStateStrings
        {
            get
            {
                return new TheoryDataSet<string, string>
                {
                    { "?", "%3f" },
                    { "=", "%3d" },
                    { "<acb>", "%3cacb%3e" },
                    { "{acb}", "%7bacb%7d" },
                    { "[acb]", "%5bacb%5d" },
                    { "\"acb\"", "%22acb%22" },
                    { "a,b", "a%2cb" },
                    { "a;b", "a%3bb" },
                    { "a\\b", "a%5cb" },
                    { "[]{}\\|!@#$%^&*()_-+=", "%5b%5d%7b%7d%5c%7c!%40%23%24%25%5e%26*()_-%2b%3d" },
                };
            }
        }

        [Fact]
        public void CookieState_CtorThrowsOnNullName()
        {
            Assert.ThrowsArgumentNull(() => new CookieState(null, "value"), "name");
        }

        [Theory]
        [PropertyData("InvalidCookieNames")]
        public void CookieState_CtorThrowsOnInvalidName(string name)
        {
            Assert.ThrowsArgument(() => new CookieState(name, "value"), "name");
        }

        [Fact]
        public void CookieState_CtorThrowsOnNullNameValueCollection()
        {
            Assert.ThrowsArgumentNull(() => new CookieState("name", (NameValueCollection)null), "values");
        }

        [Theory]
        [InlineData("name", "")]
        [InlineData("name", "value")]
        [InlineData("name", "+=\\[]{}!@#$%^&*()_")]
        public void CookieState_Ctor1InitializesCorrectly(string name, string value)
        {
            CookieState cookie = new CookieState(name, value);
            Assert.Equal(name, cookie.Name);
            Assert.Equal(value, cookie.Values.AllKeys[0]);
            Assert.Equal(value, cookie.Value);
        }

        [Fact]
        public void CookieState_Ctor2InitializesCorrectly()
        {
            // Arrange
            NameValueCollection nvc = new NameValueCollection();
            nvc.Add("n1", "v1");

            // Act
            CookieState cookie = new CookieState("name", nvc);

            // Assert
            Assert.Equal("name", cookie.Name);
            Assert.Single(cookie.Values);
            Assert.Equal("n1", cookie.Values.AllKeys[0]);
            Assert.Equal("v1", cookie.Values["n1"]);
            Assert.Equal("n1", cookie.Value);
        }

        [Fact]
        public void CookieState_Value()
        {
            CookieState cookie = new CookieState("name");
            Assert.Equal(String.Empty, cookie.Value);

            cookie.Value = "value1";
            Assert.Equal("value1", cookie.Value);

            cookie.Values.AllKeys[0] = "value2";
            Assert.Equal("value2", cookie.Value);
        }

        [Fact]
        public void CookieState_ItemTreatsNullNameAsEmpty()
        {
            // Arrange
            CookieState state = new CookieState("name", "value");

            // Act
            state[null] = "v1";

            // Assert
            Assert.Equal("name=value&=v1", state.ToString());
        }

        [Theory]
        [PropertyData("EncodedCookieStateStrings")]
        public void CookieState_ItemEncodesName(string subname, string encodedSubname)
        {
            // Arrange
            CookieState state = new CookieState("name", "value");

            // Act
            state[subname] = "v1";

            // Assert
            string value = String.Format("name=value&{0}=v1", encodedSubname);
            Assert.Equal(value, state.ToString());
        }

        [Theory]
        [PropertyData("EncodedCookieStateStrings")]
        public void CookieState_ItemEncodesValue(string subvalue, string encodedSubvalue)
        {
            // Arrange
            CookieState state = new CookieState("name", "value");

            // Act
            state["n1"] = subvalue;

            // Assert
            string value = String.Format("name=value&n1={0}", encodedSubvalue);
            Assert.Equal(value, state.ToString());
        }

        [Fact]
        public void CookieState_Clone()
        {
            // Arrange
            NameValueCollection nvc = new NameValueCollection();
            nvc.Add("n1", "v1");
            CookieState expectedValue = new CookieState("name", nvc);

            // Act
            CookieState actualValue = expectedValue.Clone() as CookieState;

            // Assert
            Assert.Equal("name", actualValue.Name);
            Assert.Single(actualValue.Values);
            Assert.Equal("n1", actualValue.Values.AllKeys[0]);
            Assert.Equal("v1", actualValue.Values["n1"]);
        }

        [Theory]
        [PropertyData("EncodedCookieStateStrings")]
        public void CookieState_ToStringWithSingleValue(string subValue, string encodedSubvalue)
        {
            // Arrange
            CookieState cookie = new CookieState("name", subValue);

            // Act
            string actualValue = cookie.ToString();

            // Assert
            string expectedValue = String.Format("name={0}", encodedSubvalue);
            Assert.Equal(expectedValue, actualValue);
        }

        [Theory]
        [PropertyData("EncodedCookieStateStrings")]
        public void CookieState_ToStringWithNameValueCollection(string subValue, string encodedSubvalue)
        {
            // Arrange
            NameValueCollection nvc = new NameValueCollection();
            nvc.Add("n1", subValue);
            nvc.Add("n2", subValue);
            nvc.Add("n3", subValue);
            CookieState cookie = new CookieState("name", nvc);

            // Act
            string actualValue = cookie.ToString();

            // Assert
            string expectedValue = String.Format("name=n1={0}&n2={0}&n3={0}", encodedSubvalue);
            Assert.Equal(expectedValue, actualValue);
        }
    }
}
