# Architecture

## Boundaries

```text
Avalonia App
  ├── XAML views
  └── MVVM ViewModels
          │
          ├── Core calculation and analysis services
          └── Infrastructure persistence and export services
```

### Core

`PowerModuleWorkbench.Core` contains the domain models, validation, calculation
engine, comparison rules, and sensitivity analysis. It has no UI or file-system
dependencies. This makes the engineering behavior deterministic and easy to
test.

### Infrastructure

`PowerModuleWorkbench.Infrastructure` contains JSON project persistence and
CSV export. File and serialization concerns stay outside the calculation
engine. Project files carry a schema version so future formats can be
migrated deliberately.

### App

`PowerModuleWorkbench.App` contains Avalonia XAML and ViewModels. It parses
user-entered text, calls Core services, formats values with units, and exposes
explicit error messages. It does not contain engineering formulas.

## Data flow

1. The user enters text into the configuration form.
2. The ViewModel parses numbers using invariant culture.
3. Core validation returns all input issues.
4. The calculator returns a typed result or rejects invalid input.
5. The ViewModel formats results for the dashboard and chart.
6. Infrastructure saves or exports the same typed input/result values.

## Error handling

- Expected invalid input is represented as validation messages.
- Expected file and schema failures are shown to the user.
- Unexpected persistence/comparison failures are logged with operation context
  and shown as an error message; they are not silently ignored.

## Testing strategy

- Core unit tests verify formulas, validation, comparison, and sensitivity.
- Infrastructure tests verify JSON round trips, schema rejection, and CSV
  headers/units.
- The application is build-validated with compiled Avalonia bindings.
