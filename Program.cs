/*
 * =====================================================
 * Program.cs - Application Entry Point
 * =====================================================
 * 
 * Ye file application ka starting point hai.
 * Jab "dotnet run" karte ho, ye file sabse pehle run hoti hai.
 * 
 * Isme hum:
 * 1. Services register karte hain (Dependency Injection)
 * 2. Middleware configure karte hain
 * 3. Routing setup karte hain
 * 
 * =====================================================
 */

using AuthMvcApp.Services;

// ===== STEP 1: Builder Create =====
// WebApplication builder banao
var builder = WebApplication.CreateBuilder(args);

// ===== STEP 2: Services Register (Dependency Injection) =====
// Ye services baad mein Controllers mein automatically inject hongi

// MVC Controllers aur Views enable karo
builder.Services.AddControllersWithViews();

// ----- Data Services -----
// Singleton = Ek hi instance puri app mein (memory efficient)
builder.Services.AddSingleton<IJsonDataService, JsonDataService>();  // Users data (users.json)
builder.Services.AddSingleton<IUserDataService, UserDataService>();  // Added users (userdata.json)
builder.Services.AddSingleton<INoteService, NoteService>();          // Notes data (notes.json)
builder.Services.AddSingleton<IHabitService, HabitService>();        // Habits data (habits.json)

// ----- Authentication Services -----
builder.Services.AddSingleton<IOtpService, OtpService>();            // OTP generate karna
builder.Services.AddScoped<IEmailService, EmailService>();           // Email bhejne ke liye
// Scoped = Har request ke liye naya instance

// ----- API Services (HttpClient use karte hain) -----
// AddHttpClient = HttpClient automatically inject hota hai
builder.Services.AddHttpClient<IWeatherService, WeatherService>();   // Weather API
builder.Services.AddHttpClient<ICurrencyService, CurrencyService>(); // Currency API
builder.Services.AddHttpClient<ICountryService, CountryService>();   // Country API
builder.Services.AddHttpClient<INewsService, NewsService>();         // News API

// ----- Other Services -----
builder.Services.AddSingleton<ITimeZoneService, TimeZoneService>();  // Time zone conversion

// ===== STEP 3: Session Configuration =====
// Session = User ko yaad rakhne ke liye (login status, etc.)
builder.Services.AddDistributedMemoryCache(); // Session data memory mein store hoga

builder.Services.AddSession(options =>
{
    // Session 30 minutes tak valid rahega
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    
    // Security settings
    options.Cookie.HttpOnly = true;      // JavaScript se access nahi ho sakta
    options.Cookie.IsEssential = true;   // GDPR ke liye zaroori
});

// ===== STEP 4: Application Build =====
var app = builder.Build();

// ===== STEP 5: Middleware Pipeline =====
// Middleware = Request aur Response ke beech mein processing

// Production mein error handling
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts(); // HTTP Strict Transport Security
}

// HTTPS redirect (http:// ko https:// par bhejo)
app.UseHttpsRedirection();

// Static files serve karo (CSS, JS, images from wwwroot folder)
app.UseStaticFiles();

// Routing enable karo
app.UseRouting();

// Session middleware (Session use karne ke liye zaroori)
app.UseSession();

// Authorization (future mein roles ke liye)
app.UseAuthorization();

// ===== STEP 6: Default Route Setup =====
// URL pattern: /{controller}/{action}/{id?}
// Default: /Account/Login (Login page pehle dikhega)
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

/*
 * Route Examples:
 * /                     → AccountController.Login()
 * /Account/Register     → AccountController.Register()
 * /Dashboard/Index      → DashboardController.Index()
 * /Dashboard/Weather    → DashboardController.Weather()
 * /User/Edit/5          → UserController.Edit(id: 5)
 */

// ===== STEP 7: Application Run =====
// Server start karo aur requests listen karo
app.Run();
