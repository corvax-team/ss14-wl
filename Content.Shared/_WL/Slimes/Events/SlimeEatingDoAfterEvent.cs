using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Shared._WL.Slimes.Events;

[Serializable, NetSerializable]
public sealed partial class SlimeEatingDoAfterEvent : SimpleDoAfterEvent { }
