using Content.Server.Administration;
using Content.Shared._WL.Skills;
using Content.Shared._WL.Skills.Components;
using Content.Shared.Administration;
using Robust.Shared.Toolshed;
using Robust.Shared.Toolshed.Errors;
using Robust.Shared.Toolshed.Syntax;
using Robust.Shared.Toolshed.TypeParsers;
using Robust.Shared.Utility;
using System.Diagnostics;
using System.Linq;

namespace Content.Server._WL.Skills.Commands;

[ToolshedCommand, AdminCommand(AdminFlags.Spawn | AdminFlags.Admin)]
public sealed class SkillsCommand : ToolshedCommand
{
    private SkillsSystem? _skills;

    [CommandImplementation("getskills")]
    public IEnumerable<FormattedMessage> Get(
        [CommandInvocationContext] IInvocationContext ctx,
        [CommandArgument] ValueRef<EntityUid> entity)
    {
        _skills ??= GetSys<SkillsSystem>();

        var ent = entity.Evaluate(ctx);

        if (!TryComp<SkillsHolderComponent>(ent, out var comp))
        {
            ctx.ReportError(new HasNoSkillsHolderComponent());
            return [];
        }

        var list = new List<FormattedMessage>();

        ctx.WriteLine("\n");
        foreach (var (skill, level) in comp.Skills)
        {
            var msg = _skills.GetFormattedMessage(skill, level, (ent, comp));
            if (msg == null)
                continue;

            list.Add(msg);
        }

        return list;
    }

    [CommandImplementation("getskilllevels")]
    public IEnumerable<FormattedMessage> GetLevels()
    {
        var values = Enum.GetValues<SkillLevel>();

        return values.Select(v =>
        {
            var msg = new FormattedMessage();
            msg.AddMarkupOrThrow($"{SkillsSystem.GetSkillLocName(v)} - {v}");

            return msg;
        });
    }

    [CommandImplementation("getprotos")]
    public IEnumerable<FormattedMessage> GetProtos()
    {
        _skills ??= GetSys<SkillsSystem>();

        return _skills.GetFormattedSkillPrototypes();
    }

    [CommandImplementation("set")]
    public void Set(
        [CommandInvocationContext] IInvocationContext ctx,
        [CommandArgument] ValueRef<EntityUid> entity,
        [CommandArgument] Prototype<SkillPrototype> proto,
        [CommandArgument] SkillLevel level)
    {
        _skills ??= GetSys<SkillsSystem>();

        var ent = entity.Evaluate(ctx);
        var skillProto = proto.Id.Id;

        if (!TryComp<SkillsHolderComponent>(ent, out var comp))
        {
            ctx.ReportError(new HasNoSkillsHolderComponent());
            return;
        }

        _skills.SetSkill((ent, comp), skillProto, level, ctx.Session);
    }
}

public record struct HasNoSkillsHolderComponent : IConError
{
    public readonly FormattedMessage DescribeInner()
    {
        return FormattedMessage.FromMarkupOrThrow($"Указанная сущность не имеет компонента {nameof(SkillsHolderComponent)}.");
    }

    public string? Expression { get; set; }
    public Vector2i? IssueSpan { get; set; }
    public StackTrace? Trace { get; set; }
}

public record struct ParsingFailed : IConError
{
    public readonly FormattedMessage DescribeInner()
    {
        return FormattedMessage.FromMarkupOrThrow($"Указанный уровень скилла не существует.");
    }

    public string? Expression { get; set; }
    public Vector2i? IssueSpan { get; set; }
    public StackTrace? Trace { get; set; }
}
