# QuizTest

> This project is a concept demo intended for students to explore before an upcoming lab. It serves as a reference for how a small .NET console application can be structured using interfaces, dependency injection, and external APIs.

A console-based multiple-choice quiz game built with .NET 10, powered by the [Open Trivia Database](https://opentdb.com) API and rendered with [Spectre.Console](https://spectreconsole.net).

The solution is split into:

- `QuizTest.Domain` for domain models (no external dependencies).
- `QuizTest.Application` for use-case logic and application contracts (depends only on Domain).
- `QuizTest.Infrastructure` for external integrations (API client, data access adapters).
- `QuizTest.Core` for library composition (no DI framework dependency—purely orchestrates layers).
- `QuizTest` for console UI, composition root, and app entry point.

## Features

- Select difficulty: easy, medium, or hard
- Choose number of questions: 5, 10, 15, or 20
- Browse and filter by quiz category
- Randomized answer order per question
- Immediate feedback after each answer
- Final score summary with percentage

## Requirements

- [.NET 10 SDK](https://dotnet.microsoft.com/download)

## Getting Started

```bash
git clone <repo-url>
cd QuizTest
dotnet run --project src/QuizTest
```

## Running Tests

```bash
dotnet test
```

## Dependency Injection

Service registration is configured in the UI project's composition root:

```csharp
services.AddQuizCore();
```

Optional overrides:

```csharp
services.AddQuizCore(
    apiBaseUrl: "https://opentdb.com",
    apiTimeout: TimeSpan.FromSeconds(30));
```

## Architecture

Dependency direction (clean architecture):

```mermaid
flowchart LR
  UI[QuizTest<br/>Composition Root]
  Infrastructure[QuizTest.Infrastructure<br/>API Adapters]
  Application[QuizTest.Application<br/>Use Cases + Contracts]
  Domain[QuizTest.Domain<br/>Models]
  Api[Open Trivia DB<br/>API]

  UI -->|references| Infrastructure
  UI -->|references| Application
  Infrastructure -->|implements contracts| Application
  Infrastructure -->|uses| Domain
  Application -->|depends on| Domain
  Infrastructure -->|HTTP calls| Api
```

**Inner layers have zero framework dependencies:**
- Domain: pure models, no external packages
- Application: use cases + contracts, depends only on Domain
- Infrastructure: API adapters, depends on Application + Domain
- Core: orchestrates layers (no code needed, kept for future expansion)
- UI: console app + composition root, only layer with Microsoft.Extensions.DependencyInjection and Spectre.Console

## Project Structure

```zsh
src/
  QuizTest.Domain/
    Domain/                 # Domain models (QuizQuestion, QuizCategory)
  QuizTest.Application/
    Contracts/              # Application Ports: IQuizApiClient, IQuizUi, IAnswerShuffler
    Services/               # Use-case logic: QuizGame, RandomAnswerShuffler
  QuizTest.Infrastructure/
    Integrations/OpenTrivia/ # OpenTrivia API DTOs
    QuizApiClient.cs         # Open Trivia API adapter (implements IQuizApiClient)
  QuizTest.Core/
    (orchestration—no code, kept for future composites)
  QuizTest/
    DependencyInjection/    # `AddQuizCore()` extension method
    Services/               # UI adapter: SpectreQuizUi (implements IQuizUi)
    Program.cs              # Composition root (entry point)
tests/
  QuizTest.Tests/          # xUnit + Moq integration tests
```

## Tech Stack

- **Framework:** .NET 10
- **UI:** Spectre.Console
- **DI:** Microsoft.Extensions.DependencyInjection
- **Testing:** xUnit, Moq, Coverlet
- **API:** [Open Trivia Database](https://opentdb.com)
