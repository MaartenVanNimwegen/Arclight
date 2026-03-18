# Arclight

![Build Status](https://github.com/MaartenVanNimwegen/Arclight/actions/workflows/pr-validator.yml/badge.svg)
![Release Status](https://github.com/MaartenVanNimwegen/Arclight/actions/workflows/master-release.yml/badge.svg)
![Security Scan](https://github.com/MaartenVanNimwegen/Arclight/actions/workflows/codeql.yml/badge.svg)
![Code Coverage](https://raw.githubusercontent.com/MaartenVanNimwegen/Arclight/badges/badge_shield.svg)
![.NET Version](https://img.shields.io/badge/.NET-10.0-purple)

Arclight is a modern RESTful Web API built with .NET 10 and **Entity Framework Core**, following **Clean Architecture** principles. This project demonstrates a professional setup including automated CI/CD pipelines, database migrations, and strict development workflows.

---

## 📚 Table of Contents
- [Tech Stack](#-tech-stack)
- [Architecture](#-architecture)
- [Getting Started](#-getting-started)
- [Database Setup](#-database-setup)
- [Development Workflow](#-development-workflow)
- [CI/CD Pipelines](#-cicd-pipelines)

---

## 🛠 Tech Stack

* **Framework:** .NET 10 (C# 12)
* **ORM:** Entity Framework Core
* **Database:** PostgreSQL (Production) / PostgreSQL (Dev)
* **Testing:** nUnit
* **CI/CD:** GitHub Actions
* **Security:** CodeQL, Dependabot
* **Versioning:** Semantic Versioning (Tag-based)

---

## 🏗 Architecture

The solution is structured to enforce separation of concerns:

* **`Arclight.Application`**: Contains enterprise logic via services, DTOs, and Interfaces.
* **`Arclight.Domain`**: Contains Entities and Enums. No dependencies on other projects.
* **`Arclight.Infrastructure`**: Handles data access (DbContext), migrations, and external service implementations.
* **`Arclight.Api`**: The entry point. Contains Controllers, Configuration, and Dependency Injection setup.

---

## 🚀 Getting Started

### Prerequisites
* [.NET 10.0 SDK](https://dotnet.microsoft.com/download)
* [Entity Framework Core Tools](https://learn.microsoft.com/en-us/ef/core/cli/dotnet) (`dotnet tool install --global dotnet-ef`)
* PostgreSQL

### Installation

1.  **Clone the repository**
    ```bash
    git clone [https://github.com/MaartenVanNimwegen/Arclight.git](https://github.com/MaartenVanNimwegen/Arclight.git)
    cd Arclight
    ```

2.  **Restore dependencies**
    ```bash
    dotnet restore
    ```

3.  **Configuration**
    Update `appsettings.json` in `Arclight.Api` with your connection string.

4.  **Run the application**
    ```bash
    dotnet run
    ```
    The API will be available at `https://localhost:7xxx`.

---

## 🗄 Database Setup

### Migrations
This project uses EF Core Code-First migrations.

* **Apply migrations to local database:**
    ```bash
    dotnet ef database update --project Arclight.Infrastructure --startup-project Arclight.Api
    ```

* **Add a new migration:**
    ```bash
    dotnet ef migrations add NameOfMigration --project Arclight.Infrastructure --startup-project Arclight.Api
    ```

### Data Seeding
The application includes a `DbInitializer` that runs automatically in the **Development** environment.
* If the database is empty, it seeds test data (generated via **Bogus**) for debugging purposes.
* Check `Program.cs` and `DbInitializer.cs` to customize this behavior.

---

## 🔄 Development Workflow

We follow **GitHub Flow** and **Conventional Commits**.

1.  **Create a Branch:** Always branch off `master`.
    * `feat/new-endpoint`
    * `fix/validation-error`
2.  **Commit Messages:** Use the [Conventional Commits](https://www.conventionalcommits.org/) format.
    * `feat(users): add get-by-id endpoint`
    * `fix(db): correct column type in migration`
3.  **Pull Request:**
    * Push to origin.
    * Open a PR to `master`.
    * **CI Checks:** The `PR Validation` pipeline must pass.
    * **Merge:** Squash & Merge is preferred.

---

## ⚙️ CI/CD Pipelines

This repository uses **GitHub Actions** for automation:

| Workflow | Trigger | Description |
| :--- | :--- | :--- |
| **PR Validation** | Pull Requests | Builds the solution and runs all Unit Tests. Prevents broken code from reaching master. |
| **CodeQL Scan** | PRs & Schedule | Scans the codebase for security vulnerabilities and code quality issues. |
| **Production Release** | Tags (`v*`) | Triggered when a tag (e.g., `v1.0.0`) is pushed. Builds the Release artifact and attaches it to the GitHub Release. |
| **Dependabot** | Weekly | Automatically checks for NuGet package updates and opens PRs. |

### Creating a Release
To deploy a new version:

1.  Ensure `master` is up to date.
2.  Create and push a tag:
    ```bash
    git tag v1.0.0
    git push origin v1.0.0
    ```
3.  The **Production Release** pipeline will build the artifact with version `1.0.0`.
