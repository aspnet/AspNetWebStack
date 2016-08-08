// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Web.WebPages.Html;
using Microsoft.TestCommon;

namespace System.Web.Mvc.Test
{
    public class TagBuilderTest
    {
        [Fact]
        public void AddCssClassPrepends()
        {
            // Arrange
            TagBuilder builder = new TagBuilder("SomeTag");
            builder.MergeAttribute("class", "oldA");

            // Act
            builder.AddCssClass("newA");

            // Assert
            Assert.Equal("newA oldA", builder.Attributes["class"]);
        }

        [Fact]
        public void AttributesProperty()
        {
            // Arrange
            TagBuilder builder = new TagBuilder("SomeTag");

            // Act
            SortedDictionary<string, string> attributes = builder.Attributes as SortedDictionary<string, string>;

            // Assert
            Assert.NotNull(attributes);
            Assert.Equal(StringComparer.Ordinal, attributes.Comparer);
        }

        [Fact]
        public void ConstructorSetsTagNameProperty()
        {
            // Arrange
            TagBuilder builder = new TagBuilder("SomeTag");

            // Act
            string tagName = builder.TagName;

            // Assert
            Assert.Equal("SomeTag", tagName);
        }

        [Fact]
        public void ConstructorWithEmptyTagNameThrows()
        {
            Assert.ThrowsArgumentNullOrEmptyString(
                delegate { new TagBuilder(String.Empty); }, "tagName");
        }

        [Fact]
        public void ConstructorWithNullTagNameThrows()
        {
            Assert.ThrowsArgumentNullOrEmptyString(
                delegate { new TagBuilder(null /* tagName */); }, "tagName");
        }

        [Fact]
        public void CreateSanitizedIdThrowsIfInvalidCharReplacementIsNull()
        {
            // Act & Assert
            Assert.ThrowsArgumentNull(
                () => TagBuilder.CreateSanitizedId("tagId", null),
                "invalidCharReplacement");
        }

        [Fact]
        public void CreateSanitizedIdDefaultsToHtmlHelperIdAttributeDotReplacement()
        {
            // Arrange
            String defaultReplacementChar = HtmlHelper.IdAttributeDotReplacement;

            // Act
            string sanitizedId = TagBuilder.CreateSanitizedId("Hello world");

            // Assert
            Assert.Equal("Hello" + defaultReplacementChar + "world", sanitizedId);
        }

        [Fact]
        public void CreateSanitizedId_ReturnsNullIfOriginalIdBeginsWithNonLetter()
        {
            // Act
            string retVal = TagBuilder.CreateSanitizedId("_DoesNotBeginWithALetter", "!REPL!");

            // Assert
            Assert.Null(retVal);
        }

        [Fact]
        public void CreateSanitizedId_ReturnsNullIfOriginalIdIsEmpty()
        {
            // Act
            string retVal = TagBuilder.CreateSanitizedId("", "!REPL!");

            // Assert
            Assert.Null(retVal);
        }

        [Fact]
        public void CreateSanitizedId_ReturnsNullIfOriginalIdIsNull()
        {
            // Act
            string retVal = TagBuilder.CreateSanitizedId(null, "!REPL!");

            // Assert
            Assert.Null(retVal);
        }

        [Fact]
        public void CreateSanitizedId_ReturnsSanitizedId()
        {
            // Arrange
            string expected = "ABCXYZabcxyz012789!REPL!!REPL!!REPL!!REPL!!REPL!!REPL!!REPL!!REPL!!REPL!!REPL!-!REPL!_!REPL!!REPL!:";

            // Act
            string retVal = TagBuilder.CreateSanitizedId("ABCXYZabcxyz012789!@#$%^&*()-=_+.:", "!REPL!");

            // Assert
            Assert.Equal(expected, retVal);
        }

        [Fact]
        public void GenerateId_AddsSanitizedId()
        {
            // Arrange
            TagBuilder builder = new TagBuilder("div");
            builder.IdAttributeDotReplacement = "x";

            // Act
            builder.GenerateId("Hello, world.");

            // Assert
            Assert.Equal("Helloxxworldx", builder.Attributes["id"]);
        }

        [Fact]
        public void GenerateId_DoesNotAddIdIfIdAlreadyExists()
        {
            // Arrange
            TagBuilder builder = new TagBuilder("div");
            builder.GenerateId("old");

            // Act
            builder.GenerateId("new");

            // Assert
            Assert.Equal("old", builder.Attributes["id"]);
        }

        [Fact]
        public void GenerateId_DoesNotAddIdIfSanitizationReturnsNull()
        {
            // Arrange
            TagBuilder builder = new TagBuilder("div");

            // Act
            builder.GenerateId("");

            // Assert
            Assert.False(builder.Attributes.ContainsKey("id"));
        }

        [Fact]
        public void InnerHtmlProperty()
        {
            // Arrange
            TagBuilder builder = new TagBuilder("SomeTag");

            // Act & Assert
            Assert.Equal(String.Empty, builder.InnerHtml);
            builder.InnerHtml = "foo";
            Assert.Equal("foo", builder.InnerHtml);
            builder.InnerHtml = null;
            Assert.Equal(String.Empty, builder.InnerHtml);
        }

        [Fact]
        public void MergeAttributeDoesNotOverwriteExistingValuesByDefault()
        {
            // Arrange
            TagBuilder builder = new TagBuilder("SomeTag");
            builder.MergeAttribute("a", "oldA");

            // Act
            builder.MergeAttribute("a", "newA");

            // Assert
            Assert.Equal("oldA", builder.Attributes["a"]);
        }

        [Fact]
        public void MergeAttributeOverwritesExistingValueIfAsked()
        {
            // Arrange
            TagBuilder builder = new TagBuilder("SomeTag");
            builder.MergeAttribute("a", "oldA");

            // Act
            builder.MergeAttribute("a", "newA", true);

            // Assert
            Assert.Equal("newA", builder.Attributes["a"]);
        }

        [Fact]
        public void MergeAttributeWithEmptyKeyThrows()
        {
            // Arrange
            TagBuilder builder = new TagBuilder("SomeTag");

            // Act & Assert
            Assert.ThrowsArgumentNullOrEmptyString(
                delegate { builder.MergeAttribute(String.Empty, "value"); }, "key");
        }

        [Fact]
        public void MergeAttributeWithNullKeyThrows()
        {
            // Arrange
            TagBuilder builder = new TagBuilder("SomeTag");

            // Act & Assert
            Assert.ThrowsArgumentNullOrEmptyString(
                delegate { builder.MergeAttribute(null, "value"); }, "key");
        }

        [Fact]
        public void MergeAttributesDoesNotOverwriteExistingValuesByDefault()
        {
            // Arrange
            TagBuilder builder = new TagBuilder("SomeTag");
            builder.Attributes["a"] = "oldA";

            Dictionary<string, string> newAttrs = new Dictionary<string, string>
            {
                { "a", "newA" },
                { "b", "newB" }
            };

            // Act
            builder.MergeAttributes(newAttrs);

            // Assert
            Assert.Equal(2, builder.Attributes.Count);
            Assert.Equal("oldA", builder.Attributes["a"]);
            Assert.Equal("newB", builder.Attributes["b"]);
        }

        [Fact]
        public void MergeAttributesOverwritesExistingValueIfAsked()
        {
            // Arrange
            TagBuilder builder = new TagBuilder("SomeTag");
            builder.Attributes["a"] = "oldA";

            Dictionary<string, string> newAttrs = new Dictionary<string, string>
            {
                { "a", "newA" },
                { "b", "newB" }
            };

            // Act
            builder.MergeAttributes(newAttrs, true);

            // Assert
            Assert.Equal(2, builder.Attributes.Count);
            Assert.Equal("newA", builder.Attributes["a"]);
            Assert.Equal("newB", builder.Attributes["b"]);
        }

        [Fact]
        public void MergeAttributesWithNullAttributesDoesNothing()
        {
            // Arrange
            TagBuilder builder = new TagBuilder("SomeTag");

            // Act
            builder.MergeAttributes<string, string>(null);

            // Assert
            Assert.Equal(0, builder.Attributes.Count);
        }

        [Fact]
        public void SetInnerTextEncodes()
        {
            // Arrange
            TagBuilder builder = new TagBuilder("SomeTag");

            // Act
            builder.SetInnerText("<>");

            // Assert
            Assert.Equal("&lt;&gt;", builder.InnerHtml);
        }

        [Fact]
        public void ToStringDefaultsToNormal()
        {
            // Arrange
            TagBuilder builder = new TagBuilder("SomeTag")
            {
                InnerHtml = "<x&y>"
            };
            builder.MergeAttributes(GetAttributesDictionary());

            // Act
            string output = builder.ToString();

            // Assert
            Assert.Equal(@"<SomeTag a=""Foo"" b=""Bar&amp;Baz"" c=""&lt;&quot;Quux&quot;>""><x&y></SomeTag>", output);
        }

        [Fact]
        public void ToStringDoesNotOutputEmptyIdTags()
        {
            // Arrange
            TagBuilder builder = new TagBuilder("SomeTag");
            builder.Attributes["foo"] = "fooValue";
            builder.Attributes["bar"] = "barValue";
            builder.Attributes["id"] = "";

            // Act
            string output = builder.ToString(TagRenderMode.SelfClosing);

            Assert.Equal(@"<SomeTag bar=""barValue"" foo=""fooValue"" />", output);
        }

        [Fact]
        public void ToStringEndTag()
        {
            // Arrange
            TagBuilder builder = new TagBuilder("SomeTag")
            {
                InnerHtml = "<x&y>"
            };
            builder.MergeAttributes(GetAttributesDictionary());

            // Act
            string output = builder.ToString(TagRenderMode.EndTag);

            // Assert
            Assert.Equal(@"</SomeTag>", output);
        }

        [Fact]
        public void ToStringNormal()
        {
            // Arrange
            TagBuilder builder = new TagBuilder("SomeTag")
            {
                InnerHtml = "<x&y>"
            };
            builder.MergeAttributes(GetAttributesDictionary());

            // Act
            string output = builder.ToString(TagRenderMode.Normal);

            // Assert
            Assert.Equal(@"<SomeTag a=""Foo"" b=""Bar&amp;Baz"" c=""&lt;&quot;Quux&quot;>""><x&y></SomeTag>", output);
        }

        [Fact]
        public void ToStringSelfClosing()
        {
            // Arrange
            TagBuilder builder = new TagBuilder("SomeTag")
            {
                InnerHtml = "<x&y>"
            };
            builder.MergeAttributes(GetAttributesDictionary());

            // Act
            string output = builder.ToString(TagRenderMode.SelfClosing);

            // Assert
            Assert.Equal(@"<SomeTag a=""Foo"" b=""Bar&amp;Baz"" c=""&lt;&quot;Quux&quot;>"" />", output);
        }

        [Fact]
        public void ToStringStartTag()
        {
            // Arrange
            TagBuilder builder = new TagBuilder("SomeTag")
            {
                InnerHtml = "<x&y>"
            };
            builder.MergeAttributes(GetAttributesDictionary());

            // Act
            string output = builder.ToString(TagRenderMode.StartTag);

            // Assert
            Assert.Equal(@"<SomeTag a=""Foo"" b=""Bar&amp;Baz"" c=""&lt;&quot;Quux&quot;>"">", output);
        }

        private static IDictionary<string, string> GetAttributesDictionary()
        {
            return new SortedDictionary<string, string>
            {
                { "a", "Foo" },
                { "b", "Bar&Baz" },
                { "c", @"<""Quux"">" }
            };
        }
    }
}
