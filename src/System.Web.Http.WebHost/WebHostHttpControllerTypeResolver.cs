// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Web.Compilation;
using System.Web.Http.Dispatcher;

namespace System.Web.Http.WebHost
{
    /// <summary>
    /// Wraps ASP build manager
    /// </summary>
    internal sealed class WebHostHttpControllerTypeResolver : DefaultHttpControllerTypeResolver
    {
        private const string TypeCacheName = "MS-ApiControllerTypeCache.xml";

        /// <summary>
        /// Returns a list of controllers available for the application.
        /// </summary>
        /// <returns>An <see cref="ICollection{Type}" /> of controllers.</returns>
        public override ICollection<Type> GetControllerTypes(IAssembliesResolver assembliesResolver)
        {
            HttpControllerTypeCacheSerializer serializer = new HttpControllerTypeCacheSerializer();

            // First, try reading from the cache on disk
            List<Type> matchingTypes = ReadTypesFromCache(TypeCacheName, IsControllerTypePredicate, serializer);
            if (matchingTypes != null)
            {
                return matchingTypes;
            }

            // If reading from the cache failed, enumerate over every assembly looking for a matching type
            matchingTypes = base.GetControllerTypes(assembliesResolver).ToList();

            // Finally, save the cache back to disk
            SaveTypesToCache(TypeCacheName, matchingTypes, serializer);

            return matchingTypes;
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Cache failures are not fatal, and the code should continue executing normally.")]
        private static List<Type> ReadTypesFromCache(string cacheName, Predicate<Type> predicate, HttpControllerTypeCacheSerializer serializer)
        {
            try
            {
                Stream stream = BuildManager.ReadCachedFile(cacheName);
                if (stream != null)
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        ICollection<Type> deserializedTypes = serializer.DeserializeTypes(reader);
                        if (deserializedTypes != null && deserializedTypes.All(type => predicate(type)))
                        {
                            // If all read types still match the predicate, success!
                            return deserializedTypes.ToList();
                        }
                    }
                }
            }
            catch
            {
                // cache failures are not considered fatal -- keep running.
            }

            return null;
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Cache failures are not fatal, and the code should continue executing normally.")]
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Disposed later")]
        private static void SaveTypesToCache(string cacheName, IEnumerable<Type> matchingTypes, HttpControllerTypeCacheSerializer serializer)
        {
            try
            {
                Stream stream = BuildManager.CreateCachedFile(cacheName);
                if (stream != null)
                {
                    using (StreamWriter writer = new StreamWriter(stream))
                    {
                        serializer.SerializeTypes(matchingTypes, writer);
                    }
                }
            }
            catch
            {
                // cache failures are not considered fatal -- keep running.
            }
        }
    }
}
