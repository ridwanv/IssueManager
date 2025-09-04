using FluentAssertions;
using IssueManager.Bot.Services;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text.Json;
using Xunit;

namespace Infrastructure.UnitTests.Services
{
    public class WhatsAppMessageParserTests
    {
        private readonly Mock<ILogger<WhatsAppMessageParser>> _mockLogger;
        private readonly WhatsAppMessageParser _parser;

        public WhatsAppMessageParserTests()
        {
            _mockLogger = new Mock<ILogger<WhatsAppMessageParser>>();
            _parser = new WhatsAppMessageParser(_mockLogger.Object);
        }

        [Fact]
        public void ParseMessage_WithValidPayload_ShouldReturnParsedData()
        {
            // Arrange
            var validPayload = @"{
                ""object"": ""whatsapp_business_account"",
                ""entry"": [{
                    ""id"": ""123456789"",
                    ""changes"": [{
                        ""field"": ""messages"",
                        ""value"": {
                            ""messaging_product"": ""whatsapp"",
                            ""metadata"": {
                                ""display_phone_number"": ""+1234567890"",
                                ""phone_number_id"": ""phone123""
                            },
                            ""messages"": [{
                                ""id"": ""msg123"",
                                ""from"": ""+27123456789"",
                                ""timestamp"": ""1699123456"",
                                ""type"": ""text"",
                                ""text"": {
                                    ""body"": ""Hello, I need help with an issue""
                                }
                            }]
                        }
                    }]
                }]
            }";

            // Act
            var result = _parser.ParseMessage(validPayload);

            // Assert
            result.Should().NotBeNull();
            result.Object.Should().Be("whatsapp_business_account");
            result.Entry.Should().HaveCount(1);
            result.Entry[0].Id.Should().Be("123456789");
            result.Entry[0].Changes.Should().HaveCount(1);
            result.Entry[0].Changes[0].Field.Should().Be("messages");
        }

        [Fact]
        public void ParseMessage_WithEmptyPayload_ShouldReturnNull()
        {
            // Act
            var result = _parser.ParseMessage("");

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void ParseMessage_WithNullPayload_ShouldReturnNull()
        {
            // Act
            var result = _parser.ParseMessage(null);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void ParseMessage_WithInvalidJson_ShouldReturnNull()
        {
            // Arrange
            var invalidJson = @"{""object"": ""whatsapp_business_account"",";

            // Act
            var result = _parser.ParseMessage(invalidJson);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void ParseMessage_WithMissingRequiredFields_ShouldReturnNull()
        {
            // Arrange
            var incompletePayload = @"{
                ""entry"": [{
                    ""id"": ""123456789""
                }]
            }";

            // Act
            var result = _parser.ParseMessage(incompletePayload);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void ExtractMessageData_WithValidPayload_ShouldReturnMessageData()
        {
            // Arrange
            var payload = new WhatsAppWebhookPayload
            {
                Object = "whatsapp_business_account",
                Entry = new List<WhatsAppEntry>
                {
                    new WhatsAppEntry
                    {
                        Id = "123456789",
                        Changes = new List<WhatsAppChange>
                        {
                            new WhatsAppChange
                            {
                                Field = "messages",
                                Value = new WhatsAppChangeValue
                                {
                                    Metadata = new WhatsAppMetadata
                                    {
                                        DisplayPhoneNumber = "+1234567890",
                                        PhoneNumberId = "phone123"
                                    },
                                    Messages = new List<WhatsAppMessage>
                                    {
                                        new WhatsAppMessage
                                        {
                                            Id = "msg123",
                                            From = "+27123456789",
                                            Timestamp = "1699123456",
                                            Type = "text",
                                            Text = new WhatsAppTextMessage { Body = "Hello, I need help" }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };

            // Act
            var result = _parser.ExtractMessageData(payload);

            // Assert
            result.Should().NotBeNull();
            result.MessageId.Should().Be("msg123");
            result.From.Should().Be("+27123456789");
            result.Timestamp.Should().Be("1699123456");
            result.Type.Should().Be("text");
            result.Text.Should().Be("Hello, I need help");
            result.BusinessAccountId.Should().Be("123456789");
            result.DisplayPhoneNumber.Should().Be("+1234567890");
            result.PhoneNumberId.Should().Be("phone123");
        }

        [Fact]
        public void ExtractMessageData_WithNoMessages_ShouldReturnNull()
        {
            // Arrange
            var payload = new WhatsAppWebhookPayload
            {
                Object = "whatsapp_business_account",
                Entry = new List<WhatsAppEntry>
                {
                    new WhatsAppEntry
                    {
                        Id = "123456789",
                        Changes = new List<WhatsAppChange>
                        {
                            new WhatsAppChange
                            {
                                Field = "messages",
                                Value = new WhatsAppChangeValue
                                {
                                    Messages = new List<WhatsAppMessage>()
                                }
                            }
                        }
                    }
                }
            };

            // Act
            var result = _parser.ExtractMessageData(payload);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void ExtractMessageData_WithEmptyEntry_ShouldReturnNull()
        {
            // Arrange
            var payload = new WhatsAppWebhookPayload
            {
                Object = "whatsapp_business_account",
                Entry = new List<WhatsAppEntry>()
            };

            // Act
            var result = _parser.ExtractMessageData(payload);

            // Assert
            result.Should().BeNull();
        }

        [Theory]
        [InlineData("text", "Hello world", "text")]
        [InlineData("image", null, "image")]
        [InlineData("document", null, "document")]
        public void ExtractMessageData_WithDifferentMessageTypes_ShouldExtractCorrectly(string messageType, string expectedText, string expectedType)
        {
            // Arrange
            var message = new WhatsAppMessage
            {
                Id = "msg123",
                From = "+27123456789",
                Timestamp = "1699123456",
                Type = messageType
            };

            if (messageType == "text")
            {
                message.Text = new WhatsAppTextMessage { Body = expectedText };
            }

            var payload = new WhatsAppWebhookPayload
            {
                Object = "whatsapp_business_account",
                Entry = new List<WhatsAppEntry>
                {
                    new WhatsAppEntry
                    {
                        Id = "123456789",
                        Changes = new List<WhatsAppChange>
                        {
                            new WhatsAppChange
                            {
                                Field = "messages",
                                Value = new WhatsAppChangeValue
                                {
                                    Messages = new List<WhatsAppMessage> { message }
                                }
                            }
                        }
                    }
                }
            };

            // Act
            var result = _parser.ExtractMessageData(payload);

            // Assert
            result.Should().NotBeNull();
            result.Type.Should().Be(expectedType);
            result.Text.Should().Be(expectedText);
        }
    }
}