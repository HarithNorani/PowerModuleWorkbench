# PowerModule Workbench implementation pipeline

## 1. Product goal

Provide a small, maintainable desktop engineering tool that lets a user
configure a simplified power module, calculate electrical and thermal
indicators, understand warnings, compare operating points, and export a
traceable result.

## 2. First release scope

### Included

- DC-link voltage, output power, switching frequency, ambient temperature,
  thermal resistance, conduction resistance, switching energy, and maximum
  junction temperature.
- Approximate conduction loss, switching loss, total loss, efficiency, junction
  temperature, and thermal margin.
- Validation with units and actionable messages.
- Avalonia UI using XAML and MVVM.
- JSON project persistence and CSV results export.
- Automated tests and a GitHub Actions build pipeline.

### Excluded from the first release

- Hardware control or live measurement.
- Electromagnetic, CFD, or certified thermal simulation.
- A production component database.
- Claims that results are suitable for equipment certification.

## 3. Work breakdown

### Milestone 0 — Foundation

1. Install the .NET 8 SDK.
2. Install `Avalonia.Templates::11.3.12`.
3. Create the solution and four projects shown in the root README.
4. Enable nullable reference types and implicit usings.
5. Add repository formatting and editor configuration.
6. Add the CI workflow.

**Definition of done:** restore, Release build, and tests pass locally and in
CI.

### Milestone 1 — Domain and validation

Create immutable input and result models:

- `PowerModuleInput`
- `PowerModuleResult`
- `ValidationIssue`
- `ValidationSeverity`

Validate required values, positive ranges, finite numeric values, and the
relationship between thermal limits and ambient temperature. Return all
validation issues together so the UI can show a complete correction list.

**Definition of done:** validation tests cover boundary values and every
user-editable field.

**Implemented:** `PowerModuleInput`, `ValidationIssue`,
`ValidationSeverity`, and `PowerModuleInputValidator` are in the Core
project. The validator uses explicit unit-bearing property names and returns
all field errors in one pass.

### Milestone 2 — Calculation engine

Implement `IPowerModuleCalculator` in the Core project. Keep formulas in a
small, deterministic service:

```text
conductionLoss = currentRms² × conductionResistance
switchingLoss  = switchingEnergy × switchingFrequency
totalLoss      = conductionLoss + switchingLoss
junctionTemp   = ambientTemp + totalLoss × thermalResistance
efficiency     = outputPower / (outputPower + totalLoss)
thermalMargin  = maximumJunctionTemp - junctionTemp
```

The model must document its assumptions and expose units in property names or
documentation. It must never silently calculate with invalid input.

**Definition of done:** calculation tests verify each formula, monotonic
behaviour, efficiency bounds, and thermal-limit warnings.

**Implemented:** `IPowerModuleCalculator` and `PowerModuleCalculator` provide
validated, deterministic estimates. The current model documents its RMS
current approximation and returns explicit thermal-limit warnings.

### Milestone 3 — Configuration UI

Build the Avalonia shell and a configuration View/ViewModel:

- Group inputs by electrical and thermal concerns.
- Show units beside numeric fields.
- Validate on edit and before calculation.
- Disable calculation for invalid input.
- Provide sensible example defaults.

**Definition of done:** a new user can configure a valid project without
reading source code and can understand every validation error.

**Implemented:** the Avalonia main window now provides unit-labelled inputs,
parsing and domain validation feedback, a Calculate command, and a compact
results summary for the Core calculation engine.

### Milestone 4 — Analysis dashboard

Add a results screen with KPI cards for total loss, efficiency, junction
temperature, and thermal margin. Add a status panel and charts for losses,
temperature, and efficiency across a selected operating range.

**Definition of done:** changing a valid input updates the results without
freezing the UI; invalid input leaves the last valid result visible and shows
the reason recalculation was skipped.

**Implemented:** the dashboard includes KPI-style result values and an
operating-point loss chart that evaluates the configured module across 50% to
150% of its output power.

### Milestone 5 — Persistence and export

Add versioned JSON project files and CSV export. File and serialization errors
must be reported to the user with a recovery action; they must not be swallowed.

**Definition of done:** save, reload, and export integration tests pass, and
the CSV contains column headers, units, and the displayed result values.

**Implemented:** project files use schema version 1 JSON with validation on
load, and result exports include headers, values, and units in CSV format.
The UI exposes Save project, Load project, and Export CSV actions with
explicit failure messages.

### Milestone 6 — Comparison and sensitivity

Allow two or more saved configurations to be compared. Add sensitivity
analysis for one selected input, such as switching frequency or output power.

**Definition of done:** the comparison identifies the best result according
to a documented metric and charts use the same units and scales as the
dashboard.

**Implemented:** `PowerModuleAnalysisService` compares total loss and thermal
margin using a dominance rule, and generates switching-frequency sensitivity
points for loss, efficiency, and thermal margin.

### Milestone 7 — Quality and portfolio release

- Add XML documentation for public Core APIs.
- Add architecture and calculation-model documentation.
- Add keyboard navigation and accessible labels.
- Add structured logging for unexpected failures.
- Add screenshots and a short demonstration.
- Review warnings, dead code, and dependency licenses.

**Definition of done:** a new developer can clone the repository, build it,
run the tests, and understand the model limitations from the documentation.

## 4. Git workflow

Use short branches named after a vertical slice:

```text
feature/core-calculation-engine
feature/configuration-screen
feature/results-dashboard
```

Each pull request should contain:

- A focused change.
- Tests for changed behaviour.
- A short description of the user-visible result.
- Evidence that `dotnet build` and `dotnet test` pass.

Do not merge with failing CI or unexplained test skips.

## 5. Verification gates

Run these gates before each milestone is marked complete:

1. `dotnet format --verify-no-changes`
2. `dotnet build --configuration Release`
3. `dotnet test --configuration Release`
4. Manual smoke test of the affected user journey.
5. Documentation update for changed formulas or behaviour.

## 6. Initial backlog

- [ ] Install .NET 8 SDK and Avalonia templates.
- [ ] Create the solution and project structure.
- [ ] Add nullable reference types and analyzers.
- [ ] Implement input and result models.
- [ ] Implement validation.
- [ ] Implement and test the calculation service.
- [ ] Build the configuration screen.
- [ ] Build the results dashboard.
- [ ] Add JSON persistence and CSV export.
- [ ] Add comparison and sensitivity analysis.
- [ ] Complete documentation and portfolio demo.
