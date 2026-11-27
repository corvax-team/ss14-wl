using Content.Shared._Ship14.CorticalBorer;
using Content.Shared._Ship14.CorticalBorer.Components;
using Content.Shared.Chat;
using Content.Shared.Damage;
using Content.Shared.Eye.Blinding.Systems;
using Content.Shared.FixedPoint;
using Content.Shared.IdentityManagement;
using Content.Shared.Pointing;
using Robust.Shared.Player;
using Robust.Shared.Utility;

namespace Content.Server._Ship14.CorticalBorer;

public sealed partial class CorticalBorerSystem
{
    private void OnConverseAction(Entity<CorticalBorerComponent> ent, ref CorticalBorerConverseActionEvent args)
    {
        if (args.Handled || ent.Comp.Host == null || ent.Comp.ControllingHost || ent.Comp.SugarSuppressed)
            return;

        args.Handled = true;
        OpenMenu(ent, CorticalBorerMenuMode.Converse);
    }

    private void OnPunishAction(Entity<CorticalBorerComponent> ent, ref CorticalBorerPunishActionEvent args)
    {
        if (args.Handled || ent.Comp.Host == null || ent.Comp.ControllingHost || ent.Comp.SugarSuppressed)
            return;

        args.Handled = true;
        OpenMenu(ent, CorticalBorerMenuMode.Punish);
    }

    private void OnInjectAction(Entity<CorticalBorerComponent> ent, ref CorticalBorerInjectActionEvent args)
    {
        if (args.Handled || ent.Comp.Host == null || ent.Comp.ControllingHost || ent.Comp.SugarSuppressed)
            return;

        args.Handled = true;
        OpenMenu(ent, CorticalBorerMenuMode.Inject);
    }

    private void OpenMenu(Entity<CorticalBorerComponent> ent, CorticalBorerMenuMode mode)
    {
        ent.Comp.MenuMode = mode;
        Dirty(ent);
        _ui.CloseUi(ent.Owner, CorticalBorerUiKey.Key, ent.Owner);
        _ui.SetUiState(ent.Owner, CorticalBorerUiKey.Key, new CorticalBorerBuiState(mode));
        _ui.TryOpenUi(ent.Owner, CorticalBorerUiKey.Key, ent.Owner);
    }

    private void OnConverseMessage(Entity<CorticalBorerComponent> ent, ref CorticalBorerConverseBuiMessage args)
    {
        if (args.Actor != ent.Owner || ent.Comp.MenuMode != CorticalBorerMenuMode.Converse ||
            ent.Comp.Host is not { } host || ent.Comp.ControllingHost || ent.Comp.SugarSuppressed)
            return;

        if (args.Message?.Trim() is not { Length: > 0 } message)
            return;
        if (message.Length > 200)
            message = message[..200];

        var wrapped = Loc.GetString("cortical-borer-converse-message", ("name", Name(ent)), ("message", message));
        SendPrivateChat(host, wrapped);
        SendPrivateChat(ent.Owner, wrapped);
        _ui.CloseUi(ent.Owner, CorticalBorerUiKey.Key, ent.Owner);
    }

    private void SendPrivateChat(EntityUid recipient, string message)
    {
        if (!TryComp(recipient, out ActorComponent? actor))
            return;

        _chatManager.ChatMessageToOne(ChatChannel.Notifications,
            message,
            FormattedMessage.EscapeText(message),
            default,
            false,
            actor.PlayerSession.Channel,
            colorOverride: Color.FromHex("#b48ead"));
    }

    private void OnPunishMessage(Entity<CorticalBorerComponent> ent, ref CorticalBorerPunishBuiMessage args)
    {
        if (args.Actor != ent.Owner || ent.Comp.MenuMode != CorticalBorerMenuMode.Punish ||
            ent.Comp.Host is not { } host || ent.Comp.ControllingHost ||
            !Enum.IsDefined(args.Punishment) || !TrySpendChemicals(ent, 50f))
            return;

        switch (args.Punishment)
        {
            case CorticalBorerPunishment.Agony:
                var damage = new DamageSpecifier { DamageDict = { ["Blunt"] = 15 } };
                _damage.TryChangeDamage(host, damage, origin: ent.Owner);
                _chat.TryEmoteWithChat(host,
                    "Scream",
                    ignoreActionBlocker: true,
                    forceEmote: true);
                break;
            case CorticalBorerPunishment.Blindness:
                _status.TryAddStatusEffectDuration(host,
                    BlindnessSystem.BlindingStatusEffect,
                    TimeSpan.FromSeconds(25));
                break;
            case CorticalBorerPunishment.Paralysis:
                _stun.TryUpdateParalyzeDuration(host, TimeSpan.FromSeconds(10));
                break;
        }

        _ui.CloseUi(ent.Owner, CorticalBorerUiKey.Key, ent.Owner);
    }

    private void OnInjectMessage(Entity<CorticalBorerComponent> ent, ref CorticalBorerInjectBuiMessage args)
    {
        if (args.Actor != ent.Owner || ent.Comp.MenuMode != CorticalBorerMenuMode.Inject ||
            ent.Comp.Host is not { } host || ent.Comp.ControllingHost)
            return;

        var reagent = args.Injection switch
        {
            CorticalBorerInjection.Epinephrine => "Epinephrine",
            CorticalBorerInjection.SpaceDrugs => "SpaceDrugs",
            CorticalBorerInjection.Tricordrazine => "Tricordrazine",
            CorticalBorerInjection.Ethanol => "Ethanol",
            CorticalBorerInjection.Leporazine => "Leporazine",
            CorticalBorerInjection.Desoxyephedrine => "Desoxyephedrine",
            CorticalBorerInjection.Dexalin => "Dexalin",
            _ => null,
        };
        const float cost = 50f;
        const float dose = 10f;
        if (reagent == null || !CanSpendChemicals(ent, cost))
            return;

        var injected = InjectIntoBloodstream(host, reagent, FixedPoint2.New(dose));
        if (injected <= FixedPoint2.Zero)
        {
            _popup.PopupEntity(Loc.GetString("cortical-borer-inject-failed"), ent, ent);
            return;
        }

        SpendChemicals(ent, cost * (injected.Float() / dose));
        _ui.CloseUi(ent.Owner, CorticalBorerUiKey.Key, ent.Owner);
    }

    private void OnPointAttempt(Entity<CorticalBorerComponent> ent, ref PointAttemptEvent args)
    {
        if (ent.Comp.Host == null)
            args.Cancel();
    }

    private void OnBeforePublicPoint(Entity<CorticalBorerComponent> ent, ref BeforePublicPointEvent args)
    {
        if (args.Handled || ent.Comp.Host is not { } host)
            return;

        args.Handled = true;
        var message = args.Target is { } target
            ? Loc.GetString("cortical-borer-point-entity", ("target", Identity.Entity(target, EntityManager)))
            : Loc.GetString("cortical-borer-point-location");
        SendPrivateChat(host, message);
        SendPrivateChat(ent, message);
    }
}
