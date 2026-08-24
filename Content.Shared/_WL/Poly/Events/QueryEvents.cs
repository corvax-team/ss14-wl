using Robust.Shared.Serialization;

namespace Content.Shared._WL.Poly.Events
{
    [Serializable, NetSerializable]
    public sealed partial class PolyServerQueryEvent(NetEntity entity, string queryId) : EntityEventArgs
    {
        public readonly NetEntity Entity = entity;
        public readonly string QueryId = queryId;
    }

    [Serializable, NetSerializable]
    public sealed partial class PolyClientResponseEvent(byte[]? png_stream, string queryId) : EntityEventArgs
    {
        public readonly byte[]? Stream = png_stream;
        public readonly string QueryId = queryId;
    }
}
