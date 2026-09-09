using Content.Server._radiant.Medical.Surgery;
using Content.Shared._radiant.ERP;
using Content.Shared._radiant.Medical.Surgery;
using Content.Shared._Starlight.Medical.Surgery.Events;
using Content.Shared.Humanoid;
using Robust.Shared.Enums;

namespace Content.Server._Starlight.Medical.Surgery;

public sealed partial class SurgerySystem
{
    private void InitializeIdentitySteps()
    {
        SubscribeLocalEvent<SurgeryFinalizeSexComponent, SurgeryStepEvent>(OnFinalizeSex);
        SubscribeLocalEvent<SurgeryChangeVoiceComponent, SurgeryStepEvent>(OnChangeVoice);
    }

    private void OnFinalizeSex(Entity<SurgeryFinalizeSexComponent> ent, ref SurgeryStepEvent args)
    {
        if (args.IsCancelled || IsErpDenied(args.User) || IsErpDenied(args.Body)
            || !HasSexSurgeryAccess(args.Body, args.Part)
            || !TryComp<AdultAnatomyComponent>(args.Body, out var anatomy)
            || !TryComp<HumanoidAppearanceComponent>(args.Body, out var humanoid)
            || (ent.Comp.Sex == Sex.Female
                ? !anatomy.HasBreasts || !anatomy.HasVagina || anatomy.HasPenis
                : !anatomy.HasPenis || anatomy.HasVagina || anatomy.HasBreasts))
        {
            args.IsCancelled = true;
            return;
        }

        // Prevent the ordinary SexChanged handler from regenerating default organs.
        anatomy.SurgicallyModified = true;
        Dirty(args.Body, anatomy);
        var appearance = EntityManager.System<SharedHumanoidAppearanceSystem>();
        appearance.SetSex(args.Body, ent.Comp.Sex, humanoid: humanoid);
        appearance.SetGender((args.Body, humanoid), ent.Comp.Sex == Sex.Female ? Gender.Female : Gender.Male);
        EntityManager.System<SurgicalVoiceSystem>().OpenSelection(args.Body, args.User, true);
    }

    private void OnChangeVoice(Entity<SurgeryChangeVoiceComponent> ent, ref SurgeryStepEvent args)
    {
        if (args.IsCancelled || !HasComp<HumanoidAppearanceComponent>(args.Body))
        {
            args.IsCancelled = true;
            return;
        }
        EntityManager.System<SurgicalVoiceSystem>().OpenSelection(args.Body, args.User, false);
    }
}
