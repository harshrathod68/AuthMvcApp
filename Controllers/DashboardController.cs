using Microsoft.AspNetCore.Mvc;
using AuthMvcApp.Services;
using AuthMvcApp.Models;

namespace AuthMvcApp.Controllers
{
    /// <summary>
    /// Controller for handling dashboard related actions
    /// Includes Weather, Currency Conversion, and Time Zone features
    /// </summary>
    public class DashboardController : Controller
    {
        private readonly IWeatherService _weatherService;
        private readonly ICurrencyService _currencyService;
        private readonly ITimeZoneService _timeZoneService;

        /// <summary>
        /// Constructor with dependency injection for required services
        /// </summary>
        /// <param name="weatherService">Service for weather data</param>
        /// <param name="currencyService">Service for currency conversion</param>
        /// <param name="timeZoneService">Service for time zone conversion</param>
        public DashboardController(
            IWeatherService weatherService, 
            ICurrencyService currencyService,
            ITimeZoneService timeZoneService)
        {
            _weatherService = weatherService;
            _currencyService = currencyService;
            _timeZoneService = timeZoneService;
        }

        #region Dashboard Home

        /// <summary>
        /// Displays the main dashboard with feature cards
        /// </summary>
        /// <returns>Dashboard index view</returns>
        public IActionResult Index()
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Login", "Account");
            }

            ViewBag.UserName = HttpContext.Session.GetString("UserName");
            return View();
        }

        #endregion

        #region Weather Feature

        /// <summary>
        /// Displays the weather search page
        /// </summary>
        /// <returns>Weather view with empty search model</returns>
        [HttpGet]
        public IActionResult Weather()
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Login", "Account");
            }

            ViewBag.UserName = HttpContext.Session.GetString("UserName");
            return View(new WeatherSearchModel());
        }

        /// <summary>
        /// Handles weather search form submission
        /// </summary>
        /// <param name="model">Weather search model containing city name</param>
        /// <returns>Weather view with search results</returns>
        [HttpPost]
        public async Task<IActionResult> Weather(WeatherSearchModel model)
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Login", "Account");
            }

            ViewBag.UserName = HttpContext.Session.GetString("UserName");

            // Validate city name input
            if (string.IsNullOrWhiteSpace(model.City))
            {
                ViewBag.Error = "Please enter a city name";
                return View(model);
            }

            // Fetch weather data from API
            var weather = await _weatherService.GetWeatherAsync(model.City);
            
            if (weather == null)
            {
                ViewBag.Error = "City not found. Please check the city name.";
                return View(model);
            }

            ViewBag.Weather = weather;
            return View(model);
        }

        #endregion

        #region Currency Conversion Feature

        /// <summary>
        /// Displays the currency conversion page
        /// </summary>
        /// <returns>Currency view with default conversion model</returns>
        [HttpGet]
        public IActionResult Currency()
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Login", "Account");
            }

            ViewBag.UserName = HttpContext.Session.GetString("UserName");
            
            // Initialize with default values (1 USD to INR)
            var model = new CurrencyConversionModel
            {
                Amount = 1,
                FromCurrency = "USD",
                ToCurrency = "INR"
            };
            
            return View(model);
        }

        /// <summary>
        /// Handles currency conversion form submission
        /// </summary>
        /// <param name="model">Currency conversion model with amount and currencies</param>
        /// <returns>Currency view with conversion results</returns>
        [HttpPost]
        public async Task<IActionResult> Currency(CurrencyConversionModel model)
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Login", "Account");
            }

            ViewBag.UserName = HttpContext.Session.GetString("UserName");

            // Validate model state
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Check if same currency selected
            if (model.FromCurrency == model.ToCurrency)
            {
                ViewBag.Error = "Please select different currencies for conversion";
                return View(model);
            }

            // Perform currency conversion
            var result = await _currencyService.ConvertCurrencyAsync(
                model.Amount, 
                model.FromCurrency, 
                model.ToCurrency
            );

            if (result == null)
            {
                ViewBag.Error = "Unable to fetch exchange rates. Please try again later.";
                return View(model);
            }

            ViewBag.Result = result;
            return View(model);
        }

        #endregion

        #region Time Zone Conversion Feature

        /// <summary>
        /// Displays the time zone conversion page
        /// </summary>
        /// <returns>TimeZone view with default conversion model</returns>
        [HttpGet]
        public IActionResult TimeZone()
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Login", "Account");
            }

            ViewBag.UserName = HttpContext.Session.GetString("UserName");
            
            // Initialize with default values (current time, IST to EST)
            var model = new TimeZoneConversionModel
            {
                Time = DateTime.Now.ToString("HH:mm"),
                Date = DateTime.Now.ToString("yyyy-MM-dd"),
                FromTimeZone = "India Standard Time",
                ToTimeZone = "Eastern Standard Time"
            };
            
            return View(model);
        }

        /// <summary>
        /// Handles time zone conversion form submission
        /// </summary>
        /// <param name="model">Time zone conversion model with time and zones</param>
        /// <returns>TimeZone view with conversion results</returns>
        [HttpPost]
        public IActionResult TimeZone(TimeZoneConversionModel model)
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Login", "Account");
            }

            ViewBag.UserName = HttpContext.Session.GetString("UserName");

            // Validate model state
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Check if same timezone selected
            if (model.FromTimeZone == model.ToTimeZone)
            {
                ViewBag.Error = "Please select different time zones for conversion";
                return View(model);
            }

            // Parse date and time
            if (!DateTime.TryParse($"{model.Date} {model.Time}", out DateTime dateTime))
            {
                ViewBag.Error = "Invalid date or time format";
                return View(model);
            }

            // Perform time zone conversion
            var result = _timeZoneService.ConvertTime(
                dateTime,
                model.FromTimeZone,
                model.ToTimeZone
            );

            if (result == null)
            {
                ViewBag.Error = "Unable to convert time. Please check the selected time zones.";
                return View(model);
            }

            ViewBag.Result = result;
            return View(model);
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Checks if the current user is authenticated
        /// </summary>
        /// <returns>True if user is logged in, false otherwise</returns>
        private bool IsAuthenticated()
        {
            return !string.IsNullOrEmpty(HttpContext.Session.GetString("UserId"));
        }

        #endregion
    }
}
