using Content.Shared.Whitelist;

namespace Content.Shared._WL.Research.Components;

[RegisterComponent]
public sealed partial class InsertableModuleComponent : Component
{
    [DataField]
    public LocId? ExamineMessage;

    [DataField]
    public EntityWhitelist? Whitelist;

    [DataField]
    public bool InRightContainer = false;
}
