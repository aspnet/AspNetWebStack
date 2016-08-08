// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using System.Reflection;
using System.Web.Mvc.Properties;

namespace System.Web.Mvc.Html
{
    public static class EnumHelper
    {
        /// <summary>
        /// Gets a value indicating whether the given <paramref name="type"/> or an expression of this
        /// <see cref="Type"/> is suitable for use in <see cref="GetSelectList(Type)"/> and <see
        /// cref="SelectExtensions.EnumDropDownListFor{TModel,TEnum}(HtmlHelper{TModel}, Expression{Func{TModel, TEnum}})"/>
        /// calls.
        /// </summary>
        /// <param name="type">The <see cref="Type"/> to check.</param>
        /// <returns>
        /// <see langword="true"/> if <see cref="GetSelectList(Type)"/> will not throw when passed given
        /// <see cref="Type"/> and <see
        /// cref="SelectExtensions.EnumDropDownListFor{TModel,TEnum}(HtmlHelper{TModel}, Expression{Func{TModel, TEnum}})"/>
        /// will not throw when passed an expression of this <see cref="Type"/>; <see langword="false"/> otherwise.
        /// </returns>
        /// <remarks>
        /// Currently returns <see langref="true"/> if the <paramref name="type"/> parameter is
        /// non-<see langref="null"/>, is an <see langref="enum"/> type, and does not have a
        /// <see cref="FlagsAttribute"/> attribute.
        /// </remarks>
        public static bool IsValidForEnumHelper(Type type)
        {
            bool isValid = false;
            if (type != null)
            {
                // Type.IsEnum is false for Nullable<T> even if T is an enum.  Check underlying type (if any).
                // Do not support Enum type itself -- IsEnum property is false for that class.
                Type checkedType = Nullable.GetUnderlyingType(type) ?? type;
                if (checkedType.IsEnum)
                {
                    isValid = !HasFlagsInternal(checkedType);
                }
            }

            return isValid;
        }

        /// <summary>
        /// Gets a value indicating whether the given <paramref name="metadata"/> or associated expression is suitable
        /// for use in <see cref="GetSelectList(ModelMetadata)"/> and <see
        /// cref="SelectExtensions.EnumDropDownListFor{TModel,TEnum}(HtmlHelper{TModel}, Expression{Func{TModel, TEnum}})"/>
        /// calls.
        /// </summary>
        /// <param name="metadata">The <see cref="ModelMetadata"/> to check.</param>
        /// <returns>
        /// <see langword="true"/> if <see cref="GetSelectList(ModelMetadata)"/> will return not throw when passed
        /// given <see cref="ModelMetadata"/> and <see
        /// cref="SelectExtensions.EnumDropDownListFor{TModel,TEnum}(HtmlHelper{TModel}, Expression{Func{TModel, TEnum}})"/>
        /// will not throw when passed associated expression; <see langword="false"/> otherwise.
        /// </returns>
        /// <remarks>
        /// Currently returns <see langref="true"/> if the <paramref name="metadata"/> parameter is
        /// non-<see langref="null"/> and <see cref="IsValidForEnumHelper(Type)"/> returns <see langref="true"/> for
        /// <c>metadata.ModelType</c>.
        /// </remarks>
        public static bool IsValidForEnumHelper(ModelMetadata metadata)
        {
            return metadata != null && IsValidForEnumHelper(metadata.ModelType);
        }

        /// <summary>
        /// Gets a list of <see cref="SelectListItem"/> objects corresponding to enum constants defined in the given
        /// <paramref name="type"/>.
        /// </summary>
        /// <param name="type">The <see cref="Type"/> to evaluate.</param>
        /// <returns> An <see cref="IList{SelectListItem}"/> for the given <paramref name="type"/>.</returns>
        /// <remarks>
        /// Throws if <see cref="IsValidForEnumHelper(Type)"/> returns <see langref="false"/> for the given
        /// <paramref name="type"/>.
        /// </remarks>
        public static IList<SelectListItem> GetSelectList(Type type)
        {
            if (type == null)
            {
                throw Error.ArgumentNull("type");
            }

            if (!IsValidForEnumHelper(type))
            {
                throw Error.Argument("type", MvcResources.EnumHelper_InvalidParameterType, type.FullName);
            }

            IList<SelectListItem> selectList = new List<SelectListItem>();

            // According to HTML5: "The first child option element of a select element with a required attribute and
            // without a multiple attribute, and whose size is "1", must have either an empty value attribute, or must
            // have no text content."  SelectExtensions.DropDownList[For]() methods often generate a matching
            // <select/>.  Empty value for Nullable<T>, empty text for round-tripping an unrecognized value, or option
            // label serves in some cases.  But otherwise, ignoring this does not cause problems in either IE or Chrome.
            Type checkedType = Nullable.GetUnderlyingType(type) ?? type;
            if (checkedType != type)
            {
                // Underlying type was non-null so handle Nullable<T>; ensure returned list has a spot for null
                selectList.Add(new SelectListItem { Text = String.Empty, Value = String.Empty, });
            }

            // Populate the list
            const BindingFlags BindingFlags =
                BindingFlags.DeclaredOnly | BindingFlags.GetField | BindingFlags.Public | BindingFlags.Static;
            foreach (FieldInfo field in checkedType.GetFields(BindingFlags))
            {
                // fieldValue will be an numeric type (byte, ...)
                object fieldValue = field.GetRawConstantValue();

                selectList.Add(new SelectListItem { Text = GetDisplayName(field), Value = fieldValue.ToString(), });
            }

            return selectList;
        }

        /// <summary>
        /// Gets a list of <see cref="SelectListItem"/> objects corresponding to enum constants defined in the given
        /// <paramref name="metadata"/>.
        /// </summary>
        /// <param name="metadata">The <see cref="ModelMetadata"/> to evaluate.</param>
        /// <returns> An <see cref="IList{SelectListItem}"/> for the given <paramref name="metadata"/>.</returns>
        /// <remarks>
        /// Throws if <see cref="IsValidForEnumHelper(ModelMetadata)"/> returns <see langref="false"/> for the given
        /// <paramref name="metadata"/>.
        /// </remarks>
        public static IList<SelectListItem> GetSelectList(ModelMetadata metadata)
        {
            if (metadata == null)
            {
                throw Error.ArgumentNull("metadata");
            }

            if (metadata.ModelType == null)
            {
                throw Error.Argument("metadata", MvcResources.EnumHelper_InvalidMetadataParameter);
            }

            if (!IsValidForEnumHelper(metadata))
            {
                throw Error.Argument("metadata", MvcResources.EnumHelper_InvalidParameterType,
                    metadata.ModelType.FullName);
            }

            return GetSelectList(metadata.ModelType);
        }

        /// <summary>
        /// Gets a list of <see cref="SelectListItem"/> objects corresponding to enum constants defined in the given
        /// <paramref name="type"/>.  Also ensures the <paramref name="value"/> will round-trip even if it does not
        /// match a defined constant and sets the <c>Selected</c> property to <see langref="true"/> for one element in
        /// the returned list -- matching the <paramref name="value"/>.
        /// </summary>
        /// <param name="type">The <see cref="Type"/> to evaluate.</param>
        /// <param name="value">Value from <see cref="Type"/> <paramref name="type"/> to select.</param>
        /// <returns>
        /// An <see cref="IList{SelectListItem}"/> for the given <paramref name="type"/>, possibly extended to
        /// include an unrecognized <paramref name="value"/>.
        /// </returns>
        /// <remarks>
        /// Throws if <see cref="IsValidForEnumHelper(Type)"/> returns <see langref="false"/> for the given
        /// <paramref name="type"/> or if a non-null <paramref name="value"/> has a different <see cref="Type"/> than
        /// <paramref name="type"/>.
        /// </remarks>
        public static IList<SelectListItem> GetSelectList(Type type, Enum value)
        {
            IList<SelectListItem> selectList = GetSelectList(type);

            Type valueType = (value == null) ? null : value.GetType();
            if (valueType != null && valueType != type && valueType != Nullable.GetUnderlyingType(type))
            {
                throw Error.Argument("value", MvcResources.EnumHelper_InvalidValueParameter, valueType.FullName,
                    type.FullName);
            }

            if (value == null && selectList.Count != 0 && String.IsNullOrEmpty(selectList[0].Value))
            {
                // Type is Nullable<T>; use existing entry to round trip null value
                selectList[0].Selected = true;
            }
            else
            {
                // If null, use default for this (non-Nullable<T>) enum -- always has 0 integral value
                string valueString = (value == null) ? "0" : value.ToString("d");

                // Select the last matching item, imitating what at least IE and Chrome highlight when multiple
                // elements in a <select/> element have a selected attribute.
                bool foundSelected = false;
                for (int i = selectList.Count - 1; !foundSelected && i >= 0; --i)
                {
                    SelectListItem item = selectList[i];
                    item.Selected = (valueString == item.Value);
                    foundSelected |= item.Selected;
                }

                // Round trip the current value
                if (!foundSelected)
                {
                    if (selectList.Count != 0 && String.IsNullOrEmpty(selectList[0].Value))
                    {
                        // Type is Nullable<T>; use existing entry for round trip
                        selectList[0].Selected = true;
                        selectList[0].Value = valueString;
                    }
                    else
                    {
                        // Add new entry which does not display value to user
                        selectList.Insert(0,
                            new SelectListItem { Selected = true, Text = String.Empty, Value = valueString, });
                    }
                }
            }

            return selectList;
        }

        /// <summary>
        /// Gets a list of <see cref="SelectListItem"/> objects corresponding to enum constants defined in the given
        /// <paramref name="metadata"/>.  Also ensures the <paramref name="value"/> will round-trip even if it does not
        /// match a defined constant and sets the <c>Selected</c> property to <see langref="true"/> for one element in
        /// the returned list -- matching the <paramref name="value"/>.
        /// </summary>
        /// <param name="metadata">The <see cref="ModelMetadata"/> to evaluate.</param>
        /// <param name="value">Value from <see cref="Type"/> of <paramref name="metadata"/> to select.</param>
        /// <returns>
        /// An <see cref="IList{SelectListItem}"/> for the given <paramref name="metadata"/>, possibly extended to
        /// include an unrecognized <paramref name="value"/>.
        /// </returns>
        /// <remarks>
        /// Throws if <see cref="IsValidForEnumHelper(ModelMetadata)"/> returns <see langref="false"/> for the given
        /// <paramref name="metadata"/> or if a non-null <paramref name="value"/> has a different <see cref="Type"/>
        /// than <c>metadata.ModelType</c>.
        /// </remarks>
        public static IList<SelectListItem> GetSelectList(ModelMetadata metadata, Enum value)
        {
            if (metadata == null)
            {
                throw Error.ArgumentNull("metadata");
            }

            if (metadata.ModelType == null)
            {
                throw Error.Argument("metadata", MvcResources.EnumHelper_InvalidMetadataParameter);
            }

            if (!IsValidForEnumHelper(metadata))
            {
                throw Error.Argument("metadata", MvcResources.EnumHelper_InvalidParameterType,
                    metadata.ModelType.FullName);
            }

            return GetSelectList(metadata.ModelType, value);
        }

        internal static bool HasFlags(Type type)
        {
            Contract.Assert(type != null);

            Type checkedType = Nullable.GetUnderlyingType(type) ?? type;
            return HasFlagsInternal(checkedType);
        }

        private static bool HasFlagsInternal(Type type)
        {
            Contract.Assert(type != null);

            FlagsAttribute attribute = type.GetCustomAttribute<FlagsAttribute>(inherit: false);
            return attribute != null;
        }

        // Return non-empty name specified in a [Display] attribute for the given field, if any; field's name otherwise
        private static string GetDisplayName(FieldInfo field)
        {
            DisplayAttribute display = field.GetCustomAttribute<DisplayAttribute>(inherit: false);
            if (display != null)
            {
                string name = display.GetName();
                if (!String.IsNullOrEmpty(name))
                {
                    return name;
                }
            }

            return field.Name;
        }
    }
}
