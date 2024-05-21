namespace Content.Shared._WL.Economics
{
    public readonly record struct EconomicPenalty(
        float Coefficient,
        EntityUid PenaltyTarget,
        string Reason,
        TimeSpan? RemoveTime = null,
        EntityUid? User = null);
}
