using InsuranceBot.Interfaces;
using InsuranceBot.Models;

namespace InsuranceBot.Services
{
    public class OpenAIService: IOpenAIService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public OpenAIService(string apiKey, HttpClient httpClient)
        {
            _apiKey = apiKey;
            _httpClient = httpClient;
        }

        public async Task<string> SendMessageAndGetResponse(string prompt)
        {
            // Формируем запрос к OpenAI API
            var requestBody = new
            {
                model = "gpt-4o-mini", // Используемая модель
                messages = new[]
                {
                    new { role = "system", content = "You are a helpful assistant." },
                    new { role = "user", content = prompt }
                }
            };

            // Устанавливаем заголовки
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _apiKey);

            // Отправляем запрос
            var response = await _httpClient.PostAsJsonAsync("https://api.openai.com/v1/chat/completions", requestBody);

            // Проверяем успешность ответа
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Error while accessing OpenAI API: {error}");
            }

            // Читаем ответ
            var result = await response.Content.ReadFromJsonAsync<OpenAIResponse>();
            return result?.Choices?.FirstOrDefault()?.Message?.Content ?? "No answer from OpenAI.";
        }
    }
}