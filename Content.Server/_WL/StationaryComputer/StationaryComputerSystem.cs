using Content.Shared._WL.StationaryComputer;
using Robust.Server.GameObjects;
using Robust.Shared.Toolshed.TypeParsers;
using System.Diagnostics.CodeAnalysis;

namespace Content.Server._WL.StationaryComputer;

public sealed partial class StationaryComputerSystem : SharedStationaryComputerSystem
{
    [Dependency] private UserInterfaceSystem _ui = default!;
    [Dependency] private ILocalizationManager _loc = default!;

    private static readonly string EmptyResponse = "stationary-computer-response-unknown-command";

    public override void Initialize()
    {
        base.Initialize();

        Subs.BuiEvents<StationaryComputerComponent>(StationaryComputerUiKey.Key, subs =>
        {
            subs.Event<StationaryComputerMessage>(OnMessage);
        });
    }

    private void OnMessage(EntityUid uid, StationaryComputerComponent component, StationaryComputerMessage args)
    {
        var state = new StationaryComputerBUIState();

        component.AddContent(args.RawText, args.Root);

        var ev = new CheckCommandEnterEvent(args.CommandName, args.Positional, args.Flags);

        RaiseLocalEvent(uid, ref ev);

        var response = ev.Handled ? ev.Response : _loc.GetString(EmptyResponse);

        component.AddContent(response);

        Dirty<StationaryComputerComponent>((uid, component));

        UpdateUIState(uid, state);
    }

    [SubscribeLocalEvent]
    public void OnCommandEvent(Entity<StationaryComputerComponent> ent, ref CheckCommandEnterEvent args)
    {
        if (TryClientCommand(ent, args.Name, args.Positional, args.Options, out var output))
            args.Handle(output);
    }

    private bool TryClientCommand(
        Entity<StationaryComputerComponent> ent,
        string name,
        List<string> positional,
        Dictionary<string, List<string>> options,
        out string? output
        )
    {
        output = null;

        switch (name)
        {
            case "clear":
                ent.Comp.Content.Clear();
                return true;

            case "style":
                if (options.TryGetValue("color", out var values) && values.Count >= 1)
                {
                    Color color;
                    if (Color.TryFromName(values[0], out var color_))
                    {
                        color = color_;
                    }
                    else
                    {

                        if (!Color.TryFromHex(values[0], out var color2))
                        {
                            output = _loc.GetString("stationary-computer-response-unknown-color");
                            return true;
                        }

                        color = color2;
                    }

                    ent.Comp.ConsoleColor = color.WithAlpha(255);
                }

                output = _loc.GetString("stationary-computer-response-style-changed");
                return true;
        }

        return false;
    }

    private void UpdateUIState(Entity<UserInterfaceComponent?> ent, StationaryComputerBUIState state)
    {
        _ui.SetUiState(ent, StationaryComputerUiKey.Key, state);
    }
}
