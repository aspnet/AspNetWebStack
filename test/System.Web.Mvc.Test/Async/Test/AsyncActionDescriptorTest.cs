// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Mvc.Async.Test
{
    public class AsyncActionDescriptorTest
    {
        [Fact]
        public void SynchronousExecuteThrows()
        {
            // Arrange
            AsyncActionDescriptor actionDescriptor = new TestableAsyncActionDescriptor();

            // Act & Assert
            Assert.Throws<InvalidOperationException>(
                () => actionDescriptor.Execute(new Mock<ControllerContext>().Object, parameters: null),
                "The asynchronous action method 'testAction' cannot be executed synchronously."
                );
        }

        private class TestableAsyncActionDescriptor : AsyncActionDescriptor
        {
            public override string ActionName
            {
                get { return "testAction"; }
            }

            public override ControllerDescriptor ControllerDescriptor
            {
                get { throw new NotImplementedException(); }
            }

            public override IAsyncResult BeginExecute(ControllerContext controllerContext, IDictionary<string, object> parameters, AsyncCallback callback, object state)
            {
                throw new NotImplementedException();
            }

            public override object EndExecute(IAsyncResult asyncResult)
            {
                throw new NotImplementedException();
            }

            public override ParameterDescriptor[] GetParameters()
            {
                throw new NotImplementedException();
            }
        }
    }
}
