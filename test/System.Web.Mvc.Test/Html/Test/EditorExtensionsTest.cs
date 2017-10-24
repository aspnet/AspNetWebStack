// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using Microsoft.TestCommon;
using Microsoft.Web.UnitTestUtil;
using Moq;

namespace System.Web.Mvc.Html.Test
{
    [Xunit.Collection("Uses ScopeStorage or ViewEngines.Engines")]
    public class EditorExtensionsTest
    {
        [Fact]
        public void EditorNullExpressionThrows()
        {
            // Act & Assert
            Assert.ThrowsArgumentNull(
                () => MvcHelper.GetHtmlHelper().Editor(expression: null),
                "expression");
        }

        [Theory]
        [PropertyData("AttributeEncodedData_NoHtmlEncode", PropertyType = typeof(EncodedDataSets))]
        public void BooleanTemplate_AttributeEncodes_AddedHtmlAttributes(string text, string htmlEncodedText)
        {
            // Arrange
            var expectedResult = "<input attribute=\"" +
                htmlEncodedText +
                "\" checked=\"checked\" class=\"check-box\" id=\"Prefix\" name=\"Prefix\" type=\"checkbox\" " +
                "value=\"true\" />" +
                "<input name=\"Prefix\" type=\"hidden\" value=\"false\" />";
            var viewData = new ViewDataDictionary<bool>(true);
            viewData.Add("htmlAttributes", new { attribute = text, });
            viewData.TemplateInfo.HtmlFieldPrefix = "Prefix";

            var html = MvcHelper.GetHtmlHelper(viewData);

            var viewContext = Mock.Get(html.ViewContext);
            viewContext.Setup(c => c.TempData).Returns(new TempDataDictionary());
            viewContext.Setup(c => c.View).Returns(new DummyView());
            viewContext.Setup(c => c.Writer).Returns(TextWriter.Null);

            string editorResult;
            string editorForResult;
            string editorForModelResult;
            using (new TemplateHelpersSafeScope())
            {
                // Act
                editorResult = html.Editor("").ToHtmlString();
                editorForResult = html.EditorFor(m => m).ToHtmlString();
                editorForModelResult = html.EditorForModel().ToHtmlString();
            }

            // Assert
            Assert.Equal(expectedResult, editorResult);
            Assert.Equal(expectedResult, editorForResult);
            Assert.Equal(expectedResult, editorForModelResult);
        }

        [Theory]
        [PropertyData("ConditionallyHtmlEncodedData", PropertyType = typeof(EncodedDataSets))]
        public void CollectionTemplateWrappingObjectTemplate_EncodesSimpleDisplayTextOfItems_IfHtmlEncode(
            string text,
            bool htmlEncode,
            string expectedResult)
        {
            // Arrange
            var innerModel = new ObjectTemplateModel
            {
                Property1 = text,           // SimpleDisplayText uses first property by default.
            };
            var model = new[] { innerModel, innerModel, };
            var viewData = new ViewDataDictionary<ObjectTemplateModel[]>(model);
            var html = MvcHelper.GetHtmlHelper(viewData);

            // GetHtmlHelper does not mock enough of the ViewContext for TemplateHelpers use.
            var viewContext = Mock.Get(html.ViewContext);
            viewContext.Setup(c => c.TempData).Returns(new TempDataDictionary());
            viewContext.Setup(c => c.View).Returns(new DummyView());
            viewContext.Setup(c => c.Writer).Returns(TextWriter.Null);

            // Developers might need to do something similar (including MetadataOverrideScope or another approach
            // replacing ModelMetadataProviders.Current) since for example [DisplayFormat] cannot be applied to a class.
            var metadata = ModelMetadataProviders.Current.GetMetadataForType(() => null, typeof(ObjectTemplateModel));
            metadata.HtmlEncode = htmlEncode;

            string editorResult;
            string editorForResult;
            string editorForModelResult;
            using (new TemplateHelpersSafeScope())
            {
                using (new MetadataOverrideScope(metadata))
                {
                    // Act
                    editorResult = html.Editor("").ToHtmlString();
                    editorForResult = html.EditorFor(m => m).ToHtmlString();
                    editorForModelResult = html.EditorForModel().ToHtmlString();
                }
            }

            // Assert
            Assert.Equal(expectedResult + expectedResult, editorResult);
            Assert.Equal(expectedResult + expectedResult, editorForResult);
            Assert.Equal(expectedResult + expectedResult, editorForModelResult);
        }

        // Inconsistent but long-standing behavior.
        [Theory]
        [PropertyData("ConditionallyHtmlEncodedData_NoEncodedText", PropertyType = typeof(EncodedDataSets))]
        public void CollectionTemplateWrappingObjectTemplate_DoesNotEncodeNullDisplayText_IfNull(
            string text,
            bool htmlEncode)
        {
            // Arrange
            var model = new[] { (ObjectTemplateModel)null, };
            var viewData = new ViewDataDictionary<ObjectTemplateModel[]>(model);
            var html = MvcHelper.GetHtmlHelper(viewData);

            // GetHtmlHelper does not mock enough of the ViewContext for TemplateHelpers use.
            var viewContext = Mock.Get(html.ViewContext);
            viewContext.Setup(c => c.TempData).Returns(new TempDataDictionary());
            viewContext.Setup(c => c.View).Returns(new DummyView());
            viewContext.Setup(c => c.Writer).Returns(TextWriter.Null);

            // Developers might need to do something similar (including MetadataOverrideScope or another approach
            // replacing ModelMetadataProviders.Current) since for example [DisplayFormat] cannot be applied to a class.
            var metadata = ModelMetadataProviders.Current.GetMetadataForType(() => null, typeof(ObjectTemplateModel));
            metadata.HtmlEncode = htmlEncode;
            metadata.NullDisplayText = text;

            string editorResult;
            string editorForResult;
            string editorForModelResult;
            using (new TemplateHelpersSafeScope())
            {
                using (new MetadataOverrideScope(metadata))
                {
                    // Act
                    editorResult = html.Editor("").ToHtmlString();
                    editorForResult = html.EditorFor(m => m).ToHtmlString();
                    editorForModelResult = html.EditorForModel().ToHtmlString();
                }
            }

            // Assert
            Assert.Equal(text, editorResult);
            Assert.Equal(text, editorForResult);
            Assert.Equal(text, editorForModelResult);
        }

        [Theory]
        [PropertyData("AttributeEncodedData_NoHtmlEncode", PropertyType = typeof(EncodedDataSets))]
        public void StringTemplate_AttributeEncodes_AddedHtmlAttributes(string text, string htmlEncodedText)
        {
            // Arrange
            var expectedResult = "<input attribute=\"" +
                htmlEncodedText +
                "\" class=\"text-box single-line\" id=\"Prefix\" name=\"Prefix\" type=\"text\" value=\"string\" />";
            var viewData = new ViewDataDictionary<string>("string");
            viewData.Add("htmlAttributes", new { attribute = text, });
            viewData.TemplateInfo.HtmlFieldPrefix = "Prefix";

            var html = MvcHelper.GetHtmlHelper(viewData);

            var viewContext = Mock.Get(html.ViewContext);
            viewContext.Setup(c => c.TempData).Returns(new TempDataDictionary());
            viewContext.Setup(c => c.View).Returns(new DummyView());
            viewContext.Setup(c => c.Writer).Returns(TextWriter.Null);

            string editorResult;
            string editorForResult;
            string editorForModelResult;
            using (new TemplateHelpersSafeScope())
            {
                // Act
                editorResult = html.Editor("").ToHtmlString();
                editorForResult = html.EditorFor(m => m).ToHtmlString();
                editorForModelResult = html.EditorForModel().ToHtmlString();
            }

            // Assert
            Assert.Equal(expectedResult, editorResult);
            Assert.Equal(expectedResult, editorForResult);
            Assert.Equal(expectedResult, editorForModelResult);
        }

        [Theory]
        [PropertyData("AttributeEncodedData_NoHtmlEncode", PropertyType = typeof(EncodedDataSets))]
        public void StringTemplate_AttributeEncodesText(string text, string htmlEncodedText)
        {
            // Arrange
            var expectedResult =
                "<input class=\"text-box single-line\" id=\"Prefix\" name=\"Prefix\" type=\"text\" value=\"" +
                    htmlEncodedText +
                    "\" />";
            var viewData = new ViewDataDictionary<string>(text);
            viewData.TemplateInfo.HtmlFieldPrefix = "Prefix";

            var html = MvcHelper.GetHtmlHelper(viewData);

            var viewContext = Mock.Get(html.ViewContext);
            viewContext.Setup(c => c.TempData).Returns(new TempDataDictionary());
            viewContext.Setup(c => c.View).Returns(new DummyView());
            viewContext.Setup(c => c.Writer).Returns(TextWriter.Null);

            string editorResult;
            string editorForResult;
            string editorForModelResult;
            using (new TemplateHelpersSafeScope())
            {
                // Act
                editorResult = html.Editor("").ToHtmlString();
                editorForResult = html.EditorFor(m => m).ToHtmlString();
                editorForModelResult = html.EditorForModel().ToHtmlString();
            }

            // Assert
            Assert.Equal(expectedResult, editorResult);
            Assert.Equal(expectedResult, editorForResult);
            Assert.Equal(expectedResult, editorForModelResult);
        }

        [Fact]
        public void Editor_FindsViewDataMember()
        {
            // Arrange
            var model = new ObjectTemplateModel { Property1 = "Model string" };
            var viewData = new ViewDataDictionary<ObjectTemplateModel>(model);
            viewData["Property1"] = "ViewData string";
            var html = MvcHelper.GetHtmlHelper(viewData);

            var viewContext = Mock.Get(html.ViewContext);
            viewContext.Setup(c => c.TempData).Returns(new TempDataDictionary());
            viewContext.Setup(c => c.View).Returns(new DummyView());
            viewContext.Setup(c => c.Writer).Returns(TextWriter.Null);

            MvcHtmlString result;
            using (new TemplateHelpersSafeScope())
            {
                // Act
                result = html.Editor("Property1");
            }

            // Assert
            Assert.Equal(
                "<input class=\"text-box single-line\" id=\"Property1\" name=\"Property1\" type=\"text\" value=\"ViewData string\" />",
                result.ToString());
        }

        [Fact]
        public void EditorFor_FindsModel()
        {
            var model = new ObjectTemplateModel { Property1 = "Model string" };
            var viewData = new ViewDataDictionary<ObjectTemplateModel>(model);
            viewData["Property1"] = "ViewData string";
            var html = MvcHelper.GetHtmlHelper(viewData);

            var viewContext = Mock.Get(html.ViewContext);
            viewContext.Setup(c => c.TempData).Returns(new TempDataDictionary());
            viewContext.Setup(c => c.View).Returns(new DummyView());
            viewContext.Setup(c => c.Writer).Returns(TextWriter.Null);

            MvcHtmlString result;
            using (new TemplateHelpersSafeScope())
            {
                // Act
                result = html.EditorFor(m => m.Property1);
            }

            // Assert
            Assert.Equal(
                "<input class=\"text-box single-line\" id=\"Property1\" name=\"Property1\" type=\"text\" value=\"Model string\" />",
                result.ToString());
        }

        [Fact]
        public void Editor_FindsModel_IfNoViewDataMember()
        {
            // Arrange
            var model = new ObjectTemplateModel { Property1 = "Model string" };
            var viewData = new ViewDataDictionary<ObjectTemplateModel>(model);
            var html = MvcHelper.GetHtmlHelper(viewData);

            var viewContext = Mock.Get(html.ViewContext);
            viewContext.Setup(c => c.TempData).Returns(new TempDataDictionary());
            viewContext.Setup(c => c.View).Returns(new DummyView());
            viewContext.Setup(c => c.Writer).Returns(TextWriter.Null);

            MvcHtmlString result;
            using (new TemplateHelpersSafeScope())
            {
                // Act
                result = html.Editor("Property1");
            }

            // Assert
            Assert.Equal(
                "<input class=\"text-box single-line\" id=\"Property1\" name=\"Property1\" type=\"text\" value=\"Model string\" />",
                result.ToString());
        }

        [Fact]
        public void EditorFor_FindsModel_EvenIfNull()
        {
            var model = new ObjectTemplateModel();
            var viewData = new ViewDataDictionary<ObjectTemplateModel>(model);
            viewData["Property1"] = "ViewData string";
            var html = MvcHelper.GetHtmlHelper(viewData);

            var viewContext = Mock.Get(html.ViewContext);
            viewContext.Setup(c => c.TempData).Returns(new TempDataDictionary());
            viewContext.Setup(c => c.View).Returns(new DummyView());
            viewContext.Setup(c => c.Writer).Returns(TextWriter.Null);

            MvcHtmlString result;
            using (new TemplateHelpersSafeScope())
            {
                // Act
                result = html.EditorFor(m => m.Property1);
            }

            // Assert
            Assert.Equal(
                "<input class=\"text-box single-line\" id=\"Property1\" name=\"Property1\" type=\"text\" value=\"\" />",
                result.ToString());
        }

        private class ObjectTemplateModel
        {
            public string Property1 { get; set; }
            public string Property2 { get; set; }
        }

        private class DummyView : IView
        {
            public void Render(ViewContext viewContext, TextWriter writer)
            {
                throw new NotImplementedException();
            }
        }
    }
}
