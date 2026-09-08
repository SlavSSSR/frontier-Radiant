using System.Linq;
using Content.Shared.Chemistry.Components;
using Content.Shared.FixedPoint;
using Content.Shared.Body.Part;
using Content.Shared._Starlight.Medical.Surgery.Components;

namespace Content.Shared._Starlight.Medical.Surgery;

public abstract partial class SharedSurgerySystem
{
    public static string? SurgicalSolutionReagent(IComponent requirement)
        => requirement switch
        {
            SurgicalAntisepticComponent => "Ethanol",
            SurgicalLavageComponent => "Saline",
            _ => null,
        };

    public bool HasPureSurgicalSolution(EntityUid item, string reagent, FixedPoint2 amount)
        => HasComp<Content.Shared.Item.ItemComponent>(item)
            && _solutionContainerSystem.TryGetDrainableSolution(item, out _, out var solution)
            && SurgicalSolutionRules.IsPure(solution, reagent, amount);

    protected EntityUid FindSurgeryStepTool(IEnumerable<EntityUid> tools, IComponent requirement, SurgeryStepComponent step)
    {
        var reagent = SurgicalSolutionReagent(requirement);
        return tools.FirstOrDefault(item => reagent != null
            ? HasPureSurgicalSolution(item, reagent, step.ReagentQuantity)
            : HasComp(item, requirement.GetType()) && !IsDeadSurgicalItem(item));
    }

    public bool IsDeadSurgicalItem(EntityUid item)
        => HasComp<SurgicalDeadLimbComponent>(item)
            || TryComp<SurgicalOrganNecrosisComponent>(item, out var organ) && organ.Dead
            || TryComp<BodyPartComponent>(item, out var part)
                && part.PartType is BodyPartType.Arm or BodyPartType.Hand or BodyPartType.Leg or BodyPartType.Foot
                && TryComp<SurgicalSterilityComponent>(item, out var state)
                && state.NecrosisSeconds.Values.Any(age => age >= SurgicalNecrosisRules.LimbDeath);
}

public static class SurgicalSolutionRules
{
    public const int OrdinaryContainerContamination = 5;

    public static bool IsPure(Solution solution, string reagent, FixedPoint2 amount)
        => solution.Volume >= amount && solution.Volume > 0
            && solution.GetTotalPrototypeQuantity(reagent) == solution.Volume;
}
