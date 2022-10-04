using System.Text.RegularExpressions;
using Telegram.Bot;
using Telegram.Bot.Types;
using Сurrency.API;

namespace TelegramBot.API
{
    public class CurrencyRateTelegramBot
    {
        private TelegramBotClient _client;
        private string _token;
        private string _startMessage = "Добро пожаловать в CurrencyRateBot, для начала работы введите код валюты и дату на которую вы хотите получить информацию, в формате:\nUSD 26.08.2021";
        private string _instructionMessage = "Для продолжения работы введите код валюты и дату на которую вы хотите получить информацию, в формате:\nUSD 26.08.2021";
        private string _futureDateErrorMessage = $"Дата не может быть больше текущей";
        private string _inputErrorMessage = $"Введен некорректный запрос";
        private string _networkErrorMessage = $"Произошла сетевая ошибка, пожалуйста повторите запрос";
        private string _emptyDataMessage = $"Нет данных на запрашиваемую дату";
        private string _currencyCodeErrorMessage = $"Нет данных для указанной валюты или введен неверный код валюты";

        public CurrencyRateTelegramBot(string token)
        {
            if (string.IsNullOrEmpty(token)) throw new Exception("Invalid initial data");
            _token = token;          
        }

        public void Start()
        {
            _client = new TelegramBotClient(_token);
            _client.StartReceiving(Update, Error);
        }

        private Task Error(ITelegramBotClient arg1, Exception arg2, CancellationToken arg3)
        {
            throw new NotImplementedException();
        }

        private async Task Update(ITelegramBotClient botClient, Update update, CancellationToken token)
        {
            var message = update.Message;

            if(message != null)
            {
                if (message.Text.ToLower() == "/start")
                {
                    await SendStartMessage(botClient, message);
                    return;
                }

                if (message.Text.Split().Count() == 2)
                {
                    var enteredCurrencyCode = message.Text.Split()[0];
                    var enteredDate = message.Text.Split()[1];

                    if (Regex.IsMatch(enteredCurrencyCode, "^[A-Z]{3}$") && DateTime.TryParse(enteredDate, out DateTime date))
                    {
                        if (date > DateTime.Now)
                        {
                            await SendDateErrorMessage(botClient, message);
                            await SendInstructionMessage(botClient, message);
                            return;
                        }

                        var exchangeRates = await new CurrencyRate(date).GetExchangeRateAsync();

                        if (exchangeRates == null)
                        {
                            await SendNetworkErrorMessage(botClient, message);
                            await SendInstructionMessage(botClient, message);
                            return;
                        }

                        if (exchangeRates.Count == 0)
                        {
                            await SendEmptyDataMessage(botClient, message);
                            await SendInstructionMessage(botClient, message);
                            return;
                        }

                        if (exchangeRates.Exists(d => d.Currency.Contains(enteredCurrencyCode)))
                        {
                            await SendCurrencyRateMessage(botClient, message, enteredCurrencyCode, enteredDate, exchangeRates);
                            return;
                        }
                        else
                        {
                            await SendСurrencyCodeErrorMessage(botClient, message);
                            await SendInstructionMessage(botClient, message);
                            return;
                        }
                    }
                }

                await SendInputErrorMessage(botClient, message);
                await SendInstructionMessage(botClient, message);
            }
        }

        private static async Task SendCurrencyRateMessage(ITelegramBotClient botClient, Message? message, string enteredCurrencyCode, string enteredDate, List<ExchangeRate> exchangeRates)
        {
            var exchangeRate = exchangeRates.FirstOrDefault(d => d.Currency == enteredCurrencyCode);
            string exchangeRateMessage = $"Курс {enteredCurrencyCode} к UAH на {enteredDate}\n" +
            $"Покупка ПБ: {exchangeRate.PurchaseRate} \nПродажа ПБ: {exchangeRate.SaleRate}\n" +
            $"Покупка НБУ: {exchangeRate.PurchaseRateNB} \nПродажа НБУ: {exchangeRate.SaleRateNB}";
            await botClient.SendTextMessageAsync(message.Chat, exchangeRateMessage);
        }

        private async Task SendInputErrorMessage(ITelegramBotClient botClient, Message? message)
        {
            await botClient.SendTextMessageAsync(message.Chat, _inputErrorMessage);
        }

        private async Task SendInstructionMessage(ITelegramBotClient botClient, Message? message)
        {
            await botClient.SendTextMessageAsync(message.Chat, _instructionMessage);
        }

        private async Task SendDateErrorMessage(ITelegramBotClient botClient, Message? message)
        {
            await botClient.SendTextMessageAsync(message.Chat, _futureDateErrorMessage);
        }

        private async Task SendStartMessage(ITelegramBotClient botClient, Message? message)
        {
            await botClient.SendTextMessageAsync(message.Chat, _startMessage);
        }

        private async Task SendNetworkErrorMessage(ITelegramBotClient botClient, Message? message)
        {
            await botClient.SendTextMessageAsync(message.Chat, _networkErrorMessage);
        }

        private async Task SendEmptyDataMessage(ITelegramBotClient botClient, Message? message)
        {
            await botClient.SendTextMessageAsync(message.Chat, _emptyDataMessage);
        }

        private async Task SendСurrencyCodeErrorMessage(ITelegramBotClient botClient, Message? message)
        {
            await botClient.SendTextMessageAsync(message.Chat, _currencyCodeErrorMessage);
        }
    }
}