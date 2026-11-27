using Content.Shared._WL.Parry;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Client.Utility;
using Robust.Shared.Enums;
using Robust.Shared.Graphics.RSI;
using Robust.Shared.Timing;
using System.Numerics;

namespace Content.Client._WL.Parry;

public sealed partial class ParryOverlay : Overlay
{
    [Dependency] private IEntityManager _entityManager = default!;
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private IEyeManager _eyeManager = default!;

    private readonly SpriteSystem _sprite;
    private readonly TransformSystem _transform;

    private readonly Dictionary<EntityUid, TimeSpan> _startedAt = new();
    private readonly HashSet<EntityUid> _seenThisFrame = new();
    private readonly List<EntityUid> _stale = new();

    private readonly EntityQuery<ParryComponent> _parryQuery;

    private const float InitialScale = 1.33f;
    private const float FinalScale = 0.9f;

    public override OverlaySpace Space =>
        OverlaySpace.WorldSpaceBelowEntities | OverlaySpace.WorldSpaceEntities;

    public ParryOverlay()
    {
        IoCManager.InjectDependencies(this);

        ZIndex = (int)Shared.DrawDepth.DrawDepth.Mobs + 1;

        _sprite = _entityManager.System<SpriteSystem>();
        _transform = _entityManager.System<TransformSystem>();

        _parryQuery = _entityManager.GetEntityQuery<ParryComponent>();
    }


    protected override void Draw(in OverlayDrawArgs args)
    {
        var handle = args.WorldHandle;
        var parries = _entityManager.EntityQueryEnumerator<ActiveParryComponent, TransformComponent, SpriteComponent>();

        _seenThisFrame.Clear();

        while (parries.MoveNext(out var uid, out var activeParry, out var xform, out var bodySprite))
        {
            _seenThisFrame.Add(uid);

            if (!_startedAt.TryGetValue(uid, out var startedAt))
            {
                startedAt = _timing.CurTime;
                _startedAt.Add(uid, startedAt);
            }

            if (xform.MapID != args.MapId
                || !_entityManager.TryGetComponent(activeParry.Weapon, out SpriteComponent? weaponSprite))
                continue;

            if (!_parryQuery.TryGetComponent(activeParry.Weapon, out var parryComp))
                continue;

            var eyeRotation = _eyeManager.CurrentEye.Rotation;

            var facingAngle = (_transform.GetWorldRotation(xform) + eyeRotation)
                .Reduced()
                .FlipPositive();

            var facing = bodySprite.EnableDirectionOverride
                ? bodySprite.DirectionOverride.Convert(RsiDirectionType.Dir4)
                : SpriteComponent.Layer.GetDirection(RsiDirectionType.Dir4, facingAngle);

            var facingDirection = facing.Convert();

            if (args.Space == OverlaySpace.WorldSpaceBelowEntities)
            {
                if (facingDirection != Direction.North)
                    continue;
            }
            else if (args.Space == OverlaySpace.WorldSpaceEntities)
            {
                if (facingDirection == Direction.North)
                    continue;
            }

            var worldRotation = facingDirection.ToVec().ToAngle() - eyeRotation;

            var elapsed = (float)(_timing.CurTime - startedAt).TotalSeconds;
            var progress = Math.Clamp(elapsed / (float)parryComp.ParryDuration.TotalSeconds, 0f, 1f);
            var scale = InitialScale + (FinalScale - InitialScale) * progress;

            var position = _transform.GetWorldPosition(xform);
            position += worldRotation.RotateVec(new Vector2(parryComp.ForwardOffset, 0f));

            var matrix =
                Matrix3x2.CreateScale(scale) *
                Matrix3Helpers.CreateRotation(worldRotation) *
                Matrix3x2.CreateTranslation(position);

            handle.SetTransform(matrix);

            for (var i = 0; _sprite.TryGetLayer((activeParry.Weapon, weaponSprite), i, out var layer, false); i++)
            {
                if (!layer.Visible || layer.ActualState is not { } state)
                    continue;

                var textureDirection = layer.EffectiveDirection(
                    state,
                    facingAngle,
                    facingDirection);

                var texture = state.GetFrame(
                    textureDirection,
                    Math.Clamp(layer.AnimationFrame, 0, state.DelayCount - 1));

                var textureSize = (Vector2)texture.Size / EyeManager.PixelsPerMeter;

                handle.DrawTexture(
                    texture,
                    -textureSize / 2f,
                    layer.Rotation + parryComp.AngleOffset,
                    layer.Color);
            }
        }

        _stale.Clear();

        foreach (var entity in _startedAt.Keys)
        {
            if (!_seenThisFrame.Contains(entity))
                _stale.Add(entity);
        }

        foreach (var entity in _stale)
        {
            _startedAt.Remove(entity);
        }

        handle.SetTransform(Matrix3x2.Identity);
    }
}
