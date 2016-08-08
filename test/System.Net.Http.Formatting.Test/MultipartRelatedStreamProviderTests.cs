// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net.Http.Headers;
using Microsoft.TestCommon;

namespace System.Net.Http
{
    public class MultipartRelatedStreamProviderTests : MultipartStreamProviderTestBase<MultipartRelatedStreamProvider>
    {
        private const string ContentID = "12345";
        private const string Boundary = "-A-";

        private const string DefaultRootContent = "Default root content";
        private const string ContentIDRootContent = "Content with matching Content-ID";
        private const string OtherContent = "Other Content";

        public static TheoryDataSet<string, bool> MultipartRelatedWithStartParameter
        {
            get
            {
                return new TheoryDataSet<string, bool>
                {
                    { String.Format("multipart/related; boundary={0}; start=\"{1}\"", Boundary, ContentID), true },
                    { String.Format("multipart/related; start={0}; boundary={1}", ContentID, Boundary), true },
                };
            }
        }

        public static TheoryDataSet<string, bool> MultipartWithMissingOrInvalidStartParameter
        {
            get
            {
                return new TheoryDataSet<string, bool>
                {
                    { String.Format("multipart/form-data; start=\"{0}\"; boundary={1}", ContentID, Boundary), false },
                    { String.Format("multipart/form-data; start={0}; boundary={1}", ContentID, Boundary), false },
                    { String.Format("multipart/form-data; boundary={0}", Boundary), false },
                    { String.Format("multipart/related; boundary={0}", Boundary), false },
                    { String.Format("multipart/mixed; start={0}; boundary={1}", ContentID, Boundary), false },
                    { String.Format("multipart/mixed; boundary={1}", ContentID, Boundary), false },
                };
            }
        }

        [Fact]
        public void RootContent_ReturnsNull()
        {
            MultipartRelatedStreamProvider provider = new MultipartRelatedStreamProvider();
            Assert.Null(provider.RootContent);
        }

        [Theory]
        [PropertyData("MultipartRelatedWithStartParameter")]
        public void RootContent_ReturnsNullIfContentIDIsNotMatched(string mediaType, bool hasStartParameter)
        {
            // Arrange
            MultipartContent content = new MultipartContent("related", Boundary);
            content.Headers.ContentType = MediaTypeHeaderValue.Parse(mediaType);

            content.Add(new StringContent(DefaultRootContent));
            content.Add(new StringContent(OtherContent));

            HttpContent expectedRootContent = new StringContent(ContentIDRootContent);
            expectedRootContent.Headers.Add("Content-ID", "NoMatch");
            content.Add(expectedRootContent);

            MultipartRelatedStreamProvider provider = content.ReadAsMultipartAsync(new MultipartRelatedStreamProvider()).Result;

            // Act
            HttpContent actualRootContent = provider.RootContent;

            // Assert
            Assert.Null(actualRootContent);
        }

        [Theory]
        [PropertyData("MultipartRelatedWithStartParameter")]
        [PropertyData("MultipartWithMissingOrInvalidStartParameter")]
        public void RootContent_PicksContent(string mediaType, bool hasStartParameter)
        {
            // Arrange
            MultipartContent content = new MultipartContent("related", Boundary);
            content.Headers.ContentType = MediaTypeHeaderValue.Parse(mediaType);

            content.Add(new StringContent(DefaultRootContent));
            content.Add(new StringContent(OtherContent));

            HttpContent contentIDContent = new StringContent(ContentIDRootContent);
            contentIDContent.Headers.Add("Content-ID", ContentID);
            content.Add(contentIDContent);

            MultipartRelatedStreamProvider provider = content.ReadAsMultipartAsync(new MultipartRelatedStreamProvider()).Result;

            // Act
            HttpContent actualRootContent = provider.RootContent;
            string result = actualRootContent.ReadAsStringAsync().Result;

            // Assert
            if (hasStartParameter)
            {
                Assert.Equal(ContentIDRootContent, result);
            }
            else
            {
                Assert.Equal(DefaultRootContent, result);
            }
        }
    }
}
