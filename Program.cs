using InsuranceBot.Interfaces;
using InsuranceBot.Services;
using Mindee;
using Telegram.Bot;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

// Configure TelegramBotClient as Singleton
builder.Services.AddSingleton<ITelegramBotClient>(provider =>
{
    var token = config["Telegram:BotToken"];
    return new TelegramBotClient(token);
});

builder.Services.AddSingleton<MindeeClient>(provider =>
{
    var apiKey = config["Mindee:ApiKey"];
    return new MindeeClient(apiKey);
});

builder.Services.AddHttpClient<IMindeeService, MindeeService>();

// Core services
builder.Services.AddScoped<IMindeeService, MindeeService>();
builder.Services.AddSingleton<IOpenAIService>(provider =>
{
    var config = provider.GetRequiredService<IConfiguration>();
    var apiKey = config["OpenAI:ApiKey"];
    var httpClient = provider.GetRequiredService<IHttpClientFactory>().CreateClient();
    return new OpenAIService(apiKey, httpClient);
});
builder.Services.AddScoped<TelegramUpdateHandler>();

// Hosted service for bot
builder.Services.AddHostedService<BotHostedService>();

// Add controllers and Swagger
builder.Services.AddControllers();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwagger();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();