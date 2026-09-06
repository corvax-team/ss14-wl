using Content.Shared._WL.Research;
using Content.Shared._WL.Research.Components;
using Content.Shared._WL.Research.Prototypes;
using Content.Shared.Research.Components;
using Content.Shared.FixedPoint;
using Content.Server.Administration;
using Content.Shared.Administration;
using Robust.Shared.Prototypes;
using Robust.Shared.Console;
using Robust.Shared.Utility;

namespace Content.Server._WL.Research.Systems;

public sealed partial class ResearchSystemNew
{
    [Dependency] private IConsoleHost _consoleHost = default!;

    private void InitializeCommands()
    {
        _consoleHost.RegisterCommand("writepoints", Loc.GetString("anomaly-command-pulse"), "writepoints <uid> <protoId> <value>",
            WritePointsCommand,
            WritePointsCompletion);
    }

    [AdminCommand(AdminFlags.Fun)]
    private void WritePointsCommand(IConsoleShell shell, string argstr, string[] args)
    {
        if (args.Length != 3)
            shell.WriteError("Argument length must be 3");

        if (!NetEntity.TryParse(args[0], out var uidNet) || !TryGetEntity(uidNet, out var uid))
            return;

        if (!TryComp<PointsDataStorageComponent>(uid, out var storage))
            return;

        if (!ProtoMan.TryIndex<ResearchPointsTypePrototype>(args[1], out var pointsProto))
        {
            shell.WriteError("Points type proto is wrong");
            return;
        }

        var pointsDict = new ResearchPointsSpecifier(pointsProto, FixedPoint2.New(args[2]));

        if (pointsDict.AnyPositive())
        {
            if (TryWritePoints(uid.Value, ref pointsDict, out _, storage))
                shell.WriteLine("Points written");
            else
                shell.WriteError("No.");
        }
        else
        {
            var drawDict = -pointsDict;
            if (TryDeletePoints(uid.Value, ref drawDict, out _, storage))
                shell.WriteLine("Points drawed");
            else
                shell.WriteError("No.");
        }
    }

    private CompletionResult WritePointsCompletion(IConsoleShell shell, string[] args)
    {
        switch (args.Length)
        {
            case 1: return CompletionResult.FromHintOptions(CompletionHelper.Components<PointsDataStorageComponent>(args[0]), "<uid>");
            case 2: return CompletionResult.FromHintOptions(CompletionHelper.PrototypeIdsLimited<ResearchPointsTypePrototype>(args[1], ProtoMan), "<protoId>");
            default: return CompletionResult.Empty;
        }
    }
}
