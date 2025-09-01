using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;
using System.Text.RegularExpressions;

namespace RM.ChatGPT
{
    public class StockInsightService
    {
        private readonly HttpClient _httpClient;
        private const string OpenAiApiKey = "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";
        private const string OpenAiUrl = "https://api.openai.com/v1/chat/completions";

        public StockInsightService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<string>> GetDailyStockInsightsAsync()
        {
            //var messages = new[]
            //{
            //    new { role = "system", content = "You are a stock analyst focused on Indian markets (NSE/BSE). Only include news that may impact these markets today." },
            //    new { role = "user", content = "Provide top 5 market-moving news items for NSE/BSE today. Format the response with numbered points like:\n1. First item\n2. Second item\n3. Third item.\nAvoid using bullets or dashes." }
            //};


            var messages = new[]
            {
                new
                {
                    role = "system",
                    content = "You are a financial news summarizer focused on the Indian stock market (NSE/BSE). Your task is to create concise, impactful one-liner summaries of the latest 2–3 days of market-moving news. Only include news that may influence stock prices in India."
                },
                new
                {
                    role = "user",
                    content = "Based on the latest Indian stock market news, generate one-liner summaries suitable for social media reels and headlines.\n" +
                              "Each summary must:\n" +
                              "- Be short and impactful (max 50 words).\n" +
                              "- Focus on events like block deals, new orders, capex, policy changes, earnings, corporate exits, or frauds.\n" +
                              "- Be numbered like:\n1. First item\n2. Second item\n3. Third item.\n" +
                              "Avoid bullets, titles, or extra text. Only output the numbered list of summaries."
                }
            };



            var requestBody = new
            {
                model = "gpt-3.5-turbo", // change this from gpt-4
                messages = messages,
                temperature = 0.7
            };

            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", OpenAiApiKey);

            var response = await _httpClient.PostAsync(OpenAiUrl,
                new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json"));

            var jsonString = await response.Content.ReadAsStringAsync();
            var doc = JsonDocument.Parse(jsonString);
            var content = doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            //            string content = @"1. Indian government announces new stimulus measures to support the economy, boosting investor sentiment
            //2. Top IT company reports stronger-than-expected quarterly earnings, lifting tech sector stocks
            //3. RBI governor's speech hints at further monetary policy easing to spur economic growth";

            var matches = Regex.Matches(content, @"\d+\.\s*(.+?)(?=\n\d+\.|\z)", RegexOptions.Singleline);
            var list = matches.Select(m => m.Groups[1].Value.Trim()).ToList();


            return list;
        }
    }

}
