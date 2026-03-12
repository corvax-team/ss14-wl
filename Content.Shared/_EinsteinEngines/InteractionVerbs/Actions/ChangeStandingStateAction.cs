using Content.Shared.InteractionVerbs;
using Content.Shared.Standing;

namespace Content.Shared.InteractionVerbs.Actions;

[Serializable]
public sealed partial class ChangeStandingStateAction : InteractionAction
{
    [DataField]
    public bool MakeStanding, MakeLaying;

    public override bool CanPerform(InteractionArgs args, InteractionVerbPrototype proto, bool isBefore, VerbDependencies deps)
    {
        if (!deps.EntMan.TryGetComponent<StandingStateComponent>(args.Target, out var state))
            return false;

        return state.Standing && MakeLaying
               || !state.Standing && MakeStanding;
    }

    public override bool Perform(InteractionArgs args, InteractionVerbPrototype proto, VerbDependencies deps)
    {
        var stateSystem = deps.EntMan.System<StandingStateSystem>();

        if (!deps.EntMan.TryGetComponent<StandingStateComponent>(args.Target, out var state))
            return false;

        return state.Standing switch
        {
            // Note: these will get cancelled if the target is forced to stand/lay, e.g. due to a buckle or stun or something else.
            false when MakeStanding => stateSystem.Stand(args.Target),
            true when MakeLaying => stateSystem.Down(args.Target),
            _ => false
        };
    }
}
