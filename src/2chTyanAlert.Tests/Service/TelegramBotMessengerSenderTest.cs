using _2chTyanAlert.Service;
using Microsoft.Extensions.Configuration;
using Moq.AutoMock;
using Xunit;

namespace _2chTyanAlert.Tests.Service
{
    public class TelegramBotMessengerSenderTest
    {
        public TelegramBotMessengerSender Sut { get; set; }

        public TelegramBotMessengerSenderTest()
        {
            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", false, true);

            var config = builder.Build();
            var mocker = new AutoMocker();

            mocker.Use<IConfiguration>(config);
            mocker.Use(new HttpClient());

            Sut = mocker.CreateInstance<TelegramBotMessengerSender>();
        }

        [Fact]
        public async Task SendMessageAsyncTest()
        {
            //await Sut.SendMessageAsync("Test", "Test");
        }
    }
}
