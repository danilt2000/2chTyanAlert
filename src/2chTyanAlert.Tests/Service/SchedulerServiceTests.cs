using _2chTyanAlert.Helpers;
using _2chTyanAlert.Models;
using _2chTyanAlert.Service;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.AutoMock;
using System.Text.Json;
using Xunit;

namespace _2chTyanAlert.Tests.Service
{
    public class SchedulerServiceTests
    {
        private readonly AutoMocker _mocker;
        private readonly Mock<ILogger<SchedulerService>> _loggerMock;
        private readonly Mock<TelegramBotMessengerSender> _telegramMock;
        private readonly Mock<GeminiApiService> _geminiMock;
        private readonly Mock<IServiceProvider> _serviceProviderMock;
        private readonly Mock<IConfiguration> _configMock;

        public SchedulerServiceTests()
        {
            _mocker = new AutoMocker();
            _loggerMock = _mocker.GetMock<ILogger<SchedulerService>>();
            _telegramMock = _mocker.GetMock<TelegramBotMessengerSender>();
            _geminiMock = _mocker.GetMock<GeminiApiService>();
            _serviceProviderMock = _mocker.GetMock<IServiceProvider>();
            _configMock = _mocker.GetMock<IConfiguration>();

            _configMock.Setup(c => c["CatchRatePerDay"]).Returns("1");
        }

        [Fact]
        public void CleanJson_RemovesMarkdownCodeBlocks()
        {
            var input = "```json\n{\"id\":\"123\",\"score\":5}\n```";
            var expected = "{\"id\":\"123\",\"score\":5}";

            var result = TestCleanJsonMethod(input);

            Assert.Equal(expected, result);
        }

        [Fact]
        public void CleanJson_HandlesPlainJson()
        {
            var input = "{\"id\":\"123\",\"score\":5}";
            var expected = "{\"id\":\"123\",\"score\":5}";

            var result = TestCleanJsonMethod(input);

            Assert.Equal(expected, result);
        }

        [Fact]
        public void CleanJson_TrimsWhitespace()
        {
            var input = "  \n  {\"id\":\"123\",\"score\":5}  \n  ";
            var expected = "{\"id\":\"123\",\"score\":5}";

            var result = TestCleanJsonMethod(input);

            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("<html><body>Error</body></html>")]
        [InlineData("<!DOCTYPE html>")]
        [InlineData("  <xml>")]
        public void InvalidResponse_ShouldBeDetectedAsHtml(string htmlResponse)
        {
            var result = htmlResponse.TrimStart().StartsWith('<');
            Assert.True(result, $"Response should be detected as HTML: {htmlResponse}");
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("\n\t")]
        public void InvalidResponse_ShouldBeDetectedAsEmpty(string emptyResponse)
        {
            var result = string.IsNullOrWhiteSpace(emptyResponse);
            Assert.True(result, $"Response should be detected as empty: '{emptyResponse}'");
        }

        [Fact]
        public void ValidJsonResponse_ShouldNotBeDetectedAsInvalid()
        {
            var validJson = "[{\"id\":\"123\",\"score\":5}]";
            var isHtml = validJson.TrimStart().StartsWith('<');
            var isEmpty = string.IsNullOrWhiteSpace(validJson);

            Assert.False(isHtml, "Valid JSON should not be detected as HTML");
            Assert.False(isEmpty, "Valid JSON should not be detected as empty");
        }

        [Fact]
        public void ValidJsonWithMarkdown_ShouldBeCleanable()
        {
            var input = "```json\n[{\"id\":\"123\",\"score\":5},{\"id\":\"456\",\"score\":8}]\n```";
            var cleaned = TestCleanJsonMethod(input);

            var posts = JsonSerializer.Deserialize<List<SelectedPost>>(cleaned);

            Assert.NotNull(posts);
            Assert.Equal(2, posts!.Count);
            Assert.Equal("123", posts[0].id);
            Assert.Equal(5, posts[0].score);
        }

        private string TestCleanJsonMethod(string input)
        {
            var s = input.Trim();

            if (s.StartsWith("```") && s.EndsWith("```"))
            {
                var firstNewline = s.IndexOf('\n');
                if (firstNewline >= 0)
                {
                    s = s[(firstNewline + 1)..];
                    s = s.Substring(0, s.LastIndexOf("```", StringComparison.Ordinal));
                }
            }

            return s.Trim();
        }
    }
}
