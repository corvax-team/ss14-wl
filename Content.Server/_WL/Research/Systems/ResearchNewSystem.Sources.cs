using System.Numerics;
using System.Linq;
using Content.Server.Power.EntitySystems;
using Content.Shared._WL.Research;
using Content.Shared._WL.Research.Methods;
using Content.Shared._WL.Research.Components;
using Content.Shared._WL.Research.Prototypes;
using Content.Shared.FixedPoint;
using Robust.Shared.Prototypes;

namespace Content.Server._WL.Research.Systems;

public sealed partial class ResearchSystemNew
{
    [Dependency] private SharedResearchConditionsSystem _researchConditions = default!;

    private void InitializeSource()
    { }

    [SubscribeLocalEvent]
    private void OnSourceDetect(Entity<ResearchSourceComponent> ent, ref GetResearchDataEvent args)
    {
        if (!TryGetConditionsData(ent, args.ResearchTypes, out var data, out var pointsVec, args.Detector))
            return;

        args.ResearchData = data;
        args.PointsVec = pointsVec;
    }

    private bool TryGetConditionsData(
            EntityUid uid,
            List<ProtoId<ResearchTypePrototype>> types,
            out Dictionary<ProtoId<ResearchTypePrototype>, FixedPoint4> data,
            out ResearchPointsSpecifier pointsVec,
            EntityUid? detectorUid = null)
    {
        data = new();
        pointsVec = new();

        if (!TryComp<ResearchSourceComponent>(uid, out var source) ||
                !ProtoMan.TryIndex<ResearchCategoryPrototype>(source.Category, out var category) ||
                !source.Active)
            return false;

        foreach (var typeId in types)
        {
            if (!category.ResearchTypes.Contains(typeId) ||
                    !ProtoMan.TryIndex<ResearchTypePrototype>(typeId, out var type))
                continue;

            var (researchValue, points) = _researchConditions.GetCondition(uid, type.Condition, detectorUid);

            pointsVec += points;
            data.Add(typeId, researchValue);
        }

        if (pointsVec.Empty)
            return false;

        if (category.DefaultType is {} defaultType)
            pointsVec = pointsVec.ConvertToType(defaultType, [ExtrimalType]);

        pointsVec = pointsVec.Normalize();

        return true;
    }
}
