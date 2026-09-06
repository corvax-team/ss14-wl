using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Content.Server.Power.EntitySystems;
using Content.Shared._WL.Research;
using Content.Shared._WL.Research.Components;
using Content.Shared._WL.Research.Prototypes;
using Content.Shared.Research.Prototypes;
using Robust.Shared.Utility;
using Robust.Shared.Prototypes;

namespace Content.Server._WL.Research.Systems;

public sealed partial class ResearchSystemNew
{
    private void InitializeTechnologies()
    { }

    private void UpdateResearchesStatus(EntityUid uid, TechnologyServerComponent? techServer = null, ResearchServerNewComponent? server = null)
    {
        if (!Resolve(uid, ref techServer) || !Resolve(uid, ref server))
            return;

        foreach (var (proto, state) in techServer.Researches)
        {
            if (state.Status != ResearchStatus.NotResearched)
                continue;

            var bufferState = techServer.Researches[proto];
            bufferState.DepsState = GetResearchState(uid, proto, bufferState, techServer);

            techServer.Researches[proto] = bufferState;
        }

        Dirty(uid, techServer);
    }

    private void UpdateResearchesProgress(EntityUid uid, TechnologyServerComponent? techServer = null, ResearchServerNewComponent? server = null)
    {
        if (!Resolve(uid, ref techServer) || !Resolve(uid, ref server))
            return;

        if (techServer.ResearchQueue.Count == 0)
            return;

        var mainResearchId = techServer.ResearchQueue[0];
        var mainResearchProto = ProtoMan.Index<ResearchPrototype>(mainResearchId);

        if (!techServer.Researches.TryGetValue(mainResearchProto, out var mainResearchState))
            return;

        var researchSpeed = GetResearchSpeed(uid, server);
        // var modeProto = ProtoMan.Index(mainResearchState.ModeId);

        var cost = (int)(mainResearchState.PackagesCostModed);

        mainResearchState.ResearchedPackages = Math.Min(mainResearchState.ResearchedPackages.Int() + researchSpeed, cost);

        if (mainResearchState.ResearchedPackages >= cost)
        {
            FinishResearch(uid, mainResearchProto, server);
            mainResearchState.Status = ResearchStatus.Researched;

            techServer.ResearchQueue.RemoveAt(0);

            if (techServer.ResearchQueue.Count > 0 &&
                techServer.Researches.TryGetValue(techServer.ResearchQueue[0], out var newResearchState))
            {
                newResearchState.Status = ResearchStatus.Researching;
                techServer.Researches[techServer.ResearchQueue[0]] = newResearchState;
            }
        }

        techServer.Researches[mainResearchProto] = mainResearchState;

        Dirty(uid, server);
        Dirty(uid, techServer);
    }

    private void FinishResearch(EntityUid uid, ProtoId<ResearchPrototype> researchProto, ResearchServerNewComponent? server = null)
    {
        if (!Resolve(uid, ref server))
            return;

        //TODO: add finish research event
    }

    private bool TryStartResearch(EntityUid uid, ProtoId<ResearchPrototype> researchId, TechnologyServerComponent? techServer = null, ProtoId<ResearchModePrototype>? modeId = null)
    {
        if (!Resolve(uid, ref techServer))
            return false;

        if (!techServer.Researches.TryGetValue(researchId, out var researchState))
            return false;

        if (GetResearchState(uid, researchId, researchState, techServer) != ResearchDepsStatus.Allowed)
            return false;

        modeId ??= researchState.ModeId;

        var modeProto = ProtoMan.Index<ResearchModePrototype>(modeId);
        var researchProto = ProtoMan.Index<ResearchPrototype>(researchId);

        TryModifyPoints(uid, -researchProto.PointsCost * modeProto.PackagesModifier, false); // TODO: apply modifier, -1 & value check

        techServer.ResearchQueue.Add(researchId);

        if (techServer.ResearchQueue.Count == 1)
            researchState.Status = ResearchStatus.Researching;
        else
            researchState.Status = ResearchStatus.InQueue;

        researchState.ModeId = modeId.Value;
        researchState.PackagesCostModed = modeProto.PackagesModifier * researchProto.PackagesCost;

        techServer.Researches[researchId] = researchState;

        Dirty(uid, techServer);

        return true;
    }

    public ResearchDepsStatus GetResearchState(EntityUid uid, ProtoId<ResearchPrototype> researchProto, ResearchState researchState, TechnologyServerComponent? techServer = null, PointsDataStorageComponent? storage = null)
    {
        if (!Resolve(uid, ref techServer) || !Resolve(uid, ref storage))
            return ResearchDepsStatus.Invalid;

        var research = ProtoMan.Index(researchProto);
        var modeProto = ProtoMan.Index(researchState.ModeId);

        foreach (var parent in research.ParentsResearches)
        {
            if (!techServer.Researches.TryGetValue(parent, out var parentState) ||
                    parentState.Status != ResearchStatus.Researched)
                return ResearchDepsStatus.ParentsReq;
        }

        if (!storage.Points.IsSuperset(research.PointsCost * modeProto.PointsModifier))
                return ResearchDepsStatus.PointsReq;

        return ResearchDepsStatus.Allowed;
    }
}
