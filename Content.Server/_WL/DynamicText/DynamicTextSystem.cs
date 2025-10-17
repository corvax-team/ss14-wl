using Content.Server._WL.CharacterInformation;
using Content.Shared._WL.CCVars;
using Content.Shared._WL.DynamicText;
using Robust.Shared.Configuration;
using Robust.Shared.Player;
using Robust.Shared.Utility;

namespace Content.Server._WL.DynamicText;

public sealed class DynamicTextSystem : EntitySystem
{
    [Dependency] private readonly IEntityManager _ent = default!;
    [Dependency] private readonly IConfigurationManager _cfm = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeNetworkEvent<SetDynamicTextEvent>(SetDynamicText);
        SubscribeNetworkEvent<RequestDynamicTextEvent>(RequestDynamicText);
    }

    private void SetDynamicText(SetDynamicTextEvent ev, EntitySessionEventArgs args)
    {
        if (!_ent.TryGetEntity(ev.Entity, out var ent))
            return;

        if (!TryComp<CharacterInformationComponent>(ent, out var comp))
            return;

        if (args.SenderSession.AttachedEntity != ent)
            return;

        var maxDynamicTextLength = _cfm.GetCVar(WLCVars.MaxDynamicTextLength);

        comp.DynamicText = ev.DynamicText.Length > maxDynamicTextLength ? FormattedMessage.RemoveMarkupOrThrow(ev.DynamicText)[..maxDynamicTextLength] : FormattedMessage.RemoveMarkupOrThrow(ev.DynamicText);
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
}
