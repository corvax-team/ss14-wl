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
}
