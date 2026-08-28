using Content.Shared.Radio;
using Robust.Shared.Prototypes;

namespace Content.Server._WL.CartridgeLoader.Cartridges;

/// <summary>
///     Marks a PDA program as a NanoChat client. Holds only references - all actual chat
///     data lives on the inserted ID card's <see cref="Content.Shared._WL.NanoChat.NanoChatCardComponent"/>.
/// </summary>
[RegisterComponent, Access(typeof(NanoChatCartridgeSystem))]
public sealed partial class NanoChatCartridgeComponent : Component
{
    public const int MessagePageSize = 50;

    /// <summary>
    ///     The station this cartridge's loader currently belongs to.
    /// </summary>
    [DataField]
    public EntityUid? Station;

    /// <summary>
    ///     The NanoChat card currently inserted into the parent PDA, if any.
    /// </summary>
    [DataField]
    public EntityUid? Card;

    /// <summary>
    ///     The radio channel required to send or receive NanoChat messages.
    /// </summary>
    [DataField]
    public ProtoId<RadioChannelPrototype> RadioChannel = "Common";

    /// <summary>
    ///     Amount of retained history requested by this open client per conversation.
    ///     The card applies its own generous storage cap; BUI transfer is paged separately.
    /// </summary>
    public Dictionary<Content.Shared._WL.NanoChat.NanoChatConversationId, int> LoadedMessageCounts = new();
}
