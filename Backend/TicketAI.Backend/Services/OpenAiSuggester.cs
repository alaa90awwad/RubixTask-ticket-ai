using System.Text.Json;
using System.Text;
using TicketAI.Backend.Models;

namespace TicketAI.Backend.Services
{
    public class OpenAiSuggester : ICategorySuggester
    {
        private static readonly string[] Allowed =
        [
            "Billing","Technical Support","Login Issue","Feature Request","General Feedback"
        ];

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _config;
        private readonly ILogger<OpenAiSuggester> _logger;
        private readonly string _model;
        private readonly string _apiKey;

        public OpenAiSuggester(IHttpClientFactory httpClientFactory, IConfiguration config, ILogger<OpenAiSuggester> logger)
        {
            _httpClientFactory = httpClientFactory;
            _config = config;
            _logger = logger;
            _model = _config["OpenAI:Model"] ?? "gpt-4o-mini";
            _apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY") ?? "";
            if (string.IsNullOrWhiteSpace(_apiKey))
                _logger.LogWarning("OPENAI_API_KEY is not set.");
        }

        public async Task<SuggestResponse> SuggestAsync(SuggestRequest req, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(req.Title) || string.IsNullOrWhiteSpace(req.Description))
                throw new ArgumentException("Title and Description are required.");

            var system = "You are a ticket categorizer. ONLY respond with one category from this exact list: ['Billing','Technical Support','Login Issue','Feature Request','General Feedback']. If uncertain, choose the closest single category. Return only the category text, nothing else.";
            var userPayload = new { title = req.Title, description = req.Description };

            var payload = new
            {
                model = _model,
                messages = new object[]
                {
                new { role = "system", content = system },
                new { role = "user", content = JsonSerializer.Serialize(userPayload) }
                },
                temperature = 0
            };

            using var client = _httpClientFactory.CreateClient("openai");
            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            // Basic retry (2 retries on 429/5xx)
            for (int attempt = 0; attempt < 3; attempt++)
            {
                var resp = await client.PostAsync("https://api.openai.com/v1/chat/completions", content, ct);
                if (resp.IsSuccessStatusCode)
                {
                    using var doc = JsonDocument.Parse(await resp.Content.ReadAsStringAsync(ct));
                    var raw = doc.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();
                    var coerced = Coerce(raw);
                    var exact = Allowed.Contains(raw?.Trim() ?? "", StringComparer.Ordinal);
                    return new SuggestResponse { Category = coerced, Model = _model, Coerced = !exact };
                }

                // retry on transient
                if ((int)resp.StatusCode == 429 || (int)resp.StatusCode >= 500)
                {
                    await Task.Delay(250 * (attempt + 1), ct);
                    continue;
                }

                var body = await resp.Content.ReadAsStringAsync(ct);
                throw new HttpRequestException($"OpenAI error: {(int)resp.StatusCode} {body}");
            }

            throw new HttpRequestException("OpenAI service unavailable after retries.");
        }

        private static string Coerce(string? raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return "General Feedback";
            var norm = raw.Trim().ToLowerInvariant();

            foreach (var k in Allowed)
                if (norm == k.ToLowerInvariant()) return k;

            if (norm.Contains("bill")) return "Billing";
            if (norm.Contains("login") || norm.Contains("auth") || norm.Contains("password")) return "Login Issue";
            if (norm.Contains("feature") || norm.Contains("request") || norm.Contains("suggestion")) return "Feature Request";
            if (norm.Contains("crash") || norm.Contains("bug") || norm.Contains("error") || norm.Contains("technical")) return "Technical Support";
            return "General Feedback";
        }
    }
}
