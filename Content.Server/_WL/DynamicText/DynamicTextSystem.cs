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

        SubscribeNetworkEvent<SetDynamicTextEvent>(DynamicText);
    }

    private void DynamicText(SetDynamicTextEvent ev, EntitySessionEventArgs args)
    {
        if (!_ent.TryGetEntity(ev.Entity, out var ent))
            return;

        if (!TryComp<CharacterInformationComponent>(ent, out var comp))
            return;
        if (ev.DynamicText == string.Empty)
            RaiseNetworkEvent(new RequestDynamicTextEvent(comp.DynamicText), Filter.SinglePlayer(args.SenderSession));
        else
            comp.DynamicText = !string.IsNullOrEmpty(ev.DynamicText) ? ev.DynamicText : comp.DynamicText;
    }
}
