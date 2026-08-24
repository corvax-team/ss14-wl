namespace Content.Shared._WL.Light.Events;

public sealed partial class BeforeDealHeatDamageFromLightBulbEvent(EntityUid bulb) : CancellableEntityEventArgs
{
    public readonly EntityUid Bulb = bulb;
}
