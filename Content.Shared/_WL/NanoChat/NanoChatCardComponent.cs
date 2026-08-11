using Content.Shared._WL.CartridgeLoader.Cartridges;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared._WL.NanoChat;

/// <summary>
///     Stores NanoChat data on an ID card. This is the single source of truth for a NanoChat
///     identity - the cartridge only keeps a reference to the card that is currently inserted.
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(SharedNanoChatSystem))]
[AutoGenerateComponentPause, AutoGenerateComponentState]
public sealed partial class NanoChatCardComponent : Component
{
    /// <summary>
    ///     The number assigned to this card.
    /// </summary>
    [DataField, AutoNetworkedField]
    public uint? Number;

    /// <summary>
    ///     All chat recipients stored on this card.
    /// </summary>
    [DataField]
    public Dictionary<uint, NanoChatRecipient> Recipients = new();

    /// <summary>
    ///     All messages stored on this card, keyed by recipient number.
    /// </summary>
    [DataField]
    public Dictionary<uint, List<NanoChatMessage>> Messages = new();

    /// <summary>
    ///     The currently selected chat recipient number.
    /// </summary>
    [DataField]
    public uint? CurrentChat;

    /// <summary>
    ///     The maximum amount of recipients this card supports.
    /// </summary>
    [DataField]
    public int MaxRecipients = 50;

    /// <summary>
    ///     The maximum amount of messages retained per conversation.
    /// </summary>
    [DataField]
    public int MaxMessagesPerChat = 100;

    /// <summary>
    ///     The minimum delay between messages sent from this card.
    /// </summary>
    [DataField]
    public TimeSpan MessageSendDelay = TimeSpan.FromSeconds(1);

    /// <summary>
    ///     Last time a message was sent, for rate limiting.
    /// </summary>
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField]
    public TimeSpan LastMessageTime;

    /// <summary>
    ///     Whether to send notifications.
    /// </summary>
    [DataField]
    public bool NotificationsMuted;

    /// <summary>
    ///     The PDA that this card is currently inserted into.
    /// </summary>
    [DataField]
    public EntityUid? PdaUid;

    /// <summary>
    ///     Whether the card's number should be listed in NanoChat's directory search.
    /// </summary>
    [DataField]
    public bool ListNumber = true;

    /// <summary>
    ///     Whether this card may view the station NanoChat directory.
    /// </summary>
    [DataField]
    public bool CanAccessStationDirectory = true;
}
