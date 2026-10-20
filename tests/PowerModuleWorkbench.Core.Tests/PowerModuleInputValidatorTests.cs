using PowerModuleWorkbench.Core.Domain;

namespace PowerModuleWorkbench.Core.Tests;

public sealed class PowerModuleInputValidatorTests
{
    [Fact]
    public void ValidateValidInputReturnsNoIssues()
    {
        var issues = PowerModuleInputValidator.Validate(CreateValidInput());

        Assert.Empty(issues);
    }

    [Fact]
    public void ValidateInvalidInputReturnsAllFieldIssues()
    {
        var input = new PowerModuleInput
        {
            DcLinkVoltageV = 0,
            OutputPowerW = -1,
            SwitchingFrequencyHz = double.NaN,
            AmbientTemperatureC = double.PositiveInfinity,
            ThermalResistanceCPerW = 0,
            ConductionResistanceOhm = -0.1,
            SwitchingEnergyJ = -0.01,
            MaximumJunctionTemperatureC = double.NaN,
        };

        var issues = PowerModuleInputValidator.Validate(input);

        Assert.Equal(8, issues.Count);
        Assert.Contains(issues, issue => issue.PropertyName == nameof(input.DcLinkVoltageV));
        Assert.Contains(issues, issue => issue.PropertyName == nameof(input.OutputPowerW));
        Assert.Contains(issues, issue => issue.PropertyName == nameof(input.SwitchingFrequencyHz));
        Assert.Contains(issues, issue => issue.PropertyName == nameof(input.AmbientTemperatureC));
        Assert.Contains(issues, issue => issue.PropertyName == nameof(input.ThermalResistanceCPerW));
        Assert.Contains(issues, issue => issue.PropertyName == nameof(input.ConductionResistanceOhm));
        Assert.Contains(issues, issue => issue.PropertyName == nameof(input.SwitchingEnergyJ));
        Assert.Contains(issues, issue => issue.PropertyName == nameof(input.MaximumJunctionTemperatureC));
    }

    [Fact]
    public void ValidateTemperatureLimitAtAmbientTemperatureReturnsIssue()
    {
        var input = CreateValidInput() with
        {
            MaximumJunctionTemperatureC = 40,
            AmbientTemperatureC = 40,
        };

        var issues = PowerModuleInputValidator.Validate(input);

        var issue = Assert.Single(issues);
        Assert.Equal("THERMAL_LIMIT_BELOW_AMBIENT", issue.Code);
        Assert.Equal(nameof(input.MaximumJunctionTemperatureC), issue.PropertyName);
    }

    [Fact]
    public void ValidateZeroSwitchingEnergyIsAllowed()
    {
        var input = CreateValidInput() with { SwitchingEnergyJ = 0 };

        var issues = PowerModuleInputValidator.Validate(input);

        Assert.Empty(issues);
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
