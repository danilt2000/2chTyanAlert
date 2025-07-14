using _2chTyanAlert.Models;
using System.Text;
using System.Text.Json;

namespace _2chTyanAlert.Service
{
    public class TelegramBotMessengerSender(IConfiguration configuration)
    {
        private readonly string _botToken = configuration.GetValue<string>("TelegramBotToken")!;
        private readonly string _chatId = configuration.GetValue<string>("TelegramChatId")!;

        public async Task SendPostsAsync(IEnumerable<SocPost> posts)
        {
            using var client = new HttpClient();
            foreach (var post in posts)
            {
                try
                {
                    if (post.imageUrls == null || !post.imageUrls.Any())
                        await SendTextOnlyAsync(client, post);
                    else if (post.imageUrls.Count == 1)
                        await SendSinglePhotoAsync(client, post, post.imageUrls[0]);
                    else
                        await SendMediaGroupAsync(client, post, post.imageUrls);
                }
                catch (Exception ex)
                {
                    await Console.Error.WriteLineAsync($"[Error] post #{post.Num}: {ex.GetType().Name}: {ex.Message}");
                }
            }
        }

        private async Task SendTextOnlyAsync(HttpClient client, SocPost post)
        {
            var caption = BuildCaption(post);
            var payload = new
            {
                chat_id = _chatId,
                text = EscapeMarkdown(caption),
                parse_mode = "MarkdownV2"
            };
            var url = $"https://api.telegram.org/bot{_botToken}/sendMessage";
            await PostJsonAsync(client, url, payload);
        }

        private async Task SendSinglePhotoAsync(HttpClient client, SocPost post, string photoUrl)
        {
            var caption = BuildCaption(post);
            var payload = new
            {
                chat_id = _chatId,
                photo = photoUrl,
                caption = EscapeMarkdown(caption),
                parse_mode = "MarkdownV2"
            };
            var url = $"https://api.telegram.org/bot{_botToken}/sendPhoto";

            try
            {
                await PostJsonAsync(client, url, payload);
            }
            catch (HttpRequestException ex)
            {
                await Console.Error.WriteLineAsync($"[BadRequest] sendPhoto failed: {ex.Message}");
                var photoOnly = new { chat_id = _chatId, photo = photoUrl };
                await PostJsonAsync(client, url, photoOnly);

                await SendTextOnlyAsync(client, post);
            }
        }

        private async Task SendMediaGroupAsync(HttpClient client, SocPost post, List<string> urls)
        {
            var media = urls
                .Select((u, i) => new Dictionary<string, object>
                {
                    ["type"] = "photo",
                    ["media"] = u,
                    ["caption"] = i == 0 ? EscapeMarkdown(BuildCaption(post)) : null!,
                    ["parse_mode"] = i == 0 ? "HTML" : null!
                })
                .Select(d => d.Where(kv => kv.Value != null)
                              .ToDictionary(kv => kv.Key, kv => kv.Value!))
                .ToArray();

            var payload = new
            {
                chat_id = _chatId,
                media = media
            };
            var url = $"https://api.telegram.org/bot{_botToken}/sendMediaGroup";
            await PostJsonAsync(client, url, payload);
        }

        private static string BuildCaption(SocPost post)
        {
            return $"Post #{post.Num} (score: {post.Score})\n{post.Comment}";
        }

        private static async Task PostJsonAsync(HttpClient client, string url, object payload)
        {
            var json = JsonSerializer.Serialize(payload);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");
            var resp = await client.PostAsync(url, content);
            var body = await resp.Content.ReadAsStringAsync();

            if (!resp.IsSuccessStatusCode)
            {
                throw new HttpRequestException(
                    $"HTTP {(int)resp.StatusCode} ({resp.StatusCode}) from Telegram. " +
                    $"Response body: {body}"
                );
            }
        }

        private static string EscapeMarkdown(string text)
        {
            if (string.IsNullOrEmpty(text)) return "";
            var sb = new StringBuilder(text.Length);
            foreach (var ch in text)
            {
                if ("_*[]()~`>#+\\-=|{}.!".Contains(ch))
                    sb.Append('\\');
                sb.Append(ch);
            }
            return sb.ToString();
        }
    }
}
