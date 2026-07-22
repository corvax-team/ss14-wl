using Content.Shared._WL.Wallet;
using Robust.Client.GameObjects;
using Robust.Shared.Containers;

namespace Content.Client._WL.Wallet;

public sealed partial class WalletVisualsSystem : EntitySystem
{
    [Dependency] private SpriteSystem _sprite = default!;
    [Dependency] private SharedContainerSystem _container = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<WalletComponent, AppearanceChangeEvent>(OnAppearanceChanged);
    }

    private void OnAppearanceChanged(Entity<WalletComponent> ent, ref AppearanceChangeEvent args)
    {
        if (args.Sprite == null)
            return;

        var hasId = _container.TryGetContainer(ent, ent.Comp.IdSlotId, out var container) &&
                    container.ContainedEntities.Count > 0;

        _sprite.LayerSetVisible((ent, args.Sprite), WalletVisualLayers.Frame, hasId);

        if (!hasId)
        {
            _sprite.LayerSetVisible((ent, args.Sprite), WalletVisualLayers.IdBase, false);
            _sprite.LayerSetVisible((ent, args.Sprite), WalletVisualLayers.IdIcon, false);
            return;
        }

        var idUid = container!.ContainedEntities[0];

        if (!TryComp<SpriteComponent>(idUid, out var idSprite))
            return;

        var baseCard = idSprite[0];
        _sprite.LayerSetRsi((ent, args.Sprite), WalletVisualLayers.IdBase, baseCard.Rsi ?? idSprite.BaseRSI);
        _sprite.LayerSetRsiState((ent, args.Sprite), WalletVisualLayers.IdBase, baseCard.RsiState);
        _sprite.LayerSetColor((ent, args.Sprite), WalletVisualLayers.IdBase, baseCard.Color);
        _sprite.LayerSetOffset((ent, args.Sprite), WalletVisualLayers.IdBase, ent.Comp.CardOffset);
        _sprite.LayerSetVisible((ent, args.Sprite), WalletVisualLayers.IdBase, true);

        var baseIdIcon = idSprite[1];
        _sprite.LayerSetRsi((ent, args.Sprite), WalletVisualLayers.IdIcon, baseIdIcon.Rsi ?? idSprite.BaseRSI);
        _sprite.LayerSetRsiState((ent, args.Sprite), WalletVisualLayers.IdIcon, baseIdIcon.RsiState);
        _sprite.LayerSetColor((ent, args.Sprite), WalletVisualLayers.IdIcon, baseIdIcon.Color);
        _sprite.LayerSetOffset((ent, args.Sprite), WalletVisualLayers.IdIcon, ent.Comp.IdOffset);
        _sprite.LayerSetVisible((ent, args.Sprite), WalletVisualLayers.IdIcon, true);
    }

}
