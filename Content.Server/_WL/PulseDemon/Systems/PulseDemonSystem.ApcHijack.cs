using Content.Server.Power.Components;
using Content.Server.Power.EntitySystems;
using Content.Server._WL.PulseDemon.Components;
using Content.Shared.Tag;
using Content.Shared.Verbs;

namespace Content.Server._WL.PulseDemon.Systems;

public sealed partial class PulseDemonSystem : EntitySystem
{
    [Dependency] private readonly ApcSystem _apc = default!;
    [Dependency] private readonly TagSystem _tag = default!;

    private const string HijackedDeviceTag = "HijackedDevice";

    private void InitializeHijackedComponent()
    {
        SubscribeLocalEvent<HijackedByPulseDemonComponent, ComponentStartup>(OnHijackedStartup);
        SubscribeLocalEvent<HijackedByPulseDemonComponent, GetVerbsEvent<InteractionVerb>>(OnVerb);
    }

    private void OnHijackedStartup(EntityUid uid, HijackedByPulseDemonComponent comp, ComponentStartup args)
    {
        var tagComp = EnsureComp<TagComponent>(uid);
        _tag.AddTag(tagComp.Owner, HijackedDeviceTag);
    }

    private void OnVerb(EntityUid uid, HijackedByPulseDemonComponent comp, GetVerbsEvent<InteractionVerb> args)
    {
        if (!TryComp<ApcComponent>(uid, out var apcComp) || !HasComp<PulseDemonComponent>(args.User))
            return;

        args.Verbs.Add(new()
        {
            Act = () =>
            {
                _apc.ApcToggleBreaker(uid, apcComp);
                _apc.UpdateApcState(uid, apcComp);
                _apc.UpdateUIState(uid, apcComp);
            },
            Message = Loc.GetString("pulse-demon-hijacked-apc-toggle-breaker"),
            Text = Loc.GetString("pulse-demon-hijacked-apc-toggle-breaker")
        });
    }
}
