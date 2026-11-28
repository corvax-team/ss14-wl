namespace Content.Shared._WL.CombatIndicator;

[ByRefEvent]
public readonly struct CombatModeChangedEvent(bool Enabled)
{
    public readonly bool Enabled = Enabled;
}

[ByRefEvent]
public readonly struct CombatIndicatorChangedEvent(bool Enabled)
{
    public readonly bool Enabled = Enabled;
}
