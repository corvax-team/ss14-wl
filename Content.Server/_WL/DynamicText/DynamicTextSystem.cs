using Content.Server._WL.CharacterInformation;
using Content.Server.Popups;
using Content.Shared._WL.CCVars;
using Content.Shared._WL.DynamicText;
using Content.Shared.Administration.Logs;
using Content.Shared.Database;
using Content.Shared.Examine;
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

    private int _maxLength;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeNetworkEvent<SetDynamicTextEvent>(SetDynamicText);
        SubscribeNetworkEvent<RequestDynamicTextEvent>(RequestDynamicText);
        SubscribeLocalEvent<CharacterInformationComponent, ExaminedEvent>(OnExamine);
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

        if (!TryComp<CharacterInformationComponent>(ent, out var comp))
            return;

        if (args.SenderSession.AttachedEntity != ent)
            return;

        var newText = ev.DynamicText.Length > _maxLength
            ? FormattedMessage.RemoveMarkupOrThrow(ev.DynamicText)[.._maxLength]
            : FormattedMessage.RemoveMarkupOrThrow(ev.DynamicText);

        if (newText == comp.DynamicText)
            return;

        comp.DynamicText = newText;

        var name = Name(ent.Value);
        _popup.PopupEntity(Loc.GetString("dynamic-text-changed-popup", ("name", name)), ent.Value);

        _adminLogger.Add(LogType.WLCharDesc, LogImpact.Low, $"{ent.Value} change description of {name}: {newText}.");
    }

    private void RequestDynamicText(RequestDynamicTextEvent ev, EntitySessionEventArgs args)
    {
        if (!_ent.TryGetEntity(ev.Entity, out var ent))
            return;

        if (args.SenderSession.AttachedEntity != ent)
            return;

        if (!TryComp<CharacterInformationComponent>(ent, out var comp))
            return;

        RaiseNetworkEvent(new RequestedDynamicTextEvent(comp.DynamicText ?? string.Empty), Filter.SinglePlayer(args.SenderSession));
    }

    private void OnExamine(EntityUid uid, CharacterInformationComponent comp, ExaminedEvent args)
    {
        using (args.PushGroup(nameof(CharacterInformationComponent)))
        {
            if (!string.IsNullOrEmpty(comp.DynamicText))
                args.PushMarkup("[color=#B5C7EB][bold]" + comp.DynamicText + "[/bold][/color]");
        }
    }
}
