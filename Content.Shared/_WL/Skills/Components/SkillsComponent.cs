using Content.Shared.Roles;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._WL.Skills.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(SharedSkillsSystem))]
public sealed partial class SkillsComponent : Component
{
    [DataField("skills"), AutoNetworkedField]
    public Dictionary<SkillType, int> Skills = new();

    [DataField("unspentPoints"), AutoNetworkedField]
    public int UnspentPoints;

    [DataField("spentPoints"), AutoNetworkedField]
    public int SpentPoints;

    [DataField("bonusPoints"), AutoNetworkedField]
    public int BonusPoints;

    [AutoNetworkedField]
    public ProtoId<JobPrototype>? CurrentJob = null;
}
