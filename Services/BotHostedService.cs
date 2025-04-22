using Telegram.Bot.Polling;
using Telegram.Bot;

namespace InsuranceBot.Services
{
    public class BotHostedService : IHostedService
    {
        private readonly ITelegramBotClient _bot;
        private readonly IServiceProvider _scopeFactory;

        public BotHostedService(ITelegramBotClient bot, IServiceProvider scopeFactory)
        {
            _bot = bot;
            _scopeFactory = scopeFactory;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _bot.StartReceiving(new DefaultUpdateHandler(
                async (botClient, update, token) =>
                {
                    using var scope = _scopeFactory.CreateScope();
                    var handler = scope.ServiceProvider.GetRequiredService<TelegramUpdateHandler>();
                    await handler.HandleUpdateAsync(botClient, update, token);
                },
                async (botClient, exception, token) => { /* log errors */ }),
                cancellationToken: cancellationToken);

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            // Отменяем получение обновлений через токен отмены
            Console.WriteLine("Bot is stopping...");
            cancellationToken.ThrowIfCancellationRequested();

            // Логируем завершение работы
            Console.WriteLine("Bot has stopped receiving updates.");

            return Task.CompletedTask;
        }
    }
}