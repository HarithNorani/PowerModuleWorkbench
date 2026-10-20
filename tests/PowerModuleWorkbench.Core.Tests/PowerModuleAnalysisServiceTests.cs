using PowerModuleWorkbench.Core.Analysis;
using PowerModuleWorkbench.Core.Calculations;
using PowerModuleWorkbench.Core.Domain;

namespace PowerModuleWorkbench.Core.Tests;

public sealed class PowerModuleAnalysisServiceTests
{
    private readonly PowerModuleAnalysisService service =
        new(new PowerModuleCalculator());

    [Fact]
    public void ComparePrefersConfigurationWithLowerLossAndBetterMargin()
    {
        var first = CreateInput();
        var second = first with
        {
            SwitchingFrequencyHz = 20_000,
            ThermalResistanceCPerW = 0.1,
        };

        var comparison = service.Compare(first, second);

        Assert.StartsWith("Configuration A is preferred", comparison.PreferredConfiguration);
        Assert.True(comparison.FirstResult.TotalLossW < comparison.SecondResult.TotalLossW);
        Assert.True(comparison.FirstResult.ThermalMarginC > comparison.SecondResult.ThermalMarginC);
    }

    [Fact]
    public void CompareReportsTradeoffWhenNoConfigurationDominates()
    {
        var first = CreateInput() with { ThermalResistanceCPerW = 0.05 };
        var second = CreateInput() with
        {
            SwitchingFrequencyHz = 5_000,
            ThermalResistanceCPerW = 0.2,
        };

        var comparison = service.Compare(first, second);

        Assert.Equal(
            "No single configuration dominates on both total loss and thermal margin.",
            comparison.PreferredConfiguration);
    }

    [Fact]
    public void AnalyzeSwitchingFrequencyReturnsMonotonicLossSensitivity()
    {
        var points = service.AnalyzeSwitchingFrequency(CreateInput());

        Assert.Equal(7, points.Count);
        Assert.Equal(5_000, points[0].SwitchingFrequencyHz);
        Assert.Equal(15_000, points[^1].SwitchingFrequencyHz);
        Assert.True(points[0].TotalLossW < points[^1].TotalLossW);
        Assert.True(points[0].EfficiencyPercent > points[^1].EfficiencyPercent);
        Assert.True(points[0].ThermalMarginC > points[^1].ThermalMarginC);
    }

    private static PowerModuleInput CreateInput() =>
        new()
        {
            DcLinkVoltageV = 800,
            OutputPowerW = 100_000,
            SwitchingFrequencyHz = 10_000,
            AmbientTemperatureC = 25,
            ThermalResistanceCPerW = 0.05,
            ConductionResistanceOhm = 0.01,
            SwitchingEnergyJ = 0.02,
            MaximumJunctionTemperatureC = 150,
        };
}
