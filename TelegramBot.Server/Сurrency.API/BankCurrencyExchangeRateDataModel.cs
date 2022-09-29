using System.Text.Json.Serialization;

namespace Сurrency.API
{
    public class BankCurrencyExchangeRateDataModel
    {
        [JsonPropertyName("date")]
        public string Date { get; set; }

        [JsonPropertyName("bank")]
        public string Bank { get; set; }

        [JsonPropertyName("baseCurrency")]
        public int BaseCurrency { get; set; }

        [JsonPropertyName("baseCurrencyLit")]
        public string BaseCurrencyLit { get; set; }

        [JsonPropertyName("exchangeRate")]
        public List<ExchangeRate> ExchangeRate { get; set; }
    }
    public class ExchangeRate
    {
        [JsonPropertyName("baseCurrency")]
        public string BaseCurrency { get; set; }

        [JsonPropertyName("currency")]
        public string Currency { get; set; }

        [JsonPropertyName("saleRateNB")]
        public double SaleRateNB { get; set; }

        [JsonPropertyName("purchaseRateNB")]
        public double PurchaseRateNB { get; set; }

        [JsonPropertyName("saleRate")]
        public double? SaleRate { get; set; }

        [JsonPropertyName("purchaseRate")]
        public double? PurchaseRate { get; set; }
    }
}
