# QuizTest

> This project is a concept demo intended for students to explore before an upcoming lab. It serves as a reference for how a small .NET console application can be structured using interfaces, dependency injection, and external APIs.

A console-based multiple-choice quiz game built with .NET 10, powered by the [Open Trivia Database](https://opentdb.com) API and rendered with [Spectre.Console](https://spectreconsole.net).

The solution is split into:

- `QuizTest.Domain` for domain models (no external dependencies).
- `QuizTest.Application` for use-case logic and application contracts (depends only on Domain).
- `QuizTest.Infrastructure` for external integrations (API client, data access adapters).
- `QuizTest.Core` for DI composition (composes all layers).
- `QuizTest` for console UI and app entry point.

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

Core service registration is exposed through an extension method:

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
  UI[QuizTest\nConsole UI]
  Core[QuizTest.Core\nDI Composition]
  Application[QuizTest.Application\nUse Cases]
  Infrastructure[QuizTest.Infrastructure\nAPI Adapters]
  Domain[QuizTest.Domain\nModels]
  Api[Open Trivia DB API]

  UI -->|references| Core
  Core -->|references| Application
  Application -->|no external deps| Domain
  Infrastructure -->|implements contracts| Application
  Infrastructure -->|uses| Domain
  Infrastructure -->|HTTP calls| Api
```

Runtime flow:

- `Program.cs` calls `AddQuizCore()` to wire all layers.
- `AddQuizCore()` registers Infrastructure implementations of Application contracts (`IQuizApiClient` -> `QuizApiClient`).
- Application services (`QuizGame`, `RandomAnswerShuffler`) are configured as dependencies.
- UI registers `IQuizUi` implementation with `SpectreQuizUi`.
- Inner layers (Application, Domain, Infrastructure) have no dependency on UI or external frameworks.

## Project Structure

```zsh
src/
  QuizTest.Domain/
    Domain/          # Domain models (QuizQuestion, QuizCategory)
  QuizTest.Application/
    Contracts/       # Application contracts (IQuizApiClient, IQuizUi, IAnswerShuffler)
    Services/        # Application use-case logic (QuizGame, RandomAnswerShuffler)
  QuizTest.Infrastructure/
    Integrations/OpenTrivia/ # OpenTrivia API response models
    QuizApiClient.cs        # Open Trivia API adapter implementing IQuizApiClient
  QuizTest.Core/
    DependencyInjection/    # IServiceCollection extensions (AddQuizCore)
  QuizTest/
    Services/        # UI implementation (Spectre.Console, QuizTest.Ui.Services)
    Program.cs       # Entry point and DI composition
tests/
  QuizTest.Tests/    # xUnit tests with Moq
```

## Tech Stack

- **Framework:** .NET 10
- **UI:** Spectre.Console
- **DI:** Microsoft.Extensions.DependencyInjection
- **Testing:** xUnit, Moq, Coverlet
- **API:** [Open Trivia Database](https://opentdb.com)
