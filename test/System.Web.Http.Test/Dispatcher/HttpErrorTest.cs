// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.IO;
using System.Net.Http.Formatting;
using System.Threading.Tasks;
using System.Web.Http.ModelBinding;
using Microsoft.TestCommon;
using Newtonsoft.Json.Linq;

namespace System.Web.Http.Dispatcher
{
    public class HttpErrorTest
    {
        public static TheoryDataSet<HttpError, Func<string>, string, string> ErrorKeyValue
        {
            get
            {
                HttpError httpError = new HttpError();
                return new TheoryDataSet<HttpError, Func<string>, string, string>
                {
                    { httpError, () => httpError.Message, "Message", "Message_Value" },
                    { httpError, () => httpError.MessageDetail, "MessageDetail", "MessageDetail_Value" },
                    { httpError, () => httpError.ExceptionMessage, "ExceptionMessage", "ExceptionMessage_Value" },
                    { httpError, () => httpError.ExceptionType, "ExceptionType", "ExceptionType_Value" },
                    { httpError, () => httpError.StackTrace, "StackTrace", "StackTrace_Value" },
                };
            }
        }

        public static TheoryDataSet<HttpError> HttpErrors
        {
            get
            {
                return new TheoryDataSet<HttpError>()
                {
                    new HttpError(),
                    new HttpError("error"),
                    new HttpError(new NotImplementedException(), true),
                    new HttpError(new ModelStateDictionary() { { "key", new ModelState() { Errors = { new ModelError("error") } } } }, true),
                    new HttpError("error", "errordetail"),
                };
            }
        }

        [Fact]
        public void Constructor_GuardClauses()
        {
            Assert.ThrowsArgumentNull(
                () => new HttpError(message: null),
                "message");
            Assert.ThrowsArgumentNull(
                () => new HttpError(exception: null, includeErrorDetail: false),
                "exception");
            Assert.ThrowsArgumentNull(
                () => new HttpError(modelState: null, includeErrorDetail: false),
                "modelState");
        }

        [Fact]
        public void StringConstructor_AddsCorrectDictionaryItems()
        {
            HttpError error = new HttpError("something bad happened");

            Assert.Contains(new KeyValuePair<string, object>("Message", "something bad happened"), error);
        }

        [Fact]
        public void ExceptionConstructorWithDetail_AddsCorrectDictionaryItems()
        {
            HttpError error = new HttpError(new ArgumentException("error", new Exception()), true);

            Assert.Contains(new KeyValuePair<string, object>("Message", "An error has occurred."), error);
            Assert.Contains(new KeyValuePair<string, object>("ExceptionMessage", "error"), error);
            Assert.Contains(new KeyValuePair<string, object>("ExceptionType", "System.ArgumentException"), error);
            Assert.True(error.ContainsKey("StackTrace"));
            Assert.True(error.ContainsKey("InnerException"));
            Assert.IsType<HttpError>(error["InnerException"]);
        }

        [Fact]
        public void ModelStateConstructorWithDetail_AddsCorrectDictionaryItems()
        {
            ModelStateDictionary modelState = new ModelStateDictionary();
            modelState.AddModelError("[0].Name", "error1");
            modelState.AddModelError("[0].Name", "error2");
            modelState.AddModelError("[0].Address", "error");
            modelState.AddModelError("[2].Name", new Exception("OH NO"));

            HttpError error = new HttpError(modelState, true);
            HttpError modelStateError = error["ModelState"] as HttpError;

            Assert.Contains(new KeyValuePair<string, object>("Message", "The request is invalid."), error);
            Assert.Contains("error1", modelStateError["[0].Name"] as IEnumerable<string>);
            Assert.Contains("error2", modelStateError["[0].Name"] as IEnumerable<string>);
            Assert.Contains("error", modelStateError["[0].Address"] as IEnumerable<string>);
            Assert.True(modelStateError.ContainsKey("[2].Name"));
            Assert.Contains("OH NO", modelStateError["[2].Name"] as IEnumerable<string>);
        }

        [Fact]
        public void ExceptionConstructorWithoutDetail_AddsCorrectDictionaryItems()
        {
            HttpError error = new HttpError(new ArgumentException("error", new Exception()), false);

            Assert.Contains(new KeyValuePair<string, object>("Message", "An error has occurred."), error);
            Assert.False(error.ContainsKey("ExceptionMessage"));
            Assert.False(error.ContainsKey("ExceptionType"));
            Assert.False(error.ContainsKey("StackTrace"));
            Assert.False(error.ContainsKey("InnerException"));
        }

        [Fact]
        public void ModelStateConstructorWithoutDetail_AddsCorrectDictionaryItems()
        {
            ModelStateDictionary modelState = new ModelStateDictionary();
            modelState.AddModelError("[0].Name", "error1");
            modelState.AddModelError("[0].Name", "error2");
            modelState.AddModelError("[0].Address", "error");
            modelState.AddModelError("[2].Name", new Exception("OH NO"));

            HttpError error = new HttpError(modelState, false);
            HttpError modelStateError = error["ModelState"] as HttpError;

            Assert.Contains(new KeyValuePair<string, object>("Message", "The request is invalid."), error);
            Assert.Contains("error1", modelStateError["[0].Name"] as IEnumerable<string>);
            Assert.Contains("error2", modelStateError["[0].Name"] as IEnumerable<string>);
            Assert.Contains("error", modelStateError["[0].Address"] as IEnumerable<string>);
            Assert.True(modelStateError.ContainsKey("[2].Name"));
            Assert.DoesNotContain("OH NO", modelStateError["[2].Name"] as IEnumerable<string>);
        }

        [Fact]
        public async Task HttpError_Roundtrips_WithJsonFormatter()
        {
            HttpError error = new HttpError("error") { { "ErrorCode", 42 }, { "Data", new[] { "a", "b", "c" } } };
            MediaTypeFormatter formatter = new JsonMediaTypeFormatter();
            MemoryStream stream = new MemoryStream();

            await formatter.WriteToStreamAsync(typeof(HttpError), error, stream, content: null, transportContext: null);
            stream.Position = 0;
            HttpError roundtrippedError = (await formatter.ReadFromStreamAsync(typeof(HttpError), stream, content: null, formatterLogger: null)) as HttpError;

            Assert.NotNull(roundtrippedError);
            Assert.Equal("error", roundtrippedError.Message);
            Assert.Equal(42L, roundtrippedError["ErrorCode"]);
            JArray data = roundtrippedError["Data"] as JArray;
            Assert.Equal(3, data.Count);
            Assert.Contains("a", data);
            Assert.Contains("b", data);
            Assert.Contains("c", data);
        }

        [Fact]
        public async Task HttpError_Roundtrips_WithXmlFormatter()
        {
            HttpError error = new HttpError("error") { { "ErrorCode", 42 }, { "Data", new[] { "a", "b", "c" } } };
            MediaTypeFormatter formatter = new XmlMediaTypeFormatter();
            MemoryStream stream = new MemoryStream();

            await formatter.WriteToStreamAsync(typeof(HttpError), error, stream, content: null, transportContext: null);
            stream.Position = 0;
            HttpError roundtrippedError = (await formatter.ReadFromStreamAsync(typeof(HttpError), stream, content: null, formatterLogger: null)) as HttpError;

            Assert.NotNull(roundtrippedError);
            Assert.Equal("error", roundtrippedError.Message);
            Assert.Equal("42", roundtrippedError["ErrorCode"]);
            Assert.Equal("a b c", roundtrippedError["Data"]);
        }

        [Fact]
        public async Task HttpErrorWithWhitespace_Roundtrips_WithXmlFormatter()
        {
            string message = "  foo\n bar  \n ";
            HttpError error = new HttpError(message);
            MediaTypeFormatter formatter = new XmlMediaTypeFormatter();
            MemoryStream stream = new MemoryStream();

            await formatter.WriteToStreamAsync(typeof(HttpError), error, stream, content: null, transportContext: null);
            stream.Position = 0;
            HttpError roundtrippedError = (await formatter.ReadFromStreamAsync(typeof(HttpError), stream, content: null, formatterLogger: null)) as HttpError;

            Assert.NotNull(roundtrippedError);
            Assert.Equal(message, roundtrippedError.Message);
        }

        [Fact]
        public async Task HttpError_Roundtrips_WithXmlSerializer()
        {
            HttpError error = new HttpError("error") { { "ErrorCode", 42 }, { "Data", new[] { "a", "b", "c" } } };
            MediaTypeFormatter formatter = new XmlMediaTypeFormatter() { UseXmlSerializer = true };
            MemoryStream stream = new MemoryStream();

            await formatter.WriteToStreamAsync(typeof(HttpError), error, stream, content: null, transportContext: null);
            stream.Position = 0;
            HttpError roundtrippedError = (await formatter.ReadFromStreamAsync(typeof(HttpError), stream, content: null, formatterLogger: null)) as HttpError;

            Assert.NotNull(roundtrippedError);
            Assert.Equal("error", roundtrippedError.Message);
            Assert.Equal("42", roundtrippedError["ErrorCode"]);
            Assert.Equal("a b c", roundtrippedError["Data"]);
        }

        [Fact]
        public async Task HttpErrorForInnerException_Serializes_WithXmlSerializer()
        {
            HttpError error = new HttpError(new ArgumentException("error", new Exception("innerError")), includeErrorDetail: true);
            MediaTypeFormatter formatter = new XmlMediaTypeFormatter() { UseXmlSerializer = true };
            MemoryStream stream = new MemoryStream();

            await formatter.WriteToStreamAsync(typeof(HttpError), error, stream, content: null, transportContext: null);
            stream.Position = 0;
            string serializedError = new StreamReader(stream).ReadToEnd();

            Assert.NotNull(serializedError);
            Assert.Equal(
                "<Error><Message>An error has occurred.</Message><ExceptionMessage>error</ExceptionMessage><ExceptionType>System.ArgumentException</ExceptionType><StackTrace /><InnerException><Message>An error has occurred.</Message><ExceptionMessage>innerError</ExceptionMessage><ExceptionType>System.Exception</ExceptionType><StackTrace /></InnerException></Error>",
                serializedError);
        }

        [Fact]
        public void HttpError_Message_RoundTrips()
        {
            string message = "HelloWorld";
            Assert.Reflection.Property(
                new HttpError(message),
                e => e.Message,
                expectedDefaultValue: message,
                allowNull: true,
                roundTripTestValue: "HelloAgain");
        }

        [Fact]
        public void HttpError_MessageDetail_RoundTrips()
        {
            string messageDetail = "HelloWorld";
            Assert.Reflection.Property(
                new HttpError("message", messageDetail),
                e => e.MessageDetail,
                expectedDefaultValue: messageDetail,
                allowNull: true,
                roundTripTestValue: "HelloAgain");
        }

        [Fact]
        public void HttpError_ExceptionMessage_RoundTrips()
        {
            string exceptionMessage = "ExceptionMessage";
            Exception exception = new Exception(exceptionMessage);
            Assert.Reflection.Property(
                new HttpError(exception, includeErrorDetail: true),
                e => e.ExceptionMessage,
                expectedDefaultValue: exceptionMessage,
                allowNull: true,
                roundTripTestValue: "HelloAgain");
        }

        [Fact]
        public void HttpError_ExceptionType_RoundTrips()
        {
            ApplicationException exception = new ApplicationException("HelloWorld");
            Assert.Reflection.Property(
                new HttpError(exception, includeErrorDetail: true),
                e => e.ExceptionType,
                expectedDefaultValue: exception.GetType().FullName,
                allowNull: true,
                roundTripTestValue: "HelloAgain");
        }

        [Fact]
        public void HttpError_StackTrace_RoundTrips()
        {
            Exception exception;
            try
            {
                throw new Exception("HelloWorld");
            }
            catch (Exception e)
            {
                exception = e;
            }

            Assert.Reflection.Property(
                new HttpError(exception, includeErrorDetail: true),
                e => e.StackTrace,
                expectedDefaultValue: exception.StackTrace,
                allowNull: true,
                roundTripTestValue: "HelloAgain");
        }

        [Fact]
        public void GetPropertyValue_GetsValue_IfTypeMatches()
        {
            HttpError error = new HttpError();
            error["key"] = "x";

            Assert.Equal("x", error.GetPropertyValue<string>("key"));
            Assert.Equal("x", error.GetPropertyValue<object>("key"));
        }

        [Fact]
        public void GetPropertyValue_GetsDefault_IfTypeDoesNotMatch()
        {
            HttpError error = new HttpError();
            error["key"] = "x";

            Assert.Null(error.GetPropertyValue<Uri>("key"));
            Assert.Equal(0, error.GetPropertyValue<int>("key"));
        }

        [Fact]
        public void GetPropertyValue_GetsDefault_IfPropertyMissing()
        {
            HttpError error = new HttpError();

            Assert.Null(error.GetPropertyValue<string>("key"));
            Assert.Equal(0, error.GetPropertyValue<int>("key"));
        }

        [Theory]
        [PropertyData("ErrorKeyValue")]
        public void HttpErrorStringProperties_UseCorrectHttpErrorKey(HttpError httpError, Func<string> productUnderTest, string key, string actualValue)
        {
            // Arrange
            httpError[key] = actualValue;

            // Act
            string expectedValue = productUnderTest.Invoke();

            // Assert
            Assert.Equal(expectedValue, actualValue);
        }

        [Fact]
        public void HttpErrorProperty_InnerException_UsesCorrectHttpErrorKey()
        {
            // Arrange
            HttpError error = new HttpError(new ArgumentException("error", new Exception()), true);

            // Act
            HttpError innerException = error.InnerException;

            // Assert
            Assert.Same(error["InnerException"], innerException);
        }

        [Fact]
        public void HttpErrorProperty_ModelState_UsesCorrectHttpErrorKey()
        {
            // Arrange
            ModelStateDictionary modelState = new ModelStateDictionary();
            modelState.AddModelError("[0].Name", "error1");
            HttpError error = new HttpError(modelState, true);

            // Act
            HttpError actualModelStateError = error.ModelState;

            // Assert
            Assert.Same(error["ModelState"], actualModelStateError);
        }

        [Theory]
        [PropertyData("HttpErrors")]
        public void HttpErrors_UseCaseInsensitiveComparer(HttpError httpError)
        {
            var lowercaseKey = "abcd";
            var uppercaseKey = "ABCD";

            httpError[lowercaseKey] = "error";

            Assert.True(httpError.ContainsKey(lowercaseKey));
            Assert.True(httpError.ContainsKey(uppercaseKey));
        }
    }
}
