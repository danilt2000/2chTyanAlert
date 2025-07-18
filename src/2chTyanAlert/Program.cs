
using _2chTyanAlert.Service;
using GenerativeAI;

namespace _2chTyanAlert
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddHostedService<SchedulerService>();
            builder.Services.AddScoped<Api2chService>();
            builder.Services.AddSingleton<IGenerativeModel>(_ =>
            {
                var apiKey = builder.Configuration["Gemini:ApiKey"];
                return new GenerativeModel(apiKey!, "gemini-2.0-flash-lite");
            });
            builder.Services.AddSingleton<GeminiApiService>();
            builder.Services.AddSingleton<TelegramBotMessengerSender>();
            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
