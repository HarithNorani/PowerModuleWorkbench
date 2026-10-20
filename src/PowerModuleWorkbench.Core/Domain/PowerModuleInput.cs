namespace PowerModuleWorkbench.Core.Domain;

public sealed record PowerModuleInput
{
    public double DcLinkVoltageV { get; init; }

    public double OutputPowerW { get; init; }

    public double SwitchingFrequencyHz { get; init; }

    public double AmbientTemperatureC { get; init; }

    public double ThermalResistanceCPerW { get; init; }

    public double ConductionResistanceOhm { get; init; }

    public double SwitchingEnergyJ { get; init; }

    public double MaximumJunctionTemperatureC { get; init; }
}
