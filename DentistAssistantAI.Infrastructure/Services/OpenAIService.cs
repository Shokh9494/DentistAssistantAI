using DentistAssistantAI.Core.Configuration;
using DentistAssistantAI.Core.Interfaces;
using DentistAssistantAI.Core.Models;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace DentistAssistantAI.Infrastructure.Services
{
    public class OpenAIService : IOpenAIService
    {
        private readonly HttpClient _httpClient;

        public OpenAIService(HttpClient httpClient, string apiKey)
        {
            _httpClient = httpClient;
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", apiKey);
        }

        public async Task<AIResult> SendAsync(string prompt, string? imagePath = null, string? systemPrompt = null)
        {
            try
            {
                string requestJson;

                if (imagePath != null)
                {
                    var bytes = await File.ReadAllBytesAsync(imagePath);
                    var base64 = Convert.ToBase64String(bytes);
                    var ext = Path.GetExtension(imagePath).TrimStart('.').ToLowerInvariant();
                    var mime = ext == "png" ? "image/png" : "image/jpeg";
                    var fullUserText = DentalAIConfig.ImageAnalysisInstruction + prompt;

                    requestJson = BuildVisionRequest(fullUserText, $"data:{mime};base64,{base64}", systemPrompt);
                }
                else
                {
                    requestJson = BuildTextRequest(prompt, systemPrompt);
                }

                return await ExecuteAsync(requestJson);
            }
            catch (HttpRequestException ex)
            {
                return new AIResult { IsSuccess = false, Error = "Network error: " + ex.Message };
            }
            catch (TaskCanceledException)
            {
                return new AIResult { IsSuccess = false, Error = "Request timeout" };
            }
            catch (Exception ex)
            {
                return new AIResult { IsSuccess = false, Error = "Unexpected error: " + ex.Message };
            }
        }

        /// <summary>
        /// Builds the multimodal vision request JSON using JsonNode — guarantees correct
        /// structure for the image_url content block regardless of serializer behavior.
        /// </summary>
        private static string BuildVisionRequest(string userText, string imageDataUrl, string? systemPrompt = null)
        {
            var userContent = new JsonArray
            {
                new JsonObject
                {
                    ["type"] = "text",
                    ["text"] = userText
                },
                new JsonObject
                {
                    ["type"] = "image_url",
                    ["image_url"] = new JsonObject
                    {
                        ["url"]    = imageDataUrl,
                        ["detail"] = "high"
                    }
                }
            };

            var request = new JsonObject
            {
                ["model"] = DentalAIConfig.VisionModel,
                ["messages"] = new JsonArray
                {
                    new JsonObject { ["role"] = "system", ["content"] = systemPrompt ?? DentalAIConfig.SystemPrompt },
                    new JsonObject { ["role"] = "user",   ["content"] = userContent }
                },
                ["max_tokens"] = 2000
            };

            return request.ToJsonString();
        }

        private static string BuildTextRequest(string prompt, string? systemPrompt = null)
        {
            var request = new JsonObject
            {
                ["model"] = DentalAIConfig.TextModel,
                ["messages"] = new JsonArray
                {
                    new JsonObject { ["role"] = "system", ["content"] = systemPrompt ?? DentalAIConfig.SystemPrompt },
                    new JsonObject { ["role"] = "user",   ["content"] = prompt }
                }
            };

            return request.ToJsonString();
        }

        private async Task<AIResult> ExecuteAsync(string requestJson)
        {
            var content = new StringContent(requestJson, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(
                "https://api.openai.com/v1/chat/completions",
                content);

            var responseString = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                return new AIResult { IsSuccess = false, Error = $"API Error: {responseString}" };

            using var doc = JsonDocument.Parse(responseString);

            if (!doc.RootElement.TryGetProperty("choices", out var choices))
                return new AIResult { IsSuccess = false, Error = "Invalid response format" };

            var message = choices[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            return new AIResult { IsSuccess = true, Content = message };
        }
    }
}