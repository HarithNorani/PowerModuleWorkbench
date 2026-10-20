using PowerModuleWorkbench.Core.Domain;

namespace PowerModuleWorkbench.Core.Calculations;

/// <summary>
/// Calculates an intentionally simplified estimate for a power module.
/// </summary>
/// <remarks>
/// RMS current is approximated as output power divided by DC-link voltage.
/// Conduction loss uses a resistive model, and switching loss uses a constant
/// switching energy per event. These estimates are not a substitute for
/// validated semiconductor or thermal simulation.
/// </remarks>
public sealed class PowerModuleCalculator : IPowerModuleCalculator
{
    public PowerModuleResult Calculate(PowerModuleInput input)
    {
        ArgumentNullException.ThrowIfNull(input);

        var validationIssues = PowerModuleInputValidator.Validate(input);
        if (validationIssues.Count > 0)
        {
            throw new ArgumentException(
                $"Cannot calculate with invalid input: {string.Join(" ", validationIssues.Select(issue => issue.Message))}",
                nameof(input));
        }

        var estimatedRmsCurrentA = input.OutputPowerW / input.DcLinkVoltageV;
        var conductionLossW = Math.Pow(estimatedRmsCurrentA, 2) * input.ConductionResistanceOhm;
        var switchingLossW = input.SwitchingEnergyJ * input.SwitchingFrequencyHz;
        var totalLossW = conductionLossW + switchingLossW;
        var estimatedJunctionTemperatureC =
            input.AmbientTemperatureC + (totalLossW * input.ThermalResistanceCPerW);
        var efficiencyPercent = input.OutputPowerW / (input.OutputPowerW + totalLossW) * 100;
        var thermalMarginC =
            input.MaximumJunctionTemperatureC - estimatedJunctionTemperatureC;

        EnsureFiniteResults(
            estimatedRmsCurrentA,
            conductionLossW,
            switchingLossW,
            totalLossW,
            estimatedJunctionTemperatureC,
            efficiencyPercent,
            thermalMarginC);

        var warnings = thermalMarginC < 0
            ? new[]
            {
                new ValidationIssue(
                    "JUNCTION_TEMPERATURE_LIMIT_EXCEEDED",
                    nameof(PowerModuleInput.MaximumJunctionTemperatureC),
                    "Estimated junction temperature exceeds the configured maximum.",
                    ValidationSeverity.Warning),
            }
            : Array.Empty<ValidationIssue>();

        return new PowerModuleResult
        {
            EstimatedRmsCurrentA = estimatedRmsCurrentA,
            ConductionLossW = conductionLossW,
            SwitchingLossW = switchingLossW,
            TotalLossW = totalLossW,
            EfficiencyPercent = efficiencyPercent,
            EstimatedJunctionTemperatureC = estimatedJunctionTemperatureC,
            ThermalMarginC = thermalMarginC,
            IsWithinThermalLimit = thermalMarginC >= 0,
            Warnings = warnings,
        };
    }

    private static void EnsureFiniteResults(params double[] values)
    {
        if (values.Any(value => !double.IsFinite(value)))
        {
            throw new InvalidOperationException(
                "The calculation produced a non-finite result. Review the input range.");
        }
    }
}
