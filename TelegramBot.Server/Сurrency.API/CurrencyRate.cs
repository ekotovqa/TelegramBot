using System.Text.Json;

namespace Сurrency.API
{
    public class CurrencyRate
    {
        private static readonly HttpClient _client;

        public string Date { get; set; }
        public string Currency { get; set; }

        public CurrencyRate(string date, string currency)
        {
            if (string.IsNullOrEmpty(date) || string.IsNullOrEmpty(currency)) throw new Exception("Invalid initial data");
            Date = date;
            Currency = currency;
        }

        static CurrencyRate()
        {
            _client = new HttpClient()
            {
                BaseAddress = new Uri($"https://api.privatbank.ua")
            };
        }

        public async Task<ExchangeRate> GetExchangeRateAsync()
        {
            var url = $"/p24api/exchange_rates?json&date={Date}";
            var result = new ExchangeRate();
            using (var response = await _client.GetAsync(url))
                if (response.IsSuccessStatusCode)
                {
                    var stringResponse = await response.Content.ReadAsStringAsync();
                    var rawResult = JsonSerializer.Deserialize<BankCurrencyExchangeRateDataModel>(stringResponse);
                    foreach (var exchangeRate in rawResult.ExchangeRate)
                    {
                        if (exchangeRate.Currency == Currency) return exchangeRate;
                    }
                }
                else
                {
                    await File.AppendAllTextAsync("log.txt", $"{DateTime.Now} {response.ReasonPhrase}");
                    return null;
                }
            return result;
        }

        public static async Task<List<string>> GetListAvailableCurrencies(string date)
        {
            var url = $"/p24api/exchange_rates?json&date={date}";
            var result = new List<string>();
            using (var response = await _client.GetAsync(url))
                if (response.IsSuccessStatusCode)
                {
                    var stringResponse = await response.Content.ReadAsStringAsync();
                    var rawResult = JsonSerializer.Deserialize<BankCurrencyExchangeRateDataModel>(stringResponse);
                    foreach (var exchangeRate in rawResult.ExchangeRate)
                    {
                        result.Add(exchangeRate.Currency);
                    }
                }
                else
                {
                    await File.AppendAllTextAsync("log.txt", $"{DateTime.Now} {response.ReasonPhrase}");
                    return null;
                }
            return result;
        }
    }
}