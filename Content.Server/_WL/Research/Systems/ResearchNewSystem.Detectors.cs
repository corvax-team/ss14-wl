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
        Logger.Debug("RESEARCHDET START");
        if (!CanProduce(ent) ||
                !TryComp<ResearchDetectorComponent>(ent, out var detector) ||
                !TryFindSources(ent.Owner, detector.Range, out var sources))
            return;

        Logger.Debug("RESEARCHDET1 START");
        foreach (var source in sources)
        {
            Logger.Debug(source.Id.ToString());
            if (!TryComp<ResearchSourceComponent>(source, out var sourceComp) ||
                    !TryAnalyzeByDetector(ent, source, out var data, out var pointsVec))
                continue;

            ResearchDataFrame dataFrame;

            if (!args.ResearchData.TryGetValue(sourceComp.Category, out var currDataFrame))
            {
                dataFrame = new();
                args.ResearchData.Add(sourceComp.Category, dataFrame);
            }
            else
            {
                dataFrame = currDataFrame;
            }

            Logger.Debug(dataFrame.TryInsertData(data, ent, pointsVec).ToString());
        }
    }

    private bool TryFindSources(EntityUid ent, float range, out List<EntityUid> finded)
    {
        var entXForm = Transform(ent);
        var worldPos = _transform.GetWorldPosition(entXForm);

        var box = Box2.CenteredAround(worldPos, new Vector2(range));

        finded = new List<EntityUid>();

        foreach (var entity in _lookup.GetEntitiesIntersecting(entXForm.MapID, box))
        {
            if (HasComp<ResearchSourceComponent>(entity))
                finded.Add(entity);
        }

        return finded.Count != 0;
    }

    public bool CanProduce(Entity<PassiveDetectorComponent> source)
    {
        return source.Comp.Active && this.IsPowered(source, EntityManager);
    }

    private bool TryAnalyzeByDetector(
            EntityUid detector,
            EntityUid source,
            out Dictionary<ProtoId<ResearchTypePrototype>, FixedPoint4> data,
            out ResearchPointsSpecifier pointsRatioVec)
    {
        data = new();
        pointsRatioVec = new();

        if (!TryComp<ResearchDetectorComponent>(detector, out var detectorComp) ||
                !HasComp<ResearchSourceComponent>(source))
            return false;

        var ev = new GetResearchDataEvent(detector, detectorComp.ResearchTypes);
        RaiseLocalEvent(source, ref ev);

        Logger.Debug("TRY ANALIZE");

        if (ev.PointsVec.Empty)
            return false;

        Logger.Debug("SOME PTS");
        pointsRatioVec = ev.PointsVec * detectorComp.ResearchPercent;

        if (detectorComp.DefaultPointsType is {} defaultType &&
                detectorComp.DetectorPriority)
            pointsRatioVec = pointsRatioVec.ConvertToType(defaultType, [ExtrimalType]);

        data = ev.ResearchData;

        return true;
    }
}
