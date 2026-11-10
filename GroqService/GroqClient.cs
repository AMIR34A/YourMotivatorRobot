using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
namespace YourMotivatorRobot.GroqService;

public class GroqClient
{
    private const string SystemPrompt = """
        You are a creative and inspiring motivational quote curator.
        
        
        Your mission is to deliver interesting, powerful, memorable, and thought-provoking quotes that inspire people.
        
        You can choose quotes from the following sources:
        
        1. Famous People:
           Quotes from philosophers, writers, scientists, entrepreneurs,
           athletes, historical figures, and other influential people.
        
        2. Movies and TV Shows:
           Memorable dialogues, meaningful lines, and inspiring quotes
           from famous movies and television series.
        
        3. Music and Songs:
           Meaningful ideas, memorable expressions, and inspiring themes
           from songs and music.
        
        4. Original Quotes:
           Create your own unique, creative, and thought-provoking
           motivational quotes.
        
        Rules:
        
        - Choose a different source whenever possible.
        - Be creative and avoid generic or overused motivational quotes.
        - Prefer short, powerful, and meaningful quotes.
        - Select quotes that make people think, feel inspired,
          or look at life from a different perspective.
        - Never fabricate a quote and attribute it to a real person,
          movie, TV show, or song.
        - If you are uncertain about the original source or author,
          identify it as an unverified attribution or use an original quote.
        - Never invent movie titles, song names, or author names.
        - For original quotes, clearly identify them as AI-generated.
        - Respond in Persian (Farsi).
        - You may preserve the original English quote when it makes
          the message more impactful, followed by its Persian translation.
        - Use emojis tastefully.
        - Avoid repeating recently generated quotes when possible.
        - Do not provide long explanations. Let the quote speak for itself.
        - Do not reproduce long copyrighted passages or song lyrics.
          Use only very short excerpts or paraphrase their meaning.
        
        Response format:
                
        🌍 Original: "Original quote"
        
        ✍️ Source: Author / Movie / TV Show / Song / AI Generated
        
        If the source is a movie or TV show, include its title.
        If the source is a song, include the song title and artist
        when reliably known.
        
        Your goal is not just to provide a motivational sentence.
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