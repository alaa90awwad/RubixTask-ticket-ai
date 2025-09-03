# RubixTask-ticket-ai
Task
# RubixTask — AI Support Ticket Categorizer

A small full-stack app where users create support tickets and get an **AI-suggested category**.  
Frontend uses **React (Create React App) + Bootstrap**. Backend is **ASP.NET Core Web API (controllers)** that **proxies OpenAI** (key is never exposed to the browser).

**Repo:** https://github.com/alaa90awwad/RubixTask-ticket-ai

---

## Features

- Ticket form: **Title** (required) & **Description** (required)
- **Suggest category** (AI) → shows one label from:
  `['Billing','Technical Support','Login Issue','Feature Request','General Feedback']`
- Manual **override** via dropdown
- **Submit** adds ticket to a list (persisted in `localStorage`)
- Loading & error states for **suggest** and **submit**
- Basic validation + disabled buttons during requests
- Security: API keys only on the **server**; **CORS** enabled for local dev

---

## Tech Stack

- **Frontend:** React (CRA), JavaScript, Bootstrap
- **Backend:** ASP.NET Core 8 Web API (Controllers), HttpClient
- **AI:** OpenAI Chat Completions (strict system prompt)
- **State/Persistence:** React state + `localStorage`

---

How to run (local)

Backend

Set OPENAI_API_KEY in your environment.

cd backend → dotnet run

Frontend

cd frontend → create .env with REACT_APP_API_BASE_URL=<backend_url>

npm install → npm start
