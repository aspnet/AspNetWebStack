// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Web.Http.Controllers;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http
{
    public class HttpParameterBindingTest
    {
        [Fact]
        public void GetValueMissing()
        {
            CustomBinding binding = new CustomBinding("p1");

            HttpActionContext ctx = new HttpActionContext();

            var result = binding.GetValue(ctx);

            Assert.Null(result);
        }

        [Fact]
        public void GetValue_Returns_Set()
        {
            CustomBinding binding = new CustomBinding("p1");

            HttpActionContext ctx = new HttpActionContext();

            // Act
            object result = "abc";
            binding.SetValue(ctx, result);
            var result2 = binding.GetValue(ctx);

            // Assert
            Assert.Same(result, result2);
        }

        [Fact]
        public void Can_Set_Null()
        {
            // It's legal to set a parameter to null. Test against spurious null checks.
            CustomBinding binding = new CustomBinding("p1");

            HttpActionContext ctx = new HttpActionContext();

            // Act            
            binding.SetValue(ctx, null);

            var resultFinal = binding.GetValue(ctx);

            // Assert
            Assert.Null(resultFinal);
        }

        [Fact]
        public void Call_Set_Multiple_Times()
        {
            // Make sure a binding can set the argument multiple times and that we get the latest. 
            // This is interesting with composite bindings that chain to an inner binding. 
            CustomBinding binding = new CustomBinding("p1");

            HttpActionContext ctx = new HttpActionContext();

            // Act
            object result1 = "abc";
            binding.SetValue(ctx, result1);

            object result2 = 123;
            binding.SetValue(ctx, result2);

            var resultFinal = binding.GetValue(ctx);

            // Assert
            Assert.Same(result2, resultFinal);
        }

        [Fact]
        public void Set_Modifies_Dictionary()
        {
            string name = "p1";
            CustomBinding binding = new CustomBinding(name);

            HttpActionContext ctx = new HttpActionContext();

            // Act
            object result = "abc";
            binding.SetValue(ctx, result);
            var result2 = ctx.ActionArguments[name];

            // Assert
            Assert.Same(result, result2);
        }

        [Fact]
        public void Reuse_Binding_With_Different_Contexts()
        { 
            // The same binding can be used across multiple action contexts.

            string name = "p1";
            CustomBinding binding = new CustomBinding(name);

            HttpActionContext ctx1 = new HttpActionContext();
            HttpActionContext ctx2 = new HttpActionContext();

            // Act
            object result1 = "abc";
            binding.SetValue(ctx1, result1);

            object result2 = 123;            
            binding.SetValue(ctx2, result2);

            // Assert
            Assert.Same(result1, binding.GetValue(ctx1));
            Assert.Same(result2, binding.GetValue(ctx2));
        }

        // Helper for testing. 
        // Easily construct, mock out the the right things, and expose protected members to the tests. 
        public class CustomBinding : HttpParameterBinding
        {
            public CustomBinding(string paramName) : base(Build(paramName))
            {
            }

            static HttpParameterDescriptor Build(string paramName)
            {
                Mock<HttpParameterDescriptor> mock = new Mock<HttpParameterDescriptor>();
                mock.Setup(x => x.ParameterName).Returns(paramName);
                return mock.Object;
            }

            public override Threading.Tasks.Task ExecuteBindingAsync(Metadata.ModelMetadataProvider metadataProvider, HttpActionContext actionContext, Threading.CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }

            // Expose protected
            public new object GetValue(HttpActionContext actionContext)
            {
                return base.GetValue(actionContext);
            }

            public new void SetValue(HttpActionContext actionContext, object value)
            {
                base.SetValue(actionContext, value);
            }
        }
    }


}
