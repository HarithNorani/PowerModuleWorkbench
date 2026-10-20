using System.Globalization;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using PowerModuleWorkbench.Core.Analysis;
using PowerModuleWorkbench.Core.Calculations;
using PowerModuleWorkbench.Core.Domain;
using PowerModuleWorkbench.Infrastructure.Export;
using PowerModuleWorkbench.Infrastructure.Persistence;

namespace PowerModuleWorkbench.App.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private static readonly Action<ILogger, string, Exception?> ProjectSaveFailed =
        LoggerMessage.Define<string>(
            LogLevel.Error,
            new EventId(1, nameof(ProjectSaveFailed)),
            "Unable to save project file {ProjectFilePath}");

    private static readonly Action<ILogger, string, Exception?> ProjectLoadFailed =
        LoggerMessage.Define<string>(
            LogLevel.Error,
            new EventId(2, nameof(ProjectLoadFailed)),
            "Unable to load project file {ProjectFilePath}");

    private static readonly Action<ILogger, string, Exception?> ResultExportFailed =
        LoggerMessage.Define<string>(
            LogLevel.Error,
            new EventId(3, nameof(ResultExportFailed)),
            "Unable to export result file {ExportFilePath}");

    private static readonly Action<ILogger, string, string, Exception?> ProjectComparisonFailed =
        LoggerMessage.Define<string, string>(
            LogLevel.Error,
            new EventId(4, nameof(ProjectComparisonFailed)),
            "Unable to compare project files {ProjectFilePath} and {ComparisonProjectFilePath}");

    private readonly PowerModuleCalculator calculator = new();
    private readonly PowerModuleAnalysisService analysisService;
    private readonly ILogger<MainWindowViewModel> logger;
    private PowerModuleInput? lastInput;
    private PowerModuleResult? lastResult;

    public MainWindowViewModel()
    {
        analysisService = new PowerModuleAnalysisService(calculator);
        logger = NullLogger<MainWindowViewModel>.Instance;
    }

    [ObservableProperty]
    private string dcLinkVoltageV = "800";

    [ObservableProperty]
    private string outputPowerW = "100000";

    [ObservableProperty]
    private string switchingFrequencyHz = "10000";

    [ObservableProperty]
    private string ambientTemperatureC = "25";

    [ObservableProperty]
    private string thermalResistanceCPerW = "0.05";

    [ObservableProperty]
    private string conductionResistanceOhm = "0.01";

    [ObservableProperty]
    private string switchingEnergyJ = "0.02";

    [ObservableProperty]
    private string maximumJunctionTemperatureC = "150";

    [ObservableProperty]
    private string validationMessage = string.Empty;

    [ObservableProperty]
    private bool hasValidationMessage;

    [ObservableProperty]
    private bool hasResult;

    [ObservableProperty]
    private string estimatedRmsCurrentA = "-";

    [ObservableProperty]
    private string conductionLossW = "-";

    [ObservableProperty]
    private string switchingLossW = "-";

    [ObservableProperty]
    private string totalLossW = "-";

    [ObservableProperty]
    private string efficiencyPercent = "-";

    [ObservableProperty]
    private string estimatedJunctionTemperatureC = "-";

    [ObservableProperty]
    private string thermalMarginC = "-";

    [ObservableProperty]
    private string thermalStatus = "No calculation yet";

    [ObservableProperty]
    private string projectFilePath = "power-module-project.json";

    [ObservableProperty]
    private string exportFilePath = "power-module-results.csv";

    [ObservableProperty]
    private string comparisonProjectFilePath = "power-module-project-b.json";

    [ObservableProperty]
    private bool hasComparison;

    [ObservableProperty]
    private string comparisonSummary = "Load two project files to compare them.";

    [ObservableProperty]
    private string comparisonMetrics = string.Empty;

    [ObservableProperty]
    private PlotModel lossChart = new()
    {
        Title = "Losses across output power",
        IsLegendVisible = true,
    };

    [ObservableProperty]
    private PlotModel sensitivityChart = new()
    {
        Title = "Switching-frequency sensitivity",
        IsLegendVisible = true,
    };

    [RelayCommand]
    private void Calculate()
    {
        if (!TryCreateInput(out var input, out var issues))
        {
            SetValidationMessage(issues);
            HasResult = false;
            return;
        }

        try
        {
            var result = calculator.Calculate(input);
            ClearValidationMessage();
            SetResult(result);
            lastInput = input;
            lastResult = result;
        }
        catch (ArgumentException exception)
        {
            SetValidationMessage(new[]
            {
                new ValidationIssue("CALCULATION_INPUT_INVALID", string.Empty, exception.Message),
            });
            HasResult = false;
        }
    }

    [RelayCommand]
    private async Task SaveProjectAsync()
    {
        if (!TryCreateInput(out var input, out var issues))
        {
            SetValidationMessage(issues);
            return;
        }

        try
        {
            await PowerModuleProjectStore.SaveAsync(ProjectFilePath, input, lastResult);
            ClearValidationMessage();
        }
        catch (IOException exception)
        {
            ProjectSaveFailed(logger, ProjectFilePath, exception);
            SetValidationMessage(new[]
            {
                new ValidationIssue("PROJECT_SAVE_FAILED", string.Empty, $"Could not save project: {exception.Message}"),
            });
        }
    }

    [RelayCommand]
    private async Task LoadProjectAsync()
    {
        try
        {
            var project = await PowerModuleProjectStore.LoadAsync(ProjectFilePath);
            ApplyInput(project.Input);
            ClearValidationMessage();
            if (project.Result is not null)
            {
                SetResult(project.Result);
                lastInput = project.Input;
                lastResult = project.Result;
            }
        }
        catch (Exception exception) when (
            exception is IOException
            or InvalidDataException
            or JsonException)
        {
            ProjectLoadFailed(logger, ProjectFilePath, exception);
            SetValidationMessage(new[]
            {
                new ValidationIssue("PROJECT_LOAD_FAILED", string.Empty, $"Could not load project: {exception.Message}"),
            });
        }
    }

    [RelayCommand]
    private async Task ExportResultsAsync()
    {
        if (lastInput is null || lastResult is null)
        {
            SetValidationMessage(new[]
            {
                new ValidationIssue("NO_RESULT_TO_EXPORT", string.Empty, "Calculate a valid result before exporting."),
            });
            return;
        }

        try
        {
            await PowerModuleCsvExporter.ExportAsync(ExportFilePath, lastInput, lastResult);
            ClearValidationMessage();
        }
        catch (IOException exception)
        {
            ResultExportFailed(logger, ExportFilePath, exception);
            SetValidationMessage(new[]
            {
                new ValidationIssue("RESULT_EXPORT_FAILED", string.Empty, $"Could not export results: {exception.Message}"),
            });
        }
    }

    [RelayCommand]
    private async Task CompareProjectsAsync()
    {
        try
        {
            var firstProject = await PowerModuleProjectStore.LoadAsync(ProjectFilePath);
            var secondProject = await PowerModuleProjectStore.LoadAsync(ComparisonProjectFilePath);
            var comparison = analysisService.Compare(firstProject.Input, secondProject.Input);
            var sensitivity = analysisService.AnalyzeSwitchingFrequency(firstProject.Input);

            ComparisonSummary = comparison.PreferredConfiguration;
            ComparisonMetrics =
                $"A: {comparison.FirstResult.TotalLossW:N2} W loss | " +
                $"{comparison.FirstResult.EfficiencyPercent:N3} % efficiency | " +
                $"{comparison.FirstResult.ThermalMarginC:N2} °C margin{Environment.NewLine}" +
                $"B: {comparison.SecondResult.TotalLossW:N2} W loss | " +
                $"{comparison.SecondResult.EfficiencyPercent:N3} % efficiency | " +
                $"{comparison.SecondResult.ThermalMarginC:N2} °C margin";
            SensitivityChart = CreateSensitivityChart(sensitivity);
            HasComparison = true;
            ClearValidationMessage();
        }
        catch (Exception exception) when (
            exception is IOException
            or InvalidDataException
            or JsonException
            or ArgumentException)
        {
            ProjectComparisonFailed(logger, ProjectFilePath, ComparisonProjectFilePath, exception);
            SetValidationMessage(new[]
            {
                new ValidationIssue(
                    "PROJECT_COMPARISON_FAILED",
                    string.Empty,
                    $"Could not compare projects: {exception.Message}"),
            });
            HasComparison = false;
        }
    }

    private bool TryCreateInput(
        out PowerModuleInput input,
        out IReadOnlyList<ValidationIssue> issues)
    {
        var parseIssues = new List<ValidationIssue>();
        var values = new double[8];
        var rawValues = new[]
        {
            (nameof(DcLinkVoltageV), DcLinkVoltageV, "DC-link voltage must be a number in V."),
            (nameof(OutputPowerW), OutputPowerW, "Output power must be a number in W."),
            (nameof(SwitchingFrequencyHz), SwitchingFrequencyHz, "Switching frequency must be a number in Hz."),
            (nameof(AmbientTemperatureC), AmbientTemperatureC, "Ambient temperature must be a number in °C."),
            (nameof(ThermalResistanceCPerW), ThermalResistanceCPerW, "Thermal resistance must be a number in °C/W."),
            (nameof(ConductionResistanceOhm), ConductionResistanceOhm, "Conduction resistance must be a number in Ω."),
            (nameof(SwitchingEnergyJ), SwitchingEnergyJ, "Switching energy must be a number in J."),
            (nameof(MaximumJunctionTemperatureC), MaximumJunctionTemperatureC, "Maximum junction temperature must be a number in °C."),
        };

        for (var index = 0; index < rawValues.Length; index++)
        {
            if (!double.TryParse(
                    rawValues[index].Item2,
                    NumberStyles.Float,
                    CultureInfo.InvariantCulture,
                    out values[index]))
            {
                parseIssues.Add(new ValidationIssue(
                    "VALUE_NOT_NUMERIC",
                    rawValues[index].Item1,
                    rawValues[index].Item3));
            }
        }

        input = new PowerModuleInput
        {
            DcLinkVoltageV = values[0],
            OutputPowerW = values[1],
            SwitchingFrequencyHz = values[2],
            AmbientTemperatureC = values[3],
            ThermalResistanceCPerW = values[4],
            ConductionResistanceOhm = values[5],
            SwitchingEnergyJ = values[6],
            MaximumJunctionTemperatureC = values[7],
        };

        issues = parseIssues.Count > 0
            ? parseIssues.Concat(PowerModuleInputValidator.Validate(input)).ToArray()
            : PowerModuleInputValidator.Validate(input);
        return issues.Count == 0;
    }

    private void SetResult(PowerModuleResult result)
    {
        EstimatedRmsCurrentA = $"{result.EstimatedRmsCurrentA:N2} A";
        ConductionLossW = $"{result.ConductionLossW:N2} W";
        SwitchingLossW = $"{result.SwitchingLossW:N2} W";
        TotalLossW = $"{result.TotalLossW:N2} W";
        EfficiencyPercent = $"{result.EfficiencyPercent:N3} %";
        EstimatedJunctionTemperatureC = $"{result.EstimatedJunctionTemperatureC:N2} °C";
        ThermalMarginC = $"{result.ThermalMarginC:N2} °C";
        ThermalStatus = result.IsWithinThermalLimit
            ? "PASS: Within thermal limit"
            : "WARNING: Thermal limit exceeded";
        LossChart = CreateLossChart(ParseInputOrThrow());
        HasResult = true;
    }

    private void ApplyInput(PowerModuleInput input)
    {
        DcLinkVoltageV = input.DcLinkVoltageV.ToString(CultureInfo.InvariantCulture);
        OutputPowerW = input.OutputPowerW.ToString(CultureInfo.InvariantCulture);
        SwitchingFrequencyHz = input.SwitchingFrequencyHz.ToString(CultureInfo.InvariantCulture);
        AmbientTemperatureC = input.AmbientTemperatureC.ToString(CultureInfo.InvariantCulture);
        ThermalResistanceCPerW = input.ThermalResistanceCPerW.ToString(CultureInfo.InvariantCulture);
        ConductionResistanceOhm = input.ConductionResistanceOhm.ToString(CultureInfo.InvariantCulture);
        SwitchingEnergyJ = input.SwitchingEnergyJ.ToString(CultureInfo.InvariantCulture);
        MaximumJunctionTemperatureC = input.MaximumJunctionTemperatureC.ToString(CultureInfo.InvariantCulture);
    }

    private PlotModel CreateLossChart(PowerModuleInput input)
    {
        var model = new PlotModel
        {
            Title = "Losses across output power",
            IsLegendVisible = true,
        };
        model.Axes.Add(new LinearAxis
        {
            Position = AxisPosition.Bottom,
            Title = "Output power (kW)",
            Minimum = input.OutputPowerW * 0.5 / 1000,
            Maximum = input.OutputPowerW * 1.5 / 1000,
        });
        model.Axes.Add(new LinearAxis
        {
            Position = AxisPosition.Left,
            Title = "Losses (W)",
        });

        var conductionSeries = new LineSeries { Title = "Conduction loss" };
        var switchingSeries = new LineSeries { Title = "Switching loss" };
        var totalSeries = new LineSeries { Title = "Total loss", StrokeThickness = 3 };
        for (var index = 0; index <= 10; index++)
        {
            var outputPowerW = input.OutputPowerW * (0.5 + index * 0.1);
            var pointInput = input with { OutputPowerW = outputPowerW };
            var pointResult = calculator.Calculate(pointInput);
            var outputPowerKw = outputPowerW / 1000;
            conductionSeries.Points.Add(new DataPoint(outputPowerKw, pointResult.ConductionLossW));
            switchingSeries.Points.Add(new DataPoint(outputPowerKw, pointResult.SwitchingLossW));
            totalSeries.Points.Add(new DataPoint(outputPowerKw, pointResult.TotalLossW));
        }

        model.Series.Add(conductionSeries);
        model.Series.Add(switchingSeries);
        model.Series.Add(totalSeries);
        return model;
    }

    private static PlotModel CreateSensitivityChart(
        IReadOnlyList<SensitivityPoint> points)
    {
        var model = new PlotModel
        {
            Title = "Switching-frequency sensitivity",
            IsLegendVisible = true,
        };
        model.Axes.Add(new LinearAxis
        {
            Position = AxisPosition.Bottom,
            Title = "Switching frequency (kHz)",
        });
        model.Axes.Add(new LinearAxis
        {
            Position = AxisPosition.Left,
            Title = "Total loss (W)",
        });

        var totalLossSeries = new LineSeries
        {
            Title = "Total loss",
            StrokeThickness = 3,
        };
        foreach (var point in points)
        {
            totalLossSeries.Points.Add(
                new DataPoint(point.SwitchingFrequencyHz / 1000, point.TotalLossW));
        }

        model.Series.Add(totalLossSeries);
        return model;
    }

    private PowerModuleInput ParseInputOrThrow()
    {
        if (!TryCreateInput(out var input, out var issues))
        {
            throw new InvalidOperationException(
                $"Cannot create chart data: {string.Join(" ", issues.Select(issue => issue.Message))}");
        }

        return input;
    }

    private void SetValidationMessage(IEnumerable<ValidationIssue> issues)
    {
        ValidationMessage = string.Join(
            Environment.NewLine,
            issues.Select(issue => $"• {issue.Message}"));
        HasValidationMessage = true;
    }

    private void ClearValidationMessage()
    {
        ValidationMessage = string.Empty;
        HasValidationMessage = false;
    }
}
