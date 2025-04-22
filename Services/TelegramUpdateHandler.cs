using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;
using Telegram.Bot;
using InsuranceBot.Models;
using InsuranceBot.Interfaces;
using Telegram.Bot.Types.ReplyMarkups;

namespace InsuranceBot.Services
{
    public class TelegramUpdateHandler
    {
        private readonly IMindeeService _mindee;
        private readonly IOpenAIService _openAI;
        private readonly static Dictionary<long, List<string>> _userDocuments = new();
        private static readonly Dictionary<long, string> _userExpected = new();


        public TelegramUpdateHandler(IMindeeService mindee, IOpenAIService openAI)
        {
            _mindee = mindee;
            _openAI = openAI;
        }

        public async Task HandleUpdateAsync(ITelegramBotClient bot, Update update, CancellationToken ct)
        {

            if (update.Type == UpdateType.Message && update.Message is { } msg)
            {
                var chatId = msg.Chat.Id;

                if (msg.Text == "/start")
                {
                    //Приветствие через OpenAI
                    string greeting;
                    try
                    {
                        greeting = await _openAI.SendMessageAndGetResponse(
                            "Напиши українською коротке привітальне повідомлення бота, "
                          + "який допомагає з оформленням автостраховки. Без питань, без слова «привіт».");
                    }
                    catch
                    {
                        greeting = "Я допоможу вам швидко оформити автостраховку.";
                    }

                    await bot.SendMessage(chatId, greeting, cancellationToken: ct);

                    //Кнопки выбора
                    await bot.SendMessage(
                        chatId,
                        "Будь ласка, відправте спочатку фото посвідчення водія",
                        replyMarkup: new InlineKeyboardMarkup(new[]
                        {
                    new[] { InlineKeyboardButton.WithCallbackData("Посвідчення водія",  "send_driver") },
                    new[] { InlineKeyboardButton.WithCallbackData("Документ на авто",   "send_vehicle") }
                        }),
                        cancellationToken: ct);
                    return;
                }

                //Oбработка фото
                if (msg.Photo != null && _userExpected.TryGetValue(chatId, out var docType))
                {
                    //Сохраняем файл
                    var fileId = msg.Photo.Last().FileId;
                    var tgFile = await bot.GetFile(fileId, ct);
                    var dir = Path.Combine("uploads", chatId.ToString());
                    Directory.CreateDirectory(dir);
                    var path = Path.Combine(dir, $"{tgFile.FileId}.jpg");

                    await using (var fs = File.OpenWrite(path))
                        await bot.DownloadFile(tgFile.FilePath, fs, ct);

                    //Сохраняем путь
                    if (!_userDocuments.ContainsKey(chatId))
                        _userDocuments[chatId] = new List<string>();
                    _userDocuments[chatId].Add(path);
                    _userExpected.Remove(chatId);             //Ожидание погашено

                    //Ждём второй документ?
                    if (_userDocuments[chatId].Count == 1)
                    {
                        var ask = await _openAI.SendMessageAndGetResponse(
                            docType == "DriverLicense"
                            ? "Скажи українською: Фото посвідчення водія отримано. Тепер надішліть фото документа на автомобіль."
                            : "Скажи українською: Фото документа на автомобіль отримано. Тепер надішліть фото посвідчення водія.");
                        await bot.SendMessage(chatId, ask, cancellationToken: ct);

                        //Устанавливаем ожидание второго типа
                        _userExpected[chatId] = docType == "DriverLicense" ? "VehicleDocument" : "DriverLicense";
                        return;
                    }

                    //Oба фото получены
                    var processing = await _openAI.SendMessageAndGetResponse(
                        "Скажи українською: Фото документів отримано. Обробляю дані…");
                    await bot.SendMessage(chatId, processing, cancellationToken: ct);

                    var passport = await _mindee.ExtractAsync(_userDocuments[chatId][0], "DriverLicense");
                    var vehicle = await _mindee.ExtractAsync(_userDocuments[chatId][1], "VehicleDocument");

                    var data = new DocumentData
                    {
                        Name = passport.Name,
                        LastName = passport.LastName,
                        DriverLicenceId = passport.DriverLicenceId,
                        Vin = vehicle.Vin
                    };

                    var confirm = await _openAI.SendMessageAndGetResponse(
                        $"Сформуй українською ввічливе повідомлення з проханням підтвердити дані:" +
                        $"\nІм'я: {data.Name}" +
                        $"\nПрізвище: {data.LastName}" +
                        $"\nНомер посвідчення водія: {data.DriverLicenceId}"+
                        $"\nVIN: {data.Vin}. Після цього пункту нічого більше не пиши");

                    await bot.SendMessage(
                        chatId,
                        confirm,
                        replyMarkup: new InlineKeyboardMarkup(new[]
                        {
                    new[] {
                        InlineKeyboardButton.WithCallbackData("Підтвердити", "confirm"),
                        InlineKeyboardButton.WithCallbackData("Переслати документи", "resubmit")
                    }
                        }),
                        cancellationToken: ct);

                    _userDocuments.Remove(chatId); //Очистили
                    return;
                }
            }

            //CallbackQuery
            if (update.Type == UpdateType.CallbackQuery && update.CallbackQuery is { } cq)
            {
                var chatId = cq.Message.Chat.Id;

                switch (cq.Data)
                {
                    case "send_driver":
                        _userExpected[chatId] = "DriverLicense";
                        await bot.SendMessage(chatId,
                            "Будь ласка, надішліть фото вашого посвідчення водія.",
                            cancellationToken: ct);
                        break;

                    case "send_vehicle":
                        _userExpected[chatId] = "VehicleDocument";
                        await bot.SendMessage(chatId,
                            "Будь ласка, надішліть фото документа на автомобіль.",
                            cancellationToken: ct);
                        break;

                    case "confirm":
                        var priceAsk = await _openAI.SendMessageAndGetResponse(
                            "Скажи українською: Дані підтверджені. Вартість страховки — 100 $. Ви згодні?");
                        await bot.SendMessage(chatId, priceAsk,
                            replyMarkup: new InlineKeyboardMarkup(new[]
                            {
                        new[] {
                            InlineKeyboardButton.WithCallbackData("Згоден",   "agree"),
                            InlineKeyboardButton.WithCallbackData("Не згоден", "disagree")
                        }
                            }),
                            cancellationToken: ct);
                        break;

                    case "resubmit":
                        await bot.SendMessage(chatId,
                            "Будь ласка, надішліть фото документів ще раз.",
                            cancellationToken: ct);
                        break;

                    case "agree":
                        await bot.SendMessage(chatId,
                            "Дякую за підтвердження! Генерую ваш страховий поліс…",
                            cancellationToken: ct);

                        var policyText = await _openAI.SendMessageAndGetResponse(
                            "Створи українською текст страхового поліса на 1 рік, ціна 100 $, "
                          + "підстав отримані дані cтрахувальника та сьогоднішню дату.");

                        var dir = Path.Combine("uploads", chatId.ToString());
                        Directory.CreateDirectory(dir);
                        var policyPath = Path.Combine(dir, "InsurancePolicy.txt");
                        await File.WriteAllTextAsync(policyPath, policyText, ct);

                        await using (var fs = File.OpenRead(policyPath))
                            await bot.SendDocument(chatId,
                                new InputFileStream(fs, "InsurancePolicy.txt"),
                                caption: "Ваш страховий поліс готовий!",
                                cancellationToken: ct);
                        break;

                    case "disagree":
                        var refuse = await _openAI.SendMessageAndGetResponse(
                            "Поясни українською: Вибачте, але вартість страховки фіксована — 100 $. "
                          + "Якщо передумаєте, напишіть мені ще раз. Або натисніть кнопку згоден, якщо передумали.");
                        await bot.SendMessage(chatId, refuse, cancellationToken: ct);
                        break;
                }
            }
        }
    }
}