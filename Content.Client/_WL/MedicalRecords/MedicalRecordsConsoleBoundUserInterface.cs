using Content.Shared._WL.MedicalRecords;
using Content.Shared.StationRecords;
using Robust.Client.UserInterface;

namespace Content.Client._WL.MedicalRecords;

public sealed class MedicalRecordsConsoleBoundUserInterface : BoundUserInterface
{
    private MedicalRecordsConsoleWindow? _window;

    public MedicalRecordsConsoleBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey) { }

    protected override void Open()
    {
        base.Open();

        _window = this.CreateWindow<MedicalRecordsConsoleWindow>();
        _window.OnKeySelected += key => SendMessage(new MedicalRecordsSelectStationRecord(key));
        _window.OnFiltersChanged += (type, filterValue) =>
            SendMessage(new MedicalRecordsSetStationRecordFilter(type, filterValue));
        _window.OnClose += Close;

        _window.OpenCentered();
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        if (state is not MedicalRecordsConsoleState cast)
            return;

        _window?.UpdateState(cast);
    }
}
