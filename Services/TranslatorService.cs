using MyApps.Models;
using System.Net.Http;
using System.Text.Json;

namespace MyApps.Services
{
    /// <summary>
    /// Language Translation Service using Google Translate API
    /// </summary>
    public class TranslatorService : ITranslatorService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<TranslatorService> _logger;

        // Supported languages with flags
        private static readonly List<LanguageInfo> SupportedLanguages = new()
        {
            new LanguageInfo { Code = "en", Name = "English", Flag = "🇺🇸" },
            new LanguageInfo { Code = "hi", Name = "हिंदी", Flag = "🇮🇳" },
            new LanguageInfo { Code = "bn", Name = "বাংলা", Flag = "🇮🇳" },
            new LanguageInfo { Code = "ta", Name = "தமிழ்", Flag = "🇮🇳" },
            new LanguageInfo { Code = "te", Name = "తెలుగు", Flag = "🇮🇳" },
            new LanguageInfo { Code = "mr", Name = "मराठी", Flag = "🇮🇳" },
            new LanguageInfo { Code = "gu", Name = "ગુજરાતી", Flag = "🇮🇳" },
            new LanguageInfo { Code = "kn", Name = "ಕನ್ನಡ", Flag = "🇮🇳" },
            new LanguageInfo { Code = "ml", Name = "മലയാളം", Flag = "🇮🇳" },
            new LanguageInfo { Code = "pa", Name = "ਪੰਜਾਬੀ", Flag = "🇮🇳" },
            new LanguageInfo { Code = "ar", Name = "العربية", Flag = "🇸🇦" },
            new LanguageInfo { Code = "zh-CN", Name = "中文 (简体)", Flag = "🇨🇳" },
            new LanguageInfo { Code = "zh-TW", Name = "中文 (繁體)", Flag = "🇹🇼" },
            new LanguageInfo { Code = "ja", Name = "日本語", Flag = "🇯🇵" },
            new LanguageInfo { Code = "ko", Name = "한국어", Flag = "🇰🇷" },
            new LanguageInfo { Code = "fr", Name = "Français", Flag = "🇫🇷" },
            new LanguageInfo { Code = "de", Name = "Deutsch", Flag = "🇩🇪" },
            new LanguageInfo { Code = "es", Name = "Español", Flag = "🇪🇸" },
            new LanguageInfo { Code = "it", Name = "Italiano", Flag = "🇮🇹" },
            new LanguageInfo { Code = "pt", Name = "Português", Flag = "🇵🇹" },
            new LanguageInfo { Code = "ru", Name = "Русский", Flag = "🇷🇺" },
            new LanguageInfo { Code = "pl", Name = "Polski", Flag = "🇵🇱" },
            new LanguageInfo { Code = "tr", Name = "Türkçe", Flag = "🇹🇷" },
            new LanguageInfo { Code = "th", Name = "ไทย", Flag = "🇹🇭" },
            new LanguageInfo { Code = "vi", Name = "Tiếng Việt", Flag = "🇻🇳" },
            new LanguageInfo { Code = "id", Name = "Bahasa Indonesia", Flag = "🇮🇩" },
            new LanguageInfo { Code = "ms", Name = "Bahasa Melayu", Flag = "🇲🇾" },
            new LanguageInfo { Code = "fil", Name = "Filipino", Flag = "🇵🇭" },
            new LanguageInfo { Code = "uk", Name = "Українська", Flag = "🇺🇦" },
            new LanguageInfo { Code = "cs", Name = "Čeština", Flag = "🇨🇿" },
            new LanguageInfo { Code = "nl", Name = "Nederlands", Flag = "🇳🇱" },
            new LanguageInfo { Code = "sv", Name = "Svenska", Flag = "🇸🇪" },
            new LanguageInfo { Code = "da", Name = "Dansk", Flag = "🇩🇰" },
            new LanguageInfo { Code = "no", Name = "Norsk", Flag = "🇳🇴" },
            new LanguageInfo { Code = "fi", Name = "Suomi", Flag = "🇫🇮" },
            new LanguageInfo { Code = "el", Name = "Ελληνικά", Flag = "🇬🇷" },
            new LanguageInfo { Code = "he", Name = "עברית", Flag = "🇮🇱" },
            new LanguageInfo { Code = "hu", Name = "Magyar", Flag = "🇭🇺" },
            new LanguageInfo { Code = "ro", Name = "Română", Flag = "🇷🇴" },
            new LanguageInfo { Code = "sk", Name = "Slovenčina", Flag = "🇸🇰" }
        };

        public TranslatorService(HttpClient httpClient, ILogger<TranslatorService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        /// <summary>
        /// Translate text using Google Translate API
        /// </summary>
        public async Task<string> TranslateAsync(string text, string sourceLanguage, string targetLanguage)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(text))
                    return string.Empty;

                // Google Translate API endpoint
                string url = $"https://translate.googleapis.com/translate_a/element.js?cb=googleTranslateElementInit";
                
                // Using free Google Translate endpoint (no API key required)
                string apiUrl = $"https://api.mymemory.translated.net/get?q={Uri.EscapeDataString(text)}&langpair={sourceLanguage}|{targetLanguage}";

                var response = await _httpClient.GetAsync(apiUrl);
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var jsonDoc = JsonDocument.Parse(content);
                    
                    if (jsonDoc.RootElement.TryGetProperty("responseData", out var responseData))
                    {
                        if (responseData.TryGetProperty("translatedText", out var translatedText))
                        {
                            return translatedText.GetString() ?? text;
                        }
                    }
                }

                _logger.LogWarning($"Translation failed for text: {text}");
                return text;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Translation error: {ex.Message}");
                return text;
            }
        }

        /// <summary>
        /// Get all supported languages
        /// </summary>
        public List<LanguageInfo> GetSupportedLanguages()
        {
            return SupportedLanguages.OrderBy(l => l.Name).ToList();
        }

        /// <summary>
        /// Get language name by code
        /// </summary>
        public string GetLanguageName(string languageCode)
        {
            var language = SupportedLanguages.FirstOrDefault(l => l.Code == languageCode);
            return language?.Name ?? languageCode;
        }
    }
}
