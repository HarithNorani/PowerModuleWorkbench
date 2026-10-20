using PowerModuleWorkbench.Core.Calculations;
using PowerModuleWorkbench.Core.Domain;
using PowerModuleWorkbench.Infrastructure.Export;
using PowerModuleWorkbench.Infrastructure.Persistence;

namespace PowerModuleWorkbench.Core.Tests;

public sealed class PowerModulePersistenceTests
{
    [Fact]
    public async Task ProjectStorePreservesInputAndResultOnRoundTrip()
    {
        var filePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}", "project.json");
        var input = CreateValidInput();
        var result = new PowerModuleCalculator().Calculate(input);
        await PowerModuleProjectStore.SaveAsync(filePath, input, result);
        var loaded = await PowerModuleProjectStore.LoadAsync(filePath);

        Assert.Equal(1, loaded.SchemaVersion);
        Assert.Equal(input, loaded.Input);
        Assert.NotNull(loaded.Result);
        Assert.Equal(result.TotalLossW, loaded.Result.TotalLossW);
        Assert.Equal(result.EfficiencyPercent, loaded.Result.EfficiencyPercent);
        Assert.Equal(result.EstimatedJunctionTemperatureC, loaded.Result.EstimatedJunctionTemperatureC);
        Assert.Equal(result.ThermalMarginC, loaded.Result.ThermalMarginC);
    }

    [Fact]
    public async Task ProjectStoreRejectsUnknownSchemaVersion()
    {
        var filePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}", "project.json");
        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
        await File.WriteAllTextAsync(
            filePath,
            """{"schemaVersion":99,"input":{"dcLinkVoltageV":800,"outputPowerW":100000,"switchingFrequencyHz":10000,"ambientTemperatureC":25,"thermalResistanceCPerW":0.05,"conductionResistanceOhm":0.01,"switchingEnergyJ":0.02,"maximumJunctionTemperatureC":150}}""");

        var exception = await Assert.ThrowsAsync<InvalidDataException>(
            () => PowerModuleProjectStore.LoadAsync(filePath));

        Assert.Contains("not supported", exception.Message);
    }

    [Fact]
    public async Task CsvExporterWritesHeadersUnitsAndResults()
    {
        var filePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}", "results.csv");
        var input = CreateValidInput();
        var result = new PowerModuleCalculator().Calculate(input);

        await PowerModuleCsvExporter.ExportAsync(filePath, input, result);
        var csv = await File.ReadAllTextAsync(filePath);

        Assert.Contains("Metric,Value,Unit", csv);
        Assert.Contains("Total loss,356.25,W", csv);
        Assert.Contains("Efficiency,99.64501463536153,%", csv);
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
