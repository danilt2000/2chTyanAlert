using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using _2chTyanAlert.Service;
using Microsoft.Extensions.Configuration;
using Moq.AutoMock;
using Xunit;

namespace _2chTyanAlert.Tests.Service
{
    public class Api2ChServiceTests
    {
        public Api2chService Sut { get; set; }

        public Api2ChServiceTests()
        {
            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", false, true);

            var config = builder.Build();
            var mocker = new AutoMocker();

            mocker.Use(config);
            mocker.Use(new HttpClient());

            Sut = mocker.CreateInstance<Api2chService>();
        }

        [Fact]
        public async Task ExtractSocThreadIdTest()
        {
            var result = await Sut.ExtractSocThreadIdAsync();

            Assert.NotNull(result);
        }

        [Fact]
        public async Task FetchThreadJsonTest()
        {
            var threadId = await Sut.ExtractSocThreadIdAsync();

            var result = await Sut.FetchThreadJsonAsync(threadId);

            Assert.NotNull(result);
        }
    }
}
