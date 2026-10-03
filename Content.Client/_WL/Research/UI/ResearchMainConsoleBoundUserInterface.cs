using Content.Shared._WL.Research.Components;
using Content.Shared._WL.Research.Prototypes;
using JetBrains.Annotations;
using Robust.Client.UserInterface;
using Robust.Shared.Prototypes;

namespace Content.Client._WL.Research.UI;

[UsedImplicitly]
public sealed class ResearchMainConsoleBoundUserInterface : BoundUserInterface
{
    [ViewVariables]
    private ResearchMainConsoleMenu? _consoleMenu;

    public ResearchMainConsoleBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    { }

    protected override void Open()
    {
        base.Open();

        var owner = Owner;

        _consoleMenu = this.CreateWindow<ResearchMainConsoleMenu>();
        _consoleMenu.SetEntity(owner);

        _consoleMenu.OnResearchStartPressed += id =>
        {
            SendMessage(new TerminalStartResearchMessage(id));
        };

        _consoleMenu.OnServerButtonPressed += () =>
        {
            SendMessage(new TerminalServerSelectionMessage());
        };
    }

    /*
    public override void OnProtoReload(PrototypesReloadedEventArgs args)
    {
        base.OnProtoReload(args);

        if (!args.WasModified<TechnologyPrototype>())
            return;

        if (State is not ResearchConsoleBoundInterfaceState rState)
            return;

        _consoleMenu?.UpdatePanels(rState);
        _consoleMenu?.UpdateInformationPanel(rState);
    }
    */

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        if (state is not ResearchMainConsoleBoundInterfaceState castState)
            return;

        _consoleMenu?.UpdatePoints(castState);
        _consoleMenu?.UpdateResearches(castState);
    }
}
