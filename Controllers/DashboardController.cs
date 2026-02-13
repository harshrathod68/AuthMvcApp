/*
 * =====================================================
 * DashboardController.cs - Main Dashboard Controller
 * =====================================================
 * 
 * Ye controller dashboard ke sab features handle karta hai:
 * - Weather (Mausam)
 * - Currency Converter (Paisa convert)
 * - Time Zone Converter (Time convert)
 * - Country Information (Desh ki jaankari)
 * - News (Samachar)
 * 
 * Har feature ke liye 2 methods hain:
 * 1. GET method - Page dikhata hai (empty form)
 * 2. POST method - Form submit hone par data fetch karta hai
 * 
 * Author: Harsh Rathod
 * =====================================================
 */

using Microsoft.AspNetCore.Mvc;
using MyApps.Services;
using MyApps.Models;

namespace MyApps.Controllers
{
    public class DashboardController : Controller
    {
        // ========== SERVICES (Dependency Injection) ==========
        // Ye services Program.cs mein register hain
        // Constructor mein automatically inject hoti hain
        private readonly IWeatherService _weatherService;     // Weather API
        private readonly ICurrencyService _currencyService;   // Currency API
        private readonly ITimeZoneService _timeZoneService;   // Time conversion
        private readonly ICountryService _countryService;     // Country API
        private readonly INewsService _newsService;           // News API
        private readonly ITranslatorService _translatorService; // Language Translator
        private readonly IEmergencyService _emergencyService; // Emergency Numbers
        private readonly IHolidayService _holidayService;     // Public Holidays
        private readonly ITimeTrackService _timeTrackService; // Time Track
        private readonly IRolePermissionService _rolePermissionService; // Role Permissions

        // Constructor - Jab controller banta hai tab services milti hain
        public DashboardController(
            IWeatherService weatherService, 
            ICurrencyService currencyService,
            ITimeZoneService timeZoneService,
            ICountryService countryService,
            INewsService newsService,
            ITranslatorService translatorService,
            IEmergencyService emergencyService,
            IHolidayService holidayService,
            ITimeTrackService timeTrackService,
            IRolePermissionService rolePermissionService)
        {
            _weatherService = weatherService;
            _currencyService = currencyService;
            _timeZoneService = timeZoneService;
            _countryService = countryService;
            _newsService = newsService;
            _translatorService = translatorService;
            _emergencyService = emergencyService;
            _holidayService = holidayService;
            _timeTrackService = timeTrackService;
            _rolePermissionService = rolePermissionService;
        }

        // ========================================
        // DASHBOARD HOME PAGE
        // ========================================
        // URL: /Dashboard or /Dashboard/Index
        public IActionResult Index()
        {
            // Step 1: Check karo user logged in hai ya nahi
            if (!IsAuthenticated()) 
                return RedirectToAction("Login", "Account");
            
            // Step 2: User ka naam aur role ViewBag mein daalo
            ViewBag.UserName = HttpContext.Session.GetString("UserName");
            var userRole = HttpContext.Session.GetString("UserRole") ?? "User";
            ViewBag.UserRole = userRole;
            
            // Step 3: Get allowed apps for user's role
            var rolePermission = _rolePermissionService.GetRolePermission(userRole);
            ViewBag.AllowedApps = rolePermission?.AllowedApps ?? new List<string>();
            
            // Step 4: Dashboard page dikhao
            return View();
        }

        // ========================================
        // WEATHER FEATURE (Mausam)
        // ========================================
        #region Weather
        
        // GET: /Dashboard/Weather - Weather page dikhao
        [HttpGet]
        public IActionResult Weather()
        {
            if (!IsAuthenticated()) 
                return RedirectToAction("Login", "Account");
            
            ViewBag.UserName = HttpContext.Session.GetString("UserName");
            
            // Empty form dikhao
            return View(new WeatherSearchModel());
        }

        // POST: /Dashboard/Weather - City search karne par
        [HttpPost]
        public async Task<IActionResult> Weather(WeatherSearchModel model)
        {
            if (!IsAuthenticated()) 
                return RedirectToAction("Login", "Account");
            
            ViewBag.UserName = HttpContext.Session.GetString("UserName");

            // Step 1: City name check karo
            if (string.IsNullOrWhiteSpace(model.City))
            {
                ViewBag.Error = "Please enter a city name";
                return View(model);
            }

            // Step 2: Weather API call karo (async = wait karo response ke liye)
            var weather = await _weatherService.GetWeatherAsync(model.City);
            
            // Step 3: Agar city nahi mili
            if (weather == null)
            {
                ViewBag.Error = "City not found. Please check the city name.";
                return View(model);
            }

            // Step 4: Weather data ViewBag mein daalo
            ViewBag.Weather = weather;
            return View(model);
        }
        #endregion

        // ========================================
        // CURRENCY CONVERTER (Paisa Convert)
        // ========================================
        #region Currency
        
        // GET: /Dashboard/Currency - Currency page dikhao
        [HttpGet]
        public IActionResult Currency()
        {
            if (!IsAuthenticated()) 
                return RedirectToAction("Login", "Account");
            
            ViewBag.UserName = HttpContext.Session.GetString("UserName");
            
            // Default values set karo (USD to INR, amount 1)
            return View(new CurrencyConversionModel 
            { 
                Amount = 1, 
                FromCurrency = "USD", 
                ToCurrency = "INR" 
            });
        }

        // POST: /Dashboard/Currency - Convert karne par
        [HttpPost]
        public async Task<IActionResult> Currency(CurrencyConversionModel model)
        {
            if (!IsAuthenticated()) 
                return RedirectToAction("Login", "Account");
            
            ViewBag.UserName = HttpContext.Session.GetString("UserName");

            // Form validation
            if (!ModelState.IsValid) 
                return View(model);

            // Same currency check
            if (model.FromCurrency == model.ToCurrency)
            {
                ViewBag.Error = "Please select different currencies";
                return View(model);
            }

            // Currency API call
            var result = await _currencyService.ConvertCurrencyAsync(
                model.Amount, 
                model.FromCurrency, 
                model.ToCurrency
            );
            
            if (result == null)
            {
                ViewBag.Error = "Unable to fetch exchange rates.";
                return View(model);
            }

            ViewBag.Result = result;
            return View(model);
        }
        #endregion

        // ========================================
        // TIME ZONE CONVERTER (Time Convert)
        // ========================================
        #region TimeZone
        
        // GET: /Dashboard/TimeZone - TimeZone page dikhao
        [HttpGet]
        public IActionResult TimeZone()
        {
            if (!IsAuthenticated()) 
                return RedirectToAction("Login", "Account");
            
            ViewBag.UserName = HttpContext.Session.GetString("UserName");
            
            // Default values: Current time, India to USA
            return View(new TimeZoneConversionModel
            {
                Time = DateTime.Now.ToString("HH:mm"),
                Date = DateTime.Now.ToString("yyyy-MM-dd"),
                FromTimeZone = "India Standard Time",
                ToTimeZone = "Eastern Standard Time"
            });
        }

        // POST: /Dashboard/TimeZone - Convert karne par
        [HttpPost]
        public IActionResult TimeZone(TimeZoneConversionModel model)
        {
            if (!IsAuthenticated()) 
                return RedirectToAction("Login", "Account");
            
            ViewBag.UserName = HttpContext.Session.GetString("UserName");

            if (!ModelState.IsValid) 
                return View(model);

            // Same timezone check
            if (model.FromTimeZone == model.ToTimeZone)
            {
                ViewBag.Error = "Please select different time zones";
                return View(model);
            }

            // Time parse karo (string se TimeSpan mein)
            if (!TimeSpan.TryParse(model.Time, out TimeSpan timeSpan))
            {
                ViewBag.Error = "Invalid time format";
                return View(model);
            }

            // Time convert karo
            var result = _timeZoneService.ConvertTime(
                DateTime.Today.Add(timeSpan), 
                model.FromTimeZone, 
                model.ToTimeZone
            );
            
            if (result == null)
            {
                ViewBag.Error = "Unable to convert time.";
                return View(model);
            }

            ViewBag.Result = result;
            return View(model);
        }
        #endregion

        // ========================================
        // COUNTRY INFORMATION (Desh ki Jaankari)
        // ========================================
        #region Country
        
        // GET: /Dashboard/Country - Country page dikhao
        [HttpGet]
        public async Task<IActionResult> Country()
        {
            if (!IsAuthenticated()) 
                return RedirectToAction("Login", "Account");
            
            ViewBag.UserName = HttpContext.Session.GetString("UserName");
            
            // Default value "India" set karo aur automatically load karo
            var model = new CountrySearchModel { Country = "India" };
            
            // India ki information automatically load karo
            var countryInfo = await _countryService.GetCountryInfoAsync("India");
            if (countryInfo != null)
            {
                ViewBag.CountryInfo = countryInfo;
            }
            
            return View(model);
        }

        // POST: /Dashboard/Country - Search karne par
        [HttpPost]
        public async Task<IActionResult> Country(CountrySearchModel model)
        {
            if (!IsAuthenticated()) 
                return RedirectToAction("Login", "Account");
            
            ViewBag.UserName = HttpContext.Session.GetString("UserName");

            if (string.IsNullOrWhiteSpace(model.Country))
            {
                ViewBag.Error = "Please enter a country name";
                return View(model);
            }

            // Country API call
            var countryInfo = await _countryService.GetCountryInfoAsync(model.Country);
            
            if (countryInfo == null)
            {
                ViewBag.Error = "Country not found.";
                return View(model);
            }

            ViewBag.CountryInfo = countryInfo;
            return View(model);
        }
        #endregion

        // ========================================
        // NEWS FEATURE (Samachar)
        // ========================================
        #region News
        
        // GET: /Dashboard/News - News page dikhao
        // Query parameters: country, category, language, page
        [HttpGet]
        public async Task<IActionResult> News(
            string country = "India",    // Default country
            string category = "",        // Category filter
            string language = "en",      // Language (en = English)
            int page = 1)                // Page number for pagination
        {
            if (!IsAuthenticated()) 
                return RedirectToAction("Login", "Account");
            
            ViewBag.UserName = HttpContext.Session.GetString("UserName");

            // Model banao with current values
            var model = new NewsSearchModel 
            { 
                Country = country, 
                Category = category, 
                Language = language, 
                Page = page 
            };
            
            // Search query banao
            var searchQuery = country;
            if (!string.IsNullOrEmpty(category)) 
                searchQuery += " " + category;
            
            // News API call
            var articles = await _newsService.SearchNewsAsync(searchQuery, language, page);
            
            ViewBag.Articles = articles;
            ViewBag.HasMore = articles.Count >= 9; // Agar 9 articles hain to aur bhi ho sakte hain
            return View(model);
        }

        // POST: /Dashboard/News - Search karne par
        [HttpPost]
        public async Task<IActionResult> News(NewsSearchModel model)
        {
            if (!IsAuthenticated()) 
                return RedirectToAction("Login", "Account");
            
            ViewBag.UserName = HttpContext.Session.GetString("UserName");

            List<NewsArticle> articles;
            var searchQuery = "";
            
            // Search query decide karo
            if (!string.IsNullOrWhiteSpace(model.SearchQuery))
            {
                // Direct search query use karo
                searchQuery = model.SearchQuery;
            }
            else if (!string.IsNullOrWhiteSpace(model.Country))
            {
                // Country + category use karo
                searchQuery = model.Country;
                if (!string.IsNullOrEmpty(model.Category)) 
                    searchQuery += " " + model.Category;
            }

            // News fetch karo
            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                articles = await _newsService.SearchNewsAsync(searchQuery, model.Language, model.Page);
            }
            else
            {
                // Default: India ki top headlines
                articles = await _newsService.GetTopHeadlinesAsync("in", null, model.Language, model.Page);
            }

            if (!articles.Any())
            {
                ViewBag.Error = "No more news found.";
            }

            ViewBag.Articles = articles;
            ViewBag.HasMore = articles.Count >= 9;
            ViewBag.SearchQuery = searchQuery;
            return View(model);
        }
        #endregion

        // ========================================
        // LANGUAGE TRANSLATOR (Bhasha Anuvaad)
        // ========================================
        #region Translator
        
        // GET: /Dashboard/Translator - Translator page dikhao
        [HttpGet]
        public IActionResult Translator()
        {
            if (!IsAuthenticated()) 
                return RedirectToAction("Login", "Account");
            
            ViewBag.UserName = HttpContext.Session.GetString("UserName");
            
            // Supported languages list ViewBag mein daalo
            ViewBag.Languages = _translatorService.GetSupportedLanguages();
            
            // Default values: English to Hindi
            return View(new TranslatorModel 
            { 
                SourceLanguage = "en", 
                TargetLanguage = "hi",
                SourceLanguageName = "English",
                TargetLanguageName = "हिंदी"
            });
        }

        // POST: /Dashboard/Translator - Translate karne par
        [HttpPost]
        public async Task<IActionResult> Translator(TranslatorModel model)
        {
            if (!IsAuthenticated()) 
                return RedirectToAction("Login", "Account");
            
            ViewBag.UserName = HttpContext.Session.GetString("UserName");
            ViewBag.Languages = _translatorService.GetSupportedLanguages();

            // Validation
            if (string.IsNullOrWhiteSpace(model.SourceText))
            {
                ViewBag.Error = "Please enter text to translate";
                return View(model);
            }

            if (model.SourceLanguage == model.TargetLanguage)
            {
                ViewBag.Error = "Please select different languages";
                return View(model);
            }

            // Translate karo
            var translatedText = await _translatorService.TranslateAsync(
                model.SourceText, 
                model.SourceLanguage, 
                model.TargetLanguage
            );

            // Language names set karo
            model.TargetLanguageName = _translatorService.GetLanguageName(model.TargetLanguage);
            model.SourceLanguageName = _translatorService.GetLanguageName(model.SourceLanguage);
            model.TranslatedText = translatedText;

            return View(model);
        }

        // API: /Dashboard/TranslateText - Live translation ke liye
        [HttpPost]
        public async Task<IActionResult> TranslateText([FromBody] TranslatorModel model)
        {
            if (!IsAuthenticated()) 
                return Json(new { success = false, error = "Not authenticated" });

            if (string.IsNullOrWhiteSpace(model.SourceText))
                return Json(new { success = false, error = "No text provided" });

            if (model.SourceLanguage == model.TargetLanguage)
                return Json(new { success = false, error = "Same language selected" });

            try
            {
                var translatedText = await _translatorService.TranslateAsync(
                    model.SourceText, 
                    model.SourceLanguage, 
                    model.TargetLanguage
                );

                return Json(new { success = true, translatedText = translatedText });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }
        #endregion

        // ========================================
        // EMERGENCY NUMBERS (Aapatkaaleen Nambar)
        // ========================================
        #region Emergency
        
        // GET: /Dashboard/Emergency - Emergency page dikhao
        [HttpGet]
        public async Task<IActionResult> Emergency()
        {
            if (!IsAuthenticated()) 
                return RedirectToAction("Login", "Account");
            
            ViewBag.UserName = HttpContext.Session.GetString("UserName");
            
            // Default value "India" set karo aur automatically load karo
            var model = new EmergencySearchModel { Country = "India" };
            
            // India ki emergency numbers automatically load karo
            var emergencyNumbers = await _emergencyService.GetEmergencyNumbersAsync("India");
            if (emergencyNumbers != null)
            {
                ViewBag.EmergencyNumbers = emergencyNumbers;
            }
            
            return View(model);
        }

        // POST: /Dashboard/Emergency - Search karne par
        [HttpPost]
        public async Task<IActionResult> Emergency(EmergencySearchModel model)
        {
            if (!IsAuthenticated()) 
                return RedirectToAction("Login", "Account");
            
            ViewBag.UserName = HttpContext.Session.GetString("UserName");

            if (string.IsNullOrWhiteSpace(model.Country))
            {
                ViewBag.Error = "Please enter a country name";
                return View(model);
            }

            // Emergency API call
            var emergencyNumbers = await _emergencyService.GetEmergencyNumbersAsync(model.Country);
            
            if (emergencyNumbers == null)
            {
                ViewBag.Error = "Country not found or no emergency data available.";
                return View(model);
            }

            ViewBag.EmergencyNumbers = emergencyNumbers;
            return View(model);
        }
        #endregion

        // ========================================
        // PUBLIC HOLIDAYS (Chhuttiyan)
        // ========================================
        #region Holidays
        
        // GET: /Dashboard/Holidays - Holidays page dikhao
        [HttpGet]
        public async Task<IActionResult> Holidays()
        {
            if (!IsAuthenticated()) 
                return RedirectToAction("Login", "Account");
            
            ViewBag.UserName = HttpContext.Session.GetString("UserName");
            
            var model = new HolidaySearchModel { Country = "India", Year = DateTime.Now.Year };
            
            // India ki current year holidays automatically load karo
            var holidays = await _holidayService.GetPublicHolidaysAsync("IN", DateTime.Now.Year);
            ViewBag.Holidays = holidays;
            ViewBag.TotalHolidays = holidays.Count;
            
            return View(model);
        }

        // POST: /Dashboard/Holidays - Search karne par
        [HttpPost]
        public async Task<IActionResult> Holidays(HolidaySearchModel model)
        {
            if (!IsAuthenticated()) 
                return RedirectToAction("Login", "Account");
            
            ViewBag.UserName = HttpContext.Session.GetString("UserName");

            if (string.IsNullOrWhiteSpace(model.Country))
            {
                ViewBag.Error = "Please enter a country name";
                return View(model);
            }

            if (model.Year < 2020 || model.Year > 2049)
            {
                ViewBag.Error = "Please select a year between 2020 and 2049";
                return View(model);
            }

            var holidays = await _holidayService.GetPublicHolidaysAsync(model.Country, model.Year);
            
            if (!holidays.Any())
            {
                ViewBag.Error = "No holidays found for this country/year";
                return View(model);
            }

            ViewBag.Holidays = holidays;
            ViewBag.TotalHolidays = holidays.Count;
            return View(model);
        }
        #endregion

        // ========================================
        // TIME TRACK LIST
        // ========================================
        #region TimeTrack
        
        // GET: /Dashboard/TimeTrack
        [HttpGet]
        public async Task<IActionResult> TimeTrack(string? searchDate)
        {
            if (!IsAuthenticated()) 
                return RedirectToAction("Login", "Account");
            
            var userId = HttpContext.Session.GetString("UserId")!;
            ViewBag.UserName = HttpContext.Session.GetString("UserName");
            
            List<TimeTrackEntry> entries = new List<TimeTrackEntry>();
            
            // Only show data if search date is provided
            if (!string.IsNullOrEmpty(searchDate))
            {
                if (DateTime.TryParse(searchDate, out DateTime filterDate))
                {
                    var allEntries = await _timeTrackService.GetAllEntriesAsync(userId);
                    entries = allEntries.Where(e => e.Date.Date == filterDate.Date).ToList();
                    ViewBag.SearchDate = searchDate;
                }
            }
            
            return View(entries);
        }

        // POST: Add Entry
        [HttpPost]
        public async Task<IActionResult> AddTimeTrack(string workName, string date, string startTime, string endTime)
        {
            if (!IsAuthenticated()) 
                return RedirectToAction("Login", "Account");

            var userId = HttpContext.Session.GetString("UserId")!;
            
            if (string.IsNullOrWhiteSpace(workName))
            {
                TempData["Error"] = "Work name is required";
                return RedirectToAction("TimeTrack");
            }

            try
            {
                var entryDate = DateTime.Parse(date);
                var start = TimeSpan.Parse(startTime);
                var end = TimeSpan.Parse(endTime);

                await _timeTrackService.AddEntryAsync(userId, workName, entryDate, start, end);
                TempData["Success"] = "Entry added successfully!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error: {ex.Message}";
            }

            return RedirectToAction("TimeTrack");
        }

        // POST: Delete Entry
        [HttpPost]
        public async Task<IActionResult> DeleteTimeTrack(int id)
        {
            if (!IsAuthenticated()) 
                return RedirectToAction("Login", "Account");

            var userId = HttpContext.Session.GetString("UserId")!;
            var result = await _timeTrackService.DeleteEntryAsync(id, userId);
            
            TempData[result ? "Success" : "Error"] = result ? "Entry deleted!" : "Failed to delete entry";
            return RedirectToAction("TimeTrack");
        }
        #endregion

        // ========================================
        // HELPER METHOD
        // ========================================
        
        /// <summary>
        /// Check karo user logged in hai ya nahi
        /// Session mein UserId hai to logged in hai
        /// </summary>
        private bool IsAuthenticated() 
            => !string.IsNullOrEmpty(HttpContext.Session.GetString("UserId"));
    }
}
