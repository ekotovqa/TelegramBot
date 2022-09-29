using Telegram.Bot;
using Telegram.Bot.Types;
using Сurrency.API;

namespace TelegramBot.API
{
    public class CurrencyRateTelegramBot
    {
        private TelegramBotClient _client  { get; set; }
        private string Token { get; set; }
        private string _startMessage = "Добро пожаловать в CurrencyRateBot, для начала работы введите дату на которую вы хотите получить информацию в формате дд/мм/гггг";
        private string _retryMessage = "Для продолжения работы введите дату на которую вы хотите получить информацию в формате дд/мм/гггг";
        private string SelectedDate { get; set; } = null;
        private List<string> AvailableCurrencies { get; set; } = new List<string>();
        public CurrencyRateTelegramBot(string token)
        {
            if (string.IsNullOrEmpty(token)) throw new Exception("Invalid initial data");
            Token = token;          
        }

        public void Start()
        {
            _client = new TelegramBotClient(Token);
            _client.StartReceiving(Update, Error);
        }

        private Task Error(ITelegramBotClient arg1, Exception arg2, CancellationToken arg3)
        {
            throw new NotImplementedException();
        }

        private async Task Update(ITelegramBotClient botClient, Update update, CancellationToken token)
        {
            var message = update.Message;

            if (message.Text.ToLower() == "/start")
            {
                await botClient.SendTextMessageAsync(message.Chat, _startMessage);
                return;
            }

            if (DateTime.TryParse(message.Text, out DateTime dateTime)) 
            {
                if(dateTime > DateTime.Now)
                {
                    await botClient.SendTextMessageAsync(message.Chat, $"Дата не может быть больше текущей.\n{_retryMessage}");
                    return;
                }
                SelectedDate = message.Text;
                AvailableCurrencies =  await CurrencyRate.GetListAvailableCurrencies(SelectedDate);
                if(AvailableCurrencies.Count == 0)
                {
                    await botClient.SendTextMessageAsync(message.Chat, $"Для выбранной вами даты, данные не доступны.\n{_retryMessage}");
                    return;
                }
                AvailableCurrencies.RemoveAll(d => d == null);
                string responseMessage = "Выберите интересующую вас валюту: ";
                foreach (var currency in AvailableCurrencies)
                {
                    responseMessage += $"{currency}, ";
                }
                responseMessage = responseMessage.Remove(responseMessage.Length - 2, 2);
                await botClient.SendTextMessageAsync(message.Chat, responseMessage);
                return;
            }


            if (AvailableCurrencies.Exists(d => d.Contains(message.Text.ToUpper())))
            {
                var selectedCurrency = message.Text.ToUpper();
                var currencyRate = new CurrencyRate(SelectedDate, selectedCurrency);
                var exchangeRate = await currencyRate.GetExchangeRateAsync();
                string exchangeRateMessage = $"Курс UAH к {selectedCurrency} на {SelectedDate}\n" +
                    $"Покупка ПБ: {exchangeRate.PurchaseRate} \nПродажа ПБ: {exchangeRate.SaleRate}\n" +
                    $"Покупка НБУ: {exchangeRate.PurchaseRateNB} \nПродажа НБУ: {exchangeRate.SaleRateNB}";
                await botClient.SendTextMessageAsync(message.Chat, exchangeRateMessage);
                SelectedDate = null;
                AvailableCurrencies = new List<string>();
                return;
            }
            else if(AvailableCurrencies.Count > 0)
            {
                string responseMessage = "Выберите интересующую вас валюту из списка: ";
                foreach (var currency in AvailableCurrencies)
                {
                    responseMessage += $"{currency}, ";
                }
                responseMessage = responseMessage.Remove(responseMessage.Length - 2, 2);
                await botClient.SendTextMessageAsync(message.Chat, responseMessage);
                return;
            }

            if (message.Text.ToLower() != "/start")
            {
                await botClient.SendTextMessageAsync(message.Chat, _retryMessage);
                return;
            }
        }
    }
}