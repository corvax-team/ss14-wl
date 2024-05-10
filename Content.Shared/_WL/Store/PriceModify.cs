using JetBrains.Annotations;

namespace Content.Shared._WL.Store;


[ImplicitDataDefinitionForInheritors]
[MeansImplicitUse]
public abstract partial class PriceModify
{
    /// <summary>
    /// Depending on the number of purchases, sets the price.
    /// </summary>
    /// <returns>The cost of the next purchase</returns>
    public abstract float Function(PriceModifyArgs args);
}

public readonly record struct PriceModifyArgs(int PurchasesNumber, float OldCost);
