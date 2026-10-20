using System.Text.Json;
using System.Text.Json.Serialization;
using PowerModuleWorkbench.Core.Domain;

namespace PowerModuleWorkbench.Infrastructure.Persistence;

public sealed class PowerModuleProjectStore
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter() },
    };

    public static async Task SaveAsync(
        string filePath,
        PowerModuleInput input,
        PowerModuleResult? result,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        ArgumentNullException.ThrowIfNull(input);

        var project = new PowerModuleProjectFile(1, input, result);
        var json = JsonSerializer.Serialize(project, SerializerOptions);
        var directory = Path.GetDirectoryName(Path.GetFullPath(filePath));
        if (directory is not null)
        {
            Directory.CreateDirectory(directory);
        }

        await File.WriteAllTextAsync(filePath, json, cancellationToken);
    }

    public static async Task<PowerModuleProjectFile> LoadAsync(
        string filePath,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        await using var stream = File.OpenRead(filePath);
        var project = await JsonSerializer.DeserializeAsync<PowerModuleProjectFile>(
            stream,
            SerializerOptions,
            cancellationToken);

        if (project is null)
        {
            throw new InvalidDataException("The project file is empty.");
        }

        if (project.SchemaVersion != 1)
        {
            throw new InvalidDataException(
                $"Project schema version {project.SchemaVersion} is not supported.");
        }

        var issues = PowerModuleInputValidator.Validate(project.Input);
        if (issues.Count > 0)
        {
            throw new InvalidDataException(
                $"The project contains invalid input: {string.Join(" ", issues.Select(issue => issue.Message))}");
        }

        return project;
    }
}
