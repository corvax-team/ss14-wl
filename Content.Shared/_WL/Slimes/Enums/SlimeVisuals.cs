using Robust.Shared.Serialization;

namespace Content.Shared._WL.Slimes.Enums;

[Serializable, NetSerializable]
public enum SlimeVisualState : byte
{
    Body, /// <see cref="SlimeLifeStage"/>
    Face
}

public enum SlimeVisualLayers : byte
{
    FaceLayer
}
