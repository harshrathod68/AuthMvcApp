# 🎯 Skill Roadmap Planner - Complete Guide

## Overview
Skill Roadmap Planner aapko help karta hai kisi bhi skill ko systematically seekhne mein. Aap day-by-day planning kar sakte ho, progress track kar sakte ho, aur apne learning journey ko organize kar sakte ho.

## ✨ Features

### 1. **Create Roadmap**
- Skill name define karo (e.g., Web Development, Python, Data Science)
- Description add karo - kya achieve karna hai
- Difficulty level select karo: Beginner, Intermediate, Advanced
- Target days set karo - kitne din mein complete karna hai
- Start date choose karo

### 2. **My Roadmaps**
- Sabhi roadmaps ek jagah dekho
- Progress percentage real-time
- Status tracking: Not Started, In Progress, Completed
- Tasks completion count
- Beautiful card-based UI

### 3. **Daily Tasks View**
- Aaj ke tasks dekho
- Previous/Next day navigate karo
- Task completion checkbox
- Time estimation per task
- Priority levels: Low, Medium, High
- Time slots: Morning, Afternoon, Evening, Night

### 4. **Roadmap Details**
- Complete overview
- Progress statistics
- Task summary
- Duration and timeline

## 🚀 How to Use

### Step 1: Create Your First Roadmap
1. Dashboard se "Skill Roadmap" app open karo
2. "Create New Roadmap" button click karo
3. Form fill karo:
   - **Skill Name**: e.g., "Full Stack Web Development"
   - **Description**: "Learn MERN stack and build projects"
   - **Difficulty**: Intermediate
   - **Target Days**: 90
   - **Start Date**: Today
4. "Create Roadmap" click karo

### Step 2: View Your Roadmap
- Roadmap card pe click karo
- Details page open hoga
- Progress aur stats dekho

### Step 3: Daily Tasks (Coming Soon - Manual for Now)
Currently, aap manually tasks add kar sakte ho `Data/roadmaptasks.json` file mein:

```json
[
  {
    "Id": 1,
    "MilestoneId": 1,
    "RoadmapId": 1,
    "TaskName": "Learn HTML Basics",
    "Description": "Study HTML tags, elements, and structure",
    "ScheduledDate": "2026-02-07T00:00:00",
    "EstimatedMinutes": 60,
    "TimeSlot": "Morning",
    "IsCompleted": false,
    "CompletedAt": null,
    "Resources": "https://developer.mozilla.org/en-US/docs/Web/HTML",
    "Notes": "",
    "Priority": "High"
  }
]
```

### Step 4: Track Progress
- Daily Tasks page se tasks complete karo
- Checkbox click karo to mark as done
- Progress automatically update hoga

## 📊 Data Structure

### Roadmap Model
```json
{
  "Id": 1,
  "SkillName": "Web Development",
  "Description": "Learn full stack development",
  "DifficultyLevel": "Intermediate",
  "TargetDays": 90,
  "StartDate": "2026-02-06",
  "EndDate": "2026-05-07",
  "UserEmail": "user@example.com",
  "Status": "In Progress",
  "TotalTasks": 50,
  "CompletedTasks": 15
}
```

### Daily Task Model
```json
{
  "Id": 1,
  "RoadmapId": 1,
  "TaskName": "Learn React Hooks",
  "Description": "Study useState, useEffect, useContext",
  "ScheduledDate": "2026-02-07",
  "EstimatedMinutes": 120,
  "TimeSlot": "Afternoon",
  "IsCompleted": false,
  "Priority": "High",
  "Resources": "https://react.dev/learn"
}
```

## 🎨 UI Features

### Dashboard Card
- Gradient background
- Skill name prominently displayed
- Difficulty badge
- Progress bar
- Task completion stats
- Status indicator
- Quick actions: View Details, Delete

### Daily Tasks View
- Date navigator (Previous/Next day)
- Summary cards: Total Tasks, Completed, Total Time
- Task cards with:
  - Checkbox for completion
  - Task title and description
  - Time slot badge
  - Priority badge
  - Estimated time
  - Resources indicator

### Color Coding
- **Beginner**: Green 🟢
- **Intermediate**: Yellow 🟡
- **Advanced**: Red 🔴
- **Priority High**: Red
- **Priority Medium**: Orange
- **Priority Low**: Green

## 📝 Example Roadmaps

### 1. Web Development (90 days)
**Phases:**
- Phase 1: HTML & CSS (15 days)
- Phase 2: JavaScript (20 days)
- Phase 3: React (25 days)
- Phase 4: Backend (20 days)
- Phase 5: Projects (10 days)

### 2. Python Programming (60 days)
**Phases:**
- Phase 1: Basics (10 days)
- Phase 2: Data Structures (15 days)
- Phase 3: OOP (10 days)
- Phase 4: Libraries (15 days)
- Phase 5: Projects (10 days)

### 3. Data Science (120 days)
**Phases:**
- Phase 1: Python Basics (15 days)
- Phase 2: Statistics (20 days)
- Phase 3: Pandas & NumPy (25 days)
- Phase 4: Machine Learning (40 days)
- Phase 5: Projects (20 days)

## 🔮 Coming Soon Features

### Phase Management
- Add multiple phases to roadmap
- Beginner → Intermediate → Advanced
- Phase-wise progress tracking

### Milestone System
- Break phases into milestones
- Checkpoint achievements
- Celebration on completion

### Task Scheduler
- Automatic task generation
- Smart scheduling based on time availability
- Recurring tasks support

### Resources Library
- Add learning materials
- Video links, articles, courses
- Bookmark important resources

### Progress Analytics
- Weekly/Monthly charts
- Completion trends
- Time spent analysis
- Streak tracking

### Reminders & Notifications
- Daily task reminders
- Deadline alerts
- Milestone celebrations

### AI Suggestions
- Recommended learning paths
- Task time estimation
- Difficulty adjustment

## 🎯 Best Practices

### 1. Set Realistic Goals
- Don't overestimate daily capacity
- Start with 1-2 hours per day
- Gradually increase as you build momentum

### 2. Break Down Tasks
- Large tasks → Small actionable items
- Each task should be completable in one session
- Clear, specific task names

### 3. Consistent Schedule
- Same time every day
- Morning for theory, evening for practice
- Weekend for projects

### 4. Track Progress Daily
- Mark tasks as complete
- Add notes about learnings
- Review weekly progress

### 5. Stay Flexible
- Adjust timeline if needed
- Don't stress about delays
- Focus on learning, not just completion

## 📱 Access Control

### Admin Users
- Can create unlimited roadmaps
- Access all features
- Manage role permissions

### Normal Users
- Can create unlimited roadmaps
- Personal roadmaps only
- Full feature access

### Role Permissions
Admin can control if "Skill Roadmap" app is visible for different roles via Account Role management.

## 🗂️ Files Structure

```
Models/
  └── SkillRoadmapModel.cs       # Data models

Services/
  ├── ISkillRoadmapService.cs    # Interface
  └── SkillRoadmapService.cs     # Implementation

Controllers/
  └── SkillRoadmapController.cs  # Routes & logic

Views/SkillRoadmap/
  ├── Index.cshtml               # All roadmaps
  ├── Create.cshtml              # Create new
  ├── Details.cshtml             # Roadmap details
  └── DailyTasks.cshtml          # Daily tasks view

Data/
  ├── roadmaps.json              # Roadmaps storage
  └── roadmaptasks.json          # Tasks storage
```

## 🔧 Technical Details

### Service Registration
```csharp
builder.Services.AddSingleton<ISkillRoadmapService, SkillRoadmapService>();
```

### Routes
- `/SkillRoadmap/Index` - All roadmaps
- `/SkillRoadmap/Create` - Create new
- `/SkillRoadmap/Details/{id}` - View details
- `/SkillRoadmap/DailyTasks?date=2026-02-07` - Daily tasks
- `/SkillRoadmap/Delete/{id}` - Delete roadmap

### API Endpoints
- `POST /SkillRoadmap/ToggleTaskStatus` - Mark task complete/incomplete

## 💡 Tips & Tricks

### For Beginners
- Start with 30-day roadmaps
- Focus on fundamentals
- Don't skip basics

### For Intermediate
- 60-90 day roadmaps
- Include project work
- Practice daily

### For Advanced
- 90-120 day roadmaps
- Deep dive into topics
- Build real-world projects

## 🎓 Learning Resources

### Recommended Platforms
- **Free**: freeCodeCamp, MDN, W3Schools
- **Paid**: Udemy, Coursera, Pluralsight
- **Practice**: LeetCode, HackerRank, CodeWars
- **Projects**: GitHub, CodePen, Replit

### Time Management
- **Pomodoro Technique**: 25 min work, 5 min break
- **Time Blocking**: Dedicated learning hours
- **Weekly Review**: Sunday planning session

## 🚀 Success Stories

### Example Timeline
**Day 1-7**: HTML & CSS basics
**Day 8-14**: JavaScript fundamentals
**Day 15-21**: DOM manipulation
**Day 22-30**: First project - Portfolio website

### Milestones to Celebrate
- ✅ First 7 days streak
- ✅ 25% progress
- ✅ 50% progress
- ✅ First project completion
- ✅ 100% completion

## 📞 Support

Agar koi problem ho ya suggestion ho:
1. Check documentation
2. Review example roadmaps
3. Start with simple roadmap
4. Build gradually

---

**Application URL**: http://localhost:5019
**Status**: ✅ Fully Implemented
**Version**: 1.0.0

Happy Learning! 🎯📚🚀
