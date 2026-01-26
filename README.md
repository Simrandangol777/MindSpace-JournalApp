# 🧠 Mindspace – Personal Journaling App

Mindspace is a secure, offline-first personal journaling application designed to help users reflect, track emotions, and gain insights into their daily mental well-being.

Built using **.NET MAUI Blazor + SQLite**, Mindspace focuses on **privacy, simplicity, and emotional awareness**.

---

## 📌 Project Overview

**Purpose:**  
To provide users with a safe and intuitive digital space to record daily journal entries and track emotional patterns over time.

**Scope:**  
- Personal journaling
- Mood tracking
- Analytics & insights
- Secure access using PIN

**Objectives:**
- Encourage daily reflection
- Visualize emotional trends
- Maintain privacy with local storage
- Provide a distraction-free writing experience

---

## ✨ Key Features

### 📓 Journal Entries
- Create, view, edit, and delete journal entries
- One entry per day rule
- Rich text writing support
- Categories, moods, and tags
- Word count tracking

### 🔐 Authentication & Security
- Email + password registration
- Strong password validation
- Persistent login session
- 4-digit PIN lock on every app launch
- Separate layouts for Auth, PIN, and Main App

### 📊 Dashboard & Insights
- Current streak & longest streak
- Missed days calculation
- Mood distribution (positive / neutral / negative)
- Most frequent moods
- Most used tags
- Word count trends

### 📅 Calendar Navigation
- Monthly calendar grid 
- Highlighted days with journal entries
- Mood indicators per day
- Click a date to preview journal entry
- Quick navigation between months

### 🎨 Theme Customization
- Light mode
- Dark mode
- Theme persists across sessions

### 📤 Export Journals
- Export journal entries as **PDF**
- Select date range
- Offline export (local file)

---

## 🧩 Application Flow

1. **Register**
   - Create account
   - Set up a 4-digit PIN

2. **Login**
   - Enter email & password
   - Unlock app using PIN

3. **Daily Use**
   - Write journal entries
   - View insights on dashboard

4. **App Relaunch**
   - PIN unlock only (no email/password required)

---

## 🛠 Tech Stack

- **Frontend:** Blazor (Razor Components)
- **Backend Logic:** C#
- **Framework:** .NET MAUI Blazor
- **Database:** SQLite (local storage)
- **PDF Export:** QuestPDF
- **Security:** Hashing + local PIN lock


