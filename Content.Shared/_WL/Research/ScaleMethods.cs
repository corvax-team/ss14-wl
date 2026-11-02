using Content.Shared.FixedPoint;

namespace Content.Shared._WL.Research;

[ImplicitDataDefinitionForInheritors]
public abstract partial class ScaleMethod
{
    [DataField]
    public FixedPoint4 min = 1;

    [DataField]
    public FixedPoint4 max = 100;

    internal abstract FixedPoint4 GetModifier(FixedPoint4 x); // D [0, 1]; E [0, 1]

    public FixedPoint4 GetModifier(int x, int size)
    {
        FixedPoint4 sized = FixedPoint4.Clamp(x / size, 0, 1);

        return GetModifier(sized) * (max - min) + min;
    }
}

public sealed partial class ConstMethod : ScaleMethod
{
    internal override FixedPoint4 GetModifier(FixedPoint4 x)
    {
        return 0.5;
    }
}

public sealed partial class LinearMethod : ScaleMethod
{
    internal override FixedPoint4 GetModifier(FixedPoint4 x)
    {
        return x;
    }
}

public sealed partial class ExpMethod : ScaleMethod
{
    [DataField(required: true)]
    public FixedPoint4 dbase = FixedPoint4.New(Math.E);

    internal override FixedPoint4 GetModifier(FixedPoint4 x)
    {
        if (dbase <= 1)
            return 0;

        return (Math.Pow(dbase.Double(), x.Double()) - 1)/(dbase.Double() - 1);
    }
}

public sealed partial class FlattedAbsMethod : ScaleMethod
{
    [DataField(required: true)]
    public FixedPoint4 intMin;

    [DataField(required: true)]
    public FixedPoint4 intMax;

    [DataField]
    public FixedPoint4 slope = FixedPoint4.New(1);

    internal override FixedPoint4 GetModifier(FixedPoint4 x)
    {
        if (x < intMin)
            return (intMin - x) * slope;

        if (x < intMax)
            return (x - intMax) * slope;

        return 0;
    }
}

