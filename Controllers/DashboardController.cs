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
using AuthMvcApp.Services;
using AuthMvcApp.Models;

namespace AuthMvcApp.Controllers
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

        // Constructor - Jab controller banta hai tab services milti hain
        public DashboardController(
            IWeatherService weatherService, 
            ICurrencyService currencyService,
            ITimeZoneService timeZoneService,
            ICountryService countryService,
            INewsService newsService)
        {
            _weatherService = weatherService;
            _currencyService = currencyService;
            _timeZoneService = timeZoneService;
            _countryService = countryService;
            _newsService = newsService;
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
            
            // Step 2: User ka naam ViewBag mein daalo (View mein dikhane ke liye)
            ViewBag.UserName = HttpContext.Session.GetString("UserName");
            
            // Step 3: Dashboard page dikhao
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
        public IActionResult Country()
        {
            if (!IsAuthenticated()) 
                return RedirectToAction("Login", "Account");
            
            ViewBag.UserName = HttpContext.Session.GetString("UserName");
            return View(new CountrySearchModel());
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
