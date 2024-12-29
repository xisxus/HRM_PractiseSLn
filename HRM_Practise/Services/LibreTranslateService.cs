using System.Text;
using System.Text.Json;
using Azure.Core;
using Newtonsoft.Json;

namespace HRM_Practise.Services
{
    public class LibreTranslateService
    {
        private readonly HttpClient _httpClient;

        public LibreTranslateService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string> TranslateTextAsync(string text, string targetLang)
        {
            //var requestContent = new
            //{
            //    //q = text,
            //    //source = "en", // Default source language (you can change it based on user preference)
            //    //target = targetLang

            //    q = text,
            //    target = targetLang
            //};

            //var content = new StringContent(JsonConvert.SerializeObject(requestContent), Encoding.UTF8, "application/json");

            //var response = await _httpClient.PostAsync("https://libretranslate.com/translate", content);


            //var response = await _httpClient.PostAsJsonAsync("https://libretranslate.com/translate", requestContent);

            var translateApiUrl = "https://libretranslate.com/translate";
            var jsonContent = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(new 
            { 
                q = text, 
                source = "auto", 
                target = targetLang, 
                format = "text", 
                alternatives = 3, 
                api_key = "" 
            }),  Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(translateApiUrl, jsonContent); 
            var responseBody = await response.Content.ReadAsStringAsync();




            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsStringAsync();
                // Parse the translated text from the response
                using var jsonDoc = JsonDocument.Parse(result);
                return jsonDoc.RootElement.GetProperty("translatedText").GetString();
            }

            return "Translation failed";  // In case the API fails
        }
    }
}
