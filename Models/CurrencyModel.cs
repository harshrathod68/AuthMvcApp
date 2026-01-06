using System.ComponentModel.DataAnnotations;

namespace AuthMvcApp.Models
{
    /// <summary>
    /// Model for currency conversion input
    /// </summary>
    public class CurrencyConversionModel
    {
        [Required(ErrorMessage = "Amount is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        [Display(Name = "Amount")]
        public decimal Amount { get; set; } = 1;

        [Required(ErrorMessage = "Please select source currency")]
        [Display(Name = "From Currency")]
        public string FromCurrency { get; set; } = "USD";

        [Required(ErrorMessage = "Please select target currency")]
        [Display(Name = "To Currency")]
        public string ToCurrency { get; set; } = "INR";
    }

    /// <summary>
    /// Model for currency conversion result
    /// </summary>
    public class CurrencyResultModel
    {
        public decimal OriginalAmount { get; set; }
        public string FromCurrency { get; set; } = string.Empty;
        public string ToCurrency { get; set; } = string.Empty;
        public decimal ConvertedAmount { get; set; }
        public decimal ExchangeRate { get; set; }
        public DateTime LastUpdated { get; set; }
    }

    /// <summary>
    /// Static class containing supported currencies with their details
    /// </summary>
    public static class SupportedCurrencies
    {
        public static readonly Dictionary<string, CurrencyInfo> Currencies = new()
        {
            { "USD", new CurrencyInfo("USD", "US Dollar", "$", "🇺🇸") },
            { "EUR", new CurrencyInfo("EUR", "Euro", "€", "🇪🇺") },
            { "GBP", new CurrencyInfo("GBP", "British Pound", "£", "🇬🇧") },
            { "INR", new CurrencyInfo("INR", "Indian Rupee", "₹", "🇮🇳") },
            { "JPY", new CurrencyInfo("JPY", "Japanese Yen", "¥", "🇯🇵") },
            { "AUD", new CurrencyInfo("AUD", "Australian Dollar", "A$", "🇦🇺") },
            { "CAD", new CurrencyInfo("CAD", "Canadian Dollar", "C$", "🇨🇦") },
            { "CHF", new CurrencyInfo("CHF", "Swiss Franc", "Fr", "🇨🇭") },
            { "CNY", new CurrencyInfo("CNY", "Chinese Yuan", "¥", "🇨🇳") },
            { "AED", new CurrencyInfo("AED", "UAE Dirham", "د.إ", "🇦🇪") },
            { "SAR", new CurrencyInfo("SAR", "Saudi Riyal", "﷼", "🇸🇦") },
            { "SGD", new CurrencyInfo("SGD", "Singapore Dollar", "S$", "🇸🇬") },
            { "NZD", new CurrencyInfo("NZD", "New Zealand Dollar", "NZ$", "🇳🇿") },
            { "BRL", new CurrencyInfo("BRL", "Brazilian Real", "R$", "🇧🇷") },
            { "RUB", new CurrencyInfo("RUB", "Russian Ruble", "₽", "🇷🇺") },
            { "ZAR", new CurrencyInfo("ZAR", "South African Rand", "R", "🇿🇦") },
            { "MXN", new CurrencyInfo("MXN", "Mexican Peso", "$", "🇲🇽") },
            { "KRW", new CurrencyInfo("KRW", "South Korean Won", "₩", "🇰🇷") },
            { "THB", new CurrencyInfo("THB", "Thai Baht", "฿", "🇹🇭") },
            { "MYR", new CurrencyInfo("MYR", "Malaysian Ringgit", "RM", "🇲🇾") }
        };
    }

    /// <summary>
    /// Currency information model
    /// </summary>
    public class CurrencyInfo
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string Symbol { get; set; }
        public string Flag { get; set; }

        public CurrencyInfo(string code, string name, string symbol, string flag)
        {
            Code = code;
            Name = name;
            Symbol = symbol;
            Flag = flag;
        }
    }
}
