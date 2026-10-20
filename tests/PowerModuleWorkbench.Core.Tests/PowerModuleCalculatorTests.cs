using PowerModuleWorkbench.Core.Calculations;
using PowerModuleWorkbench.Core.Domain;

namespace PowerModuleWorkbench.Core.Tests;

public sealed class PowerModuleCalculatorTests
{
    private readonly PowerModuleCalculator calculator = new();

    [Fact]
    public void CalculateReturnsExpectedValuesForValidInput()
    {
        var input = CreateValidInput();

        var result = calculator.Calculate(input);

        Assert.Equal(125, result.EstimatedRmsCurrentA, precision: 10);
        Assert.Equal(156.25, result.ConductionLossW, precision: 10);
        Assert.Equal(200, result.SwitchingLossW, precision: 10);
        Assert.Equal(356.25, result.TotalLossW, precision: 10);
        Assert.Equal(99.6450146354, result.EfficiencyPercent, precision: 10);
        Assert.Equal(42.8125, result.EstimatedJunctionTemperatureC, precision: 10);
        Assert.Equal(107.1875, result.ThermalMarginC, precision: 10);
        Assert.True(result.IsWithinThermalLimit);
        Assert.Empty(result.Warnings);
    }

    [Fact]
    public void CalculateRejectsInvalidInputInsteadOfCalculating()
    {
        var input = CreateValidInput() with { OutputPowerW = 0 };

        var exception = Assert.Throws<ArgumentException>(() => calculator.Calculate(input));

        Assert.Contains("Output power must be greater than 0 W.", exception.Message);
    }

    [Fact]
    public void CalculateSwitchingLossIncreasesWithSwitchingFrequency()
    {
        var lowerFrequency = calculator.Calculate(
            CreateValidInput() with { SwitchingFrequencyHz = 5_000 });
        var higherFrequency = calculator.Calculate(
            CreateValidInput() with { SwitchingFrequencyHz = 10_000 });

        Assert.True(higherFrequency.SwitchingLossW > lowerFrequency.SwitchingLossW);
        Assert.True(higherFrequency.TotalLossW > lowerFrequency.TotalLossW);
        Assert.True(higherFrequency.EfficiencyPercent < lowerFrequency.EfficiencyPercent);
    }

    [Fact]
    public void CalculateReportsWarningWhenThermalLimitIsExceeded()
    {
        var input = CreateValidInput() with
        {
            ThermalResistanceCPerW = 1,
            MaximumJunctionTemperatureC = 100,
        };

        var result = calculator.Calculate(input);

        Assert.False(result.IsWithinThermalLimit);
        Assert.True(result.ThermalMarginC < 0);
        var warning = Assert.Single(result.Warnings);
        Assert.Equal("JUNCTION_TEMPERATURE_LIMIT_EXCEEDED", warning.Code);
        Assert.Equal(ValidationSeverity.Warning, warning.Severity);
    }

    [Fact]
    public void CalculateKeepsEfficiencyBetweenZeroAndOneHundredPercent()
    {
        var result = calculator.Calculate(CreateValidInput());

        Assert.InRange(result.EfficiencyPercent, 0, 100);
    }

    private static PowerModuleInput CreateValidInput() =>
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
