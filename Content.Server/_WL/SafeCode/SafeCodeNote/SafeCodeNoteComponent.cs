using Content.Shared._WL.SafeCode.CapsSpareSafe;

namespace Content.Server._WL.SafeCode.SafeCodeNote;

/// <summary>
/// When placed on a paper entity, automatically fills it with the code
/// from a randomly chosen entity that has <see cref="CapsSpareSafeComponent"/>.
/// </summary>
[RegisterComponent]
public sealed partial class SafeCodeNoteComponent : Component
{
    [DataField]
    public LocId NoteText = "safe-code-note-default";
}
