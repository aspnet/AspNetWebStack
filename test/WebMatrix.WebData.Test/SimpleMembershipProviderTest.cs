// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Microsoft.TestCommon;
using Moq;
using WebMatrix.Data;

namespace WebMatrix.WebData.Test
{
    public class SimpleMembershipProviderTest
    {
        [Fact]
        public void ConfirmAccountReturnsFalseIfNoRecordExistsForToken()
        {
            // Arrange
            var database = new Mock<MockDatabase>(MockBehavior.Strict);
            database.As<IDisposable>().Setup(d => d.Dispose());
            database.Setup(d => d.Query("SELECT [UserId], [ConfirmationToken] FROM webpages_Membership WHERE [ConfirmationToken] = @0", "foo"))
                .Returns(Enumerable.Empty<DynamicRecord>());
            var simpleMembershipProvider = new TestSimpleMembershipProvider(database.Object);

            // Act
            bool result = simpleMembershipProvider.ConfirmAccount("foo");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void ConfirmAccountReturnsFalseIfConfirmationTokenDoesNotMatchInCase()
        {
            // Arrange
            var database = new Mock<MockDatabase>(MockBehavior.Strict);
            DynamicRecord record = GetRecord(98, "Foo");
            database.As<IDisposable>().Setup(d => d.Dispose());
            database.Setup(d => d.Query("SELECT [UserId], [ConfirmationToken] FROM webpages_Membership WHERE [ConfirmationToken] = @0", "foo"))
                .Returns(new[] { record });
            var simpleMembershipProvider = new TestSimpleMembershipProvider(database.Object);

            // Act
            bool result = simpleMembershipProvider.ConfirmAccount("foo");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void ConfirmAccountReturnsFalseIfNoConfirmationTokenFromMultipleListMatchesInCase()
        {
            // Arrange
            var database = new Mock<MockDatabase>(MockBehavior.Strict);
            DynamicRecord recordA = GetRecord(98, "Foo");
            DynamicRecord recordB = GetRecord(99, "fOo");
            database.As<IDisposable>().Setup(d => d.Dispose());
            database.Setup(d => d.Query("SELECT [UserId], [ConfirmationToken] FROM webpages_Membership WHERE [ConfirmationToken] = @0", "foo"))
                .Returns(new[] { recordA, recordB });
            var simpleMembershipProvider = new TestSimpleMembershipProvider(database.Object);

            // Act
            bool result = simpleMembershipProvider.ConfirmAccount("foo");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void ConfirmAccountUpdatesIsConfirmedFieldIfConfirmationTokenMatches()
        {
            // Arrange
            var database = new Mock<MockDatabase>(MockBehavior.Strict);
            DynamicRecord record = GetRecord(100, "foo");
            database.As<IDisposable>().Setup(d => d.Dispose());
            database.Setup(d => d.Query("SELECT [UserId], [ConfirmationToken] FROM webpages_Membership WHERE [ConfirmationToken] = @0", "foo"))
                .Returns(new[] { record }).Verifiable();
            database.Setup(d => d.Execute("UPDATE webpages_Membership SET [IsConfirmed] = 1 WHERE [UserId] = @0", 100)).Returns(1).Verifiable();
            var simpleMembershipProvider = new TestSimpleMembershipProvider(database.Object);

            // Act
            bool result = simpleMembershipProvider.ConfirmAccount("foo");

            // Assert
            Assert.True(result);
            database.Verify();
        }

        [Fact]
        public void ConfirmAccountUpdatesIsConfirmedFieldIfAnyOneOfReturnRecordConfirmationTokenMatches()
        {
            // Arrange
            var database = new Mock<MockDatabase>(MockBehavior.Strict);
            DynamicRecord recordA = GetRecord(100, "Foo");
            DynamicRecord recordB = GetRecord(101, "foo");
            DynamicRecord recordC = GetRecord(102, "fOo");
            database.As<IDisposable>().Setup(d => d.Dispose());
            database.Setup(d => d.Query("SELECT [UserId], [ConfirmationToken] FROM webpages_Membership WHERE [ConfirmationToken] = @0", "foo"))
                .Returns(new[] { recordA, recordB, recordC }).Verifiable();
            database.Setup(d => d.Execute("UPDATE webpages_Membership SET [IsConfirmed] = 1 WHERE [UserId] = @0", 101)).Returns(1).Verifiable();
            var simpleMembershipProvider = new TestSimpleMembershipProvider(database.Object);

            // Act
            bool result = simpleMembershipProvider.ConfirmAccount("foo");

            // Assert
            Assert.True(result);
            database.Verify();
        }

        [Fact]
        public void ConfirmAccountWithUserNameReturnsFalseIfNoRecordExistsForToken()
        {
            // Arrange
            var database = new Mock<MockDatabase>(MockBehavior.Strict);
            database.As<IDisposable>().Setup(d => d.Dispose());
            database.Setup(d => d.QuerySingle("SELECT m.[UserId], m.[ConfirmationToken] FROM webpages_Membership m JOIN [Users] u ON m.[UserId] = u.[UserId] WHERE m.[ConfirmationToken] = @0 AND u.[UserName] = @1", "foo", "user12")).Returns(null);
            var simpleMembershipProvider = new TestSimpleMembershipProvider(database.Object) { UserIdColumn = "UserId", UserNameColumn = "UserName", UserTableName = "Users" };

            // Act
            bool result = simpleMembershipProvider.ConfirmAccount("user12", "foo");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void ConfirmAccountWithUserNameReturnsFalseIfConfirmationTokenDoesNotMatchInCase()
        {
            // Arrange
            var database = new Mock<MockDatabase>(MockBehavior.Strict);
            DynamicRecord record = GetRecord(98, "Foo");
            database.As<IDisposable>().Setup(d => d.Dispose());
            database.Setup(d => d.QuerySingle("SELECT m.[UserId], m.[ConfirmationToken] FROM webpages_Membership m JOIN [Users_bkp2_1] u ON m.[UserId] = u.[wishlist_site_real_user_id] WHERE m.[ConfirmationToken] = @0 AND u.[wishlist_site_real_user_name] = @1", "foo", "user13")).Returns(record);
            var simpleMembershipProvider = new TestSimpleMembershipProvider(database.Object) { UserIdColumn = "wishlist_site_real_user_id", UserNameColumn = "wishlist_site_real_user_name", UserTableName = "Users_bkp2_1" };

            // Act
            bool result = simpleMembershipProvider.ConfirmAccount("user13", "foo");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void ConfirmAccountWithUserNameUpdatesIsConfirmedFieldIfConfirmationTokenMatches()
        {
            // Arrange
            var database = new Mock<MockDatabase>(MockBehavior.Strict);
            DynamicRecord record = GetRecord(100, "foo");
            database.As<IDisposable>().Setup(d => d.Dispose());
            database.Setup(d => d.QuerySingle("SELECT m.[UserId], m.[ConfirmationToken] FROM webpages_Membership m JOIN [Users] u ON m.[UserId] = u.[Id] WHERE m.[ConfirmationToken] = @0 AND u.[UserName] = @1", "foo", "user14"))
                .Returns(record).Verifiable();
            database.Setup(d => d.Execute("UPDATE webpages_Membership SET [IsConfirmed] = 1 WHERE [UserId] = @0", 100)).Returns(1).Verifiable();
            var simpleMembershipProvider = new TestSimpleMembershipProvider(database.Object) { UserTableName = "Users", UserIdColumn = "Id", UserNameColumn = "UserName" };

            // Act
            bool result = simpleMembershipProvider.ConfirmAccount("user14", "foo");

            // Assert
            Assert.True(result);
            database.Verify();
        }

        [Fact]
        public void GenerateTokenHtmlEncodesValues()
        {
            // Arrange
            var generator = new Mock<RandomNumberGenerator>(MockBehavior.Strict);
            var generatedBytes = Encoding.Default.GetBytes("|aÿx§#½oÿ↨îA8Eµ");
            generator.Setup(g => g.GetBytes(It.IsAny<byte[]>())).Callback((byte[] array) => Array.Copy(generatedBytes, array, generatedBytes.Length));

            // Act
            var result = SimpleMembershipProvider.GenerateToken(generator.Object);

            // Assert
            Assert.Equal("fGH/eKcjvW//P+5BOEW1", Convert.ToBase64String(generatedBytes));
            Assert.Equal("fGH_eKcjvW__P-5BOEW1AA2", result);
        }

        [Fact]
        public void GetUserId_WithCaseNormalization()
        {
            // Arrange
            var database = new Mock<MockDatabase>(MockBehavior.Strict);
            var expectedQuery = @"SELECT userId FROM users WHERE (UPPER(userName) = UPPER(@0))";
            database.Setup(d => d.QueryValue(expectedQuery, "zeke")).Returns(999);

            // Act
            var result = SimpleMembershipProvider.GetUserId(
                database.Object,
                "users",
                "userName",
                "userId",
                SimpleMembershipProviderCasingBehavior.NormalizeCasing,
                "zeke");

            // Assert
            Assert.Equal(999, result);
        }

        [Fact]
        public void GetUserId_WithoutCaseNormalization()
        {
            // Arrange
            var database = new Mock<MockDatabase>(MockBehavior.Strict);
            var expectedQuery = @"SELECT userId FROM users WHERE (userName = @0)";
            database.Setup(d => d.QueryValue(expectedQuery, "zeke")).Returns(999);

            // Act
            var result = SimpleMembershipProvider.GetUserId(
                database.Object,
                "users",
                "userName",
                "userId",
                SimpleMembershipProviderCasingBehavior.RelyOnDatabaseCollation,
                "zeke");

            // Assert
            Assert.Equal(999, result);
        }

        [Theory]
        [ReplaceCulture]
        [InlineData(-1, false)]
        [InlineData(0, true)]
        [InlineData(1, true)]
        [InlineData(2, false)]
        public void SimpleMembershipProvider_CasingBehavior_ValidatesRange(int value, bool isValid)
        {
            // Arrange
            var provider = new SimpleMembershipProvider();

            var message =
                "The value of argument 'value' (" + value + ") is invalid for Enum type " +
                "'SimpleMembershipProviderCasingBehavior'." + Environment.NewLine +
                "Parameter name: value";

            // Act
            Exception exception = null;

            try
            {
                provider.CasingBehavior = (SimpleMembershipProviderCasingBehavior)value;
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            // Assert
            if (isValid)
            {
                Assert.Equal((SimpleMembershipProviderCasingBehavior)value, provider.CasingBehavior);
            }
            else
            {
                Assert.NotNull(exception);
                Assert.IsAssignableFrom<InvalidEnumArgumentException>(exception);
                Assert.Equal(message, exception.Message);
            }
        }

        private static DynamicRecord GetRecord(int userId, string confirmationToken)
        {
            var data = new Mock<IDataRecord>(MockBehavior.Strict);
            data.Setup(c => c[0]).Returns(userId);
            data.Setup(c => c[1]).Returns(confirmationToken);
            return new DynamicRecord(new[] { "UserId", "ConfirmationToken" }, data.Object);
        }

        private class TestSimpleMembershipProvider : SimpleMembershipProvider
        {
            private readonly IDatabase _database;

            public TestSimpleMembershipProvider(IDatabase database)
            {
                _database = database;
            }

            internal override IDatabase ConnectToDatabase()
            {
                return _database;
            }

            internal override void VerifyInitialized()
            {
                // Do nothing.
            }
        }
    }
}
