
# InsuranceBot

## 🚀 Setup Instructions

1. **Clone the repository:**

```bash
git clone https://github.com/yourusername/InsuranceBot.git
cd InsuranceBot
```

2. **Configure environment variables or `appsettings.json`** with the following:
   - `Telegram:BotToken`
   - `OpenAI:ApiKey`
   - `Mindee:ApiKey` 

3. **Build and run the application:**

```bash
dotnet build
dotnet run
```

---

## 📜 Bot Workflow

1. The user sends the `/start` command.
2. The bot replies with a greeting message generated using OpenAI.
3. The user selects which document to send first:

   - 📄 **Driver's license** – real scan processing via Mindee.
   - 🚗 **Vehicle document** – mock processing.

4. After receiving each photo:
   - The bot saves the file.
   - Extracts data using Mindee.
   - Stores the result in memory.

5. After both documents are received:
   - The bot confirms the recognized data with the user.
   - Upon user confirmation, generates an insurance policy using OpenAI.
   - Sends the policy to the user as a text file.

---

## 🧩 Technologies Used

- .NET 8.0
- Telegram.Bot
- Mindee API
- OpenAI API (GPT)

---

## 🔗 Live Bot Link

[Try the bot on Telegram](https://t.me/InsuranceCar11_bot)
