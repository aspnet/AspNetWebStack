// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Imaging;
using System.Reflection;

namespace System.Web.Helpers
{
    internal static class ConversionUtil
    {
        private static MethodInfo _stringToEnumMethod;

        internal static string ToString<T>(T obj)
        {
            Type type = typeof(T);
            if (type.IsEnum)
            {
                return obj.ToString();
            }
            TypeConverter converter = TypeDescriptor.GetConverter(type);
            if ((converter != null) && (converter.CanConvertTo(typeof(string))))
            {
                return converter.ConvertToInvariantString(obj);
            }
            return null;
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes",
            Justification = "TypeConverter throws System.Exception instead of a more specific one.")]
        internal static bool TryFromString(Type type, string value, out object result)
        {
            result = null;
            if (type == typeof(string))
            {
                result = value;
                return true;
            }
            if (type.IsEnum)
            {
                return TryFromStringToEnumHelper(type, value, out result);
            }
            if (type == typeof(Color))
            {
                Color color;
                bool rval = TryFromStringToColor(value, out color);
                result = color;
                return rval;
            }
            // TypeConverter doesn't really have TryConvert APIs.  We should avoid TypeConverter.IsValid
            // which performs a duplicate conversion, and just handle the general exception ourselves.
            TypeConverter converter = TypeDescriptor.GetConverter(type);
            if ((converter != null) && converter.CanConvertFrom(typeof(string)))
            {
                try
                {
                    result = converter.ConvertFromInvariantString(value);
                    return true;
                }
                catch
                {
                    // Do nothing
                }
            }
            return false;
        }

        internal static bool TryFromStringToEnum<T>(string value, out T result) where T : struct
        {
            return Enum.TryParse(value, ignoreCase: true, result: out result);
        }

        private static bool TryFromStringToEnumHelper(Type enumType, string value, out object result)
        {
            result = null;
            if (_stringToEnumMethod == null)
            {
                _stringToEnumMethod = typeof(ConversionUtil).GetMethod("TryFromStringToEnum",
                                                                       BindingFlags.Static | BindingFlags.NonPublic);
                Debug.Assert(_stringToEnumMethod != null);
            }
            var args = new object[] { value, null };
            var rval = (bool)_stringToEnumMethod.MakeGenericMethod(enumType).Invoke(null, args);
            result = args[1];
            return rval;
        }

        internal static bool TryFromStringToFontFamily(string fontFamily, out FontFamily result)
        {
            result = null;
            bool converted = false;
            foreach (FontFamily fontFamilyTemp in FontFamily.Families)
            {
                if (fontFamily.Equals(fontFamilyTemp.Name, StringComparison.OrdinalIgnoreCase))
                {
                    result = fontFamilyTemp;
                    converted = true;
                    break;
                }
            }
            return converted;
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes",
            Justification = "TypeConverter throws System.Exception instad of a more specific one.")]
        internal static bool TryFromStringToColor(string value, out Color result)
        {
            result = default(Color);

            // Parse color specified as hex number
            if (value.StartsWith("#", StringComparison.OrdinalIgnoreCase))
            {
                // Only allow colors in form of #RRGGBB or #RGB
                if ((value.Length != 7) && (value.Length != 4))
                {
                    return false;
                }

                // Expand short version
                if (value.Length == 4)
                {
                    char[] newValue = new char[7];
                    newValue[0] = '#';
                    newValue[1] = newValue[2] = value[1];
                    newValue[3] = newValue[4] = value[2];
                    newValue[5] = newValue[6] = value[3];
                    value = new string(newValue);
                }
            }

            TypeConverter converter = TypeDescriptor.GetConverter(typeof(Color));
            Debug.Assert((converter != null) && (converter.CanConvertFrom(typeof(string))));

            // There are no TryConvert APIs on TypeConverter so we have to catch exception. 
            // In addition to that, invalid conversion just throws System.Exception with misleading message,
            // instead of a more specific exception type. 
            try
            {
                result = (Color)converter.ConvertFromInvariantString(value);
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        [SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase",
            Justification = "Format names are used in Http headers and are usually specified in lower case")]
        internal static string NormalizeImageFormat(string value)
        {
            value = value.ToLowerInvariant();
            switch (value)
            {
                case "jpeg":
                case "jpg":
                case "pjpeg":
                    return "jpeg";

                case "png":
                case "x-png":
                    return "png";

                case "icon":
                case "ico":
                case "x-icon":
                    return "icon";
            }
            return value;
        }

        internal static bool TryFromStringToImageFormat(string value, out ImageFormat result)
        {
            result = default(ImageFormat);

            if (String.IsNullOrEmpty(value))
            {
                return false;
            }
            if (value.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
            {
                value = value.Substring("image/".Length);
            }
            value = NormalizeImageFormat(value);

            TypeConverter converter = TypeDescriptor.GetConverter(typeof(ImageFormat));
            Debug.Assert((converter != null) && (converter.CanConvertFrom(typeof(string))));

            try
            {
                result = (ImageFormat)converter.ConvertFromInvariantString(value);
            }
            catch (NotSupportedException)
            {
                return false;
            }

            return true;
        }
    }
}
