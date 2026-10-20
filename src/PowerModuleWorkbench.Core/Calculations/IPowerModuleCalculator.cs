using PowerModuleWorkbench.Core.Domain;

namespace PowerModuleWorkbench.Core.Calculations;

public interface IPowerModuleCalculator
{
    PowerModuleResult Calculate(PowerModuleInput input);
}
