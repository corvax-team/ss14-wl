using Content.Shared._WL.Store;

namespace Content.Server._WL.Store.PriceFunctions;

/// <summary>
/// Calculates the new Listing cost using a linear function, where X is the number of purchases.
/// </summary>
/// <remarks>y = mx + b</remarks>
public sealed partial class LinearDependenceFunction : PriceModify
{
    /// <summary>
    ///   The coefficient M in the linear function y=mx+b
    /// </summary>
    [DataField("coefM", required: true)]
    public float M;

    /// <summary>
    ///   The coefficient B in the linear function y=mx+b
    /// </summary>
    [DataField("coefB", required: true)]
    public float B;

    public override float Function(PriceModifyArgs args)
    {
        return args.PurchasesNumber * M + B;
    }
}
