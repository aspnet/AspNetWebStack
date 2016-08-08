// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Threading;

namespace System.Web.Mvc
{
    internal static class DescriptorUtil
    {
        private static void AppendPartToUniqueIdBuilder(StringBuilder builder, object part)
        {
            if (part == null)
            {
                builder.Append("[-1]");
            }
            else
            {
                string partString = Convert.ToString(part, CultureInfo.InvariantCulture);
                builder.AppendFormat("[{0}]{1}", partString.Length, partString);
            }
        }

        public static void AppendUniqueId(StringBuilder builder, object part)
        {
            // We can special-case certain part types
            MemberInfo memberInfo = part as MemberInfo;
            if (memberInfo != null)
            {
                AppendPartToUniqueIdBuilder(builder, memberInfo.Module.ModuleVersionId);
                AppendPartToUniqueIdBuilder(builder, memberInfo.MetadataToken);
                return;
            }

            IUniquelyIdentifiable uniquelyIdentifiable = part as IUniquelyIdentifiable;
            if (uniquelyIdentifiable != null)
            {
                AppendPartToUniqueIdBuilder(builder, uniquelyIdentifiable.UniqueId);
                return;
            }

            AppendPartToUniqueIdBuilder(builder, part);
        }

        public static string CreateUniqueId(object part0, object part1)
        {
            StringBuilder builder = new StringBuilder();
            AppendUniqueId(builder, part0);
            AppendUniqueId(builder, part1);
            return builder.ToString();
        }

        public static string CreateUniqueId(object part0, object part1, object part2)
        {
            StringBuilder builder = new StringBuilder();
            AppendUniqueId(builder, part0);
            AppendUniqueId(builder, part1);
            AppendUniqueId(builder, part2);
            return builder.ToString();
        }

        public static TDescriptor[] LazilyFetchOrCreateDescriptors<TReflection, TDescriptor, TArgument>(
            ref TDescriptor[] cacheLocation,
            Func<TArgument, TReflection[]> initializer,
            Func<TReflection, TArgument, TDescriptor> converter,
            TArgument state)
        {
            // did we already calculate this once?
            TDescriptor[] existingCache = Interlocked.CompareExchange(ref cacheLocation, null, null);
            if (existingCache != null)
            {
                return existingCache;
            }

            // Note: since this code operates on arrays it is more efficient to call simple array operations
            // instead of LINQ-y extension methods such as Select and Where. DO NOT attempt to simplify this
            // without testing the performance impact.
            TReflection[] memberInfos = initializer(state);
            List<TDescriptor> descriptorsList = new List<TDescriptor>(memberInfos.Length);
            for (int i = 0; i < memberInfos.Length; i++)
            {
                TDescriptor descriptor = converter(memberInfos[i], state);
                if (descriptor != null)
                {
                    descriptorsList.Add(descriptor);
                }
            }
            TDescriptor[] descriptors = descriptorsList.ToArray();

            TDescriptor[] updatedCache = Interlocked.CompareExchange(ref cacheLocation, descriptors, null);
            return updatedCache ?? descriptors;
        }
    }
}
