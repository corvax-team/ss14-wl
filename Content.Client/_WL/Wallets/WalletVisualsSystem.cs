using System.Linq;
using System.Numerics;
using Content.Shared._WL.Wallets;
using Robust.Client.GameObjects;
using Robust.Shared.Containers;

namespace Content.Client._WL.Wallets;

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

        if (!hasId || !TryComp<SpriteComponent>(container!.ContainedEntities[0], out var idSprite))
        {
            _sprite.LayerSetVisible((ent, args.Sprite), WalletVisualLayers.IdBase, false);
            _sprite.LayerSetVisible((ent, args.Sprite), WalletVisualLayers.IdIcon, false);
            return;
        }

        SetIdLayer(ent, args.Sprite, idSprite, 0, WalletVisualLayers.IdBase, ent.Comp.CardOffset);
        SetIdLayer(ent, args.Sprite, idSprite, 1, WalletVisualLayers.IdIcon, ent.Comp.IdOffset);
    }

    private void SetIdLayer(
        EntityUid uid,
        SpriteComponent walletSprite,
        SpriteComponent idSprite,
        int idLayerIndex,
        WalletVisualLayers targetLayer,
        Vector2 offset)
    {
        if (idLayerIndex >= idSprite.AllLayers.Count())
        {
            _sprite.LayerSetVisible((uid, walletSprite), targetLayer, false);
            return;
        }

        var layer = idSprite[idLayerIndex];
        _sprite.LayerSetRsi((uid, walletSprite), targetLayer, layer.Rsi ?? idSprite.BaseRSI, layer.RsiState);
        _sprite.LayerSetColor((uid, walletSprite), targetLayer, layer.Color);
        _sprite.LayerSetOffset((uid, walletSprite), targetLayer, offset);
        _sprite.LayerSetVisible((uid, walletSprite), targetLayer, true);
    }
}
