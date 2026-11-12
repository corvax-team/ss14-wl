using System.Numerics;
using System.Linq;
using Content.Server.Power.EntitySystems;
using Content.Shared._WL.Research;
using Content.Shared._WL.Research.Methods;
using Content.Shared._WL.Research.Components;
using Content.Shared._WL.Research.Prototypes;
using Content.Shared.FixedPoint;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Server._WL.Research.Systems;

public sealed partial class ResearchSystemNew
{
    private void InitializeScanners()
    { }

    [SubscribeLocalEvent]
    private void OnScannedEvent(Entity<ResearchScannerComponent> ent, ref ScannedResearchEvent args)
    {
        var outputEv = new PostScannedEvent(ent, new List<string>());
        RaiseLocalEvent(args.Target, ref outputEv);
        Logger.Debug("SCANNED");

        UpdateScanerWindow(ent, outputEv.Output, true);
    }

    [SubscribeLocalEvent]
    private void OnPostScannedEvent(Entity<ResearchScannableComponent> ent, ref PostScannedEvent args)
    {
        var name = ent.Comp.Name == string.Empty ? Name(ent) : ent.Comp.Name;
        var desc = ent.Comp.Description == string.Empty ? Description(ent) : ent.Comp.Description;

        if (name != string.Empty)
            args.Output.Add(Loc.GetString("research-scanner-ui-string-name", ("targetString", name)));

        if (desc != string.Empty)
            args.Output.Add(Loc.GetString("research-scanner-ui-string-desc", ("targetString", desc)));
    }

    [SubscribeLocalEvent]
    private void OnSourceScannedEvent(Entity<ResearchSourceComponent> ent, ref PostScannedEvent args)
    {
        if (!TryComp<ResearchDetectorComponent>(args.Scanner, out var detector))
            return;

        if (TryAnalyzeToServer(args.Scanner, ent, detector, ent.Comp))
            args.Output.Add(Loc.GetString("research-scanner-ui-research-succes"));
        else
            args.Output.Add(Loc.GetString("research-scanner-ui-research-fail"));
    }

    [SubscribeLocalEvent]
    private void OnSimpleScannedEvent(Entity<SimpleSourceComponent> ent, ref PostScannedEvent args)
    {
        if (!TryComp<ResearchDetectorComponent>(args.Scanner, out var detector))
            return;

        Logger.Debug("SCANNED SIMPLE");
        if (TrySimpleAnalyze(args.Scanner, ent, detector, ent.Comp))
            args.Output.Add(Loc.GetString("research-scanner-ui-research-succes"));
        else
            args.Output.Add(Loc.GetString("research-scanner-ui-research-fail"));
    }

    private void UpdateScanerWindow(EntityUid uid, List<string> strings, bool clearOutput)
    {
        var msg = new FormattedMessage();

        foreach (var outString in strings)
        {
            msg.AddMarkupOrThrow(outString);
            msg.PushNewline();
            msg.PushNewline();
        }

        var state = new ResearchScannerUserInterfaceState(msg, clearOutput);
        UI.SetUiState(uid, ResearchScannerUiKey.Key, state);
    }
}
