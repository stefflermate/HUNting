using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using HUNting.API.Controllers;
using HUNting.API.Models.Chat;
using HUNting.API.Services;

namespace HUNting.API.Tests.Controllers
{
    public class ChatControllerTests
    {
        private readonly Mock<IOpenAiService> _openAiMock;
        private readonly ChatController _controller;

        public ChatControllerTests()
        {
            _openAiMock = new Mock<IOpenAiService>();
            _controller = new ChatController(_openAiMock.Object);
        }

        [Fact]
        public async Task Post_EmptyMessage_ReturnsBadRequest()
        {
            // Arrange
            var request = new ChatRequest { Message = "" };

            // Act
            var result = await _controller.Post(request);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
            _openAiMock.Verify(s => s.GetChatResponseAsync(It.IsAny<List<ChatMessage>>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task Post_WhitespaceMessage_ReturnsBadRequest()
        {
            // Arrange
            var request = new ChatRequest { Message = "   " };

            // Act
            var result = await _controller.Post(request);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task Post_ValidMessage_ReturnsOkWithReply()
        {
            // Arrange
            var request = new ChatRequest { Message = "Mikor vadaszhato az oz?" };
            _openAiMock.Setup(s => s.GetChatResponseAsync(It.IsAny<List<ChatMessage>>(), request.Message))
                       .ReturnsAsync("A valasz.");

            // Act
            var result = await _controller.Post(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var replyProperty = okResult.Value!.GetType().GetProperty("reply");
            Assert.NotNull(replyProperty);
            Assert.Equal("A valasz.", replyProperty!.GetValue(okResult.Value));
        }

        [Fact]
        public async Task Post_PassesHistoryToService()
        {
            // Arrange
            var history = new List<ChatMessage> { new ChatMessage { Role = "user", Content = "Elozo kerdes" } };
            var request = new ChatRequest { Message = "Uj kerdes", History = history };
            _openAiMock.Setup(s => s.GetChatResponseAsync(history, "Uj kerdes")).ReturnsAsync("Valasz");

            // Act
            await _controller.Post(request);

            // Assert
            _openAiMock.Verify(s => s.GetChatResponseAsync(history, "Uj kerdes"), Times.Once);
        }

        [Fact]
        public async Task Post_ServiceThrows_Returns500()
        {
            // Arrange
            var request = new ChatRequest { Message = "Kerdes" };
            _openAiMock.Setup(s => s.GetChatResponseAsync(It.IsAny<List<ChatMessage>>(), It.IsAny<string>()))
                       .ThrowsAsync(new Exception("API hiba"));

            // Act
            var result = await _controller.Post(request);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, objectResult.StatusCode);
        }
    }
}
