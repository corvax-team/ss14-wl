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
        var outputEv = new PostScannedEvent(args.Target, new List<string>());
        RaiseLocalEvent(ent, ref outputEv);

        UpdateScanerWindow(ent, outputEv.Output, true);
    }

    [SubscribeLocalEvent]
    private void OnScannedTextEvent(Entity<ResearchScannableComponent> ent, ref PostScannedEvent args)
    {
        if (!TryComp<ResearchScannableComponent>(args.Target, out var scannable))
            return;

        args.Output.Add(Loc.GetString("research-scanner-ui-string-succes"));

        if (scannable.Name != string.Empty)
            args.Output.Add(Loc.GetString("research-scanner-ui-string-name", ("targetString", scannable.Name)));

        if (scannable.Description != string.Empty)
            args.Output.Add(Loc.GetString("research-scanner-ui-string-desc", ("targetString", scannable.Description)));
    }


    [SubscribeLocalEvent]
    private void OnScannedEvent(Entity<ResearchDetectorComponent> ent, ref PostScannedEvent args)
    {
        if (!TryComp<ResearchSourceComponent>(args.Target, out var source))
            return;

        if (TryAnalyzeToServer(ent, args.Target, ent.Comp, source))
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
