using Content.Client._WL.DynamicText;
using Content.Client.Gameplay;
using Content.Client.Info;
using Content.Shared.Guidebook;
using Content.Shared.Info;
using Robust.Client.Console;
using Robust.Client.UserInterface.Controllers;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;

namespace Content.Client._WL.DynamicText.UI;

public sealed class DynamicTextUIController : UIController
{
    private DynamicTextWindow? _dynamicTextWindow;

    public override void Initialize()
    {
        base.Initialize();
    }

    public void OpenWindow()
    {
        if (_dynamicTextWindow == null || _dynamicTextWindow.Disposed)
            _dynamicTextWindow = UIManager.CreateWindow<DynamicTextWindow>();

        _dynamicTextWindow?.OpenCentered();

        // _dynamicTextWindow?.OnDynamicTextSaveButtonPressed += OnSave;
    }

    private void OnSave()
    {
        _dynamicTextWindow?.Close();
    }
}
