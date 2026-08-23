using Content.Server.GameTicking;
using Content.Server.Corvax.GuideGenerator;
using Content.Server.Mind;
using Content.Server.Roles.Jobs;
using Content.Server.Station.Systems;
using Content.Shared.Hands;
using Content.Shared.IdentityManagement;
using Content.Shared.Interaction;
using Content.Shared._WL.Paper;
using Content.Shared.Paper;
using Content.Shared.Verbs;
using Robust.Shared.Timing;

namespace Content.Server._WL.Documents
{
    public sealed partial class PrintedDocumentFormatSystem : EntitySystem
    {
        [Dependency] private PaperSystem _paper = default!;
        [Dependency] private StationSystem _station = default!;
        [Dependency] private IGameTiming _gameTime = default!;
        [Dependency] private GameTicker _gameTick = default!;
        [Dependency] private JobSystem _job = default!;
        [Dependency] private MindSystem _mind = default!;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<PrintedDocumentFormatComponent, MapInitEvent>(OnMapInit, after: [typeof(PaperSystem)]);
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

            FormatDocument(
                (document, paperComp),
                true,
                (Loc.GetString("doc-var-date"), formattedDate),
                (Loc.GetString("doc-var-station"), stationName ?? "Station XX-000"));
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

            FormatDocument(
                paper,
                false,
                (Loc.GetString("doc-var-name"), Identity.Name(user, EntityManager)),
                (Loc.GetString("doc-var-job"), job != null ? TextTools.CapitalizeString(job) : string.Empty));
        }

        private void FormatDocument(
            Entity<PaperComponent> paper,
            bool localizeLegacy,
            params (string Search, string Replacement)[] replacements)
        {
            if (localizeLegacy && !HasComp<StructuredPaperComponent>(paper))
                _paper.SetContent(paper, Loc.GetString(paper.Comp.Content));

            _paper.TransformContent(paper, content =>
            {
                foreach (var (search, replacement) in replacements)
                    content = content.Replace(search, replacement);

                return content;
            });
        }
    }
}
