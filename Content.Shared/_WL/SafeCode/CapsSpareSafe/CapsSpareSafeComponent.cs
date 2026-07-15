namespace Content.Shared._WL.SafeCode.CapsSpareSafe;

/// <summary>
/// Marks this entity as a "spare safe" that can be referenced by SafeCodeNoteComponent.
/// The entity must also have a <see cref="SafeCodeComponent"/>.
/// </summary>

[RegisterComponent]
public sealed partial class CapsSpareSafeComponent : Component
{

}
