using PowerModuleWorkbench.Core.Domain;

namespace PowerModuleWorkbench.Infrastructure.Persistence;

public sealed record PowerModuleProjectFile(
    int SchemaVersion,
    PowerModuleInput Input,
    PowerModuleResult? Result);
