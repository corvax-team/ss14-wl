using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Shared._WL.Stand_Fall_Crouch
{
    [RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true)]
    public sealed partial class StandFallCrouchComponent : Component
    {
        [DataField(customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
        public string StandFallToggleAction = "ActionToggleFall";

        [DataField, AutoNetworkedField]
        public EntityUid? StandFallToggleActionEntity;

        [/*ViewVariables(VVAccess.ReadWrite), */DataField, AutoNetworkedField]
        public bool IsWantStay = true;

        [DataField/*, AutoNetworkedField/*, ViewVariables(VVAccess.ReadWrite)*/]
        public float WalkModifier = 0.1f;

        [DataField/*, AutoNetworkedField/*, ViewVariables(VVAccess.ReadWrite)*/]
        public float SprintModifier = 0.2f;

        [DataField]
        public float TimeToPutDownSelf = 0.15f;

        [DataField]
        public float TimeToPutDownOther = 2.0f;

        [DataField]
        public float TimeToStandUpSelf = 1.25f;

        [DataField]
        public float TimeToStandUpOther = 3.5f;

        [DataField, AutoNetworkedField]
        public bool IsCrawling = false;
    }
}
