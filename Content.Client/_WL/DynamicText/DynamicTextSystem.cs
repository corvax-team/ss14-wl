using Content.Client._WL.DynamicText.UI;
using Content.Client.Mind;
using Content.Shared._WL.DynamicText;
using Content.Shared.Mobs.Components;
using Content.Shared.Verbs;
using Robust.Client.Player;
using Robust.Client.UserInterface;
using Robust.Shared.Utility;

namespace Content.Client._WL.DynamicText;

public sealed partial class DynamicTextSystem : EntitySystem
{
    [Dependency] private IEntityManager _ent = default!;
    [Dependency] private IPlayerManager _player = default!;
    [Dependency] private IUserInterfaceManager _userInterfaceManager = default!;
    [Dependency] private MindSystem _mindSystem = default!;

    private EntityUid? _editingEntity;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeNetworkEvent<RequestedDynamicTextEvent>(OnDynamicTextReceived);
        SubscribeLocalEvent<GetVerbsEvent<Verb>>(OnGetVerbs);
    }

    private void OnGetVerbs(GetVerbsEvent<Verb> args)
    {
        if (_player.LocalEntity is not { } player ||
            args.User != player)
        {
            return;
        }

        var isSelf = args.Target == player;

        if (!isSelf &&
            (_mindSystem.TryGetMind(args.Target, out _, out _) || HasComp<MobStateComponent>(args.Target)))
        {
            return;
        }

        args.Verbs.Add(new Verb
        {
            Text = Loc.GetString("dynamic-text-verb"),
            Icon = new SpriteSpecifier.Texture(
                new ResPath("/Textures/_WL/Interface/VerbIcons/pen.svg.192dpi.png")),
            ClientExclusive = true,

            Act = () =>
            {
                SetEditingEntity(args.Target);

                _userInterfaceManager
                    .GetUIController<DynamicTextUIController>()
                    .OpenWindow();
            },
        });
    }

    public void SaveDynamicText(string text)
    {
        if (_editingEntity is not { } editingEntity)
            return;

        if (!_ent.TryGetNetEntity(editingEntity, out var netEntity))
            return;

        RaiseNetworkEvent(new SetDynamicTextEvent(netEntity.Value, text));
    }

    public void RequestDynamicText()
    {
        if (_editingEntity is not { } editingEntity)
            return;

        if (!_ent.TryGetNetEntity(editingEntity, out var netEntity))
            return;

        RaiseNetworkEvent(new RequestDynamicTextEvent(netEntity.Value));
    }

    private void OnDynamicTextReceived(RequestedDynamicTextEvent ev, EntitySessionEventArgs args)
    {
        _userInterfaceManager.GetUIController<DynamicTextUIController>().SetDynamicText(ev.DynamicText);
    }

    public void SetEditingEntity(EntityUid uid)
    {
        _editingEntity = uid;
    }

    public void ClearEditingEntity()
    {
        _editingEntity = null;
    }
}
