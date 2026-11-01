using System.Linq;
using Content.Shared.Damage.Components;
using Content.Shared.FixedPoint;
using Content.Shared.Humanoid;
using Content.Shared.Humanoid.Prototypes;
using Robust.Client.GameObjects;
using Robust.Shared.Serialization.Manager;

namespace Content.Client.Damage;

[RegisterComponent]
public sealed partial class ImitatorDamageVisualsBackupComponent : Component
{
    public EntityUid Image;
    public DamageVisualsComponent? Original;
}

public sealed partial class DamageVisualsSystem
{
    [Dependency] private ISerializationManager _imitatorSerialization = default!;

    public void RefreshImitator(EntityUid wearer, EntityUid source)
    {
        DamageVisualsComponent? replacement;
        if (source != wearer)
        {
            var backup = EnsureComp<ImitatorDamageVisualsBackupComponent>(wearer);
            if (backup.Image == source)
                return;
            if (backup.Image == default && TryComp<DamageVisualsComponent>(wearer, out var original))
                backup.Original = _imitatorSerialization.CreateCopy(original, notNullableOverride: true);
            backup.Image = source;
            replacement = null;
            if (TryComp<HumanoidProfileComponent>(source, out var profile) &&
                ProtoMan.Resolve(profile.Species, out SpeciesPrototype? species) &&
                ProtoMan.Index(species.Prototype).Components.TryGetValue("DamageVisuals", out var entry))
                replacement = _imitatorSerialization.CreateCopy((DamageVisualsComponent) entry.Component, notNullableOverride: true);
        }
        else
        {
            if (!TryComp<ImitatorDamageVisualsBackupComponent>(wearer, out var backup))
                return;
            replacement = backup.Original;
            RemComp<ImitatorDamageVisualsBackupComponent>(wearer);
        }

        if (TryComp<DamageVisualsComponent>(wearer, out var current))
        {
            RemoveImitatorDamageLayers(wearer, current);
            RemComp<DamageVisualsComponent>(wearer);
        }
        if (replacement == null)
            return;
        replacement.Thresholds.RemoveAll(threshold => threshold == FixedPoint2.Zero);
        AddComp(wearer, replacement);
        if (replacement.Valid && TryComp<SpriteComponent>(wearer, out var sprite) &&
            TryComp<DamageableComponent>(wearer, out var damage))
            ForceUpdateLayers((wearer, damage, sprite, replacement));
        if (TryComp<AppearanceComponent>(wearer, out var appearance))
            EntityManager.System<AppearanceSystem>().QueueUpdate(wearer, appearance);
    }

    private void RemoveImitatorDamageLayers(EntityUid wearer, DamageVisualsComponent visuals)
    {
        if (!visuals.Overlay || !TryComp<SpriteComponent>(wearer, out var sprite))
            return;
        var prefixes = visuals.TargetLayerMapKeys.Count > 0
            ? visuals.TargetLayerMapKeys.Select(layer => layer.ToString()).ToArray()
            : new[] { "DamageOverlay" };
        foreach (var prefix in prefixes)
        {
            var keys = visuals.DamageOverlayGroups != null
                ? visuals.DamageOverlayGroups.Keys.Select(group => $"{prefix}{group}")
                : new[] { visuals.TargetLayerMapKeys.Count > 0 ? $"{prefix}trackDamage" : prefix };
            foreach (var key in keys)
            {
                _displacement.EnsureDisplacementIsNotOnSprite((wearer, sprite), key);
                if (!SpriteSystem.LayerMapTryGet((wearer, sprite), key, out var index, false))
                    continue;
                SpriteSystem.LayerMapRemove((wearer, sprite), key);
                SpriteSystem.RemoveLayer((wearer, sprite), index);
            }
        }
    }
}
