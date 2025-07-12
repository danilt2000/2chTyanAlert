using System.Text.Json;
using _2chTyanAlert.Service;
using GenerativeAI;
using Microsoft.Extensions.Configuration;
using Moq.AutoMock;
using Xunit;

namespace _2chTyanAlert.Tests.Service
{
    public class GeminiApiServiceTests
    {
        public GeminiApiService Sut { get; set; }

        public GeminiApiServiceTests()
        {
            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", false, true);

            var config = builder.Build();
            var mocker = new AutoMocker();

            mocker.Use(config);

            var apiKey = config["Gemini:ApiKey"];
            if (string.IsNullOrWhiteSpace(apiKey))
                throw new InvalidOperationException("Gemini:ApiKey not configured. Set env GEMINI_API_KEY or appsettings.integration.json");

            var model = new GenerativeModel(apiKey, "gemini-2.0-flash");

            mocker.Use<IGenerativeModel>(model);

            Sut = mocker.CreateInstance<GeminiApiService>();
        }

        [Fact]
        public async Task SummarizeAsync_ReturnsExactTest()
        {
            const string schema =
                @"{""type"":""object"",""properties"":{""answer"":{""type"":""string""}},""required"":[""answer""]}";

            var prompt =
                $"""
                 Reply with a JSON object matching this schema — no markdown, no triple back-ticks:

                 {schema}

                 The “answer” value must be exactly the lowercase word "test".
                 """;

            var raw = await Sut.SummarizeAsync(prompt);

            using var doc = JsonDocument.Parse(raw);
            var answer = doc.RootElement.GetProperty("answer").GetString();

            Assert.Equal("test", answer);
        }
    }
}
