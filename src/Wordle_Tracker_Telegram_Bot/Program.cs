using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Wordle_Tracker_Telegram_Bot;
using Wordle_Tracker_Telegram_Bot.Data;
using Wordle_Tracker_Telegram_Bot.Services;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using HealthChecks.UI.Client;
using Wordle_Tracker_Telegram_Bot.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

var botConfig = builder.Configuration.GetSection("BotConfiguration").Get<BotConfiguration>();

builder.Services.AddHttpClient("tgwebhook")
                .AddTypedClient<ITelegramBotClient>(httpClient => new TelegramBotClient(botConfig.BotToken, httpClient));

builder.Services.AddDbContext<DatabaseContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("PostgreSQL"));
    options.EnableSensitiveDataLogging();
    options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
}, ServiceLifetime.Transient);

builder.Services.AddHostedService<ConfigureWebhook>();

builder.Services.AddScoped<HandleUpdateService>();
builder.Services.AddScoped<IGameService, GameService>();
builder.Services.AddScoped<IDatabaseRepository, DatabaseRepository>();

// Health Checks
//builder.Services.AddHealthChecks()
//    .AddNpgSql(builder.Configuration.GetConnectionString("PostgreSQL"), tags: new[] { "database" })
//    .AddCheck<ServerHealthCheck>("ServerHealthCheck", tags: new[] { "custom" });
//builder.Services.AddHealthChecksUI().AddInMemoryStorage();

// swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddControllers().AddNewtonsoftJson();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();

app.UseAuthorization();

//app.MapHealthChecksUI();
//app.MapHealthChecks("/health/custom", new HealthCheckOptions
//{
//    Predicate = reg => reg.Tags.Contains("custom"),
//    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
//});

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute(
        name: "tgwebhook",
        pattern: $"bot/{botConfig.BotToken}",
        new { controller = "Webhook", action = "Post" }
    );

    endpoints.MapControllers();
});

app.Run();
