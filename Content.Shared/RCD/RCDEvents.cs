using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared.RCD;

[Serializable, NetSerializable]
public sealed class RCDSystemMessage(ProtoId<RCDPrototype> protoId) : BoundUserInterfaceMessage
{
    public ProtoId<RCDPrototype> ProtoId = protoId;
}

[Serializable, NetSerializable]
public sealed class RCDConstructionGhostRotationEvent(NetEntity netEntity, Direction direction) : EntityEventArgs
{
    public readonly NetEntity NetEntity = netEntity;
    public readonly Direction Direction = direction;
}

[Serializable, NetSerializable]
public enum RcdUiKey : byte
{
    Key
}

// WL-Changes-start: RPD
[Serializable, NetSerializable] // pipe layers
public sealed class RCDOverrideProtoIdEvent(NetEntity netEntity, string? proto) : EntityEventArgs
{
    public readonly NetEntity NetEntity = netEntity;
    public readonly string? OverrideProtoId = proto;
}

[Serializable, NetSerializable] // RPD port from Goob-Station
public sealed class RCDConstructionGhostFlipEvent(NetEntity netEntity, bool useMirrorPrototype) : EntityEventArgs
{
    public readonly NetEntity NetEntity = netEntity;
    public readonly bool UseMirrorPrototype = useMirrorPrototype;
}
// WL-Changes-end
