using Content.Shared.Tiles;

namespace Content.Shared.Climbing.Events;

[ByRefEvent]
public record struct AttemptClimbEvent(
    EntityUid User,
    EntityUid Climber,
    EntityUid Climbable)
{
    public bool Cancelled;
    /*WL-skills-start*/
    public float DoAfterTime;
    /*WL-Skills-end*/
}
