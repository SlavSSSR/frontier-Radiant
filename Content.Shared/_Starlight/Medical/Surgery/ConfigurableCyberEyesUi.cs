using Robust.Shared.Serialization;

namespace Content.Shared._Starlight.Medical.Surgery;

[Serializable, NetSerializable]
public enum ConfigurableCyberEyesUiKey : byte
{
    Key,
}

[Serializable, NetSerializable]
public sealed class ConfigurableCyberEyesUiState(Color irisColor) : BoundUserInterfaceState
{
    public readonly Color IrisColor = irisColor;
}

[Serializable, NetSerializable]
public sealed class ConfigurableCyberEyesSaveMessage(Color irisColor) : BoundUserInterfaceMessage
{
    public readonly Color IrisColor = irisColor;
}
