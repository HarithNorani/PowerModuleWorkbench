# PowerModule Workbench

PowerModule Workbench is a C#/.NET Avalonia desktop application for
configuring and analysing simplified power-electronic modules. It will
calculate losses, efficiency, and thermal behaviour, show the results in an
engineering dashboard, compare configurations, and export traceable results.

The project is designed as a portfolio project for a .NET GUI engineering
role. It is an educational prototype and must not be used to certify or
operate real power-electronics equipment.

## Development pipeline

The project is delivered in vertical slices. Each slice must compile, be
tested, and leave the application in a demonstrable state before the next
slice begins.

| Phase | Deliverable | Exit criteria |
|---|---|---|
| 0. Foundation | Solution, projects, formatting, CI | Clean restore/build/test on a supported .NET SDK |
| 1. Domain model | Inputs, results, validation, units | Invalid inputs are rejected with actionable messages |
| 2. Calculation engine | Electrical and thermal calculations | Core calculations have focused unit tests |
| 3. Configuration UI | Avalonia XAML form and MVVM bindings | A user can enter and validate a project |
| 4. Results dashboard | KPI cards, status, and charts | Recalculating inputs updates visible results |
| 5. Persistence | JSON save/load and CSV export | A save/load round trip preserves values and results |
| 6. Comparison | Multiple configurations and sensitivity analysis | Configurations can be compared on the same chart |
| 7. Quality | Documentation, accessibility, diagnostics | README, assumptions, tests, and CI are complete |

Detailed milestones, work items, and acceptance criteria are in
[`docs/implementation-pipeline.md`](docs/implementation-pipeline.md).

## Planned repository layout

```text
PowerModuleWorkbench.sln
src/
  PowerModuleWorkbench.App/             # Avalonia views, XAML, ViewModels
  PowerModuleWorkbench.Core/            # Domain models and calculations
  PowerModuleWorkbench.Infrastructure/  # JSON, CSV, and file services
tests/
  PowerModuleWorkbench.Core.Tests/
  PowerModuleWorkbench.App.Tests/
docs/
  implementation-pipeline.md
  calculation-model.md
  architecture.md
  user-guide.md
```

## Local prerequisites

- .NET SDK 8.0 or newer LTS
- Git
- Visual Studio 2022, JetBrains Rider, or VS Code with C# support

Install the Avalonia templates used by this project:

```bash
dotnet new install Avalonia.Templates::11.3.12
```

The project intentionally uses Avalonia 11.3.12 with .NET 8. The newer
Avalonia 12 template currently defaults to .NET 10, so it should not be used
until the project is intentionally migrated.

The solution is implemented through the comparison and sensitivity-analysis
milestones. The standard validation command is:

```bash
dotnet restore
dotnet build --configuration Release --no-restore
dotnet test --configuration Release --no-build
```

The solution has been verified successfully on macOS arm64 with
`.NET SDK 8.0.426` and Avalonia `11.3.12`.

Read the [architecture](docs/architecture.md), [calculation model](docs/calculation-model.md),
and [user guide](docs/user-guide.md) before extending the application.

## Engineering boundaries

- Keep calculations in `PowerModuleWorkbench.Core`; do not put business logic
  in XAML code-behind or ViewModels.
- Treat units explicitly in names and labels.
- Use validation results for expected user input errors; reserve exceptions
  for unexpected failures.
- Document every assumption used by the calculation model.
- Do not use real Siemens Energy data, proprietary designs, or confidential
  information.
