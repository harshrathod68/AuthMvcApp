# 📚 AuthMvcApp - Complete Project Documentation
## For 6 Months Internship Freshers

---

## 🎯 Project Overview (Project ka Introduction)

**Project Name:** AuthMvcApp - Multi-Feature Dashboard  
**Technology:** ASP.NET Core MVC (.NET 9)  
**Database:** JSON Files (No SQL Database)  
**Pattern:** MVC (Model-View-Controller)

### Ye Project Kya Karta Hai?
Ek complete web application jo multiple features provide karta hai:
- User Registration & Login (with OTP verification)
- Weather Information
- Currency Converter
- Time Zone Converter
- Country Information
- News Reader
- Notes App
- Habit Tracker

---

## 📁 Folder Structure (Project ki Files)

```
AuthMvcApp/
│
├── Controllers/          ← Yahan sab Controllers hain (Logic)
│   ├── AccountController.cs    → Login, Register, Logout
│   ├── DashboardController.cs  → Weather, Currency, TimeZone, Country, News
│   ├── UserController.cs       → User CRUD operations
│   ├── NoteController.cs       → Notes CRUD
│   └── HabitController.cs      → Habit Tracker
│
├── Models/               ← Yahan sab Data Models hain (Data Structure)
│   ├── UserModel.cs           → User data structure
│   ├── WeatherModel.cs        → Weather data
│   ├── CurrencyModel.cs       → Currency data
│   ├── NoteModel.cs           → Notes data
│   └── HabitModel.cs          → Habit data
│
├── Services/             ← Yahan sab Services hain (Business Logic)
│   ├── EmailService.cs        → Email bhejne ka kaam
│   ├── OtpService.cs          → OTP generate karna
│   ├── WeatherService.cs      → Weather API call
│   ├── CurrencyService.cs     → Currency API call
│   └── ...
│
├── Views/                ← Yahan sab HTML Pages hain (UI)
│   ├── Account/              → Login, Register pages
│   ├── Dashboard/            → Weather, Currency pages
│   ├── User/                 → User list, create pages
│   ├── Note/                 → Notes pages
│   ├── Habit/                → Habit tracker pages
│   └── Shared/               → Layout (common header/footer)
│
├── Data/                 ← Yahan JSON files hain (Database)
│   ├── users.json            → Registered users
│   ├── userdata.json         → Added users
│   ├── notes.json            → User notes
│   └── habits.json           → User habits
│
├── wwwroot/              ← Static files (CSS, JS, Images)
├── appsettings.json      ← Configuration (API keys, SMTP settings)
└── Program.cs            ← Application entry point
```

---

## 🔄 MVC Pattern Samjho (Simple Explanation)

```
┌─────────────┐     ┌─────────────┐     ┌─────────────┐
│   USER      │────▶│ CONTROLLER  │────▶│   MODEL     │
│ (Browser)   │     │  (Logic)    │     │   (Data)    │
└─────────────┘     └─────────────┘     └─────────────┘
       ▲                   │
       │                   ▼
       │            ┌─────────────┐
       └────────────│    VIEW     │
                    │   (HTML)    │
                    └─────────────┘
```

**Simple Example:**
1. User clicks "Login" button
2. Controller receives request → `AccountController.Login()`
3. Controller checks data from Model → `UserModel`
4. Controller returns View → `Login.cshtml`
5. User sees the page

---

## 🔐 Feature 1: Authentication (Login/Register)

### Flow Diagram:
```
REGISTER:
User fills form → Controller validates → OTP sent to email → User enters OTP → Account created

LOGIN:
User enters email/password → Controller checks → If correct → Dashboard
                                              → If wrong → Show error
```

### Key Files:
- `Controllers/AccountController.cs` - Login/Register logic
- `Services/EmailService.cs` - Email bhejta hai
- `Services/OtpService.cs` - 6-digit OTP generate karta hai
- `Views/Account/Login.cshtml` - Login page
- `Views/Account/Register.cshtml` - Register page

### Code Example (AccountController.cs):
```csharp
// Jab user login form submit kare
[HttpPost]
public IActionResult Login(LoginModel model)
{
    // Step 1: Check if email exists
    var user = _jsonDataService.GetUserByEmail(model.Email);
    
    // Step 2: If user not found
    if (user == null)
    {
        ModelState.AddModelError("", "Email not found");
        return View(model);
    }
    
    // Step 3: Check password
    if (user.Password != model.Password)
    {
        ModelState.AddModelError("", "Wrong password");
        return View(model);
    }
    
    // Step 4: Login successful - Save in session
    HttpContext.Session.SetString("UserId", user.Id.ToString());
    HttpContext.Session.SetString("UserName", user.Name);
    
    // Step 5: Redirect to dashboard
    return RedirectToAction("Index", "Dashboard");
}
```

---

## 🌤️ Feature 2: Weather App

### Flow:
```
User enters city name → Controller calls WeatherService → 
WeatherService calls OpenWeatherMap API → Returns weather data → Shows on page
```

### API Used:
- **OpenWeatherMap API** (Free)
- URL: `https://api.openweathermap.org/data/2.5/weather?q={city}&appid={apikey}`

### Key Files:
- `Controllers/DashboardController.cs` → `Weather()` method
- `Services/WeatherService.cs` → API call logic
- `Models/WeatherModel.cs` → Weather data structure
- `Views/Dashboard/Weather.cshtml` → Weather page

### Code Example (WeatherService.cs):
```csharp
public async Task<WeatherData> GetWeatherAsync(string city)
{
    // Step 1: Build API URL
    var url = $"{_baseUrl}?q={city}&appid={_apiKey}&units=metric";
    
    // Step 2: Call API
    var response = await _httpClient.GetAsync(url);
    
    // Step 3: Check if successful
    if (!response.IsSuccessStatusCode)
    {
        return null; // City not found
    }
    
    // Step 4: Read JSON response
    var json = await response.Content.ReadAsStringAsync();
    
    // Step 5: Convert JSON to C# object
    var data = JsonSerializer.Deserialize<WeatherApiResponse>(json);
    
    // Step 6: Return weather data
    return new WeatherData
    {
        City = data.Name,
        Temperature = data.Main.Temp,
        Description = data.Weather[0].Description
    };
}
```

---

## 💱 Feature 3: Currency Converter

### Flow:
```
User selects currencies & amount → Controller calls CurrencyService →
CurrencyService calls ExchangeRate API → Returns converted amount
```

### API Used:
- **ExchangeRate-API** (Free, no key needed)
- URL: `https://api.exchangerate-api.com/v4/latest/{currency}`

---

## 📝 Feature 4: Notes App

### CRUD Operations:
- **C**reate - Add new note
- **R**ead - View notes list
- **U**pdate - Edit note
- **D**elete - Remove note

### Data Storage:
Notes are saved in `Data/notes.json` file

### Code Example (NoteService.cs):
```csharp
// Add new note
public void AddNote(NoteModel note)
{
    // Step 1: Read existing notes from JSON file
    var notes = ReadNotesFromFile();
    
    // Step 2: Generate new ID
    note.Id = notes.Any() ? notes.Max(n => n.Id) + 1 : 1;
    
    // Step 3: Set created date
    note.CreatedAt = DateTime.Now;
    
    // Step 4: Add to list
    notes.Add(note);
    
    // Step 5: Save back to file
    SaveNotesToFile(notes);
}
```

---

## 🎯 Feature 5: Habit Tracker

### Features:
- Add daily habits
- Mark complete/incomplete
- Track streaks (kitne din lagatar kiya)
- Weekly/Monthly/Yearly progress view

### Streak Calculation:
```csharp
// Current streak calculate karna
int currentStreak = 0;
var today = DateTime.Today;

for (int i = 0; i <= 365; i++)
{
    var date = today.AddDays(-i).ToString("yyyy-MM-dd");
    var isCompleted = habit.Logs.Any(l => l.Date == date && l.IsCompleted);
    
    if (isCompleted)
        currentStreak++;
    else
        break; // Streak toot gaya
}
```

---

## 🔧 Important Concepts for Freshers

### 1. Dependency Injection (DI)
```csharp
// Program.cs mein services register karte hain
builder.Services.AddScoped<IEmailService, EmailService>();

// Controller mein automatically inject ho jata hai
public class AccountController : Controller
{
    private readonly IEmailService _emailService;
    
    public AccountController(IEmailService emailService)
    {
        _emailService = emailService; // Automatically milta hai
    }
}
```

### 2. Session (User ko yaad rakhna)
```csharp
// Login ke baad user info save karo
HttpContext.Session.SetString("UserId", "123");
HttpContext.Session.SetString("UserName", "Harsh");

// Kisi bhi page par check karo
var userId = HttpContext.Session.GetString("UserId");
if (userId == null)
{
    // User logged in nahi hai
    return RedirectToAction("Login", "Account");
}
```

### 3. ViewBag (Controller se View mein data bhejna)
```csharp
// Controller mein
ViewBag.Message = "Welcome!";
ViewBag.UserName = "Harsh";

// View (.cshtml) mein
<h1>@ViewBag.Message</h1>
<p>Hello, @ViewBag.UserName</p>
```

### 4. Model Binding (Form data automatically object mein)
```csharp
// HTML Form
<input name="Email" />
<input name="Password" />

// Controller automatically LoginModel mein convert kar deta hai
public IActionResult Login(LoginModel model)
{
    var email = model.Email;    // Form se aaya
    var password = model.Password;
}
```

### 5. Async/Await (API calls ke liye)
```csharp
// API call mein time lagta hai, isliye async use karte hain
public async Task<WeatherData> GetWeatherAsync(string city)
{
    // await = jab tak response na aaye, wait karo
    var response = await _httpClient.GetAsync(url);
    return data;
}
```

---

## 🚀 How to Run Project

### Step 1: Prerequisites
- Install .NET 9 SDK
- Install Visual Studio Code or Visual Studio

### Step 2: Clone/Download
```bash
git clone https://github.com/harshrathod68/AuthMvcApp.git
cd AuthMvcApp
```

### Step 3: Run
```bash
dotnet build
dotnet run
```

### Step 4: Open Browser
- Go to: `https://localhost:5001` or `http://localhost:5000`

---

## 📧 API Keys & Configuration

All settings are in `appsettings.json`:

```json
{
  "EmailSettings": {
    "SmtpHost": "smtp.gmail.com",
    "SmtpPort": "587",
    "SmtpUser": "your-email@gmail.com",
    "SmtpPass": "your-app-password"
  },
  "WeatherApi": {
    "ApiKey": "your-openweathermap-key"
  },
  "NewsApi": {
    "ApiKey": "your-gnews-key"
  }
}
```

---

## 🎓 Tips for Freshers

1. **Pehle MVC pattern samjho** - Controller, Model, View ka role
2. **Debugging seekho** - Breakpoints lagao, step-by-step dekho
3. **Console.WriteLine use karo** - Values check karne ke liye
4. **Google karo** - Error aaye to copy-paste karke search karo
5. **Code padhne ki aadat daalo** - Dusron ka code samjho

---

## 📞 Contact

**Developer:** Harsh Rathod  
**GitHub:** https://github.com/harshrathod68/AuthMvcApp

---

*Last Updated: January 2026*
