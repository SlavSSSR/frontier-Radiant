using System.Linq;
using Content.Shared._Starlight.Medical.Surgery.Components;

namespace Content.Shared._Starlight.Medical.Surgery;

public abstract partial class SharedSurgerySystem
{
    public float GetStepSuccessRate(EntityUid step, IEnumerable<EntityUid> tools)
    {
        if (!TryComp<SurgeryStepComponent>(step, out var definition) || definition.Tools == null)
            return 1f;
        var chance = 1f;
        foreach (var item in tools)
        foreach (var tool in EntityManager.GetComponents(item).OfType<ISurgeryToolComponent>())
        {
            if (definition.Tools.ContainsKey(tool.ToolType))
                chance = Math.Min(chance, Math.Clamp(tool.SuccessRate, 0f, 1f));
        }
        return chance;
    }
}
