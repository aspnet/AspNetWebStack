// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Text;
using Microsoft.TestCommon;

namespace System.Web.Mvc.Test
{
    public class DescriptorUtilTest
    {
        [Fact]
        public void AppendUniqueId_FromIUniquelyIdentifiable()
        {
            // Arrange
            CustomUniquelyIdentifiable custom = new CustomUniquelyIdentifiable("hello-world");
            StringBuilder builder = new StringBuilder();

            // Act
            DescriptorUtil.AppendUniqueId(builder, custom);

            // Assert
            Assert.Equal("[11]hello-world", builder.ToString());
        }

        [Fact]
        public void AppendUniqueId_FromMemberInfo()
        {
            // Arrange
            string moduleVersionId = typeof(DescriptorUtilTest).Module.ModuleVersionId.ToString();
            string metadataToken = typeof(DescriptorUtilTest).MetadataToken.ToString();
            string expected = String.Format("[{0}]{1}[{2}]{3}", moduleVersionId.Length, moduleVersionId, metadataToken.Length, metadataToken);
            StringBuilder builder = new StringBuilder();

            // Act
            DescriptorUtil.AppendUniqueId(builder, typeof(DescriptorUtilTest));

            // Assert
            Assert.Equal(expected, builder.ToString());
        }

        [Fact]
        public void CreateUniqueId_FromSimpleTypes()
        {
            // Act
            string retVal = DescriptorUtil.CreateUniqueId("foo", null, 12345);

            // Assert
            Assert.Equal("[3]foo[-1][5]12345", retVal);
        }

        private sealed class CustomUniquelyIdentifiable : IUniquelyIdentifiable
        {
            public CustomUniquelyIdentifiable(string uniqueId)
            {
                UniqueId = uniqueId;
            }

            public string UniqueId { get; private set; }
        }
    }
}
