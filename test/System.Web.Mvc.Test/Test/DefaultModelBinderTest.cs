// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web.Routing;
using Microsoft.TestCommon;
using Microsoft.Web.UnitTestUtil;
using Moq;
using Moq.Protected;

namespace System.Web.Mvc.Test
{
    [Xunit.Collection("Uses ScopeStorage or ViewEngines.Engines")] // Uses ModelMetadataProviders.Current
    public class DefaultModelBinderTest
    {
        [Fact]
        public void BindComplexElementalModelReturnsIfOnModelUpdatingReturnsFalse()
        {
            // Arrange
            ControllerContext controllerContext = new Mock<ControllerContext>().Object;

            MyModel model = new MyModel() { ReadWriteProperty = 3 };
            ModelBindingContext bindingContext = new ModelBindingContext()
            {
                ModelMetadata = ModelMetadataProviders.Current.GetMetadataForType(() => model, model.GetType()),
            };

            Mock<DefaultModelBinderHelper> mockHelper = new Mock<DefaultModelBinderHelper>() { CallBase = true };
            mockHelper.Setup(b => b.PublicOnModelUpdating(controllerContext, It.IsAny<ModelBindingContext>())).Returns(false);
            DefaultModelBinderHelper helper = mockHelper.Object;

            // Act
            helper.BindComplexElementalModel(controllerContext, bindingContext, model);

            // Assert
            Assert.Equal(3, model.ReadWriteProperty);
            mockHelper.Verify();
            mockHelper.Verify(b => b.PublicGetModelProperties(controllerContext, It.IsAny<ModelBindingContext>()), Times.Never());
            mockHelper.Verify(b => b.PublicBindProperty(controllerContext, It.IsAny<ModelBindingContext>(), It.IsAny<PropertyDescriptor>()), Times.Never());
        }

        [Fact]
        public void BindComplexModelCanBindArrays()
        {
            // Arrange
            ControllerContext controllerContext = new Mock<ControllerContext>().Object;

            ModelBindingContext bindingContext = new ModelBindingContext()
            {
                ModelMetadata = ModelMetadataProviders.Current.GetMetadataForType(null, typeof(int[])),
                ModelName = "foo",
                PropertyFilter = _ => false,
                ValueProvider = new SimpleValueProvider()
                {
                    { "foo[0]", null },
                    { "foo[1]", null },
                    { "foo[2]", null }
                }
            };

            Mock<IModelBinder> mockInnerBinder = new Mock<IModelBinder>();
            mockInnerBinder
                .Setup(b => b.BindModel(It.IsAny<ControllerContext>(), It.IsAny<ModelBindingContext>()))
                .Returns(
                    delegate(ControllerContext cc, ModelBindingContext bc)
                    {
                        Assert.Equal(controllerContext, cc);
                        Assert.Equal(typeof(int), bc.ModelType);
                        Assert.Equal(bindingContext.ModelState, bc.ModelState);
                        Assert.Equal(bindingContext.PropertyFilter, bc.PropertyFilter);
                        Assert.Equal(bindingContext.ValueProvider, bc.ValueProvider);
                        return Int32.Parse(bc.ModelName.Substring(4, 1), CultureInfo.InvariantCulture);
                    });

            DefaultModelBinder binder = new DefaultModelBinder()
            {
                Binders = new ModelBinderDictionary()
                {
                    { typeof(int), mockInnerBinder.Object }
                }
            };

            // Act
            object newModel = binder.BindComplexModel(controllerContext, bindingContext);

            // Assert
            var newIntArray = Assert.IsType<int[]>(newModel);
            Assert.Equal(new[] { 0, 1, 2 }, newIntArray);
        }

        [Fact]
        public void BindComplexModelCanBindCollections()
        {
            // Arrange
            ControllerContext controllerContext = new Mock<ControllerContext>().Object;

            ModelBindingContext bindingContext = new ModelBindingContext()
            {
                ModelMetadata = ModelMetadataProviders.Current.GetMetadataForType(null, typeof(IList<int>)),
                ModelName = "foo",
                PropertyFilter = _ => false,
                ValueProvider = new SimpleValueProvider()
                {
                    { "foo[0]", null },
                    { "foo[1]", null },
                    { "foo[2]", null }
                }
            };

            Mock<IModelBinder> mockInnerBinder = new Mock<IModelBinder>();
            mockInnerBinder
                .Setup(b => b.BindModel(It.IsAny<ControllerContext>(), It.IsAny<ModelBindingContext>()))
                .Returns(
                    delegate(ControllerContext cc, ModelBindingContext bc)
                    {
                        Assert.Equal(controllerContext, cc);
                        Assert.Equal(typeof(int), bc.ModelType);
                        Assert.Equal(bindingContext.ModelState, bc.ModelState);
                        Assert.Equal(bindingContext.PropertyFilter, bc.PropertyFilter);
                        Assert.Equal(bindingContext.ValueProvider, bc.ValueProvider);
                        return Int32.Parse(bc.ModelName.Substring(4, 1), CultureInfo.InvariantCulture);
                    });

            DefaultModelBinder binder = new DefaultModelBinder()
            {
                Binders = new ModelBinderDictionary()
                {
                    { typeof(int), mockInnerBinder.Object }
                }
            };

            // Act
            object newModel = binder.BindComplexModel(controllerContext, bindingContext);

            // Assert
            var modelAsList = Assert.IsAssignableFrom<IList<int>>(newModel);
            Assert.Equal(new[] { 0, 1, 2 }, modelAsList.ToArray());
        }

        [Fact]
        public void BindComplexModelCanBindDictionariesWithDotsNotation()
        {
            // Arrange
            ControllerContext controllerContext = new Mock<ControllerContext>().Object;

            ModelBindingContext bindingContext = new ModelBindingContext()
            {
                ModelMetadata = ModelMetadataProviders.Current.GetMetadataForType(null, typeof(IDictionary<string, CountryState>)),
                ModelName = "countries",
                PropertyFilter = _ => true,
                ValueProvider = new DictionaryValueProvider<object>(new Dictionary<string, object>()
                {
                    { "countries.CA.Name", "Canada" },
                    { "countries.CA.States[0]", "Québec" },
                    { "countries.CA.States[1]", "British Columbia" },
                    { "countries.US.Name", "United States" },
                    { "countries.US.States[0]", "Washington" },
                    { "countries.US.States[1]", "Oregon" }
                }, CultureInfo.CurrentCulture)
            };

            DefaultModelBinder binder = new DefaultModelBinder();

            // Act
            object newModel = binder.BindComplexModel(controllerContext, bindingContext);

            // Assert
            var modelAsDictionary = Assert.IsAssignableFrom<IDictionary<string, CountryState>>(newModel);
            Assert.Equal(2, modelAsDictionary.Count);
            Assert.Equal("Canada", modelAsDictionary["CA"].Name);
            Assert.Equal("United States", modelAsDictionary["US"].Name);
            Assert.Equal(2, modelAsDictionary["CA"].States.Count());
            Assert.Contains("Québec", modelAsDictionary["CA"].States);
            Assert.Contains("British Columbia", modelAsDictionary["CA"].States);
            Assert.Equal(2, modelAsDictionary["US"].States.Count());
            Assert.Contains("Washington", modelAsDictionary["US"].States);
            Assert.Contains("Oregon", modelAsDictionary["US"].States);
        }

        [Fact]
        public void BindComplexModelCanBindDictionariesWithBracketsNotation()
        {
            // Arrange
            ControllerContext controllerContext = new Mock<ControllerContext>().Object;

            ModelBindingContext bindingContext = new ModelBindingContext()
            {
                ModelMetadata = ModelMetadataProviders.Current.GetMetadataForType(null, typeof(IDictionary<string, CountryState>)),
                ModelName = "countries",
                PropertyFilter = _ => true,
                ValueProvider = new DictionaryValueProvider<object>(new Dictionary<string, object>()
                {
                    { "countries[CA].Name", "Canada" },
                    { "countries[CA].States[0]", "Québec" },
                    { "countries[CA].States[1]", "British Columbia" },
                    { "countries[US].Name", "United States" },
                    { "countries[US].States[0]", "Washington" },
                    { "countries[US].States[1]", "Oregon" }
                }, CultureInfo.CurrentCulture)
            };

            DefaultModelBinder binder = new DefaultModelBinder();

            // Act
            object newModel = binder.BindComplexModel(controllerContext, bindingContext);

            // Assert
            var modelAsDictionary = Assert.IsAssignableFrom<IDictionary<string, CountryState>>(newModel);
            Assert.Equal(2, modelAsDictionary.Count);
            Assert.Equal("Canada", modelAsDictionary["CA"].Name);
            Assert.Equal("United States", modelAsDictionary["US"].Name);
            Assert.Equal(2, modelAsDictionary["CA"].States.Count());
            Assert.Contains("Québec", modelAsDictionary["CA"].States);
            Assert.Contains("British Columbia", modelAsDictionary["CA"].States);
            Assert.Equal(2, modelAsDictionary["US"].States.Count());
            Assert.Contains("Washington", modelAsDictionary["US"].States);
            Assert.Contains("Oregon", modelAsDictionary["US"].States);
        }

        [Fact]
        public void BindComplexModelCanBindDictionariesWithBracketsAndDotsNotation()
        {
            // Arrange
            ControllerContext controllerContext = new Mock<ControllerContext>().Object;

            ModelBindingContext bindingContext = new ModelBindingContext()
            {
                ModelMetadata = ModelMetadataProviders.Current.GetMetadataForType(null, typeof(IDictionary<string, CountryState>)),
                ModelName = "countries",
                PropertyFilter = _ => true,
                ValueProvider = new DictionaryValueProvider<object>(new Dictionary<string, object>()
                {
                    { "countries[CA].Name", "Canada" },
                    { "countries[CA].States[0]", "Québec" },
                    { "countries.CA.States[1]", "British Columbia" },
                    { "countries.US.Name", "United States" },
                    { "countries.US.States[0]", "Washington" },
                    { "countries.US.States[1]", "Oregon" }
                }, CultureInfo.CurrentCulture)
            };

            DefaultModelBinder binder = new DefaultModelBinder();

            // Act
            object newModel = binder.BindComplexModel(controllerContext, bindingContext);

            // Assert
            var modelAsDictionary = Assert.IsAssignableFrom<IDictionary<string, CountryState>>(newModel);
            Assert.Equal(2, modelAsDictionary.Count);
            Assert.Equal("Canada", modelAsDictionary["CA"].Name);
            Assert.Equal("United States", modelAsDictionary["US"].Name);
            string state = Assert.Single(modelAsDictionary["CA"].States);
            Assert.Contains("Québec", state);

            // We do not accept double notation for a same entry, so we can't find that state.
            Assert.DoesNotContain("British Columbia", state);
            Assert.Equal(2, modelAsDictionary["US"].States.Count());
            Assert.Contains("Washington", modelAsDictionary["US"].States);
            Assert.Contains("Oregon", modelAsDictionary["US"].States);
        }

        [Fact]
        public void BindComplexModelCanBindDictionaries()
        {
            // Arrange
            ControllerContext controllerContext = new Mock<ControllerContext>().Object;

            ModelBindingContext bindingContext = new ModelBindingContext()
            {
                ModelMetadata = ModelMetadataProviders.Current.GetMetadataForType(null, typeof(IDictionary<int, string>)),
                ModelName = "foo",
                PropertyFilter = _ => false,
                ValueProvider = new SimpleValueProvider()
                {
                    { "foo[0].key", null }, { "foo[0].value", null },
                    { "foo[1].key", null }, { "foo[1].value", null },
                    { "foo[2].key", null }, { "foo[2].value", null }
                }
            };

            Mock<IModelBinder> mockIntBinder = new Mock<IModelBinder>();
            mockIntBinder
                .Setup(b => b.BindModel(It.IsAny<ControllerContext>(), It.IsAny<ModelBindingContext>()))
                .Returns(
                    delegate(ControllerContext cc, ModelBindingContext bc)
                    {
                        Assert.Equal(controllerContext, cc);
                        Assert.Equal(typeof(int), bc.ModelType);
                        Assert.Equal(bindingContext.ModelState, bc.ModelState);
                        Assert.Equal(new ModelBindingContext().PropertyFilter, bc.PropertyFilter);
                        Assert.Equal(bindingContext.ValueProvider, bc.ValueProvider);
                        return Int32.Parse(bc.ModelName.Substring(4, 1), CultureInfo.InvariantCulture) + 10;
                    });

            Mock<IModelBinder> mockStringBinder = new Mock<IModelBinder>();
            mockStringBinder
                .Setup(b => b.BindModel(It.IsAny<ControllerContext>(), It.IsAny<ModelBindingContext>()))
                .Returns(
                    delegate(ControllerContext cc, ModelBindingContext bc)
                    {
                        Assert.Equal(controllerContext, cc);
                        Assert.Equal(typeof(string), bc.ModelType);
                        Assert.Equal(bindingContext.ModelState, bc.ModelState);
                        Assert.Equal(bindingContext.PropertyFilter, bc.PropertyFilter);
                        Assert.Equal(bindingContext.ValueProvider, bc.ValueProvider);
                        return (Int32.Parse(bc.ModelName.Substring(4, 1), CultureInfo.InvariantCulture) + 10) + "Value";
                    });

            DefaultModelBinder binder = new DefaultModelBinder()
            {
                Binders = new ModelBinderDictionary()
                {
                    { typeof(int), mockIntBinder.Object },
                    { typeof(string), mockStringBinder.Object }
                }
            };

            // Act
            object newModel = binder.BindComplexModel(controllerContext, bindingContext);

            // Assert
            var modelAsDictionary = Assert.IsAssignableFrom<IDictionary<int, string>>(newModel);
            Assert.Equal(3, modelAsDictionary.Count);
            Assert.Equal("10Value", modelAsDictionary[10]);
            Assert.Equal("11Value", modelAsDictionary[11]);
            Assert.Equal("12Value", modelAsDictionary[12]);
        }

        [Fact]
        public void BindComplexModelCanBindObjects()
        {
            // Arrange
            ControllerContext controllerContext = new Mock<ControllerContext>().Object;

            ModelWithoutBindAttribute model = new ModelWithoutBindAttribute()
            {
                Foo = "FooPreValue",
                Bar = "BarPreValue",
                Baz = "BazPreValue",
            };
            ModelBindingContext bindingContext = new ModelBindingContext()
            {
                ModelMetadata = ModelMetadataProviders.Current.GetMetadataForType(() => model, model.GetType()),
                ValueProvider = new SimpleValueProvider() { { "Foo", null }, { "Bar", null } }
            };

            Mock<IModelBinder> mockInnerBinder = new Mock<IModelBinder>();
            mockInnerBinder
                .Setup(b => b.BindModel(It.IsAny<ControllerContext>(), It.IsAny<ModelBindingContext>()))
                .Returns(
                    delegate(ControllerContext cc, ModelBindingContext bc)
                    {
                        Assert.Equal(controllerContext, cc);
                        Assert.Equal(bindingContext.ValueProvider, bc.ValueProvider);
                        return bc.ModelName + "PostValue";
                    });

            DefaultModelBinder binder = new DefaultModelBinder()
            {
                Binders = new ModelBinderDictionary()
                {
                    { typeof(string), mockInnerBinder.Object }
                }
            };

            // Act
            object updatedModel = binder.BindComplexModel(controllerContext, bindingContext);

            // Assert
            Assert.Same(model, updatedModel);
            Assert.Equal("FooPostValue", model.Foo);
            Assert.Equal("BarPostValue", model.Bar);
            Assert.Equal("BazPreValue", model.Baz);
        }

        [Fact]
        public void BindComplexModelReturnsNullArrayIfNoValuesProvided()
        {
            // Arrange
            ModelBindingContext bindingContext = new ModelBindingContext()
            {
                ModelMetadata = ModelMetadataProviders.Current.GetMetadataForType(null, typeof(int[])),
                ModelName = "foo",
                ValueProvider = new SimpleValueProvider() { { "foo", null } }
            };

            Mock<IModelBinder> mockInnerBinder = new Mock<IModelBinder>();
            mockInnerBinder
                .Setup(b => b.BindModel(It.IsAny<ControllerContext>(), It.IsAny<ModelBindingContext>()))
                .Returns(
                    delegate(ControllerContext cc, ModelBindingContext bc) { return Int32.Parse(bc.ModelName.Substring(4, 1), CultureInfo.InvariantCulture); });

            DefaultModelBinder binder = new DefaultModelBinder()
            {
                Binders = new ModelBinderDictionary()
                {
                    { typeof(int), mockInnerBinder.Object }
                }
            };

            // Act
            object newModel = binder.BindComplexModel(null, bindingContext);

            // Assert
            Assert.Null(newModel);
        }

        [Fact]
        public void BindComplexModelWhereModelTypeContainsBindAttribute()
        {
            // Arrange
            ControllerContext controllerContext = new Mock<ControllerContext>().Object;

            ModelWithBindAttribute model = new ModelWithBindAttribute()
            {
                Foo = "FooPreValue",
                Bar = "BarPreValue",
                Baz = "BazPreValue",
            };
            ModelBindingContext bindingContext = new ModelBindingContext()
            {
                ModelMetadata = ModelMetadataProviders.Current.GetMetadataForType(() => model, model.GetType()),
                ValueProvider = new SimpleValueProvider() { { "Foo", null }, { "Bar", null } }
            };

            Mock<IModelBinder> mockInnerBinder = new Mock<IModelBinder>();
            mockInnerBinder
                .Setup(b => b.BindModel(It.IsAny<ControllerContext>(), It.IsAny<ModelBindingContext>()))
                .Returns(
                    delegate(ControllerContext cc, ModelBindingContext bc)
                    {
                        Assert.Equal(controllerContext, cc);
                        Assert.Equal(bindingContext.ValueProvider, bc.ValueProvider);
                        return bc.ModelName + "PostValue";
                    });

            DefaultModelBinder binder = new DefaultModelBinder()
            {
                Binders = new ModelBinderDictionary()
                {
                    { typeof(string), mockInnerBinder.Object }
                }
            };

            // Act
            binder.BindComplexModel(controllerContext, bindingContext);

            // Assert
            Assert.Equal("FooPreValue", model.Foo);
            Assert.Equal("BarPostValue", model.Bar);
            Assert.Equal("BazPreValue", model.Baz);
        }

        [Fact]
        public void BindComplexModelWhereModelTypeDoesNotContainBindAttribute()
        {
            // Arrange
            ControllerContext controllerContext = new Mock<ControllerContext>().Object;

            ModelWithoutBindAttribute model = new ModelWithoutBindAttribute()
            {
                Foo = "FooPreValue",
                Bar = "BarPreValue",
                Baz = "BazPreValue",
            };
            ModelBindingContext bindingContext = new ModelBindingContext()
            {
                ModelMetadata = ModelMetadataProviders.Current.GetMetadataForType(() => model, model.GetType()),
                ValueProvider = new SimpleValueProvider() { { "Foo", null }, { "Bar", null } }
            };

            Mock<IModelBinder> mockInnerBinder = new Mock<IModelBinder>();
            mockInnerBinder
                .Setup(b => b.BindModel(It.IsAny<ControllerContext>(), It.IsAny<ModelBindingContext>()))
                .Returns(
                    delegate(ControllerContext cc, ModelBindingContext bc)
                    {
                        Assert.Equal(controllerContext, cc);
                        Assert.Equal(bindingContext.ValueProvider, bc.ValueProvider);
                        return bc.ModelName + "PostValue";
                    });

            DefaultModelBinder binder = new DefaultModelBinder()
            {
                Binders = new ModelBinderDictionary()
                {
                    { typeof(string), mockInnerBinder.Object }
                }
            };

            // Act
            binder.BindComplexModel(controllerContext, bindingContext);

            // Assert
            Assert.Equal("FooPostValue", model.Foo);
            Assert.Equal("BarPostValue", model.Bar);
            Assert.Equal("BazPreValue", model.Baz);
        }

        // BindModel tests

        [Fact]
        public void BindModelCanBindObjects()
        {
            // Arrange
            ControllerContext controllerContext = new Mock<ControllerContext>().Object;

            ModelWithoutBindAttribute model = new ModelWithoutBindAttribute()
            {
                Foo = "FooPreValue",
                Bar = "BarPreValue",
                Baz = "BazPreValue",
            };
            ModelBindingContext bindingContext = new ModelBindingContext()
            {
                ModelMetadata = ModelMetadataProviders.Current.GetMetadataForType(() => model, model.GetType()),
                ValueProvider = new SimpleValueProvider() { { "Foo", null }, { "Bar", null } }
            };

            Mock<IModelBinder> mockInnerBinder = new Mock<IModelBinder>();
            mockInnerBinder
                .Setup(b => b.BindModel(It.IsAny<ControllerContext>(), It.IsAny<ModelBindingContext>()))
                .Returns(
                    delegate(ControllerContext cc, ModelBindingContext bc)
                    {
                        Assert.Equal(controllerContext, cc);
                        Assert.Equal(bindingContext.ValueProvider, bc.ValueProvider);
                        return bc.ModelName + "PostValue";
                    });

            DefaultModelBinder binder = new DefaultModelBinder()
            {
                Binders = new ModelBinderDictionary()
                {
                    { typeof(string), mockInnerBinder.Object }
                }
            };

            // Act
            object updatedModel = binder.BindModel(controllerContext, bindingContext);

            // Assert
            Assert.Same(model, updatedModel);
            Assert.Equal("FooPostValue", model.Foo);
            Assert.Equal("BarPostValue", model.Bar);
            Assert.Equal("BazPreValue", model.Baz);
        }

        [Fact]
        public void BindModelCanBindSimpleTypes()
        {
            // Arrange
            ModelBindingContext bindingContext = new ModelBindingContext()
            {
                ModelMetadata = ModelMetadataProviders.Current.GetMetadataForType(null, typeof(int)),
                ModelName = "foo",
                ValueProvider = new SimpleValueProvider()
                {
                    { "foo", "42" }
                }
            };

            DefaultModelBinder binder = new DefaultModelBinder();

            // Act
            object updatedModel = binder.BindModel(new ControllerContext(), bindingContext);

            // Assert
            Assert.Equal(42, updatedModel);
        }

        [Fact]
        public void BindModel_PerformsValidationByDefault()
        {
            // Arrange
            ModelMetadata metadata = new DataAnnotationsModelMetadataProvider().GetMetadataForType(null, typeof(string));

            ControllerContext controllerContext = new ControllerContext();
            controllerContext.Controller = new SimpleController();

            ModelBindingContext bindingContext = new ModelBindingContext()
            {
                ModelMetadata = metadata,
                ModelName = "foo",
                ValueProvider = new CustomUnvalidatedValueProvider()
            };

            DefaultModelBinder binder = new DefaultModelBinder();

            // Act
            object updatedModel = binder.BindModel(controllerContext, bindingContext);

            // Assert
            Assert.Equal("fooValidated", updatedModel);
        }

        [Fact]
        public void BindModel_SkipsValidationIfControllerOptsOut()
        {
            // Arrange
            ModelMetadata metadata = new DataAnnotationsModelMetadataProvider().GetMetadataForType(null, typeof(string));

            ControllerContext controllerContext = new ControllerContext();
            controllerContext.Controller = new SimpleController();
            controllerContext.Controller.ValidateRequest = false;

            ModelBindingContext bindingContext = new ModelBindingContext()
            {
                ModelMetadata = metadata,
                ModelName = "foo",
                ValueProvider = new CustomUnvalidatedValueProvider()
            };

            DefaultModelBinder binder = new DefaultModelBinder();

            // Act
            object updatedModel = binder.BindModel(controllerContext, bindingContext);

            // Assert
            Assert.Equal("fooUnvalidated", updatedModel);
        }

        [Fact]
        public void BindModel_SkipsValidationIfModelOptsOut()
        {
            // Arrange
            ModelMetadata metadata = new DataAnnotationsModelMetadataProvider().GetMetadataForType(null, typeof(string));
            metadata.RequestValidationEnabled = false;

            ControllerContext controllerContext = new ControllerContext();
            controllerContext.Controller = new SimpleController();

            ModelBindingContext bindingContext = new ModelBindingContext()
            {
                ModelMetadata = metadata,
                ModelName = "foo",
                ValueProvider = new CustomUnvalidatedValueProvider()
            };

            DefaultModelBinder binder = new DefaultModelBinder();

            // Act
            object updatedModel = binder.BindModel(controllerContext, bindingContext);

            // Assert
            Assert.Equal("fooUnvalidated", updatedModel);
        }

        [Fact]
        public void BindModelReturnsNullIfKeyNotFound()
        {
            // Arrange
            ModelBindingContext bindingContext = new ModelBindingContext()
            {
                ModelMetadata = ModelMetadataProviders.Current.GetMetadataForType(null, typeof(int)),
                ModelName = "foo",
                ValueProvider = new SimpleValueProvider()
            };

            DefaultModelBinder binder = new DefaultModelBinder();

            // Act
            object returnedModel = binder.BindModel(new ControllerContext(), bindingContext);

            // Assert
            Assert.Null(returnedModel);
        }

        [Fact]
        public void BindModelThrowsIfBindingContextIsNull()
        {
            // Arrange
            DefaultModelBinder binder = new DefaultModelBinder();

            // Act & assert
            Assert.ThrowsArgumentNull(
                delegate { binder.BindModel(new ControllerContext(), null); }, "bindingContext");
        }

        [Fact]
        public void BindModelValuesCanBeOverridden()
        {
            // Arrange
            ModelBindingContext bindingContext = new ModelBindingContext()
            {
                ModelMetadata = ModelMetadataProviders.Current.GetMetadataForType(() => new ModelWithoutBindAttribute(), typeof(ModelWithoutBindAttribute)),
                ModelName = "",
                ValueProvider = new SimpleValueProvider()
                {
                    { "foo", "FooPostValue" },
                    { "bar", "BarPostValue" },
                    { "baz", "BazPostValue" }
                }
            };
            Mock<DefaultModelBinder> binder = new Mock<DefaultModelBinder> { CallBase = true };
            binder.Protected().Setup<object>("GetPropertyValue",
                                             ItExpr.IsAny<ControllerContext>(), ItExpr.IsAny<ModelBindingContext>(),
                                             ItExpr.IsAny<PropertyDescriptor>(), ItExpr.IsAny<IModelBinder>())
                .Returns("Hello, world!");

            // Act
            ModelWithoutBindAttribute model = (ModelWithoutBindAttribute)binder.Object.BindModel(new ControllerContext(), bindingContext);

            // Assert
            Assert.Equal("Hello, world!", model.Bar);
            Assert.Equal("Hello, world!", model.Baz);
            Assert.Equal("Hello, world!", model.Foo);
        }

        [Fact]
        public void BindModelWithTypeConversionErrorUpdatesModelStateMessage()
        {
            // Arrange
            ModelBindingContext bindingContext = new ModelBindingContext
            {
                ModelMetadata = ModelMetadataProviders.Current.GetMetadataForType(() => new PropertyTestingModel(), typeof(PropertyTestingModel)),
                ModelName = "",
                ValueProvider = new SimpleValueProvider()
                {
                    { "IntReadWrite", "foo" }
                },
            };
            DefaultModelBinder binder = new DefaultModelBinder();

            // Act
            PropertyTestingModel model = (PropertyTestingModel)binder.BindModel(new ControllerContext(), bindingContext);

            // Assert
            ModelState modelState = bindingContext.ModelState["IntReadWrite"];
            Assert.NotNull(modelState);
            ModelError error = Assert.Single(modelState.Errors);
            Assert.Equal("The value 'foo' is not valid for IntReadWrite.", error.ErrorMessage);
        }

        [Fact]
        public void BindModelWithPrefix()
        {
            // Arrange
            ModelWithoutBindAttribute model = new ModelWithoutBindAttribute()
            {
                Foo = "FooPreValue",
                Bar = "BarPreValue",
                Baz = "BazPreValue",
            };
            ModelBindingContext bindingContext = new ModelBindingContext()
            {
                ModelMetadata = ModelMetadataProviders.Current.GetMetadataForType(() => model, model.GetType()),
                ModelName = "prefix",
                ValueProvider = new SimpleValueProvider()
                {
                    { "prefix.foo", "FooPostValue" },
                    { "prefix.bar", "BarPostValue" }
                }
            };

            DefaultModelBinder binder = new DefaultModelBinder();

            // Act
            object updatedModel = binder.BindModel(new ControllerContext(), bindingContext);

            // Assert
            Assert.Same(model, updatedModel);
            Assert.Equal("FooPostValue", model.Foo);
            Assert.Equal("BarPostValue", model.Bar);
            Assert.Equal("BazPreValue", model.Baz);
        }

        [Fact]
        public void BindModelWithPrefixAndFallback()
        {
            // Arrange
            ModelWithoutBindAttribute model = new ModelWithoutBindAttribute()
            {
                Foo = "FooPreValue",
                Bar = "BarPreValue",
                Baz = "BazPreValue",
            };
            ModelBindingContext bindingContext = new ModelBindingContext()
            {
                FallbackToEmptyPrefix = true,
                ModelMetadata = ModelMetadataProviders.Current.GetMetadataForType(() => model, model.GetType()),
                ModelName = "prefix",
                ValueProvider = new SimpleValueProvider()
                {
                    { "foo", "FooPostValue" },
                    { "bar", "BarPostValue" }
                }
            };

            DefaultModelBinder binder = new DefaultModelBinder();

            // Act
            object updatedModel = binder.BindModel(new ControllerContext(), bindingContext);

            // Assert
            Assert.Same(model, updatedModel);
            Assert.Equal("FooPostValue", model.Foo);
            Assert.Equal("BarPostValue", model.Bar);
            Assert.Equal("BazPreValue", model.Baz);
        }

        [Fact]
        public void BindModelWithPrefixReturnsNullIfFallbackNotSpecifiedAndValueProviderContainsNoEntries()
        {
            // Arrange
            ModelWithoutBindAttribute model = new ModelWithoutBindAttribute()
            {
                Foo = "FooPreValue",
                Bar = "BarPreValue",
                Baz = "BazPreValue",
            };
            ModelBindingContext bindingContext = new ModelBindingContext()
            {
                ModelMetadata = ModelMetadataProviders.Current.GetMetadataForType(() => model, model.GetType()),
                ModelName = "prefix",
                ValueProvider = new SimpleValueProvider()
                {
                    { "foo", "FooPostValue" },
                    { "bar", "BarPostValue" }
                }
            };

            DefaultModelBinder binder = new DefaultModelBinder();

            // Act
            object updatedModel = binder.BindModel(new ControllerContext(), bindingContext);

            // Assert
            Assert.Null(updatedModel);
        }

        [Fact]
        public void BindModelReturnsNullIfSimpleTypeNotFound()
        {
            // DevDiv 216165: ModelBinders should not try and instantiate simple types

            // Arrange
            ModelBindingContext bindingContext = new ModelBindingContext()
            {
                ModelMetadata = ModelMetadataProviders.Current.GetMetadataForType(null, typeof(string)),
                ModelName = "prefix",
                ValueProvider = new SimpleValueProvider()
                {
                    { "prefix.foo", "foo" },
                    { "prefix.bar", "bar" }
                }
            };

            DefaultModelBinder binder = new DefaultModelBinder();

            // Act
            object updatedModel = binder.BindModel(new ControllerContext(), bindingContext);

            // Assert
            Assert.Null(updatedModel);
        }

        // BindProperty tests

        [Fact]
        public void BindPropertyCanUpdateComplexReadOnlyProperties()
        {
            // Arrange
            // the Customer type contains a single read-only Address property
            Customer model = new Customer();
            ModelBindingContext bindingContext = new ModelBindingContext()
            {
                ModelMetadata = ModelMetadataProviders.Current.GetMetadataForType(() => model, model.GetType()),
                ValueProvider = new SimpleValueProvider() { { "Address", null } }
            };

            Mock<IModelBinder> mockInnerBinder = new Mock<IModelBinder>();
            mockInnerBinder
                .Setup(b => b.BindModel(It.IsAny<ControllerContext>(), It.IsAny<ModelBindingContext>()))
                .Returns(
                    delegate(ControllerContext cc, ModelBindingContext bc)
                    {
                        Address address = (Address)bc.Model;
                        address.Street = "1 Microsoft Way";
                        address.Zip = "98052";
                        return address;
                    });

            PropertyDescriptor pd = TypeDescriptor.GetProperties(model)["Address"];
            DefaultModelBinderHelper helper = new DefaultModelBinderHelper()
            {
                Binders = new ModelBinderDictionary()
                {
                    { typeof(Address), mockInnerBinder.Object }
                }
            };

            // Act
            helper.PublicBindProperty(new ControllerContext(), bindingContext, pd);

            // Assert
            Assert.Equal("1 Microsoft Way", model.Address.Street);
            Assert.Equal("98052", model.Address.Zip);
        }

        [Fact]
        public void BindPropertyDoesNothingIfValueProviderContainsNoEntryForProperty()
        {
            // Arrange
            MyModel2 model = new MyModel2() { IntReadWrite = 3 };
            ModelBindingContext bindingContext = new ModelBindingContext()
            {
                ModelMetadata = ModelMetadataProviders.Current.GetMetadataForType(() => model, model.GetType()),
                ValueProvider = new SimpleValueProvider()
            };

            PropertyDescriptor pd = TypeDescriptor.GetProperties(model)["IntReadWrite"];
            DefaultModelBinderHelper helper = new DefaultModelBinderHelper();

            // Act
            helper.PublicBindProperty(new ControllerContext(), bindingContext, pd);

            // Assert
            Assert.Equal(3, model.IntReadWrite);
        }

        [Fact]
        public void BindProperty()
        {
            // Arrange
            ControllerContext controllerContext = new Mock<ControllerContext>().Object;
            MyModel2 model = new MyModel2() { IntReadWrite = 3 };
            ModelBindingContext bindingContext = new ModelBindingContext()
            {
                ModelMetadata = ModelMetadataProviders.Current.GetMetadataForType(() => model, model.GetType()),
                ValueProvider = new SimpleValueProvider()
                {
                    { "IntReadWrite", "42" }
                }
            };

            PropertyDescriptor pd = TypeDescriptor.GetProperties(model)["IntReadWrite"];

            Mock<DefaultModelBinderHelper> mockHelper = new Mock<DefaultModelBinderHelper>() { CallBase = true };
            mockHelper.Setup(b => b.PublicOnPropertyValidating(controllerContext, bindingContext, pd, 42)).Returns(true).Verifiable();
            mockHelper.Setup(b => b.PublicSetProperty(controllerContext, bindingContext, pd, 42)).Verifiable();
            mockHelper.Setup(b => b.PublicOnPropertyValidated(controllerContext, bindingContext, pd, 42)).Verifiable();
            DefaultModelBinderHelper helper = mockHelper.Object;

            // Act
            helper.PublicBindProperty(controllerContext, bindingContext, pd);

            // Assert
            mockHelper.Verify();
        }

        [Fact]
        public void BindPropertyReturnsIfOnPropertyValidatingReturnsFalse()
        {
            // Arrange
            ControllerContext controllerContext = new Mock<ControllerContext>().Object;
            MyModel2 model = new MyModel2() { IntReadWrite = 3 };
            ModelBindingContext bindingContext = new ModelBindingContext()
            {
                ModelMetadata = ModelMetadataProviders.Current.GetMetadataForType(() => model, model.GetType()),
                ValueProvider = new SimpleValueProvider()
                {
                    { "IntReadWrite", "42" }
                }
            };

            PropertyDescriptor pd = TypeDescriptor.GetProperties(model)["IntReadWrite"];

            Mock<DefaultModelBinderHelper> mockHelper = new Mock<DefaultModelBinderHelper>() { CallBase = true };
            mockHelper.Setup(b => b.PublicOnPropertyValidating(controllerContext, bindingContext, pd, 42)).Returns(false);
            DefaultModelBinderHelper helper = mockHelper.Object;

            // Act
            helper.PublicBindProperty(controllerContext, bindingContext, pd);

            // Assert
            Assert.Equal(3, model.IntReadWrite);
            mockHelper.Verify();
            mockHelper.Verify(b => b.PublicSetProperty(controllerContext, bindingContext, pd, 42), Times.Never());
            mockHelper.Verify(b => b.PublicOnPropertyValidated(controllerContext, bindingContext, pd, 42), Times.Never());
        }

        [Fact]
        public void BindPropertySetsPropertyToNullIfUserLeftTextEntryFieldBlankForOptionalValue()
        {
            // Arrange
            MyModel2 model = new MyModel2() { NullableIntReadWrite = 8 };
            ModelBindingContext bindingContext = new ModelBindingContext()
            {
                ModelMetadata = ModelMetadataProviders.Current.GetMetadataForType(() => model, model.GetType()),
                ValueProvider = new SimpleValueProvider() { { "NullableIntReadWrite", null } }
            };

            Mock<IModelBinder> mockInnerBinder = new Mock<IModelBinder>();
            mockInnerBinder.Setup(b => b.BindModel(new ControllerContext(), It.IsAny<ModelBindingContext>())).Returns((object)null);

            PropertyDescriptor pd = TypeDescriptor.GetProperties(model)["NullableIntReadWrite"];
            DefaultModelBinderHelper helper = new DefaultModelBinderHelper()
            {
                Binders = new ModelBinderDictionary()
                {
                    { typeof(int?), mockInnerBinder.Object }
                }
            };

            // Act
            helper.PublicBindProperty(new ControllerContext(), bindingContext, pd);

            // Assert
            Assert.Empty(bindingContext.ModelState);
            Assert.Null(model.NullableIntReadWrite);
        }

        [Fact]
        public void BindPropertyUpdatesPropertyOnFailureIfInnerBinderReturnsNonNullObject()
        {
            // Arrange
            MyModel2 model = new MyModel2() { IntReadWriteNonNegative = 8 };
            ModelBindingContext bindingContext = new ModelBindingContext()
            {
                ModelMetadata = ModelMetadataProviders.Current.GetMetadataForType(() => model, model.GetType()),
                ValueProvider = new SimpleValueProvider() { { "IntReadWriteNonNegative", null } }
            };

            Mock<IModelBinder> mockInnerBinder = new Mock<IModelBinder>();
            mockInnerBinder
                .Setup(b => b.BindModel(It.IsAny<ControllerContext>(), It.IsAny<ModelBindingContext>()))
                .Returns(
                    delegate(ControllerContext cc, ModelBindingContext bc)
                    {
                        bc.ModelState.AddModelError("IntReadWriteNonNegative", "Some error text.");
                        return 4;
                    });

            PropertyDescriptor pd = TypeDescriptor.GetProperties(model)["IntReadWriteNonNegative"];
            DefaultModelBinderHelper helper = new DefaultModelBinderHelper()
            {
                Binders = new ModelBinderDictionary()
                {
                    { typeof(int), mockInnerBinder.Object }
                }
            };

            // Act
            helper.PublicBindProperty(new ControllerContext(), bindingContext, pd);

            // Assert
            Assert.False(bindingContext.ModelState.IsValidField("IntReadWriteNonNegative"));
            var error = Assert.Single(bindingContext.ModelState["IntReadWriteNonNegative"].Errors);
            Assert.Equal("Some error text.", error.ErrorMessage);
            Assert.Equal(4, model.IntReadWriteNonNegative);
        }

        [Fact]
        public void BindPropertyUpdatesPropertyOnSuccess()
        {
            // Arrange
            // Effectively, this is just testing updating a single property named "IntReadWrite"
            ControllerContext controllerContext = new Mock<ControllerContext>().Object;

            MyModel2 model = new MyModel2() { IntReadWrite = 3 };
            ModelBindingContext bindingContext = new ModelBindingContext()
            {
                ModelMetadata = ModelMetadataProviders.Current.GetMetadataForType(() => model, model.GetType()),
                ModelName = "foo",
                ModelState = new ModelStateDictionary() { { "blah", new ModelState() } },
                ValueProvider = new SimpleValueProvider() { { "foo.IntReadWrite", null } }
            };

            Mock<IModelBinder> mockInnerBinder = new Mock<IModelBinder>();
            mockInnerBinder
                .Setup(b => b.BindModel(It.IsAny<ControllerContext>(), It.IsAny<ModelBindingContext>()))
                .Returns(
                    delegate(ControllerContext cc, ModelBindingContext bc)
                    {
                        Assert.Equal(controllerContext, cc);
                        Assert.Equal(3, bc.Model);
                        Assert.Equal(typeof(int), bc.ModelType);
                        Assert.Equal("foo.IntReadWrite", bc.ModelName);
                        Assert.Equal(new ModelBindingContext().PropertyFilter, bc.PropertyFilter);
                        Assert.Equal(bindingContext.ModelState, bc.ModelState);
                        Assert.Equal(bindingContext.ValueProvider, bc.ValueProvider);
                        return 4;
                    });

            PropertyDescriptor pd = TypeDescriptor.GetProperties(model)["IntReadWrite"];
            DefaultModelBinderHelper helper = new DefaultModelBinderHelper()
            {
                Binders = new ModelBinderDictionary()
                {
                    { typeof(int), mockInnerBinder.Object }
                }
            };

            // Act
            helper.PublicBindProperty(controllerContext, bindingContext, pd);

            // Assert
            Assert.Equal(4, model.IntReadWrite);
        }

        [Theory]
        [InlineData(typeof(OverflowException))]
        [InlineData(typeof(FormatException))]
        public void BindPropertyReplaceErrorMessages(Type exceptionType)
        {
            // Arrange
            const string propertyName = "IntReadWriteNonNegative";
            MyModel2 model = new MyModel2() { IntReadWriteNonNegative = 8 };
            ModelBindingContext bindingContext = new ModelBindingContext()
            {
                ModelMetadata = ModelMetadataProviders.Current.GetMetadataForType(() => model, model.GetType()),
                ValueProvider = new SimpleValueProvider() { { propertyName, null } }
            };

            Mock<IModelBinder> mockInnerBinder = new Mock<IModelBinder>();
            mockInnerBinder
                .Setup(b => b.BindModel(It.IsAny<ControllerContext>(), It.IsAny<ModelBindingContext>()))
                .Returns(
                    delegate(ControllerContext cc, ModelBindingContext bc)
                    {
                        bc.ModelState.AddModelError(propertyName, new Exception("", (Exception) Activator.CreateInstance(exceptionType)));
                        bc.ModelState[propertyName].Value = new ValueProviderResult(8, "8", CultureInfo.InvariantCulture);
                        return 4;
                    });

            PropertyDescriptor pd = TypeDescriptor.GetProperties(model)[propertyName];
            DefaultModelBinderHelper helper = new DefaultModelBinderHelper()
            {
                Binders = new ModelBinderDictionary()
                {
                    { typeof(int), mockInnerBinder.Object }
                }
            };

            // Act
            helper.PublicBindProperty(new ControllerContext(), bindingContext, pd);

            // Assert
            Assert.False(bindingContext.ModelState.IsValidField(propertyName));
            var error = Assert.Single(bindingContext.ModelState[propertyName].Errors);
            Assert.Equal("The value '8' is not valid for " + propertyName + ".", error.ErrorMessage);
            Assert.Equal(4, model.IntReadWriteNonNegative);
        }

        [Theory]
        [InlineData(typeof(MissingMemberException))]
        [InlineData(typeof(IndexOutOfRangeException))]
        public void BindPropertyNotShowErrorMessage(Type exceptionType)
        {
            // Arrange
            const string propertyName = "IntReadWriteNonNegative";
            MyModel2 model = new MyModel2() { IntReadWriteNonNegative = 8 };
            ModelBindingContext bindingContext = new ModelBindingContext()
            {
                ModelMetadata = ModelMetadataProviders.Current.GetMetadataForType(() => model, model.GetType()),
                ValueProvider = new SimpleValueProvider() { { propertyName, null } }
            };

            Mock<IModelBinder> mockInnerBinder = new Mock<IModelBinder>();
            mockInnerBinder
                .Setup(b => b.BindModel(It.IsAny<ControllerContext>(), It.IsAny<ModelBindingContext>()))
                .Returns(
                    delegate(ControllerContext cc, ModelBindingContext bc)
                    {
                        bc.ModelState.AddModelError(propertyName, new Exception("", (Exception)Activator.CreateInstance(exceptionType)));
                        bc.ModelState[propertyName].Value = new ValueProviderResult(8, "8", CultureInfo.InvariantCulture);
                        return 4;
                    });

            PropertyDescriptor pd = TypeDescriptor.GetProperties(model)[propertyName];
            DefaultModelBinderHelper helper = new DefaultModelBinderHelper()
            {
                Binders = new ModelBinderDictionary()
                {
                    { typeof(int), mockInnerBinder.Object }
                }
            };

            // Act
            helper.PublicBindProperty(new ControllerContext(), bindingContext, pd);

            // Assert
            Assert.False(bindingContext.ModelState.IsValidField(propertyName));
            var error = Assert.Single(bindingContext.ModelState[propertyName].Errors);
            Assert.Equal(String.Empty, error.ErrorMessage);
            Assert.Equal(4, model.IntReadWriteNonNegative);
        }

        // BindSimpleModel tests

        [Fact]
        public void BindSimpleModelCanReturnArrayTypes()
        {
            // Arrange
            ValueProviderResult result = new ValueProviderResult(42, null, null);
            ModelBindingContext bindingContext = new ModelBindingContext()
            {
                ModelMetadata = ModelMetadataProviders.Current.GetMetadataForType(null, typeof(int[])),
                ModelName = "foo",
            };

            DefaultModelBinder binder = new DefaultModelBinder();

            // Act
            object returnedValue = binder.BindSimpleModel(null, bindingContext, result);

            // Assert
            var returnedValueAsIntArray = Assert.IsType<int[]>(returnedValue);
            var returnedInt = Assert.Single(returnedValueAsIntArray);
            Assert.Equal(42, returnedInt);
        }

        [Fact]
        public void BindSimpleModelCanReturnCollectionTypes()
        {
            // Arrange
            ValueProviderResult result = new ValueProviderResult(new string[] { "42", "82" }, null, null);
            ModelBindingContext bindingContext = new ModelBindingContext()
            {
                ModelMetadata = ModelMetadataProviders.Current.GetMetadataForType(null, typeof(IEnumerable<int>)),
                ModelName = "foo",
            };

            DefaultModelBinder binder = new DefaultModelBinder();

            // Act
            object returnedValue = binder.BindSimpleModel(null, bindingContext, result);

            // Assert
            var returnedValueAsList = Assert.IsAssignableFrom<IEnumerable<int>>(returnedValue).ToList();
            Assert.Equal(2, returnedValueAsList.Count);
            Assert.Equal(42, returnedValueAsList[0]);
            Assert.Equal(82, returnedValueAsList[1]);
        }

        [Fact]
        public void BindSimpleModelCanReturnElementalTypes()
        {
            // Arrange
            ValueProviderResult result = new ValueProviderResult("42", null, null);
            ModelBindingContext bindingContext = new ModelBindingContext()
            {
                ModelMetadata = ModelMetadataProviders.Current.GetMetadataForType(null, typeof(int)),
                ModelName = "foo",
            };

            DefaultModelBinder binder = new DefaultModelBinder();

            // Act
            object returnedValue = binder.BindSimpleModel(null, bindingContext, result);

            // Assert
            Assert.Equal(42, returnedValue);
        }

        [Fact]
        public void BindSimpleModelCanReturnStrings()
        {
            // Arrange
            ValueProviderResult result = new ValueProviderResult(new object[] { "42" }, null, null);
            ModelBindingContext bindingContext = new ModelBindingContext()
            {
                ModelMetadata = ModelMetadataProviders.Current.GetMetadataForType(null, typeof(string)),
                ModelName = "foo",
            };

            DefaultModelBinder binder = new DefaultModelBinder();

            // Act
            object returnedValue = binder.BindSimpleModel(null, bindingContext, result);

            // Assert
            Assert.Equal("42", returnedValue);
        }

        [Fact]
        public void BindSimpleModelChecksValueProviderResultRawValueType()
        {
            // Arrange
            ValueProviderResult result = new ValueProviderResult(new MemoryStream(), null, null);
            ModelBindingContext bindingContext = new ModelBindingContext()
            {
                ModelMetadata = ModelMetadataProviders.Current.GetMetadataForType(null, typeof(Stream)),
                ModelName = "foo",
            };

            DefaultModelBinder binder = new DefaultModelBinder();

            // Act
            object returnedValue = binder.BindSimpleModel(null, bindingContext, result);

            // Assert
            Assert.Equal(result, bindingContext.ModelState["foo"].Value);
            Assert.Same(result.RawValue, returnedValue);
        }

        [Fact]
        public void BindSimpleModelPropagatesErrorsOnFailure()
        {
            // Arrange
            ValueProviderResult result = new ValueProviderResult("invalid", null, null);
            ModelBindingContext bindingContext = new ModelBindingContext()
            {
                ModelMetadata = ModelMetadataProviders.Current.GetMetadataForType(null, typeof(int)),
                ModelName = "foo",
            };

            DefaultModelBinder binder = new DefaultModelBinder();

            // Act
            object returnedValue = binder.BindSimpleModel(null, bindingContext, result);

            // Assert
            Assert.False(bindingContext.ModelState.IsValidField("foo"));
            InvalidOperationException exception = Assert.IsType<InvalidOperationException>(bindingContext.ModelState["foo"].Errors[0].Exception);
            Assert.Equal("The parameter conversion from type 'System.String' to type 'System.Int32' failed. See the inner exception for more information.", exception.Message);
            Assert.Null(returnedValue);
        }

        [Fact]
        public void CreateComplexElementalModelBindingContext_ReadsBindAttributeFromBuddyClass()
        {
            // Arrange
            ModelBindingContext originalBindingContext = new ModelBindingContext()
            {
                ModelMetadata = new EmptyModelMetadataProvider().GetMetadataForType(null, typeof(CreateComplexElementalModelBindingContext_ReadsBindAttributeFromBuddyClass_Model)),
                ModelName = "someName",
                ValueProvider = new SimpleValueProvider()
            };

            DefaultModelBinder binder = new DefaultModelBinder();

            // Act
            ModelBindingContext newBindingContext = binder.CreateComplexElementalModelBindingContext(new ControllerContext(), originalBindingContext, null);

            // Assert
            Assert.True(newBindingContext.PropertyFilter("foo"));
            Assert.False(newBindingContext.PropertyFilter("bar"));
        }

        [MetadataType(typeof(CreateComplexElementalModelBindingContext_ReadsBindAttributeFromBuddyClass_Model_BuddyClass))]
        private class CreateComplexElementalModelBindingContext_ReadsBindAttributeFromBuddyClass_Model
        {
            [Bind(Include = "foo")]
            private class CreateComplexElementalModelBindingContext_ReadsBindAttributeFromBuddyClass_Model_BuddyClass
            {
            }
        }

        [Fact]
        public void CreateInstanceCreatesModelInstance()
        {
            // Arrange
            DefaultModelBinderHelper helper = new DefaultModelBinderHelper();

            // Act
            object modelObj = helper.PublicCreateModel(null, null, typeof(Guid));

            // Assert
            Assert.Equal(Guid.Empty, modelObj);
        }

        [Fact]
        public void CreateInstanceCreatesModelInstanceForGenericICollection()
        {
            // Arrange
            DefaultModelBinderHelper helper = new DefaultModelBinderHelper();

            // Act
            object modelObj = helper.PublicCreateModel(null, null, typeof(ICollection<Guid>));

            // Assert
            Assert.IsAssignableFrom<ICollection<Guid>>(modelObj);
        }

        [Fact]
        public void CreateInstanceCreatesModelInstanceForGenericIDictionary()
        {
            // Arrange
            DefaultModelBinderHelper helper = new DefaultModelBinderHelper();

            // Act
            object modelObj = helper.PublicCreateModel(null, null, typeof(IDictionary<string, Guid>));

            // Assert
            Assert.IsAssignableFrom<IDictionary<string, Guid>>(modelObj);
        }

        [Fact]
        public void CreateInstanceCreatesModelInstanceForGenericIEnumerable()
        {
            // Arrange
            DefaultModelBinderHelper helper = new DefaultModelBinderHelper();

            // Act
            object modelObj = helper.PublicCreateModel(null, null, typeof(IEnumerable<Guid>));

            // Assert
            Assert.IsAssignableFrom<ICollection<Guid>>(modelObj);
        }

        [Fact]
        public void CreateInstanceCreatesModelInstanceForGenericIList()
        {
            // Arrange
            DefaultModelBinderHelper helper = new DefaultModelBinderHelper();

            // Act
            object modelObj = helper.PublicCreateModel(null, null, typeof(IList<Guid>));

            // Assert
            Assert.IsAssignableFrom<IList<Guid>>(modelObj);
        }

        [Fact]
        public void CreateInstanceCreatesModelInstanceForGenericIListWithoutParameterlessConstructor()
        {
            // Arrange
            DefaultModelBinderHelper helper = new DefaultModelBinderHelper();

            // Act
            // No need for type parameter to have a parameterless constructor
            object modelObject = helper.PublicCreateModel(null, null, typeof(IList<NoParameterlessCtor>));

            // Assert
            Assert.IsAssignableFrom<IList<NoParameterlessCtor>>(modelObject);
        }

        [Fact]
        public void CreateInstanceThrowsWithoutParameterlessConstructor()
        {
            // Arrange
            DefaultModelBinderHelper helper = new DefaultModelBinderHelper();

            // Act & Assert, confirming type name and full stack are available in Exception
            MissingMethodException exception = Assert.Throws<MissingMethodException>(
                () => helper.PublicCreateModel(null, null, typeof(NoParameterlessCtor)),
                "No parameterless constructor defined for this object. Object type 'System.Web.Mvc.Test.DefaultModelBinderTest+NoParameterlessCtor'.");
            Assert.Contains("System.Activator.CreateInstance(", exception.ToString());
        }

        [Fact]
        public void CreateSubIndexNameReturnsPrefixPlusIndex()
        {
            // Arrange
            DefaultModelBinderHelper helper = new DefaultModelBinderHelper();

            // Act
            string newName = helper.PublicCreateSubIndexName("somePrefix", 2);

            // Assert
            Assert.Equal("somePrefix[2]", newName);
        }

        [Fact]
        public void CreateSubPropertyNameReturnsPrefixPlusPropertyName()
        {
            // Arrange
            DefaultModelBinderHelper helper = new DefaultModelBinderHelper();

            // Act
            string newName = helper.PublicCreateSubPropertyName("somePrefix", "someProperty");

            // Assert
            Assert.Equal("somePrefix.someProperty", newName);
        }

        [Fact]
        public void CreateSubPropertyNameReturnsPropertyNameIfPrefixIsEmpty()
        {
            // Arrange
            DefaultModelBinderHelper helper = new DefaultModelBinderHelper();

            // Act
            string newName = helper.PublicCreateSubPropertyName(String.Empty, "someProperty");

            // Assert
            Assert.Equal("someProperty", newName);
        }

        [Fact]
        public void CreateSubPropertyNameReturnsPropertyNameIfPrefixIsNull()
        {
            // Arrange
            DefaultModelBinderHelper helper = new DefaultModelBinderHelper();

            // Act
            string newName = helper.PublicCreateSubPropertyName(null, "someProperty");

            // Assert
            Assert.Equal("someProperty", newName);
        }

        [Fact]
        public void GetFilteredModelPropertiesFiltersNonUpdateableProperties()
        {
            // Arrange
            ModelBindingContext bindingContext = new ModelBindingContext()
            {
                ModelMetadata = ModelMetadataProviders.Current.GetMetadataForType(null, typeof(PropertyTestingModel)),
                PropertyFilter = new BindAttribute() { Exclude = "Blacklisted" }.IsPropertyAllowed
            };

            DefaultModelBinderHelper helper = new DefaultModelBinderHelper();

            // Act
            PropertyDescriptorCollection properties = new PropertyDescriptorCollection(helper.PublicGetFilteredModelProperties(null, bindingContext).ToArray());

            // Assert
            Assert.NotNull(properties["StringReadWrite"]);
            Assert.Null(properties["StringReadOnly"]);
            Assert.NotNull(properties["IntReadWrite"]);
            Assert.Null(properties["IntReadOnly"]);
            Assert.NotNull(properties["ArrayReadWrite"]);
            Assert.Null(properties["ArrayReadOnly"]);
            Assert.NotNull(properties["AddressReadWrite"]);
            Assert.NotNull(properties["AddressReadOnly"]);
            Assert.NotNull(properties["Whitelisted"]);
            Assert.Null(properties["Blacklisted"]);
            Assert.Equal(6, properties.Count);
        }

        [Fact]
        public void GetModelPropertiesReturnsUnfilteredPropertyList()
        {
            // Arrange
            ModelBindingContext bindingContext = new ModelBindingContext()
            {
                ModelMetadata = ModelMetadataProviders.Current.GetMetadataForType(null, typeof(PropertyTestingModel)),
                PropertyFilter = new BindAttribute() { Exclude = "Blacklisted" }.IsPropertyAllowed
            };

            DefaultModelBinderHelper helper = new DefaultModelBinderHelper();

            // Act
            PropertyDescriptorCollection properties = helper.PublicGetModelProperties(null, bindingContext);

            // Assert
            Assert.NotNull(properties["StringReadWrite"]);
            Assert.NotNull(properties["StringReadOnly"]);
            Assert.NotNull(properties["IntReadWrite"]);
            Assert.NotNull(properties["IntReadOnly"]);
            Assert.NotNull(properties["ArrayReadWrite"]);
            Assert.NotNull(properties["ArrayReadOnly"]);
            Assert.NotNull(properties["AddressReadWrite"]);
            Assert.NotNull(properties["AddressReadOnly"]);
            Assert.NotNull(properties["Whitelisted"]);
            Assert.NotNull(properties["Blacklisted"]);
            Assert.Equal(10, properties.Count);
        }

        [Fact]
        public void IsModelValidWithNullBindingContextThrows()
        {
            // Act & Assert
            Assert.ThrowsArgumentNull(
                () => DefaultModelBinderHelper.PublicIsModelValid(null),
                "bindingContext");
        }

        [Fact]
        public void IsModelValidReturnsModelStateIsValidWhenModelNameIsEmpty()
        {
            // Arrange
            ModelBindingContext contextWithNoErrors = new ModelBindingContext { ModelName = "" };
            ModelBindingContext contextWithErrors = new ModelBindingContext { ModelName = "" };
            contextWithErrors.ModelState.AddModelError("foo", "bar");

            // Act & Assert
            Assert.True(DefaultModelBinderHelper.PublicIsModelValid(contextWithNoErrors));
            Assert.False(DefaultModelBinderHelper.PublicIsModelValid(contextWithErrors));
        }

        [Fact]
        public void IsModelValidReturnsValidityOfSubModelStateWhenModelNameIsNotEmpty()
        {
            // Arrange
            ModelBindingContext contextWithNoErrors = new ModelBindingContext { ModelName = "foo" };
            ModelBindingContext contextWithErrors = new ModelBindingContext { ModelName = "foo" };
            contextWithErrors.ModelState.AddModelError("foo.bar", "baz");
            ModelBindingContext contextWithUnrelatedErrors = new ModelBindingContext { ModelName = "foo" };
            contextWithUnrelatedErrors.ModelState.AddModelError("biff", "baz");

            // Act & Assert
            Assert.True(DefaultModelBinderHelper.PublicIsModelValid(contextWithNoErrors));
            Assert.False(DefaultModelBinderHelper.PublicIsModelValid(contextWithErrors));
            Assert.True(DefaultModelBinderHelper.PublicIsModelValid(contextWithUnrelatedErrors));
        }

        [Fact]
        public void OnModelUpdatingReturnsTrue()
        {
            // By default, this method does nothing, so we just want to make sure it returns true

            // Arrange
            DefaultModelBinderHelper helper = new DefaultModelBinderHelper();

            // Act
            bool returned = helper.PublicOnModelUpdating(null, null);

            // Arrange
            Assert.True(returned);
        }

        // OnModelUpdated tests

        [Fact]
        public void OnModelUpdatedCalledWhenOnModelUpdatingReturnsTrue()
        {
            // Arrange
            ModelBindingContext bindingContext = new ModelBindingContext()
            {
                ModelMetadata = ModelMetadataProviders.Current.GetMetadataForType(() => new ModelWithoutBindAttribute(), typeof(ModelWithoutBindAttribute)),
                ModelName = "",
                ValueProvider = new SimpleValueProvider()
            };
            Mock<DefaultModelBinder> binder = new Mock<DefaultModelBinder> { CallBase = true };
            binder.Protected().Setup<bool>("OnModelUpdating",
                                           ItExpr.IsAny<ControllerContext>(), ItExpr.IsAny<ModelBindingContext>())
                .Returns(true);
            binder.Protected().Setup("OnModelUpdated",
                                     ItExpr.IsAny<ControllerContext>(), ItExpr.IsAny<ModelBindingContext>())
                .Verifiable();

            // Act
            binder.Object.BindModel(new ControllerContext(), bindingContext);

            // Assert
            binder.Verify();
        }

        [Fact]
        public void OnModelUpdatedNotCalledWhenOnModelUpdatingReturnsFalse()
        {
            // Arrange
            ModelBindingContext bindingContext = new ModelBindingContext()
            {
                ModelMetadata = ModelMetadataProviders.Current.GetMetadataForType(() => new ModelWithoutBindAttribute(), typeof(ModelWithoutBindAttribute)),
                ModelName = "",
                ValueProvider = new SimpleValueProvider()
            };
            Mock<DefaultModelBinder> binder = new Mock<DefaultModelBinder> { CallBase = true };
            binder.Protected().Setup<bool>("OnModelUpdating",
                                           ItExpr.IsAny<ControllerContext>(), ItExpr.IsAny<ModelBindingContext>())
                .Returns(false);

            // Act
            binder.Object.BindModel(new ControllerContext(), bindingContext);

            // Assert
            binder.Verify();
            binder.Protected().Verify("OnModelUpdated", Times.Never(),
                                      ItExpr.IsAny<ControllerContext>(), ItExpr.IsAny<ModelBindingContext>());
        }

        [Fact]
        public void OnModelUpdatedDoesntAddNewMessagesWhenMessagesAlreadyExist()
        {
            // Arrange
            var binder = new TestableDefaultModelBinder<SetPropertyModel>();
            binder.Context.ModelState.AddModelError(BASE_MODEL_NAME + ".NonNullableStringWithAttribute", "Some pre-existing error");

            // Act
            binder.OnModelUpdated();

            // Assert
            var modelState = binder.Context.ModelState[BASE_MODEL_NAME + ".NonNullableStringWithAttribute"];
            var error = Assert.Single(modelState.Errors);
            Assert.Equal("Some pre-existing error", error.ErrorMessage);
        }

        [Fact]
        public void OnPropertyValidatingNotCalledOnPropertiesWithErrors()
        {
            // Arrange
            ModelWithoutBindAttribute model = new ModelWithoutBindAttribute();
            ModelBindingContext bindingContext = new ModelBindingContext
            {
                ModelMetadata = ModelMetadataProviders.Current.GetMetadataForType(() => model, model.GetType()),
                ModelName = "",
                ValueProvider = new SimpleValueProvider()
                {
                    { "foo", "foo" }
                },
            };
            bindingContext.ModelState.AddModelError("foo", "Pre-existing error");
            Mock<DefaultModelBinder> binder = new Mock<DefaultModelBinder> { CallBase = true };

            // Act
            binder.Object.BindModel(new ControllerContext(), bindingContext);

            // Assert
            binder.Verify();
            binder.Protected().Verify("OnPropertyValidating", Times.Never(),
                                      ItExpr.IsAny<ControllerContext>(), ItExpr.IsAny<ModelBindingContext>(),
                                      ItExpr.IsAny<PropertyDescriptor>(), ItExpr.IsAny<object>());
        }

        public class ExtraValueModel
        {
            public int RequiredValue { get; set; }
        }

        [Fact]
        public void ExtraValueRequiredMessageNotAddedForAlreadyInvalidProperty()
        {
            // Arrange
            DefaultModelBinder binder = new DefaultModelBinder();
            ModelBindingContext bindingContext = new ModelBindingContext()
            {
                ModelMetadata = ModelMetadataProviders.Current.GetMetadataForType(null, typeof(MyModel)),
                ModelName = "theModel",
                ValueProvider = new SimpleValueProvider()
            };
            bindingContext.ModelState.AddModelError("theModel.ReadWriteProperty", "Existing Error Message");

            // Act
            binder.BindModel(new ControllerContext(), bindingContext);

            // Assert
            ModelState modelState = bindingContext.ModelState["theModel.ReadWriteProperty"];
            var error = Assert.Single(modelState.Errors);
            Assert.Equal("Existing Error Message", error.ErrorMessage);
        }

        [Fact]
        public void OnPropertyValidatingReturnsTrueOnSuccess()
        {
            // Arrange
            ModelBindingContext bindingContext = new ModelBindingContext()
            {
                ModelMetadata = ModelMetadataProviders.Current.GetMetadataForType(null, typeof(MyModel)),
                ModelName = "theModel"
            };

            PropertyDescriptor property = TypeDescriptor.GetProperties(typeof(MyModel))["ReadWriteProperty"];
            DefaultModelBinderHelper helper = new DefaultModelBinderHelper();
            bindingContext.PropertyMetadata["ReadWriteProperty"].Model = 42;

            // Act
            bool returned = helper.PublicOnPropertyValidating(new ControllerContext(), bindingContext, property, 42);

            // Assert
            Assert.True(returned);
            Assert.Empty(bindingContext.ModelState);
        }

        [Fact]
        public void UpdateCollectionCreatesDefaultEntriesForInvalidElements()
        {
            // Arrange
            List<int> model = new List<int>() { 4, 5, 6, 7, 8 };
            ModelBindingContext bindingContext = new ModelBindingContext()
            {
                ModelMetadata = ModelMetadataProviders.Current.GetMetadataForType(() => model, model.GetType()),
                ModelName = "foo",
                ValueProvider = new SimpleValueProvider()
                {
                    { "foo[0]", null },
                    { "foo[1]", null },
                    { "foo[2]", null }
                }
            };

            Mock<IModelBinder> mockInnerBinder = new Mock<IModelBinder>();
            mockInnerBinder
                .Setup(b => b.BindModel(It.IsAny<ControllerContext>(), It.IsAny<ModelBindingContext>()))
                .Returns(
                    delegate(ControllerContext cc, ModelBindingContext bc)
                    {
                        int fooIdx = Int32.Parse(bc.ModelName.Substring(4, 1), CultureInfo.InvariantCulture);
                        return (fooIdx == 1) ? (object)null : fooIdx;
                    });

            DefaultModelBinder binder = new DefaultModelBinder()
            {
                Binders = new ModelBinderDictionary()
                {
                    { typeof(int), mockInnerBinder.Object }
                }
            };

            // Act
            object updatedModel = binder.UpdateCollection(null, bindingContext, typeof(int));

            // Assert
            Assert.Equal(3, model.Count);
            Assert.False(bindingContext.ModelState.IsValidField("foo[1]"));
            Assert.Equal("A value is required.", bindingContext.ModelState["foo[1]"].Errors[0].ErrorMessage);
            Assert.Equal(0, model[0]);
            Assert.Equal(0, model[1]);
            Assert.Equal(2, model[2]);
        }

        [Fact]
        public void UpdateCollectionReturnsModifiedCollectionOnSuccess_ExplicitIndex()
        {
            // Arrange
            ControllerContext controllerContext = new Mock<ControllerContext>().Object;

            List<int> model = new List<int>() { 4, 5, 6, 7, 8 };
            ModelBindingContext bindingContext = new ModelBindingContext()
            {
                ModelMetadata = ModelMetadataProviders.Current.GetMetadataForType(() => model, model.GetType()),
                ModelName = "foo",
                PropertyFilter = _ => false,
                ValueProvider = new SimpleValueProvider()
                {
                    { "foo.index", new string[] { "alpha", "bravo", "charlie" } }, // 'bravo' will be skipped
                    { "foo[alpha]", "10" },
                    { "foo[charlie]", "30" }
                }
            };

            DefaultModelBinder binder = new DefaultModelBinder();

            // Act
            object updatedModel = binder.UpdateCollection(controllerContext, bindingContext, typeof(int));

            // Assert
            Assert.Same(model, updatedModel);
            Assert.Equal(2, model.Count);
            Assert.Equal(10, model[0]);
            Assert.Equal(30, model[1]);
        }

        [Fact]
        public void UpdateCollectionReturnsModifiedCollectionOnSuccess_ZeroBased()
        {
            // Arrange
            ControllerContext controllerContext = new Mock<ControllerContext>().Object;

            List<int> model = new List<int>() { 4, 5, 6, 7, 8 };
            ModelBindingContext bindingContext = new ModelBindingContext()
            {
                ModelMetadata = ModelMetadataProviders.Current.GetMetadataForType(() => model, model.GetType()),
                ModelName = "foo",
                PropertyFilter = _ => false,
                ValueProvider = new SimpleValueProvider()
                {
                    { "foo[0]", null },
                    { "foo[1]", null },
                    { "foo[2]", null }
                }
            };

            Mock<IModelBinder> mockInnerBinder = new Mock<IModelBinder>();
            mockInnerBinder
                .Setup(b => b.BindModel(It.IsAny<ControllerContext>(), It.IsAny<ModelBindingContext>()))
                .Returns(
                    delegate(ControllerContext cc, ModelBindingContext bc)
                    {
                        Assert.Equal(controllerContext, cc);
                        Assert.Equal(typeof(int), bc.ModelType);
                        Assert.Equal(bindingContext.ModelState, bc.ModelState);
                        Assert.Equal(bindingContext.PropertyFilter, bc.PropertyFilter);
                        Assert.Equal(bindingContext.ValueProvider, bc.ValueProvider);
                        return Int32.Parse(bc.ModelName.Substring(4, 1), CultureInfo.InvariantCulture);
                    });

            DefaultModelBinder binder = new DefaultModelBinder()
            {
                Binders = new ModelBinderDictionary()
                {
                    { typeof(int), mockInnerBinder.Object }
                }
            };

            // Act
            object updatedModel = binder.UpdateCollection(controllerContext, bindingContext, typeof(int));

            // Assert
            Assert.Same(model, updatedModel);
            Assert.Equal(3, model.Count);
            Assert.Equal(0, model[0]);
            Assert.Equal(1, model[1]);
            Assert.Equal(2, model[2]);
        }

        [Fact]
        public void UpdateCollectionReturnsNullIfZeroIndexNotFound()
        {
            // Arrange
            ModelBindingContext bindingContext = new ModelBindingContext()
            {
                ValueProvider = new SimpleValueProvider()
            };
            DefaultModelBinder binder = new DefaultModelBinder();

            // Act
            object updatedModel = binder.UpdateCollection(null, bindingContext, typeof(object));

            // Assert
            Assert.Null(updatedModel);
        }

        [Fact]
        public void UpdateDictionaryCreatesDefaultEntriesForInvalidValues()
        {
            // Arrange
            Dictionary<string, int> model = new Dictionary<string, int>
            {
                { "one", 1 },
                { "two", 2 }
            };
            ModelBindingContext bindingContext = new ModelBindingContext()
            {
                ModelMetadata = ModelMetadataProviders.Current.GetMetadataForType(() => model, model.GetType()),
                ModelName = "foo",
                ValueProvider = new SimpleValueProvider()
                {
                    { "foo[0].key", null }, { "foo[0].value", null },
                    { "foo[1].key", null }, { "foo[1].value", null },
                    { "foo[2].key", null }, { "foo[2].value", null }
                }
            };

            Mock<IModelBinder> mockStringBinder = new Mock<IModelBinder>();
            mockStringBinder
                .Setup(b => b.BindModel(It.IsAny<ControllerContext>(), It.IsAny<ModelBindingContext>()))
                .Returns(
                    delegate(ControllerContext cc, ModelBindingContext bc) { return (Int32.Parse(bc.ModelName.Substring(4, 1), CultureInfo.InvariantCulture) + 10) + "Value"; });

            Mock<IModelBinder> mockIntBinder = new Mock<IModelBinder>();
            mockIntBinder
                .Setup(b => b.BindModel(It.IsAny<ControllerContext>(), It.IsAny<ModelBindingContext>()))
                .Returns(
                    delegate(ControllerContext cc, ModelBindingContext bc)
                    {
                        int fooIdx = Int32.Parse(bc.ModelName.Substring(4, 1), CultureInfo.InvariantCulture);
                        return (fooIdx == 1) ? (object)null : fooIdx;
                    });

            DefaultModelBinder binder = new DefaultModelBinder()
            {
                Binders = new ModelBinderDictionary()
                {
                    { typeof(string), mockStringBinder.Object },
                    { typeof(int), mockIntBinder.Object }
                }
            };

            // Act
            object updatedModel = binder.UpdateDictionary(null, bindingContext, typeof(string), typeof(int));

            // Assert
            Assert.Equal(3, model.Count);
            Assert.False(bindingContext.ModelState.IsValidField("foo[1].value"));
            Assert.Equal("A value is required.", bindingContext.ModelState["foo[1].value"].Errors[0].ErrorMessage);
            Assert.Equal(0, model["10Value"]);
            Assert.Equal(0, model["11Value"]);
            Assert.Equal(2, model["12Value"]);
        }

        [Fact]
        public void UpdateDictionaryReturnsModifiedDictionaryOnSuccess_ExplicitIndex()
        {
            // Arrange
            ControllerContext controllerContext = new Mock<ControllerContext>().Object;

            Dictionary<int, string> model = new Dictionary<int, string>
            {
                { 1, "one" },
                { 2, "two" }
            };
            ModelBindingContext bindingContext = new ModelBindingContext()
            {
                ModelMetadata = ModelMetadataProviders.Current.GetMetadataForType(() => model, model.GetType()),
                ModelName = "foo",
                PropertyFilter = _ => false,
                ValueProvider = new SimpleValueProvider()
                {
                    { "foo.index", new string[] { "alpha", "bravo", "charlie" } }, // 'bravo' will be skipped
                    { "foo[alpha].key", "10" }, { "foo[alpha].value", "ten" },
                    { "foo[charlie].key", "30" }, { "foo[charlie].value", "thirty" }
                }
            };

            DefaultModelBinder binder = new DefaultModelBinder();

            // Act
            object updatedModel = binder.UpdateDictionary(controllerContext, bindingContext, typeof(int), typeof(string));

            // Assert
            Assert.Same(model, updatedModel);
            Assert.Equal(2, model.Count);
            Assert.Equal("ten", model[10]);
            Assert.Equal("thirty", model[30]);
        }

        [Fact]
        public void UpdateDictionaryReturnsModifiedDictionaryOnSuccess_ZeroBased()
        {
            // Arrange
            ControllerContext controllerContext = new Mock<ControllerContext>().Object;

            Dictionary<int, string> model = new Dictionary<int, string>
            {
                { 1, "one" },
                { 2, "two" }
            };
            ModelBindingContext bindingContext = new ModelBindingContext()
            {
                ModelMetadata = ModelMetadataProviders.Current.GetMetadataForType(() => model, model.GetType()),
                ModelName = "foo",
                PropertyFilter = _ => false,
                ValueProvider = new SimpleValueProvider()
                {
                    { "foo[0].key", null }, { "foo[0].value", null },
                    { "foo[1].key", null }, { "foo[1].value", null },
                    { "foo[2].key", null }, { "foo[2].value", null }
                }
            };

            Mock<IModelBinder> mockIntBinder = new Mock<IModelBinder>();
            mockIntBinder
                .Setup(b => b.BindModel(It.IsAny<ControllerContext>(), It.IsAny<ModelBindingContext>()))
                .Returns(
                    delegate(ControllerContext cc, ModelBindingContext bc)
                    {
                        Assert.Equal(controllerContext, cc);
                        Assert.Equal(typeof(int), bc.ModelType);
                        Assert.Equal(bindingContext.ModelState, bc.ModelState);
                        Assert.Equal(new ModelBindingContext().PropertyFilter, bc.PropertyFilter);
                        Assert.Equal(bindingContext.ValueProvider, bc.ValueProvider);
                        return Int32.Parse(bc.ModelName.Substring(4, 1), CultureInfo.InvariantCulture) + 10;
                    });

            Mock<IModelBinder> mockStringBinder = new Mock<IModelBinder>();
            mockStringBinder
                .Setup(b => b.BindModel(It.IsAny<ControllerContext>(), It.IsAny<ModelBindingContext>()))
                .Returns(
                    delegate(ControllerContext cc, ModelBindingContext bc)
                    {
                        Assert.Equal(controllerContext, cc);
                        Assert.Equal(typeof(string), bc.ModelType);
                        Assert.Equal(bindingContext.ModelState, bc.ModelState);
                        Assert.Equal(bindingContext.PropertyFilter, bc.PropertyFilter);
                        Assert.Equal(bindingContext.ValueProvider, bc.ValueProvider);
                        return (Int32.Parse(bc.ModelName.Substring(4, 1), CultureInfo.InvariantCulture) + 10) + "Value";
                    });

            DefaultModelBinder binder = new DefaultModelBinder()
            {
                Binders = new ModelBinderDictionary()
                {
                    { typeof(int), mockIntBinder.Object },
                    { typeof(string), mockStringBinder.Object }
                }
            };

            // Act
            object updatedModel = binder.UpdateDictionary(controllerContext, bindingContext, typeof(int), typeof(string));

            // Assert
            Assert.Same(model, updatedModel);
            Assert.Equal(3, model.Count);
            Assert.Equal("10Value", model[10]);
            Assert.Equal("11Value", model[11]);
            Assert.Equal("12Value", model[12]);
        }

        [Fact]
        public void UpdateDictionaryReturnsEmptyDictionaryIfNoValidElementsFound()
        {
            // Arrange
            Mock<ControllerContext> mockController = new Mock<ControllerContext>();

            mockController
                .Setup(c => c.RouteData)
                .Returns(
                    delegate()
                    {
                        RouteData routeData = new RouteData();
                        routeData.Values["controller"] = "controller";
                        return routeData;
                    }
            );

            ControllerContext controllerContext = mockController.Object;

            Dictionary<int, string> model = new Dictionary<int, string>();

            RouteDataValueProviderFactory routeDataFactory = new RouteDataValueProviderFactory();

            ModelBindingContext bindingContext = new ModelBindingContext()
            {
                ModelMetadata = ModelMetadataProviders.Current.GetMetadataForType(() => model, model.GetType()),
                ValueProvider = routeDataFactory.GetValueProvider(controllerContext)
            };

            DefaultModelBinder binder = new DefaultModelBinder();

            // Act
            Dictionary<int, string> updatedModel = binder.UpdateDictionary(controllerContext, bindingContext, typeof(int), typeof(string)) as Dictionary<int, string>;

            // Assert
            Assert.NotNull(updatedModel);
            Assert.Empty(updatedModel);
        }

        [Fact]
        public void UpdateDictionarySkipsInvalidKeys()
        {
            // Arrange
            Dictionary<int, string> model = new Dictionary<int, string>
            {
                { 1, "one" },
                { 2, "two" }
            };
            ModelBindingContext bindingContext = new ModelBindingContext()
            {
                ModelMetadata = ModelMetadataProviders.Current.GetMetadataForType(() => model, model.GetType()),
                ModelName = "foo",
                ValueProvider = new SimpleValueProvider()
                {
                    { "foo[0].key", null }, { "foo[0].value", null },
                    { "foo[1].key", null }, { "foo[1].value", null },
                    { "foo[2].key", null }, { "foo[2].value", null }
                }
            };

            Mock<IModelBinder> mockIntBinder = new Mock<IModelBinder>();
            mockIntBinder
                .Setup(b => b.BindModel(It.IsAny<ControllerContext>(), It.IsAny<ModelBindingContext>()))
                .Returns(
                    delegate(ControllerContext cc, ModelBindingContext bc)
                    {
                        int fooIdx = Int32.Parse(bc.ModelName.Substring(4, 1), CultureInfo.InvariantCulture);
                        return (fooIdx == 1) ? (object)null : fooIdx;
                    });

            Mock<IModelBinder> mockStringBinder = new Mock<IModelBinder>();
            mockStringBinder
                .Setup(b => b.BindModel(It.IsAny<ControllerContext>(), It.IsAny<ModelBindingContext>()))
                .Returns(
                    delegate(ControllerContext cc, ModelBindingContext bc) { return (Int32.Parse(bc.ModelName.Substring(4, 1), CultureInfo.InvariantCulture) + 10) + "Value"; });

            DefaultModelBinder binder = new DefaultModelBinder()
            {
                Binders = new ModelBinderDictionary()
                {
                    { typeof(int), mockIntBinder.Object },
                    { typeof(string), mockStringBinder.Object }
                }
            };

            // Act
            object updatedModel = binder.UpdateDictionary(null, bindingContext, typeof(int), typeof(string));

            // Assert
            Assert.Equal(2, model.Count);
            Assert.False(bindingContext.ModelState.IsValidField("foo[1].key"));
            Assert.Equal("A value is required.", bindingContext.ModelState["foo[1].key"].Errors[0].ErrorMessage);
            Assert.Equal("10Value", model[0]);
            Assert.Equal("12Value", model[2]);
        }

        [ModelBinder(typeof(DefaultModelBinder))]
        private class MyModel
        {
            public int ReadOnlyProperty
            {
                get { return 4; }
            }

            public int ReadWriteProperty { get; set; }
            public int ReadWriteProperty2 { get; set; }
        }

        private class MyClassWithoutConverter
        {
        }

        [Bind(Exclude = "Alpha,Echo")]
        private class MyOtherModel
        {
            public string Alpha { get; set; }
            public string Bravo { get; set; }
            public string Charlie { get; set; }
            public string Delta { get; set; }
            public string Echo { get; set; }
            public string Foxtrot { get; set; }
        }

        public class Customer
        {
            private Address _address = new Address();

            public Address Address
            {
                get { return _address; }
            }
        }

        public class Address
        {
            public string Street { get; set; }
            public string Zip { get; set; }
        }

        public class IntegerContainer
        {
            public int Integer { get; set; }
            public int? NullableInteger { get; set; }
        }

        [TypeConverter(typeof(CultureAwareConverter))]
        public class StringContainer
        {
            public StringContainer(string value)
            {
                Value = value;
            }

            public string Value { get; private set; }
        }

        private class CultureAwareConverter : TypeConverter
        {
            public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
            {
                return (sourceType == typeof(string));
            }

            public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
            {
                return (destinationType == typeof(string));
            }

            public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
            {
                string stringValue = value as string;
                if (stringValue == null || stringValue.Length < 3)
                {
                    throw new Exception("Value must have at least 3 characters.");
                }
                return new StringContainer(AppendCultureName(stringValue, culture));
            }

            public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
            {
                StringContainer container = value as StringContainer;
                if (container.Value == null || container.Value.Length < 3)
                {
                    throw new Exception("Value must have at least 3 characters.");
                }

                return AppendCultureName(container.Value, culture);
            }

            private static string AppendCultureName(string value, CultureInfo culture)
            {
                string cultureName = (!String.IsNullOrEmpty(culture.Name)) ? culture.Name : culture.ThreeLetterWindowsLanguageName;
                return value + " (" + cultureName + ")";
            }
        }

        [ModelBinder(typeof(MyStringModelBinder))]
        private class MyStringModel
        {
            public string Value { get; set; }
        }

        private class MyStringModelBinder : IModelBinder
        {
            public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
            {
                MyStringModel castModel = bindingContext.Model as MyStringModel;
                if (castModel != null)
                {
                    castModel.Value += "_Update";
                }
                else
                {
                    castModel = new MyStringModel() { Value = bindingContext.ModelName + "_Create" };
                }
                return castModel;
            }
        }

        private class CustomUnvalidatedValueProvider : IUnvalidatedValueProvider
        {
            public ValueProviderResult GetValue(string key, bool skipValidation)
            {
                string newValue = key + ((skipValidation) ? "Unvalidated" : "Validated");
                return new ValueProviderResult(newValue, newValue, null);
            }

            public bool ContainsPrefix(string prefix)
            {
                return true;
            }

            public ValueProviderResult GetValue(string key)
            {
                return GetValue(key, skipValidation: false);
            }
        }

        private class SimpleController : Controller
        {
        }

        public class DefaultModelBinderHelper : DefaultModelBinder
        {
            public virtual void PublicBindProperty(ControllerContext controllerContext, ModelBindingContext bindingContext, PropertyDescriptor property)
            {
                base.BindProperty(controllerContext, bindingContext, property);
            }

            protected override void BindProperty(ControllerContext controllerContext, ModelBindingContext bindingContext, PropertyDescriptor property)
            {
                PublicBindProperty(controllerContext, bindingContext, property);
            }

            public virtual object PublicCreateModel(ControllerContext controllerContext, ModelBindingContext bindingContext, Type modelType)
            {
                return base.CreateModel(controllerContext, bindingContext, modelType);
            }

            protected override object CreateModel(ControllerContext controllerContext, ModelBindingContext bindingContext, Type modelType)
            {
                return PublicCreateModel(controllerContext, bindingContext, modelType);
            }

            public virtual IEnumerable<PropertyDescriptor> PublicGetFilteredModelProperties(ControllerContext controllerContext, ModelBindingContext bindingContext)
            {
                return base.GetFilteredModelProperties(controllerContext, bindingContext);
            }

            public virtual PropertyDescriptorCollection PublicGetModelProperties(ControllerContext controllerContext, ModelBindingContext bindingContext)
            {
                return base.GetModelProperties(controllerContext, bindingContext);
            }

            protected override PropertyDescriptorCollection GetModelProperties(ControllerContext controllerContext, ModelBindingContext bindingContext)
            {
                return PublicGetModelProperties(controllerContext, bindingContext);
            }

            public string PublicCreateSubIndexName(string prefix, int indexName)
            {
                return CreateSubIndexName(prefix, indexName);
            }

            public string PublicCreateSubPropertyName(string prefix, string propertyName)
            {
                return CreateSubPropertyName(prefix, propertyName);
            }

            public static bool PublicIsModelValid(ModelBindingContext bindingContext)
            {
                return IsModelValid(bindingContext);
            }

            public virtual bool PublicOnModelUpdating(ControllerContext controllerContext, ModelBindingContext bindingContext)
            {
                return base.OnModelUpdating(controllerContext, bindingContext);
            }

            protected override bool OnModelUpdating(ControllerContext controllerContext, ModelBindingContext bindingContext)
            {
                return PublicOnModelUpdating(controllerContext, bindingContext);
            }

            public virtual void PublicOnModelUpdated(ControllerContext controllerContext, ModelBindingContext bindingContext)
            {
                base.OnModelUpdated(controllerContext, bindingContext);
            }

            protected override void OnModelUpdated(ControllerContext controllerContext, ModelBindingContext bindingContext)
            {
                PublicOnModelUpdated(controllerContext, bindingContext);
            }

            public virtual bool PublicOnPropertyValidating(ControllerContext controllerContext, ModelBindingContext bindingContext, PropertyDescriptor property, object value)
            {
                return base.OnPropertyValidating(controllerContext, bindingContext, property, value);
            }

            protected override bool OnPropertyValidating(ControllerContext controllerContext, ModelBindingContext bindingContext, PropertyDescriptor property, object value)
            {
                return PublicOnPropertyValidating(controllerContext, bindingContext, property, value);
            }

            public virtual void PublicOnPropertyValidated(ControllerContext controllerContext, ModelBindingContext bindingContext, PropertyDescriptor property, object value)
            {
                base.OnPropertyValidated(controllerContext, bindingContext, property, value);
            }

            protected override void OnPropertyValidated(ControllerContext controllerContext, ModelBindingContext bindingContext, PropertyDescriptor property, object value)
            {
                PublicOnPropertyValidated(controllerContext, bindingContext, property, value);
            }

            public virtual void PublicSetProperty(ControllerContext controllerContext, ModelBindingContext bindingContext, PropertyDescriptor property, object value)
            {
                base.SetProperty(controllerContext, bindingContext, property, value);
            }

            protected override void SetProperty(ControllerContext controllerContext, ModelBindingContext bindingContext, PropertyDescriptor property, object value)
            {
                PublicSetProperty(controllerContext, bindingContext, property, value);
            }
        }

        private class MyModel2
        {
            private int _intReadWriteNonNegative;

            public int IntReadOnly
            {
                get { return 4; }
            }

            public int IntReadWrite { get; set; }

            public int IntReadWriteNonNegative
            {
                get { return _intReadWriteNonNegative; }
                set
                {
                    if (value < 0)
                    {
                        throw new ArgumentOutOfRangeException("value", "Value must be non-negative.");
                    }
                    _intReadWriteNonNegative = value;
                }
            }

            public int? NullableIntReadWrite { get; set; }
        }

        [Bind(Exclude = "Foo")]
        private class ModelWithBindAttribute : ModelWithoutBindAttribute
        {
        }

        private class ModelWithoutBindAttribute
        {
            public string Foo { get; set; }
            public string Bar { get; set; }
            public string Baz { get; set; }
        }

        private class PropertyTestingModel
        {
            public string StringReadWrite { get; set; }
            public string StringReadOnly { get; private set; }
            public int IntReadWrite { get; set; }
            public int IntReadOnly { get; private set; }
            public object[] ArrayReadWrite { get; set; }
            public object[] ArrayReadOnly { get; private set; }
            public Address AddressReadWrite { get; set; }
            public Address AddressReadOnly { get; private set; }
            public string Whitelisted { get; set; }
            public string Blacklisted { get; set; }
        }

        // --------------------------------------------------------------------------------
        //  DataAnnotations tests

        public const string BASE_MODEL_NAME = "BaseModelName";

        // GetModelProperties tests

        [MetadataType(typeof(Metadata))]
        class GetModelPropertiesModel
        {
            [Required]
            public int LocalAttributes { get; set; }

            public int MetadataAttributes { get; set; }

            [Required]
            public int MixedAttributes { get; set; }

            class Metadata
            {
                [Range(10, 100)]
                public int MetadataAttributes { get; set; }

                [Range(10, 100)]
                public int MixedAttributes { get; set; }
            }
        }

        [Fact]
        public void GetModelPropertiesWithLocalAttributes()
        {
            // Arrange
            TestableDefaultModelBinder<GetModelPropertiesModel> modelBinder = new TestableDefaultModelBinder<GetModelPropertiesModel>();

            // Act
            PropertyDescriptor property = modelBinder.GetModelProperties()
                .Cast<PropertyDescriptor>()
                .Where(pd => pd.Name == "LocalAttributes")
                .Single();

            // Assert
            Assert.Contains(property.Attributes.Cast<Attribute>(), a => a is RequiredAttribute);
        }

        [Fact]
        public void GetModelPropertiesWithMetadataAttributes()
        {
            // Arrange
            TestableDefaultModelBinder<GetModelPropertiesModel> modelBinder = new TestableDefaultModelBinder<GetModelPropertiesModel>();

            // Act
            PropertyDescriptor property = modelBinder.GetModelProperties()
                .Cast<PropertyDescriptor>()
                .Where(pd => pd.Name == "MetadataAttributes")
                .Single();

            // Assert
            Assert.Contains(property.Attributes.Cast<Attribute>(), a => a is RangeAttribute);
        }

        [Fact]
        public void GetModelPropertiesWithMixedAttributes()
        {
            // Arrange
            TestableDefaultModelBinder<GetModelPropertiesModel> modelBinder = new TestableDefaultModelBinder<GetModelPropertiesModel>();

            // Act
            PropertyDescriptor property = modelBinder.GetModelProperties()
                .Cast<PropertyDescriptor>()
                .Where(pd => pd.Name == "MixedAttributes")
                .Single();

            // Assert
            Assert.Contains(property.Attributes.Cast<Attribute>(), a => a is RequiredAttribute);
            Assert.Contains(property.Attributes.Cast<Attribute>(), a => a is RangeAttribute);
        }

        // GetPropertyValue tests

        class GetPropertyValueModel
        {
            public string NoAttribute { get; set; }

            [DisplayFormat(ConvertEmptyStringToNull = false)]
            public string AttributeWithoutConversion { get; set; }

            [DisplayFormat(ConvertEmptyStringToNull = true)]
            public string AttributeWithConversion { get; set; }
        }

        [Fact]
        public void GetPropertyValueWithNoAttributeConvertsEmptyStringToNull()
        {
            // Arrange
            TestableDefaultModelBinder<GetPropertyValueModel> binder = new TestableDefaultModelBinder<GetPropertyValueModel>();
            binder.Context.ModelMetadata = binder.Context.PropertyMetadata["NoAttribute"];

            // Act
            object result = binder.GetPropertyValue("NoAttribute", String.Empty);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetPropertyValueWithFalseAttributeDoesNotConvertEmptyStringToNull()
        {
            // Arrange
            TestableDefaultModelBinder<GetPropertyValueModel> binder = new TestableDefaultModelBinder<GetPropertyValueModel>();
            binder.Context.ModelMetadata = binder.Context.PropertyMetadata["AttributeWithoutConversion"];

            // Act
            object result = binder.GetPropertyValue("AttributeWithoutConversion", String.Empty);

            // Assert
            Assert.Equal(String.Empty, result);
        }

        [Fact]
        public void GetPropertyValueWithTrueAttributeConvertsEmptyStringToNull()
        {
            // Arrange
            TestableDefaultModelBinder<GetPropertyValueModel> binder = new TestableDefaultModelBinder<GetPropertyValueModel>();
            binder.Context.ModelMetadata = binder.Context.PropertyMetadata["AttributeWithConversion"];

            // Act
            object result = binder.GetPropertyValue("AttributeWithConversion", String.Empty);

            // Assert
            Assert.Null(result);
        }

        // OnModelUpdated tests

        [Fact]
        public void OnModelUpdatedPassesNullContainerToValidate()
        {
            Mock<ModelValidatorProvider> provider = null;

            try
            {
                // Arrange
                ModelMetadata metadata = ModelMetadataProviders.Current.GetMetadataForType(null, typeof(object));
                ControllerContext context = new ControllerContext();
                Mock<ModelValidator> validator = new Mock<ModelValidator>(metadata, context);
                provider = new Mock<ModelValidatorProvider>();
                provider.Setup(p => p.GetValidators(It.IsAny<ModelMetadata>(), It.IsAny<ControllerContext>()))
                    .Returns(new ModelValidator[] { validator.Object });
                ModelValidatorProviders.Providers.Add(provider.Object);
                object model = new object();
                TestableDefaultModelBinder<object> modelBinder = new TestableDefaultModelBinder<object>(model);

                // Act
                modelBinder.OnModelUpdated();

                // Assert
                validator.Verify(v => v.Validate(null));
            }
            finally
            {
                if (provider != null)
                {
                    ModelValidatorProviders.Providers.Remove(provider.Object);
                }
            }
        }

        public class MinMaxValidationAttribute : ValidationAttribute
        {
            public MinMaxValidationAttribute()
                : base("Minimum must be less than or equal to Maximum")
            {
            }

            public override bool IsValid(object value)
            {
                OnModelUpdatedModelMultipleParameters model = (OnModelUpdatedModelMultipleParameters)value;
                return model.Minimum <= model.Maximum;
            }
        }

        [MinMaxValidation]
        public class OnModelUpdatedModelMultipleParameters
        {
            public int Minimum { get; set; }
            public int Maximum { get; set; }
        }

        [Fact]
        public void OnModelUpdatedWithValidationAttributeMultipleParameters()
        {
            // Arrange
            OnModelUpdatedModelMultipleParameters model = new OnModelUpdatedModelMultipleParameters { Minimum = 250, Maximum = 100 };
            TestableDefaultModelBinder<OnModelUpdatedModelMultipleParameters> modelBinder = new TestableDefaultModelBinder<OnModelUpdatedModelMultipleParameters>(model);

            // Act
            modelBinder.OnModelUpdated();

            // Assert
            Assert.Single(modelBinder.ModelState);
            ModelState stateModel = modelBinder.ModelState[BASE_MODEL_NAME];
            Assert.NotNull(stateModel);
            ModelError error = Assert.Single(stateModel.Errors);
            Assert.Equal("Minimum must be less than or equal to Maximum", error.ErrorMessage);
        }

        [Fact]
        public void OnModelUpdatedWithInvalidPropertyValidationWillNotRunEntityLevelValidation()
        {
            // Arrange
            OnModelUpdatedModelMultipleParameters model = new OnModelUpdatedModelMultipleParameters { Minimum = 250, Maximum = 100 };
            TestableDefaultModelBinder<OnModelUpdatedModelMultipleParameters> modelBinder = new TestableDefaultModelBinder<OnModelUpdatedModelMultipleParameters>(model);
            modelBinder.ModelState.AddModelError(BASE_MODEL_NAME + ".Minimum", "The minimum value was invalid.");

            // Act
            modelBinder.OnModelUpdated();

            // Assert
            Assert.Null(modelBinder.ModelState[BASE_MODEL_NAME]);
        }

        public class AlwaysInvalidAttribute : ValidationAttribute
        {
            public AlwaysInvalidAttribute()
            {
            }

            public AlwaysInvalidAttribute(string message)
                : base(message)
            {
            }

            public override bool IsValid(object value)
            {
                return false;
            }
        }

        [AlwaysInvalid("The object just isn't right")]
        public class OnModelUpdatedModelNoParameters
        {
        }

        [Fact]
        public void OnModelUpdatedWithValidationAttributeNoParameters()
        {
            // Arrange
            TestableDefaultModelBinder<OnModelUpdatedModelNoParameters> modelBinder = new TestableDefaultModelBinder<OnModelUpdatedModelNoParameters>();

            // Act
            modelBinder.OnModelUpdated();

            // Assert
            Assert.Single(modelBinder.ModelState);
            ModelState stateModel = modelBinder.ModelState[BASE_MODEL_NAME];
            Assert.NotNull(stateModel);
            ModelError error = Assert.Single(stateModel.Errors);
            Assert.Equal("The object just isn't right", error.ErrorMessage);
        }

        [AlwaysInvalid]
        public class OnModelUpdatedModelNoValidationResult
        {
        }

        [Fact]
        [ReplaceCulture]
        public void OnModelUpdatedWithValidationAttributeNoValidationMessage()
        {
            // Arrange
            TestableDefaultModelBinder<OnModelUpdatedModelNoValidationResult> modelBinder = new TestableDefaultModelBinder<OnModelUpdatedModelNoValidationResult>();

            // Act
            modelBinder.OnModelUpdated();

            // Assert
            Assert.Single(modelBinder.ModelState);
            ModelState stateModel = modelBinder.ModelState[BASE_MODEL_NAME];
            Assert.NotNull(stateModel);
            ModelError error = Assert.Single(stateModel.Errors);
            Assert.Equal("The field OnModelUpdatedModelNoValidationResult is invalid.", error.ErrorMessage);
        }

        [Fact]
        public void OnModelUpdatedDoesNotPlaceErrorMessagesInModelStateWhenSubPropertiesHaveErrors()
        {
            // Arrange
            TestableDefaultModelBinder<OnModelUpdatedModelNoValidationResult> modelBinder = new TestableDefaultModelBinder<OnModelUpdatedModelNoValidationResult>();
            modelBinder.ModelState.AddModelError("Foo.Bar", "Foo.Bar is invalid");
            modelBinder.Context.ModelName = "Foo";

            // Act
            modelBinder.OnModelUpdated();

            // Assert
            Assert.Null(modelBinder.ModelState["Foo"]);
        }

        // OnPropertyValidating tests

        class OnPropertyValidatingModel
        {
            public string NotValidated { get; set; }

            [Range(10, 65)]
            public int RangedInteger { get; set; }

            [Required(ErrorMessage = "Custom Required Message")]
            public int RequiredInteger { get; set; }
        }

        [Fact]
        public void OnPropertyValidatingWithoutValidationAttribute()
        {
            // Arrange
            TestableDefaultModelBinder<OnPropertyValidatingModel> modelBinder = new TestableDefaultModelBinder<OnPropertyValidatingModel>();

            // Act
            modelBinder.OnPropertyValidating("NotValidated", 42);

            // Assert
            Assert.Empty(modelBinder.ModelState);
        }

        [Fact]
        public void OnPropertyValidatingWithValidationAttributePassing()
        {
            // Arrange
            TestableDefaultModelBinder<OnPropertyValidatingModel> modelBinder = new TestableDefaultModelBinder<OnPropertyValidatingModel>();
            modelBinder.Context.PropertyMetadata["RangedInteger"].Model = 42;

            // Act
            bool result = modelBinder.OnPropertyValidating("RangedInteger", 42);

            // Assert
            Assert.True(result);
            Assert.Empty(modelBinder.ModelState);
        }

        // SetProperty tests

        [Fact]
        public void SetPropertyWithRequiredOnValueTypeOnlyResultsInSingleMessage()
        { // DDB #225150
            // Arrange
            TestableDefaultModelBinder<OnPropertyValidatingModel> modelBinder = new TestableDefaultModelBinder<OnPropertyValidatingModel>();
            modelBinder.Context.ModelMetadata.Model = new OnPropertyValidatingModel();

            // Act
            modelBinder.SetProperty("RequiredInteger", null);

            // Assert
            ModelState modelState = modelBinder.ModelState[BASE_MODEL_NAME + ".RequiredInteger"];
            ModelError modelStateError = modelState.Errors.Single();
            Assert.Equal("Custom Required Message", modelStateError.ErrorMessage);
        }

        [Fact]
        public void SetPropertyAddsDefaultMessageForNonBindableNonNullableValueTypes()
        {
            DataAnnotationsModelValidatorProvider.AddImplicitRequiredAttributeForValueTypes = false;

            try
            {
                // Arrange
                TestableDefaultModelBinder<List<String>> modelBinder = new TestableDefaultModelBinder<List<String>>();
                modelBinder.Context.ModelMetadata.Model = null;

                // Act
                modelBinder.SetProperty("Count", null);

                // Assert
                ModelState modelState = modelBinder.ModelState[BASE_MODEL_NAME + ".Count"];
                ModelError modelStateError = modelState.Errors.Single();
                Assert.Equal("A value is required.", modelStateError.ErrorMessage);
            }
            finally
            {
                DataAnnotationsModelValidatorProvider.AddImplicitRequiredAttributeForValueTypes = true;
            }
        }

        [Fact]
        [ReplaceCulture]
        public void SetPropertyCreatesValueRequiredErrorIfNecessary()
        {
            // Arrange
            ModelBindingContext bindingContext = new ModelBindingContext()
            {
                ModelMetadata = ModelMetadataProviders.Current.GetMetadataForType(() => new MyModel(), typeof(MyModel)),
                ModelName = "theModel",
            };

            PropertyDescriptor property = TypeDescriptor.GetProperties(typeof(MyModel))["ReadWriteProperty"];
            DefaultModelBinderHelper helper = new DefaultModelBinderHelper();

            // Act
            helper.PublicSetProperty(new ControllerContext(), bindingContext, property, null);

            // Assert
            Assert.Equal("The ReadWriteProperty field is required.", bindingContext.ModelState["theModel.ReadWriteProperty"].Errors[0].ErrorMessage);
        }

        [Fact]
        public void SetPropertyWithThrowingSetter()
        {
            // Arrange
            TestableDefaultModelBinder<SetPropertyModel> binder = new TestableDefaultModelBinder<SetPropertyModel>();

            // Act
            binder.SetProperty("NonNullableString", null);

            // Assert
            ModelState modelState = binder.Context.ModelState[BASE_MODEL_NAME + ".NonNullableString"];
            ModelError error = Assert.Single(modelState.Errors);
            Assert.IsType<ArgumentNullException>(error.Exception);
        }

        [Fact]
        public void SetPropertyWithNullValueAndThrowingSetterWithRequiredAttribute()
        { // DDB #227809
            // Arrange
            TestableDefaultModelBinder<SetPropertyModel> binder = new TestableDefaultModelBinder<SetPropertyModel>();

            // Act
            binder.SetProperty("NonNullableStringWithAttribute", null);

            // Assert
            ModelState modelState = binder.Context.ModelState[BASE_MODEL_NAME + ".NonNullableStringWithAttribute"];
            var error = Assert.Single(modelState.Errors);
            Assert.Equal("My custom required message", error.ErrorMessage);
        }

        [Fact]
        public void SetPropertyDoesNothingIfPropertyIsReadOnly()
        {
            // Arrange
            MyModel model = new MyModel();
            ModelBindingContext bindingContext = new ModelBindingContext()
            {
                ModelMetadata = ModelMetadataProviders.Current.GetMetadataForType(() => model, model.GetType()),
                ModelName = "theModel"
            };

            PropertyDescriptor property = TypeDescriptor.GetProperties(model)["ReadOnlyProperty"];
            DefaultModelBinderHelper helper = new DefaultModelBinderHelper();

            // Act
            helper.PublicSetProperty(new ControllerContext(), bindingContext, property, 42);

            // Assert
            Assert.Empty(bindingContext.ModelState);
        }

        [Fact]
        public void SetPropertySuccess()
        {
            // Arrange
            TestableDefaultModelBinder<SetPropertyModel> binder = new TestableDefaultModelBinder<SetPropertyModel>();

            // Act
            binder.SetProperty("NullableString", "The new value");

            // Assert
            Assert.Equal("The new value", ((SetPropertyModel)binder.Context.Model).NullableString);
            Assert.Empty(binder.Context.ModelState);
        }

        class SetPropertyModel
        {
            public string NullableString { get; set; }

            public string NonNullableString
            {
                get { return null; }
                set
                {
                    if (value == null)
                    {
                        throw new ArgumentNullException("value");
                    }
                }
            }

            [Required(ErrorMessage = "My custom required message")]
            public string NonNullableStringWithAttribute
            {
                get { return null; }
                set
                {
                    if (value == null)
                    {
                        throw new ArgumentNullException("value");
                    }
                }
            }
        }

        // Helper methods

        static PropertyDescriptor GetProperty<T>(string propertyName)
        {
            return TypeDescriptor.GetProperties(typeof(T))
                .Cast<PropertyDescriptor>()
                .Where(p => p.Name == propertyName)
                .Single();
        }

        static ICustomTypeDescriptor GetType<T>()
        {
            return TypeDescriptor.GetProvider(typeof(T)).GetTypeDescriptor(typeof(T));
        }

        // Helper classes

        class TestableDefaultModelBinder<TModel> : DefaultModelBinder
            where TModel : new()
        {
            public TestableDefaultModelBinder()
                : this(new TModel())
            {
            }

            public TestableDefaultModelBinder(TModel model)
            {
                ModelState = new ModelStateDictionary();

                Context = new ModelBindingContext();
                Context.ModelMetadata = ModelMetadataProviders.Current.GetMetadataForType(() => model, typeof(TModel));
                Context.ModelName = BASE_MODEL_NAME;
                Context.ModelState = ModelState;
            }

            public ModelBindingContext Context { get; private set; }

            public ModelStateDictionary ModelState { get; set; }

            public void BindProperty(string propertyName)
            {
                base.BindProperty(new ControllerContext(), Context, GetProperty<TModel>(propertyName));
            }

            public PropertyDescriptorCollection GetModelProperties()
            {
                return base.GetModelProperties(new ControllerContext(), Context);
            }

            public object GetPropertyValue(string propertyName, object existingValue)
            {
                Mock<IModelBinder> mockModelBinder = new Mock<IModelBinder>();
                mockModelBinder.Setup(b => b.BindModel(It.IsAny<ControllerContext>(), Context)).Returns(existingValue);
                return base.GetPropertyValue(new ControllerContext(), Context, GetProperty<TModel>(propertyName), mockModelBinder.Object);
            }

            public bool OnPropertyValidating(string propertyName, object value)
            {
                return base.OnPropertyValidating(new ControllerContext(), Context, GetProperty<TModel>(propertyName), value);
            }

            public void OnPropertyValidated(string propertyName, object value)
            {
                base.OnPropertyValidated(new ControllerContext(), Context, GetProperty<TModel>(propertyName), value);
            }

            public void OnModelUpdated()
            {
                base.OnModelUpdated(new ControllerContext(), Context);
            }

            public void SetProperty(string propertyName, object value)
            {
                base.SetProperty(new ControllerContext(), Context, GetProperty<TModel>(propertyName), value);
            }
        }

        private class CountryState
        {
            public string Name { get; set; }

            public IEnumerable<string> States { get; set; }
        }

        private class NoParameterlessCtor
        {
            private NoParameterlessCtor()
            {
            }
        }
    }
}
