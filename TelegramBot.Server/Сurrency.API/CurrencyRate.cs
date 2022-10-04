using System.Text.Json;
using Serilog;

namespace Сurrency.API
{
    public class CurrencyRate
    {
        private static readonly HttpClient _client = new HttpClient
        {
            BaseAddress = new Uri($"https://api.privatbank.ua")
        };
        public DateTime Date { get; set; }
        private const string _url = $"/p24api/exchange_rates?json&date=";
        private const string _errorLogPath = "log.txt";
        private static Serilog.Core.Logger _logger = new LoggerConfiguration().WriteTo.File(_errorLogPath).CreateLogger();

        public CurrencyRate(DateTime date)
        {
            Date = date;
        }

        public async Task<List<ExchangeRate>> GetExchangeRateAsync()
        {
            var result = new List<ExchangeRate>();
            using (var response = await _client.GetAsync(_url + Date))
                if (response.IsSuccessStatusCode)
                {
                    var stringResponse = await response.Content.ReadAsStringAsync();
                    result = JsonSerializer.Deserialize<BankCurrencyExchangeRateDataModel>(stringResponse).ExchangeRate;
                    result.RemoveAll(d => d == null);
                    result.RemoveAll(d => d.Currency == null);
                    return result;
                }
                else
                {
                    _logger.Error(response.ToString());
                    return null;
                }
        }
    }
}