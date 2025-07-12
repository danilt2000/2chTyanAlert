using _2chTyanAlert.Models;
using System.Text;
using System.Text.Json;

namespace _2chTyanAlert.Service
{
    public class TelegramBotMessengerSender
    {
        private readonly string _botToken;
        private readonly string _chatId;

        public TelegramBotMessengerSender(IConfiguration configuration)
        {
            _botToken = configuration.GetValue<string>("TelegramBotToken")!;
            _chatId = configuration.GetValue<string>("TelegramChatId")!;
        }

        public async Task SendPostsAsync(IEnumerable<SocPost> posts)
        {
            using var client = new HttpClient();
            foreach (var post in posts)
            {
                if (post.imageUrls == null || !post.imageUrls.Any())
                {
                    // Если картинок нет — просто текст
                    await SendTextOnlyAsync(client, post);
                }
                else if (post.imageUrls.Count == 1)
                {
                    // Одна картинка
                    await SendSinglePhotoAsync(client, post, post.imageUrls[0]);
                }
                else
                {
                    // Несколько картинок
                    await SendMediaGroupAsync(client, post, post.imageUrls);
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
            await PostJsonAsync(client, url, payload);
        }

        private async Task SendMediaGroupAsync(HttpClient client, SocPost post, List<string> urls)
        {
            // в массиве media только первый элемент получает caption
            var media = urls
                .Select((u, i) => new Dictionary<string, object>
                {
                    ["type"] = "photo",
                    ["media"] = u,
                    // caption только для первого изображения
                    [i == 0 ? "caption" : ""] = i == 0 ? EscapeMarkdown(BuildCaption(post)) : null,
                    [i == 0 ? "parse_mode" : ""] = i == 0 ? "MarkdownV2" : null
                })
                // отфильтровываем пустые ключи
                .Select(d => d.Where(kv => !string.IsNullOrEmpty(kv.Key))
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
            // Форматируем: номер, очки и исходный HTML-комментарий
            // Если в Comment есть теги <a>, Telegram их не отобразит под MarkdownV2,
            // поэтому либо экранируем, либо предварительно конвертим в plain text.
            return $"Post #{post.Num} (score: {post.Score})\n{post.Comment}";
        }

        private static async Task PostJsonAsync(HttpClient client, string url, object payload)
        {
            var json = JsonSerializer.Serialize(payload);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");
            var resp = await client.PostAsync(url, content);
            resp.EnsureSuccessStatusCode();
        }

        private static string EscapeMarkdown(string text)
        {
            if (string.IsNullOrEmpty(text)) return "";
            var builder = new StringBuilder(text.Length);
            foreach (var ch in text)
            {
                // список символов, обязательных к экранированию в MarkdownV2
                if ("_*[]()~`>#+-=|{}.!".Contains(ch))
                    builder.Append('\\');
                builder.Append(ch);
            }
            return builder.ToString();
        }
    }
}
