using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;
using HUNting.API.Models.Chat;

namespace HUNting.API.Services;

public class OpenAiService : IOpenAiService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<OpenAiService> _logger;

    public OpenAiService(HttpClient httpClient, IConfiguration configuration, ILogger<OpenAiService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<string> GetChatResponseAsync(List<ChatMessage> history, string newMessage)
    {
        var apiKey = _configuration["OpenAI:ApiKey"];
        if (string.IsNullOrEmpty(apiKey))
        {
            _logger.LogError("OpenAI API Key is missing from configuration.");
            throw new Exception("Belső szerverhiba: OpenAI API kulcs nincs konfigurálva.");
        }

        var systemPrompt = @"Te egy magyar nyelvű professzionális vadászati és fegyverügyi asszisztens vagy.
KIZÁRÓLAG az 1996. évi LV. törvény a vad védelméről, a vadgazdálkodásról, valamint a vadászatról, továbbá a fegyvertartási és fegyverüzemeltetési szabályok alapján dolgozhatsz és válaszolhatsz.
SZIGORÚ SZABÁLYOK:
1. Ha a felhasználó kérdése a vadászattal, fegyvertartással vagy fegyverismerettel kapcsolatos, szakszerűen, pontosan a törvény/szabályozás szellemében adj választ.
2. Ha a kérdés túlmegy az 1996. évi hazai vadászati törvény és fegyverismereti vizsga/szabályozás témakörén (pl. receptek, napi politika, programozás, viccek), kedvesen, de határozottan UTASÍTSD EL a választ a következőhöz hasonlóan: 'Elnézést, de csak a vadászati törvénnyel és fegyverismerettel kapcsolatos kérdésekre tudok válaszolni a megadott dokumentumok alapján.'
3. Soha ne találj ki (hallucinálj) olyan jogszabályi adatot vagy fegyverismereti tényt, amely nem felel meg a valóságnak.
Légy segítőkész, de szigorúan ragaszkodj ehhez a szakterülethez!";

        var messages = new List<object>
        {
            new { role = "system", content = systemPrompt }
        };

        // Előzmények hozzáadása
        foreach (var msg in history)
        {
            // Biztonsági okokból csak valid role-okat engedünk át
            var role = msg.Role == "user" || msg.Role == "assistant" ? msg.Role : "user";
            messages.Add(new { role = role, content = msg.Content });
        }

        // Új üzenet
        messages.Add(new { role = "user", content = newMessage });

        var requestBody = new
        {
            model = "gpt-4o", // a legújabb ChatGPT modell
            messages = messages,
            temperature = 0.2, // Alacsony hőmérséklet a konzekvens és tényszerű válaszokhoz
            max_tokens = 1500
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/chat/completions");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        request.Content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

        try
        {
            var response = await _httpClient.SendAsync(request);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("OpenAI API hiba: {StatusCode} {ErrorContent}", response.StatusCode, errorContent);
                throw new Exception("Hiba történt az AI szerviz hívása során.");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            using var document = JsonDocument.Parse(responseContent);
            
            var result = document.RootElement
                                 .GetProperty("choices")[0]
                                 .GetProperty("message")
                                 .GetProperty("content")
                                 .GetString();

            return result ?? "Nem sikerült értelmezhető választ kapni a rendszertől.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "OpenAI API hívási kivétel.");
            throw;
        }
    }
}
