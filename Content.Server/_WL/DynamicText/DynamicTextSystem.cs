using Content.Server._WL.CharacterInformation;
using Content.Shared._WL.DynamicText;
using Robust.Shared.Player;

namespace Content.Server._WL.DynamicText;

public sealed class DynamicTextSystem : EntitySystem
{
    [Dependency] private readonly IEntityManager _ent = default!;

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

        comp.DynamicText = ev.DynamicText;
    }

    private void RequestDynamicText(RequestDynamicTextEvent ev, EntitySessionEventArgs args)
    {
        if (!_ent.TryGetEntity(ev.Entity, out var ent))
            return;

        if (!TryComp<CharacterInformationComponent>(ent, out var comp))
            return;

        RaiseNetworkEvent(new RequestedDynamicTextEvent(comp.DynamicText), Filter.SinglePlayer(args.SenderSession));
    }
}
