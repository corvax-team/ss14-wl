using Content.Shared._WL.StationaryComputer;
using JetBrains.Annotations;
using Robust.Client.UserInterface;
using Robust.Shared.Prototypes;

namespace Content.Client._WL.UserInterface.Systems.Computers;

[UsedImplicitly]
public sealed class StationaryComputerBUI : BoundUserInterface
{
    [ViewVariables]
    private StationaryComputerWindow? _window;

    public StationaryComputerBUI(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {

    }

    protected override void Open()
    {
        base.Open();

        if (!EntMan.TryGetComponent<StationaryComputerComponent>(Owner, out var comp))
            return;

        _window = this.CreateWindow<StationaryComputerWindow>();
        _window.SetRoot(comp.CurrentRoot);

        _window.OnClose += Close;
        _window.OnCommandEntered += OnCommand;

        Populate((Owner, comp));

        _window.OpenCentered();
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        if (state is not StationaryComputerBUIState computerState)
            return;

        if (!EntMan.TryGetComponent<StationaryComputerComponent>(Owner, out var comp))
            return;

        Populate((Owner, comp));
        _window?.InputLine.SetText(string.Empty, true);
    }

    private void OnCommand(CmdLineEdit.CmdLineCommandEntry entry)
    {
        if (_window == null)
            return;

        SendMessage(new StationaryComputerMessage(
            _window.CurrentRoot,
            entry.CommandName,
            entry.RawText,
            entry.Positional,
            entry.Flags
        ));
    }

    private void Populate(Entity<StationaryComputerComponent> ent)
    {
        if (_window == null)
            return;

        var comp = ent.Comp;

        _window.ClearConsole();
        _window.SetConsoleColor(comp.ConsoleColor);
        _window.SetRoot(comp.CurrentRoot);

        foreach (var loc in comp.BaseContent)
        {
            _window.AddOutputLine(null, Loc.GetString(loc));
        }

        foreach (var loc in comp.Content)
        {
            _window.AddOutputLine(loc.Root, loc.Content);
        }

        _window.UnlockLineEdit();
    }
}
