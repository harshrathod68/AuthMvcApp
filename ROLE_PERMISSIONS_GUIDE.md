# Role-Based App Permissions System

## Overview
Admin can now control which applications are visible for each user role (Admin and User) in the dashboard.

## Features Implemented

### 1. **Account Role Button (Admin Only)**
- Added "Account Role" button in navbar
- Only visible to Admin users
- Located next to "Users" button

### 2. **Role Management Page**
- **URL**: `/Role/Index`
- Shows two role cards: Admin and User
- Each card has "Manage Apps" button
- Beautiful gradient design with hover effects

### 3. **App Selection Page**
- **URL**: `/Role/Edit?role=Admin` or `/Role/Edit?role=User`
- Grid layout with checkboxes for each app
- Visual indicators for selected apps
- Apps included:
  - Weather Details 🌤️
  - Currency Conversion 💱
  - Time Conversion 🕐
  - Country Information 🌍
  - Latest News 📰
  - My Notes 📝
  - Habit Tracker 🎯
  - Language Translator 🌐
  - Emergency Numbers 🚨
  - Time Track ⏱️

### 4. **Dashboard Filtering**
- Dashboard automatically shows only allowed apps for user's role
- Apps are filtered based on role permissions
- Seamless user experience

### 5. **Access Control**
- Normal users CANNOT access Role Management
- Only Admin can view and modify role permissions
- Attempting to access redirects to Dashboard with error message

## Files Created

### Models
- `Models/RolePermissionModel.cs` - Data models for role permissions

### Services
- `Services/IRolePermissionService.cs` - Interface
- `Services/RolePermissionService.cs` - Implementation

### Controllers
- `Controllers/RoleController.cs` - Manages role permissions

### Views
- `Views/Role/Index.cshtml` - Role selection page
- `Views/Role/Edit.cshtml` - App selection page

### Data
- `Data/rolepermissions.json` - Stores role permissions

## Files Modified

### Configuration
- `Program.cs` - Registered RolePermissionService

### Controllers
- `Controllers/DashboardController.cs` - Added role filtering logic

### Views
- `Views/Shared/_Layout.cshtml` - Added Account Role button
- `Views/Dashboard/Index.cshtml` - Added app filtering based on permissions

## How It Works

### For Admin:
1. Login as Admin
2. Click "Account Role" button in navbar
3. Select a role (Admin or User)
4. Check/uncheck apps to control visibility
5. Click "Save Permissions"
6. Changes apply immediately to all users with that role

### For Normal Users:
- Cannot see "Account Role" button
- Cannot access role management pages
- Dashboard shows only apps allowed for their role
- No way to modify permissions

## Default Permissions
By default, both Admin and User roles have access to all apps. Admin can customize this as needed.

## Technical Details

### Permission Storage
```json
[
  {
    "Role": "Admin",
    "AllowedApps": ["Weather", "Currency", "TimeZone", ...]
  },
  {
    "Role": "User",
    "AllowedApps": ["Weather", "Currency", ...]
  }
]
```

### Dashboard Filtering Logic
```csharp
var rolePermission = _rolePermissionService.GetRolePermission(userRole);
ViewBag.AllowedApps = rolePermission?.AllowedApps ?? new List<string>();
```

### View Filtering
```razor
@if (allowedApps.Contains("Weather", StringComparer.OrdinalIgnoreCase))
{
    <!-- Show Weather app -->
}
```

## Security
- Role-based access control enforced at controller level
- Session validation on every request
- Normal users redirected with error message if they try to access admin features
- Permissions stored in JSON file with proper validation

## Testing

### Test as Admin:
1. Login with admin credentials
2. Navigate to Account Role
3. Modify User role permissions (uncheck some apps)
4. Logout and login as normal user
5. Verify only selected apps are visible

### Test as Normal User:
1. Login with user credentials
2. Verify "Account Role" button is NOT visible
3. Try accessing `/Role/Index` directly
4. Should redirect to Dashboard with "Access Denied" message

## Future Enhancements
- Add more granular permissions (read/write/delete)
- Create custom roles beyond Admin/User
- Add permission history/audit log
- Bulk permission management
- Export/Import permission configurations

---

**Application URL**: http://localhost:5019
**Status**: ✅ Fully Implemented and Tested
