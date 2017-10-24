// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq.Expressions;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Mvc.Html.Test
{
    public class LabelExtensionsTest
    {
        Mock<ModelMetadataProvider> metadataProvider;
        Mock<ModelMetadata> metadata;
        ViewDataDictionary viewData;
        Mock<ViewContext> viewContext;
        Mock<IViewDataContainer> viewDataContainer;
        HtmlHelper<object> html;

        public LabelExtensionsTest()
        {
            metadataProvider = new Mock<ModelMetadataProvider>();
            metadata = new Mock<ModelMetadata>(metadataProvider.Object, null, null, typeof(object), null);
            viewData = new ViewDataDictionary();

            viewContext = new Mock<ViewContext>();
            viewContext.Setup(c => c.ViewData).Returns(viewData);

            viewDataContainer = new Mock<IViewDataContainer>();
            viewDataContainer.Setup(c => c.ViewData).Returns(viewData);

            html = new HtmlHelper<object>(viewContext.Object, viewDataContainer.Object);

            metadataProvider.Setup(p => p.GetMetadataForProperties(It.IsAny<object>(), It.IsAny<Type>()))
                .Returns(new ModelMetadata[0]);
            metadataProvider.Setup(p => p.GetMetadataForProperty(It.IsAny<Func<object>>(), It.IsAny<Type>(), It.IsAny<string>()))
                .Returns(metadata.Object);
            metadataProvider.Setup(p => p.GetMetadataForType(It.IsAny<Func<object>>(), It.IsAny<Type>()))
                .Returns(metadata.Object);
        }

        // Label tests

        [Fact]
        public void LabelNullExpressionThrows()
        {
            // Act & Assert
            Assert.ThrowsArgumentNull(
                () => html.Label(null),
                "expression");
        }

        [Fact]
        public void LabelViewDataNotFound()
        {
            // Act
            MvcHtmlString result = html.Label("PropertyName", null, null, metadataProvider.Object);

            // Assert
            Assert.Equal(@"<label for=""PropertyName"">PropertyName</label>", result.ToHtmlString());
        }

        [Fact]
        public void LabelViewDataNull()
        {
            // Act
            viewData["PropertyName"] = null;
            MvcHtmlString result = html.Label("PropertyName", null, null, metadataProvider.Object);

            // Assert
            Assert.Equal(@"<label for=""PropertyName"">PropertyName</label>", result.ToHtmlString());
        }

        class Model
        {
            public string PropertyName { get; set; }
        }

        [Fact]
        public void LabelViewDataFromPropertyGetsActualPropertyType()
        {
            // Arrange
            Model model = new Model { PropertyName = "propertyValue" };
            HtmlHelper<Model> html = new HtmlHelper<Model>(viewContext.Object, viewDataContainer.Object);
            viewData.Model = model;
            metadataProvider.Setup(p => p.GetMetadataForProperty(It.IsAny<Func<object>>(), typeof(Model), "PropertyName"))
                .Returns(metadata.Object)
                .Verifiable();

            // Act
            html.Label("PropertyName", null, null, metadataProvider.Object);

            // Assert
            metadataProvider.Verify();
        }

        [Fact]
        public void LabelUsesTemplateInfoPrefix()
        {
            // Arrange
            viewData.TemplateInfo.HtmlFieldPrefix = "Prefix";

            // Act
            MvcHtmlString result = html.Label("PropertyName", null, null, metadataProvider.Object);

            // Assert
            Assert.Equal(@"<label for=""Prefix_PropertyName"">PropertyName</label>", result.ToHtmlString());
        }

        [Fact]
        public void LabelUsesLabelTextBeforeMetadata()
        {
            // Arrange
            metadata = new Mock<ModelMetadata>(metadataProvider.Object, null, null, typeof(object), "Custom property name from metadata");
            metadataProvider.Setup(p => p.GetMetadataForType(It.IsAny<Func<object>>(), It.IsAny<Type>()))
                .Returns(metadata.Object);

            //Act
            MvcHtmlString result = html.Label("PropertyName", "Label Text", null, metadataProvider.Object);

            // Assert
            Assert.Equal(@"<label for=""PropertyName"">Label Text</label>", result.ToHtmlString());
        }

        [Theory]
        [PropertyData("HtmlEncodedData_NoHtmlEncode", PropertyType = typeof(EncodedDataSets))]
        public void Label_HtmlEncodes_LabelText(string text, string expectedText)
        {
            // Arrange & Act
            var result =
                html.Label("PropertyName", text, htmlAttributes: null, metadataProvider: metadataProvider.Object);

            // Assert
            Assert.Equal(@"<label for=""PropertyName"">" + expectedText + "</label>", result.ToHtmlString());
        }

        [Fact]
        public void LabelUsesMetadataForDisplayTextWhenLabelTextIsNull()
        {
            // Arrange
            metadata.Setup(m => m.DisplayName).Returns("Custom display name from metadata");

            // Act
            MvcHtmlString result = html.Label("PropertyName", null, null, metadataProvider.Object);

            // Assert
            Assert.Equal(@"<label for=""PropertyName"">Custom display name from metadata</label>", result.ToHtmlString());
        }

        [Fact]
        public void LabelUsesMetadataForPropertyNameWhenDisplayNameIsNull()
        {
            // Arrange
            metadata = new Mock<ModelMetadata>(metadataProvider.Object, null, null, typeof(object), "Custom property name from metadata");
            metadataProvider.Setup(p => p.GetMetadataForType(It.IsAny<Func<object>>(), It.IsAny<Type>()))
                .Returns(metadata.Object);

            // Act
            MvcHtmlString result = html.Label("PropertyName", null, null, metadataProvider.Object);

            // Assert
            Assert.Equal(@"<label for=""PropertyName"">Custom property name from metadata</label>", result.ToHtmlString());
        }

        [Fact]
        public void LabelEmptyDisplayNameReturnsEmptyLabelText()
        {
            // Arrange
            metadata.Setup(m => m.DisplayName).Returns(String.Empty);

            // Act
            MvcHtmlString result = html.Label("PropertyName", null, null, metadataProvider.Object);

            // Assert
            Assert.Equal(String.Empty, result.ToHtmlString());
        }

        [Fact]
        public void LabelWithAnonymousValues()
        {
            // Act
            MvcHtmlString result = html.Label("PropertyName", null, new { @for = "attrFor" }, metadataProvider.Object);

            // Assert
            Assert.Equal(@"<label for=""attrFor"">PropertyName</label>", result.ToHtmlString());
        }

        [Fact]
        public void LabelWithAnonymousValuesAndLabelText()
        {
            // Act
            MvcHtmlString result = html.Label("PropertyName", "Label Text", new { @for = "attrFor" }, metadataProvider.Object);

            // Assert
            Assert.Equal(@"<label for=""attrFor"">Label Text</label>", result.ToHtmlString());
        }

        [Theory]
        [PropertyData("AttributeEncodedData_NoHtmlEncode", PropertyType = typeof(EncodedDataSets))]
        public void LabelWithAnonymousValues_AttributeEncodes_AddedHtmlAttributes(string text, string expectedText)
        {
            // Arrange & Act
            var result = html.Label("PropertyName", "text", new { attribute = text }, metadataProvider.Object);

            // Assert
            Assert.Equal(
                @"<label attribute=""" + expectedText + @""" for=""PropertyName"">text</label>",
                result.ToHtmlString());
        }

        [Fact]
        public void LabelWithTypedAttributes()
        {
            // Arrange
            Dictionary<string, object> htmlAttributes = new Dictionary<string, object>
            {
                { "foo", "bar" },
                { "quux", "baz" }
            };

            // Act
            MvcHtmlString result = html.Label("PropertyName", null, htmlAttributes, metadataProvider.Object);

            // Assert
            Assert.Equal(@"<label foo=""bar"" for=""PropertyName"" quux=""baz"">PropertyName</label>", result.ToHtmlString());
        }

        [Fact]
        public void LabelWithTypedAttributesAndLabelText()
        {
            // Arrange
            Dictionary<string, object> htmlAttributes = new Dictionary<string, object>
            {
                { "foo", "bar" },
                { "quux", "baz" }
            };

            // Act
            MvcHtmlString result = html.Label("PropertyName", "Label Text", htmlAttributes, metadataProvider.Object);

            // Assert
            Assert.Equal(@"<label foo=""bar"" for=""PropertyName"" quux=""baz"">Label Text</label>", result.ToHtmlString());
        }

        // LabelFor tests

        [Fact]
        public void LabelForNullExpressionThrows()
        {
            // Act & Assert
            Assert.ThrowsArgumentNull(
                () => html.LabelFor((Expression<Func<Object, Object>>)null),
                "expression");
        }

        [Fact]
        public void LabelForNonMemberExpressionThrows()
        {
            // Act & Assert
            Assert.Throws<InvalidOperationException>(
                () => html.LabelFor(model => new { foo = "Bar" }, null, null, metadataProvider.Object),
                "Templates can be used only with field access, property access, single-dimension array index, or single-parameter custom indexer expressions.");
        }

        [Fact]
        public void LabelForViewDataNotFound()
        {
            // Arrange
            string unknownKey = "this is a dummy parameter value";

            // Act
            MvcHtmlString result = html.LabelFor(model => unknownKey, null, null, metadataProvider.Object);

            // Assert
            Assert.Equal(@"<label for=""unknownKey"">unknownKey</label>", result.ToHtmlString());
        }

        [Fact]
        public void LabelForUsesTemplateInfoPrefix()
        {
            // Arrange
            viewData.TemplateInfo.HtmlFieldPrefix = "Prefix";
            string unknownKey = "this is a dummy parameter value";

            // Act
            MvcHtmlString result = html.LabelFor(model => unknownKey, null, null, metadataProvider.Object);

            // Assert
            Assert.Equal(@"<label for=""Prefix_unknownKey"">unknownKey</label>", result.ToHtmlString());
        }

        [Fact]
        public void LabelForUsesLabelTextBeforeModelMetadata()
        {
            // Arrange
            metadata.Setup(m => m.DisplayName).Returns("Custom display name from metadata");
            string unknownKey = "this is a dummy parameter value";

            //Act
            MvcHtmlString result = html.LabelFor(model => unknownKey, "Label Text", null, metadataProvider.Object);

            // Assert
            Assert.Equal(@"<label for=""unknownKey"">Label Text</label>", result.ToHtmlString());
        }

        [Theory]
        [PropertyData("HtmlEncodedData_NoHtmlEncode", PropertyType = typeof(EncodedDataSets))]
        public void LabelFor_HtmlEncodes_LabelText(string text, string expectedText)
        {
            // Arrange
            var dummy = "this is a dummy parameter value";

            // Arrange & Act
            var result =
                html.LabelFor(m => dummy, text, htmlAttributes: null, metadataProvider: metadataProvider.Object);

            // Assert
            Assert.Equal(@"<label for=""dummy"">" + expectedText + "</label>", result.ToHtmlString());
        }

        [Fact]
        public void LabelForUsesModelMetadata()
        {
            // Arrange
            metadata.Setup(m => m.DisplayName).Returns("Custom display name from metadata");
            string unknownKey = "this is a dummy parameter value";

            // Act
            MvcHtmlString result = html.LabelFor(model => unknownKey, null, null, metadataProvider.Object);

            // Assert
            Assert.Equal(@"<label for=""unknownKey"">Custom display name from metadata</label>", result.ToHtmlString());
        }

        [Fact]
        public void LabelForEmptyDisplayNameReturnsEmptyLabelText()
        {
            // Arrange
            metadata.Setup(m => m.DisplayName).Returns(String.Empty);
            string unknownKey = "this is a dummy parameter value";

            // Act
            MvcHtmlString result = html.LabelFor(model => unknownKey, null, null, metadataProvider.Object);

            // Assert
            Assert.Equal(String.Empty, result.ToHtmlString());
        }

        [Fact]
        public void LabelForWithAnonymousValues()
        {
            //Arrange
            string unknownKey = "this is a dummy parameter value";

            // Act
            MvcHtmlString result = html.LabelFor(model => unknownKey, null, new { @for = "attrFor" }, metadataProvider.Object);

            // Assert
            Assert.Equal(@"<label for=""attrFor"">unknownKey</label>", result.ToHtmlString());
        }

        [Theory]
        [PropertyData("AttributeEncodedData_NoHtmlEncode", PropertyType = typeof(EncodedDataSets))]
        public void LabeForlWithAnonymousValues_AttributeEncodes_AddedHtmlAttributes(string text, string expectedText)
        {
            // Arrange
            var dummy = "this is a dummy parameter value";

            // Act
            var result =
                html.LabelFor(m => dummy, "text", new { attribute = text }, metadataProvider.Object);

            // Assert
            Assert.Equal(
                @"<label attribute=""" + expectedText + @""" for=""dummy"">text</label>",
                result.ToHtmlString());
        }

        [Fact]
        public void LabelForWithAnonymousValuesAndLabelText()
        {
            //Arrange
            string unknownKey = "this is a dummy parameter value";

            // Act
            MvcHtmlString result = html.LabelFor(model => unknownKey, "Label Text", new { @for = "attrFor" }, metadataProvider.Object);

            // Assert
            Assert.Equal(@"<label for=""attrFor"">Label Text</label>", result.ToHtmlString());
        }

        [Fact]
        public void LabelForWithTypedAttributes()
        {
            //Arrange
            string unknownKey = "this is a dummy parameter value";

            Dictionary<string, object> htmlAttributes = new Dictionary<string, object>
            {
                { "foo", "bar" },
                { "quux", "baz" }
            };

            // Act
            MvcHtmlString result = html.LabelFor(model => unknownKey, null, htmlAttributes, metadataProvider.Object);

            // Assert
            Assert.Equal(@"<label foo=""bar"" for=""unknownKey"" quux=""baz"">unknownKey</label>", result.ToHtmlString());
        }

        [Fact]
        public void LabelForWithTypedAttributesAndLabelText()
        {
            //Arrange
            string unknownKey = "this is a dummy parameter value";

            Dictionary<string, object> htmlAttributes = new Dictionary<string, object>
            {
                { "foo", "bar" },
                { "quux", "baz" }
            };

            // Act
            MvcHtmlString result = html.LabelFor(model => unknownKey, "Label Text", htmlAttributes, metadataProvider.Object);

            // Assert
            Assert.Equal(@"<label foo=""bar"" for=""unknownKey"" quux=""baz"">Label Text</label>", result.ToHtmlString());
        }

        [Fact]
        public void LabelForWithNestedClass()
        { // Dev10 Bug #936323
            // Arrange
            HtmlHelper<NestedProduct> html = new HtmlHelper<NestedProduct>(viewContext.Object, viewDataContainer.Object);

            // Act
            MvcHtmlString result = html.LabelFor(nested => nested.product.Id, null, null, metadataProvider.Object);

            //Assert
            Assert.Equal(@"<label for=""product_Id"">Id</label>", result.ToHtmlString());
        }

        [Fact]
        public void LabelForWithArrayExpression()
        { // Dev10 Bug #905780
            // Arrange
            HtmlHelper<Cart> html = new HtmlHelper<Cart>(viewContext.Object, viewDataContainer.Object);

            // Act
            MvcHtmlString result = html.LabelFor(cart => cart.Products[0].Id, null, null, metadataProvider.Object);

            // Assert
            Assert.Equal(@"<label for=""Products_0__Id"">Id</label>", result.ToHtmlString());
        }

        private class Product
        {
            public int Id { get; set; }
        }

        private class Cart
        {
            public Product[] Products { get; set; }
        }

        private class NestedProduct
        {
            public Product product = new Product();
        }

        // LabelForModel tests

        [Fact]
        public void LabelForModelUsesLabelTextBeforeModelMetadata()
        {
            // Arrange
            viewData.ModelMetadata = metadata.Object;
            viewData.TemplateInfo.HtmlFieldPrefix = "Prefix";
            metadata.Setup(m => m.DisplayName).Returns("Custom display name from metadata");

            // Act
            MvcHtmlString result = html.LabelForModel("Label Text");

            // Assert
            Assert.Equal(@"<label for=""Prefix"">Label Text</label>", result.ToHtmlString());
        }

        [Fact]
        public void LabelForModelUsesModelMetadata()
        {
            // Arrange
            viewData.ModelMetadata = metadata.Object;
            viewData.TemplateInfo.HtmlFieldPrefix = "Prefix";
            metadata.Setup(m => m.DisplayName).Returns("Custom display name from metadata");

            // Act
            MvcHtmlString result = html.LabelForModel();

            // Assert
            Assert.Equal(@"<label for=""Prefix"">Custom display name from metadata</label>", result.ToHtmlString());
        }

        [Fact]
        public void LabelForModelWithAnonymousValues()
        {
            //Arrange
            viewData.ModelMetadata = metadata.Object;
            viewData.TemplateInfo.HtmlFieldPrefix = "Prefix";
            metadata.Setup(m => m.DisplayName).Returns("Custom display name from metadata");

            // Act
            MvcHtmlString result = html.LabelForModel(new { @for = "attrFor" });

            // Assert
            Assert.Equal(@"<label for=""attrFor"">Custom display name from metadata</label>", result.ToHtmlString());
        }

        [Fact]
        public void LabelForModelWithAnonymousValuesAndLabelText()
        {
            //Arrange
            viewData.ModelMetadata = metadata.Object;
            viewData.TemplateInfo.HtmlFieldPrefix = "Prefix";
            metadata.Setup(m => m.DisplayName).Returns("Custom display name from metadata");

            // Act
            MvcHtmlString result = html.LabelForModel("Label Text", new { @for = "attrFor" });

            // Assert
            Assert.Equal(@"<label for=""attrFor"">Label Text</label>", result.ToHtmlString());
        }

        [Fact]
        public void LabelForModelWithTypedAttributes()
        {
            //Arrange
            viewData.ModelMetadata = metadata.Object;
            viewData.TemplateInfo.HtmlFieldPrefix = "Prefix";
            metadata.Setup(m => m.DisplayName).Returns("Custom display name from metadata");

            Dictionary<string, object> htmlAttributes = new Dictionary<string, object>
            {
                { "foo", "bar" },
                { "quux", "baz" }
            };

            // Act
            MvcHtmlString result = html.LabelForModel(htmlAttributes);

            // Assert
            Assert.Equal(@"<label foo=""bar"" for=""Prefix"" quux=""baz"">Custom display name from metadata</label>", result.ToHtmlString());
        }

        [Fact]
        public void LabelForModelWithTypedAttributesAndLabelText()
        {
            //Arrange
            viewData.ModelMetadata = metadata.Object;
            viewData.TemplateInfo.HtmlFieldPrefix = "Prefix";
            metadata.Setup(m => m.DisplayName).Returns("Custom display name from metadata");

            Dictionary<string, object> htmlAttributes = new Dictionary<string, object>
            {
                { "foo", "bar" },
                { "quux", "baz" }
            };

            // Act
            MvcHtmlString result = html.LabelForModel("Label Text", htmlAttributes);

            // Assert
            Assert.Equal(@"<label foo=""bar"" for=""Prefix"" quux=""baz"">Label Text</label>", result.ToHtmlString());
        }
    }
}
