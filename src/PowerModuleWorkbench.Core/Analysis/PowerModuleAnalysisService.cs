using PowerModuleWorkbench.Core.Calculations;
using PowerModuleWorkbench.Core.Domain;

namespace PowerModuleWorkbench.Core.Analysis;

public sealed class PowerModuleAnalysisService
{
    private readonly IPowerModuleCalculator calculator;

    public PowerModuleAnalysisService(IPowerModuleCalculator calculator)
    {
        this.calculator = calculator ?? throw new ArgumentNullException(nameof(calculator));
    }

    public PowerModuleComparison Compare(
        PowerModuleInput firstInput,
        PowerModuleInput secondInput)
    {
        ArgumentNullException.ThrowIfNull(firstInput);
        ArgumentNullException.ThrowIfNull(secondInput);

        var firstResult = calculator.Calculate(firstInput);
        var secondResult = calculator.Calculate(secondInput);
        var preferredConfiguration = CompareResults(firstResult, secondResult);

        return new PowerModuleComparison(
            firstResult,
            secondResult,
            preferredConfiguration);
    }

    public IReadOnlyList<SensitivityPoint> AnalyzeSwitchingFrequency(
        PowerModuleInput input,
        int pointCount = 7)
    {
        ArgumentNullException.ThrowIfNull(input);
        if (pointCount < 2)
        {
            throw new ArgumentOutOfRangeException(nameof(pointCount), "At least two points are required.");
        }

        var points = new List<SensitivityPoint>(pointCount);
        var minimumFrequency = input.SwitchingFrequencyHz * 0.5;
        var step = input.SwitchingFrequencyHz / (pointCount - 1);
        for (var index = 0; index < pointCount; index++)
        {
            var frequency = minimumFrequency + index * step;
            var result = calculator.Calculate(input with { SwitchingFrequencyHz = frequency });
            points.Add(new SensitivityPoint(
                frequency,
                result.TotalLossW,
                result.EfficiencyPercent,
                result.ThermalMarginC));
        }

        return points;
    }

    private static string CompareResults(
        PowerModuleResult firstResult,
        PowerModuleResult secondResult)
    {
        if (firstResult.TotalLossW < secondResult.TotalLossW
            && firstResult.ThermalMarginC >= secondResult.ThermalMarginC)
        {
            return "Configuration A is preferred: lower loss and equal or better thermal margin.";
        }

        if (secondResult.TotalLossW < firstResult.TotalLossW
            && secondResult.ThermalMarginC >= firstResult.ThermalMarginC)
        {
            return "Configuration B is preferred: lower loss and equal or better thermal margin.";
        }

        return "No single configuration dominates on both total loss and thermal margin.";
    }
}
