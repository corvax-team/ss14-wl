using Content.Shared._WL.DynamicText;
using Robust.Client.Player;
using Robust.Client.UserInterface.Controllers;

namespace Content.Client._WL.DynamicText.UI;

public sealed class DynamicTextUIController : UIController
{
    [Dependency] private readonly IEntityManager _entManager = default!;

    private DynamicTextWindow? _dynamicTextWindow;

    public override void Initialize()
    {
        base.Initialize();

        //SubscribeNetworkEvent<RequestDynamicTextEvent>(SetDynamic);
    }

    public void OpenWindow()
    {
        if (_dynamicTextWindow == null || _dynamicTextWindow.Disposed)
            _dynamicTextWindow = UIManager.CreateWindow<DynamicTextWindow>();

        _dynamicTextWindow?.OpenCentered();

        if (_dynamicTextWindow != null)
        {
            _dynamicTextWindow.OnDynamicTextSaveButtonPressed += OnSave;
        }
        //_entManager.System<DynamicTextSystem>().LoadDynamic();
    }
    //private void SetDynamic(RequestDynamicTextEvent ev, EntitySessionEventArgs args)
    //{
    //    _dynamicTextWindow?.SetDynamicText(ev.DynamicText);
    //}

    private void OnSave(string text)
    {
        _entManager.System<DynamicTextSystem>().SaveDynamic(text);
    }
}
