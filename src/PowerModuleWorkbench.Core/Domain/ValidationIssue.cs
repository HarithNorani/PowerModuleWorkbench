namespace PowerModuleWorkbench.Core.Domain;

public sealed record ValidationIssue(
    string Code,
    string PropertyName,
    string Message,
    ValidationSeverity Severity = ValidationSeverity.Error);
