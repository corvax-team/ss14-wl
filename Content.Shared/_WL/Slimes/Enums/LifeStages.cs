using Robust.Shared.Serialization;

namespace Content.Shared._WL.Slimes.Enums;

[Serializable, NetSerializable]
public enum SlimeLifeStage
{
    Dead, //The Dead stage is necessary for the correct display of the sprite of the deceased slime. (Too lazy to make a visualizer)
    Young,
    Adult,
    Old,
    Ancient,
    Humanoid //TODO Add and change to Jelly People
}
