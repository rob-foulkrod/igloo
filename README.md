# 🏔️ Igloo Events

> *Because even penguins need a social calendar.*

Welcome to **Igloo Events** — a cozy little ASP.NET Core MVC app for managing community events and registrations. No databases were harmed in the making of this application.

## ✨ What It Does

- **Create events** like "Hot Cocoa Social" or "Competitive Ice Sculpting"
- **Register attendees** until the igloo is full (capacity enforcement included)
- **CRUD all the things** — list, view, create, edit, delete events
- **Look good doing it** with Bootstrap 5

## 🏗️ Project Structure

```
igloo/
├── docs/           # You are here (spiritually)
├── infra/          # Future home of cloud stuff
├── src/
│   ├── web/        # The app — ASP.NET Core MVC
│   └── web.tests/  # xUnit tests — because we're responsible adults
└── igloo.sln       # The glue that holds it all together
```

## 🚀 Getting Started

```bash
# Clone it
git clone <your-repo-url>
cd igloo

# Run it
dotnet run --project src/web

# Test it
dotnet test

# Break it (optional, but inevitable)
```

## 🛠️ Tech Stack

| What | Why |
|------|-----|
| .NET 10 | Because we live on the edge |
| ASP.NET Core MVC | Razor views go brrr |
| In-Memory Data | Who needs a database when you have RAM? |
| Bootstrap 5 | Pretty without trying |
| xUnit | Red, green, refactor, repeat |

## 🐧 Seed Data

The app ships with a few events to get you started:

1. **Community Ice Skating** — 50 spots, bring your own mittens
2. **Winter Film Festival** — 100 spots, popcorn not included
3. **Hot Cocoa Social** — 30 spots, marshmallows mandatory

## 📋 Non-Goals (for now)

- ❌ Authentication (everyone is welcome in this igloo)
- ❌ Database (memory is forever... until you restart)
- ❌ File uploads (no snowball fights via HTTP)
- ❌ Real-time notifications (check back manually, like it's 1999)

## 🤝 Contributing

1. Pick a work item from the board
2. Branch from `main`
3. Code like the wind
4. PR it up

## 📄 License

MIT — go forth and build igloos.

---

*Built with ❄️ and caffeine!!*
