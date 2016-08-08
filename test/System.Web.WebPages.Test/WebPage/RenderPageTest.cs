// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Web.WebPages.Resources;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.WebPages.Test
{
    public class RenderPageTest
    {
        [Fact]
        public void RenderBasicTest()
        {
            // A simple page that does the following:
            // @{ PageData["Title"] = "MyPage"; }
            // @PageData["Title"]
            // hello world
            //
            // Expected rendered result is "MyPagehello world"

            var content = "hello world";
            var title = "MyPage";
            var result = Utils.RenderWebPage(
                p =>
                {
                    p.PageData["Title"] = title;
                    p.Write(p.PageData["Title"]);
                    p.Write(content);
                });

            Assert.Equal(title + content, result);
        }

        [Fact]
        public void RenderDynamicDictionaryBasicTest()
        {
            // A simple page that does the following:
            // @{ Page.Title = "MyPage"; }
            // @Page.Title
            // hello world
            //
            // Expected rendered result is "MyPagehello world"

            var content = "hello world";
            var title = "MyPage";
            var result = Utils.RenderWebPage(
                p =>
                {
                    p.Page.Title = title;
                    p.Write(p.Page.Title);
                    p.Write(content);
                });

            Assert.Equal(title + content, result);
        }

        [Fact]
        public void RenderPageBasicTest()
        {
            // ~/index.cshtml does the following:
            // hello
            // @RenderPage("subpage.cshtml")
            //
            // ~/subpage.cshtml does the following:
            // world
            //
            // Expected output is "helloworld"

            var result = Utils.RenderWebPageWithSubPage(
                p =>
                {
                    p.Write("hello");
                    p.Write(p.RenderPage("subpage.cshtml"));
                },
                p => { p.Write("world"); });
            Assert.Equal("helloworld", result);
        }

        [Fact]
        public void RenderPageAnonymousTypeTest()
        {
            // Test for passing an anonymous type object as an argument to RenderPage
            //
            // ~/index.cshtml does the following:
            // @RenderPage("subpage.cshtml", new { HelloKey = "hellovalue", MyKey = "myvalue" })
            //
            // ~/subpage.cshtml does the following:
            // @PageData["HelloKey"] @PageData["MyKey"] @Model.HelloKey @Model.MyKey
            //
            // Expected result: hellovalue myvalue hellovalue myvalue
            var result = Utils.RenderWebPageWithSubPage(
                p => { p.Write(p.RenderPage("subpage.cshtml", new { HelloKey = "hellovalue", MyKey = "myvalue" })); },
                p =>
                {
                    p.Write(p.PageData["HelloKey"]);
                    p.Write(" ");
                    p.Write(p.PageData["MyKey"]);
                    p.Write(" ");
                    p.Write(p.Model.HelloKey);
                    p.Write(" ");
                    p.Write(p.Model.MyKey);
                });
            Assert.Equal("hellovalue myvalue hellovalue myvalue", result);
        }

        [Fact]
        public void RenderPageDynamicDictionaryAnonymousTypeTest()
        {
            // Test for passing an anonymous type object as an argument to RenderPage
            //
            // ~/index.cshtml does the following:
            // @RenderPage("subpage.cshtml", new { HelloKey = "hellovalue", MyKey = "myvalue" })
            //
            // ~/subpage.cshtml does the following:
            // @Page.HelloKey @Page.MyKey @Model.HelloKey @Model.MyKey
            //
            // Expected result: hellovalue myvalue hellovalue myvalue
            var result = Utils.RenderWebPageWithSubPage(
                p => { p.Write(p.RenderPage("subpage.cshtml", new { HelloKey = "hellovalue", MyKey = "myvalue" })); },
                p =>
                {
                    p.Write(p.Page.HelloKey);
                    p.Write(" ");
                    p.Write(p.Page.MyKey);
                    p.Write(" ");
                    p.Write(p.Model.HelloKey);
                    p.Write(" ");
                    p.Write(p.Model.MyKey);
                });
            Assert.Equal("hellovalue myvalue hellovalue myvalue", result);
        }

        [Fact]
        public void RenderPageDictionaryTest()
        {
            // Test for passing a dictionary instance as an argument to RenderPage
            //
            // ~/index.cshtml does the following:
            // @RenderPage("subpage.cshtml", new Dictionary<string, object>(){ { "foo", 1 }, { "bar", "hello"} })
            //
            // ~/subpage.cshtml does the following:
            // @PageData["foo"] @PageData["bar"] @PageData[0]
            //
            // Expected result: 1 hello System.Collections.Generic.Dictionary`2[System.String,System.Object]

            var result = Utils.RenderWebPageWithSubPage(
                p => { p.Write(p.RenderPage("subpage.cshtml", new Dictionary<string, object>() { { "foo", 1 }, { "bar", "hello" } })); },
                p =>
                {
                    p.Write(p.PageData["foo"]);
                    p.Write(" ");
                    p.Write(p.PageData["bar"]);
                    p.Write(" ");
                    p.Write(p.PageData[0]);
                });
            Assert.Equal("1 hello System.Collections.Generic.Dictionary`2[System.String,System.Object]", result);
        }

        [Fact]
        public void RenderPageDynamicDictionaryTest()
        {
            // Test for passing a dictionary instance as an argument to RenderPage
            //
            // ~/index.cshtml does the following:
            // @RenderPage("subpage.cshtml", new Dictionary<string, object>(){ { "foo", 1 }, { "bar", "hello"} })
            //
            // ~/subpage.cshtml does the following:
            // @Page.foo @Page.bar @Page[0]
            //
            // Expected result: 1 hello System.Collections.Generic.Dictionary`2[System.String,System.Object]

            var result = Utils.RenderWebPageWithSubPage(
                p => { p.Write(p.RenderPage("subpage.cshtml", new Dictionary<string, object>() { { "foo", 1 }, { "bar", "hello" } })); },
                p =>
                {
                    p.Write(p.Page.foo);
                    p.Write(" ");
                    p.Write(p.Page.bar);
                    p.Write(" ");
                    p.Write(p.Page[0]);
                });
            Assert.Equal("1 hello System.Collections.Generic.Dictionary`2[System.String,System.Object]", result);
        }

        [Fact]
        public void RenderPageListTest()
        {
            // Test for passing a list of arguments to RenderPage
            //
            // ~/index.cshtml does the following:
            // @RenderPage("subpage.cshtml", "hello", "world", 1, 2, 3)
            //
            // ~/subpage.cshtml does the following:
            // @PageData[0] @PageData[1] @PageData[2] @PageData[3] @PageData[4]
            //
            // Expected result: hello world 1 2 3

            var result = Utils.RenderWebPageWithSubPage(
                p => { p.Write(p.RenderPage("subpage.cshtml", "hello", "world", 1, 2, 3)); },
                p =>
                {
                    p.Write(p.PageData[0]);
                    p.Write(" ");
                    p.Write(p.PageData[1]);
                    p.Write(" ");
                    p.Write(p.PageData[2]);
                    p.Write(" ");
                    p.Write(p.PageData[3]);
                    p.Write(" ");
                    p.Write(p.PageData[4]);
                });
            Assert.Equal("hello world 1 2 3", result);
        }

        [Fact]
        public void RenderPageDynamicDictionaryListTest()
        {
            // Test for passing a list of arguments to RenderPage
            //
            // ~/index.cshtml does the following:
            // @RenderPage("subpage.cshtml", "hello", "world", 1, 2, 3)
            //
            // ~/subpage.cshtml does the following:
            // @Page[0] @Page[1] @Page[2] @Page[3] @Page[4]
            //
            // Expected result: hello world 1 2 3

            var result = Utils.RenderWebPageWithSubPage(
                p => { p.Write(p.RenderPage("subpage.cshtml", "hello", "world", 1, 2, 3)); },
                p =>
                {
                    p.Write(p.Page[0]);
                    p.Write(" ");
                    p.Write(p.Page[1]);
                    p.Write(" ");
                    p.Write(p.Page[2]);
                    p.Write(" ");
                    p.Write(p.Page[3]);
                    p.Write(" ");
                    p.Write(p.Page[4]);
                });
            Assert.Equal("hello world 1 2 3", result);
        }

        private class Person
        {
            public string FirstName { get; set; }
        }

        [Fact]
        public void RenderPageDynamicValueTest()
        {
            // Test that PageData[key] returns a dynamic value.
            // ~/index.cshtml does the following:
            // @RenderPage("subpage.cshtml", new Person(){ FirstName="MyFirstName" })
            //
            // ~/subpage.cshtml does the following:
            // @PageData[0].FirstName
            //
            // Expected result: MyFirstName
            var result = Utils.RenderWebPageWithSubPage(
                p => { p.Write(p.RenderPage("subpage.cshtml", new Person() { FirstName = "MyFirstName" })); },
                p => { p.Write(p.PageData[0].FirstName); });
            Assert.Equal("MyFirstName", result);
        }

        [Fact]
        public void RenderPageDynamicDictionaryDynamicValueTest()
        {
            // Test that PageData[key] returns a dynamic value.
            // ~/index.cshtml does the following:
            // @RenderPage("subpage.cshtml", new Person(){ FirstName="MyFirstName" })
            //
            // ~/subpage.cshtml does the following:
            // @Page[0].FirstName
            //
            // Expected result: MyFirstName
            var result = Utils.RenderWebPageWithSubPage(
                p => { p.Write(p.RenderPage("subpage.cshtml", new Person() { FirstName = "MyFirstName" })); },
                p => { p.Write(p.Page[0].FirstName); });
            Assert.Equal("MyFirstName", result);
        }

        [Fact]
        public void PageDataSetByParentTest()
        {
            // Items set in the PageData should be accessible by the subpage
            var result = Utils.RenderWebPageWithSubPage(
                p =>
                {
                    p.PageData["test"] = "hello";
                    p.Write(p.RenderPage("subpage.cshtml"));
                },
                p => { p.Write(p.PageData["test"]); });
            Assert.Equal("hello", result);
        }

        [Fact]
        public void DynamicDictionarySetByParentTest()
        {
            // Items set in the PageData should be accessible by the subpage
            var result = Utils.RenderWebPageWithSubPage(
                p =>
                {
                    p.Page.test = "hello";
                    p.Write(p.RenderPage("subpage.cshtml"));
                },
                p => { p.Write(p.Page.test); });
            Assert.Equal("hello", result);
        }

        [Fact]
        public void OverridePageDataSetByParentTest()
        {
            // Items set in the PageData should be accessible by the subpage unless
            // overriden by parameters passed into RenderPage, in which case the 
            // specified value should be used.
            var result = Utils.RenderWebPageWithSubPage(
                p =>
                {
                    p.PageData["test"] = "hello";
                    p.Write(p.RenderPage("subpage.cshtml", new { Test = "world" }));
                },
                p =>
                {
                    p.Write(p.PageData["test"]);
                    p.Write(p.PageData[0].Test);
                });
            Assert.Equal("worldworld", result);
        }

        [Fact]
        public void OverrideDynamicDictionarySetByParentTest()
        {
            // Items set in the PageData should be accessible by the subpage unless
            // overriden by parameters passed into RenderPage, in which case the 
            // specified value should be used.
            var result = Utils.RenderWebPageWithSubPage(
                p =>
                {
                    p.PageData["test"] = "hello";
                    p.Write(p.RenderPage("subpage.cshtml", new { Test = "world" }));
                },
                p =>
                {
                    p.Write(p.Page.test);
                    p.Write(p.Page[0].Test);
                });
            Assert.Equal("worldworld", result);
        }

        [Fact]
        public void RenderPageMissingKeyTest()
        {
            // Test that using PageData with a missing key returns null
            //
            // ~/index.cshtml does the following:
            // @RenderPage("subpage.cshtml", new Dictionary<string, object>(){ { "foo", 1 }, { "bar", "hello"} })
            // @RenderPage("subpage.cshtml", "x", "y", "z")
            //
            // ~/subpage.cshtml does the following:
            // @(PageData[1] ?? "null")
            // @(PageData["bar"] ?? "null")
            //
            // Expected result: null hello y null

            var result = Utils.RenderWebPageWithSubPage(
                p =>
                {
                    p.Write(p.RenderPage("subpage.cshtml", new Dictionary<string, object>() { { "foo", 1 }, { "bar", "hello" } }));
                    p.Write(p.RenderPage("subpage.cshtml", "x", "y", "z"));
                },
                p =>
                {
                    p.Write(p.PageData[1] ?? "null1");
                    p.Write(" ");
                    p.Write(p.PageData["bar"] ?? "null2");
                    p.Write(" ");
                });
            Assert.Equal("null1 hello y null2 ", result);
        }

        [Fact]
        public void RenderPageDynamicDictionaryMissingKeyTest()
        {
            // Test that using PageData with a missing key returns null
            //
            // ~/index.cshtml does the following:
            // @RenderPage("subpage.cshtml", new Dictionary<string, object>(){ { "foo", 1 }, { "bar", "hello"} })
            // @RenderPage("subpage.cshtml", "x", "y", "z")
            //
            // ~/subpage.cshtml does the following:
            // @(Page[1] ?? "null")
            // @(Page.bar ?? "null")
            //
            // Expected result: null hello y null

            Action<WebPage> subPage = p =>
            {
                p.Write(p.Page[1] ?? "null1");
                p.Write(" ");
                p.Write(p.Page.bar ?? "null2");
                p.Write(" ");
            };
            var result = Utils.RenderWebPageWithSubPage(
                p => { p.Write(p.RenderPage("subpage.cshtml", new Dictionary<string, object>() { { "foo", 1 }, { "bar", "hello" } })); }, subPage);
            Assert.Equal("null1 hello ", result);
            result = Utils.RenderWebPageWithSubPage(
                p => { p.Write(p.RenderPage("subpage.cshtml", "x", "y", "z")); }, subPage);
            Assert.Equal("y null2 ", result);
        }

        [Fact]
        public void RenderPageNoArgumentsTest()
        {
            // Test that using PageData within the calling page, and also 
            // within the subppage when the calling page doesn't provide any arguments
            //
            // ~/index.cshtml does the following:
            // @(PageData["foo"] ?? "null1")
            // @RenderPage("subpage.cshtml")
            //
            // ~/subpage.cshtml does the following:
            // @(PageData[1] ?? "null2")
            // @(PageData["bar"] ?? "null3")
            //
            // Expected result: null1 null2 null3

            var result = Utils.RenderWebPageWithSubPage(
                p =>
                {
                    p.Write(p.PageData["foo"] ?? "null1 ");
                    p.Write(p.RenderPage("subpage.cshtml"));
                },
                p =>
                {
                    p.Write(p.PageData[1] ?? "null2");
                    p.Write(" ");
                    p.Write(p.PageData["bar"] ?? "null3");
                });
            Assert.Equal("null1 null2 null3", result);
        }

        [Fact]
        public void RenderPageDynamicDictionaryNoArgumentsTest()
        {
            // Test that using PageData within the calling page, and also 
            // within the subppage when the calling page doesn't provide any arguments
            //
            // ~/index.cshtml does the following:
            // @(Page.foo ?? "null1")
            // @RenderPage("subpage.cshtml")
            //
            // ~/subpage.cshtml does the following:
            // @(Page[1] ?? "null2")
            // @(Page.bar ?? "null3")
            //
            // Expected result: null1 null2 null3

            var result = Utils.RenderWebPageWithSubPage(
                p =>
                {
                    p.Write(p.Page.foo ?? "null1 ");
                    p.Write(p.RenderPage("subpage.cshtml"));
                },
                p =>
                {
                    p.Write(p.Page[1] ?? "null2");
                    p.Write(" ");
                    p.Write(p.Page.bar ?? "null3");
                });
            Assert.Equal("null1 null2 null3", result);
        }

        [Fact]
        public void RenderPageNestedSubPageListTest()
        {
            // Test that PageData for each level of nesting returns the values as specified in the 
            // previous calling page.
            //
            // ~/index.cshtml does the following:
            // @(PageData["foo"] ?? "null")
            // @RenderPage("subpage1.cshtml", "a", "b", "c")
            //
            // ~/subpage1.cshtml does the following:
            // @(PageData[0] ?? "sub1null0")
            // @(PageData[1] ?? "sub1null1")
            // @(PageData[2] ?? "sub1null2")
            // @(PageData[3] ?? "sub1null3")
            // @RenderPage("subpage2.cshtml", "x", "y", "z")
            //
            // ~/subpage2.cshtml does the following:
            // @(PageData[0] ?? "sub2null0")
            // @(PageData[1] ?? "sub2null1")
            // @(PageData[2] ?? "sub2null2")
            // @(PageData[3] ?? "sub2null3")
            //
            // Expected result: null a b c sub1null3 x y z sub2null3
            var page = Utils.CreatePage(
                p =>
                {
                    p.Write(p.PageData["foo"] ?? "null ");
                    p.Write(p.RenderPage("subpage1.cshtml", "a", "b", "c"));
                });
            var subpage1Path = "~/subpage1.cshtml";
            var subpage1 = Utils.CreatePage(
                p =>
                {
                    p.Write(p.PageData[0] ?? "sub1null0");
                    p.Write(" ");
                    p.Write(p.PageData[1] ?? "sub1null1");
                    p.Write(" ");
                    p.Write(p.PageData[2] ?? "sub1null2");
                    p.Write(" ");
                    p.Write(p.PageData[3] ?? "sub1null3");
                    p.Write(" ");
                    p.Write(p.RenderPage("subpage2.cshtml", "x", "y", "z"));
                }, subpage1Path);
            var subpage2Path = "~/subpage2.cshtml";
            var subpage2 = Utils.CreatePage(
                p =>
                {
                    p.Write(p.PageData[0] ?? "sub2null0");
                    p.Write(" ");
                    p.Write(p.PageData[1] ?? "sub2null1");
                    p.Write(" ");
                    p.Write(p.PageData[2] ?? "sub2null2");
                    p.Write(" ");
                    p.Write(p.PageData[3] ?? "sub2null3");
                }, subpage2Path);

            Utils.AssignObjectFactoriesAndDisplayModeProvider(page, subpage1, subpage2);

            var result = Utils.RenderWebPage(page);
            Assert.Equal("null a b c sub1null3 x y z sub2null3", result);
        }

        [Fact]
        public void RenderPageNestedSubPageAnonymousTypeTest()
        {
            // Test that PageData for each level of nesting returns the values as specified in the 
            // previous calling page.
            //
            // ~/index.cshtml does the following:
            // @(PageData["foo"] ?? "null")
            // @RenderPage("subpage.cshtml", new { foo = 1 , bar = "hello" })
            //
            // ~/subpage1.cshtml does the following:
            // @(PageData["foo"] ?? "sub1nullfoo")
            // @(PageData["bar"] ?? "sub1nullbar")
            // @(PageData["x"] ?? "sub1nullx")
            // @(PageData["y"] ?? "sub1nully")
            // @RenderPage("subpage2.cshtml", new { bar = "world", x = "good", y = "bye"})
            //
            // ~/subpage2.cshtml does the following:
            // @(PageData["foo"] ?? "sub2nullfoo")
            // @(PageData["bar"] ?? "sub2nullbar")
            // @(PageData["x"] ?? "sub2nullx")
            // @(PageData["y"] ?? "sub2nully")
            //
            // Expected result: null 1 hello sub1nullx sub1nully sub2nullfoo world good bye
            var page = Utils.CreatePage(
                p =>
                {
                    p.Write(p.PageData["foo"] ?? "null ");
                    p.Write(p.RenderPage("subpage1.cshtml", new { foo = 1, bar = "hello" }));
                });
            var subpage1Path = "~/subpage1.cshtml";
            var subpage1 = Utils.CreatePage(
                p =>
                {
                    p.Write(p.PageData["foo"] ?? "sub1nullfoo");
                    p.Write(" ");
                    p.Write(p.PageData["bar"] ?? "sub1nullbar");
                    p.Write(" ");
                    p.Write(p.PageData["x"] ?? "sub1nullx");
                    p.Write(" ");
                    p.Write(p.PageData["y"] ?? "sub1nully");
                    p.Write(" ");
                    p.Write(p.RenderPage("subpage2.cshtml", new { bar = "world", x = "good", y = "bye" }));
                }, subpage1Path);
            var subpage2Path = "~/subpage2.cshtml";
            var subpage2 = Utils.CreatePage(
                p =>
                {
                    p.Write(p.PageData["foo"] ?? "sub2nullfoo");
                    p.Write(" ");
                    p.Write(p.PageData["bar"] ?? "sub2nullbar");
                    p.Write(" ");
                    p.Write(p.PageData["x"] ?? "sub2nullx");
                    p.Write(" ");
                    p.Write(p.PageData["y"] ?? "sub2nully");
                }, subpage2Path);

            Utils.AssignObjectFactoriesAndDisplayModeProvider(subpage1, subpage2, page);

            var result = Utils.RenderWebPage(page);
            Assert.Equal("null 1 hello sub1nullx sub1nully sub2nullfoo world good bye", result);
        }

        [Fact]
        public void RenderPageNestedSubPageDictionaryTest()
        {
            // Test that PageData for each level of nesting returns the values as specified in the 
            // previous calling page.
            //
            // ~/index.cshtml does the following:
            // @(PageData["foo"] ?? "null")
            // @RenderPage("subpage.cshtml", new Dictionary<string, object>(){ { "foo", 1 }, { "bar", "hello"} })
            //
            // ~/subpage1.cshtml does the following:
            // @(PageData["foo"] ?? "sub1nullfoo")
            // @(PageData["bar"] ?? "sub1nullbar")
            // @(PageData["x"] ?? "sub1nullx")
            // @(PageData["y"] ?? "sub1nully")
            // @RenderPage("subpage2.cshtml", new Dictionary<string, object>(){ { { "bar", "world"}, {"x", "good"}, {"y", "bye"} })
            //
            // ~/subpage2.cshtml does the following:
            // @(PageData["foo"] ?? "sub2nullfoo")
            // @(PageData["bar"] ?? "sub2nullbar")
            // @(PageData["x"] ?? "sub2nullx")
            // @(PageData["y"] ?? "sub2nully")
            //
            // Expected result: null 1 hello sub1nullx sub1nully sub2nullfoo world good bye
            var page = Utils.CreatePage(
                p =>
                {
                    p.Write(p.PageData["foo"] ?? "null ");
                    p.Write(p.RenderPage("subpage1.cshtml", new Dictionary<string, object>() { { "foo", 1 }, { "bar", "hello" } }));
                });
            var subpage1Path = "~/subpage1.cshtml";
            var subpage1 = Utils.CreatePage(
                p =>
                {
                    p.Write(p.PageData["foo"] ?? "sub1nullfoo");
                    p.Write(" ");
                    p.Write(p.PageData["bar"] ?? "sub1nullbar");
                    p.Write(" ");
                    p.Write(p.PageData["x"] ?? "sub1nullx");
                    p.Write(" ");
                    p.Write(p.PageData["y"] ?? "sub1nully");
                    p.Write(" ");
                    p.Write(p.RenderPage("subpage2.cshtml", new Dictionary<string, object>() { { "bar", "world" }, { "x", "good" }, { "y", "bye" } }));
                }, subpage1Path);
            var subpage2Path = "~/subpage2.cshtml";
            var subpage2 = Utils.CreatePage(
                p =>
                {
                    p.Write(p.PageData["foo"] ?? "sub2nullfoo");
                    p.Write(" ");
                    p.Write(p.PageData["bar"] ?? "sub2nullbar");
                    p.Write(" ");
                    p.Write(p.PageData["x"] ?? "sub2nullx");
                    p.Write(" ");
                    p.Write(p.PageData["y"] ?? "sub2nully");
                }, subpage2Path);

            Utils.AssignObjectFactoriesAndDisplayModeProvider(page, subpage1, subpage2);

            var result = Utils.RenderWebPage(page);
            Assert.Equal("null 1 hello sub1nullx sub1nully sub2nullfoo world good bye", result);
        }

        [Fact]
        public void RenderPageNestedParentPageDataTest()
        {
            // PageData should return values set by parent pages.
            var page = Utils.CreatePage(
                p =>
                {
                    p.PageData["key1"] = "value1";
                    p.Write(p.RenderPage("subpage1.cshtml"));
                });
            var subpage1Path = "~/subpage1.cshtml";
            var subpage1 = Utils.CreatePage(
                p =>
                {
                    p.WriteLiteral("<subpage1>");
                    p.Write(p.PageData["key1"]);
                    p.Write(p.RenderPage("subpage2.cshtml"));
                    p.Write(p.PageData["key1"]);
                    p.PageData["key1"] = "value2";
                    p.Write(p.RenderPage("subpage2.cshtml"));
                    p.WriteLiteral("</subpage1>");
                }, subpage1Path);
            var subpage2Path = "~/subpage2.cshtml";
            var subpage2 = Utils.CreatePage(
                p =>
                {
                    p.WriteLiteral("<subpage2>");
                    p.Write(p.PageData["key1"]);
                    // Setting the value in the child page should
                    // not affect the parent page
                    p.PageData["key1"] = "value3";
                    p.Write(p.RenderPage("subpage3.cshtml", new { Key1 = "value4" }));
                    p.WriteLiteral("</subpage2>");
                }, subpage2Path);
            var subpage3Path = "~/subpage3.cshtml";
            var subpage3 = Utils.CreatePage(
                p =>
                {
                    p.WriteLiteral("<subpage3>");
                    p.Write(p.PageData["key1"]);
                    p.WriteLiteral("</subpage3>");
                }, subpage3Path);

            Utils.AssignObjectFactoriesAndDisplayModeProvider(subpage1, subpage2, subpage3, page);

            var result = Utils.RenderWebPage(page);
            Assert.Equal("<subpage1>value1<subpage2>value1<subpage3>value4</subpage3></subpage2>value1<subpage2>value2<subpage3>value4</subpage3></subpage2></subpage1>", result);
        }

        [Fact]
        public void WriteNullTest()
        {
            // Test for @null
            var result = Utils.RenderWebPage(
                p =>
                {
                    p.Write(null);
                    p.Write((object)null);
                    p.Write((HelperResult)null);
                });

            Assert.Equal("", result);
        }

        [Fact]
        public void WriteTest()
        {
            // Test for calling WebPage.Write on text and HtmlHelper
            var text = "Hello";
            var wrote = false;
            Action<TextWriter> action = tw =>
            {
                tw.Write(text);
                wrote = true;
            };
            var helper = new HelperResult(action);
            var result = Utils.RenderWebPage(
                p => { p.Write(helper); });
            Assert.Equal(text, result);
            Assert.True(wrote);
        }

        [Fact]
        public void WriteLiteralTest()
        {
            // Test for calling WebPage.WriteLiteral on text and HtmlHelper
            var text = "Hello";
            var wrote = false;
            Action<TextWriter> action = tw =>
            {
                tw.Write(text);
                wrote = true;
            };
            var helper = new HelperResult(action);
            var result = Utils.RenderWebPage(
                p => { p.WriteLiteral(helper); });
            Assert.Equal(text, result);
            Assert.True(wrote);
        }

        [Fact]
        public void ExtensionNotSupportedTest()
        {
            // Tests that calling RenderPage on an unsupported extension returns a new simpler error message
            // instead of the full error about build providers in system.web.dll.
            var vpath = "~/hello/world.txt";
            var ext = ".txt";
            var compilationUtilThrowingBuildManager = new CompilationUtil();
            var otherExceptionBuildManager = new Mock<IVirtualPathFactory>();
            var msg = "The file \"~/hello/world.txt\" could not be rendered, because it does not exist or is not a valid page.";
            otherExceptionBuildManager.Setup(c => c.CreateInstance(It.IsAny<string>())).Throws(new HttpException(msg));

            Assert.Throws<HttpException>(() =>
                                                  WebPage.CreateInstanceFromVirtualPath(vpath, new VirtualPathFactoryManager(compilationUtilThrowingBuildManager)),
                                                  String.Format(CultureInfo.CurrentCulture, WebPageResources.WebPage_FileNotSupported, ext, vpath));

            // Test that other error messages are thrown unmodified.
            Assert.Throws<HttpException>(() => WebPage.CreateInstanceFromVirtualPath(vpath, otherExceptionBuildManager.Object), msg);
        }

        [Fact]
        public void RenderBodyCalledInChildPageTest()
        {
            // A Page that is called by RenderPage should not be able to call RenderBody().

            Assert.Throws<HttpException>(() =>
                Utils.RenderWebPageWithSubPage(
                    p =>
                    {
                        p.Write("hello");
                        p.Write(p.RenderPage("subpage.cshtml"));
                    },
                    p =>
                    {
                        p.Write("world");
                        p.RenderBody();
                    }),
                String.Format(CultureInfo.CurrentCulture, WebPageResources.WebPage_CannotRequestDirectly, "~/subpage.cshtml", "RenderBody"));
        }

        [Fact]
        public void RenderPageInvalidPageType()
        {
            var pagePath = "~/foo.js";
            var page = Utils.CreatePage(p => { p.Write(p.RenderPage(pagePath)); });

            var objectFactory = new Mock<IVirtualPathFactory>();
            objectFactory.Setup(c => c.Exists(It.IsAny<string>())).Returns<string>(p => pagePath.Equals(p, StringComparison.OrdinalIgnoreCase));
            objectFactory.Setup(c => c.CreateInstance(It.IsAny<string>())).Returns<string>(_ => null);
            page.VirtualPathFactory = objectFactory.Object;

            Assert.Throws<HttpException>(() =>
            {
                page.VirtualPathFactory = objectFactory.Object;
                page.DisplayModeProvider = new DisplayModeProvider();
                Utils.RenderWebPage(page);
            },
            String.Format(CultureInfo.CurrentCulture, WebPageResources.WebPage_InvalidPageType, pagePath));
        }

        [Fact]
        public void RenderPageValidPageType()
        {
            var pagePath = "~/foo.js";
            var page = Utils.CreatePage(p => { p.Write(p.RenderPage(pagePath)); });

            var contents = "hello world";
            var subPage = Utils.CreatePage(p => p.Write(contents), pagePath);

            Utils.AssignObjectFactoriesAndDisplayModeProvider(page, subPage);

            Assert.Equal(contents, Utils.RenderWebPage(page));
        }

        [Fact]
        public void RenderPageNull()
        {
            Assert.ThrowsArgumentNullOrEmptyString(() => Utils.RenderWebPage(p => p.RenderPage(null)), "path");
        }

        [Fact]
        public void RenderPageEmptyString()
        {
            Assert.ThrowsArgumentNullOrEmptyString(() => Utils.RenderWebPage(p => p.RenderPage("")), "path");
        }

        [Fact]
        public void SamePageCaseInsensitiveTest()
        {
            var result = Utils.RenderWebPage(
                p =>
                {
                    p.PageData["xyz"] = "value";
                    p.PageData["XYZ"] = "VALUE";
                    p.Write(p.PageData["xYz"]);
                });

            Assert.Equal("VALUE", result);
        }
    }
}
