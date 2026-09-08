using Content.Server.NPC.HTN;
using Robust.Shared.Prototypes;

namespace Content.Server._radiant.Xenos;

/// <summary>
/// Keeps the queen-maid spawn-only ghost role inactive until a player claims it.
/// </summary>
public sealed class AlienQueenMaidSystem : EntitySystem
{
    public override void Initialize()
    {
        SubscribeLocalEvent<AlienQueenMaidComponent, MapInitEvent>(OnMaidMapInit);
    }

    private void OnMaidMapInit(Entity<AlienQueenMaidComponent> ent, ref MapInitEvent args)
    {
        // The maid is intentionally neutral while vacant. A player mind can still use
        // its inherited hands and melee weapon after taking the ghost role.
        RemComp<HTNComponent>(ent);
    }
}

[RegisterComponent]
public sealed partial class AlienQueenMaidComponent : Component;
