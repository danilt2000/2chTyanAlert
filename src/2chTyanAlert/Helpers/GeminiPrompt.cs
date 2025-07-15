using System.Text.Encodings.Web;
using System.Text.Json;
using _2chTyanAlert.Models;

namespace _2chTyanAlert.Helpers;

public static class GeminiPrompt
{
    private const string Head = @"Ты — эксперт-фильтр анкет.

Вход  
• JSON-массив объектов формата  
  {  
    ""id"": ""<уникальный_идентификатор>"",  
    ""text"": ""<оригинальный_пост_целиком>""  
  }

Задача  
1. Для каждого объекта определи пол автора (женский / мужской / неясно) по содержимому, кун = мужской, тян = женский.  
2. Сохрани только женские анкеты.  
3. Оцени каждую из них по критериям пригодности (0 – 100 баллов).

Критерии пригодности (примерно равные веса)  
- Автор: старше 18 лет и не старше 25 лет.  
- Нравятся манга или аниме.  
- Увлекается IT / Machine Learning / backend / frontend-разработкой.  
- Стеснительная / интроверт (слова «стесн», «интроверт», «скромн» и т.п.).  
- Играет в видеоигры.  
- Любит читать книги.  
- Девственница (ключевые слова: «девственна», «не было секса», «нет интимного опыта»).
- Пониженный вес.
- Ищет куна.

Алгоритм оценки  
• Начни с 0.  
• За каждый выполненный критерий прибавь ~12–15 баллов, но за «девственница» ~40 баллов.  
• Округляй до целого.

Формат вывода  
[
  { ""id"": ""<id_анкеты>"", ""score"": <целое_0-100> }
]

⚠️ Только валидный JSON без пояснений.

";

    public static string ToGeminiFilterPrompt(this IEnumerable<SocPost> posts)
    {
        var json = JsonSerializer.Serialize(
            posts.Select(p => new
            {
                id = p.Num.ToString(),
                text = p.Comment.Replace('\n', ' ').Replace('\r', ' ')
            }),
            new JsonSerializerOptions
            {
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            });

        return Head + json;
    }
}