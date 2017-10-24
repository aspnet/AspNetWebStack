// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.IO;
using Microsoft.TestCommon;

namespace System.Web.Mvc.Test
{
    public class TypeCacheUtilTest
    {
        [Fact]
        public void GetFilteredTypesFromAssemblies_FallThrough()
        {
            // Arrange
            Type[] expectedTypes = new Type[]
            {
                typeof(TypeCacheValidFoo),
                typeof(TypeCacheValidBar)
            };

            string cacheName = "testCache";
            MockBuildManager buildManager = new MockBuildManager();
            Predicate<Type> predicate = type => type.IsDefined(typeof(TypeCacheMarkerAttribute), true);

            // Act
            List<Type> returnedTypes = TypeCacheUtil.GetFilteredTypesFromAssemblies(cacheName, predicate, buildManager);

            // Assert
            Assert.Equal(expectedTypes, returnedTypes.ToArray());

            MemoryStream cachedStream = buildManager.CachedFileStore[cacheName] as MemoryStream;
            Assert.NotNull(cachedStream);
            Assert.NotEmpty(cachedStream.ToArray());
        }

        [Fact]
        public void SaveToCache_ReadFromCache_ReturnsNullIfTypesAreInvalid()
        {
            // SAVING

            // Arrange
            Type[] expectedTypes = new Type[]
            {
                typeof(object),
                typeof(string)
            };

            TypeCacheSerializer serializer = new TypeCacheSerializer();
            string cacheName = "testCache";
            MockBuildManager buildManager = new MockBuildManager();

            // Act
            TypeCacheUtil.SaveTypesToCache(cacheName, expectedTypes, buildManager, serializer);

            // Assert
            MemoryStream writeStream = buildManager.CachedFileStore[cacheName] as MemoryStream;
            Assert.NotNull(writeStream);

            byte[] streamContents = writeStream.ToArray();
            Assert.NotEmpty(streamContents);

            // READING

            // Arrange
            MemoryStream readStream = new MemoryStream(streamContents);
            buildManager.CachedFileStore[cacheName] = readStream;

            // Act
            List<Type> returnedTypes = TypeCacheUtil.ReadTypesFromCache(cacheName, _ => false /* all types are invalid */, buildManager, serializer);

            // Assert
            Assert.Null(returnedTypes);
        }

        [Fact]
        public void SaveToCache_ReadFromCache_Success()
        {
            // SAVING

            // Arrange
            Type[] expectedTypes = new Type[]
            {
                typeof(object),
                typeof(string)
            };

            TypeCacheSerializer serializer = new TypeCacheSerializer();
            string cacheName = "testCache";
            MockBuildManager buildManager = new MockBuildManager();

            // Act
            TypeCacheUtil.SaveTypesToCache(cacheName, expectedTypes, buildManager, serializer);

            // Assert
            MemoryStream writeStream = buildManager.CachedFileStore[cacheName] as MemoryStream;
            Assert.NotNull(writeStream);

            byte[] streamContents = writeStream.ToArray();
            Assert.NotEmpty(streamContents);

            // READING

            // Arrange
            MemoryStream readStream = new MemoryStream(streamContents);
            buildManager.CachedFileStore[cacheName] = readStream;

            // Act
            List<Type> returnedTypes = TypeCacheUtil.ReadTypesFromCache(cacheName, _ => true /* all types are valid */, buildManager, serializer);

            // Assert
            Assert.Equal(expectedTypes, returnedTypes.ToArray());
        }
    }

    public class TypeCacheMarkerAttribute : Attribute
    {
    }

    [TypeCacheMarker]
    public class TypeCacheValidFoo
    {
    }

    [TypeCacheMarker]
    public class TypeCacheValidBar
    {
    }

    [TypeCacheMarker]
    internal class TypeCacheInvalidInternal
    {
    }

    [TypeCacheMarker]
    public abstract class TypeCacheInvalidAbstract
    {
    }

    [TypeCacheMarker]
    public struct TypeCacheInvalidStruct
    {
    }
}
