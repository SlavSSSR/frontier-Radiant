using Content.Shared._radiant.ERP;
using Content.Shared._radiant.Medical.Surgery;
using Content.Shared._Starlight.Medical.Surgery.Components;
using Content.Shared._Starlight.Medical.Surgery.Events;
using Content.Shared.Humanoid;

namespace Content.Shared._Starlight.Medical.Surgery;

public abstract partial class SharedSurgerySystem
{
    private void InitializeIdentitySurgery()
    {
        SubscribeLocalEvent<SurgeryChangeSexComponent, SurgeryValidEvent>(OnSexSurgeryValid);
        SubscribeLocalEvent<SurgeryChangeVoiceComponent, SurgeryValidEvent>(OnVoiceSurgeryValid);
    }

    private void OnVoiceSurgeryValid(Entity<SurgeryChangeVoiceComponent> ent, ref SurgeryValidEvent args)
    {
        args.Cancelled |= !HasComp<HumanoidAppearanceComponent>(args.Body);
    }

    private void OnSexSurgeryValid(Entity<SurgeryChangeSexComponent> ent, ref SurgeryValidEvent args)
    {
        // Existing organs must be removed through their normal extraction operations first.
        if (!HasSexSurgeryAccess(args.Body, args.Part)
            || !TryComp<HumanoidAppearanceComponent>(args.Body, out var appearance)
            || appearance.Sex == ent.Comp.Sex
            || !TryComp<AdultAnatomyComponent>(args.Body, out var anatomy)
            || anatomy.HasPenis || anatomy.HasVagina || anatomy.HasBreasts)
            args.Cancelled = true;
    }

    public bool HasSexSurgeryAccess(EntityUid body, EntityUid part)
        => TryComp<SurgicalCavityStateComponent>(part, out var cavity)
           && cavity.RibcageOpen && cavity.GroinOpen
           && HasComp<AdultAnatomyComponent>(body);
}
