using Content.Shared._WL.Skills.Systems;
using Content.Shared.Item;
using Robust.Shared.Prototypes;

namespace Content.Shared._WL.Skills.Components.Mechanics
{
    [RegisterComponent]
    [Access(typeof(SharedSkillsSystem))]
    public sealed partial class AthleticSkillComponent : Component
    {
        [DataField("sm")] public float StaminaModifier = 1f;
        [DataField("swm")] public float StaminaWasteValue = 1f;
        [DataField("tfm")] public float ThrowForceModifier = 1f;
        [DataField("trm")] public float ThrowRangeModifier = 1f;
        [DataField("tswm")] public float ThrowStaminaWasteModifier = 1f;
        [DataField("bi")] public ProtoId<ItemSizePrototype>? BlockedItemSize = null;
        [DataField("pwsm")] public float PullWalkSpeedModifier = 1f;
        [DataField("pssm")] public float PullSprintSpeedModifier = 1f;
        [DataField("ptm")] public float PryTimeModifier = 1f;
        [DataField("ctm")] public float ClimbTimeModifier = 1f;
        [DataField("stv")] public float StaminaRegenerateValue = 3f;
    }
}
