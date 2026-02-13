namespace MyApps.Models
{
    /// <summary>
    /// Model for Language Translator feature
    /// </summary>
    public class TranslatorModel
    {
        public string SourceText { get; set; }
        public string TranslatedText { get; set; }
        public string SourceLanguage { get; set; }
        public string TargetLanguage { get; set; }
        public string SourceLanguageName { get; set; }
        public string TargetLanguageName { get; set; }
    }

    /// <summary>
    /// Language information model
    /// </summary>
    public class LanguageInfo
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string Flag { get; set; }
    }
}
