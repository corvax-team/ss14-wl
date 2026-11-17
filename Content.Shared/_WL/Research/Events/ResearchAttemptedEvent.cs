using Content.Shared._WL.Research.Components;

namespace Content.Shared._WL.Research.Events;

[ByRefEvent]
public record struct ResearchAttemtedEvent
{
    public Entity<ResearchDetectorComponent> Detector;

    public EntityUid Source;

    public bool Cancelled { get; private set; }

    public void Cancel()
    {
        Cancelled = true;
    }

    public void Uncancel()
    {
        Cancelled = false;
    }

    public ResearchAttemtedEvent(Entity<ResearchDetectorComponent> detector, EntityUid source)
    {
        Detector = detector;
        Source = source;
    }
}
