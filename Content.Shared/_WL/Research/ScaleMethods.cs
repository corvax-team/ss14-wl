using Content.Shared.FixedPoint;

namespace Content.Shared._WL.Research;

[ImplicitDataDefinitionForInheritors]
public abstract partial class ScaleMethod
{
    [DataField]
    public FixedPoint2 min = 1;

    [DataField]
    public FixedPoint2 max = 100;

    internal abstract FixedPoint2 GetModifier(FixedPoint2 x); // D [0, 1]; E [0, 1]. 0 => 0, 1 => 1.

    public FixedPoint2 GetModifier(int x, int size)
    {
        FixedPoint2 sized = FixedPoint2.Clamp(x / size, 0, 1);

        return GetModifier(sized) * (max - min) + min;
    }
}

public sealed partial class ConstMethod : ScaleMethod
{
    internal override FixedPoint2 GetModifier(FixedPoint2 x)
    {
        return 0.5;
    }
}

public sealed partial class LinearMethod : ScaleMethod
{
    internal override FixedPoint2 GetModifier(FixedPoint2 x)
    {
        return x;
    }
}

public sealed partial class ExpMethod : ScaleMethod
{
    [DataField(required: true)]
    public FixedPoint2 dbase;

    internal override FixedPoint2 GetModifier(FixedPoint2 x)
    {
        if (dbase <= 1)
            return 0;

        return (Math.Pow(dbase.Double(), x.Double()) - 1)/(dbase.Double() - 1);
    }
}
