using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Moq.Protected;
using HUNting.API.Models.Chat;
using HUNting.API.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;

namespace HUNting.API.Tests.Services
{
    public class OpenAiServiceTests
    {
        private static OpenAiService CreateService(
            HttpResponseMessage response,
            string? apiKey = "test-api-key",
            Action<string>? captureRequestBody = null,
            Action<HttpRequestMessage>? captureRequest = null)
        {
            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .Callback<HttpRequestMessage, CancellationToken>((req, _) =>
                {
                    captureRequest?.Invoke(req);
                    if (captureRequestBody != null && req.Content != null)
                    {
                        // A kérés törzsét még a dispose előtt kell kiolvasni
                        captureRequestBody(req.Content.ReadAsStringAsync().GetAwaiter().GetResult());
                    }
                })
                .ReturnsAsync(response);

            var configValues = new Dictionary<string, string?>();
            if (apiKey != null)
            {
                configValues["OpenAI:ApiKey"] = apiKey;
            }

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(configValues)
                .Build();

            return new OpenAiService(new HttpClient(handlerMock.Object), configuration, NullLogger<OpenAiService>.Instance);
        }

        private static HttpResponseMessage SuccessResponse(string content) =>
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    JsonSerializer.Serialize(new
                    {
                        choices = new[] { new { message = new { content } } }
                    }),
                    Encoding.UTF8,
                    "application/json")
            };

        [Fact]
        public async Task GetChatResponseAsync_MissingApiKey_ThrowsException()
        {
            // Arrange
            var service = CreateService(SuccessResponse("valasz"), apiKey: null);

            // Act + Assert
            await Assert.ThrowsAsync<Exception>(
                () => service.GetChatResponseAsync(new List<ChatMessage>(), "Kerdes"));
        }

        [Fact]
        public async Task GetChatResponseAsync_SuccessfulCall_ReturnsContentAndSendsApiKey()
        {
            // Arrange
            string? authScheme = null;
            string? authToken = null;
            var service = CreateService(
                SuccessResponse("Az oz majus 1-tol vadaszhato."),
                captureRequest: req =>
                {
                    authScheme = req.Headers.Authorization?.Scheme;
                    authToken = req.Headers.Authorization?.Parameter;
                });

            // Act
            var result = await service.GetChatResponseAsync(new List<ChatMessage>(), "Mikor vadaszhato az oz?");

            // Assert
            Assert.Equal("Az oz majus 1-tol vadaszhato.", result);
            Assert.Equal("Bearer", authScheme);
            Assert.Equal("test-api-key", authToken);
        }

        [Fact]
        public async Task GetChatResponseAsync_ApiError_ThrowsException()
        {
            // Arrange
            var errorResponse = new HttpResponseMessage(HttpStatusCode.InternalServerError)
            {
                Content = new StringContent("server error")
            };
            var service = CreateService(errorResponse);

            // Act + Assert
            await Assert.ThrowsAsync<Exception>(
                () => service.GetChatResponseAsync(new List<ChatMessage>(), "Kerdes"));
        }

        [Fact]
        public async Task GetChatResponseAsync_InvalidHistoryRole_IsSanitizedToUser()
        {
            // Arrange
            string? requestBody = null;
            var service = CreateService(SuccessResponse("ok"), captureRequestBody: body => requestBody = body);
            var history = new List<ChatMessage>
            {
                new ChatMessage { Role = "hacker", Content = "gyanus uzenet" },
                new ChatMessage { Role = "assistant", Content = "korabbi valasz" }
            };

            // Act
            await service.GetChatResponseAsync(history, "Uj kerdes");

            // Assert
            Assert.NotNull(requestBody);
            using var document = JsonDocument.Parse(requestBody!);
            var messages = document.RootElement.GetProperty("messages");
            Assert.Equal("system", messages[0].GetProperty("role").GetString());
            Assert.Equal("user", messages[1].GetProperty("role").GetString()); // "hacker" -> "user"
            Assert.Equal("assistant", messages[2].GetProperty("role").GetString());
            Assert.Equal("user", messages[3].GetProperty("role").GetString());
            Assert.Equal("Uj kerdes", messages[3].GetProperty("content").GetString());
        }
    }
}
