// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Formatting;
using System.Web.Http.ModelBinding;
using System.Web.Http.Validation;
using System.Web.Http.ValueProviders;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.Controllers
{
    public class ParameterBindingExtensionsTest
    {
        [Fact]
        public void BindAsError()
        {
            HttpParameterDescriptor param = CreateParameterDescriptor();

            string message = "error message";
            HttpParameterBinding binding = param.BindAsError(message);

            Assert.NotNull(binding);
            Assert.False(binding.IsValid);
            Assert.Equal(message, binding.ErrorMessage);
        }

        [Fact]
        public void BindWithModelBinding_Default()
        {
            HttpParameterDescriptor param = CreateParameterDescriptor();

            ModelBinderParameterBinding binding = (ModelBinderParameterBinding) param.BindWithModelBinding();

            Assert.NotNull(binding);
        }

        [Fact]
        public void BindWithModelBinding_Attribute()
        {
            HttpParameterDescriptor param = CreateParameterDescriptor();

            ModelBinderAttribute attribute = new ModelBinderAttribute(typeof(CustomModelBinder));
            ModelBinderParameterBinding binding = (ModelBinderParameterBinding) param.BindWithAttribute(attribute);

            Assert.NotNull(binding);
            Assert.IsType<CustomModelBinder>(binding.Binder);
        }

        [Fact]
        public void BindWithModelBinding_IModelBinder()
        {
            HttpParameterDescriptor param = CreateParameterDescriptor();

            var binder = new CustomModelBinder();
            ModelBinderParameterBinding binding = (ModelBinderParameterBinding) param.BindWithModelBinding(binder);

            Assert.NotNull(binding);
            Assert.Equal(binder, binding.Binder);
        }

        [Fact]
        public void BindWithModelBinding_ValueProviderFactory_Array()
        {
            HttpParameterDescriptor param = CreateParameterDescriptor();

            ModelBinderParameterBinding binding = (ModelBinderParameterBinding) param.BindWithModelBinding(new CustomValueProviderFactory1(), new CustomValueProviderFactory2());

            Assert.NotNull(binding);

            ValueProviderFactory[] vpfs = binding.ValueProviderFactories.ToArray();
            Assert.Equal(2, vpfs.Length);            
            Assert.IsType<CustomValueProviderFactory1>(vpfs[0]);
            Assert.IsType<CustomValueProviderFactory2>(vpfs[1]);
        }

        [Fact]
        public void BindWithModelBinding_ValueProviderFactory_IEnumerable()
        {
            HttpParameterDescriptor param = CreateParameterDescriptor();

            var binder = new CustomModelBinder();
            ModelBinderParameterBinding binding = (ModelBinderParameterBinding) param.BindWithModelBinding(binder, new List<ValueProviderFactory>() { new CustomValueProviderFactory1(), new CustomValueProviderFactory2() });

            Assert.NotNull(binding);
            Assert.Equal(binder, binding.Binder);

            ValueProviderFactory[] vpfs = binding.ValueProviderFactories.ToArray();
            Assert.Equal(2, vpfs.Length);
            Assert.IsType<CustomValueProviderFactory1>(vpfs[0]);
            Assert.IsType<CustomValueProviderFactory2>(vpfs[1]);
        }

        [Fact]
        public void BindWithModelBinding_ValueProviderFactory_Binder_and_IEnumerable()
        {
            HttpParameterDescriptor param = CreateParameterDescriptor();

            ModelBinderParameterBinding binding = (ModelBinderParameterBinding) param.BindWithModelBinding(new List<ValueProviderFactory>() { new CustomValueProviderFactory1(), new CustomValueProviderFactory2() });

            Assert.NotNull(binding);

            ValueProviderFactory[] vpfs = binding.ValueProviderFactories.ToArray();
            Assert.Equal(2, vpfs.Length);
            Assert.IsType<CustomValueProviderFactory1>(vpfs[0]);
            Assert.IsType<CustomValueProviderFactory2>(vpfs[1]);
        }

        [Fact]
        public void BindWithFormatter()
        {
            HttpParameterDescriptor param = CreateParameterDescriptor();

            FormatterParameterBinding binding = (FormatterParameterBinding) param.BindWithFormatter();

            Assert.NotNull(binding);
        }

        [Fact]
        public void BindWithFormatter_Formatter_Array()
        {
            HttpParameterDescriptor param = CreateParameterDescriptor();

            MediaTypeFormatter formatter1 = new XmlMediaTypeFormatter();
            MediaTypeFormatter formatter2 = new JsonMediaTypeFormatter();

            FormatterParameterBinding binding = (FormatterParameterBinding) param.BindWithFormatter(formatter1, formatter2);
            
            Assert.NotNull(binding);
            MediaTypeFormatter[] formatters = binding.Formatters.ToArray();
            Assert.Equal(2, formatters.Length);
            Assert.Equal(formatter1, formatters[0]);
            Assert.Equal(formatter2, formatters[1]);
        }

        [Fact]
        public void BindWithFormatter_Formatter_IEnumerable()
        {
            HttpParameterDescriptor param = CreateParameterDescriptor();

            MediaTypeFormatter formatter1 = new XmlMediaTypeFormatter();
            MediaTypeFormatter formatter2 = new JsonMediaTypeFormatter();

            FormatterParameterBinding binding = (FormatterParameterBinding) param.BindWithFormatter(new List<MediaTypeFormatter> { formatter1, formatter2 });

            Assert.NotNull(binding);
            MediaTypeFormatter[] formatters = binding.Formatters.ToArray();
            Assert.Equal(2, formatters.Length);
            Assert.Equal(formatter1, formatters[0]);
            Assert.Equal(formatter2, formatters[1]);
        }

        [Fact]
        public void BindWithFormatter_Formatters_and_Validator()
        {
            HttpParameterDescriptor param = CreateParameterDescriptor();

            IBodyModelValidator bodyModelValidator = new Mock<IBodyModelValidator>().Object;
            MediaTypeFormatter formatter1 = new XmlMediaTypeFormatter();
            MediaTypeFormatter formatter2 = new JsonMediaTypeFormatter();

            FormatterParameterBinding binding = (FormatterParameterBinding) param.BindWithFormatter(new List<MediaTypeFormatter> { formatter1, formatter2 }, bodyModelValidator);

            Assert.NotNull(binding);
            Assert.Equal(bodyModelValidator, binding.BodyModelValidator);
            MediaTypeFormatter[] formatters = binding.Formatters.ToArray();
            Assert.Equal(2, formatters.Length);
            Assert.Equal(formatter1, formatters[0]);
            Assert.Equal(formatter2, formatters[1]);
        }

        // Create a parameter that's sufficiently complete that we can run a basic Bind() operation on it. 
        private static HttpParameterDescriptor CreateParameterDescriptor()
        {
            // Need config because bind looks up in config.
            HttpConfiguration config = new HttpConfiguration();
            HttpParameterDescriptor param = CreateParameterDescriptor(typeof(object), "thing");
            param.Configuration = config;
            param.ActionDescriptor = new Mock<HttpActionDescriptor>().Object;
            param.ActionDescriptor.ControllerDescriptor = new HttpControllerDescriptor(config);

            return param;
        }

        private static HttpParameterDescriptor CreateParameterDescriptor(Type type, string name)
        {
            Mock<HttpParameterDescriptor> mock = new Mock<HttpParameterDescriptor>();
            mock.Setup(p => p.ParameterType).Returns(type);
            mock.Setup(p => p.ParameterName).Returns(name);
            return mock.Object;
        }

        public class CustomModelBinder : IModelBinder
        {
            public bool BindModel(HttpActionContext actionContext, ModelBindingContext bindingContext)
            {
                throw new NotImplementedException();
            }
        }

        public class CustomValueProviderFactory1 : ValueProviderFactory
        {
            public override IValueProvider GetValueProvider(HttpActionContext actionContext)
            {
                throw new NotImplementedException();
            }
        }
        public class CustomValueProviderFactory2 : ValueProviderFactory
        {
            public override IValueProvider GetValueProvider(HttpActionContext actionContext)
            {
                throw new NotImplementedException();
            }
        }
    }
}
