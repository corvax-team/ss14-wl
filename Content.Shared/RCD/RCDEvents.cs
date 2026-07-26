using Content.Shared.Atmos.Components;
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
// WL-Changes-start
[Serializable, NetSerializable]
public sealed class RCDConstructionGhostFlipEvent : EntityEventArgs // rpd port from FunkyStation
{
    public readonly NetEntity NetEntity;
    public readonly bool UseMirrorPrototype;
    public RCDConstructionGhostFlipEvent(NetEntity netEntity, bool useMirrorPrototype)
    {
        NetEntity = netEntity;
        UseMirrorPrototype = useMirrorPrototype;
    }
}

[Serializable, NetSerializable]
public sealed class RPDEyeRotationEvent : EntityEventArgs // rpd port from FunkyStation
{
    public readonly NetEntity NetEntity;
    public float? EyeRotation;

    public RPDEyeRotationEvent(NetEntity netEntity, float? eyeRotation)
    {
        NetEntity = netEntity;
        EyeRotation = eyeRotation;
    }
}

[Serializable, NetSerializable]
public sealed class RPDLayerUpdateEvent : EntityEventArgs
{
    public readonly NetEntity NetEntity;
    public AtmosPipeLayer NewLayer;

    public RPDLayerUpdateEvent(NetEntity netEntity, AtmosPipeLayer newLayer)
    {
        NetEntity = netEntity;
        NewLayer = newLayer;
    }
}

[Serializable, NetSerializable]
public sealed class RDChangeModeEvent(NetEntity rcd) : EntityEventArgs // Ignition
{
    public readonly NetEntity Rcd = rcd;
}
// WL-Changes-end

[Serializable, NetSerializable]
public enum RcdUiKey : byte
{
    Key
}
