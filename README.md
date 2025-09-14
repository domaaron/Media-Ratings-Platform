# 🎬 Media Ratings Platform (MRP)

The **Media Ratings Platform (MRP)** is a standalone RESTful HTTP server written in **Java** or **C#**.  
It provides an **API backend** for managing media content (movies, series, and games), allowing users to register, rate content, and receive personalized recommendations.  

> ⚠️ This project only implements the **API server** – no frontend (web, mobile, console) is included.

---

## 🚀 Features

### 👤 User
- Register and log in with unique credentials
- View and edit profile (including personal statistics)
- Create, update, and delete media entries
- Rate media (⭐ 1–5) with optional comment
- Edit or delete own ratings
- Like ratings of other users (max. 1 like per rating)
- Mark media entries as favorites
- View rating history and favorite list
- Receive recommendations based on:
  - previous rating behavior  
  - genre/content similarity  

### 🎥 Media Entry
- Represents a **movie, series, or game**
- Contains: title, description, media type, release year, genre(s), age restriction
- Created by a user, only editable/deletable by its creator
- Includes a list of ratings and a calculated average score
- Can be marked as favorite by other users

### ⭐ Rating
- Bound to one media entry and one user
- Contains: star value (1–5), optional comment, timestamp
- Can be liked by other users
- Editable/deletable by its creator
- **Moderation feature**: comments must be confirmed by the creator before becoming public

---

## 📌 Use Cases
- User registration & login  
- Manage media entries (CRUD)  
- Rate and comment on media  
- Like ratings  
- Search media by title (supports partial matches)  
- Filter media by genre, type, release year, age restriction, or rating  
- Sort results by title, year, or score  
- Mark/unmark media as favorites  
- View a public leaderboard of most active users  
- Recommendations based on:
  - genre similarity to highly rated media  
  - content similarity (genre, type, age restriction)  

---

## 🔧 Implementation Requirements
- RESTful HTTP server
  - Correct HTTP codes:
    - `2XX` → success
    - `4XX` → client errors (e.g., invalid input, missing authentication)
    - `5XX` → server errors (e.g., database unavailable)
- **Framework restrictions**:  
  - Use only low-level HTTP helpers (e.g., `HttpListener`)  
  - ❌ No ASP.NET, Spring, JSP/JSF
- Object serialization: Jackson, Newtonsoft.JSON, or equivalent
- Database: **PostgreSQL** (Dockerized setup allowed)
- Provide **Postman Collection** or **curl script** for integration testing
- Write **≥ 20 unit tests** for core business logic

---

## 🔐 Token-Based Authentication
All requests (except registration and login) must be authenticated.  

- Login returns a **token string** (`username-mrpToken`)
- Token must be included in all subsequent requests as an **Authorization Bearer header**

### Example: Login
```http
POST http://localhost:8080/api/users/login
Content-Type: application/json

{
  "Username": "mustermann",
  "Password": "max"
}
```

### Response:
```arduino
"mustermann-mrpToken"
```

---

## 📊 Additional Features
- Public leaderboard sorted by number of ratings per user
- Extended user profile statistics:
  - total number of ratings
  - average score
  - favorite genre
- Favorite list for quick access to bookmarked entries

---

## 🌟 Optional Features
- Extra features may compensate for mistakes in core requirements
- Maximum score remains 100% (cannot be exceeded)

---

## 🧪 Testing & Submission
### Deliverables
1. **Intermediate Submission (Class 13)**
   - HTTP server  
   - User registration & login  
   - Basic media management (CRUD)  

2. **Final Submission (Class 22)**
   - Full business logic  
   - Ratings, favorites, filtering, recommendations  

3. **Hand-in package must include:**
   - Source code  
   - README.md with GitHub repository link  
   - Postman Collection or curl script for endpoint testing  
   - protocol.md containing:  
     - Technical steps & architecture decisions  
     - Explanation of unit test coverage  
     - Problems encountered & solutions  
     - Time tracking per major task  
     - (Git history serves as documentation, no need to duplicate)  

---

## 🎓 Final Presentation (Class 22–25)

Be prepared with:
- Working solution running on your machine  
- Postman/curl tests ready to execute  
- Open design/protocol to present architecture & approach

---

## 📂 Suggested Project Structure
```bash
/src
  /controllers
  /models
  /services
  /repositories
/tests
  /unit
/docs
  protocol.md
  postman_collection.json
README.md
```
