using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._WL.CombatIndicator;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true, true)]
public sealed partial class CombatIndicatorComponent : Component
{
    [DataField, AutoNetworkedField]
    public EntProtoId IndicatorProto = "EffectCombatIndicator";

    [DataField]
    public SoundSpecifier? IndicatorSound = new SoundPathSpecifier
        ("/Audio/_WL/Effects/combat_indicator.ogg");

    [ViewVariables]
    public EntityUid? IndicatorEntity;
}
