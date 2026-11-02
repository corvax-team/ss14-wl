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
    [Dependency] private SharedTransformSystem _transform = default!;

    private void InitializeSource()
    { }

    [SubscribeLocalEvent]
    private void OnDetectorServerResearch(Entity<PassiveDetectorComponent> ent, ref GetServerResearchEvent args)
    {
        if (!CanProduce(ent))
            return;

        if (!TryComp<ResearchDetectorComponent>(ent, out var detector))
            return;

        if (!TryFindResearches(ent.Owner, detector.Range, out var sources))
            return;

        foreach (var researchType in detector.ResearchTypes)
        {
            var ev = new GetResearchDataEvent(ent.Owner, researchType);
            foreach (var source in sources)
            {
                RaiseLocalEvent(source, ref ev);
            }

            foreach (var (category, (researchValue, data)) in ev.ResearchData)
            {
                var normileCoof = data.GetTotal();
                if (normileCoof <= 0)
                    continue;

                foreach (var (type, value) in data.PointsDict)
                {
                    var pointsType = type;

                    if (type != detector.ExtrimalPointsType && detector.DefaultPointsType is { } detectorType)
                        pointsType = detectorType;

                    AddResearch(category, researchType, researchValue, pointsType, value / normileCoof * detector.ResearchPercent, ref args);
                }
            }
        }
    }

    [SubscribeLocalEvent]
    private void OnSourceDetect(Entity<ResearchSourceComponent> ent, ref GetResearchDataEvent args)
    {
        if (args.ResearchData.ContainsKey(ent.Comp.Category))
            return;

        var categoryProto = ProtoMan.Index<ResearchCategoryPrototype>(ent.Comp.Category);

        if (!categoryProto.ResearchTypes.Contains(args.ResearchType))
            return;

        var typeProto = ProtoMan.Index<ResearchTypePrototype>(args.ResearchType);

        var (researchValue, points) = _researchConditions.GetCondition(ent, typeProto.Condition, args.Detector);

        if (categoryProto.DefaultType is { } pointsType)
            points = points.ConvertToType(pointsType, new List<ProtoId<ResearchPointsTypePrototype>>() { categoryProto.ExtrimalType });

        args.ResearchData.Add(ent.Comp.Category, (researchValue, points));
    }

    [SubscribeLocalEvent]
    private void OnDebugServerResearch(Entity<PointsSourceDebugComponent> ent, ref GetServerResearchEvent args)
    {
        AddResearch(ent.Comp.DebugCategory, ent.Comp.Type, ent.Comp.Value, ent.Comp.PointsType, ent.Comp.Points, ref args);
    }

    private void AddResearch(
        ProtoId<ResearchCategoryPrototype> category,
        ProtoId<ResearchTypePrototype> resType,
        FixedPoint4 resValue,
        ProtoId<ResearchPointsTypePrototype> pointsType,
        FixedPoint4 pointsValue,
        ref GetServerResearchEvent args)
    {
        if (!args.ResearchData.ContainsKey(category))
        {
            args.ResearchData.Add(
                category,
                (new Dictionary<ProtoId<ResearchTypePrototype>, FixedPoint4>() {[resType] = resValue},
                 new ResearchPointsSpecifier(pointsType, pointsValue))
            );
            return;
        }

        if (args.ResearchData[category].Item1.TryAdd(resType, resValue))
            if (!args.ResearchData[category].Item2.PointsDict.TryAdd(pointsType, pointsValue))
                args.ResearchData[category].Item2.PointsDict[pointsType] = args.ResearchData[category].Item2[pointsType] + pointsValue;
    }

    private bool TryFindResearches(EntityUid ent, float range, out List<EntityUid> finded)
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

    public bool TryGetResearch(
            EntityUid uid,
            EntityUid target,
            out Dictionary<ProtoId<ResearchTypePrototype>, FixedPoint4> data,
            out ResearchPointsSpecifier points,
            ResearchDetectorComponent? detector = null)
    {
        data = new Dictionary<ProtoId<ResearchTypePrototype>, FixedPoint4>();
        points = new ResearchPointsSpecifier();

        if (!Resolve(uid, ref detector))
            return false;

        if (!TryComp<ResearchSourceComponent>(target, out var source))
            return false;

        var category = ProtoMan.Index(source.Category);
        foreach (var researchType in category.ResearchTypes)
        {
            if (data.ContainsKey(researchType) || !detector.ResearchTypes.Contains(researchType))
                continue;

            var typeProto = ProtoMan.Index<ResearchTypePrototype>(researchType);

            var (conditionData, conditionPoints) = _researchConditions.GetCondition(target, typeProto.Condition, uid);
            data[researchType] = conditionData;
            points += conditionPoints;
        }

        if (category.DefaultType is {} type)
            points = points.ConvertToType(type, new List<ProtoId<ResearchPointsTypePrototype>>() { category.ExtrimalType });

        return true;
    }

    public bool CanProduce(Entity<PassiveDetectorComponent> source)
    {
        return source.Comp.Active && this.IsPowered(source, EntityManager);
    }

    public void SendResearch(EntityUid uid, EntityUid? extTarget = null, ResearchDetectorComponent? detector = null, ResearchSourceComponent? source = null)
    {
        var target = extTarget ?? uid;

        if (!Resolve(uid, ref detector) || !Resolve(target, ref source))
            return;

        if (!TryGetClientServer(uid, out var serverUid, out var serverComp) || serverUid is not {} server)
            return;

        if (!TryGetResearch(uid, target, out var data, out var points, detector))
            return;

        if (points.Empty)
            return;

        InsertResearchData(server, source.Category, data, points / points.GetTotal(), serverComp);
    }
}
