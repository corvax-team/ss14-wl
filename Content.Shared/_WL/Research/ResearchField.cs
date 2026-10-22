using System.Linq;
using Content.Shared._WL.Types;
using Content.Shared._WL.Research.Components;
using Content.Shared._WL.Research.Prototypes;
using Content.Shared.FixedPoint;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Generic;

namespace Content.Shared._WL.Research;

[Serializable, NetSerializable]
public struct ResearchPoint
{
    public readonly FixedPoint2 Max;
    public FixedPoint2 Value { get; private set; }

    public ResearchPoint(FixedPoint2 max)
    {
        Max = max;
        Value = 0;
    }

    public ResearchPoint AddPoints(FixedPoint2 value, out FixedPoint2 diff)
    {
        diff = FixedPoint2.Min(Max, (Value + value)) - Value;
        Value += diff;

        return this;
    }

    public ResearchPoint AddPercent(FixedPoint2 value, out FixedPoint2 diff)
    {
        diff = FixedPoint2.Min(Max - Value, (Max * value));
        Value += diff;

        return this;
    }

    public bool IsMax()
    {
        return Value == Max;
    }
}

//[Serializable, NetSerializable]
public sealed partial class ResearchField
{
    public int Rank { get; private set; }

    public FixedPoint2 Max { get; private set; }

    public ProtoId<ResearchTypePrototype>[] ResearchTypes { get; private set; }

    private RobustableArray<ResearchPoint> FieldArray;

    private FixedPoint2 _fieldScale;

    public ResearchField(ProtoId<ResearchTypePrototype>[] researchTypes, FixedPoint2 maxPoints)
    {
        int[] sizes = new int[researchTypes.Length];
        ResearchTypePrototype[] typesProto = new ResearchTypePrototype[researchTypes.Length];
        var protoMan = IoCManager.Resolve<IPrototypeManager>();

        for (int i = 0; i < researchTypes.Length; i++)
        {
            typesProto[i] = protoMan.Index<ResearchTypePrototype>(researchTypes[i]);
            sizes[i] = typesProto[i].Size;
        }

        (FieldArray, var fieldSum) = GenField(typesProto, sizes);

        ResearchTypes = researchTypes;

        Rank = researchTypes.Length;

        Max = maxPoints;

        _fieldScale = fieldSum / maxPoints ;
    }

    private (RobustableArray<ResearchPoint>, FixedPoint2) GenField(ResearchTypePrototype[] researchProtos, ReadOnlySpan<int> sizes)
    {
        var field = new RobustableArray<ResearchPoint>(sizes);

        var count = 1;
        var rank = sizes.Length;

        foreach (var size in sizes)
        {
            count *= size;
        }

        FixedPoint2 sum = 0;

        for (int i = count - 1; i >= 0; i--)
        {
            int[] coords = new int[rank];
            FixedPoint2 max = 0;
            var offset = i;

            for (int j = rank - 1; j >= 0; j--)
            {
                coords[j] = offset % sizes[j];
                offset /= sizes[j];

                max += researchProtos[j].GetScale(coords[j]);
            }

            sum += max;

            field.Set(coords, new ResearchPoint(max));
        }

        return (field, sum);
    }

    public FixedPoint2 ResearchData(Dictionary<ProtoId<ResearchTypePrototype>, FixedPoint2> researchData, FixedPoint2 points)
    {
        int[] coords = new int[Rank];
        var protoMan = IoCManager.Resolve<IPrototypeManager>();

        for (int i = 0; i < Rank; i++)
        {
            if (researchData.TryGetValue(ResearchTypes[i], out var value))
            {
                var type = protoMan.Index(ResearchTypes[i]);
                coords[i] = FixedPoint2.Clamp((value - type.MinValue) / (type.MaxValue - type.MinValue) * type.Size, 0, type.Size - 1).Int();
            }
            else
            {
                coords[i] = 0;
            }
        }

        var point = FieldArray.Get(coords.AsSpan()).AddPercent(points, out var diff);
        FieldArray.Set(coords.AsSpan(), point);

        return diff / _fieldScale;
    }
}
