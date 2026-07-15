using Content.Shared._WL.SafeCode;
using Robust.Client.GameObjects;

namespace Content.Client._WL.SafeCode;


public sealed partial class SafeCodeVisualizerSystem : VisualizerSystem<SafeCodeComponent>
{
    [Dependency] private SpriteSystem _sprite = default!;
    protected override void OnAppearanceChange(EntityUid uid, SafeCodeComponent component, ref AppearanceChangeEvent args)
    {
        if (args.Sprite == null)
            return;

        if (!TryComp<SpriteComponent>(uid, out var sprite))
            return;

        if (!AppearanceSystem.TryGetData<bool>(uid, SafeCodeVisuals.Broken, out var broken))
            broken = false;

        if (!AppearanceSystem.TryGetData<bool>(uid, SafeCodeVisuals.Locked, out var locked))
            locked = true;

        if (!_sprite.LayerMapTryGet((uid, sprite), SafeCodeVisualLayers.Lights, out _, false))
            return;

        if (broken)
        {
            _sprite.LayerSetVisible((uid, sprite),SafeCodeVisualLayers.Lights, false);
            return;
        }

        _sprite.LayerSetVisible((uid, sprite),SafeCodeVisualLayers.Lights, true);
        _sprite.LayerSetRsiState((uid, sprite), SafeCodeVisualLayers.Lights, locked ? "locked" : "unlocked");
    }
}
