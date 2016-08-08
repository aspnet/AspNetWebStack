// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections;
using System.Web;
using System.Web.Helpers.Test;
using System.Web.TestUtil;
using Microsoft.TestCommon;
using Moq;

namespace Microsoft.Web.Helpers.Test
{
    public class FileUploadTest
    {
        private const string _fileUploadScript = "<script type=\"text/javascript\"> if (!window[\"FileUploadHelper\"]) window[\"FileUploadHelper\"] = {};  FileUploadHelper.addInputElement = function(index, name) {  var inputElem = document.createElement(\"input\");  inputElem.type = \"file\";  inputElem.name = name;  var divElem = document.createElement(\"div\");  divElem.appendChild(inputElem.cloneNode(false));   var inputs = document.getElementById(\"file-upload-\" + index);  inputs.appendChild(divElem);  } </script>";

        [Fact]
        public void RenderThrowsWhenNumberOfFilesIsLessThanZero()
        {
            // Act and Assert
            Assert.ThrowsArgumentGreaterThanOrEqualTo(
                () => FileUpload._GetHtml(GetContext(), name: null, initialNumberOfFiles: -2, allowMoreFilesToBeAdded: false, includeFormTag: false, addText: "", uploadText: "").ToString(),
                "initialNumberOfFiles", "0");
        }

        [Fact]
        public void ResultIncludesFormTagAndSubmitButtonWhenRequested()
        {
            // Arrange
            string expectedResult = @"<form action="""" enctype=""multipart/form-data"" method=""post""><div class=""file-upload"" id=""file-upload-0"">"
                                    + @"<div><input name=""fileUpload"" type=""file"" /></div></div>"
                                    + @"<div class=""file-upload-buttons""><input type=""submit"" value=""Upload"" /></div></form>";

            // Act
            var actualResult = FileUpload._GetHtml(GetContext(), name: null, initialNumberOfFiles: 1, allowMoreFilesToBeAdded: false, includeFormTag: true, addText: null, uploadText: null);

            // Assert
            UnitTestHelper.AssertEqualsIgnoreWhitespace(expectedResult, actualResult.ToString());
        }

        [Fact]
        public void ResultDoesNotIncludeFormTagAndSubmitButtonWhenNotRequested()
        {
            // Arrange
            string expectedResult = @"<div class=""file-upload"" id=""file-upload-0""><div><input name=""fileUpload"" type=""file"" /></div></div>";

            // Act 
            var actualResult = FileUpload._GetHtml(GetContext(), name: null, initialNumberOfFiles: 1, allowMoreFilesToBeAdded: false, includeFormTag: false, addText: null, uploadText: null);

            // Assert
            UnitTestHelper.AssertEqualsIgnoreWhitespace(expectedResult, actualResult.ToString());
        }

        [Fact]
        public void ResultIncludesCorrectNumberOfInputFields()
        {
            // Arrange
            string expectedResult = @"<div class=""file-upload"" id=""file-upload-0""><div><input name=""fileUpload"" type=""file"" /></div><div><input name=""fileUpload"" type=""file"" /></div>"
                                    + @"<div><input name=""fileUpload"" type=""file"" /></div></div>";

            // Act
            var actualResult = FileUpload._GetHtml(GetContext(), name: null, initialNumberOfFiles: 3, allowMoreFilesToBeAdded: false, includeFormTag: false, addText: null, uploadText: null);

            // Assert
            UnitTestHelper.AssertEqualsIgnoreWhitespace(expectedResult, actualResult.ToString());
        }

        [Fact]
        public void ResultIncludesAnchorTagWithCorrectAddText()
        {
            // Arrange
            string customAddText = "Add More";
            string expectedResult = _fileUploadScript
                                    + @"<div class=""file-upload"" id=""file-upload-0""><div><input name=""fileUpload"" type=""file"" /></div></div>"
                                    + @"<div class=""file-upload-buttons""><a href=""#"" onclick=""FileUploadHelper.addInputElement(0, &quot;fileUpload&quot;); return false;"">" + customAddText + "</a></div>";

            // Act
            var result = FileUpload._GetHtml(GetContext(), name: null, allowMoreFilesToBeAdded: true, includeFormTag: false, addText: customAddText, initialNumberOfFiles: 1, uploadText: null);

            // Assert
            UnitTestHelper.AssertEqualsIgnoreWhitespace(expectedResult, result.ToString());
        }

        [Fact]
        public void ResultDoesNotIncludeAnchorTagNorAddTextWhenNotRequested()
        {
            // Arrange
            string customAddText = "Add More";
            string expectedResult = @"<div class=""file-upload"" id=""file-upload-0""><div><input name=""fileUpload"" type=""file"" /></div></div>";

            // Act
            var result = FileUpload._GetHtml(GetContext(), name: null, allowMoreFilesToBeAdded: false, includeFormTag: false, addText: customAddText, uploadText: null, initialNumberOfFiles: 1);

            // Assert
            UnitTestHelper.AssertEqualsIgnoreWhitespace(expectedResult, result.ToString());
        }

        [Fact]
        public void ResultIncludesSubmitInputTagWithCustomUploadText()
        {
            // Arrange
            string customUploadText = "Now!";
            string expectedResult = @"<form action="""" enctype=""multipart/form-data"" method=""post""><div class=""file-upload"" id=""file-upload-0"">"
                                    + @"<div><input name=""fileUpload"" type=""file"" /></div></div>"
                                    + @"<div class=""file-upload-buttons""><input type=""submit"" value=""" + customUploadText + @""" /></div></form>";

            // Act
            var result = FileUpload._GetHtml(GetContext(), name: null, includeFormTag: true, uploadText: customUploadText, allowMoreFilesToBeAdded: false, initialNumberOfFiles: 1, addText: null);

            // Assert
            UnitTestHelper.AssertEqualsIgnoreWhitespace(expectedResult, result.ToString());
        }

        [Fact]
        public void FileUploadGeneratesUniqueIdsForMultipleCallsForCommonRequest()
        {
            // Arrange
            var context = GetContext();
            string expectedResult1 = @"<div class=""file-upload"" id=""file-upload-0""><div><input name=""fileUpload"" type=""file"" /></div><div><input name=""fileUpload"" type=""file"" /></div>"
                                     + @"<div><input name=""fileUpload"" type=""file"" /></div></div>";
            string expectedResult2 = @"<form action="""" enctype=""multipart/form-data"" method=""post""><div class=""file-upload"" id=""file-upload-1""><div><input name=""fileUpload"" type=""file"" /></div>"
                                     + @"<div><input name=""fileUpload"" type=""file"" /></div></div><div class=""file-upload-buttons""><input type=""submit"" value=""Upload"" /></div></form>";

            // Act
            var result1 = FileUpload._GetHtml(context, name: null, initialNumberOfFiles: 3, allowMoreFilesToBeAdded: false, includeFormTag: false, addText: null, uploadText: null);
            var result2 = FileUpload._GetHtml(context, name: null, initialNumberOfFiles: 2, allowMoreFilesToBeAdded: false, includeFormTag: true, addText: null, uploadText: null);

            // Assert
            UnitTestHelper.AssertEqualsIgnoreWhitespace(expectedResult1, result1.ToString());
            UnitTestHelper.AssertEqualsIgnoreWhitespace(expectedResult2, result2.ToString());
        }

        [Fact]
        public void FileUploadGeneratesScriptOncePerRequest()
        {
            // Arrange
            var context = GetContext();
            string expectedResult1 = _fileUploadScript
                                     + @"<div class=""file-upload"" id=""file-upload-0""><div><input name=""fileUpload"" type=""file"" /></div></div>"
                                     + @"<div class=""file-upload-buttons""><a href=""#"" onclick=""FileUploadHelper.addInputElement(0, &quot;fileUpload&quot;); return false;"">Add more files</a></div>";
            string expectedResult2 = @"<form action="""" enctype=""multipart/form-data"" method=""post""><div class=""file-upload"" id=""file-upload-1""><div><input name=""fileUpload"" type=""file"" /></div></div>"
                                     + @"<div class=""file-upload-buttons""><a href=""#"" onclick=""FileUploadHelper.addInputElement(1, &quot;fileUpload&quot;); return false;"">Add more files</a><input type=""submit"" value=""Upload"" /></div></form>";
            string expectedResult3 = @"<form action="""" enctype=""multipart/form-data"" method=""post""><div class=""file-upload"" id=""file-upload-2"">"
                                     + @"<div><input name=""fileUpload"" type=""file"" /></div></div>"
                                     + @"<div class=""file-upload-buttons""><input type=""submit"" value=""Upload"" /></div></form>";

            // Act
            var result1 = FileUpload._GetHtml(context, name: null, initialNumberOfFiles: 1, allowMoreFilesToBeAdded: true, includeFormTag: false, addText: null, uploadText: null);
            var result2 = FileUpload._GetHtml(context, name: null, initialNumberOfFiles: 1, allowMoreFilesToBeAdded: true, includeFormTag: true, addText: null, uploadText: null);
            var result3 = FileUpload._GetHtml(context, name: null, initialNumberOfFiles: 1, allowMoreFilesToBeAdded: false, includeFormTag: true, addText: null, uploadText: null);

            // Assert
            UnitTestHelper.AssertEqualsIgnoreWhitespace(expectedResult1, result1.ToString());
            UnitTestHelper.AssertEqualsIgnoreWhitespace(expectedResult2, result2.ToString());
            UnitTestHelper.AssertEqualsIgnoreWhitespace(expectedResult3, result3.ToString());
        }

        [Fact]
        public void FileUploadUsesNamePropertyInJavascript()
        {
            // Arrange
            var context = GetContext();
            string name = "foobar";
            string expectedResult = _fileUploadScript
                                    + @"<form action="""" enctype=""multipart/form-data"" method=""post""><div class=""file-upload"" id=""file-upload-0""><div><input name=""foobar"" type=""file"" /></div></div>"
                                    + @"<div class=""file-upload-buttons""><a href=""#"" onclick=""FileUploadHelper.addInputElement(0, &quot;foobar&quot;); return false;"">Add more files</a><input type=""submit"" value=""Upload"" /></div></form>";

            // Act
            var result = FileUpload._GetHtml(context, name: name, initialNumberOfFiles: 1, allowMoreFilesToBeAdded: true, includeFormTag: true, addText: null, uploadText: null);

            // Assert
            UnitTestHelper.AssertEqualsIgnoreWhitespace(expectedResult, result.ToString());
        }

        [Fact]
        public void _GetHtmlWithDefaultArgumentsProducesValidXhtml()
        {
            // Act 
            var result = FileUpload._GetHtml(GetContext(), name: null, initialNumberOfFiles: 1, includeFormTag: false, allowMoreFilesToBeAdded: false, addText: null, uploadText: null);

            // Assert
            XhtmlAssert.Validate1_1(result, "div");
        }

        [Fact]
        public void _GetHtmlWithoutFormTagProducesValidXhtml()
        {
            // Act
            var result = FileUpload._GetHtml(GetContext(), name: null, includeFormTag: false, initialNumberOfFiles: 1, allowMoreFilesToBeAdded: false, addText: null, uploadText: null);

            XhtmlAssert.Validate1_1(result, "div");
        }

        private HttpContextBase GetContext()
        {
            var context = new Mock<HttpContextBase>();
            context.Setup(c => c.Items).Returns(new Hashtable());
            return context.Object;
        }
    }
}
