using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Starlight.Medical.Surgery.Components;

/// <summary>Fragments travel with the actual foot, including after amputation or transplantation.</summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class EmbeddedGlassComponent : Component
{
    [DataField, AutoNetworkedField] public List<EntProtoId> Fragments = new();
}

[RegisterComponent, NetworkedComponent]
public sealed partial class SurgeryExtractGlassComponent : Component;

[RegisterComponent, NetworkedComponent]
public sealed partial class GlassShardEmbedComponent : Component
{
    public bool Embedded;
}
