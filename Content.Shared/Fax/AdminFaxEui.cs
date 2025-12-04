using Content.Shared.Eui;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;
using System.Numerics;

namespace Content.Shared.Fax;

[Serializable, NetSerializable]
public sealed class AdminFaxEuiState : EuiStateBase
{
    public List<AdminFaxEntry> Entries { get; }

    public AdminFaxEuiState(List<AdminFaxEntry> entries)
    {
        Entries = entries;
    }
}

[Serializable, NetSerializable]
public sealed class AdminFaxEntry
{
    public NetEntity Uid { get; }
    public string Name { get; }
    public string Address { get; }

    public AdminFaxEntry(NetEntity uid, string name, string address)
    {
        Uid = uid;
        Name = name;
        Address = address;
    }
}

public static class AdminFaxEuiMsg
{
    [Serializable, NetSerializable]
    public sealed class Close : EuiMessageBase
    {
    }

    [Serializable, NetSerializable]
    public sealed class Follow : EuiMessageBase
    {
        public NetEntity TargetFax { get; }

        public Follow(NetEntity targetFax)
        {
            TargetFax = targetFax;
        }
    }

    [Serializable, NetSerializable]
    public sealed class Send : EuiMessageBase
    {
        public NetEntity Target { get; }
        public string Title { get; }
        public string From { get; }
        public string Content { get; }
        public string StampState { get; }
        public Color StampColor { get; }
        public bool Locked { get; }

        // WL-Changes-start
        public SpriteSpecifier.Texture Texture { get; }
        public bool IsTextureBorder { get; }
        public Vector2 Size { get; }
        // WL-Changes-end

        public Send(
            NetEntity target,
            string title,
            string from,
            string content,
            string stamp,
            Color stampColor,
            bool locked,
            SpriteSpecifier.Texture texture, // WL-Changes-start
            bool isTextureBorder, // WL-Changes-end
            Vector2 size // WL-Changes-end
            )
        {
            Target = target;
            Title = title;
            From = from;
            Content = content;
            StampState = stamp;
            StampColor = stampColor;
            Locked = locked;

            // WL-Changes-start
            Texture = texture;
            IsTextureBorder = isTextureBorder;
            Size = size;
            // WL-Changes-end
        }
    }
}
