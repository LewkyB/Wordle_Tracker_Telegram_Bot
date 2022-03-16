using Telegram.Bot;
using Wordle_Tracker_Telegram_Bot;
using Wordle_Tracker_Telegram_Bot.Services;

var builder = WebApplication.CreateBuilder(args);

var botConfig = builder.Configuration.GetSection("BotConfiguration").Get<BotConfiguration>();

builder.Services.AddHttpClient("tgwebhook")
                .AddTypedClient<ITelegramBotClient>(httpClient => new TelegramBotClient(botConfig.BotToken, httpClient));

builder.Services.AddHostedService<ConfigureWebhook>();

builder.Services.AddScoped<HandleUpdateService>();

builder.Services.AddControllers().AddNewtonsoftJson();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
}

app.UseRouting();

app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute(
        name: "tgwebhook",
        pattern: $"bot/{botConfig.BotToken}",
        new { controller = "Webhook", action = "Post" }
    );

    endpoints.MapControllers();
});

app.MapControllers();

app.Run();
