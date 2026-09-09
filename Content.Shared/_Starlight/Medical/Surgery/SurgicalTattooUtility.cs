using System.Linq;
using Content.Shared.Body.Part;
using Content.Shared.Humanoid;
using Content.Shared.Humanoid.Markings;
using Robust.Shared.Prototypes;

namespace Content.Shared._Starlight.Medical.Surgery;

/// <summary>
/// Matches surgically removable skin markings to the exact operated body part.
/// </summary>
public static class SurgicalTattooUtility
{
    public static bool IsTattooOnPart(
        Marking marking,
        BodyPartComponent part,
        IPrototypeManager prototypes)
    {
        if (!prototypes.TryIndex<MarkingPrototype>(marking.MarkingId, out var prototype)
            || !IsTattoo(prototype))
            return false;

        // The marking category is authoritative for torso patterns. In particular,
        // Arcana torso markings were imported with an incorrect LLeg visual layer;
        // without this early return they also appear as removable from the left leg.
        if (prototype.MarkingCategory == MarkingCategories.Chest)
            return part.PartType == BodyPartType.Torso;

        return part.PartType switch
        {
            BodyPartType.Head => prototype.BodyPart is HumanoidVisualLayers.Head
                or HumanoidVisualLayers.Eyes
                or HumanoidVisualLayers.Snout,
            BodyPartType.Arm => prototype.BodyPart == SideLayer(
                part.Symmetry,
                HumanoidVisualLayers.LArm,
                HumanoidVisualLayers.RArm),
            BodyPartType.Hand => prototype.BodyPart == SideLayer(
                part.Symmetry,
                HumanoidVisualLayers.LHand,
                HumanoidVisualLayers.RHand),
            BodyPartType.Leg => prototype.BodyPart == SideLayer(
                part.Symmetry,
                HumanoidVisualLayers.LLeg,
                HumanoidVisualLayers.RLeg),
            BodyPartType.Foot => prototype.BodyPart == SideLayer(
                part.Symmetry,
                HumanoidVisualLayers.LFoot,
                HumanoidVisualLayers.RFoot),
            _ => false,
        };
    }

    private static bool IsTattoo(MarkingPrototype prototype)
    {
        if (prototype.ID.StartsWith("Tattoo", StringComparison.OrdinalIgnoreCase)
            || prototype.ID.StartsWith("DemonChest", StringComparison.OrdinalIgnoreCase))
            return true;

        if (prototype.Coloring.Default.Type is TattooColoring
            || prototype.Coloring.Default.FallbackTypes.Any(type => type is TattooColoring))
            return true;

        return prototype.Coloring.Layers?.Values.Any(layer =>
            layer.Type is TattooColoring
            || layer.FallbackTypes.Any(type => type is TattooColoring)) == true;
    }

    private static HumanoidVisualLayers SideLayer(
        BodyPartSymmetry symmetry,
        HumanoidVisualLayers left,
        HumanoidVisualLayers right)
        => symmetry == BodyPartSymmetry.Right ? right : left;
}
