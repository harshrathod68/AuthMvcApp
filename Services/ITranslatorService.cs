using MyApps.Models;

namespace MyApps.Services
{
    /// <summary>
    /// Interface for Language Translation Service
    /// </summary>
    public interface ITranslatorService
    {
        /// <summary>
        /// Translate text from source language to target language
        /// </summary>
        Task<string> TranslateAsync(string text, string sourceLanguage, string targetLanguage);

        /// <summary>
        /// Get list of all supported languages
        /// </summary>
        List<LanguageInfo> GetSupportedLanguages();

        /// <summary>
        /// Get language name by code
        /// </summary>
        string GetLanguageName(string languageCode);
    }
}
