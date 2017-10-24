// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Web.WebPages.Scope;
using System.Web.WebPages.TestUtils;
using Microsoft.TestCommon;

namespace System.Web.Helpers.Test
{
    [Xunit.Collection("Uses ScopeStorage or ViewEngines.Engines")]
    public class WebMailTest : IDisposable
    {
        const string FromAddress = "abc@123.com";
        const string Server = "myserver.com";
        const int Port = 100;
        const string UserName = "My UserName";
        const string Password = "My Password";

        [Fact]
        public void WebMailSmtpServerTests()
        {
            // All tests prior to setting smtp server go here
            // Verify Send throws if no SmtpServer is set
            Assert.Throws<InvalidOperationException>(
                () => WebMail.Send(to: "test@test.com", subject: "test", body: "test body"),
                "\"SmtpServer\" was not specified."
                );

            // Verify SmtpServer uses scope storage.
            // Arrange
            var value = "value";

            // Act
            WebMail.SmtpServer = value;

            // Assert
            Assert.Equal(WebMail.SmtpServer, value);
            Assert.Equal(ScopeStorage.CurrentScope[WebMail.SmtpServerKey], value);
        }

        [Fact]
        public void WebMailSendThrowsIfPriorityIsInvalid()
        {
            Assert.ThrowsArgument(
                () => WebMail.Send(to: "test@test.com", subject: "test", body: "test body", priority: "foo"),
                "priority",
                "The \"priority\" value is invalid. Valid values are \"Low\", \"Normal\" and \"High\"."
                );
        }

        [Fact]
        public void WebMailUsesScopeStorageForSmtpPort()
        {
            // Arrange
            var value = 4;

            // Act
            WebMail.SmtpPort = value;

            // Assert
            Assert.Equal(WebMail.SmtpPort, value);
            Assert.Equal(ScopeStorage.CurrentScope[WebMail.SmtpPortKey], value);
        }

        [Fact]
        public void WebMailUsesScopeStorageForEnableSsl()
        {
            // Arrange
            var value = true;

            // Act
            WebMail.EnableSsl = value;

            // Assert
            Assert.Equal(WebMail.EnableSsl, value);
            Assert.Equal(ScopeStorage.CurrentScope[WebMail.EnableSslKey], value);
        }

        [Fact]
        public void WebMailUsesScopeStorageForDefaultCredentials()
        {
            // Arrange
            var value = true;

            // Act
            WebMail.SmtpUseDefaultCredentials = value;

            // Assert
            Assert.Equal(WebMail.SmtpUseDefaultCredentials, value);
            Assert.Equal(ScopeStorage.CurrentScope[WebMail.SmtpUseDefaultCredentialsKey], value);
        }

        [Fact]
        public void WebMailUsesScopeStorageForUserName()
        {
            // Arrange
            var value = "value";

            // Act
            WebMail.UserName = value;

            // Assert
            Assert.Equal(WebMail.UserName, value);
            Assert.Equal(ScopeStorage.CurrentScope[WebMail.UserNameKey], value);
        }

        [Fact]
        public void WebMailUsesScopeStorageForPassword()
        {
            // Arrange
            var value = "value";

            // Act
            WebMail.Password = value;

            // Assert
            Assert.Equal(WebMail.Password, value);
            Assert.Equal(ScopeStorage.CurrentScope[WebMail.PasswordKey], value);
        }

        [Fact]
        public void WebMailUsesScopeStorageForFrom()
        {
            // Arrange
            var value = "value";

            // Act
            WebMail.From = value;

            // Assert
            Assert.Equal(WebMail.From, value);
            Assert.Equal(ScopeStorage.CurrentScope[WebMail.FromKey], value);
        }

        [Fact]
        public void WebMailThrowsWhenSmtpServerValueIsNullOrEmpty()
        {
            // Act and Assert
            Assert.ThrowsArgumentNullOrEmptyString(() => WebMail.SmtpServer = null, "SmtpServer");
            Assert.ThrowsArgumentNullOrEmptyString(() => WebMail.SmtpServer = String.Empty, "SmtpServer");
        }

        [Fact]
        public void ParseHeaderParsesStringInKeyValueFormat()
        {
            // Arrange
            string header = "foo: bar";

            // Act
            string key, value;

            // Assert
            Assert.True(WebMail.TryParseHeader(header, out key, out value));
            Assert.Equal("foo", key);
            Assert.Equal("bar", value);
        }

        [Fact]
        public void ParseHeaderReturnsFalseIfHeaderIsNotInCorrectFormat()
        {
            // Arrange
            string header = "foo bar";

            // Act
            string key, value;

            // Assert
            Assert.False(WebMail.TryParseHeader(header, out key, out value));
            Assert.Null(key);
            Assert.Null(value);
        }

        [Fact]
        public void SetPropertiesOnMessageTest_SetsAllInfoCorrectlyOnMailMessageTest()
        {
            // Arrange
            MailMessage message = new MailMessage();
            string to = "abc123@xyz.com";
            string subject = "subject1";
            string body = "body1";
            string from = FromAddress;
            string cc = "cc@xyz.com";
            string attachmentName = "NETLogo.png";
            string bcc = "foo@bar.com";
            string replyTo = "x@y.com,z@pqr.com";
            string contentEncoding = "utf-8";
            string headerEncoding = "utf-16";
            var priority = MailPriority.Low;

            // Act
            string fileToAttach = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

            try
            {
                TestFile.Create(attachmentName).Save(fileToAttach);
                bool isBodyHtml = true;
                var additionalHeaders = new[] { "header1:value1" };
                WebMail.SetPropertiesOnMessage(message, to, subject, body, from, cc, bcc, replyTo, contentEncoding, headerEncoding, priority, new[] { fileToAttach }, isBodyHtml, additionalHeaders);

                // Assert
                Assert.Equal(body, message.Body);
                Assert.Equal(subject, message.Subject);
                Assert.Equal(to, message.To[0].Address);
                Assert.Equal(cc, message.CC[0].Address);
                Assert.Equal(from, message.From.Address);
                Assert.Equal(bcc, message.Bcc[0].Address);
                Assert.Equal("x@y.com", message.ReplyToList[0].Address);
                Assert.Equal("z@pqr.com", message.ReplyToList[1].Address);
                Assert.Equal(MailPriority.Low, message.Priority);
                Assert.Equal(Encoding.UTF8, message.BodyEncoding);
                Assert.Equal(Encoding.Unicode, message.HeadersEncoding);

                Assert.Contains("header1", message.Headers.AllKeys);
                Assert.Single(message.Attachments);
            }
            finally
            {
                try
                {
                    File.Delete(fileToAttach);
                }
                catch (IOException)
                {
                } // Try our best to clean up after ourselves
            }
        }

        [Fact]
        public void MailSendWithNullInCollection_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(
                () => WebMail.Send("foo@bar.com", "sub", "body", filesToAttach: new string[] { "c:\\foo.txt", null }),
                "A string in the collection is null or empty.\r\nParameter name: filesToAttach"
            );

            Assert.Throws<ArgumentException>(
                () => WebMail.Send("foo@bar.com", "sub", "body", additionalHeaders: new string[] { "foo:bar", null }),
                "A string in the collection is null or empty.\r\nParameter name: additionalHeaders"
            );
        }

        [Fact]
        public void AssignHeaderValuesIgnoresMalformedHeaders()
        {
            // Arrange
            var message = new MailMessage();
            var headers = new[] { "foo1:bar1", "foo2", "foo3|bar3", "foo4 bar4" };

            // Act
            WebMail.AssignHeaderValues(message, headers);

            // Assert
            Assert.Single(message.Headers);
            Assert.Equal("foo1", message.Headers.AllKeys[0]);
            Assert.Equal("bar1", message.Headers[0]);
        }

        [Fact]
        public void PropertiesDuplicatedAcrossHeaderAndArgumentDoesNotThrow()
        {
            // Arrange
            var message = new MailMessage();
            var headers = new[] { "to:to@test.com" };

            // Act
            WebMail.SetPropertiesOnMessage(message, "to@test.com", null, null, "from@test.com", null, null, null, null, null, MailPriority.Normal, null, false, headers);

            // Assert
            Assert.Equal(2, message.To.Count);
            Assert.Equal("to@test.com", message.To.First().Address);
            Assert.Equal("to@test.com", message.To.Last().Address);
        }

        [Fact]
        public void AssignHeaderValuesSetsPropertiesForKnownHeaderValues()
        {
            // Arrange
            var message = new MailMessage();
            var headers = new[]
            {
                "cc:cc@test.com", "bcc:bcc@test.com,bcc2@test.com", "from:from@test.com", "priority:high", "reply-to:replyto1@test.com,replyto2@test.com",
                "sender: sender@test.com", "to:to@test.com"
            };

            // Act
            WebMail.AssignHeaderValues(message, headers);

            // Assert
            Assert.Equal("cc@test.com", message.CC.Single().Address);
            Assert.Equal("bcc@test.com", message.Bcc.First().Address);
            Assert.Equal("bcc2@test.com", message.Bcc.Last().Address);
            Assert.Equal("from@test.com", message.From.Address);
            Assert.Equal(MailPriority.High, message.Priority);
            Assert.Equal("replyto1@test.com", message.ReplyToList.First().Address);
            Assert.Equal("replyto2@test.com", message.ReplyToList.Last().Address);
            Assert.Equal("sender@test.com", message.Sender.Address);
            Assert.Equal("to@test.com", message.To.Single().Address);

            // Assert we transparently set header values
            Assert.Equal(headers.Count(), message.Headers.Count);
        }

        [Fact]
        public void AssignHeaderDoesNotThrowIfPriorityValueIsInvalid()
        {
            // Arrange
            var message = new MailMessage();
            var headers = new[] { "priority:invalid-value" };

            // Act
            WebMail.AssignHeaderValues(message, headers);

            // Assert
            Assert.Equal(MailPriority.Normal, message.Priority);

            // Assert we transparently set header values
            Assert.Single(message.Headers);
            Assert.Equal("Priority", message.Headers.Keys[0]);
            Assert.Equal("invalid-value", message.Headers["Priority"]);
        }

        [Fact]
        public void AssignHeaderDoesNotThrowIfMailAddressIsInvalid()
        {
            // Arrange
            var message = new MailMessage();
            var headers = new[] { "to:not-#-email@@" };

            // Act
            WebMail.AssignHeaderValues(message, headers);

            // Assert
            Assert.Empty(message.To);

            // Assert we transparently set header values
            Assert.Single(message.Headers);
            Assert.Equal("To", message.Headers.Keys[0]);
            Assert.Equal("not-#-email@@", message.Headers["To"]);
        }

        [Fact]
        public void AssignHeaderDoesNotThrowIfKnownHeaderValuesAreEmptyOrMalformed()
        {
            // Arrange
            var message = new MailMessage();
            var headers = new[] { "to:", ":reply-to", "priority:false" };

            // Act
            WebMail.AssignHeaderValues(message, headers);

            // Assert
            Assert.Empty(message.To);

            // Assert we transparently set header values
            Assert.Single(message.Headers);
            Assert.Equal("Priority", message.Headers.Keys[0]);
            Assert.Equal("false", message.Headers["Priority"]);
        }

        [Fact]
        public void ArgumentsToSendTakePriorityOverHeader()
        {
            // Arrange
            var message = new MailMessage();
            var headers = new[] { "from:header-from@test.com", "cc:header-cc@test.com", "priority:low" };

            // Act
            WebMail.SetPropertiesOnMessage(message, null, null, null, "direct-from@test.com", "direct-cc@test.com", null, null, null, null, MailPriority.High, null, false, headers);

            // Assert
            Assert.Equal("direct-from@test.com", message.From.Address);
            Assert.Equal("header-cc@test.com", message.CC.First().Address);
            Assert.Equal("direct-cc@test.com", message.CC.Last().Address);
            Assert.Equal(MailPriority.High, message.Priority);
        }

        public void Dispose()
        {
            // Reset ScopeStorage (written via e.g. WebMail.SmtpServer) between tests to avoid unexpected interactions.
            ScopeStorage.CurrentProvider = new StaticScopeStorageProvider();
            ScopeStorage.GlobalScope.Clear();
        }
    }
}
