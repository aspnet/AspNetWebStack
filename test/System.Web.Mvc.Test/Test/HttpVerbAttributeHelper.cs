// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.TestCommon;

namespace System.Web.Mvc.Test
{
    internal static class HttpVerbAttributeHelper
    {
        internal static void TestHttpVerbAttributeNullControllerContext<THttpVerb>()
            where THttpVerb : ActionMethodSelectorAttribute, new()
        {
            // Arrange
            ActionMethodSelectorAttribute attribute = new THttpVerb();

            // Act & Assert
            Assert.ThrowsArgumentNull(
                delegate { attribute.IsValidForRequest(null, null); }, "controllerContext");
        }

        internal static void TestHttpVerbAttributeWithValidVerb<THttpVerb>(string validVerb)
            where THttpVerb : ActionMethodSelectorAttribute, new()
        {
            // Arrange
            ActionMethodSelectorAttribute attribute = new THttpVerb();
            ControllerContext context = AcceptVerbsAttributeTest.GetControllerContextWithHttpVerb(validVerb);

            // Act
            bool result = attribute.IsValidForRequest(context, null);

            // Assert
            Assert.True(result);
        }

        internal static void TestHttpVerbAttributeWithInvalidVerb<THttpVerb>(string invalidVerb)
            where THttpVerb : ActionMethodSelectorAttribute, new()
        {
            // Arrange
            ActionMethodSelectorAttribute attribute = new THttpVerb();
            ControllerContext context = AcceptVerbsAttributeTest.GetControllerContextWithHttpVerb(invalidVerb);

            // Act
            bool result = attribute.IsValidForRequest(context, null);

            // Assert
            Assert.False(result);
        }
    }
}
