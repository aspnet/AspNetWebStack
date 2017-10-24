// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net.Http.Formatting;
using System.Web.Http.ModelBinding;
using Microsoft.TestCommon;

namespace System.Web.Http.Validation
{
    public class ModelStateFormatterLoggerTest
    {
        [Fact]
        public void LogErrorAddsErrorToModelState()
        {
            ModelStateDictionary modelState = new ModelStateDictionary();
            string prefix = "prefix";
            IFormatterLogger formatterLogger = new ModelStateFormatterLogger(modelState, prefix);

            formatterLogger.LogError("property", "error");

            Assert.True(modelState.ContainsKey("prefix.property"));
            ModelError error = Assert.Single(modelState["prefix.property"].Errors);
            Assert.Equal("error", error.ErrorMessage);
        }

        [Fact]
        public void LogErrorAddsExceptionToModelState()
        {
            ModelStateDictionary modelState = new ModelStateDictionary();
            string prefix = "prefix";
            IFormatterLogger formatterLogger = new ModelStateFormatterLogger(modelState, prefix);

            Exception e = new Exception("error");

            formatterLogger.LogError("property", e);

            Assert.True(modelState.ContainsKey("prefix.property"));
            ModelError error = Assert.Single(modelState["prefix.property"].Errors);
            Assert.Equal(e, error.Exception);
        }
    }
}
