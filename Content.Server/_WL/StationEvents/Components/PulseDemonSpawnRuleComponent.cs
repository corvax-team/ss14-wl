using Content.Server._WL.StationEvents.Events;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Server._WL.StationEvents.Components;


[RegisterComponent, Access(typeof(PulseDemonSpawnRule))]
public sealed partial class PulseDemonSpawnRuleComponent : Component
{
    [DataField("prototype", customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
    public string Prototype = "SpawnPointPulseDemon";
}
