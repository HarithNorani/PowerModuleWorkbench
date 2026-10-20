namespace PowerModuleWorkbench.Core.Domain;

public sealed record PowerModuleResult
{
    public required double EstimatedRmsCurrentA { get; init; }

    public required double ConductionLossW { get; init; }

    public required double SwitchingLossW { get; init; }

    public required double TotalLossW { get; init; }

    public required double EfficiencyPercent { get; init; }

    public required double EstimatedJunctionTemperatureC { get; init; }

    public required double ThermalMarginC { get; init; }

    public required bool IsWithinThermalLimit { get; init; }

    public required IReadOnlyList<ValidationIssue> Warnings { get; init; }
}
