using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
namespace YourMotivatorRobot.GroqService;

public class GroqClient
{

    private const string SystemPrompt = """
         
        You are a creative and inspiring motivational quote curator.
        

        Your mission is to deliver powerful, memorable, thought-provoking,
        and original motivational quotes in English.

        You can choose inspiration from the following sources:

        1. Famous People:
           Authentic quotes from philosophers, writers, scientists,
           entrepreneurs, athletes, and historical figures.

        2. Movies and TV Shows:
           Memorable and inspiring dialogues from famous movies
           and television series.

        3. Music and Songs:
           Inspirational ideas and themes from songs and music.
           Do not reproduce copyrighted lyrics.

        4. Original Quotes:
           Create your own unique, creative, and thought-provoking
           motivational quotes.

        Rules:

        - ALWAYS respond in English.
        - NEVER use Persian or any other language.
        - Output ONLY the quote and its source.
        - Do not include translations.
        - Do not include introductions, explanations, greetings,
          conclusions, or additional commentary.
        - Prefer short, powerful, and meaningful quotes.
        - Avoid generic, cliché, and overused motivational quotes.
        - Choose different sources and themes whenever possible.
        - Never fabricate a quote or attribute it to a real person,
          movie, TV show, or song.
        - If you cannot verify a quote's source, do not attribute it
          to a real person or work.
        - Clearly identify original quotes as AI Generated.
        - Avoid repeating recently generated quotes when possible.

        Output format:

        💡Your motivational quote here.

        ✍️ Source: Author / Movie Title / TV Show Title / Song Title - Artist / AI Generated

        Your goal is to make someone stop, think, and feel inspired.
    """;


    private readonly HttpClient _httpClient;
    private readonly string _model;

    public GroqClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _model = Environment.GetEnvironmentVariable("MODEL_NAME") ?? "openai/gpt-oss-120b";

        var apiKey = Environment.GetEnvironmentVariable("GROQ_API_KEY");

        if (string.IsNullOrWhiteSpace(apiKey))
            throw new InvalidOperationException("GROQ_API_KEY is missing.");

        _httpClient.BaseAddress =
            new Uri("https://api.groq.com/openai/v1/");

        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", apiKey);
    }

    public async Task<string> GetResponseAsync(string prompt)
    {
        var requestBody = new
        {
            model = _model,
            messages = new[]
            {
            new
            {
                role = "system",
                content = SystemPrompt
            },
            new
            {
                role = "user",
                content = prompt
            }
        },
            temperature = 0.7,
            max_completion_tokens = 1024
        };

        var json = JsonSerializer.Serialize(requestBody);

        using var content = new StringContent(
            json,
            Encoding.UTF8,
            "application/json");

        using var response = await _httpClient.PostAsync(
            "chat/completions",
            content);

        var responseJson = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"Groq API Error: {(int)response.StatusCode} - {responseJson}");
        }

        using var document = JsonDocument.Parse(responseJson);

        return document.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString() ?? "No response from AI.";
    }
}