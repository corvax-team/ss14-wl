using Robust.Client.UserInterface.Controllers;

namespace Content.Client._WL.DynamicText.UI;

public sealed partial class DynamicTextUIController : UIController
{
    [Dependency] private IEntityManager _entManager = default!;

    private DynamicTextWindow? _dynamicTextWindow;

    public void OpenWindow()
    {
        if (_dynamicTextWindow == null || _dynamicTextWindow.Disposed)
            _dynamicTextWindow = UIManager.CreateWindow<DynamicTextWindow>();

        if (_dynamicTextWindow != null)
        {
            _dynamicTextWindow.OnDynamicTextSaveButtonPressed += OnSave;
        }
        _entManager.System<DynamicTextSystem>().RequestDynamicText();
        _dynamicTextWindow?.OpenCentered();
    }
    public void SetDynamicText(string text)
    {
        _dynamicTextWindow?.SetDynamicText(text);
    }

    private void OnSave(string text)
    {
        _entManager.System<DynamicTextSystem>().SaveDynamicText(text);
    }
}
