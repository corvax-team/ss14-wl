using Content.Shared._WL.Slimes.Enums;

namespace Content.Shared._WL.Slimes.Events;

/// <summary>
/// Raised when a slime changes its stage of life. <see cref="SlimeLifeStage"/>.
/// </summary>
public sealed partial class SlimeLifeStageChangeEvent : EntityEventArgs
{
    public readonly SlimeLifeStage? PreviousStage = null;

    public readonly SlimeLifeStage NewStage;

    public readonly EntityUid? Slime;

    public SlimeLifeStageChangeEvent(EntityUid slime, SlimeLifeStage newStage, SlimeLifeStage? previousStage = null)
    {
        Slime = slime;
        NewStage = newStage;

        PreviousStage = previousStage;
    }
}
