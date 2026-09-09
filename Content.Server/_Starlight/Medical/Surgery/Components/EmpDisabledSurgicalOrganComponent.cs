using Robust.Shared.GameObjects;

namespace Content.Server._Starlight.Medical.Surgery.Components;

/// <summary>Server-only state needed to restore a functional surgical organ after EMP.</summary>
[RegisterComponent]
public sealed partial class EmpDisabledSurgicalOrganComponent : Component
{
    public EntityUid Body;
    public HashSet<Type> Installed = [];
    public bool Disabled;
}

/// <summary>Server-only ownership state for an action granted by a surgical implant.</summary>
[RegisterComponent]
public sealed partial class EmpDisabledActionOrganComponent : Component
{
    public EntityUid Body;
    public bool Disabled;
}

/// <summary>Server-only snapshot used to restore a cyberlimb after EMP.</summary>
[RegisterComponent]
public sealed partial class EmpDisabledCyberLimbComponent : Component
{
    public EntityUid? Body;
    public bool Disabled;
    public string? RemovedHandId;
    public bool HadMovement;
    public float WalkSpeed;
    public float SprintSpeed;
    public float Acceleration;
}

/// <summary>Tracks a conventional subdermal implant while its granted functionality is EMP-disabled.</summary>
[RegisterComponent]
public sealed partial class EmpDisabledSubdermalImplantComponent : Component
{
    public bool Disabled;
}

/// <summary>Aggregates the temporary cardiac consequences of EMP-disabled heart implants.</summary>
[RegisterComponent]
public sealed partial class HeartImplantEmpEffectComponent : Component
{
    public HashSet<EntityUid> Sources = [];
    public TimeSpan NextTick;
    public TimeSpan NextPainPopup;
}

/// <summary>Stores organ-local components removed for the duration of an EMP.</summary>
[RegisterComponent]
public sealed partial class EmpDisabledCyberneticOrganComponent : Component
{
    public Dictionary<Type, IComponent> Removed = new();
}
