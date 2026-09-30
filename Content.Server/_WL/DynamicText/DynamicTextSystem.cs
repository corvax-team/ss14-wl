using Content.Server.Mind;
using Content.Server.Popups;
using Content.Shared._WL.CCVars;
using Content.Shared._WL.DynamicText;
using Content.Shared.Administration.Logs;
using Content.Shared.Database;
using Content.Shared.Examine;
using Content.Shared.Ghost.Components;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Robust.Shared.Configuration;
using Robust.Shared.Player;
using Robust.Shared.Utility;

namespace Content.Server._WL.DynamicText;

public sealed partial class DynamicTextSystem : EntitySystem
{
    [Dependency] private IEntityManager _ent = default!;
    [Dependency] private IConfigurationManager _cfg = default!;
    [Dependency] private PopupSystem _popup = default!;
    [Dependency] private ISharedAdminLogManager _adminLogger = default!;
    [Dependency] private MindSystem _mindSystem = default!;
    [Dependency] private MobStateSystem _mobStateSystem = default!;

    private int _maxLength;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeNetworkEvent<SetDynamicTextEvent>(SetDynamicText);
        SubscribeNetworkEvent<RequestDynamicTextEvent>(RequestDynamicText);
        SubscribeLocalEvent<DynamicTextComponent, ExaminedEvent>(OnExamine);
        _cfg.OnValueChanged(WLCCVars.MaxDynamicTextLength, (val) => _maxLength = val, true);
    }

    public override void Shutdown()
    {
        base.Shutdown();
        _cfg.UnsubValueChanged(WLCCVars.MaxDynamicTextLength, (val) => _maxLength = val);
    }

    private void SetDynamicText(SetDynamicTextEvent ev, EntitySessionEventArgs args)
    {
        if (!_ent.TryGetEntity(ev.Entity, out var ent))
            return;

        var sender = args.SenderSession.AttachedEntity;

        if (sender == null)
            return;

        if (HasComp<GhostComponent>(sender.Value))
            return;

        if (sender != ent
            && (_mindSystem.TryGetMind(ent.Value, out _, out _)
                || HasComp<MobStateComponent>(ent)
                || _mobStateSystem.IsIncapacitated(sender.Value)))
        {
            return;
        }

        var comp = EnsureComp<DynamicTextComponent>(ent.Value);

        var newText = ev.DynamicText.Length > _maxLength
            ? FormattedMessage.RemoveMarkupOrThrow(ev.DynamicText)[.._maxLength]
            : FormattedMessage.RemoveMarkupOrThrow(ev.DynamicText);

        if (newText == comp.Text)
            return;

        comp.Text = newText;
        Dirty(ent.Value, comp);

        var name = Name(ent.Value);
        _popup.PopupEntity(Loc.GetString("dynamic-text-changed-popup", ("name", name)), ent.Value);

        Log.Info($"Dynamic text of {name} changed to: {newText}");

        _adminLogger.Add(LogType.WLCharDesc,
            LogImpact.Low,
            $"{ToPrettyString(args.SenderSession.AttachedEntity):actor} changed the description of {ToPrettyString(ent.Value):entity} to: {newText}.");
    }

    private void RequestDynamicText(RequestDynamicTextEvent ev, EntitySessionEventArgs args)
    {
        if (!_ent.TryGetEntity(ev.Entity, out var ent))
            return;
        var sender = args.SenderSession.AttachedEntity;

        if (sender == null)
            return;

        if (sender != ent
            && (_mindSystem.TryGetMind(ent.Value, out var _, out var _)
            || HasComp<MobStateComponent>(ent)
            || _mobStateSystem.IsIncapacitated(sender.Value)
            || HasComp<GhostComponent>(sender.Value)))
            return;

        var comp = EnsureComp<DynamicTextComponent>(ent.Value);

        RaiseNetworkEvent(new RequestedDynamicTextEvent(comp.Text ?? string.Empty), Filter.SinglePlayer(args.SenderSession));
    }

    private void OnExamine(EntityUid uid, DynamicTextComponent comp, ExaminedEvent args)
    {
        using (args.PushGroup(nameof(DynamicTextComponent)))
        {
            if (!string.IsNullOrEmpty(comp.Text))
                args.PushMarkup("[color=#B5C7EB][bold]" + comp.Text + "[/bold][/color]");
        }
    }
}
