# Igloo Events – Application Specification

## Overview

**Igloo Events** is an ASP.NET Core MVC web application for managing community events and attendee registrations. It uses in-memory data storage (no database/EF Core), Bootstrap for UI, and follows the repository/service pattern with dependency injection.

## Technology Stack

| Layer | Technology |
|-------|-----------|
| Framework | .NET 10 / ASP.NET Core MVC |
| UI | Razor Views, Bootstrap 5 |
| Data | In-memory collections (no EF Core) |
| Testing | xUnit |
| CI/CD | Azure DevOps Pipelines |
| Hosting | TBD (Azure App Service or Container Apps) |

## Project Structure

```
igloo/
├── docs/                    # Documentation & specs
│   └── SPEC.md
├── infra/                   # Infrastructure-as-Code (Bicep/Terraform)
├── src/
│   ├── web/                 # ASP.NET Core MVC application
│   │   ├── Controllers/
│   │   ├── Models/
│   │   ├── Services/
│   │   ├── Views/
│   │   └── Program.cs
│   └── web.tests/           # xUnit test project
│       ├── Controllers/
│       └── Services/
├── igloo.sln
└── .gitignore
```

## Domain Models

### Event

| Property | Type | Validation |
|----------|------|-----------|
| Id | int | Auto-generated |
| Title | string | Required, max 100 chars |
| Description | string | Required, max 500 chars |
| StartDate | DateTime | Required, must be future |
| EndDate | DateTime | Required, must be after StartDate |
| Location | string | Required |
| Capacity | int | Required, Range 1–10000 |

### Registration

| Property | Type | Validation |
|----------|------|-----------|
| Id | int | Auto-generated |
| EventId | int | Required |
| Name | string | Required, max 100 chars |
| Email | string | Required, valid email format |
| RegisteredAt | DateTime | Auto-set to UTC now |

## Service Interfaces

### IEventService

```csharp
IEnumerable<Event> GetAll();
Event? GetById(int id);
Event Create(Event evt);
Event Update(Event evt);
bool Delete(int id);
```

### IRegistrationService

```csharp
IEnumerable<Registration> GetByEventId(int eventId);
Registration? GetById(int id);
Registration Create(Registration registration);
bool Delete(int id);
int GetRegistrationCount(int eventId);
bool HasCapacity(int eventId);
```

## Features

### 1. Event Management (CRUD)

- List all events with title, date, location, capacity
- View event details with registration count and remaining capacity
- Create new events with validation
- Edit existing events
- Delete events (with confirmation)

### 2. Registration System

- Register for an event from the event details page
- Capacity enforcement – reject registrations when event is full
- Display registration count and remaining spots

### 3. UI & Navigation

- Bootstrap 5 responsive layout
- Navigation bar: Home, Events
- Landing page with app description and link to events

### 4. Validation & Error Handling

- Data annotations on models (`[Required]`, `[Range]`, `[EmailAddress]`, etc.)
- `ModelState` validation in controllers
- Global error handling middleware
- `ILogger` usage in controllers

## Seed Data

The `InMemoryEventService` initializes with 2–3 sample events:

1. **Community Ice Skating** – capacity 50
2. **Winter Film Festival** – capacity 100
3. **Hot Cocoa Social** – capacity 30

## Non-Goals (v1)

- No database / Entity Framework
- No authentication / authorization
- No file uploads
- No real-time notifications
- No API endpoints (MVC only)
