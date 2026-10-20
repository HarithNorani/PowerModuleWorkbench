using System.Globalization;
using PowerModuleWorkbench.Core.Domain;

namespace PowerModuleWorkbench.Infrastructure.Export;

public sealed class PowerModuleCsvExporter
{
    public static async Task ExportAsync(
        string filePath,
        PowerModuleInput input,
        PowerModuleResult result,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(result);

        var directory = Path.GetDirectoryName(Path.GetFullPath(filePath));
        if (directory is not null)
        {
            Directory.CreateDirectory(directory);
        }

        var lines = new[]
        {
            "Metric,Value,Unit",
            $"DC-link voltage,{input.DcLinkVoltageV.ToString(CultureInfo.InvariantCulture)},V",
            $"Output power,{input.OutputPowerW.ToString(CultureInfo.InvariantCulture)},W",
            $"Switching frequency,{input.SwitchingFrequencyHz.ToString(CultureInfo.InvariantCulture)},Hz",
            $"Ambient temperature,{input.AmbientTemperatureC.ToString(CultureInfo.InvariantCulture)},°C",
            $"Estimated RMS current,{result.EstimatedRmsCurrentA.ToString(CultureInfo.InvariantCulture)},A",
            $"Conduction loss,{result.ConductionLossW.ToString(CultureInfo.InvariantCulture)},W",
            $"Switching loss,{result.SwitchingLossW.ToString(CultureInfo.InvariantCulture)},W",
            $"Total loss,{result.TotalLossW.ToString(CultureInfo.InvariantCulture)},W",
            $"Efficiency,{result.EfficiencyPercent.ToString(CultureInfo.InvariantCulture)},%",
            $"Estimated junction temperature,{result.EstimatedJunctionTemperatureC.ToString(CultureInfo.InvariantCulture)},°C",
            $"Thermal margin,{result.ThermalMarginC.ToString(CultureInfo.InvariantCulture)},°C",
        };

        await File.WriteAllLinesAsync(filePath, lines, cancellationToken);
    }
}
