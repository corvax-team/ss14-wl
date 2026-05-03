using Content.Shared.Whitelist;
using Robust.Shared.Prototypes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Content.Server._WL.Collision;

[RegisterComponent]
public sealed partial class SpawnOnCollideWithComponent : Component
{
    [DataField]
    public EntityWhitelist Whitelist = new();

    [DataField]
    public ProtoId<EntityPrototype>? Prototype = null;

    [DataField]
    public bool DestroySelf = true;
    [DataField]
    public bool DestroyOther = true;
}
