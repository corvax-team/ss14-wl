using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._WL.CombatIndicator;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class CombatIndicatorComponent : Component
{
    [DataField]
    public EntProtoId IndicatorProto = "EffectCombatIndicator";

    [DataField]
    public SoundSpecifier? IndicatorSound = new SoundPathSpecifier
        ("/Audio/_WL/Effects/combat_indicator.ogg");

    [DataField, AutoNetworkedField]
    public bool Enabled;

    [DataField, AutoNetworkedField]
    public bool AutoEnable;

    [ViewVariables]
    public EntityUid? IndicatorEntity;
}
