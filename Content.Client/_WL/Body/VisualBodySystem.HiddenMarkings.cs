using Content.Shared._WL.Body;
using Content.Shared.Body;
using Content.Shared.Humanoid;
using Robust.Client.GameObjects;

namespace Content.Client.Body;

public sealed partial class VisualBodySystem
{
    [Dependency] private BodySystem _hiddenMarkingsBody = default!;
    private void InitializeHiddenMarkings()
    {
        SubscribeLocalEvent<HideUnderClothingComponent, AfterAutoHandleStateEvent>(OnHiddenMarkingsState);
        SubscribeLocalEvent<HideUnderClothingComponent, MarkingsUnderClothingChangedEvent>(OnHiddenMarkingsChanged);
    }

    private bool IsMarkingTucked(EntityUid body, Content.Shared.Humanoid.Markings.MarkingPrototype marking)
    {
        if (!marking.CanHideUnderClothing || !TryComp<HideUnderClothingComponent>(body, out var tucked))
            return false;
        return marking.BodyPart switch
        {
            HumanoidVisualLayers.HeadTop => tucked.EarsHidden,
            HumanoidVisualLayers.Tail or HumanoidVisualLayers.TailOverlay => tucked.TailHidden,
            _ => false,
        };
    }

    private void OnHiddenMarkingsState(Entity<HideUnderClothingComponent> ent, ref AfterAutoHandleStateEvent args)
    {
        RefreshHiddenMarkings(ent);
    }

    private void OnHiddenMarkingsChanged(Entity<HideUnderClothingComponent> ent, ref MarkingsUnderClothingChangedEvent args)
    {
        RefreshHiddenMarkings(ent);
    }

    private void RefreshHiddenMarkings(Entity<HideUnderClothingComponent> ent)
    {
        if (!TryComp<SpriteComponent>(ent, out var sprite))
            return;
        TryComp<HideableHumanoidLayersComponent>(ent, out var hidden);
        foreach (var organ in _hiddenMarkingsBody.EnumerateOrgans<VisualOrganMarkingsComponent>(ent.Owner))
        foreach (var marking in organ.Comp2.AppliedMarkings)
        {
            if (!_marking.TryGetMarking(marking, out var proto) || !proto.CanHideUnderClothing)
                continue;
            var visible = true;
            if (_sprite.LayerMapTryGet((ent.Owner, sprite), proto.BodyPart, out var index, false) &&
                _sprite.TryGetLayer((ent.Owner, sprite), index, out var layer, false))
                visible = layer.Visible;
            if (hidden != null)
            {
                foreach (var hiddenLayer in hidden.HiddenLayers.Keys)
                {
                    if (organ.Comp2.HideableLayers.Contains(hiddenLayer) &&
                        (hiddenLayer == proto.BodyPart ||
                         organ.Comp2.DependentHidingLayers.TryGetValue(hiddenLayer, out var dependent) && dependent.Contains(proto.BodyPart)))
                        visible = false;
                }
            }
            UpdateMarkingLayerVisibility(proto, ent.Owner, visible);
        }
    }
}
