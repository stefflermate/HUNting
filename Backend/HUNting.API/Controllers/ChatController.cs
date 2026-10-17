using Microsoft.AspNetCore.Mvc;
using HUNting.API.Models.Chat;
using HUNting.API.Services;

namespace HUNting.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChatController : ControllerBase
{
    private readonly IOpenAiService _openAiService;

    public ChatController(IOpenAiService openAiService)
    {
        _openAiService = openAiService;
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] ChatRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Message))
        {
            return BadRequest(new { error = "Az üzenet nem lehet üres." });
        }

        try
        {
            var response = await _openAiService.GetChatResponseAsync(request.History ?? new List<ChatMessage>(), request.Message);
            return Ok(new { reply = response });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Hiba történt a kérdés feldolgozása során. Kérjük, próbálja később." });
        }
    }
}
