using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Linq;
using Chitti.Data;
using Chitti.Models;
using Chitti.Helpers;
using Microsoft.EntityFrameworkCore;

namespace Chitti.Services;

public class GeminiService
{
    private readonly HttpClient _httpClient;
    private readonly ApplicationDbContext _dbContext;
    private const string API_URL = "https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent";

    public GeminiService()
    {
        _httpClient = new HttpClient();
        _dbContext = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite($"Data Source={AppPaths.DatabasePath}")
            .Options);
    }

    public async Task<string> ProcessText(string text, List<string> tags)
    {
        Logger.Log($"GeminiService: Preparing to process text. Tags: [{string.Join(", ", tags)}]");
        var settings = await _dbContext.AppSettings.FirstOrDefaultAsync();
        if (settings == null || string.IsNullOrEmpty(settings.ApiKey))
        {
            Logger.Log("GeminiService: API key not configured.");
            throw new InvalidOperationException("API key not configured. Please set it in Settings.");
        }

        var decryptedApiKey = ApiKeyProtector.Decrypt(settings.ApiKey);
        if (string.IsNullOrWhiteSpace(decryptedApiKey))
        {
            Logger.Log("GeminiService: Decrypted API key is empty or invalid.");
            throw new InvalidOperationException("Decrypted API key is empty or invalid.");
        }

        var prompt = BuildPrompt(text, tags);
        var requestBody = new
        {
            contents = new[]
            {
                new
                {
                    parts = new[]
                    {
                        new { text = prompt }
                    }
                }
            }
        };

        try
        {
            Logger.Log("GeminiService: Sending request to Gemini API.");
            var response = await _httpClient.PostAsync(
                $"{API_URL}?key={decryptedApiKey}",
                new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json"));

            Logger.Log($"GeminiService: API response status: {response.StatusCode}");
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Logger.Log($"GeminiService: API error content: {errorContent}");
                throw new Exception($"API request failed: {response.StatusCode} - {errorContent}");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            Logger.Log($"GeminiService: API response content: {responseContent}");
            var responseObject = JsonSerializer.Deserialize<JsonElement>(responseContent);

            return responseObject
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString() ?? string.Empty;
        }
        catch (Exception ex)
        {
            Logger.Log($"GeminiService: Exception occurred: {ex}");
            throw;
        }
    }

    private string BuildPrompt(string text, List<string> tags)
    {
        var prompt = new StringBuilder();
        prompt.AppendLine("You are a professional text enhancement assistant. Please process the following text according to these requirements:");

        // Handle translation tags
        var translationTags = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "/english", "English" },
            { "/mandarin", "Mandarin" },
            { "/hindi", "Hindi" },
            { "/spanish", "Spanish" },
            { "/french", "French" },
            { "/arabic", "Arabic" },
            { "/bengali", "Bengali" },
            { "/russian", "Russian" },
            { "/portuguese", "Portuguese" },
            { "/tamil", "Tamil" }
        };
        var foundTranslationTag = tags.FirstOrDefault(t => translationTags.ContainsKey(t));
        if (foundTranslationTag != null)
        {
            var language = translationTags[foundTranslationTag];
            prompt.AppendLine($"- Translate the following text to {language}.");
        }

        // Handle @chitti tag first
        var chittiTag = tags.FirstOrDefault(t => t.StartsWith("@chitti"));
        if (chittiTag != null)
        {
            var customPrompt = chittiTag.Substring("@chitti".Length).Trim();
            prompt.AppendLine($"- {customPrompt}");
        }

        // Grammar & Language Tags
        if (tags.Contains("grammar"))
            prompt.AppendLine("- Correct all grammar, spelling, and punctuation errors");
        if (tags.Contains("punctuation"))
            prompt.AppendLine("- Fix all punctuation marks and their usage");
        if (tags.Contains("spelling"))
            prompt.AppendLine("- Correct all spelling errors");
        if (tags.Contains("syntax"))
            prompt.AppendLine("- Improve sentence structure and syntax");
        if (tags.Contains("tense"))
            prompt.AppendLine("- Ensure consistent verb tense usage");
        if (tags.Contains("agreement"))
            prompt.AppendLine("- Fix all subject-verb agreement issues");

        // Tone Tags
        if (tags.Contains("formal"))
            prompt.AppendLine("- Make the text more professional and business-like");
        if (tags.Contains("casual"))
            prompt.AppendLine("- Make the text more relaxed and informal");
        if (tags.Contains("friendly"))
            prompt.AppendLine("- Add warmth and approachability to the tone");
        if (tags.Contains("polite"))
            prompt.AppendLine("- Add courtesy and respect to the tone");
        if (tags.Contains("assertive"))
            prompt.AppendLine("- Make the text more direct and confident");
        if (tags.Contains("diplomatic"))
            prompt.AppendLine("- Make the text more tactful and considerate");
        if (tags.Contains("empathetic"))
            prompt.AppendLine("- Add emotional understanding and sensitivity");
        if (tags.Contains("enthusiastic"))
            prompt.AppendLine("- Add energy and excitement to the tone");
        if (tags.Contains("neutral"))
            prompt.AppendLine("- Remove emotional bias and maintain objectivity");
        if (tags.Contains("humorous"))
            prompt.AppendLine("- Add appropriate humor while maintaining professionalism");
        if (tags.Contains("serious"))
            prompt.AppendLine("- Make the text more grave and important");
        if (tags.Contains("academic"))
            prompt.AppendLine("- Make the text more scholarly and research-oriented");

        // Style Tags
        if (tags.Contains("fluent"))
            prompt.AppendLine("- Improve the flow and readability");
        if (tags.Contains("concise"))
            prompt.AppendLine("- Make the text more brief and to the point");
        if (tags.Contains("detailed"))
            prompt.AppendLine("- Add more specific information and details");
        if (tags.Contains("firstletter"))
            prompt.AppendLine("- Ensure first letter of each sentence is capitalized");
        if (tags.Contains("allcaps"))
            prompt.AppendLine("- Convert text to all capital letters");
        if (tags.Contains("sentencecase"))
            prompt.AppendLine("- Convert text to sentence case");
        if (tags.Contains("titlecase"))
            prompt.AppendLine("- Convert text to Title Case");
        if (tags.Contains("bulletpoints"))
            prompt.AppendLine("- Convert text to bullet point format");
        if (tags.Contains("numbered"))
            prompt.AppendLine("- Convert text to numbered list format");
        if (tags.Contains("paragraph"))
            prompt.AppendLine("- Format text into proper paragraphs");
        if (tags.Contains("simplify"))
            prompt.AppendLine("- Use simpler vocabulary and sentence structure");
        if (tags.Contains("expand"))
            prompt.AppendLine("- Use more sophisticated vocabulary and complex structures");
        if (tags.Contains("active"))
            prompt.AppendLine("- Convert to active voice");
        if (tags.Contains("passive"))
            prompt.AppendLine("- Convert to passive voice");

        // Format Tags
        if (tags.Contains("markdown"))
            prompt.AppendLine("- Format the text in markdown");
        if (tags.Contains("html"))
            prompt.AppendLine("- Format the text in HTML");
        if (tags.Contains("json"))
            prompt.AppendLine("- Format the text as JSON");
        if (tags.Contains("xml"))
            prompt.AppendLine("- Format the text as XML");
        if (tags.Contains("csv"))
            prompt.AppendLine("- Format the text as CSV");
        if (tags.Contains("table"))
            prompt.AppendLine("- Convert the text into a table format");
        if (tags.Contains("code"))
            prompt.AppendLine("- Format the text as a code block");
        if (tags.Contains("quote"))
            prompt.AppendLine("- Format the text as a quotation");
        if (tags.Contains("indent"))
            prompt.AppendLine("- Add proper indentation");
        if (tags.Contains("spacing"))
            prompt.AppendLine("- Adjust line spacing appropriately");

        // Content Tags
        if (tags.Contains("summarize"))
            prompt.AppendLine("- Create a concise summary of the text");
        if (tags.Contains("keypoints"))
            prompt.AppendLine("- Extract and list the key points");
        if (tags.Contains("translate"))
            prompt.AppendLine("- Translate the text to the target language");
        if (tags.Contains("paraphrase"))
            prompt.AppendLine("- Rewrite the text in different words");
        if (tags.Contains("proofread"))
            prompt.AppendLine("- Perform a thorough proofreading");
        if (tags.Contains("factcheck"))
            prompt.AppendLine("- Verify factual information");
        if (tags.Contains("citations"))
            prompt.AppendLine("- Add proper citations");
        if (tags.Contains("references"))
            prompt.AppendLine("- Add a reference list");

        // Audience-Specific Tags
        if (tags.Contains("technical"))
            prompt.AppendLine("- Make the text more technical and specialized");
        if (tags.Contains("layman"))
            prompt.AppendLine("- Make the text more understandable for general audience");
        if (tags.Contains("expert"))
            prompt.AppendLine("- Make the text more specialized for experts");
        if (tags.Contains("beginner"))
            prompt.AppendLine("- Make the text more basic and introductory");
        if (tags.Contains("children"))
            prompt.AppendLine("- Make the text child-friendly");
        if (tags.Contains("senior"))
            prompt.AppendLine("- Make the text more accessible for older readers");

        // Purpose Tags
        if (tags.Contains("persuasive"))
            prompt.AppendLine("- Make the text more convincing");
        if (tags.Contains("informative"))
            prompt.AppendLine("- Focus on clear information delivery");
        if (tags.Contains("instructional"))
            prompt.AppendLine("- Make the text more tutorial-like");
        if (tags.Contains("descriptive"))
            prompt.AppendLine("- Add more description and detail");
        if (tags.Contains("narrative"))
            prompt.AppendLine("- Make the text more story-like");
        if (tags.Contains("analytical"))
            prompt.AppendLine("- Make the text more analytical");
        if (tags.Contains("critical"))
            prompt.AppendLine("- Add critical analysis");

        // Industry-Specific Tags
        if (tags.Contains("legal"))
            prompt.AppendLine("- Use legal terminology and precision");
        if (tags.Contains("medical"))
            prompt.AppendLine("- Use medical terminology appropriately");
        if (tags.Contains("scientific"))
            prompt.AppendLine("- Use scientific language and methodology");
        if (tags.Contains("business"))
            prompt.AppendLine("- Use business terminology and concepts");

        // Special Purpose Tags
        if (tags.Contains("seo"))
            prompt.AppendLine("- Optimize the text for search engines");
        if (tags.Contains("social"))
            prompt.AppendLine("- Optimize the text for social media");
        if (tags.Contains("email"))
            prompt.AppendLine("- Format the text as a professional email");
        if (tags.Contains("report"))
            prompt.AppendLine("- Format the text as a formal report");
        if (tags.Contains("presentation"))
            prompt.AppendLine("- Format the text for a presentation");
        if (tags.Contains("blog"))
            prompt.AppendLine("- Format the text as a blog post");
        if (tags.Contains("news"))
            prompt.AppendLine("- Format the text as a news article");

        prompt.AppendLine("\nText to process:");
        prompt.AppendLine(text);
        prompt.AppendLine("\nPlease provide only the processed text without any explanations or additional context.");

        return prompt.ToString();
    }
} 