using HUNting.API.Models.Chat;

namespace HUNting.API.Services;

public interface IOpenAiService
{
    Task<string> GetChatResponseAsync(List<ChatMessage> history, string newMessage);
}
