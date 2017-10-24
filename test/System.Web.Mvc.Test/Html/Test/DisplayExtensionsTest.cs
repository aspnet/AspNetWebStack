// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using Microsoft.TestCommon;
using Microsoft.Web.UnitTestUtil;
using Moq;

namespace System.Web.Mvc.Html.Test
{
    [Xunit.Collection("Uses ScopeStorage or ViewEngines.Engines")]
    public class DisplayExtensionsTest
    {
        [Fact]
        public void DisplayNullExpressionThrows()
        {
            // Act & Assert
            Assert.ThrowsArgumentNull(
                () => MvcHelper.GetHtmlHelper().Display(expression: null),
                "expression");
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

            string displayResult;
            string displayForResult;
            string displayForModelResult;
            using (new TemplateHelpersSafeScope())
            {
                using (new MetadataOverrideScope(metadata))
                {
                    // Act
                    displayResult = html.Display("").ToHtmlString();
                    displayForResult = html.DisplayFor(m => m).ToHtmlString();
                    displayForModelResult = html.DisplayForModel().ToHtmlString();
                }
            }

            // Assert
            Assert.Equal(expectedResult + expectedResult, displayResult);
            Assert.Equal(expectedResult + expectedResult, displayForResult);
            Assert.Equal(expectedResult + expectedResult, displayForModelResult);
        }

        [Theory]
        [PropertyData("AttributeAndHtmlEncodedData", PropertyType = typeof(EncodedDataSets))]
        public void EmailTemplate_AttributeAndHtmlEncodes(
            string text,
            bool htmlEncode,
            string attributeEncodedText,
            string htmlEncodedText)
        {
            // Arrange
            var expectedResult = "<a href=\"mailto:" + attributeEncodedText + "\">" + htmlEncodedText + "</a>";
            var viewData = new ViewDataDictionary<string>(text);
            var html = MvcHelper.GetHtmlHelper(viewData);

            var viewContext = Mock.Get(html.ViewContext);
            viewContext.Setup(c => c.TempData).Returns(new TempDataDictionary());
            viewContext.Setup(c => c.View).Returns(new DummyView());
            viewContext.Setup(c => c.Writer).Returns(TextWriter.Null);

            var metadata = ModelMetadataProviders.Current.GetMetadataForType(() => null, typeof(string));
            metadata.TemplateHint = "EmailAddress";
            metadata.HtmlEncode = htmlEncode;

            string displayResult;
            string displayForResult;
            string displayForModelResult;
            using (new TemplateHelpersSafeScope())
            {
                using (new MetadataOverrideScope(metadata))
                {
                    // Act
                    displayResult = html.Display("").ToHtmlString();
                    displayForResult = html.DisplayFor(m => m).ToHtmlString();
                    displayForModelResult = html.DisplayForModel().ToHtmlString();
                }
            }

            // Assert
            Assert.Equal(expectedResult, displayResult);
            Assert.Equal(expectedResult, displayForResult);
            Assert.Equal(expectedResult, displayForModelResult);
        }

        [Theory]
        [PropertyData("ConditionallyHtmlEncodedData", PropertyType = typeof(EncodedDataSets))]
        public void HtmlOrStringTemplate_HtmlEncodesValue_IfHtmlEncode(
            string text,
            bool htmlEncode,
            string expectedResult)
        {
            // Arrange
            var viewData = new ViewDataDictionary<string>(text);
            var html = MvcHelper.GetHtmlHelper(viewData);

            var viewContext = Mock.Get(html.ViewContext);
            viewContext.Setup(c => c.TempData).Returns(new TempDataDictionary());
            viewContext.Setup(c => c.View).Returns(new DummyView());
            viewContext.Setup(c => c.Writer).Returns(TextWriter.Null);

            var metadata = ModelMetadataProviders.Current.GetMetadataForType(() => null, typeof(string));
            metadata.TemplateHint = htmlEncode ? "String" : "Html";

            string displayResult;
            string displayForResult;
            string displayForModelResult;
            using (new TemplateHelpersSafeScope())
            {
                using (new MetadataOverrideScope(metadata))
                {
                    // Act
                    displayResult = html.Display("").ToHtmlString();
                    displayForResult = html.DisplayFor(m => m).ToHtmlString();
                    displayForModelResult = html.DisplayForModel().ToHtmlString();
                }
            }

            // Assert
            Assert.Equal(expectedResult, displayResult);
            Assert.Equal(expectedResult, displayForResult);
            Assert.Equal(expectedResult, displayForModelResult);
        }

        // Inconsistent but long-standing behavior.
        [Theory]
        [PropertyData("HtmlEncodedData_JustText", PropertyType = typeof(EncodedDataSets))]
        public void ObjectTemplate_DoesNotEncodeNullDisplayText(string text)
        {
            // Arrange
            var viewData = new ViewDataDictionary<ObjectTemplateModel>(model: null);
            var html = MvcHelper.GetHtmlHelper(viewData);

            var viewContext = Mock.Get(html.ViewContext);
            viewContext.Setup(c => c.TempData).Returns(new TempDataDictionary());
            viewContext.Setup(c => c.View).Returns(new DummyView());
            viewContext.Setup(c => c.Writer).Returns(TextWriter.Null);

            var metadata = ModelMetadataProviders.Current.GetMetadataForType(() => null, typeof(ObjectTemplateModel));
            metadata.NullDisplayText = text;

            string displayResult;
            string displayForResult;
            string displayForModelResult;
            using (new TemplateHelpersSafeScope())
            {
                using (new MetadataOverrideScope(metadata))
                {
                    // Act
                    displayResult = html.Display("").ToHtmlString();
                    displayForResult = html.DisplayFor(m => m).ToHtmlString();
                    displayForModelResult = html.DisplayForModel().ToHtmlString();
                }
            }

            // Assert
            Assert.Equal(text, displayResult);
            Assert.Equal(text, displayForResult);
            Assert.Equal(text, displayForModelResult);
        }

        [Theory]
        [PropertyData("AttributeAndHtmlEncodedData", PropertyType = typeof(EncodedDataSets))]
        public void UrlTemplate_AttributeAndHtmlEncodes(
            string text,
            bool htmlEncode,
            string attributeEncodedText,
            string htmlEncodedText)
        {
            // Arrange
            var expectedResult = "<a href=\"" + attributeEncodedText + "\">" + htmlEncodedText + "</a>";
            var viewData = new ViewDataDictionary<string>(text);
            var html = MvcHelper.GetHtmlHelper(viewData);

            var viewContext = Mock.Get(html.ViewContext);
            viewContext.Setup(c => c.TempData).Returns(new TempDataDictionary());
            viewContext.Setup(c => c.View).Returns(new DummyView());
            viewContext.Setup(c => c.Writer).Returns(TextWriter.Null);

            var metadata = ModelMetadataProviders.Current.GetMetadataForType(() => null, typeof(string));
            metadata.TemplateHint = "Url";
            metadata.HtmlEncode = htmlEncode;

            string displayResult;
            string displayForResult;
            string displayForModelResult;
            using (new TemplateHelpersSafeScope())
            {
                using (new MetadataOverrideScope(metadata))
                {
                    // Act
                    displayResult = html.Display("").ToHtmlString();
                    displayForResult = html.DisplayFor(m => m).ToHtmlString();
                    displayForModelResult = html.DisplayForModel().ToHtmlString();
                }
            }

            // Assert
            Assert.Equal(expectedResult, displayResult);
            Assert.Equal(expectedResult, displayForResult);
            Assert.Equal(expectedResult, displayForModelResult);
        }

        [Fact]
        public void Display_FindsViewDataMember()
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
                result = html.Display("Property1");
            }

            // Assert
            Assert.Equal("ViewData string", result.ToString());
        }

        [Fact]
        public void DisplayFor_FindsModel()
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
                result = html.DisplayFor(m => m.Property1);
            }

            // Assert
            Assert.Equal("Model string", result.ToString());
        }

        [Fact]
        public void Display_FindsModel_IfNoViewDataMember()
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
                result = html.Display("Property1");
            }

            // Assert
            Assert.Equal("Model string", result.ToString());
        }

        [Fact]
        public void DisplayFor_FindsModel_EvenIfNull()
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
                result = html.DisplayFor(m => m.Property1);
            }

            // Assert
            Assert.Empty(result.ToString());
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
