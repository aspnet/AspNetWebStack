// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.Data.Objects.DataClasses;
using System.Globalization;
using System.IO;
using System.Web.UI.WebControls;
using Microsoft.TestCommon;
using Microsoft.Web.UnitTestUtil;
using Moq;

namespace System.Web.Mvc.Html.Test
{
    [Xunit.Collection("Uses ScopeStorage or ViewEngines.Engines")] // Uses ModelMetadataProviders.Current
    public class DefaultDisplayTemplatesTest
    {
        // BooleanTemplate

        [Fact]
        public void BooleanTemplateTests()
        {
            // Boolean values

            Assert.Equal(
                "<input checked=\"checked\" class=\"check-box\" disabled=\"disabled\" type=\"checkbox\" />",
                DefaultDisplayTemplates.BooleanTemplate(MakeHtmlHelper<bool>(true)));

            Assert.Equal(
                "<input class=\"check-box\" disabled=\"disabled\" type=\"checkbox\" />",
                DefaultDisplayTemplates.BooleanTemplate(MakeHtmlHelper<bool>(false)));

            Assert.Equal(
                "<input class=\"check-box\" disabled=\"disabled\" type=\"checkbox\" />",
                DefaultDisplayTemplates.BooleanTemplate(MakeHtmlHelper<bool>(null)));

            // Nullable<Boolean> values

            Assert.Equal(
                "<select class=\"tri-state list-box\" disabled=\"disabled\"><option value=\"\">Not Set</option><option selected=\"selected\" value=\"true\">True</option><option value=\"false\">False</option></select>",
                DefaultDisplayTemplates.BooleanTemplate(MakeHtmlHelper<Nullable<bool>>(true)));

            Assert.Equal(
                "<select class=\"tri-state list-box\" disabled=\"disabled\"><option value=\"\">Not Set</option><option value=\"true\">True</option><option selected=\"selected\" value=\"false\">False</option></select>",
                DefaultDisplayTemplates.BooleanTemplate(MakeHtmlHelper<Nullable<bool>>(false)));

            Assert.Equal(
                "<select class=\"tri-state list-box\" disabled=\"disabled\"><option selected=\"selected\" value=\"\">Not Set</option><option value=\"true\">True</option><option value=\"false\">False</option></select>",
                DefaultDisplayTemplates.BooleanTemplate(MakeHtmlHelper<Nullable<bool>>(null)));
        }

        // CollectionTemplate

        private static string CollectionSpyCallback(HtmlHelper html, ModelMetadata metadata, string htmlFieldName, string templateName, DataBoundControlMode mode, object additionalViewData)
        {
            return String.Format(CultureInfo.InvariantCulture,
                                 Environment.NewLine + "Model = {0}, ModelType = {1}, PropertyName = {2}, HtmlFieldName = {3}, TemplateName = {4}, Mode = {5}, TemplateInfo.HtmlFieldPrefix = {6}, AdditionalViewData = {7}",
                                 metadata.Model ?? "(null)",
                                 metadata.ModelType == null ? "(null)" : metadata.ModelType.FullName,
                                 metadata.PropertyName ?? "(null)",
                                 htmlFieldName == String.Empty ? "(empty)" : htmlFieldName ?? "(null)",
                                 templateName ?? "(null)",
                                 mode,
                                 html.ViewContext.ViewData.TemplateInfo.HtmlFieldPrefix,
                                 AnonymousObject.Inspect(additionalViewData));
        }

        [Fact]
        public void CollectionTemplateWithNullModel()
        {
            // Arrange
            HtmlHelper html = MakeHtmlHelper<object>(null);

            // Act
            string result = DefaultDisplayTemplates.CollectionTemplate(html, CollectionSpyCallback);

            // Assert
            Assert.Equal(String.Empty, result);
        }

        [Fact]
        public void CollectionTemplateNonEnumerableModelThrows()
        {
            // Arrange
            HtmlHelper html = MakeHtmlHelper<object>(new object());

            // Act & Assert
            Assert.Throws<InvalidOperationException>(
                () => DefaultDisplayTemplates.CollectionTemplate(html, CollectionSpyCallback),
                "The Collection template was used with an object of type 'System.Object', which does not implement System.IEnumerable."
                );
        }

        [Fact]
        public void CollectionTemplateWithSingleItemCollectionWithoutPrefix()
        {
            // Arrange
            HtmlHelper html = MakeHtmlHelper<List<string>>(new List<string> { "foo" });
            html.ViewContext.ViewData.TemplateInfo.HtmlFieldPrefix = null;

            // Act
            string result = DefaultDisplayTemplates.CollectionTemplate(html, CollectionSpyCallback);

            // Assert
            Assert.Equal(
                Environment.NewLine
              + "Model = foo, ModelType = System.String, PropertyName = (null), HtmlFieldName = [0], TemplateName = (null), Mode = ReadOnly, TemplateInfo.HtmlFieldPrefix = , AdditionalViewData = (null)",
                result);
        }

        [Fact]
        public void CollectionTemplateWithSingleItemCollectionWithPrefix()
        {
            // Arrange
            HtmlHelper html = MakeHtmlHelper<List<string>>(new List<string> { "foo" });
            html.ViewContext.ViewData.TemplateInfo.HtmlFieldPrefix = "ModelProperty";

            // Act
            string result = DefaultDisplayTemplates.CollectionTemplate(html, CollectionSpyCallback);

            // Assert
            Assert.Equal(
                Environment.NewLine
              + "Model = foo, ModelType = System.String, PropertyName = (null), HtmlFieldName = ModelProperty[0], TemplateName = (null), Mode = ReadOnly, TemplateInfo.HtmlFieldPrefix = , AdditionalViewData = (null)",
                result);
        }

        [Fact]
        public void CollectionTemplateWithMultiItemCollection()
        {
            // Arrange
            HtmlHelper html = MakeHtmlHelper<List<string>>(new List<string> { "foo", "bar", "baz" });
            html.ViewContext.ViewData.TemplateInfo.HtmlFieldPrefix = null;

            // Act
            string result = DefaultDisplayTemplates.CollectionTemplate(html, CollectionSpyCallback);

            // Assert
            Assert.Equal(
                Environment.NewLine
              + "Model = foo, ModelType = System.String, PropertyName = (null), HtmlFieldName = [0], TemplateName = (null), Mode = ReadOnly, TemplateInfo.HtmlFieldPrefix = , AdditionalViewData = (null)" + Environment.NewLine
              + "Model = bar, ModelType = System.String, PropertyName = (null), HtmlFieldName = [1], TemplateName = (null), Mode = ReadOnly, TemplateInfo.HtmlFieldPrefix = , AdditionalViewData = (null)" + Environment.NewLine
              + "Model = baz, ModelType = System.String, PropertyName = (null), HtmlFieldName = [2], TemplateName = (null), Mode = ReadOnly, TemplateInfo.HtmlFieldPrefix = , AdditionalViewData = (null)",
                result);
        }

        [Fact]
        public void CollectionTemplateNullITemInWeaklyTypedCollectionUsesModelTypeOfString()
        {
            // Arrange
            HtmlHelper html = MakeHtmlHelper<ArrayList>(new ArrayList { null });
            html.ViewContext.ViewData.TemplateInfo.HtmlFieldPrefix = null;

            // Act
            string result = DefaultDisplayTemplates.CollectionTemplate(html, CollectionSpyCallback);

            // Assert
            Assert.Equal(
                Environment.NewLine
              + "Model = (null), ModelType = System.String, PropertyName = (null), HtmlFieldName = [0], TemplateName = (null), Mode = ReadOnly, TemplateInfo.HtmlFieldPrefix = , AdditionalViewData = (null)",
                result);
        }

        [Fact]
        public void CollectionTemplateNullItemInStronglyTypedCollectionUsesModelTypeFromIEnumerable()
        {
            // Arrange
            HtmlHelper html = MakeHtmlHelper<List<IHttpHandler>>(new List<IHttpHandler> { null });
            html.ViewContext.ViewData.TemplateInfo.HtmlFieldPrefix = null;

            // Act
            string result = DefaultDisplayTemplates.CollectionTemplate(html, CollectionSpyCallback);

            // Assert
            Assert.Equal(
                Environment.NewLine
              + "Model = (null), ModelType = System.Web.IHttpHandler, PropertyName = (null), HtmlFieldName = [0], TemplateName = (null), Mode = ReadOnly, TemplateInfo.HtmlFieldPrefix = , AdditionalViewData = (null)",
                result);
        }

        [Fact]
        public void CollectionTemplateUsesRealObjectTypes()
        {
            // Arrange
            HtmlHelper html = MakeHtmlHelper<List<object>>(new List<object> { 1, 2.3, "Hello World" });
            html.ViewContext.ViewData.TemplateInfo.HtmlFieldPrefix = null;

            // Act
            string result = DefaultDisplayTemplates.CollectionTemplate(html, CollectionSpyCallback);

            // Assert
            Assert.Equal(
                Environment.NewLine
              + "Model = 1, ModelType = System.Int32, PropertyName = (null), HtmlFieldName = [0], TemplateName = (null), Mode = ReadOnly, TemplateInfo.HtmlFieldPrefix = , AdditionalViewData = (null)" + Environment.NewLine
              + "Model = 2.3, ModelType = System.Double, PropertyName = (null), HtmlFieldName = [1], TemplateName = (null), Mode = ReadOnly, TemplateInfo.HtmlFieldPrefix = , AdditionalViewData = (null)" + Environment.NewLine
              + "Model = Hello World, ModelType = System.String, PropertyName = (null), HtmlFieldName = [2], TemplateName = (null), Mode = ReadOnly, TemplateInfo.HtmlFieldPrefix = , AdditionalViewData = (null)",
                result);
        }

        [Fact]
        public void CollectionTemplateNullItemInCollectionOfNullableValueTypesDoesNotDiscardNullable()
        {
            // Arrange
            HtmlHelper html = MakeHtmlHelper<List<int?>>(new List<int?> { 1, null, 2 });
            html.ViewContext.ViewData.TemplateInfo.HtmlFieldPrefix = null;

            // Act
            string result = DefaultDisplayTemplates.CollectionTemplate(html, CollectionSpyCallback);

            // Assert
            Assert.Equal(
                Environment.NewLine
              + "Model = 1, ModelType = System.Nullable`1[[System.Int32, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]], PropertyName = (null), HtmlFieldName = [0], TemplateName = (null), Mode = ReadOnly, TemplateInfo.HtmlFieldPrefix = , AdditionalViewData = (null)" + Environment.NewLine
              + "Model = (null), ModelType = System.Nullable`1[[System.Int32, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]], PropertyName = (null), HtmlFieldName = [1], TemplateName = (null), Mode = ReadOnly, TemplateInfo.HtmlFieldPrefix = , AdditionalViewData = (null)" + Environment.NewLine
              + "Model = 2, ModelType = System.Nullable`1[[System.Int32, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]], PropertyName = (null), HtmlFieldName = [2], TemplateName = (null), Mode = ReadOnly, TemplateInfo.HtmlFieldPrefix = , AdditionalViewData = (null)",
                result);
        }

        // DecimalTemplate

        [Fact]
        public void DecimalTemplateTests()
        {
            Assert.Equal(
                String.Format(CultureInfo.CurrentCulture, "{0:0.00}", 12.35M),
                DefaultDisplayTemplates.DecimalTemplate(MakeHtmlHelper<decimal>(12.3456M)));

            Assert.Equal(
                "Formatted Value",
                DefaultDisplayTemplates.DecimalTemplate(MakeHtmlHelper<decimal>(12.3456M, "Formatted Value")));

            Assert.Equal(
                "&lt;script&gt;alert(&#39;XSS!&#39;)&lt;/script&gt;",
                DefaultDisplayTemplates.DecimalTemplate(MakeHtmlHelper<decimal>(12.3456M, "<script>alert('XSS!')</script>")));
        }

        // EmailAddressTemplate

        [Fact]
        public void EmailAddressTemplateTests()
        {
            Assert.Equal(
                "<a href=\"mailto:foo@bar.com\">foo@bar.com</a>",
                DefaultDisplayTemplates.EmailAddressTemplate(MakeHtmlHelper<string>("foo@bar.com")));

            Assert.Equal(
                "<a href=\"mailto:foo@bar.com\">The FooBar User</a>",
                DefaultDisplayTemplates.EmailAddressTemplate(MakeHtmlHelper<string>("foo@bar.com", "The FooBar User")));

            Assert.Equal(
                "<a href=\"mailto:&lt;script>alert(&#39;XSS!&#39;)&lt;/script>\">&lt;script&gt;alert(&#39;XSS!&#39;)&lt;/script&gt;</a>",
                DefaultDisplayTemplates.EmailAddressTemplate(MakeHtmlHelper<string>("<script>alert('XSS!')</script>")));

            Assert.Equal(
                "<a href=\"mailto:&lt;script>alert(&#39;XSS!&#39;)&lt;/script>\">&lt;b&gt;Encode me!&lt;/b&gt;</a>",
                DefaultDisplayTemplates.EmailAddressTemplate(MakeHtmlHelper<string>("<script>alert('XSS!')</script>", "<b>Encode me!</b>")));
        }

        // HiddenInputTemplate

        [Fact]
        public void HiddenInputTemplateTests()
        {
            Assert.Equal(
                "Hidden Value",
                DefaultDisplayTemplates.HiddenInputTemplate(MakeHtmlHelper<string>("Hidden Value")));

            Assert.Equal(
                "&lt;b&gt;Encode me!&lt;/b&gt;",
                DefaultDisplayTemplates.HiddenInputTemplate(MakeHtmlHelper<string>("<script>alert('XSS!')</script>", "<b>Encode me!</b>")));

            var helperWithInvisibleHtml = MakeHtmlHelper<string>("<script>alert('XSS!')</script>", "<b>Encode me!</b>");
            helperWithInvisibleHtml.ViewData.ModelMetadata.HideSurroundingHtml = true;
            Assert.Equal(
                String.Empty,
                DefaultDisplayTemplates.HiddenInputTemplate(helperWithInvisibleHtml));
        }

        // HtmlTemplate

        [Fact]
        public void HtmlTemplateTests()
        {
            Assert.Equal(
                "Hello, world!",
                DefaultDisplayTemplates.HtmlTemplate(MakeHtmlHelper<string>("", "Hello, world!")));

            Assert.Equal(
                "<b>Hello, world!</b>",
                DefaultDisplayTemplates.HtmlTemplate(MakeHtmlHelper<string>("", "<b>Hello, world!</b>")));
        }

        // ObjectTemplate

        private static string SpyCallback(HtmlHelper html, ModelMetadata metadata, string htmlFieldName, string templateName, DataBoundControlMode mode, object additionalViewData)
        {
            return String.Format("Model = {0}, ModelType = {1}, PropertyName = {2}, HtmlFieldName = {3}, TemplateName = {4}, Mode = {5}, AdditionalViewData = {6}",
                                 metadata.Model ?? "(null)",
                                 metadata.ModelType == null ? "(null)" : metadata.ModelType.FullName,
                                 metadata.PropertyName ?? "(null)",
                                 htmlFieldName == String.Empty ? "(empty)" : htmlFieldName ?? "(null)",
                                 templateName ?? "(null)",
                                 mode,
                                 AnonymousObject.Inspect(additionalViewData));
        }

        class ObjectTemplateModel
        {
            public ObjectTemplateModel()
            {
                ComplexInnerModel = new object();
            }

            public string Property1 { get; set; }
            public string Property2 { get; set; }
            public object ComplexInnerModel { get; set; }
        }

        [Fact]
        public void ObjectTemplateDisplaysSimplePropertiesOnObjectByDefault()
        {
            string expected =
                "<div class=\"display-label\">Property1</div>" + Environment.NewLine
              + "<div class=\"display-field\">Model = p1, ModelType = System.String, PropertyName = Property1, HtmlFieldName = Property1, TemplateName = (null), Mode = ReadOnly, AdditionalViewData = (null)</div>" + Environment.NewLine
              + "<div class=\"display-label\">Property2</div>" + Environment.NewLine
              + "<div class=\"display-field\">Model = (null), ModelType = System.String, PropertyName = Property2, HtmlFieldName = Property2, TemplateName = (null), Mode = ReadOnly, AdditionalViewData = (null)</div>" + Environment.NewLine;

            // Arrange
            ObjectTemplateModel model = new ObjectTemplateModel { Property1 = "p1", Property2 = null };
            HtmlHelper html = MakeHtmlHelper<ObjectTemplateModel>(model);

            // Act
            string result = DefaultDisplayTemplates.ObjectTemplate(html, SpyCallback);

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void ObjectTemplateWithDisplayNameMetadata()
        {
            string expected =
                "<div class=\"display-field\">Model = (null), ModelType = System.String, PropertyName = Property1, HtmlFieldName = Property1, TemplateName = (null), Mode = ReadOnly, AdditionalViewData = (null)</div>" + Environment.NewLine
              + "<div class=\"display-label\">Custom display name</div>" + Environment.NewLine
              + "<div class=\"display-field\">Model = (null), ModelType = System.String, PropertyName = Property2, HtmlFieldName = Property2, TemplateName = (null), Mode = ReadOnly, AdditionalViewData = (null)</div>" + Environment.NewLine;

            // Arrange
            ObjectTemplateModel model = new ObjectTemplateModel();
            HtmlHelper html = MakeHtmlHelper<ObjectTemplateModel>(model);
            Mock<ModelMetadataProvider> provider = new Mock<ModelMetadataProvider>();
            Func<object> accessor = () => model;
            Mock<ModelMetadata> metadata = new Mock<ModelMetadata>(provider.Object, null, accessor, typeof(ObjectTemplateModel), null);
            ModelMetadata prop1Metadata = new ModelMetadata(provider.Object, typeof(ObjectTemplateModel), null, typeof(string), "Property1") { DisplayName = String.Empty };
            ModelMetadata prop2Metadata = new ModelMetadata(provider.Object, typeof(ObjectTemplateModel), null, typeof(string), "Property2") { DisplayName = "Custom display name" };
            html.ViewData.ModelMetadata = metadata.Object;
            metadata.Setup(p => p.Properties).Returns(() => new[] { prop1Metadata, prop2Metadata });

            // Act
            string result = DefaultDisplayTemplates.ObjectTemplate(html, SpyCallback);

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void ObjectTemplateWithShowForDisplayMetadata()
        {
            string expected =
                "<div class=\"display-label\">Property1</div>" + Environment.NewLine
              + "<div class=\"display-field\">Model = (null), ModelType = System.String, PropertyName = Property1, HtmlFieldName = Property1, TemplateName = (null), Mode = ReadOnly, AdditionalViewData = (null)</div>" + Environment.NewLine;

            // Arrange
            ObjectTemplateModel model = new ObjectTemplateModel();
            HtmlHelper html = MakeHtmlHelper<ObjectTemplateModel>(model);
            Mock<ModelMetadataProvider> provider = new Mock<ModelMetadataProvider>();
            Func<object> accessor = () => model;
            Mock<ModelMetadata> metadata = new Mock<ModelMetadata>(provider.Object, null, accessor, typeof(ObjectTemplateModel), null);
            ModelMetadata prop1Metadata = new ModelMetadata(provider.Object, typeof(ObjectTemplateModel), null, typeof(string), "Property1") { ShowForDisplay = true };
            ModelMetadata prop2Metadata = new ModelMetadata(provider.Object, typeof(ObjectTemplateModel), null, typeof(string), "Property2") { ShowForDisplay = false };
            html.ViewData.ModelMetadata = metadata.Object;
            metadata.Setup(p => p.Properties).Returns(() => new[] { prop1Metadata, prop2Metadata });

            // Act
            string result = DefaultDisplayTemplates.ObjectTemplate(html, SpyCallback);

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void ObjectTemplatePreventsRecursionOnModelValue()
        {
            string expected =
                "<div class=\"display-label\">Property2</div>" + Environment.NewLine
              + "<div class=\"display-field\">Model = propValue2, ModelType = System.String, PropertyName = Property2, HtmlFieldName = Property2, TemplateName = (null), Mode = ReadOnly, AdditionalViewData = (null)</div>" + Environment.NewLine;

            // Arrange
            ObjectTemplateModel model = new ObjectTemplateModel();
            HtmlHelper html = MakeHtmlHelper<ObjectTemplateModel>(model);
            Mock<ModelMetadataProvider> provider = new Mock<ModelMetadataProvider>();
            Func<object> accessor = () => model;
            Mock<ModelMetadata> metadata = new Mock<ModelMetadata>(provider.Object, null, accessor, typeof(ObjectTemplateModel), null);
            ModelMetadata prop1Metadata = new ModelMetadata(provider.Object, typeof(ObjectTemplateModel), () => "propValue1", typeof(string), "Property1");
            ModelMetadata prop2Metadata = new ModelMetadata(provider.Object, typeof(ObjectTemplateModel), () => "propValue2", typeof(string), "Property2");
            html.ViewData.ModelMetadata = metadata.Object;
            metadata.Setup(p => p.Properties).Returns(() => new[] { prop1Metadata, prop2Metadata });
            html.ViewData.TemplateInfo.VisitedObjects.Add("propValue1");

            // Act
            string result = DefaultDisplayTemplates.ObjectTemplate(html, SpyCallback);

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void ObjectTemplatePreventsRecursionOnModelTypeForNullModelValues()
        {
            string expected =
                "<div class=\"display-label\">Property2</div>" + Environment.NewLine
              + "<div class=\"display-field\">Model = propValue2, ModelType = System.String, PropertyName = Property2, HtmlFieldName = Property2, TemplateName = (null), Mode = ReadOnly, AdditionalViewData = (null)</div>" + Environment.NewLine;

            // Arrange
            ObjectTemplateModel model = new ObjectTemplateModel();
            HtmlHelper html = MakeHtmlHelper<ObjectTemplateModel>(model);
            Mock<ModelMetadataProvider> provider = new Mock<ModelMetadataProvider>();
            Func<object> accessor = () => model;
            Mock<ModelMetadata> metadata = new Mock<ModelMetadata>(provider.Object, null, accessor, typeof(ObjectTemplateModel), null);
            ModelMetadata prop1Metadata = new ModelMetadata(provider.Object, typeof(ObjectTemplateModel), null, typeof(string), "Property1");
            ModelMetadata prop2Metadata = new ModelMetadata(provider.Object, typeof(ObjectTemplateModel), () => "propValue2", typeof(string), "Property2");
            html.ViewData.ModelMetadata = metadata.Object;
            metadata.Setup(p => p.Properties).Returns(() => new[] { prop1Metadata, prop2Metadata });
            html.ViewData.TemplateInfo.VisitedObjects.Add(typeof(string));

            // Act
            string result = DefaultDisplayTemplates.ObjectTemplate(html, SpyCallback);

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void ObjectTemplateDisplaysNullDisplayTextWhenObjectIsNull()
        {
            // Arrange
            HtmlHelper html = MakeHtmlHelper<ObjectTemplateModel>(null);
            ModelMetadata metadata = ModelMetadataProviders.Current.GetMetadataForType(null, typeof(ObjectTemplateModel));
            metadata.NullDisplayText = "(null value)";
            html.ViewData.ModelMetadata = metadata;

            // Act
            string result = DefaultDisplayTemplates.ObjectTemplate(html, SpyCallback);

            // Assert
            Assert.Equal(metadata.NullDisplayText, result);
        }

        [Theory]
        [PropertyData("ConditionallyHtmlEncodedData", PropertyType = typeof(EncodedDataSets))]
        public void ObjectTemplateDisplaysSimpleDisplayTextWhenTemplateDepthGreaterThanOne(
            string text,
            bool htmlEncode,
            string expectedResult)
        {
            // Arrange
            ObjectTemplateModel model = new ObjectTemplateModel();
            HtmlHelper html = MakeHtmlHelper<ObjectTemplateModel>(model);
            ModelMetadata metadata = ModelMetadataProviders.Current.GetMetadataForType(() => model, typeof(ObjectTemplateModel));
            metadata.SimpleDisplayText = text;
            metadata.HtmlEncode = htmlEncode;

            html.ViewData.ModelMetadata = metadata;
            html.ViewData.TemplateInfo.VisitedObjects.Add("foo");
            html.ViewData.TemplateInfo.VisitedObjects.Add("bar");

            // Act
            string result = DefaultDisplayTemplates.ObjectTemplate(html, SpyCallback);

            // Assert
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public void ObjectTemplateWithHiddenHtml()
        {
            string expected = "Model = propValue1, ModelType = System.String, PropertyName = Property1, HtmlFieldName = Property1, TemplateName = (null), Mode = ReadOnly, AdditionalViewData = (null)";

            // Arrange
            ObjectTemplateModel model = new ObjectTemplateModel();
            HtmlHelper html = MakeHtmlHelper<ObjectTemplateModel>(model);
            Mock<ModelMetadataProvider> provider = new Mock<ModelMetadataProvider>();
            Func<object> accessor = () => model;
            Mock<ModelMetadata> metadata = new Mock<ModelMetadata>(provider.Object, null, accessor, typeof(ObjectTemplateModel), null);
            ModelMetadata prop1Metadata = new ModelMetadata(provider.Object, typeof(ObjectTemplateModel), () => "propValue1", typeof(string), "Property1") { HideSurroundingHtml = true };
            html.ViewData.ModelMetadata = metadata.Object;
            metadata.Setup(p => p.Properties).Returns(() => new[] { prop1Metadata });

            // Act
            string result = DefaultDisplayTemplates.ObjectTemplate(html, SpyCallback);

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void ObjectTemplateAllPropertiesFromEntityObjectAreHidden()
        {
            // Arrange
            HtmlHelper html = MakeHtmlHelper<ObjectTemplateModel>(new MyEntityObject());

            // Act
            string result = DefaultDisplayTemplates.ObjectTemplate(html, SpyCallback);

            // Assert
            Assert.Equal(String.Empty, result);
        }

        private class MyEntityObject : EntityObject
        {
        }

        // StringTemplate

        [Fact]
        public void StringTemplateTests()
        {
            Assert.Equal(
                "Hello, world!",
                DefaultDisplayTemplates.StringTemplate(MakeHtmlHelper<string>("", "Hello, world!")));

            Assert.Equal(
                "&lt;b&gt;Hello, world!&lt;/b&gt;",
                DefaultDisplayTemplates.StringTemplate(MakeHtmlHelper<string>("", "<b>Hello, world!</b>")));
        }

        // UrlTemplate

        [Fact]
        public void UrlTemplateTests()
        {
            Assert.Equal(
                "<a href=\"http://www.microsoft.com/testing.aspx?value1=foo&amp;value2=bar\">http://www.microsoft.com/testing.aspx?value1=foo&amp;value2=bar</a>",
                DefaultDisplayTemplates.UrlTemplate(MakeHtmlHelper<string>("http://www.microsoft.com/testing.aspx?value1=foo&value2=bar")));

            Assert.Equal(
                "<a href=\"http://www.microsoft.com/testing.aspx?value1=foo&amp;value2=bar\">&lt;b&gt;Microsoft!&lt;/b&gt;</a>",
                DefaultDisplayTemplates.UrlTemplate(MakeHtmlHelper<string>("http://www.microsoft.com/testing.aspx?value1=foo&value2=bar", "<b>Microsoft!</b>")));
        }

        // Helpers

        private HtmlHelper MakeHtmlHelper<TModel>(object model)
        {
            return MakeHtmlHelper<TModel>(model, model);
        }

        private HtmlHelper MakeHtmlHelper<TModel>(object model, object formattedModelValue)
        {
            ViewDataDictionary viewData = new ViewDataDictionary(model);
            viewData.TemplateInfo.HtmlFieldPrefix = "FieldPrefix";
            viewData.TemplateInfo.FormattedModelValue = formattedModelValue;
            viewData.ModelMetadata = new EmptyModelMetadataProvider().GetMetadataForType(() => model, typeof(TModel));

            ViewContext viewContext = new ViewContext(new ControllerContext(), new DummyView(), viewData, new TempDataDictionary(), new StringWriter());

            return new HtmlHelper(viewContext, new SimpleViewDataContainer(viewData));
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
