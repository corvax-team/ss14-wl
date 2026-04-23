using Content.Shared._WL.Photo;
using Content.Shared.Whitelist;
using Robust.Server.GameObjects;
using Robust.Shared.Graphics;
using Robust.Shared.Map;
using Robust.Shared.Physics.Events;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Text;

namespace Content.Server._WL.Collision;

public sealed partial class SpawnOnCollideWithSystem : EntitySystem
{
    [Dependency] private readonly EntityWhitelistSystem _whitelist = default!;
    [Dependency] private readonly TransformSystem _transform = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SpawnOnCollideWithComponent, PreventCollideEvent>(OnCollide);
    }

    private void OnCollide(EntityUid uid, SpawnOnCollideWithComponent component, ref PreventCollideEvent args)
    {
        if (!_whitelist.IsValid(component.Whitelist, args.OtherEntity))
            return;

        if (component.Prototype != null)
        {
            Vector2 middlePoint = (_transform.GetWorldPosition(uid) + _transform.GetWorldPosition(args.OtherEntity)) / 2f;
            Spawn(component.Prototype, new MapCoordinates(middlePoint, _transform.GetMapId(uid)));
        }

        if (component.DestroySelf)
            Del(uid);
        if (component.DestroyOther)
            Del(args.OtherEntity);
    }
}
