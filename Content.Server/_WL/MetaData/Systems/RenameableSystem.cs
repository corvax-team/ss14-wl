using Content.Server._WL.MetaData.Components;
using Content.Server.Administration;
using Content.Server.Charges;
using Content.Shared.Charges.Components;
using Content.Shared.Verbs;
using Robust.Server.Player;
using Robust.Shared.Player;
using Robust.Shared.Utility;
using System.Linq;

namespace Content.Server._WL.MetaData.Systems;
public sealed partial class RenameableSystem : EntitySystem
{
    [Dependency] private readonly MetaDataSystem _metaData = default!;
    [Dependency] private readonly ChargesSystem _charges = default!;
    [Dependency] private readonly QuickDialogSystem _quickDialog = default!;
    [Dependency] private readonly IPlayerManager _playMan = default!;

    public static readonly LocId RenameActionLocString = "renameable-component-rename-action";
    public static readonly LocId NameTitleLocString = "renameable-component-name-field";

    private static readonly ResPath VerbTexturePath = new("/Textures/Interface/AdminActions/rename.png");

    private const int NewNameMaxLength = 40;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RenameOnInteractComponent, GetVerbsEvent<InteractionVerb>>(OnGetVerbs);
    }

    public bool TryRename(Entity<RenameOnInteractComponent?, MetaDataComponent?> entity, string newName, bool raiseEvents = true)
    {
        var name = FormatNewName(newName);

        if (!IsNewNameValid(name))
            return false;

        if (!Resolve(entity, ref entity.Comp1, ref entity.Comp2, false))
            return false;

        if (entity.Comp1.NeedCharges)
        {
            if (!TryComp<LimitedChargesComponent>(entity, out var chargesComp) || HasCharge((entity, chargesComp)) == false)
                return false;

            if (!_charges.TryUseCharge((entity, chargesComp)))
                return false;
        }

        _metaData.SetEntityName(entity, name, entity.Comp2, raiseEvents);

        return true;
    }

    public bool IsNewNameValid(string str)
    {
        if (str.Length > NewNameMaxLength)
            return false;

        if (string.IsNullOrWhiteSpace(str))
            return false;

        if (str.Any(c => char.IsNumber(c) || char.IsPunctuation(c)))
            return false;

        return true;
    }

    public string FormatNewName(string str)
    {
        return str.ToLower();
    }

    public bool TryOpenDialog(ICommonSession session, Entity<RenameOnInteractComponent?> item)
    {
        if (session.AttachedEntity == null)
            return false;

        if (!Resolve(item, ref item.Comp, false))
            return false;

        var titleLoc = Loc.GetString(RenameActionLocString);
        var promptLoc = Loc.GetString(NameTitleLocString);

        _quickDialog.OpenDialog(session, titleLoc, promptLoc, (string newName) =>
        {
            TryRename(item, newName, true);
        }, null);

        return true;
    }

    private bool? HasCharge(Entity<LimitedChargesComponent?> item)
    {
        if (!Resolve(item, ref item.Comp, false))
            return null;

        return _charges.HasCharges(item, 1);
    }

    private void OnGetVerbs(EntityUid item, RenameOnInteractComponent comp, GetVerbsEvent<InteractionVerb> ev)
    {
        if (!comp.UseVerbs)
            return;

        if (comp.NeedCharges && HasCharge(item) == false)
            return;

        var verb = new InteractionVerb()
        {
            Act = () =>
            {
                var user = ev.User;
                if (!_playMan.TryGetSessionByEntity(user, out var session))
                    return;

                TryOpenDialog(session, item);
            },
            Impact = Shared.Database.LogImpact.Low,
            Text = Loc.GetString(RenameActionLocString),
            Icon = new SpriteSpecifier.Texture(VerbTexturePath),
            Priority = 10,
        };

        ev.Verbs.Add(verb);
    }
}
