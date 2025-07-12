using System.Diagnostics;
using System.Reflection;
using System.Text.Json;
using _2chTyanAlert.Helpers;

namespace _2chTyanAlert.Service
{
    public class AlertService
    {
        private readonly Api2chService _api;

        private readonly GeminiApiService _geminiApiService;
        //private readonly double _catchRatePerDay;
        //private readonly IOpenAiService _openAi;

        public AlertService(Api2chService api, GeminiApiService geminiApiService, IConfiguration config)
        {
            _api = api;
            _geminiApiService = geminiApiService;
            //_catchRatePerDay = config.GetValue<double>("CatchRatePerDay");
            //_openAi = new OpenAiService(new OpenAiOptions { ApiKey = config["OpenAI:ApiKey"] });
        }

        public async Task PushNewFormsAsync(TimeSpan interval, string threadId)
        {
            var since = DateTime.UtcNow - interval;
            var posts = await _api.FetchPostsSinceAsync(threadId, since);
            var amount = posts.Count;

            var geminiPosts = posts.ToGeminiFilterPrompt();
            var result = await _geminiApiService.SummarizeAsync(geminiPosts);
            if (amount == 0) return;

            //var req = new ChatCompletionCreateRequest
            //{
            //    Model = Models.ChatGpt3_5Turbo,
            //    Messages = new[] { new ChatMessage("user", $"За интервал {interval} получено {amount} анкет.") }
            //};
            //await _openAi.ChatCompletion.Create(req);
        }
    }
}
