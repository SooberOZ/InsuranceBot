using dotenv.net;
using InsuranceBot.Interfaces;
using InsuranceBot.Services;
using Mindee;
using Telegram.Bot;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;
DotEnv.Load(options: new DotEnvOptions(envFilePaths: new[] { "Keys.env" }));

string botToken = Environment.GetEnvironmentVariable("TELEGRAM_BOT_TOKEN")!;
string openAiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY")!;
string mindeeKey = Environment.GetEnvironmentVariable("MINDEE_API_KEY")!;
string mindeeEndpoint = Environment.GetEnvironmentVariable("MINDEE_ENDPOINT")!;

// Configure TelegramBotClient as Singleton
builder.Services.AddSingleton<ITelegramBotClient>(provider =>
{
    return new TelegramBotClient(botToken);
});

builder.Services.AddSingleton<MindeeClient>(provider =>
{
    return new MindeeClient(mindeeKey);
});

builder.Services.AddHttpClient<IMindeeService, MindeeService>();

// Core services
builder.Services.AddScoped<IMindeeService, MindeeService>();
builder.Services.AddSingleton<IOpenAIService>(provider =>
{
    var config = provider.GetRequiredService<IConfiguration>();
    var apiKey = openAiKey;
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