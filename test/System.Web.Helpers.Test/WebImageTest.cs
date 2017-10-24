// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Web.WebPages.TestUtils;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Helpers.Test
{
    public class WebImageTest
    {
        private static readonly byte[] _JpgImageBytes = TestFile.Create("LambdaFinal.jpg").ReadAllBytes();
        private static readonly byte[] _BmpImageBytes = TestFile.Create("logo.bmp").ReadAllBytes();
        private static readonly byte[] _PngImageBytes = TestFile.Create("NETLogo.png").ReadAllBytes();
        private static readonly byte[] _IcoImageBytes = TestFile.Create("Test.ico").ReadAllBytes();

        [Fact]
        public void ConstructorThrowsWhenFilePathIsNull()
        {
            Assert.ThrowsArgument(() =>
                                                    new WebImage(GetContext(), s => new byte[] { }, filePath: null), "filePath", "Value cannot be null or an empty string.");
        }

        [Fact]
        public void ConstructorThrowsWhenFilePathIsEmpty()
        {
            Assert.ThrowsArgument(() =>
                                                    new WebImage(GetContext(), s => new byte[] { }, filePath: String.Empty), "filePath", "Value cannot be null or an empty string.");
        }

        [Fact]
        public void ConstructorThrowsWhenFilePathIsInvalid()
        {
            Assert.Throws<DirectoryNotFoundException>(() =>
                                                               new WebImage(GetContext(), s => { throw new DirectoryNotFoundException(); }, @"x:\this\does\not\exist.jpg"));
        }

        [Fact]
        public void ConstructorThrowsWhenFileContentIsInvalid()
        {
            byte[] imageContent = new byte[] { 32, 111, 209, 138, 76, 32 };
            Assert.ThrowsArgument(() => new WebImage(imageContent), "content",
                                                    "An image could not be constructed from the content provided.");
        }

        [Fact]
        public void FilePathReturnsCorrectPath()
        {
            // Arrange
            string imageName = @"x:\My-test-image.png";

            // Act
            WebImage image = new WebImage(GetContext(), s => _PngImageBytes, imageName);

            // Assert
            Assert.Equal(imageName, image.FileName);
        }

        [Fact]
        public void FilePathCanBeSet()
        {
            // Arrange
            string originalPath = @"x:\somePath.png";
            string newPath = @"x:\someOtherPath.jpg";

            // Act
            WebImage image = new WebImage(GetContext(), s => _PngImageBytes, originalPath);
            image.FileName = newPath;

            // Assert
            Assert.Equal(newPath, image.FileName);
        }

        [Fact]
        public void SimpleGetBytesClonesArray()
        {
            WebImage image = new WebImage(_PngImageBytes);

            byte[] returnedContent = image.GetBytes();

            Assert.False(ReferenceEquals(_PngImageBytes, returnedContent), "GetBytes should clone array.");
            Assert.Equal(_PngImageBytes, returnedContent);
        }

        [Fact]
        public void WebImagePreservesOriginalFormatFromFile()
        {
            WebImage image = new WebImage(_PngImageBytes);

            byte[] returnedContent = image.GetBytes();

            // If format was changed; content would be different
            Assert.Equal(_PngImageBytes, returnedContent);
        }

        [Fact]
        public void WebImagePreservesOriginalFormatFromStream()
        {
            WebImage image = null;
            byte[] originalContent = _PngImageBytes;
            using (MemoryStream stream = new MemoryStream(originalContent))
            {
                image = new WebImage(stream);
            } // dispose stream; WebImage should have no dependency on it

            byte[] returnedContent = image.GetBytes();

            // If format was changed; content would be different
            Assert.Equal(originalContent, returnedContent);
        }

        [Fact]
        public void WebImageCorrectlyReadsFromNoSeekStream()
        {
            WebImage image = null;

            byte[] originalContent = _PngImageBytes;
            using (MemoryStream stream = new MemoryStream(originalContent))
            {
                TestStream ts = new TestStream(stream);
                image = new WebImage(ts);
            } // dispose stream; WebImage should have no dependency on it

            byte[] returnedContent = image.GetBytes();

            // If chunks are not assembled correctly; content would be different and image would be corrupted.
            Assert.Equal(originalContent, returnedContent);
            Assert.Equal("png", image.ImageFormat);
        }

        [Fact]
        public void GetBytesWithNullReturnsClonesArray()
        {
            byte[] originalContent = _BmpImageBytes;
            WebImage image = new WebImage(originalContent);

            byte[] returnedContent = image.GetBytes();

            Assert.False(ReferenceEquals(originalContent, returnedContent), "GetBytes with string null should clone array.");
            Assert.Equal(originalContent, returnedContent);
        }

        [Fact]
        public void GetBytesWithSameFormatReturnsSameFormat()
        {
            byte[] originalContent = _JpgImageBytes;
            WebImage image = new WebImage(originalContent);

            byte[] returnedContent = image.GetBytes("jpeg");

            Assert.False(ReferenceEquals(originalContent, returnedContent), "GetBytes with string null should clone array.");
            Assert.Equal(originalContent, returnedContent);
        }

        [Fact]
        public void GetBytesWithDifferentFormatReturnsExpectedFormat()
        {
            byte[] originalContent = _BmpImageBytes;
            WebImage image = new WebImage(originalContent);

            // Request different format
            byte[] returnedContent = image.GetBytes("jpg");

            Assert.False(ReferenceEquals(originalContent, returnedContent), "GetBytes with string format should clone array.");
            using (MemoryStream stream = new MemoryStream(returnedContent))
            {
                using (Image tempImage = Image.FromStream(stream))
                {
                    Assert.Equal(ImageFormat.Jpeg, tempImage.RawFormat);
                }
            }
        }

        [Fact]
        public void GetBytesWithSameFormatReturnsSameFormatWhenCreatedFromFile()
        {
            byte[] originalContent = _BmpImageBytes;
            // Format is not set during construction.
            WebImage image = new WebImage(_BmpImageBytes);

            byte[] returnedContent = image.GetBytes("bmp");

            Assert.False(ReferenceEquals(originalContent, returnedContent), "GetBytes with string format should clone array.");
            Assert.Equal(originalContent, returnedContent);
        }

        [Fact]
        public void GetBytesWithNoFormatReturnsInitialFormatEvenAfterTransformations()
        {
            byte[] originalContent = _BmpImageBytes;
            // Format is not set during construction.
            WebImage image = new WebImage(_BmpImageBytes);
            image.Crop(top: 10, bottom: 10);

            byte[] returnedContent = image.GetBytes();

            Assert.NotEqual(originalContent, returnedContent);
            using (MemoryStream stream = new MemoryStream(returnedContent))
            {
                using (Image tempImage = Image.FromStream(stream))
                {
                    Assert.Equal(ImageFormat.Bmp, tempImage.RawFormat);
                }
            }
        }

        [Fact]
        public void GetBytesThrowsOnIncorrectFormat()
        {
            WebImage image = new WebImage(_JpgImageBytes);
            Assert.ThrowsArgument(
                () => image.GetBytes("bmpx"),
                "format",
                "\"bmpx\" is invalid image format. Valid values are image format names like: \"JPEG\", \"BMP\", \"GIF\", \"PNG\", etc.");
        }

        [Fact]
        public void GetBytesWithDifferentFormatReturnsExpectedFormatWhenCreatedFromFile()
        {
            // Format is not set during construction.
            WebImage image = new WebImage(_PngImageBytes);

            // Request different format
            byte[] returnedContent = image.GetBytes("jpg");

            WebImage newImage = new WebImage(returnedContent);

            Assert.Equal("jpeg", newImage.ImageFormat);
        }

        [Fact]
        public void GetImageFromRequestReturnsNullForIncorrectMimeType()
        {
            // Arrange
            Mock<HttpPostedFileBase> postedFile = new Mock<HttpPostedFileBase>();
            postedFile.Setup(c => c.FileName).Returns("index.cshtml");
            postedFile.Setup(c => c.ContentType).Returns("image/jpg");

            Mock<HttpFileCollectionBase> files = new Mock<HttpFileCollectionBase>();
            files.Setup(c => c[0]).Returns(postedFile.Object);
            Mock<HttpRequestBase> request = new Mock<HttpRequestBase>();
            request.Setup(r => r.Files).Returns(files.Object);

            // Act and Assert
            Assert.Null(WebImage.GetImageFromRequest(request.Object));
        }

        public static IEnumerable<object[]> GetImageFromRequestDeterminesMimeTypeFromExtensionData
        {
            get
            {
                yield return new object[] { "index.jpeg", _JpgImageBytes, "jpeg" };
                yield return new object[] { "file1.jpg", _JpgImageBytes, "jpeg" };
                yield return new object[] { "file4.png", _PngImageBytes, "png" };
                yield return new object[] { "file5.ico", _IcoImageBytes, "icon" };
            }
        }

        [Theory]
        [PropertyData("GetImageFromRequestDeterminesMimeTypeFromExtensionData")]
        public void GetImageFromRequestDeterminesMimeTypeFromExtension(string fileName,
                                                                       byte[] content,
                                                                       string expectedExtension)
        {
            // Arrange
            Mock<HttpPostedFileBase> postedFile = new Mock<HttpPostedFileBase>();
            postedFile.Setup(c => c.FileName).Returns(fileName);
            postedFile.Setup(c => c.ContentType).Returns("application/octet-stream");
            postedFile.Setup(c => c.ContentLength).Returns(1);
            postedFile.Setup(c => c.InputStream).Returns(new MemoryStream(content));

            Mock<HttpFileCollectionBase> files = new Mock<HttpFileCollectionBase>();
            files.Setup(c => c.Count).Returns(1);
            files.Setup(c => c[0]).Returns(postedFile.Object);
            Mock<HttpRequestBase> request = new Mock<HttpRequestBase>();
            request.Setup(r => r.Files).Returns(files.Object);

            // Act
            WebImage image = WebImage.GetImageFromRequest(request.Object);

            // Assert
            Assert.NotNull(image);
            Assert.Equal(expectedExtension, image.ImageFormat);
        }

        [Fact]
        public void GetImageFromRequestIsCaseInsensitive()
        {
            // Arrange
            Mock<HttpPostedFileBase> postedFile = new Mock<HttpPostedFileBase>();
            postedFile.SetupGet(c => c.FileName).Returns("index.JPg");
            postedFile.SetupGet(c => c.ContentType).Returns("application/octet-stream");
            postedFile.SetupGet(c => c.ContentLength).Returns(1);
            postedFile.SetupGet(c => c.InputStream).Returns(new MemoryStream(_JpgImageBytes));

            Mock<HttpFileCollectionBase> files = new Mock<HttpFileCollectionBase>();
            files.Setup(c => c.Count).Returns(1);
            files.Setup(c => c[0]).Returns(postedFile.Object);
            Mock<HttpRequestBase> request = new Mock<HttpRequestBase>();
            request.Setup(r => r.Files).Returns(files.Object);

            // Act
            WebImage image = WebImage.GetImageFromRequest(request.Object);

            // Assert
            Assert.NotNull(image);
            Assert.Equal("jpeg", image.ImageFormat);
        }

        [Fact]
        public void ImagePropertiesAreCorrectForBmpImage()
        {
            WebImage image = new WebImage(_BmpImageBytes);

            Assert.Equal("bmp", image.ImageFormat);
            Assert.Equal(108, image.Width);
            Assert.Equal(44, image.Height);
        }

        [Fact]
        public void ImagePropertiesAreCorrectForPngImage()
        {
            WebImage image = new WebImage(_PngImageBytes);

            Assert.Equal("png", image.ImageFormat);
            Assert.Equal(160, image.Width);
            Assert.Equal(152, image.Height);
        }

        [Fact]
        public void ImagePropertiesAreCorrectForJpgImage()
        {
            WebImage image = new WebImage(_JpgImageBytes);

            Assert.Equal("jpeg", image.ImageFormat);
            Assert.Equal(634, image.Width);
            Assert.Equal(489, image.Height);
        }

        [Fact]
        public void ResizePreservesRatio()
        {
            WebImage image = new WebImage(_JpgImageBytes);

            image.Resize(200, 100, preserveAspectRatio: true, preventEnlarge: true);

            Assert.Equal(130, image.Width);
            Assert.Equal(100, image.Height);
        }

        [Fact]
        public void ResizePreservesResolution()
        {
            MemoryStream output = null;
            Action<string, byte[]> saveAction = (_, content) => { output = new MemoryStream(content); };

            WebImage image = new WebImage(_PngImageBytes);

            image.Resize(100, 50, preserveAspectRatio: true, preventEnlarge: true);

            image.Save(GetContext(), saveAction, @"x:\ResizePreservesResolution.jpg", "jpeg", forceWellKnownExtension: true);
            using (Image original = Image.FromStream(new MemoryStream(_PngImageBytes)))
            {
                using (Image modified = Image.FromStream(output))
                {
                    Assert.Equal(original.HorizontalResolution, modified.HorizontalResolution);
                    Assert.Equal(original.VerticalResolution, modified.VerticalResolution);
                }
            }
        }

        [Fact]
        public void ResizePreservesFormat()
        {
            // Arrange
            WebImage image = new WebImage(_PngImageBytes);
            MemoryStream output = null;
            Action<string, byte[]> saveAction = (_, content) => { output = new MemoryStream(content); };

            // Act
            image.Resize(200, 100, preserveAspectRatio: true, preventEnlarge: true);

            // Assert
            Assert.Equal("png", image.ImageFormat);
            image.Save(GetContext(), saveAction, @"x:\1.png", null, false);

            using (Image modified = Image.FromStream(output))
            {
                Assert.Equal(ImageFormat.Png, modified.RawFormat);
            }
        }

        [Fact]
        public void SaveUpdatesFileNameOfWebImageWhenForcingWellKnownExtension()
        {
            // Arrange
            var context = GetContext();

            // Act
            WebImage image = new WebImage(context, _ => _JpgImageBytes, @"c:\images\foo.jpg");

            image.Save(context, (_, __) => { }, @"x:\1.exe", "jpg", forceWellKnownExtension: true);

            // Assert
            Assert.Equal(@"x:\1.exe.jpeg", image.FileName);
        }

        [Fact]
        public void SaveUpdatesFileNameOfWebImageWhenFormatChanges()
        {
            // Arrange
            string imagePath = @"x:\images\foo.jpg";
            var context = GetContext();

            // Act
            WebImage image = new WebImage(context, _ => _JpgImageBytes, imagePath);

            image.Save(context, (_, __) => { }, imagePath, "png", forceWellKnownExtension: true);

            // Assert
            Assert.Equal(@"x:\images\foo.jpg.png", image.FileName);
        }

        [Fact]
        public void SaveKeepsNameIfFormatIsUnchanged()
        {
            // Arrange
            string imagePath = @"x:\images\foo.jpg";
            var context = GetContext();

            // Act
            WebImage image = new WebImage(context, _ => _JpgImageBytes, imagePath);

            image.Save(context, (_, __) => { }, imagePath, "jpg", forceWellKnownExtension: true);

            // Assert
            Assert.Equal(@"x:\images\foo.jpg", image.FileName);
        }

        [Fact]
        public void ResizeThrowsOnIncorrectWidthOrHeight()
        {
            WebImage image = new WebImage(_JpgImageBytes);

            Assert.ThrowsArgumentGreaterThan(
                () => image.Resize(-1, 100, preserveAspectRatio: true, preventEnlarge: true),
                "width",
                "0");

            Assert.ThrowsArgumentGreaterThan(
                () => image.Resize(100, -1, preserveAspectRatio: true, preventEnlarge: true),
                "height",
                "0");
        }

        [Fact]
        public void ResizeAndRotateDoesOperationsInRightOrder()
        {
            WebImage image = new WebImage(_JpgImageBytes);
            image.Resize(200, 100, preserveAspectRatio: true, preventEnlarge: true).RotateLeft();

            Assert.Equal(100, image.Width);
            Assert.Equal(130, image.Height);
        }

        [Fact]
        public void ClonePreservesAllInformation()
        {
            WebImage image = new WebImage(_JpgImageBytes);
            image.Resize(200, 100, preserveAspectRatio: true, preventEnlarge: true).RotateLeft();

            // this should preserve list of transformations
            WebImage cloned = image.Clone();

            Assert.Equal(100, cloned.Width);
            Assert.Equal(130, cloned.Height);
        }

        [Fact]
        public void ResizePreventsEnlarge()
        {
            WebImage image = new WebImage(_JpgImageBytes);

            int height = image.Height;
            int width = image.Width;

            image.Resize(width * 2, height, preserveAspectRatio: true, preventEnlarge: true);
            Assert.Equal(width, image.Width);
            Assert.Equal(height, image.Height);
        }

        [Fact]
        public void CropCreatesCroppedImage()
        {
            WebImage image = new WebImage(_JpgImageBytes);
            image.Crop(20, 20, 20, 20);

            Assert.Equal(594, image.Width);
            Assert.Equal(449, image.Height);
        }

        [Fact]
        public void CropThrowsOnIncorrectArguments()
        {
            WebImage image = new WebImage(_JpgImageBytes);

            Assert.ThrowsArgumentGreaterThanOrEqualTo(
                () => image.Crop(top: -1),
                "top",
                "0");

            Assert.ThrowsArgumentGreaterThanOrEqualTo(
                () => image.Crop(left: -1),
                "left",
                "0");

            Assert.ThrowsArgumentGreaterThanOrEqualTo(
                () => image.Crop(bottom: -1),
                "bottom",
                "0");

            Assert.ThrowsArgumentGreaterThanOrEqualTo(
                () => image.Crop(right: -1),
                "right",
                "0");
        }

        [Fact]
        public void RotateLeftReturnsRotatedImage()
        {
            WebImage image = new WebImage(_PngImageBytes);
            image.RotateLeft();

            Assert.Equal(152, image.Width);
            Assert.Equal(160, image.Height);
        }

        [Fact]
        public void RotateRightReturnsRotatedImage()
        {
            WebImage image = new WebImage(_PngImageBytes);
            image.RotateRight();

            Assert.Equal(152, image.Width);
            Assert.Equal(160, image.Height);
        }

        [Fact]
        public void FlipVerticalReturnsFlippedImage()
        {
            WebImage image = new WebImage(_PngImageBytes);
            image.FlipVertical();

            Assert.Equal(160, image.Width);
            Assert.Equal(152, image.Height);
        }

        [Fact]
        public void FlipHorizontalReturnsFlippedImage()
        {
            WebImage image = new WebImage(_PngImageBytes);
            image.FlipHorizontal();

            Assert.Equal(160, image.Width);
            Assert.Equal(152, image.Height);
        }

        [Fact]
        public void MultipleCombinedOperationsExecuteInRightOrder()
        {
            WebImage image = new WebImage(_JpgImageBytes);
            image.Resize(200, 100, preserveAspectRatio: true, preventEnlarge: true).RotateLeft();
            image.Crop(top: 10, right: 10).AddTextWatermark("plan9");

            Assert.Equal(90, image.Width);
            Assert.Equal(120, image.Height);
        }

        [Fact]
        public void AddTextWatermarkPreservesImageDimension()
        {
            WebImage image = new WebImage(_JpgImageBytes);
            image.AddTextWatermark("Plan9", fontSize: 16, horizontalAlign: "Left", verticalAlign: "Bottom", opacity: 50);

            Assert.Equal(634, image.Width);
            Assert.Equal(489, image.Height);
        }

        [Fact]
        public void AddTextWatermarkParsesHexColorCorrectly()
        {
            WebImage image = new WebImage(_JpgImageBytes);
            image.AddTextWatermark("Plan9", fontSize: 16, fontColor: "#FF0000", horizontalAlign: "Center", verticalAlign: "Middle");

            Assert.Equal(634, image.Width);
            Assert.Equal(489, image.Height);
        }

        [Fact]
        public void AddTextWatermarkParsesShortHexColorCorrectly()
        {
            WebImage image = new WebImage(_JpgImageBytes);
            image.AddTextWatermark("Plan9", fontSize: 16, fontColor: "#F00", horizontalAlign: "Center", verticalAlign: "Middle");

            Assert.Equal(634, image.Width);
            Assert.Equal(489, image.Height);
        }

        [Fact]
        public void AddTextWatermarkDoesNotChangeImageIfPaddingIsTooBig()
        {
            WebImage image = new WebImage(_JpgImageBytes);
            image.AddTextWatermark("Plan9", padding: 1000);

            Assert.Equal(634, image.Width);
            Assert.Equal(489, image.Height);
        }

        [Fact]
        public void AddTextWatermarkThrowsOnNegativeOpacity()
        {
            WebImage image = new WebImage(_JpgImageBytes);

            Assert.ThrowsArgumentOutOfRange(() => image.AddTextWatermark("Plan9", opacity: -1), "opacity", "Value must be between 0 and 100.");
        }

        [Fact]
        public void AddTextWatermarkThrowsOnTooBigOpacity()
        {
            WebImage image = new WebImage(_JpgImageBytes);

            Assert.ThrowsArgumentOutOfRange(() => image.AddTextWatermark("Plan9", opacity: 155), "opacity", "Value must be between 0 and 100.");
        }

        [Fact]
        public void AddTextWatermarkThrowsOnEmptyText()
        {
            WebImage image = new WebImage(_JpgImageBytes);
            Assert.ThrowsArgumentNullOrEmptyString(
                () => image.AddTextWatermark(""),
                "text");
        }

        [Fact]
        public void AddTextWatermarkThrowsOnIncorrectColorName()
        {
            WebImage image = new WebImage(_JpgImageBytes);
            Assert.Throws<ArgumentException>(
                () => image.AddTextWatermark("p9", fontColor: "super"),
                "The \"fontColor\" value is invalid. Valid values are names like \"White\", \"Black\", or \"DarkBlue\", or hexadecimal values in the form \"#RRGGBB\" or \"#RGB\".");
        }

        [Fact]
        public void AddTextWatermarkThrowsOnIncorrectHexColorValue()
        {
            WebImage image = new WebImage(_JpgImageBytes);
            Assert.Throws<ArgumentException>(
                () => image.AddTextWatermark("p9", fontColor: "#XXX"),
                "The \"fontColor\" value is invalid. Valid values are names like \"White\", \"Black\", or \"DarkBlue\", or hexadecimal values in the form \"#RRGGBB\" or \"#RGB\".");
        }

        [Fact]
        public void AddTextWatermarkThrowsOnIncorrectHexColorLength()
        {
            WebImage image = new WebImage(_JpgImageBytes);
            Assert.Throws<ArgumentException>(
                () => image.AddTextWatermark("p9", fontColor: "#F000"),
                "The \"fontColor\" value is invalid. Valid values are names like \"White\", \"Black\", or \"DarkBlue\", or hexadecimal values in the form \"#RRGGBB\" or \"#RGB\".");
        }

        [Fact]
        public void AddTextWatermarkThrowsOnIncorrectHorizontalAlignment()
        {
            WebImage image = new WebImage(_JpgImageBytes);
            Assert.Throws<ArgumentException>(
                () => image.AddTextWatermark("p9", horizontalAlign: "Justify"),
                "The \"horizontalAlign\" value is invalid. Valid values are: \"Right\", \"Left\", and \"Center\".");
        }

        [Fact]
        public void AddTextWatermarkThrowsOnIncorrectVerticalAlignment()
        {
            WebImage image = new WebImage(_JpgImageBytes);
            Assert.Throws<ArgumentException>(
                () => image.AddTextWatermark("p9", verticalAlign: "NotSet"),
                "The \"verticalAlign\" value is invalid. Valid values are: \"Top\", \"Bottom\", and \"Middle\".");
        }

        [Fact]
        public void AddTextWatermarkThrowsOnNegativePadding()
        {
            WebImage image = new WebImage(_JpgImageBytes);
            Assert.ThrowsArgumentGreaterThanOrEqualTo(
                () => image.AddTextWatermark("p9", padding: -10),
                "padding",
                "0");
        }

        [Fact]
        public void AddTextWatermarkThrowsOnIncorrectFontSize()
        {
            WebImage image = new WebImage(_JpgImageBytes);
            Assert.ThrowsArgumentGreaterThan(
                () => image.AddTextWatermark("p9", fontSize: -10),
                "fontSize",
                "0");

            Assert.ThrowsArgumentGreaterThan(
                () => image.AddTextWatermark("p9", fontSize: 0),
                "fontSize",
                "0");
        }

        [Fact]
        public void AddTextWatermarkThrowsOnIncorrectFontStyle()
        {
            WebImage image = new WebImage(_JpgImageBytes);

            Assert.Throws<ArgumentException>(
                () => image.AddTextWatermark("p9", fontStyle: "something"),
                "The \"fontStyle\" value is invalid. Valid values are: \"Regular\", \"Bold\", \"Italic\", \"Underline\", and \"Strikeout\".");
        }

        [Fact]
        public void AddTextWatermarkThrowsOnIncorrectFontFamily()
        {
            WebImage image = new WebImage(_JpgImageBytes);

            Assert.Throws<ArgumentException>(
                () => image.AddTextWatermark("p9", fontFamily: "something"),
                "The \"fontFamily\" value is invalid. Valid values are font family names like: \"Arial\", \"Times New Roman\", etc. Make sure that the font family you are trying to use is installed on the server.");
        }

        [Fact]
        public void AddImageWatermarkPreservesImageDimension()
        {
            WebImage watermark = new WebImage(_BmpImageBytes);
            WebImage image = new WebImage(_JpgImageBytes);
            image.AddImageWatermark(watermark, horizontalAlign: "LEFT", verticalAlign: "top", opacity: 50, padding: 10);

            Assert.Equal(634, image.Width);
            Assert.Equal(489, image.Height);
        }

        [Fact]
        public void CanAddTextAndImageWatermarks()
        {
            WebImage watermark = new WebImage(_BmpImageBytes);
            WebImage image = new WebImage(_JpgImageBytes);
            image.AddImageWatermark(watermark, horizontalAlign: "LEFT", verticalAlign: "top", opacity: 30, padding: 10);
            image.AddTextWatermark("plan9");

            Assert.Equal(634, image.Width);
            Assert.Equal(489, image.Height);
        }

        [Fact]
        public void AddImageWatermarkDoesNotChangeWatermarkImage()
        {
            WebImage watermark = new WebImage(_BmpImageBytes);
            WebImage image = new WebImage(_JpgImageBytes);
            image.AddImageWatermark(watermark, width: 54, height: 22, horizontalAlign: "LEFT", verticalAlign: "top", opacity: 50, padding: 10);

            Assert.Equal(108, watermark.Width);
            Assert.Equal(44, watermark.Height);
        }

        [Fact]
        public void AddImageWatermarkThrowsOnNullImage()
        {
            WebImage image = new WebImage(_JpgImageBytes);

            Assert.ThrowsArgumentNull(
                () => image.AddImageWatermark(watermarkImage: null),
                "watermarkImage");
        }

        [Fact]
        public void AddImageWatermarkThrowsWhenJustOneDimensionIsZero()
        {
            WebImage watermark = new WebImage(_BmpImageBytes);
            WebImage image = new WebImage(_JpgImageBytes);

            string message = "Watermark width and height must both be positive or both be zero.";
            Assert.Throws<ArgumentException>(
                () => image.AddImageWatermark(watermark, width: 0, height: 22), message);

            Assert.Throws<ArgumentException>(
                () => image.AddImageWatermark(watermark, width: 100, height: 0), message);
        }

        [Fact]
        public void AddImageWatermarkThrowsWhenOpacityIsIncorrect()
        {
            WebImage watermark = new WebImage(_BmpImageBytes);
            WebImage image = new WebImage(_JpgImageBytes);

            Assert.ThrowsArgumentOutOfRange(() => image.AddImageWatermark(watermark, opacity: -1), "opacity", "Value must be between 0 and 100.");

            Assert.ThrowsArgumentOutOfRange(() => image.AddImageWatermark(watermark, opacity: 120), "opacity", "Value must be between 0 and 100.");
        }

        [Fact]
        public void AddImageWatermarkThrowsOnNegativeDimensions()
        {
            WebImage watermark = new WebImage(_BmpImageBytes);
            WebImage image = new WebImage(_JpgImageBytes);

            Assert.ThrowsArgumentGreaterThanOrEqualTo(
                () => image.AddImageWatermark(watermark, width: -1),
                "width",
                "0");

            Assert.ThrowsArgumentGreaterThanOrEqualTo(
                () => image.AddImageWatermark(watermark, height: -1),
                "height",
                "0");
        }

        [Fact]
        public void AddImageWatermarkThrowsOnIncorrectHorizontalAlignment()
        {
            WebImage watermark = new WebImage(_BmpImageBytes);
            WebImage image = new WebImage(_JpgImageBytes);

            Assert.Throws<ArgumentException>(
                () => image.AddImageWatermark(watermark, horizontalAlign: "horizontal"),
                "The \"horizontalAlign\" value is invalid. Valid values are: \"Right\", \"Left\", and \"Center\".");
        }

        [Fact]
        public void AddImageWatermarkThrowsOnIncorrectVerticalAlignment()
        {
            WebImage watermark = new WebImage(_BmpImageBytes);
            WebImage image = new WebImage(_JpgImageBytes);

            Assert.Throws<ArgumentException>(
                () => image.AddImageWatermark(watermark, verticalAlign: "vertical"),
                "The \"verticalAlign\" value is invalid. Valid values are: \"Top\", \"Bottom\", and \"Middle\".");
        }

        [Fact]
        public void AddImageWatermarkThrowsOnNegativePadding()
        {
            WebImage watermark = new WebImage(_BmpImageBytes);
            WebImage image = new WebImage(_JpgImageBytes);

            Assert.ThrowsArgumentGreaterThanOrEqualTo(
                () => image.AddImageWatermark(watermark, padding: -10),
                "padding",
                "0");
        }

        [Fact]
        public void AddImageWatermarkDoesNotChangeImageIfWatermarkIsTooBig()
        {
            WebImage watermark = new WebImage(_JpgImageBytes);
            WebImage image = new WebImage(_BmpImageBytes);
            byte[] originalBytes = image.GetBytes("jpg");

            // This will use original watermark image dimensions which is bigger than the target image.
            image.AddImageWatermark(watermark);
            byte[] watermarkedBytes = image.GetBytes("jpg");

            Assert.Equal(originalBytes, watermarkedBytes);
        }

        [Fact]
        public void AddImageWatermarkWithFileNameThrowsExceptionWhenWatermarkDirectoryDoesNotExist()
        {
            var context = GetContext();
            WebImage image = new WebImage(_BmpImageBytes);

            Assert.Throws<DirectoryNotFoundException>(
                () => image.AddImageWatermark(context, s => { throw new DirectoryNotFoundException(); }, @"x:\path\does\not\exist", width: 0, height: 0, horizontalAlign: "Right", verticalAlign: "Bottom", opacity: 100, padding: 5));
        }

        [Fact]
        public void AddImageWatermarkWithFileNameThrowsExceptionWhenWatermarkFileDoesNotExist()
        {
            var context = GetContext();
            WebImage image = new WebImage(_BmpImageBytes);
            Assert.Throws<FileNotFoundException>(
                () => image.AddImageWatermark(context, s => { throw new FileNotFoundException(); }, @"x:\there-is-no-file.jpg", width: 0, height: 0, horizontalAlign: "Right", verticalAlign: "Bottom", opacity: 100, padding: 5));
        }

        [Fact]
        public void AddImageWatermarkWithFileNameThrowsExceptionWhenWatermarkFilePathIsNull()
        {
            var context = GetContext();

            WebImage image = new WebImage(_BmpImageBytes);
            Assert.ThrowsArgument(
                () => image.AddImageWatermark(context, s => _JpgImageBytes, watermarkImageFilePath: null, width: 0, height: 0, horizontalAlign: "Right", verticalAlign: "Bottom", opacity: 100, padding: 5),
                "filePath",
                "Value cannot be null or an empty string.");
        }

        [Fact]
        public void AddImageWatermarkWithFileNameThrowsExceptionWhenWatermarkFilePathIsEmpty()
        {
            var context = GetContext();
            WebImage image = new WebImage(_BmpImageBytes);
            Assert.ThrowsArgument(
                () => image.AddImageWatermark(context, s => _JpgImageBytes, watermarkImageFilePath: null, width: 0, height: 0, horizontalAlign: "Right", verticalAlign: "Bottom", opacity: 100, padding: 5),
                "filePath",
                "Value cannot be null or an empty string.");
        }

        [Fact]
        public void CanAddImageWatermarkWithFileName()
        {
            // Arrange
            var context = GetContext();
            WebImage image = new WebImage(_BmpImageBytes);
            WebImage watermark = new WebImage(_JpgImageBytes);

            // Act
            var watermarkedWithImageArgument = image.AddImageWatermark(watermark).GetBytes();
            var watermarkedWithFilePathArgument = image.AddImageWatermark(context, (name) => _JpgImageBytes, @"x:\jpegimage.jpg", width: 0, height: 0, horizontalAlign: "Right", verticalAlign: "Bottom", opacity: 100, padding: 5).GetBytes();

            Assert.Equal(watermarkedWithImageArgument, watermarkedWithFilePathArgument);
        }

        [Fact]
        public void SaveOverwritesExistingFile()
        {
            Action<string, byte[]> saveAction = (path, content) => { };

            WebImage image = new WebImage(_BmpImageBytes);
            string newFileName = @"x:\newImage.bmp";

            image.Save(GetContext(), saveAction, newFileName, imageFormat: null, forceWellKnownExtension: true);

            image.RotateLeft();
            // just verify this does not throw
            image.Save(GetContext(), saveAction, newFileName, imageFormat: null, forceWellKnownExtension: true);
        }

        [Fact]
        public void SaveThrowsWhenPathIsNull()
        {
            Action<string, byte[]> saveAction = (path, content) => { };

            // this constructor will not set path
            byte[] originalContent = _BmpImageBytes;
            WebImage image = new WebImage(originalContent);

            Assert.ThrowsArgumentNullOrEmptyString(
                () => image.Save(GetContext(), saveAction, filePath: null, imageFormat: null, forceWellKnownExtension: true),
                "filePath");
        }

        [Fact]
        public void SaveThrowsWhenPathIsEmpty()
        {
            Action<string, byte[]> saveAction = (path, content) => { };
            WebImage image = new WebImage(_BmpImageBytes);

            Assert.ThrowsArgumentNullOrEmptyString(
                () => image.Save(GetContext(), saveAction, filePath: String.Empty, imageFormat: null, forceWellKnownExtension: true),
                "filePath");
        }

        [Fact]
        public void SaveUsesOriginalFormatWhenNoFormatIsSpecified()
        {
            // Arrange
            // Use rooted path so we by pass using HttpContext
            var specifiedOutputFile = @"C:\some-dir\foo.jpg";
            string actualOutputFile = null;
            Action<string, byte[]> saveAction = (fileName, content) => { actualOutputFile = fileName; };

            // Act
            WebImage image = new WebImage(_PngImageBytes);
            image.Save(GetContext(), saveAction, filePath: specifiedOutputFile, imageFormat: null, forceWellKnownExtension: true);

            // Assert
            Assert.Equal(".png", Path.GetExtension(actualOutputFile));
        }

        [Fact]
        public void SaveUsesOriginalFormatForStreamsWhenNoFormatIsSpecified()
        {
            // Arrange
            // Use rooted path so we by pass using HttpContext
            var specifiedOutputFile = @"x:\some-dir\foo.jpg";
            string actualOutputFile = null;
            Action<string, byte[]> saveAction = (fileName, content) => { actualOutputFile = fileName; };

            // Act
            WebImage image = new WebImage(_PngImageBytes);
            image.Save(GetContext(), saveAction, filePath: specifiedOutputFile, imageFormat: null, forceWellKnownExtension: true);

            // Assert
            Assert.Equal(".png", Path.GetExtension(actualOutputFile));
        }

        [Fact]
        public void SaveSetsExtensionBasedOnFormatWhenForceExtensionIsSet()
        {
            // Arrange
            // Use rooted path so we by pass using HttpContext
            var specifiedOutputFile = @"x:\some-dir\foo.exe";
            string actualOutputFile = null;
            Action<string, byte[]> saveAction = (fileName, content) => { actualOutputFile = fileName; };

            // Act
            WebImage image = new WebImage(_BmpImageBytes);
            image.Save(GetContext(), saveAction, filePath: specifiedOutputFile, imageFormat: "jpg", forceWellKnownExtension: true);

            // Assert
            Assert.Equal(".jpeg", Path.GetExtension(actualOutputFile));
            Assert.Equal(specifiedOutputFile + ".jpeg", actualOutputFile);
        }

        [Fact]
        public void SaveAppendsExtensionBasedOnFormatWhenForceExtensionIsSet()
        {
            // Arrange
            // Use rooted path so we by pass using HttpContext
            var specifiedOutputFile = @"x:\some-dir\foo";
            string actualOutputFile = null;
            Action<string, byte[]> saveAction = (fileName, content) => { actualOutputFile = fileName; };

            // Act
            WebImage image = new WebImage(_BmpImageBytes);
            image.Save(GetContext(), saveAction, filePath: specifiedOutputFile, imageFormat: "jpg", forceWellKnownExtension: true);

            // Assert
            Assert.Equal(".jpeg", Path.GetExtension(actualOutputFile));
        }

        [Fact]
        public void SaveDoesNotModifyExtensionWhenExtensionIsCorrect()
        {
            // Arrange
            // Use rooted path so we by pass using HttpContext
            var specifiedOutputFile = @"x:\some-dir\foo.jpg";
            string actualOutputFile = null;
            Action<string, byte[]> saveAction = (fileName, content) => { actualOutputFile = fileName; };

            // Act
            WebImage image = new WebImage(_BmpImageBytes);
            image.Save(GetContext(), saveAction, filePath: specifiedOutputFile, imageFormat: "jpg", forceWellKnownExtension: true);

            // Assert
            Assert.Equal(specifiedOutputFile, actualOutputFile);
        }

        [Fact]
        public void SaveDoesNotModifyExtensionWhenForceCorrectExtensionRenameIsCleared()
        {
            // Arrange
            // Use rooted path so we by pass using HttpContext
            var specifiedOutputFile = @"x:\some-dir\foo.exe";
            string actualOutputFile = null;
            Action<string, byte[]> saveAction = (fileName, content) => { actualOutputFile = fileName; };

            // Act
            WebImage image = new WebImage(_BmpImageBytes);
            image.Save(GetContext(), saveAction, filePath: specifiedOutputFile, imageFormat: "jpg", forceWellKnownExtension: false);

            // Assert
            Assert.Equal(specifiedOutputFile, actualOutputFile);
        }

        [Fact]
        public void ImageFormatIsSavedCorrectly()
        {
            WebImage image = new WebImage(_BmpImageBytes);
            Assert.Equal("bmp", image.ImageFormat);
        }

        [Fact]
        public void SaveUsesInitialFormatWhenNoFormatIsSpecified()
        {
            // Arrange
            string savePath = @"x:\some-dir\image.png";
            MemoryStream stream = null;
            Action<string, byte[]> saveAction = (path, content) => { stream = new MemoryStream(content); };
            var image = new WebImage(_PngImageBytes);

            // Act 
            image.FlipVertical().FlipHorizontal();

            // Assert
            image.Save(GetContext(), saveAction, savePath, imageFormat: null, forceWellKnownExtension: true);

            using (Image savedImage = Image.FromStream(stream))
            {
                Assert.Equal(savedImage.RawFormat, ImageFormat.Png);
            }
        }

        [Fact]
        public void ImageFormatIsParsedCorrectly()
        {
            WebImage image = new WebImage(_BmpImageBytes);
            Assert.Equal("bmp", image.ImageFormat);
        }

        private static HttpContextBase GetContext()
        {
            var httpContext = new Mock<HttpContextBase>();
            var httpRequest = new Mock<HttpRequestBase>();
            httpRequest.Setup(c => c.MapPath(It.IsAny<string>())).Returns((string path) => path);
            httpContext.Setup(c => c.Request).Returns(httpRequest.Object);

            return httpContext.Object;
        }

        // Test stream that pretends it can't seek. 
        private class TestStream : Stream
        {
            private MemoryStream _memoryStream;

            public TestStream(MemoryStream memoryStream)
            {
                _memoryStream = memoryStream;
            }

            public override bool CanRead
            {
                get { return _memoryStream.CanRead; }
            }

            public override bool CanSeek
            {
                get { return false; }
            }

            public override bool CanWrite
            {
                get { return _memoryStream.CanWrite; }
            }

            public override void Flush()
            {
                _memoryStream.Flush();
            }

            public override long Length
            {
                get { throw new NotSupportedException(); }
            }

            public override long Position
            {
                get { return _memoryStream.Position; }
                set { _memoryStream.Position = value; }
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                return _memoryStream.Read(buffer, offset, count);
            }

            public override long Seek(long offset, SeekOrigin origin)
            {
                throw new NotSupportedException();
            }

            public override void SetLength(long value)
            {
                _memoryStream.SetLength(value);
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                _memoryStream.Write(buffer, offset, count);
            }
        }
    }
}
