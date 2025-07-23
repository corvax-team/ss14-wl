using Content.Server.Instruments;
using Content.Shared.Mind.Components;
using Content.Shared._WL.GolemCore;
using Content.Shared.Instruments;
using Robust.Shared.Player;

namespace Content.Server._WL.GolemCore;

public sealed class PAISystem : SharedGolemCoreSystem
{
    [Dependency] private readonly InstrumentSystem _instrumentSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<GolemCoreComponent, MindRemovedMessage>(OnMindRemoved);
    }

    private void OnMindRemoved(EntityUid uid, GolemCoreComponent component, MindRemovedMessage args)
    {
        // Mind was removed, shutdown the PAI.
        PAITurningOff(uid);
    }
    public void PAITurningOff(EntityUid uid)
    {
        //  Close the instrument interface if it was open
        //  before closing
        if (HasComp<ActiveInstrumentComponent>(uid) && TryComp<ActorComponent>(uid, out var actor))
        {
            _instrumentSystem.ToggleInstrumentUi(uid, uid);
        }

        //  Stop instrument
        if (TryComp<InstrumentComponent>(uid, out var instrument)) _instrumentSystem.Clean(uid, instrument);
    }
}
