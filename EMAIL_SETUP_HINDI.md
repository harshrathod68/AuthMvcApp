# 📧 Email Setup Guide (Hindi)

## ❌ समस्या क्या है?

जब आप Register button click करते हो, तो यह error आता है:
```
Failed to send OTP email. Please check your email address and try again.
```

**कारण**: Gmail App Password `xiatxswxnwujoahd` invalid या expired हो गया है।

---

## ✅ समाधान (Step-by-Step)

### Step 1: Google Account Security खोलो

1. Browser में जाओ: **https://myaccount.google.com/security**
2. Login करो: `rathodharshan534@gmail.com`

### Step 2: 2-Step Verification Enable करो

1. Left side में "2-Step Verification" option ढूंढो
2. अगर disabled है तो enable करो
3. Setup wizard follow करो
4. **जरूरी**: App Password के लिए 2-Step Verification mandatory है

### Step 3: App Password Generate करो

1. Search box में type करो: **"App Passwords"**
2. या direct link खोलो: **https://myaccount.google.com/apppasswords**
3. "Select app" click करो → **"Mail"** choose करो
4. "Select device" click करो → **"Other (Custom name)"** choose करो
5. Name type करो: **"MyApps"**
6. **"Generate"** button click करो

### Step 4: Password Copy करो

1. Yellow box में 16-digit password दिखेगा
2. Format: `abcd efgh ijkl mnop` (spaces के साथ)
3. **Spaces हटाकर copy करो**: `abcdefghijklmnop`
4. **ध्यान दो**: यह password सिर्फ एक बार दिखेगा, save कर लो!

### Step 5: appsettings.json Update करो

1. File खोलो: `J:\User\appsettings.json`
2. `EmailSettings` section में जाओ
3. `SmtpPass` की value बदलो:

```json
"EmailSettings": {
  "SmtpHost": "smtp.gmail.com",
  "SmtpPort": "587",
  "SmtpUser": "rathodharshan534@gmail.com",
  "SmtpPass": "abcdefghijklmnop",  ← यहाँ नया password डालो
  "FromEmail": "rathodharshan534@gmail.com",
  "FromName": "MyApps"
}
```

### Step 6: Application Restart करो

```powershell
# पुराना process बंद करो
taskkill /F /IM MyApps.exe

# 2 second wait करो
Start-Sleep -Seconds 2

# Build करो
dotnet build

# Run करो
dotnet run
```

### Step 7: Test करो

#### Option A: Test Email Page (Recommended)
1. Browser खोलो: **http://localhost:5019/Account/TestEmail**
2. अपना email address डालो
3. "Send Test Email" click करो
4. अपना inbox check करो
5. अगर email आया तो setup successful! ✅

#### Option B: Registration से Test करो
1. Browser खोलो: **http://localhost:5019/Account/Register**
2. Form भरो:
   - Name: Test User
   - Email: your_email@gmail.com
   - Password: Test@123
   - Role: User या Admin
3. "Register" button click करो
4. Check करो OTP email आया या नहीं
5. Inbox में OTP check करो
6. OTP enter करो verification page पर
7. Registration complete करो

---

## 🔍 अगर फिर भी Problem हो

### Problem 1: Authentication Error आ रहा है

**Check करो:**
- ✅ 2-Step Verification enabled है या नहीं
- ✅ App Password सही copy किया (spaces नहीं होने चाहिए)
- ✅ Regular Gmail password use नहीं कर रहे
- ✅ Old App Password revoke कर दिया

**Solution:**
1. Old App Password delete करो Google Account से
2. Fresh App Password generate करो
3. Carefully copy करो (no spaces)
4. appsettings.json update करो
5. Application restart करो

### Problem 2: Email नहीं आ रहा

**Check करो:**
1. ✅ Spam/Junk folder
2. ✅ Email address spelling सही है
3. ✅ Internet connection चालू है
4. ✅ Gmail inbox full नहीं है

### Problem 3: Port Already in Use

**Error:**
```
Failed to bind to address http://127.0.0.1:5019: address already in use
```

**Solution:**
```powershell
# Existing process kill करो
taskkill /F /IM MyApps.exe

# 2 second wait करो
Start-Sleep -Seconds 2

# फिर से run करो
dotnet run
```

---

## ✅ Checklist (Test करने से पहले)

- [ ] Gmail पर 2-Step Verification enabled है
- [ ] New App Password generate किया (16 digits)
- [ ] App Password बिना spaces के copy किया
- [ ] `appsettings.json` में नया password update किया
- [ ] Application restart किया
- [ ] कोई build error नहीं है
- [ ] Server चल रहा है: http://localhost:5019
- [ ] Internet connection active है

---

## 🎯 सही Result कैसा होगा?

### जब Email सही से काम करेगा:

1. **Registration Flow:**
   ```
   Register Form भरो → OTP Generate हो → Email भेजा जाए → OTP Sent ✅
   → Inbox Check करो → OTP Enter करो → Verify हो → Account बन जाए ✅
   → Login Page पर जाओ → Login करो → Dashboard खुल जाए ✅
   ```

2. **Console में Success Logs:**
   ```
   info: Attempting to send OTP to: user@example.com
   info: Using SMTP: smtp.gmail.com:587
   info: Connecting to SMTP server...
   info: Connected successfully
   info: Authenticating...
   info: Authenticated successfully
   info: Sending email...
   info: Email sent successfully
   info: OTP sent successfully to user@example.com
   ```

3. **Email में क्या आएगा:**
   - Subject: "Email Verification OTP - MyApps"
   - Body: सुंदर HTML email with 6-digit OTP
   - OTP valid for 5 minutes

---

## 🚀 Quick Commands

```powershell
# 1. Process बंद करो
taskkill /F /IM MyApps.exe 2>$null

# 2. Build करो
dotnet build

# 3. Run करो
dotnet run

# 4. Test page खोलो
start http://localhost:5019/Account/TestEmail

# 5. या Registration खोलो
start http://localhost:5019/Account/Register
```

---

## 📝 Important Files

1. ✅ `appsettings.json` - Email configuration
2. ✅ `FIX_EMAIL_ISSUE.md` - Detailed English guide
3. ✅ `EMAIL_SETUP_GUIDE.md` - Setup instructions
4. ✅ `QUICK_FIX.txt` - Quick reference
5. ✅ `EMAIL_SETUP_HINDI.md` - यह file (Hindi guide)

---

## 💡 Tips

1. **App Password safe रखो** - कहीं save कर लो
2. **Spaces remove करना मत भूलो** - Copy करते समय
3. **2-Step Verification जरूर enable करो** - बिना इसके App Password नहीं बनेगा
4. **Test Email page पहले use करो** - Registration से पहले
5. **Console logs check करो** - Error details के लिए

---

## 🎉 Success के बाद

Email fix होने के बाद ये सब features काम करेंगे:

1. ✅ **User Registration** - OTP verification के साथ
2. ✅ **Admin Registration** - Role selection के साथ
3. ✅ **Email Verification** - OTP inbox में आएगा
4. ✅ **Forgot Password** - Reset code email में
5. ✅ **Password Reset** - OTP verification के साथ
6. ✅ **Access History** - Login tracking
7. ✅ **Role-Based Access** - Admin vs User permissions

---

**अगर फिर भी problem हो तो:**
1. Console logs ध्यान से पढ़ो
2. सभी steps फिर से check करो
3. Test Email page पहले try करो
4. Gmail account settings verify करो
5. Internet connection stable है check करो

---

**Last Updated**: 3 February 2026  
**Author**: Kiro AI Assistant  
**Project**: MyApps - HR Management System
