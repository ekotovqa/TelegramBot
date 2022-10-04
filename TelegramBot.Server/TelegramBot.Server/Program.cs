using TelegramBot.API;
using System.Configuration;

CurrencyRateTelegramBot telegramBot = new CurrencyRateTelegramBot(ConfigurationManager.AppSettings["token"]);
telegramBot.Start();
Console.ReadLine();
