using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using YourMotivatorRobot.GroqService;

const string prompt = "Give me a motivational quote for today. Try to not be duplicated";

var botToken = Environment.GetEnvironmentVariable("TELEGRAM_BOT_TOKEN");

var channelChatId = Environment.GetEnvironmentVariable("CHANNEL_CHAT_ID");

if (string.IsNullOrWhiteSpace(botToken) || string.IsNullOrWhiteSpace(channelChatId))
    throw new InvalidOperationException("TELEGRAM_BOT_TOKEN and CHANNEL_CHAT_ID must be set");

using var httpClient = new HttpClient
{
    Timeout = TimeSpan.FromSeconds(90)
};

var groqClient = new GroqClient(httpClient);

var bot = new TelegramBotClient(botToken);

string quote = await groqClient.GetResponseAsync(prompt);

await bot.SendMessage(channelChatId, quote, ParseMode.Html);