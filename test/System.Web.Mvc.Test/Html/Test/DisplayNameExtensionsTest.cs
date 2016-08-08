// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq.Expressions;
using Microsoft.TestCommon;
using Microsoft.Web.UnitTestUtil;
using Moq;

namespace System.Web.Mvc.Html.Test
{
    public class DisplayNameExtensionsTest
    {
        [Fact]
        public void DisplayNameNullExpressionThrows()
        {
            // Act & Assert
            Assert.ThrowsArgumentNull(
                () => MvcHelper.GetHtmlHelper().DisplayName(expression: null),
                "expression");
        }

        [Fact]
        public void DisplayNameWithNoModelMetadataDisplayNameOverride()
        {
            // Act
            MvcHtmlString result = MvcHelper.GetHtmlHelper().DisplayNameInternal("PropertyName", new MetadataHelper().MetadataProvider.Object);

            // Assert
            Assert.Equal("PropertyName", result.ToHtmlString());
        }

        [Fact]
        public void DisplayNameUsesMetadataForDisplayText()
        {
            // Arrange
            MetadataHelper metadataHelper = new MetadataHelper();
            metadataHelper.Metadata.Setup(m => m.DisplayName).Returns("Custom display name from metadata");

            // Act
            MvcHtmlString result = MvcHelper.GetHtmlHelper().DisplayNameInternal("PropertyName", metadataHelper.MetadataProvider.Object);

            // Assert
            Assert.Equal("Custom display name from metadata", result.ToHtmlString());
        }

        private sealed class Model
        {
            public string PropertyName { get; set; }
        }

        [Fact]
        public void DisplayNameConsultsMetadataProviderForMetadataAboutProperty()
        {
            // Arrange
            Model model = new Model { PropertyName = "propertyValue" };

            ViewDataDictionary viewData = new ViewDataDictionary();
            Mock<ViewContext> viewContext = new Mock<ViewContext>();
            viewContext.Setup(c => c.ViewData).Returns(viewData);

            Mock<IViewDataContainer> viewDataContainer = new Mock<IViewDataContainer>();
            viewDataContainer.Setup(c => c.ViewData).Returns(viewData);

            HtmlHelper<Model> html = new HtmlHelper<Model>(viewContext.Object, viewDataContainer.Object);
            viewData.Model = model;

            MetadataHelper metadataHelper = new MetadataHelper();

            metadataHelper.MetadataProvider.Setup(p => p.GetMetadataForProperty(It.IsAny<Func<object>>(), typeof(Model), "PropertyName"))
                .Returns(metadataHelper.Metadata.Object)
                .Verifiable();

            // Act
            html.DisplayNameInternal("PropertyName", metadataHelper.MetadataProvider.Object);

            // Assert
            metadataHelper.MetadataProvider.Verify();
        }

        [Fact]
        public void DisplayNameUsesMetadataForPropertyName()
        {
            // Arrange
            MetadataHelper metadataHelper = new MetadataHelper();

            metadataHelper.Metadata = new Mock<ModelMetadata>(metadataHelper.MetadataProvider.Object, null, null, typeof(object), "Custom property name from metadata");
            metadataHelper.MetadataProvider.Setup(p => p.GetMetadataForType(It.IsAny<Func<object>>(), It.IsAny<Type>()))
                .Returns(metadataHelper.Metadata.Object);

            // Act
            MvcHtmlString result = MvcHelper.GetHtmlHelper().DisplayNameInternal("PropertyName", metadataHelper.MetadataProvider.Object);

            // Assert
            Assert.Equal("Custom property name from metadata", result.ToHtmlString());
        }

        [Fact]
        public void DisplayNameForNullExpressionThrows()
        {
            // Act & Assert
            Assert.ThrowsArgumentNull(
                () => MvcHelper.GetHtmlHelper().DisplayNameFor((Expression<Func<Object, Object>>)null),
                "expression");

            Assert.ThrowsArgumentNull(
                () => GetEnumerableHtmlHelper().DisplayNameFor((Expression<Func<Foo, Object>>)null),
                "expression");
        }

        [Fact]
        public void DisplayNameForNonMemberExpressionThrows()
        {
            // Act & Assert
            Assert.Throws<InvalidOperationException>(
                () => MvcHelper.GetHtmlHelper().DisplayNameFor(model => new { foo = "Bar" }),
                "Templates can be used only with field access, property access, single-dimension array index, or single-parameter custom indexer expressions.");

            Assert.Throws<InvalidOperationException>(
                () => GetEnumerableHtmlHelper().DisplayNameFor((Expression<Func<IEnumerable<Foo>, object>>)(model => new { foo = "Bar" })),
                "Templates can be used only with field access, property access, single-dimension array index, or single-parameter custom indexer expressions.");
        }

        [Fact]
        public void DisplayNameForWithNoModelMetadataDisplayNameOverride()
        {
            // Arrange
            string unknownKey = "this is a dummy parameter value";

            // Act
            MvcHtmlString result = MvcHelper.GetHtmlHelper().DisplayNameFor(model => unknownKey);
            MvcHtmlString enumerableResult = GetEnumerableHtmlHelper().DisplayNameFor((Expression<Func<IEnumerable<Foo>, string>>)(model => unknownKey));

            // Assert
            Assert.Equal("unknownKey", result.ToHtmlString());
            Assert.Equal("unknownKey", enumerableResult.ToHtmlString());
        }

        [Fact]
        public void DisplayNameForUsesModelMetadata()
        {
            // Arrange
            MetadataHelper metadataHelper = new MetadataHelper();

            metadataHelper.Metadata.Setup(m => m.DisplayName).Returns("Custom display name from metadata");
            string unknownKey = "this is a dummy parameter value";

            // Act
            MvcHtmlString result = MvcHelper.GetHtmlHelper().DisplayNameForInternal(model => unknownKey, metadataHelper.MetadataProvider.Object);
            MvcHtmlString enumerableResult = GetEnumerableHtmlHelper().DisplayNameForInternal(model => model.Bar, metadataHelper.MetadataProvider.Object);

            // Assert
            Assert.Equal("Custom display name from metadata", result.ToHtmlString());
            Assert.Equal("Custom display name from metadata", enumerableResult.ToHtmlString());
        }

        [Fact]
        public void DisplayNameForEmptyDisplayNameReturnsEmptyName()
        {
            // Arrange
            MetadataHelper metadataHelper = new MetadataHelper();

            metadataHelper.Metadata.Setup(m => m.DisplayName).Returns(String.Empty);
            string unknownKey = "this is a dummy parameter value";

            // Act
            MvcHtmlString result = MvcHelper.GetHtmlHelper().DisplayNameForInternal(model => unknownKey, metadataHelper.MetadataProvider.Object);
            MvcHtmlString enumerableResult = GetEnumerableHtmlHelper().DisplayNameForInternal(model => model.Bar, metadataHelper.MetadataProvider.Object);

            // Assert
            Assert.Equal(String.Empty, result.ToHtmlString());
            Assert.Equal(String.Empty, enumerableResult.ToHtmlString());
        }

        [Fact]
        public void DisplayNameForModelUsesModelMetadata()
        {
            // Arrange
            ViewDataDictionary viewData = new ViewDataDictionary();
            Mock<ModelMetadata> metadata = new MetadataHelper().Metadata;
            metadata.Setup(m => m.DisplayName).Returns("Custom display name from metadata");

            viewData.ModelMetadata = metadata.Object;
            viewData.TemplateInfo.HtmlFieldPrefix = "Prefix";

            // Act
            MvcHtmlString result = MvcHelper.GetHtmlHelper(viewData).DisplayNameForModel();

            // Assert
            Assert.Equal("Custom display name from metadata", result.ToHtmlString());
        }

        [Fact]
        public void DisplayNameForWithNestedClass()
        {
            // Arrange
            ViewDataDictionary viewData = new ViewDataDictionary();
            Mock<ViewContext> viewContext = new Mock<ViewContext>();
            viewContext.Setup(c => c.ViewData).Returns(viewData);

            Mock<IViewDataContainer> viewDataContainer = new Mock<IViewDataContainer>();
            viewDataContainer.Setup(c => c.ViewData).Returns(viewData);

            HtmlHelper<NestedProduct> html = new HtmlHelper<NestedProduct>(viewContext.Object, viewDataContainer.Object);

            // Act
            MvcHtmlString result = html.DisplayNameForInternal(nested => nested.product.Id, new MetadataHelper().MetadataProvider.Object);

            //Assert
            Assert.Equal("Id", result.ToHtmlString());
        }

        [Theory]
        [PropertyData("HtmlEncodedData", PropertyType = typeof(EncodedDataSets))]
        public void DisplayNameHelpers_EncodeValue(string text, bool htmlEncode, string expectedResult)
        {
            // Arrange
            var viewData = new ViewDataDictionary<Cart>(model: null);
            viewData.ModelMetadata.DisplayName = text;
            viewData.ModelMetadata.HtmlEncode = htmlEncode;
            var helper = MvcHelper.GetHtmlHelper(viewData);

            // Act
            var nameResult = helper.DisplayName("").ToHtmlString();
            var nameForResult = helper.DisplayNameFor(m => m).ToHtmlString();
            var nameForModelResult = helper.DisplayNameForModel().ToHtmlString();

            // Assert
            Assert.Equal(expectedResult, nameResult);
            Assert.Equal(expectedResult, nameForResult);
            Assert.Equal(expectedResult, nameForModelResult);
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

        private sealed class Foo
        {
            public string Bar { get; set; }
        }

        private static HtmlHelper<IEnumerable<Foo>> GetEnumerableHtmlHelper()
        {
            return MvcHelper.GetHtmlHelper(new ViewDataDictionary<IEnumerable<Foo>>());
        }

        private sealed class MetadataHelper
        {
            public Mock<ModelMetadata> Metadata { get; set; }
            public Mock<ModelMetadataProvider> MetadataProvider { get; set; }

            public MetadataHelper()
            {
                MetadataProvider = new Mock<ModelMetadataProvider>();
                Metadata = new Mock<ModelMetadata>(MetadataProvider.Object, null, null, typeof(object), null);

                MetadataProvider.Setup(p => p.GetMetadataForProperties(It.IsAny<object>(), It.IsAny<Type>()))
                    .Returns(new ModelMetadata[0]);
                MetadataProvider.Setup(p => p.GetMetadataForProperty(It.IsAny<Func<object>>(), It.IsAny<Type>(), It.IsAny<string>()))
                    .Returns(Metadata.Object);
                MetadataProvider.Setup(p => p.GetMetadataForType(It.IsAny<Func<object>>(), It.IsAny<Type>()))
                    .Returns(Metadata.Object);
            }
        }
    }
}
