# AuthMvcApp - ASP.NET MVC Authentication with JSON Storage

## Project Structure

```
AuthMvcApp/
├── Controllers/
│   ├── AccountController.cs    # Login, Register, Logout
│   ├── AdminController.cs      # Admin Dashboard, Add/View Users
│   └── UserController.cs       # User Dashboard
├── Models/
│   ├── UserModel.cs            # User entity model
│   ├── LoginModel.cs           # Login form model
│   └── RegisterModel.cs        # Registration form model
├── Services/
│   ├── IJsonDataService.cs     # Service interface
│   └── JsonDataService.cs      # JSON file read/write operations
├── Views/
│   ├── Account/
│   │   ├── Login.cshtml        # Login page
│   │   └── Register.cshtml     # Registration page
│   ├── Admin/
│   │   ├── Index.cshtml        # Admin Dashboard (user list)
│   │   ├── AddUser.cshtml      # Add new user form
│   │   └── UserDetails.cshtml  # View user details
│   ├── User/
│   │   └── Index.cshtml        # User Dashboard (profile)
│   └── Shared/
│       └── _Layout.cshtml      # Main layout with navigation
├── Data/
│   ├── admin.json              # Admin accounts storage
│   └── users.json              # User accounts storage
└── Program.cs                  # App configuration with session setup
```

## Authentication & Authorization Flow

### Registration Flow
1. User fills registration form with Name, Email, Password, Role
2. System validates input and checks for duplicate email
3. Based on role selection:
   - Admin → saved to `Data/admin.json`
   - User → saved to `Data/users.json`
4. Redirect to login page

### Login Flow
1. User enters Email and Password
2. System first checks `admin.json` for matching credentials
3. If found → Set session as Admin → Redirect to Admin Dashboard
4. If not found → Check `users.json`
5. If found → Set session as User → Redirect to User Dashboard
6. If not found → Show error message

### Session Management
- Session stores: UserId, UserName, UserEmail, UserRole
- Session timeout: 30 minutes
- Session cleared on logout

### Authorization
- Admin pages check `UserRole == "Admin"` in session
- User pages check for valid session and prevent admin access
- Unauthenticated users redirected to login

## Sample Credentials

### Admin Account
- Email: `admin@example.com`
- Password: `Admin123`

### User Accounts
- Email: `john@example.com` | Password: `User123`
- Email: `jane@example.com` | Password: `User123`

## Running the Application

```bash
cd AuthMvcApp
dotnet run
```

Navigate to `https://localhost:5001` or `http://localhost:5000`
