using Content.Server._WL.CharacterInformation;
using Content.Shared._WL.DynamicText;
using Robust.Shared.GameObjects;

namespace Content.Server._WL.DynamicText;

public sealed class DynamicTextSystem : EntitySystem
{
    [Dependency] private readonly IEntityManager _ent = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeNetworkEvent<DynamicTextEvent>(DynamicText);
    }

    private void DynamicText(DynamicTextEvent ev)
    {
        if (!_ent.TryGetEntity(ev.Entity, out var ent))
            return;

        if (!TryComp<CharacterInformationComponent>(ent, out var comp))
            return;
        if (ev.DynamicText == null)
            RaiseNetworkEvent(new RequestDynamicTextEvent(comp.DynamicText));
        else
            comp.DynamicText = !string.IsNullOrEmpty(ev.DynamicText) ? ev.DynamicText : comp.DynamicText;
    }
}
