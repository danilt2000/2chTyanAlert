using _2chTyanAlert.Helpers;
using _2chTyanAlert.Models;
using System.Text.Json;

namespace _2chTyanAlert.Service
{
    public class SchedulerService : BackgroundService
    {
        private readonly TimeSpan _interval;

        private readonly ILogger<SchedulerService> _logger;
        private readonly TelegramBotMessengerSender _telegramBotMessengerSender;
        private readonly GeminiApiService _geminiApiService;
        private readonly IServiceProvider _serviceProvider;

        public SchedulerService(ILogger<SchedulerService> logger, TelegramBotMessengerSender telegramBotMessengerSender, GeminiApiService geminiApiService, IServiceProvider serviceProvider, IConfiguration configuration)
        {
            _logger = logger;
            _telegramBotMessengerSender = telegramBotMessengerSender;
            _geminiApiService = geminiApiService;
            _serviceProvider = serviceProvider;
            var perDay = Math.Max(configuration.GetValue<double>("CatchRatePerDay"), 1);
            _interval = TimeSpan.FromHours(24 / perDay);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("SchedulerService is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Running SchedulerService at: {time}", DateTimeOffset.Now);

                try
                {
                    string CleanJson(string input)
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

                    using var scope = _serviceProvider.CreateScope();
                    var api2ChService = scope.ServiceProvider.GetRequiredService<Api2chService>();
                    var threadId = await api2ChService.ExtractSocThreadIdAsync();
                    var since = DateTime.UtcNow - _interval;
                    var posts = await api2ChService.FetchPostsSinceAsync(threadId, since);

                    var geminiPosts = posts.ToGeminiFilterPrompt();
                    var raw = await _geminiApiService.SummarizeAsync(geminiPosts);
                    var selected = JsonSerializer.Deserialize<List<SelectedPost>>(CleanJson(raw))!;
                    var finalPosts = posts
                        .Join(
                            selected,
                            p => p.Num.ToString(),
                            s => s.id,
                            (p, s) => new SocPost(
                                p.Num,
                                p.Comment,
                                p.Timestamp,
                                p.imageUrls,
                                s.score
                            )
                        )
                        .OrderByDescending(p => p.Score)
                        .ToList();

                    await _telegramBotMessengerSender.SendPostsAsync(finalPosts);
                    _logger.LogInformation("AlertService executed at {time}.", DateTimeOffset.Now);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while processing SchedulerService.");
                }

                await Task.Delay(_interval, stoppingToken);
            }
        }
    }
}
