using Content.Server._WL.CharacterInformation;
using Content.Server.Mind;
using Content.Server.Popups;
using Content.Shared._WL.CCVars;
using Content.Shared._WL.DynamicText;
using Content.Shared.Administration.Logs;
using Content.Shared.Database;
using Content.Shared.Examine;
using Content.Shared.Mobs.Components;
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

    private int _maxLength;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeNetworkEvent<SetDynamicTextEvent>(SetDynamicText);
        SubscribeNetworkEvent<RequestDynamicTextEvent>(RequestDynamicText);
        SubscribeLocalEvent<DynamicTextComponent, ExaminedEvent>(OnExamine);
        _cfg.OnValueChanged(WLCVars.MaxDynamicTextLength, (val) => _maxLength = val, true);
    }

    public override void Shutdown()
    {
        base.Shutdown();
        _cfg.UnsubValueChanged(WLCVars.MaxDynamicTextLength, (val) => _maxLength = val);
    }

    private void SetDynamicText(SetDynamicTextEvent ev, EntitySessionEventArgs args)
    {
        if (!_ent.TryGetEntity(ev.Entity, out var ent))
            return;

        if (args.SenderSession.AttachedEntity != ent
            && (_mindSystem.TryGetMind(ent.Value, out var _, out var _)
            || HasComp<MobStateComponent>(ent)))
            return;

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

        _adminLogger.Add(LogType.WLCharDesc, LogImpact.Low, $"{ent.Value} change description of {name}: {newText}.");
    }

    private void RequestDynamicText(RequestDynamicTextEvent ev, EntitySessionEventArgs args)
    {
        if (!_ent.TryGetEntity(ev.Entity, out var ent))
            return;

        if (args.SenderSession.AttachedEntity != ent
            && (_mindSystem.TryGetMind(ent.Value, out var _, out var _)
            || HasComp<MobStateComponent>(ent)))
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
