using Content.Shared.Roles;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._WL.Skills.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(SharedSkillsSystem))]
public sealed partial class SkillsComponent : Component
{
    [DataField, AutoNetworkedField]
    public Dictionary<SkillType, int> Skills = new();

    [DataField, AutoNetworkedField]
    public int UnspentPoints;

    [DataField, AutoNetworkedField]
    public int SpentPoints;

    [DataField, AutoNetworkedField]
    public int BonusPoints;

    [AutoNetworkedField]
    public ProtoId<JobPrototype>? CurrentJob = null;
}
