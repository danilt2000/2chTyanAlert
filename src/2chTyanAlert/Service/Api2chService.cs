using System.Text.Json;
using _2chTyanAlert.Models;

namespace _2chTyanAlert.Service
{
    public class Api2chService
    {
        private readonly HttpClient _httpClient;
        private const string BaseUri = "https://2ch.hk";


        public Api2chService(IConfiguration config)
        {
            _httpClient = new HttpClient();
        }

        public async Task<string> ExtractSocThreadIdAsync()
        {
            var json = await _httpClient.GetStringAsync($"{BaseUri}/soc/index.json");
            using var doc = JsonDocument.Parse(json);
            var threadNum = doc.RootElement
                .GetProperty("threads")[1]
                .GetProperty("thread_num")
                .GetInt32();

            return threadNum.ToString();
        }

        public async Task<string> FetchThreadJsonAsync(string threadId)
        {
            return await _httpClient.GetStringAsync($"{BaseUri}/soc/res/{threadId}.json");
        }

        //public async Task<IReadOnlyList<SocPost>> FetchPostsSinceAsync(string threadId, DateTime utcSince)
        //{
        //    var json = await FetchThreadJsonAsync(threadId);
        //    using var doc = JsonDocument.Parse(json);

        //    var postsElement = doc.RootElement.TryGetProperty("posts", out var p)
        //        ? p
        //        : doc.RootElement.GetProperty("threads")[0].GetProperty("posts");

        //    return postsElement.EnumerateArray()//TODO ADD IMAGE EXTRACTION
        //        .Select(el => new SocPost(
        //            el.GetProperty("num").GetInt32(),
        //            el.GetProperty("comment").GetString() ?? string.Empty,
        //            el.GetProperty("timestamp").GetInt64()))
        //        .Where(post => DateTimeOffset.FromUnixTimeSeconds(post.Timestamp).UtcDateTime >= utcSince)
        //        .ToList();
        //}
        public async Task<IReadOnlyList<SocPost>> FetchPostsSinceAsync(string threadId, DateTime utcSince)
        {
            var json = await FetchThreadJsonAsync(threadId);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            var postsElement = root.TryGetProperty("posts", out var p)
                ? p
                : root.GetProperty("threads")[0].GetProperty("posts");

            return postsElement.EnumerateArray()
                .Select(el =>
                {
                    var num = el.GetProperty("num").GetInt32();
                    var comment = el.GetProperty("comment").GetString() ?? string.Empty;
                    var timestamp = el.GetProperty("timestamp").GetInt64();

                    var imageUrls = el.TryGetProperty("files", out var filesEl) && filesEl.ValueKind == JsonValueKind.Array
                        ? filesEl.EnumerateArray()
                            .Select(f => BaseUri + (f.GetProperty("path").GetString() ?? ""))
                            .ToList()
                        : new List<string>();

                    return new SocPost(num, comment, timestamp, imageUrls, null);
                })
                .Where(post => DateTimeOffset
                    .FromUnixTimeSeconds(post.Timestamp)
                    .UtcDateTime >= utcSince)
                .ToList();
        }
    }
}
