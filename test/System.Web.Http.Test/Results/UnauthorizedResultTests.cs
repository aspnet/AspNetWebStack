// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.TestCommon;

namespace System.Web.Http.Results
{
    public class UnauthorizedResultTests
    {
        [Fact]
        public void Constructor_Throws_WhenChallengesIsNull()
        {
            // Arrange
            IEnumerable<AuthenticationHeaderValue> challenges = null;
            HttpRequestMessage request = CreateRequest();

            // Act & Assert
            Assert.ThrowsArgumentNull(() => { CreateProductUnderTest(challenges, request); }, "challenges");
        }

        [Fact]
        public void Constructor_Throws_WhenRequestIsNull()
        {
            // Arrange
            IEnumerable<AuthenticationHeaderValue> challenges = CreateChallenges();
            HttpRequestMessage request = null;

            // Act & Assert
            Assert.ThrowsArgumentNull(() => { CreateProductUnderTest(challenges, request); }, "request");
        }

        [Fact]
        public void Challenges_Returns_InstanceProvided()
        {
            // Arrange
            IEnumerable<AuthenticationHeaderValue> expectedChallenges = CreateChallenges();

            using (HttpRequestMessage request = CreateRequest())
            {
                UnauthorizedResult result = CreateProductUnderTest(expectedChallenges, request);

                // Act
                IEnumerable<AuthenticationHeaderValue> challenges = result.Challenges;

                // Assert
                Assert.Same(expectedChallenges, challenges);
            }
        }

        [Fact]
        public void Request_Returns_InstanceProvided()
        {
            // Arrange
            IEnumerable<AuthenticationHeaderValue> challenges = CreateChallenges();

            using (HttpRequestMessage expectedRequest = CreateRequest())
            {
                UnauthorizedResult result = CreateProductUnderTest(challenges, expectedRequest);

                // Act
                HttpRequestMessage request = result.Request;

                // Assert
                Assert.Same(expectedRequest, request);
            }
        }

        [Fact]
        public async Task ExecuteAsync_Returns_CorrectResponse()
        {
            // Arrange
            AuthenticationHeaderValue expectedChallenge1 = CreateChallenge();
            AuthenticationHeaderValue expectedChallenge2 = CreateChallenge();
            IEnumerable<AuthenticationHeaderValue> challenges = new[] { expectedChallenge1, expectedChallenge2 };

            using (HttpRequestMessage expectedRequest = CreateRequest())
            {
                IHttpActionResult result = CreateProductUnderTest(challenges, expectedRequest);

                // Act
                Task<HttpResponseMessage> task = result.ExecuteAsync(CancellationToken.None);

                // Assert
                Assert.NotNull(task);

                using (HttpResponseMessage response = await task)
                {
                    Assert.NotNull(response);
                    Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
                    Assert.Equal(2, response.Headers.WwwAuthenticate.Count);
                    Assert.Same(expectedChallenge1, response.Headers.WwwAuthenticate.ElementAt(0));
                    Assert.Same(expectedChallenge2, response.Headers.WwwAuthenticate.ElementAt(1));
                    Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
                    Assert.Same(expectedRequest, response.RequestMessage);
                }
            }
        }

        [Fact]
        public void Constructor_ForApiController_Throws_WhenControllerIsNull()
        {
            // Arrange
            IEnumerable<AuthenticationHeaderValue> challenges = CreateChallenges();
            ApiController controller = null;

            // Act & Assert
            Assert.ThrowsArgumentNull(() => { CreateProductUnderTest(challenges, controller); }, "controller");
        }

        [Fact]
        public async Task ExecuteAsync_ForApiController_ReturnsCorrectResponse()
        {
            // Arrange
            AuthenticationHeaderValue expectedChallenge1 = CreateChallenge();
            AuthenticationHeaderValue expectedChallenge2 = CreateChallenge();
            IEnumerable<AuthenticationHeaderValue> challenges = new[] { expectedChallenge1, expectedChallenge2 };
            ApiController controller = CreateController();

            using (HttpRequestMessage expectedRequest = CreateRequest())
            {
                controller.Request = expectedRequest;
                IHttpActionResult result = CreateProductUnderTest(challenges, controller);

                // Act
                Task<HttpResponseMessage> task = result.ExecuteAsync(CancellationToken.None);

                // Assert
                Assert.NotNull(task);

                using (HttpResponseMessage response = await task)
                {
                    Assert.NotNull(response);
                    Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
                    Assert.Equal(2, response.Headers.WwwAuthenticate.Count);
                    Assert.Same(expectedChallenge1, response.Headers.WwwAuthenticate.ElementAt(0));
                    Assert.Same(expectedChallenge2, response.Headers.WwwAuthenticate.ElementAt(1));
                    Assert.Same(expectedRequest, response.RequestMessage);
                }
            }
        }

        [Fact]
        public void Request_ForApiController_EvaluatesLazily()
        {
            // Arrange
            IEnumerable<AuthenticationHeaderValue> challenges = CreateChallenges();
            ApiController controller = CreateController();
            UnauthorizedResult result = CreateProductUnderTest(challenges, controller);

            using (HttpRequestMessage expectedRequest = CreateRequest())
            {
                controller.Request = expectedRequest;

                // Act
                HttpRequestMessage request = result.Request;

                // Assert
                Assert.Same(expectedRequest, request);
            }
        }

        [Fact]
        public void Request_ForApiController_EvaluatesOnce()
        {
            // Arrange
            IEnumerable<AuthenticationHeaderValue> challenges = CreateChallenges();
            ApiController controller = CreateController();
            UnauthorizedResult result = CreateProductUnderTest(challenges, controller);

            using (HttpRequestMessage expectedRequest = CreateRequest())
            {
                controller.Request = expectedRequest;
                HttpRequestMessage ignore = result.Request;

                using (HttpRequestMessage otherRequest = CreateRequest())
                {
                    controller.Request = otherRequest;

                    // Act
                    HttpRequestMessage request = result.Request;

                    // Assert
                    Assert.Same(expectedRequest, request);
                }
            }
        }

        [Fact]
        public void Request_ForApiController_Throws_WhenControllerRequestIsNull()
        {
            // Arrange
            IEnumerable<AuthenticationHeaderValue> challenges = CreateChallenges();
            ApiController controller = CreateController();
            Assert.Null(controller.Request);
            UnauthorizedResult result = CreateProductUnderTest(challenges, controller);

            // Act & Assert
            InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() =>
                { HttpRequestMessage ignore = result.Request; }, "ApiController.Request must not be null.");
        }

        [Fact]
        public void ApiControllerUnauthorized_WithIEnumerable_CreatesCorrectResult()
        {
            // Arrange
            IEnumerable<AuthenticationHeaderValue> expectedChallenges = CreateChallenges();
            ApiController controller = CreateController();

            // Act
            UnauthorizedResult result = controller.Unauthorized(expectedChallenges);

            // Assert
            Assert.NotNull(result);
            Assert.Same(expectedChallenges, result.Challenges);

            using (HttpRequestMessage expectedRequest = CreateRequest())
            {
                controller.Request = expectedRequest;
                Assert.Same(expectedRequest, result.Request);
            }
        }

        [Fact]
        public void ApiControllerUnauthorized_WithParams_CreatesCorrectResult()
        {
            // Arrange
            AuthenticationHeaderValue challenge1 = CreateChallenge();
            AuthenticationHeaderValue challenge2 = CreateChallenge();
            ApiController controller = CreateController();

            // Act
            UnauthorizedResult result = controller.Unauthorized(challenge1, challenge2);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Challenges.Count());
            Assert.Same(challenge1, result.Challenges.ElementAt(0));
            Assert.Same(challenge2, result.Challenges.ElementAt(1));

            using (HttpRequestMessage expectedRequest = CreateRequest())
            {
                controller.Request = expectedRequest;
                Assert.Same(expectedRequest, result.Request);
            }
        }

        private static IEnumerable<AuthenticationHeaderValue> CreateChallenges()
        {
            return new AuthenticationHeaderValue[0];
        }

        private static AuthenticationHeaderValue CreateChallenge()
        {
            return new AuthenticationHeaderValue("IgnoreScheme");
        }

        private static ApiController CreateController()
        {
            return new FakeController();
        }

        private static UnauthorizedResult CreateProductUnderTest(IEnumerable<AuthenticationHeaderValue> challenges,
            HttpRequestMessage request)
        {
            return new UnauthorizedResult(challenges, request);
        }

        private static UnauthorizedResult CreateProductUnderTest(IEnumerable<AuthenticationHeaderValue> challenges,
            ApiController controller)
        {
            return new UnauthorizedResult(challenges, controller);
        }

        private static HttpRequestMessage CreateRequest()
        {
            return new HttpRequestMessage();
        }

        private class FakeController : ApiController
        {
        }
    }
}
