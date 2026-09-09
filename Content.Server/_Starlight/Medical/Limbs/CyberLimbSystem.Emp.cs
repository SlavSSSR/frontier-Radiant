using Content.Server._Starlight.Medical.Surgery.Components;
using Content.Server.Emp;
using Content.Shared.Actions.Components;
using Content.Shared.Actions.Events;
using Content.Shared._Starlight.Medical.Limbs;
using Content.Shared.Body.Part;
using Content.Shared.Body.Systems;
using Content.Shared.Emp;
using Content.Shared.Hands.Components;
using Content.Shared.Movement.Components;

namespace Content.Server._Starlight.Medical.Limbs;

public sealed partial class CyberLimbSystem
{
    private void InitializeEmp()
    {
        SubscribeLocalEvent<ReversibleCyberLimbComponent, EmpPulseEvent>(OnCyberLimbEmp);
        SubscribeLocalEvent<ReversibleCyberLimbComponent, EmpDisabledRemoved>(OnCyberLimbEmpRemoved);
        SubscribeLocalEvent<ActionComponent, ActionAttemptEvent>(OnActionAttempt);
    }

    private void OnActionAttempt(Entity<ActionComponent> ent, ref ActionAttemptEvent args)
    {
        if (ent.Comp.Container is not { } provider
            || !TryComp<EmpDisabledCyberLimbComponent>(provider, out var disabled)
            || !disabled.Disabled)
            return;

        args.Cancelled = true;
        _popup.PopupEntity(Loc.GetString("starlight-cyberlimb-emp-disabled"), args.User, args.User);
    }

    private void OnCyberLimbEmp(Entity<ReversibleCyberLimbComponent> ent, ref EmpPulseEvent args)
    {
        args.Affected = true;
        args.Disabled = true;

        var state = EnsureComp<EmpDisabledCyberLimbComponent>(ent);
        if (state.Disabled)
            return;

        state.Disabled = true;
        if (TryComp<BodyPartComponent>(ent, out var part))
            state.Body = part.Body;

        // Retract deployed tools before removing their action, otherwise their
        // temporary hands remain usable for the duration of the EMP.
        if (state.Body is { } body && TryComp<LimbItemDeployerComponent>(ent, out var deployer) && deployer.Toggled)
        {
            var toggle = new ToggleLimbEvent { Performer = body };
            OnLimbToggle((ent.Owner, deployer), ref toggle);
        }

        // Keep limb actions visible. ActionAttemptEvent blocks them and explains
        // why they cannot be used while the limb is EMP-disabled.

        if (state.Body is { } installedBody
            && TryComp<BodyPartComponent>(ent, out var handPart)
            && handPart.PartType == BodyPartType.Hand
            && TryComp<HandsComponent>(installedBody, out var hands))
        {
            var slot = handPart.Symmetry switch
            {
                BodyPartSymmetry.Left => "left hand",
                BodyPartSymmetry.Right => "right hand",
                _ => null,
            };

            if (slot != null)
            {
                var handId = SharedBodySystem.GetPartSlotContainerId(slot);
                if (_hands.TryGetHand((installedBody, hands), handId, out _))
                {
                    state.RemovedHandId = handId;
                    _hands.RemoveHand((installedBody, hands), handId);
                }
            }
        }

        if (TryComp<MovementBodyPartComponent>(ent, out var movement))
        {
            state.HadMovement = true;
            state.WalkSpeed = movement.WalkSpeed;
            state.SprintSpeed = movement.SprintSpeed;
            state.Acceleration = movement.Acceleration;
            RemComp<MovementBodyPartComponent>(ent);
            if (state.Body is { } movementBody)
                _body.UpdateMovementSpeed(movementBody);
        }
    }

    private void OnCyberLimbEmpRemoved(Entity<ReversibleCyberLimbComponent> ent, ref EmpDisabledRemoved args)
    {
        if (!TryComp<EmpDisabledCyberLimbComponent>(ent, out var state) || !state.Disabled)
            return;

        if (state.HadMovement)
        {
            var movement = EnsureComp<MovementBodyPartComponent>(ent);
            movement.WalkSpeed = state.WalkSpeed;
            movement.SprintSpeed = state.SprintSpeed;
            movement.Acceleration = state.Acceleration;
            Dirty(ent.Owner, movement);
        }

        if (TryComp<BodyPartComponent>(ent, out var part) && part.Body is { } body)
        {
            if (state.RemovedHandId is { } handId
                && TryComp<HandsComponent>(body, out var hands)
                && !_hands.TryGetHand((body, hands), handId, out _))
            {
                var location = part.Symmetry switch
                {
                    BodyPartSymmetry.Left => HandLocation.Left,
                    BodyPartSymmetry.Right => HandLocation.Right,
                    _ => HandLocation.Middle,
                };
                _hands.AddHand((body, hands), handId, location);
            }

            if (state.HadMovement)
                _body.UpdateMovementSpeed(body);
        }

        RemComp<EmpDisabledCyberLimbComponent>(ent);
    }
}
