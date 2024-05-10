using Content.Shared._WL.Store;

namespace Content.Server._WL.Store.PriceFunctions;

public sealed partial class QuadraticDependenceFunction : PriceModify
{
    /// <summary>
    ///   The coefficient A in the quadratic function y=ax^2 + bx + c
    /// </summary>
    [DataField("coefA", required: true)]
    public float A;

    /// <summary>
    ///   The coefficient B in the quadratic function y=ax^2 + bx + c
    /// </summary>
    [DataField("coefB", required: true)]
    public float B;

    /// <summary>
    ///   The coefficient C in the quadratic function y=ax^2 + bx + c
    /// </summary>
    [DataField("coefC", required: true)]
    public float C;

    public override float Function(PriceModifyArgs args)
    {
        return A * MathF.Pow(args.PurchasesNumber, 2) + B * args.PurchasesNumber + C;
    }
}
