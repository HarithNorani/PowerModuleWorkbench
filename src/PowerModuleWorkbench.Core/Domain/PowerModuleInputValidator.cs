namespace PowerModuleWorkbench.Core.Domain;

public static class PowerModuleInputValidator
{
    public static IReadOnlyList<ValidationIssue> Validate(PowerModuleInput input)
    {
        ArgumentNullException.ThrowIfNull(input);

        var issues = new List<ValidationIssue>();

        AddPositiveIssue(
            issues,
            input.DcLinkVoltageV,
            nameof(input.DcLinkVoltageV),
            "DC-link voltage must be greater than 0 V.");
        AddPositiveIssue(
            issues,
            input.OutputPowerW,
            nameof(input.OutputPowerW),
            "Output power must be greater than 0 W.");
        AddPositiveIssue(
            issues,
            input.SwitchingFrequencyHz,
            nameof(input.SwitchingFrequencyHz),
            "Switching frequency must be greater than 0 Hz.");
        AddFiniteIssue(
            issues,
            input.AmbientTemperatureC,
            nameof(input.AmbientTemperatureC),
            "Ambient temperature must be a finite value in °C.");
        AddPositiveIssue(
            issues,
            input.ThermalResistanceCPerW,
            nameof(input.ThermalResistanceCPerW),
            "Thermal resistance must be greater than 0 °C/W.");
        AddPositiveIssue(
            issues,
            input.ConductionResistanceOhm,
            nameof(input.ConductionResistanceOhm),
            "Conduction resistance must be greater than 0 Ω.");
        AddNonNegativeIssue(
            issues,
            input.SwitchingEnergyJ,
            nameof(input.SwitchingEnergyJ),
            "Switching energy must be 0 J or greater.");
        AddFiniteIssue(
            issues,
            input.MaximumJunctionTemperatureC,
            nameof(input.MaximumJunctionTemperatureC),
            "Maximum junction temperature must be a finite value in °C.");

        if (double.IsFinite(input.AmbientTemperatureC)
            && double.IsFinite(input.MaximumJunctionTemperatureC)
            && input.MaximumJunctionTemperatureC <= input.AmbientTemperatureC)
        {
            issues.Add(new ValidationIssue(
                "THERMAL_LIMIT_BELOW_AMBIENT",
                nameof(input.MaximumJunctionTemperatureC),
                "Maximum junction temperature must be greater than ambient temperature."));
        }

        return issues;
    }

    private static void AddFiniteIssue(
        List<ValidationIssue> issues,
        double value,
        string propertyName,
        string message)
    {
        if (!double.IsFinite(value))
        {
            issues.Add(new ValidationIssue("VALUE_NOT_FINITE", propertyName, message));
        }
    }

    private static void AddPositiveIssue(
        List<ValidationIssue> issues,
        double value,
        string propertyName,
        string message)
    {
        if (!double.IsFinite(value) || value <= 0)
        {
            issues.Add(new ValidationIssue("VALUE_MUST_BE_POSITIVE", propertyName, message));
        }
    }

    private static void AddNonNegativeIssue(
        List<ValidationIssue> issues,
        double value,
        string propertyName,
        string message)
    {
        if (!double.IsFinite(value) || value < 0)
        {
            issues.Add(new ValidationIssue("VALUE_MUST_BE_NON_NEGATIVE", propertyName, message));
        }
    }
}
