using Content.Shared._WL.Compromise;
using Content.Shared.StationRecords;
using Robust.Client.Player;

namespace Content.Client._WL.Compromise;

public sealed partial class CompromiseTabletBoundUserInterface : BoundUserInterface
{
    private CompromiseTabletWindow? _window;

    public CompromiseTabletBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey) { }

    protected override void Open()
    {
        base.Open();

        var comp = EntMan.GetComponent<CompromiseTabletComponent>(Owner);

        _window = new(Owner, comp.MaxStringLength);
        _window.OnKeySelected += key =>
            SendMessage(new SelectStationRecord(key));
        _window.OnFiltersChanged += (type, filterValue) =>
            SendMessage(new SetStationRecordFilter(type, filterValue));
        _window.OnClose += Close;
        _window.OnPrinted += key =>
            SendMessage(new PrintCompromiseRecord(key));
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        if (state is not CompromiseTabletState cast)
            return;

        _window?.UpdateState(cast);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        _window?.Close();
    }
}
