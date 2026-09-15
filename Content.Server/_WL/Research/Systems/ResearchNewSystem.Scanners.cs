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
        if (!TryComp<ResearchScannableComponent>(args.Target, out var scannable))
            return;

        var scannerOutput = new List<string>();
        scannerOutput.Add(Loc.GetString("research-scanner-ui-string-succes"));

        if (scannable.Name != string.Empty)
            scannerOutput.Add(Loc.GetString("research-scanner-ui-string-name", ("targetString", scannable.Name)));

        if (scannable.Description != string.Empty)
            scannerOutput.Add(Loc.GetString("research-scanner-ui-string-desc", ("targetString", scannable.Description)));

        UpdateScanerWindow(ent, scannerOutput, true);
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
