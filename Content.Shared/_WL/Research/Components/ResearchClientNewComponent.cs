using Robust.Shared.Serialization;

namespace Content.Shared._WL.Research.Components;

[RegisterComponent]
public sealed partial class ResearchClientNewComponent : Component
{
    public bool ConnectedToServer => Server != null;

    /// <summary>
    /// The server the client is connected to
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly)]
    public EntityUid? Server { get; set; }
}

/// <summary>
/// Raised on the client whenever its server is changed
/// </summary>
/// <param name="Server">Its new server</param>
[ByRefEvent]
public readonly record struct ResearchRegistrationNewChangedEvent(EntityUid? Server);

[Serializable, NetSerializable]
public sealed class TerminalServerSelectionMessage : BoundUserInterfaceMessage
{ }
