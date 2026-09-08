using Content.Client._Starlight.Medical.Surgery.UI;
using Content.Shared._Starlight.Medical.Surgery;
using Robust.Client.UserInterface;

namespace Content.Client._Starlight.Medical.Surgery;

public sealed class ConfigurableCyberEyesBoundUserInterface(EntityUid owner, Enum uiKey)
    : BoundUserInterface(owner, uiKey)
{
    private ConfigurableCyberEyesWindow? _window;

    protected override void Open()
    {
        base.Open();
        _window = this.CreateWindow<ConfigurableCyberEyesWindow>();
        _window.OnSave += color => SendMessage(new ConfigurableCyberEyesSaveMessage(color));
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);
        if (state is ConfigurableCyberEyesUiState eyesState)
            _window?.SetColor(eyesState.IrisColor);
    }
}
