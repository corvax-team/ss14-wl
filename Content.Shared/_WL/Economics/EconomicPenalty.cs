using Robust.Shared.Network;
using Robust.Shared.Serialization;

namespace Content.Shared._WL.Economics
{
    [Serializable, NetSerializable]
    public readonly record struct EconomicPenalty(
        float Coefficient,
        NetEntity PenaltyTarget,
        string Reason,
        TimeSpan? RemoveTime = null,
        NetEntity? User = null);
}
