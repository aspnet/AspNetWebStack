// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.ObjectModel;
using System.Web.Http.Controllers;
using System.Web.Http.Metadata;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.ModelBinding
{
    public class ParameterBindingProvidersTest
    {
        [Fact]
        public void AsCollection()
        {
            ParameterBindingRulesCollection pb = new ParameterBindingRulesCollection();

            // Knowing that it's a collection means we have known behavior around 
            // the collection methods (like insert, add, clear, etc).
            Assert.True(pb is Collection<Func<HttpParameterDescriptor, HttpParameterBinding>>);
        }

        [Fact]
        public void Lookup_empty_collection_returns_null()
        {
            // The collection is empty,  so lookup will fail.
            ParameterBindingRulesCollection pb = new ParameterBindingRulesCollection();
            HttpParameterDescriptor parameter = CreateParameterDescriptor(typeof(object), "none");

            // Act
            HttpParameterBinding binding = pb.LookupBinding(parameter);

            // Assert. 
            Assert.Null(binding);
        }

        [Fact]
        public void Lookup_is_ordered()
        {
            ParameterBindingRulesCollection pb = new ParameterBindingRulesCollection();
            HttpParameterBinding mockBinding1 = new EmptyParameterBinding();
            HttpParameterBinding mockBinding2 = new EmptyParameterBinding();
            HttpParameterBinding mockBinding3 = new EmptyParameterBinding();

            pb.Add(param => param.ParameterName == "first" ? mockBinding1 : null);
            pb.Add(param => param.ParameterName == "first" ? mockBinding2 : null);
            pb.Add(param => param.ParameterType == typeof(int) ? mockBinding3 : null);

            // Act
            HttpParameterBinding b1 = pb.LookupBinding(CreateParameterDescriptor(null, "first"));
            HttpParameterBinding b2 = pb.LookupBinding(CreateParameterDescriptor(typeof(string), "none"));
            HttpParameterBinding b3 = pb.LookupBinding(CreateParameterDescriptor(typeof(int), "first"));
            HttpParameterBinding b4 = pb.LookupBinding(CreateParameterDescriptor(typeof(int), "last"));


            // Assert
            Assert.Equal(mockBinding1, b1);
            Assert.Null(b2);
            Assert.Equal(mockBinding1, b3);
            Assert.Equal(mockBinding3, b4);
        }

        [Fact]
        public void Add_with_type_match()
        {
            ParameterBindingRulesCollection pb = new ParameterBindingRulesCollection();
            HttpParameterBinding mockBinding = new EmptyParameterBinding();

            pb.Add(typeof(string), param => mockBinding);

            // Act
            HttpParameterBinding b1 = pb.LookupBinding(CreateParameterDescriptor(typeof(string), "first"));
            HttpParameterBinding b2 = pb.LookupBinding(CreateParameterDescriptor(typeof(int), "first"));

            // Assert
            Assert.Equal(mockBinding, b1);
            Assert.Null(b2); // doesn't match type, misses.
        }

        [Fact]
        public void type_match_user_function_not_invoked_if_type_doesnt_match()
        {
            ParameterBindingRulesCollection pb = new ParameterBindingRulesCollection();
            HttpParameterBinding mockBinding = new EmptyParameterBinding();

            pb.Add(typeof(string), param => { throw new InvalidOperationException("shouldn't be called"); });
            pb.Insert(0, typeof(string), param => { throw new InvalidOperationException("shouldn't be called"); });

            // Act
            HttpParameterBinding b2 = pb.LookupBinding(CreateParameterDescriptor(typeof(int), "first"));

            // Assert - made it through the action without throwing.
        }

        [Fact]
        public void Insert_with_type_match()
        {
            ParameterBindingRulesCollection pb = new ParameterBindingRulesCollection();
            HttpParameterBinding mockBinding1 = new EmptyParameterBinding();
            HttpParameterBinding mockBinding2 = new EmptyParameterBinding();
            HttpParameterBinding mockBinding3 = new EmptyParameterBinding();

            // Act, test insertion            
            pb.Add(typeof(string), param => mockBinding2);
            pb.Add(typeof(int), param => mockBinding2);
            pb.Insert(0, typeof(string), param => mockBinding1);
            pb.Insert(2, typeof(int), param => mockBinding3); // goes in middle

            // Assert via lookups
            Assert.Equal(mockBinding1, pb.LookupBinding(CreateParameterDescriptor(typeof(string), "first")));
            Assert.Equal(mockBinding3, pb.LookupBinding(CreateParameterDescriptor(typeof(int), "first")));            
        }

        // For unit testing purposes, just create a unique binding objet. 
        // We'll just compare object reference identity to determine if binds give the expected binding back.
        // ParameterDescriptor is ignored.
        private class EmptyParameterBinding : HttpParameterBinding
        {
            public EmptyParameterBinding()
                : base(CreateParameterDescriptor(typeof(object), "dummy"))
            { 
            }

            public override Threading.Tasks.Task ExecuteBindingAsync(ModelMetadataProvider metadataProvider, HttpActionContext actionContext, Threading.CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }
        }
        
        private static HttpParameterDescriptor CreateParameterDescriptor(Type type, string name)
        {
            Mock<HttpParameterDescriptor> mock = new Mock<HttpParameterDescriptor>();
            mock.Setup(p => p.ParameterType).Returns(type);
            mock.Setup(p => p.ParameterName).Returns(name);
            return mock.Object;
        }
    }
}