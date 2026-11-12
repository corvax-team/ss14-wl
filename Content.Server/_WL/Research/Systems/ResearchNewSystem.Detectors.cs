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
    [Dependency] private SharedTransformSystem _transform = default!;

    private void InitializeDetectors()
    { }

    [SubscribeLocalEvent]
    private void OnDetectorServerResearch(Entity<PassiveDetectorComponent> ent, ref GetServerResearchEvent args)
    {
        if (!CanProduce(ent) || !TryComp<ResearchDetectorComponent>(ent, out var detector))
            return;

        foreach (var source in EnumerateSources((ent, detector), ent.Comp.Range))
        {
            if (!TryAnalyzeByDetector(ent, source, out var data))
                continue;

            foreach (var (category, (categoryData, pointsVec)) in data)
            {
                ResearchDataFrame dataFrame;

                if (!args.ResearchData.TryGetValue(category, out var currDataFrame))
                {
                    dataFrame = new();
                    args.ResearchData.Add(category, dataFrame);
                }
                else
                {
                    dataFrame = currDataFrame;
                }

                dataFrame.TryInsertData(categoryData, ent, pointsVec);
            }
        }
    }

    private IEnumerable<Entity<ResearchSourceComponent>> EnumerateSources(Entity<ResearchDetectorComponent> ent, float range)
    {
        if (ent.Comp.SelfOnly && TryComp<ResearchSourceComponent>(ent, out var source))
        {
            yield return (ent, source);
            yield break;
        }

        var entXForm = Transform(ent);
        var worldPos = _transform.GetWorldPosition(entXForm);

        var box = Box2.CenteredAround(worldPos, new Vector2(range));

        foreach (var entity in _lookup.GetEntitiesIntersecting(entXForm.MapID, box))
        {
            if (TryComp<ResearchSourceComponent>(entity, out var comp))
                yield return (entity, comp);
        }
    }

    public bool CanProduce(Entity<PassiveDetectorComponent> source)
    {
        return source.Comp.Active && this.IsPowered(source, EntityManager);
    }

    private bool TryAnalyzeByDetector(
            EntityUid detector,
            EntityUid source,
            out Dictionary<ProtoId<ResearchCategoryPrototype>, (Dictionary<ProtoId<ResearchTypePrototype>, FixedPoint4>, ResearchPointsSpecifier)> data)
    {
        data = new();

        if (!TryComp<ResearchDetectorComponent>(detector, out var detectorComp) ||
                !TryComp<ResearchSourceComponent>(source, out var sourceComp))
            return false;

        foreach (var category in sourceComp.Categories)
        {
            var ev = new GetResearchDataEvent(detector, detectorComp.ResearchTypes, category);
            RaiseLocalEvent(source, ref ev);

            if (ev.PointsVec.Empty)
                continue;

            var pointsRatioVec = ev.PointsVec * detectorComp.ResearchPercent;

            if (detectorComp.DefaultPointsType is {} defaultType &&
                    detectorComp.DetectorPriority)
                pointsRatioVec = pointsRatioVec.ConvertToType(defaultType, [ExtrimalType]);

            data.TryAdd(category, (ev.ResearchData, pointsRatioVec));
        }

        return data.Count != 0;
    }

    private bool TryAnalyzeToServer(
            EntityUid detector,
            EntityUid source,
            ResearchDetectorComponent? detectorComp = null,
            ResearchSourceComponent? sourceComp = null)
    {
        if (!Resolve(detector, ref detectorComp) || !Resolve(source, ref sourceComp))
            return false;

        if (!TryGetClientServer(detector, out var serverOut, out var serverCompOut) ||
                serverOut is not {} server ||
                serverCompOut is not {} serverComp)
            return false;

        if (!TryAnalyzeByDetector(detector, source, out var data))
            return false;

        var result = false;
        foreach (var (category, (researchData, points)) in data)
        {
            result |= TryProcessResearchData(server, category, researchData, points, serverComp);
        }

        return result;
    }

    private bool TrySimpleAnalyze(
            EntityUid detector,
            EntityUid source,
            ResearchDetectorComponent? detectorComp = null,
            SimpleSourceComponent? sourceComp = null)
    {
        if (!Resolve(detector, ref detectorComp) || !Resolve(source, ref sourceComp))
            return false;

        if (!TryGetClientServer(detector, out var serverOut, out var serverCompOut) ||
                serverOut is not {} server ||
                serverCompOut is not {} serverComp)
            return false;

        return TryProcessSimpleData(server, sourceComp.Id, sourceComp.Points, serverComp);
    }
}
