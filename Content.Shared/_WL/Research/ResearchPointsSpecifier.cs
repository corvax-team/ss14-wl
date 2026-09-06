using System.Linq;
using System.Text.Json.Serialization;
using Content.Shared._WL.Research.Components;
using Content.Shared._WL.Research.Prototypes;
using Content.Shared.FixedPoint;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Generic;

namespace Content.Shared._WL.Research
{
    [DataDefinition, Serializable, NetSerializable]
    public sealed partial class ResearchPointsSpecifier : IEquatable<ResearchPointsSpecifier>, IRobustCloneable<ResearchPointsSpecifier>
    {

        [DataField("points")]
        public Dictionary<ProtoId<ResearchPointsTypePrototype>, FixedPoint2> PointsDict { get; set; } = new();

        public FixedPoint2 GetTotal()
        {
            var total = FixedPoint2.Zero;
            foreach (var value in PointsDict.Values)
            {
                total += value;
            }
            return total;
        }

        public bool AnyPositive()
        {
            foreach (var value in PointsDict.Values)
            {
                if (value > FixedPoint2.Zero)
                    return true;
            }

            return false;
        }

        [JsonIgnore]
        public bool Empty => PointsDict.Count == 0;

        public ResearchPointsSpecifier Clone()
        {
            return new ResearchPointsSpecifier(this);
        }

        public override string ToString()
        {
            return "ResearchPointsSpecifier(" + string.Join("; ", PointsDict.Select(x => x.Key + ":" + x.Value)) + ")";
        }

        #region constructors
        public ResearchPointsSpecifier() { }

        public ResearchPointsSpecifier(Dictionary<ProtoId<ResearchPointsTypePrototype>, FixedPoint2> pointsDict)
        {
            PointsDict = new(pointsDict);
        }

        public ResearchPointsSpecifier(ResearchPointsSpecifier pointsSpec)
        {
            PointsDict = new(pointsSpec.PointsDict);
        }

        public ResearchPointsSpecifier(ResearchPointsTypePrototype type, FixedPoint2 value)
        {
            PointsDict = new() { { type.ID, value } };
        }

        public ResearchPointsSpecifier(ProtoId<ResearchPointsTypePrototype> typeId, FixedPoint2 value)
        {
            PointsDict = new() { { typeId, value } };
        }

        public ResearchPointsSpecifier(List<ProtoId<ResearchPointsTypePrototype>> typesList, FixedPoint2 value)
        {
            foreach (var typeId in typesList)
            {
                PointsDict.Add(typeId, value);
            }
        }

        #endregion constructors

        public static ResearchPointsSpecifier GetPositive(ResearchPointsSpecifier pointsSpec)
        {
            ResearchPointsSpecifier newPoints = new();

            foreach (var (key, value) in pointsSpec.PointsDict)
            {
                if (value > 0)
                    newPoints.PointsDict[key] = value;
            }

            return newPoints;
        }

        public static ResearchPointsSpecifier GetNegative(ResearchPointsSpecifier pointsSpec)
        {
            ResearchPointsSpecifier newPoints = new();

            foreach (var (key, value) in pointsSpec.PointsDict)
            {
                if (value < 0)
                    newPoints.PointsDict[key] = value;
            }

            return newPoints;
        }

        public void TrimZeros()
        {
            foreach (var (key, value) in PointsDict)
            {
                if (value == 0)
                {
                    PointsDict.Remove(key);
                }
            }
        }

        public void Clamp(FixedPoint2 minValue, FixedPoint2 maxValue)
        {
            DebugTools.Assert(minValue < maxValue);
            ClampMax(maxValue);
            ClampMin(minValue);
        }

        public void ClampMin(FixedPoint2 minValue)
        {
            foreach (var (key, value) in PointsDict)
            {
                if (value < minValue)
                {
                    PointsDict[key] = minValue;
                }
            }
        }

        public void ClampMax(FixedPoint2 maxValue)
        {
            foreach (var (key, value) in PointsDict)
            {
                if (value > maxValue)
                {
                    PointsDict[key] = maxValue;
                }
            }
        }

        public void ExclusiveAdd(ResearchPointsSpecifier other)
        {
            foreach (var (type, value) in other.PointsDict)
            {
                if (PointsDict.TryGetValue(type, out var existing))
                {
                    PointsDict[type] = existing + value;
                }
            }
        }

        public FixedPoint2 GetSize(IPrototypeManager protoManager)
        {
            var size = FixedPoint2.New(0);

            foreach (var (key, value) in PointsDict)
            {
                var typeProto = protoManager.Index(key);

                size += value / typeProto.PointsSizeCoof;
            }

            return size;
        }

        public ResearchPointsSpecifier SizedAdd(IPrototypeManager protoManager, ResearchPointsSpecifier other, FixedPoint2 size)
        {
            ResearchPointsSpecifier added = new();

            foreach (var (key, value) in other.PointsDict)
            {
                if (size == 0)
                    break;

                var typeProto = protoManager.Index(key);
                var addValue = value;

                if (value / typeProto.PointsSizeCoof > size)
                    addValue = size * typeProto.PointsSizeCoof;

                if (!PointsDict.TryAdd(key, addValue))
                {
                    PointsDict[key] += addValue;
                }

                size -= addValue / typeProto.PointsSizeCoof;
                added.PointsDict.Add(key, addValue);
            }

            return added;
        }

        #region Operators
        public static ResearchPointsSpecifier operator *(ResearchPointsSpecifier pointsSpec, FixedPoint2 factor)
        {
            ResearchPointsSpecifier newPoints = new();
            foreach (var entry in pointsSpec.PointsDict)
            {
                newPoints.PointsDict.Add(entry.Key, entry.Value * factor);
            }
            return newPoints;
        }

        public static ResearchPointsSpecifier operator *(ResearchPointsSpecifier pointsSpec, float factor)
        {
            ResearchPointsSpecifier newPoints = new();
            foreach (var entry in pointsSpec.PointsDict)
            {
                newPoints.PointsDict.Add(entry.Key, entry.Value * factor);
            }
            return newPoints;
        }

        public static ResearchPointsSpecifier operator /(ResearchPointsSpecifier pointsSpec, FixedPoint2 factor)
        {
            ResearchPointsSpecifier newPoints = new();
            foreach (var entry in pointsSpec.PointsDict)
            {
                newPoints.PointsDict.Add(entry.Key, entry.Value / factor);
            }
            return newPoints;
        }

        public static ResearchPointsSpecifier operator /(ResearchPointsSpecifier pointsSpec, float factor)
        {
            ResearchPointsSpecifier newPoints = new();

            foreach (var entry in pointsSpec.PointsDict)
            {
                newPoints.PointsDict.Add(entry.Key, entry.Value / factor);
            }
            return newPoints;
        }

        public static ResearchPointsSpecifier operator +(ResearchPointsSpecifier pointsSpecA, ResearchPointsSpecifier pointsSpecB)
        {
            ResearchPointsSpecifier newPoints = new(pointsSpecA);

            foreach (var entry in pointsSpecB.PointsDict)
            {
                if (!newPoints.PointsDict.TryAdd(entry.Key, entry.Value))
                {
                    newPoints.PointsDict[entry.Key] += entry.Value;
                }
            }
            return newPoints;
        }

        public static ResearchPointsSpecifier operator -(ResearchPointsSpecifier pointsSpecA, ResearchPointsSpecifier pointsSpecB)
        {
            ResearchPointsSpecifier newPoints = new(pointsSpecA);

            foreach (var entry in pointsSpecB.PointsDict)
            {
                if (!newPoints.PointsDict.TryAdd(entry.Key, -entry.Value))
                {
                    newPoints.PointsDict[entry.Key] -= entry.Value;
                }
            }
            return newPoints;
        }

        public static ResearchPointsSpecifier operator +(ResearchPointsSpecifier pointsSpec) => pointsSpec;

        public static ResearchPointsSpecifier operator -(ResearchPointsSpecifier pointsSpec) => pointsSpec * -1;

        public static ResearchPointsSpecifier operator *(float factor, ResearchPointsSpecifier pointsSpec) => pointsSpec * factor;

        public static ResearchPointsSpecifier operator *(FixedPoint2 factor, ResearchPointsSpecifier pointsSpec) => pointsSpec * factor;

        // Is self superset of other
        public bool IsSuperset(ResearchPointsSpecifier? other)
        {
            if (other == null || PointsDict.Count <= other.PointsDict.Count)
                return false;

            foreach (var (key, value) in other.PointsDict)
            {
                if (!PointsDict.TryGetValue(key, out var selfValue) || value > selfValue)
                    return false;
            }

            return true;
        }

        public bool Equals(ResearchPointsSpecifier? other)
        {
            if (other == null || PointsDict.Count != other.PointsDict.Count)
                return false;

            foreach (var (key, value) in PointsDict)
            {
                if (!other.PointsDict.TryGetValue(key, out var otherValue) || value != otherValue)
                    return false;
            }

            return true;
        }

        public FixedPoint2 this[string key] => PointsDict[key];
    }
    #endregion
}
