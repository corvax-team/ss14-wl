using Content.Server.GameTicking;
using Content.Server.GuideGenerator.TextTools;
using Content.Server.Mind;
using Content.Server.Roles.Jobs;
using Content.Server.Station.Systems;
using Content.Shared.Hands;
using Content.Shared.IdentityManagement;
using Content.Shared.Interaction;
using Content.Shared.Paper;
using Content.Shared.Verbs;
using Robust.Shared.Timing;

namespace Content.Server._WL.Documents
{
    public sealed partial class PrintedDocumentFormatSystem : EntitySystem
    {
        [Dependency] private readonly PaperSystem _paper = default!;
        [Dependency] private readonly StationSystem _station = default!;
        [Dependency] private readonly IGameTiming _gameTime = default!;
        [Dependency] private readonly GameTicker _gameTick = default!;
        [Dependency] private readonly JobSystem _job = default!;
        [Dependency] private readonly MindSystem _mind = default!;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<PrintedDocumentFormatComponent, MapInitEvent>(OnMapInit);
            SubscribeLocalEvent<PrintedDocumentFormatComponent, GotEquippedHandEvent>(OnPick);
            SubscribeLocalEvent<PrintedDocumentFormatComponent, GetVerbsEvent<AlternativeVerb>>(OnVerb);
            SubscribeLocalEvent<PrintedDocumentFormatComponent, InteractUsingEvent>(OnInteract, before: [typeof(PaperSystem)]);
        }

        //No public api babe
        //>:3c
        //:despair:

        private void OnMapInit(EntityUid document, PrintedDocumentFormatComponent comp, MapInitEvent args)
        {
            var paperComp = EnsureComp<PaperComponent>(document);

            var station = _station.GetOwningStation(document);
            var stationName = station != null
                ? Name(station.Value)
                : null;

            var formattedDate = $"{_gameTime.CurTime.Subtract(_gameTick.RoundStartTimeSpan).ToString(@"hh\:mm\:ss")} {DateTime.Now.AddYears(-1700):dd.MM.yyy}";

            var content = Loc.GetString(paperComp.Content)
                .Replace(Loc.GetString("doc-var-date"),  formattedDate)
                .Replace(Loc.GetString("doc-var-station"), stationName ?? "Station XX-000");

            _paper.SetContent((document, paperComp), content);
        }

        private void OnPick(EntityUid document, PrintedDocumentFormatComponent comp, GotEquippedHandEvent args)
        {
            if (args.Handled)
                return;

            if (comp.Taken)
                return;

            var paperComp = EnsureComp<PaperComponent>(document);

            comp.Taken = true;

            ChangeContentWhenPickup((document, paperComp), args.User);
        }

        private void OnVerb(EntityUid document, PrintedDocumentFormatComponent comp, GetVerbsEvent<AlternativeVerb> args)
        {
            if (!args.CanInteract || !args.CanAccess)
                return;

            if (comp.Taken)
                return;

            var paperComp = EnsureComp<PaperComponent>(document);

            comp.Taken = true;

            ChangeContentWhenPickup((document, paperComp), args.User);
        }

        private void OnInteract(EntityUid document, PrintedDocumentFormatComponent comp, InteractUsingEvent args)
        {
            if (args.Handled)
                return;

            if (comp.Taken)
                return;

            var paperComp = EnsureComp<PaperComponent>(document);

            comp.Taken = true;

            ChangeContentWhenPickup((document, paperComp), args.User);
        }

        private void ChangeContentWhenPickup(Entity<PaperComponent> paper, EntityUid user)
        {
            _mind.TryGetMind(user, out var mindId, out _);
            var job = _job.MindTryGetJobName(mindId);

            var content = paper.Comp.Content
                .Replace(Loc.GetString("doc-var-name"), Identity.Name(user, EntityManager))
                .Replace(Loc.GetString("doc-var-job"), job != null ? TextTools.CapitalizeString(job) : null);

            _paper.SetContent(paper, content);
        }
    }
}
