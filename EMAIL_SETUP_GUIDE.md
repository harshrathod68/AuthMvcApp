# 📧 Email Setup Guide - Gmail App Password

## ❌ Current Problem
```
Error: 535: 5.7.8 Username and Password not accepted
```

**Reason**: Gmail App Password `xiatxswxnwujoahd` is invalid or expired.

---

## ✅ Solution: Generate New App Password

### Step 1: Google Account Security
1. Open browser and go to: https://myaccount.google.com/security
2. Login with: `rathodharshan534@gmail.com`

### Step 2: Enable 2-Step Verification (if not already enabled)
1. Find "2-Step Verification" option
2. Click and enable it
3. Follow the setup process (required for App Passwords)

### Step 3: Generate App Password
1. Search for "App Passwords" in the search box
2. Or directly go to: https://myaccount.google.com/apppasswords
3. Click "Select app" → Choose "Mail"
4. Click "Select device" → Choose "Other (Custom name)"
5. Type name: `MyApps`
6. Click "Generate" button

### Step 4: Copy the Password
- You'll see a 16-digit password in yellow box
- Example format: `abcd efgh ijkl mnop`
- **Remove spaces** and copy: `abcdefghijklmnop`

### Step 5: Update appsettings.json
1. Open `appsettings.json` file
2. Find the `EmailSettings` section
3. Replace `SmtpPass` value with your new 16-digit password:

```json
"EmailSettings": {
  "SmtpHost": "smtp.gmail.com",
  "SmtpPort": "587",
  "SmtpUser": "rathodharshan534@gmail.com",
  "SmtpPass": "YOUR_NEW_16_DIGIT_PASSWORD_HERE",
  "FromEmail": "rathodharshan534@gmail.com",
  "FromName": "MyApps"
}
```

### Step 6: Restart Application
```bash
taskkill /F /IM MyApps.exe 2>$null
dotnet build
dotnet run
```

### Step 7: Test Registration
1. Go to: http://localhost:5019/Account/Register
2. Fill the form and select role (Admin/User)
3. Click "Register" button
4. Check if OTP email is sent successfully
5. Check your email inbox for OTP
6. Enter OTP and verify

---

## 🔍 Troubleshooting

### If still getting error:
1. **Check 2-Step Verification**: Must be enabled
2. **Check App Password**: Must be 16 digits, no spaces
3. **Check Email**: Must be `rathodharshan534@gmail.com`
4. **Check Internet**: Must have active connection
5. **Check Gmail Settings**: "Less secure app access" is NOT needed for App Passwords

### Alternative Solution:
If App Password doesn't work, try using a different Gmail account:
1. Create new Gmail account
2. Enable 2-Step Verification
3. Generate App Password
4. Update all email settings in `appsettings.json`

---

## 📝 Current Configuration Status

✅ Email Service: Properly implemented with error handling
✅ OTP Service: Working correctly
✅ Registration Flow: Complete (Register → OTP → Verify → Login)
✅ Role-Based Access: Admin and User roles implemented
❌ Gmail App Password: **NEEDS UPDATE**

---

## 🎯 Next Steps After Fixing Email

Once email is working:
1. Test Admin registration
2. Test User registration
3. Test OTP verification
4. Test login with both roles
5. Test access history logging
6. Test forgot password flow

---

**Note**: Keep your App Password secure. Don't share it publicly or commit it to GitHub!
