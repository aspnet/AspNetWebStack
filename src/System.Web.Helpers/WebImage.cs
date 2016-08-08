// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web.Helpers.Resources;
using System.Web.UI.WebControls;
using Microsoft.Internal.Web.Utils;
using Image = System.Drawing.Image;

namespace System.Web.Helpers
{
    public class WebImage
    {
        // Default resolution to use when getting bitmap from image
        private const float FixedResolution = 96f;

        private static readonly IDictionary<Guid, ImageFormat> _imageFormatLookup = new[]
        {
            Drawing.Imaging.ImageFormat.Bmp, Drawing.Imaging.ImageFormat.Emf, Drawing.Imaging.ImageFormat.Exif,
            Drawing.Imaging.ImageFormat.Gif, Drawing.Imaging.ImageFormat.Icon, Drawing.Imaging.ImageFormat.Jpeg,
            Drawing.Imaging.ImageFormat.MemoryBmp, Drawing.Imaging.ImageFormat.Png, Drawing.Imaging.ImageFormat.Tiff,
            Drawing.Imaging.ImageFormat.Wmf
        }.ToDictionary(format => format.Guid, format => format);

        private static readonly Func<string, byte[]> _defaultReadAction = File.ReadAllBytes;

        // Initial format is the format of the image when it was constructed. 
        // Current format is the format currently stored in the content buffer. This can
        // be different than initial format since image transformations can change format.
        private readonly ImageFormat _initialFormat;
        private readonly List<ImageTransformation> _transformations = new List<ImageTransformation>();
        private ImageFormat _currentFormat;
        private byte[] _content;
        private string _fileName;
        private int _height = -1;
        private int _width = -1;

        private PropertyItem[] _properties; // image metadata

        public WebImage(byte[] content)
        {
            _initialFormat = ValidateImageContent(content, "content");
            _currentFormat = _initialFormat;
            _content = (byte[])content.Clone();
        }

        public WebImage(string filePath)
            : this(new HttpContextWrapper(HttpContext.Current), _defaultReadAction, filePath)
        {
        }

        public WebImage(Stream imageStream)
        {
            if (imageStream.CanSeek)
            {
                imageStream.Seek(0, SeekOrigin.Begin);

                _content = new byte[imageStream.Length];
                using (BinaryReader reader = new BinaryReader(imageStream))
                {
                    reader.Read(_content, 0, (int)imageStream.Length);
                }
            }
            else
            {
                List<byte[]> chunks = new List<byte[]>();
                int totalSize = 0;
                using (BinaryReader reader = new BinaryReader(imageStream))
                {
                    // Pick some size for chunks that is still under limit
                    // that causes them to be placed on the large object heap.
                    int chunkSizeInBytes = 1024 * 50;
                    byte[] nextChunk = null;
                    do
                    {
                        nextChunk = reader.ReadBytes(chunkSizeInBytes);
                        totalSize += nextChunk.Length;
                        chunks.Add(nextChunk);
                    }
                    while (nextChunk.Length == chunkSizeInBytes);
                }

                _content = new byte[totalSize];
                int startIndex = 0;
                foreach (var chunk in chunks)
                {
                    chunk.CopyTo(_content, startIndex);
                    startIndex += chunk.Length;
                }
            }
            _initialFormat = ValidateImageContent(_content, "imageStream");
            _currentFormat = _initialFormat;
        }

        internal WebImage(HttpContextBase httpContext, Func<string, byte[]> readAction, string filePath)
        {
            if (String.IsNullOrEmpty(filePath))
            {
                throw new ArgumentException(CommonResources.Argument_Cannot_Be_Null_Or_Empty, "filePath");
            }

            _fileName = filePath;
            _content = readAction(VirtualPathUtil.MapPath(httpContext, filePath));
            _initialFormat = ValidateImageContent(_content, "filePath");
            _currentFormat = _initialFormat;
        }

        private WebImage(WebImage other)
        {
            Debug.Assert(other != null);
            Debug.Assert(other._content != null, "Incorrectly constructed instance.");

            // We are not validating the contents from this constructor since its a copy constructor. 
            _content = (byte[])other._content.Clone();
            _initialFormat = other._initialFormat;
            _currentFormat = other._currentFormat;
            _fileName = other._fileName;

            _height = other._height;
            _width = other._width;

            _properties = (other._properties != null) ? (PropertyItem[])other._properties.Clone() : null;

            _transformations = new List<ImageTransformation>(other._transformations);
        }

        public int Height
        {
            get
            {
                if ((_transformations.Count > 0) || (_height < 0))
                {
                    ApplyTransformationsAndSetProperties();
                }
                return _height;
            }
        }

        public int Width
        {
            get
            {
                if ((_transformations.Count > 0) || (_width < 0))
                {
                    ApplyTransformationsAndSetProperties();
                }
                return _width;
            }
        }

        public string FileName
        {
            get { return _fileName; }
            set { _fileName = value; }
        }

        [SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase",
            Justification = "No security decision is made based on this string. It's customary to display MIME format in lowercase.")]
        public string ImageFormat
        {
            get
            {
                if (_transformations.Any())
                {
                    ApplyTransformationsAndSetProperties();
                }

                Debug.Assert(_currentFormat != null);
                return ConversionUtil.ToString(_currentFormat).ToLowerInvariant();
            }
        }

        public static WebImage GetImageFromRequest(string postedFileName = null)
        {
            var request = new HttpRequestWrapper(HttpContext.Current.Request);
            return GetImageFromRequest(request, postedFileName);
        }

        internal static WebImage GetImageFromRequest(HttpRequestBase request, string postedFileName = null)
        {
            Debug.Assert(request != null);
            if ((request.Files == null) || (request.Files.Count == 0))
            {
                return null;
            }
            HttpPostedFileBase file = String.IsNullOrEmpty(postedFileName) ? request.Files[0] : request.Files[postedFileName];
            if (file == null || file.ContentLength < 1)
            {
                return null;
            }

            // The content type is specified by the browser and is unreliable. 
            // Disregard content type, acquire mime type.
            ImageFormat format;
            string mimeType = MimeMapping.GetMimeMapping(file.FileName);
            if (!ConversionUtil.TryFromStringToImageFormat(mimeType, out format))
            {
                // Unsupported image format.
                return null;
            }

            WebImage webImage = new WebImage(file.InputStream);
            webImage.FileName = file.FileName;
            return webImage;
        }

        public WebImage Clone()
        {
            return new WebImage(this);
        }

        public byte[] GetBytes(string requestedFormat = null)
        {
            if (_transformations.Count > 0)
            {
                ApplyTransformationsAndSetProperties();
            }

            ImageFormat requestedImageFormat = null;
            if (!String.IsNullOrEmpty(requestedFormat))
            {
                // This will throw if image format is incorrect.
                requestedImageFormat = GetImageFormat(requestedFormat);
            }

            requestedImageFormat = requestedImageFormat ?? _initialFormat;
            Debug.Assert(requestedImageFormat != null, "Initial format can never be null");
            if (requestedImageFormat.Equals(_currentFormat))
            {
                return (byte[])_content.Clone();
            }

            // Conversion from one format to another
            using (MemoryStream sourceBuffer = new MemoryStream(_content))
            {
                using (Image image = Image.FromStream(sourceBuffer))
                {
                    // if _properties are not initialized that means image did not go through any
                    // transformations yet and original byte array contains all metadata available
                    if (_properties != null)
                    {
                        CopyMetadata(_properties, image);
                    }

                    using (MemoryStream destinationBuffer = new MemoryStream())
                    {
                        image.Save(destinationBuffer, requestedImageFormat);
                        return destinationBuffer.ToArray();
                    }
                }
            }
        }

        public WebImage Resize(int width, int height, bool preserveAspectRatio = true, bool preventEnlarge = false)
        {
            if (width <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    "width",
                    String.Format(CultureInfo.InvariantCulture, CommonResources.Argument_Must_Be_GreaterThan, 0));
            }
            if (height <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    "height",
                    String.Format(CultureInfo.InvariantCulture, CommonResources.Argument_Must_Be_GreaterThan, 0));
            }

            ResizeTransformation trans = new ResizeTransformation(height, width, preserveAspectRatio, preventEnlarge);
            _transformations.Add(trans);
            return this;
        }

        public WebImage Crop(int top = 0, int left = 0, int bottom = 0, int right = 0)
        {
            if (top < 0)
            {
                throw new ArgumentOutOfRangeException(
                    "top",
                    String.Format(CultureInfo.InvariantCulture, CommonResources.Argument_Must_Be_GreaterThanOrEqualTo, 0));
            }
            if (left < 0)
            {
                throw new ArgumentOutOfRangeException(
                    "left",
                    String.Format(CultureInfo.InvariantCulture, CommonResources.Argument_Must_Be_GreaterThanOrEqualTo, 0));
            }
            if (bottom < 0)
            {
                throw new ArgumentOutOfRangeException(
                    "bottom",
                    String.Format(CultureInfo.InvariantCulture, CommonResources.Argument_Must_Be_GreaterThanOrEqualTo, 0));
            }
            if (right < 0)
            {
                throw new ArgumentOutOfRangeException(
                    "right",
                    String.Format(CultureInfo.InvariantCulture, CommonResources.Argument_Must_Be_GreaterThanOrEqualTo, 0));
            }

            CropTransformation crop = new CropTransformation(top, right, bottom, left);
            _transformations.Add(crop);
            return this;
        }

        public WebImage RotateLeft()
        {
            ImageTransformation transform = new RotateTransformation(RotateFlipType.Rotate270FlipNone);
            _transformations.Add(transform);
            return this;
        }

        public WebImage RotateRight()
        {
            ImageTransformation transform = new RotateTransformation(RotateFlipType.Rotate90FlipNone);
            _transformations.Add(transform);
            return this;
        }

        public WebImage FlipVertical()
        {
            ImageTransformation transform = new RotateTransformation(RotateFlipType.RotateNoneFlipY);
            _transformations.Add(transform);
            return this;
        }

        public WebImage FlipHorizontal()
        {
            ImageTransformation transform = new RotateTransformation(RotateFlipType.RotateNoneFlipX);
            _transformations.Add(transform);
            return this;
        }

        /// <summary>
        /// Adds text watermark to a WebImage.
        /// </summary>
        /// <param name="text">Text to use as a watermark.</param>
        /// <param name="fontColor">Watermark color. Can be specified as a string (e.g. "White") or as a hex value (e.g. "#00FF00").</param>
        /// <param name="fontSize">Font size in points.</param>
        /// <param name="fontStyle">Font style: bold, italics, etc.</param>
        /// <param name="fontFamily">Font family name: e.g. Microsoft Sans Serif</param>
        /// <param name="horizontalAlign">Horizontal alignment for watermark text. Can be "right", "left", or "center".</param>
        /// <param name="verticalAlign">Vertical alignment for watermark text. Can be "top", "bottom", or "middle".</param>
        /// <param name="opacity">Watermark text opacity. Should be between 0 and 100.</param>
        /// <param name="padding">Size of padding around watermark text in pixels.</param>
        /// <returns>Modified WebImage instance with added watermark.</returns>
        public WebImage AddTextWatermark(
            string text,
            string fontColor = "Black",
            int fontSize = 12,
            string fontStyle = "Regular",
            string fontFamily = "Microsoft Sans Serif",
            string horizontalAlign = "Right",
            string verticalAlign = "Bottom",
            int opacity = 100,
            int padding = 5)
        {
            if (String.IsNullOrEmpty(text))
            {
                throw new ArgumentException(CommonResources.Argument_Cannot_Be_Null_Or_Empty, "text");
            }

            Color color;
            if (!ConversionUtil.TryFromStringToColor(fontColor, out color))
            {
                throw new ArgumentException(HelpersResources.WebImage_IncorrectColorName);
            }

            if ((opacity < 0) || (opacity > 100))
            {
                throw new ArgumentOutOfRangeException("opacity", String.Format(CultureInfo.InvariantCulture, CommonResources.Argument_Must_Be_Between, 0, 100));
            }

            int alpha = 255 * opacity / 100;
            color = Color.FromArgb(alpha, color);

            if (fontSize <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    "fontSize",
                    String.Format(CultureInfo.InvariantCulture, CommonResources.Argument_Must_Be_GreaterThan, 0));
            }

            FontStyle fontStyleEnum;
            if (!ConversionUtil.TryFromStringToEnum(fontStyle, out fontStyleEnum))
            {
                throw new ArgumentException(HelpersResources.WebImage_IncorrectFontStyle);
            }

            FontFamily fontFamilyClass;
            if (!ConversionUtil.TryFromStringToFontFamily(fontFamily, out fontFamilyClass))
            {
                throw new ArgumentException(HelpersResources.WebImage_IncorrectFontFamily);
            }

            HorizontalAlign horizontalAlignEnum = ParseHorizontalAlign(horizontalAlign);
            VerticalAlign verticalAlignEnum = ParseVerticalAlign(verticalAlign);

            if (padding < 0)
            {
                throw new ArgumentOutOfRangeException(
                    "padding",
                    String.Format(CultureInfo.InvariantCulture, CommonResources.Argument_Must_Be_GreaterThanOrEqualTo, 0));
            }

            WatermarkTextTransformation transformation =
                new WatermarkTextTransformation(text, color, fontSize, fontStyleEnum, fontFamilyClass, horizontalAlignEnum, verticalAlignEnum, padding);
            _transformations.Add(transformation);
            return this;
        }

        /// <summary>
        /// Adds image watermark to an image.
        /// </summary>
        /// <param name="watermarkImage">Image to use as a watermark.</param>
        /// <param name="width">Width of watermark.</param>
        /// <param name="height">Height of watermark.</param>
        /// <param name="horizontalAlign">Horizontal alignment for watermark image. Can be "right", "left", or "center".</param>
        /// <param name="verticalAlign">Vertical alignment for watermark image. Can be "top", "bottom", or "middle".</param>
        /// <param name="opacity">Watermark text opacity. Should be between 0 and 100.</param>
        /// <param name="padding">Size of padding around watermark image in pixels.</param>
        /// <returns>Modified WebImage instance with added watermark.</returns>
        public WebImage AddImageWatermark(
            WebImage watermarkImage,
            int width = 0,
            int height = 0,
            string horizontalAlign = "Right",
            string verticalAlign = "Bottom",
            int opacity = 100,
            int padding = 5)
        {
            if (watermarkImage == null)
            {
                throw new ArgumentNullException("watermarkImage");
            }

            if (width < 0)
            {
                throw new ArgumentOutOfRangeException(
                    "width",
                    String.Format(CultureInfo.InvariantCulture, CommonResources.Argument_Must_Be_GreaterThanOrEqualTo, 0));
            }
            if (height < 0)
            {
                throw new ArgumentOutOfRangeException(
                    "height",
                    String.Format(CultureInfo.InvariantCulture, CommonResources.Argument_Must_Be_GreaterThanOrEqualTo, 0));
            }
            if (((width == 0) && (height > 0)) || ((width > 0) && (height == 0)))
            {
                throw new ArgumentException(HelpersResources.WebImage_IncorrectWidthAndHeight);
            }
            if ((opacity < 0) || (opacity > 100))
            {
                throw new ArgumentOutOfRangeException("opacity", String.Format(CultureInfo.InvariantCulture, CommonResources.Argument_Must_Be_Between, 0, 100));
            }

            HorizontalAlign horizontalAlignEnum = ParseHorizontalAlign(horizontalAlign);
            VerticalAlign verticalAlignEnum = ParseVerticalAlign(verticalAlign);

            if (padding < 0)
            {
                throw new ArgumentOutOfRangeException(
                    "padding",
                    String.Format(CultureInfo.InvariantCulture, CommonResources.Argument_Must_Be_GreaterThanOrEqualTo, 0));
            }

            WatermarkImageTransformation transformation =
                new WatermarkImageTransformation(watermarkImage.Clone(), width, height, horizontalAlignEnum, verticalAlignEnum, opacity, padding);
            _transformations.Add(transformation);
            return this;
        }

        /// <summary>
        /// Adds image watermark to an image.
        /// </summary>
        /// <param name="watermarkImageFilePath">File to read watermark image from.</param>
        /// <param name="width">Width of watermark.</param>
        /// <param name="height">Height of watermark.</param>
        /// <param name="horizontalAlign">Horizontal alignment for watermark image. Can be "right", "left", or "center".</param>
        /// <param name="verticalAlign">Vertical alignment for watermark image. Can be "top", "bottom", or "middle".</param>
        /// <param name="opacity">Watermark text opacity. Should be between 0 and 100.</param>
        /// <param name="padding">Size of padding around watermark image in pixels.</param>
        /// <returns>WebImage instance with added watermark.</returns>
        public WebImage AddImageWatermark(
            string watermarkImageFilePath,
            int width = 0,
            int height = 0,
            string horizontalAlign = "Right",
            string verticalAlign = "Bottom",
            int opacity = 100,
            int padding = 5)
        {
            return AddImageWatermark(new HttpContextWrapper(HttpContext.Current), _defaultReadAction, watermarkImageFilePath, width, height,
                                     horizontalAlign, verticalAlign, opacity, padding);
        }

        internal WebImage AddImageWatermark(
            HttpContextBase httpContext,
            Func<string, byte[]> readAction,
            string watermarkImageFilePath,
            int width,
            int height,
            string horizontalAlign,
            string verticalAlign,
            int opacity,
            int padding)
        {
            return AddImageWatermark(new WebImage(httpContext, readAction, watermarkImageFilePath), width, height, horizontalAlign, verticalAlign, opacity, padding);
        }

        [SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase",
            Justification = "No security decision is made based on this string. It's customary to display MIME format in lowercase.")]
        public WebImage Write(string requestedFormat = null)
        {
            // GetBytes takes care of executing pending transformations and 
            // determining current image format if we didn't have it set before.
            // todo: this could be made more efficient by avoiding cloning array 
            // when format is same
            requestedFormat = requestedFormat ?? _initialFormat.ToString();
            Debug.Assert(requestedFormat != null);
            byte[] content = GetBytes(requestedFormat);

            string requestedFormatWithPrefix;
            if (requestedFormat.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
            {
                requestedFormatWithPrefix = requestedFormat;
            }
            else
            {
                requestedFormatWithPrefix = "image/" + requestedFormat;
            }

            HttpResponse response = HttpContext.Current.Response;
            response.ContentType = requestedFormatWithPrefix;
            response.BinaryWrite(content);

            return this;
        }

        /// <param name="filePath">If no filePath is specified, the method falls back to the file name if the image was constructed from a file or 
        /// the file name on the client (the browser machine) if the image was built off GetImageFromRequest
        /// </param>
        /// <param name="imageFormat">The format the image is saved in</param>
        /// <param name="forceCorrectExtension">Appends a well known extension to the filePath based on the imageFormat specified. 
        /// If the filePath uses a valid extension, no change is made.
        /// e.g. format: "jpg", filePath: "foo.txt". Image saved at = "foo.txt.jpeg"
        ///      format: "png", filePath: "foo.png". Image saved at = "foo.txt.png"
        /// </param>
        [SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase",
            Justification = "File extensions are typically specified in lower case.")]
        public WebImage Save(string filePath = null, string imageFormat = null, bool forceCorrectExtension = true)
        {
            return Save(new HttpContextWrapper(HttpContext.Current), File.WriteAllBytes, filePath, imageFormat, forceCorrectExtension);
        }

        [SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase", Justification = "The string is a file extension which is typically lower case")]
        internal WebImage Save(HttpContextBase context, Action<string, byte[]> saveAction, string filePath, string imageFormat, bool forceWellKnownExtension)
        {
            filePath = filePath ?? FileName;
            if (String.IsNullOrEmpty(filePath))
            {
                throw new ArgumentNullException("filePath", CommonResources.Argument_Cannot_Be_Null_Or_Empty);
            }

            // GetBytes takes care of executing pending transformations.
            // todo: this could be made more efficient by avoiding cloning array 
            // when format is same
            byte[] content = GetBytes(imageFormat);
            if (forceWellKnownExtension)
            {
                ImageFormat saveImageFormat;
                ImageFormat requestedImageFormat = String.IsNullOrEmpty(imageFormat) ? _initialFormat : GetImageFormat(imageFormat);
                var extension = Path.GetExtension(filePath).TrimStart('.');
                // TryFromStringToImageFormat accepts mime types and image names. For images supported by System.Drawing.Imaging, the image name maps to the extension.
                // Replace the extension with the current format in the following two events:
                //  * The extension format cannot be converted to a known format
                //  * The format does not match. 
                if (!ConversionUtil.TryFromStringToImageFormat(extension, out saveImageFormat) || !saveImageFormat.Equals(requestedImageFormat))
                {
                    extension = requestedImageFormat.ToString().ToLowerInvariant();
                    filePath = filePath + "." + extension;
                }
            }
            saveAction(VirtualPathUtil.MapPath(context, filePath), content);
            // Update the FileName since it may have changed whilst saving.
            FileName = filePath;
            return this;
        }

        /// <summary>
        /// Constructs a System.Drawing.Image instance from the content which validates the contents of the image.
        /// </summary>
        /// <exception cref="System.ArgumentException">When an Image construction fails.</exception>
        private static ImageFormat ValidateImageContent(byte[] content, string paramName)
        {
            try
            {
                using (MemoryStream stream = new MemoryStream(content))
                {
                    using (Image image = Image.FromStream(stream, useEmbeddedColorManagement: false))
                    {
                        var rawFormat = image.RawFormat;
                        ImageFormat actualFormat;
                        // RawFormat returns a ImageFormat instance with the same Guid as the predefined types 
                        // This instance is not very useful when it comes to printing human readable strings and file extensions.
                        // Therefore, lookup the predefined instance 
                        if (!_imageFormatLookup.TryGetValue(rawFormat.Guid, out actualFormat))
                        {
                            actualFormat = rawFormat;
                        }
                        return actualFormat;
                    }
                }
            }
            catch (ArgumentException exception)
            {
                throw new ArgumentException(HelpersResources.WebImage_InvalidImageContents, paramName, exception);
            }
        }

        private static ImageFormat GetImageFormat(string format)
        {
            Debug.Assert(!String.IsNullOrEmpty(format), "format cannot be null");

            ImageFormat result;
            if (!ConversionUtil.TryFromStringToImageFormat(format, out result))
            {
                throw new ArgumentException(String.Format(CultureInfo.InvariantCulture, HelpersResources.Image_IncorrectImageFormat, format), "format");
            }

            return result;
        }

        private static HorizontalAlign ParseHorizontalAlign(string alignment)
        {
            bool conversionOk;
            HorizontalAlign horizontalAlign;
            conversionOk = ConversionUtil.TryFromStringToEnum(alignment, out horizontalAlign);
            if (!conversionOk || (horizontalAlign == HorizontalAlign.Justify) || (horizontalAlign == HorizontalAlign.NotSet))
            {
                throw new ArgumentException(HelpersResources.WebImage_IncorrectHorizontalAlignment);
            }
            return horizontalAlign;
        }

        private static VerticalAlign ParseVerticalAlign(string alignment)
        {
            bool conversionOk;
            VerticalAlign verticalAlign;
            conversionOk = ConversionUtil.TryFromStringToEnum(alignment, out verticalAlign);
            if (!conversionOk || (verticalAlign == VerticalAlign.NotSet))
            {
                throw new ArgumentException(HelpersResources.WebImage_IncorrectVerticalAlignment);
            }
            return verticalAlign;
        }

        private void GetContentFromImageAndUpdateFormat(Image image)
        {
            using (MemoryStream buffer = new MemoryStream())
            {
                if (image.RawFormat.Equals(Drawing.Imaging.ImageFormat.MemoryBmp))
                {
                    // Memory Bmps are an in-memory format and do not have encoders to save to disk / stream. 
                    // Save it in the current format whenever we encounter which ensures we preserve image information such as transparency.
                    image.Save(buffer, _currentFormat);
                }
                else
                {
                    // If the RawFormat has an encoder, save it as-is to prevent the cost of encoding it to another format such as the initial or current format. 
                    image.Save(buffer, image.RawFormat);
                    _currentFormat = image.RawFormat;
                }

                _content = buffer.ToArray();
            }
        }

        private void ApplyTransformationsAndSetProperties()
        {
            Debug.Assert(_content != null, "Incorrectly constructed instance.");

            MemoryStream stream = null;
            Image image = null;
            try
            {
                stream = new MemoryStream(_content);
                image = Image.FromStream(stream);

                if (_properties == null)
                {
                    // makes sure properties is never null after initialization
                    _properties = image.PropertyItems ?? new PropertyItem[0];
                }

                foreach (ImageTransformation trans in _transformations)
                {
                    Image tempImage = trans.ApplyTransformation(image);

                    // ApplyTransformation could return the same image if no transformations are made or if
                    // transformations are made on the image itself.
                    if (tempImage != image)
                    {
                        if (stream != null)
                        {
                            stream.Dispose();
                            stream = null;
                        }

                        Debug.Assert((image != null) && (tempImage != null), "Image instances should not be null.");
                        image.Dispose();
                        image = tempImage;
                    }

                    // This is just to keep FxCop happy. Otherwise it thinks that tempImage could be diposed twice.
                    tempImage = null;
                }

                // If there were any transformations we need to get new content. This will also update the current format to the RawFormat.
                if (_transformations.Any())
                {
                    GetContentFromImageAndUpdateFormat(image);
                    _transformations.Clear();
                }

                _height = image.Size.Height;
                _width = image.Size.Width;
            }
            finally
            {
                if (image != null)
                {
                    image.Dispose();
                }
                if (stream != null)
                {
                    stream.Dispose();
                }
            }
        }

        /// <remarks>Caller has to dispose of returned Bitmap object.</remarks>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope",
            Justification = "Callers of this method are responsible for disposing returned Bitmap")]
        private static Bitmap GetBitmapFromImage(Image image, int width, int height, bool preserveResolution = true)
        {
            bool indexed = (image.PixelFormat == PixelFormat.Format1bppIndexed ||
                            image.PixelFormat == PixelFormat.Format4bppIndexed ||
                            image.PixelFormat == PixelFormat.Format8bppIndexed ||
                            image.PixelFormat == PixelFormat.Indexed);

            Bitmap bitmap = indexed ? new Bitmap(width, height) : new Bitmap(width, height, image.PixelFormat);
            if (preserveResolution)
            {
                bitmap.SetResolution(image.HorizontalResolution, image.VerticalResolution);
            }
            else
            {
                bitmap.SetResolution(FixedResolution, FixedResolution);
            }

            using (Graphics graphic = Graphics.FromImage(bitmap))
            {
                if (indexed)
                {
                    graphic.FillRectangle(Brushes.White, 0, 0, width, height);
                }
                graphic.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphic.DrawImage(image, 0, 0, width, height);
            }

            return bitmap;
        }

        private static void CopyMetadata(PropertyItem[] properties, Image target)
        {
            foreach (PropertyItem property in properties)
            {
                try
                {
                    target.SetPropertyItem(property);
                }
                catch (ArgumentException)
                {
                    // just ignore it; on some configurations this fails
                }
            }
        }

        private class CropTransformation : ImageTransformation
        {
            public CropTransformation(int top, int right, int bottom, int left)
            {
                Top = top;
                Right = right;
                Bottom = bottom;
                Left = left;
            }

            public int Top { get; set; }
            public int Right { get; set; }
            public int Bottom { get; set; }
            public int Left { get; set; }

            public override Image ApplyTransformation(Image image)
            {
                if ((Top + Bottom > image.Height) || (Left + Right > image.Width))
                {
                    // If Crop arguments are too big (i.e. whole image is cropped) we don't make any changes. 
                    return image;
                }

                int width = image.Width - (Left + Right);
                int height = image.Height - (Top + Bottom);

                RectangleF rect = new RectangleF(Left, Top, width, height);

                // todo: check if we can guarantee that rect is inside the image at this point
                using (Bitmap bitmap = GetBitmapFromImage(image, image.Width, image.Height))
                {
                    try
                    {
                        return bitmap.Clone(rect, image.PixelFormat);
                    }
                    catch (OutOfMemoryException)
                    {
                        // Bitmap.Clone unfortunately throws OOM exception when rect is 
                        // outside of the source bitmap bounds
                        return image;
                    }
                }
            }
        }

        private abstract class ImageTransformation
        {
            public abstract Image ApplyTransformation(Image image);
        }

        private class ResizeTransformation : ImageTransformation
        {
            public ResizeTransformation(int height, int width, bool preserveAspectRatio, bool preventEnlarge)
            {
                Height = height;
                Width = width;
                PreserveAspectRatio = preserveAspectRatio;
                PreventEnlarge = preventEnlarge;
            }

            public int Height { get; set; }
            public int Width { get; set; }
            public bool PreserveAspectRatio { get; set; }
            public bool PreventEnlarge { get; set; }

            public override Image ApplyTransformation(Image image)
            {
                int height = Height;
                int width = Width;

                if (PreserveAspectRatio)
                {
                    double heightRatio = (height * 100.0) / image.Height;
                    double widthRatio = (width * 100.0) / image.Width;
                    if (heightRatio > widthRatio)
                    {
                        height = (int)Math.Round((widthRatio * image.Height) / 100);
                    }
                    else if (heightRatio < widthRatio)
                    {
                        width = (int)Math.Round((heightRatio * image.Width) / 100);
                    }
                }

                if (PreventEnlarge)
                {
                    if (height > image.Height)
                    {
                        height = image.Height;
                    }
                    if (width > image.Width)
                    {
                        width = image.Width;
                    }
                }

                if ((image.Height == height) && (image.Width == width))
                {
                    return image;
                }

                return GetBitmapFromImage(image, width, height);
            }
        }

        private class RotateTransformation : ImageTransformation
        {
            public RotateTransformation(RotateFlipType direction)
            {
                Direction = direction;
            }

            public RotateFlipType Direction { get; set; }

            public override Image ApplyTransformation(Image image)
            {
                image.RotateFlip(Direction);
                return image;
            }
        }

        private class WatermarkImageTransformation : WatermarkTransformation
        {
            public WatermarkImageTransformation(
                WebImage image,
                int width,
                int height,
                HorizontalAlign horizontalAlign,
                VerticalAlign verticalAlign,
                int opacity,
                int padding)
                : base(horizontalAlign, verticalAlign, padding)
            {
                WatermarkImage = image;
                Width = width;
                Height = height;
                Opacity = opacity;
            }

            public WebImage WatermarkImage { get; set; }
            public int Width { get; set; }
            public int Height { get; set; }
            public int Opacity { get; set; }

            public override Image ApplyTransformation(Image image)
            {
                // Use original image dimensions if user didn't specify any.
                if (Width == 0)
                {
                    Debug.Assert(Height == 0, "If one dimension is zero the other one must be too.");
                    Width = WatermarkImage.Width;
                    Height = WatermarkImage.Height;
                }

                if (((Padding * 2) + Width >= image.Width) || ((Padding * 2) + Height >= image.Height))
                {
                    // If watermark image + padding is too big we don't make any changes. 
                    return image;
                }

                WatermarkImage.Resize(Width, Height, preserveAspectRatio: false, preventEnlarge: false);
                float alphaScaling = ((float)Opacity) / 100;

                byte[] watermarkBuffer = WatermarkImage.GetBytes();
                Rectangle rect = GetRectangleInsideImage(image, Width, Height);

                using (Graphics targetGraphics = Graphics.FromImage(image))
                {
                    using (MemoryStream memStream = new MemoryStream(watermarkBuffer))
                    {
                        using (Image watermarkImage = Image.FromStream(memStream))
                        {
                            AddWatermark(targetGraphics, watermarkImage, rect, alphaScaling);
                        }
                    }
                }

                return image;
            }
        }

        private class WatermarkTextTransformation : WatermarkTransformation
        {
            public WatermarkTextTransformation(
                string text,
                Color fontColor,
                int fontSize,
                FontStyle fontStyle,
                FontFamily fontFamily,
                HorizontalAlign alignX,
                VerticalAlign alignY,
                int padding)
                : base(alignX, alignY, padding)
            {
                Text = text;
                FontColor = fontColor;
                FontSize = fontSize;
                FontStyle = fontStyle;
                FontFamily = fontFamily;
            }

            public string Text { get; set; }
            public Color FontColor { get; set; }
            public int FontSize { get; set; }
            public FontStyle FontStyle { get; set; }
            public FontFamily FontFamily { get; set; }

            public override Image ApplyTransformation(Image image)
            {
                if ((Padding * 2 >= image.Width) || (Padding * 2 >= image.Height))
                {
                    // If padding is too big we don't make any changes. 
                    return image;
                }

                // Get font size and text area that text fits into using fixed size resolution version of the image.
                // This is needed so we create the same size/position text watermark even when images have different
                // resolutions. Otherwise, watermark is slightly different even when text & size arguments are the same.
                int fontSize;
                SizeF textArea;
                using (Bitmap fixedResolutionImage = GetBitmapFromImage(image, image.Width, image.Height, preserveResolution: false))
                {
                    using (Graphics graphics = Graphics.FromImage(fixedResolutionImage))
                    {
                        fontSize = GetBestFontSize(image, graphics, out textArea);
                    }
                }

                int textWidth = (int)Math.Ceiling(textArea.Width);
                int textHeight = (int)Math.Ceiling(textArea.Height);
                Rectangle area = GetRectangleInsideImage(image, textWidth, textHeight);

                // Create new bitmap that only contains text in the right size.
                using (Bitmap textBitmap = new Bitmap(textWidth, textHeight))
                {
                    using (Graphics graphics = Graphics.FromImage(textBitmap))
                    {
                        using (Font font = new Font(FontFamily, fontSize, FontStyle))
                        {
                            using (Brush brushToUse = new SolidBrush(FontColor))
                            {
                                graphics.TextRenderingHint = TextRenderingHint.AntiAlias;
                                graphics.DrawString(Text, font, brushToUse, new PointF(0F, 0F));
                            }
                        }
                    }

                    // Use generated bitmap with text to apply as image watermark.
                    using (Graphics targetGraphics = Graphics.FromImage(image))
                    {
                        AddWatermark(targetGraphics, textBitmap, area, 1f);
                    }
                }

                return image;
            }

            private int GetBestFontSize(Image image, Graphics graphics, out SizeF textArea)
            {
                SizeF layoutArea = new SizeF(image.Width - (Padding * 2), image.Height - (Padding * 2));
                int bestFontSize = FontSize;
                textArea = layoutArea;

                using (StringFormat format = new StringFormat(StringFormatFlags.NoClip | StringFormatFlags.MeasureTrailingSpaces))
                {
                    for (int fontSize = FontSize; fontSize >= 2; fontSize--)
                    {
                        int numChars = 0, numLines = 0;
                        using (Font font = new Font(FontFamily, fontSize, FontStyle))
                        {
                            textArea = graphics.MeasureString(Text, font, layoutArea, format, out numChars, out numLines);
                        }

                        if ((numChars >= Text.Length) && (textArea.Width <= layoutArea.Width) && (textArea.Height <= layoutArea.Height))
                        {
                            // it fits! Exit now
                            return fontSize;
                        }
                        else
                        {
                            bestFontSize = fontSize;
                        }
                    }
                }
                return bestFontSize;
            }
        }

        private abstract class WatermarkTransformation : ImageTransformation
        {
            private static readonly float[][] _identityScalingMatrix =
                {
                    new float[] { 1, 0, 0, 0, 0 },
                    new float[] { 0, 1, 0, 0, 0 },
                    new float[] { 0, 0, 1, 0, 0 },
                    new float[] { 0, 0, 0, 1, 0 },
                    new float[] { 0, 0, 0, 0, 1 }
                };

            public WatermarkTransformation(HorizontalAlign alignX, VerticalAlign alignY, int padding)
            {
                HorizontalAlign = alignX;
                VerticalAlign = alignY;
                Padding = padding;
            }

            public HorizontalAlign HorizontalAlign { get; set; }
            public VerticalAlign VerticalAlign { get; set; }
            public int Padding { get; set; }

            public Rectangle GetRectangleInsideImage(Image image, int width, int height)
            {
                int posX, posY;

                switch (HorizontalAlign)
                {
                    case HorizontalAlign.Left:
                        posX = Padding;
                        break;
                    case HorizontalAlign.Right:
                        posX = image.Width - width - Padding;
                        break;
                    case HorizontalAlign.Center:
                    default:
                        posX = (image.Width - width) / 2;
                        break;
                }
                switch (VerticalAlign)
                {
                    case VerticalAlign.Top:
                        posY = Padding;
                        break;
                    case VerticalAlign.Bottom:
                        posY = image.Height - height - Padding;
                        break;
                    case VerticalAlign.Middle:
                    default:
                        posY = (image.Height - height) / 2;
                        break;
                }

                return new Rectangle(posX, posY, width, height);
            }

            private static float[][] GetScalingMatrix(float alphaScaling)
            {
                if (alphaScaling == 1)
                {
                    return _identityScalingMatrix;
                }

                float[][] scalingMatrix =
                    {
                        new float[] { 1, 0, 0, 0, 0 },
                        new float[] { 0, 1, 0, 0, 0 },
                        new float[] { 0, 0, 1, 0, 0 },
                        new[] { 0, 0, 0, alphaScaling, 0 },
                        new float[] { 0, 0, 0, 0, 1 }
                    };
                return scalingMatrix;
            }

            public static void AddWatermark(Graphics targetGraphics, Image watermark, Rectangle rect, float alphaScaling)
            {
                float[][] scalingMatrix = GetScalingMatrix(alphaScaling);
                ColorMatrix colorMatrix = new ColorMatrix(scalingMatrix);

                using (ImageAttributes imageAtt = new ImageAttributes())
                {
                    imageAtt.SetColorMatrix(colorMatrix, ColorMatrixFlag.Default, ColorAdjustType.Default);
                    targetGraphics.DrawImage(watermark, rect, 0, 0, watermark.Width, watermark.Height, GraphicsUnit.Pixel, imageAtt);
                }
            }
        }
    }
}
