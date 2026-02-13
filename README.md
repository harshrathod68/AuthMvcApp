# MyApps - ASP.NET MVC Multi-Feature Dashboard

## 🚀 Features

- **User Authentication** - Register, Login, OTP Verification, Forgot Password
- **Weather Details** - Search any city's weather using OpenWeatherMap API
- **Currency Converter** - Convert between 150+ currencies
- **Time Zone Converter** - Convert time between different time zones
- **Country Information** - Get details about any country
- **Latest News** - Read news from any country in multiple languages
- **Notes App** - Create, edit, delete notes with categories (Text, Image, Link, Todo, Idea)
- **Habit Tracker** - Track daily habits with streaks and progress charts

## 📁 Project Structure

```
MyApps/
├── Controllers/
│   ├── AccountController.cs    # Login, Register, Logout, OTP
│   ├── DashboardController.cs  # Weather, Currency, TimeZone, Country, News
│   ├── UserController.cs       # User CRUD operations
│   ├── NoteController.cs       # Notes CRUD
│   └── HabitController.cs      # Habit Tracker
├── Models/                     # Data models
├── Services/                   # Business logic & API calls
├── Views/                      # Razor views (HTML pages)
├── Data/                       # JSON data files
│   ├── users.json              # Registered users
│   ├── userdata.json           # Added users
│   ├── notes.json              # User notes
│   └── habits.json             # User habits
└── wwwroot/                    # Static files (CSS, JS)
```

## 🔧 Configuration

Update `appsettings.json` with your API keys:

```json
{
  "EmailSettings": {
    "SmtpHost": "smtp.gmail.com",
    "SmtpPort": "587",
    "SmtpUser": "your-email@gmail.com",
    "SmtpPass": "your-gmail-app-password"
  },
  "WeatherApi": {
    "ApiKey": "your-openweathermap-key"
  },
  "NewsApi": {
    "ApiKey": "your-gnews-key"
  }
}
```

### 📧 Email Setup (Important!)

For OTP and password reset emails to work, you need a **Gmail App Password**:

1. Go to: https://myaccount.google.com/security
2. Enable **2-Step Verification**
3. Generate **App Password** for "Mail"
4. Copy the 16-digit password (remove spaces)
5. Update `SmtpPass` in `appsettings.json`

**📖 Detailed Guide**: See [FIX_EMAIL_ISSUE.md](FIX_EMAIL_ISSUE.md) for complete setup instructions.

**🧪 Test Email**: After setup, test at: `http://localhost:5019/Account/TestEmail`

## 🚀 Running the Application

```bash
cd MyApps
dotnet build
dotnet run
```

Navigate to `http://localhost:5018`

## 👨‍💻 Developer

**Harsh Rathod**  
GitHub: https://github.com/harshrathod68/HR-MyApps
