using Content.Server.Examine;
using Content.Server.Preferences.Managers;
using Content.Shared._WL.CharacterInformation;
using Content.Shared.IdentityManagement;
using Content.Shared.Preferences;
using Content.Shared.Verbs;
using Robust.Server.GameObjects;
using Robust.Shared.Player;
using Robust.Shared.Utility;

namespace Content.Server._WL.CharacterInformation;

public sealed class CharacterInformationSystem : EntitySystem
{
    [Dependency] private readonly ExamineSystem _examineSystem = default!;
    [Dependency] private readonly UserInterfaceSystem _userInterfaceSystem = default!;
    [Dependency] private readonly IServerPreferencesManager _preferencesManager = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CharacterInformationComponent, GetVerbsEvent<ExamineVerb>>(OnGetExamineVerbs);
    }

    private void OnGetExamineVerbs(EntityUid uid, CharacterInformationComponent component, GetVerbsEvent<ExamineVerb> args)
    {
        if (!TryComp(args.User, out ActorComponent? actor))
            return;

        if (Identity.Name(args.Target, EntityManager) != MetaData(args.Target).EntityName)
            return;

        var detailsRange = _examineSystem.IsInDetailsRange(args.User, args.Target);
        var verb = new ExamineVerb
        {
            Act = () =>
            {
                _userInterfaceSystem.TryOpenUi(args.User, CharacterInformationUiKey.Key, args.User);
                UpdateUI(args.User, args.Target);
            },
            Text = Loc.GetString("detail-examinable-verb-text"),
            Category = VerbCategory.Examine,
            Disabled = !detailsRange,
            Message = detailsRange ? null : Loc.GetString("detail-examinable-verb-disabled"),
            Icon = new SpriteSpecifier.Texture(new ("/Textures/Interface/VerbIcons/examine.svg.192dpi.png"))
        };

        args.Verbs.Add(verb);
    }

    private void UpdateUI(EntityUid uid, EntityUid targetUid)
    {
        if (!TryComp<CharacterInformationComponent>(targetUid, out var charInfo))
            return;

        var charName = Identity.Name(targetUid, EntityManager);
        var state = new CharacterInformationBuiState(GetNetEntity(targetUid), charName, charInfo.FlavorText, charInfo.OocText, charInfo.DynamicText);
        _userInterfaceSystem.SetUiState(uid, CharacterInformationUiKey.Key, state);
    }
}
