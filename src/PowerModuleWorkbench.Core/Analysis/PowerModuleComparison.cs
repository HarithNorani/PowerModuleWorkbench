using PowerModuleWorkbench.Core.Domain;

namespace PowerModuleWorkbench.Core.Analysis;

public sealed record PowerModuleComparison(
    PowerModuleResult FirstResult,
    PowerModuleResult SecondResult,
    string PreferredConfiguration);

public sealed record SensitivityPoint(
    double SwitchingFrequencyHz,
    double TotalLossW,
    double EfficiencyPercent,
    double ThermalMarginC);
