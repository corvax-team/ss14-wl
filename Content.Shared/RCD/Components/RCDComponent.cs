using Content.Shared.RCD.Systems;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Physics;
using Robust.Shared.Prototypes;

namespace Content.Shared.RCD.Components;

/// <summary>
/// Main component for the RCD
/// Optionally uses LimitedChargesComponent.
/// Charges can be refilled with RCD ammo
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(RCDSystem), Other = AccessPermissions.ReadWrite)]
public sealed partial class RCDComponent : Component
{
    /// <summary>
    /// List of RCD prototypes that the device comes loaded with
    /// </summary>
    [DataField, AutoNetworkedField]
    public HashSet<ProtoId<RCDPrototype>> AvailablePrototypes { get; set; } = new();

    [DataField, AutoNetworkedField]
    public HashSet<EntProtoId> AvaliableToDeconstructEntity = new();

    /// <summary>
    /// Sound that plays when a RCD operation successfully completes
    /// </summary>
    [DataField]
    public SoundSpecifier SuccessSound { get; set; } = new SoundPathSpecifier("/Audio/Items/deconstruct.ogg");

    /// <summary>
    /// The ProtoId of the currently selected RCD prototype
    /// </summary>
    [DataField, AutoNetworkedField]
    public ProtoId<RCDPrototype> ProtoId { get; set; } = "Invalid";

    // WL-Changes-start
    /// <summary>
    /// WL-Changes: RPD pipe layers
    ///
    /// ProtoId of current prototype for AlignAtmosPipeLayers.
    /// If null RCDSystem used field Prototype from RCDPrototype
    /// </summary>
    [DataField, AutoNetworkedField, Access(Other = AccessPermissions.ReadWrite)]
    public string? OverrideProtoId;

    /// <summary>
    /// Wl-Changes: RPD
    ///
    /// Range for interaction, if Range <= 0, range is infinity(max for interaction sistem - 100f(tiles))
    /// </summary>
    [DataField, AutoNetworkedField]
    public float Range = 1.5f;

    /// <summary>
    /// RPD port from Goob-Station
    ///
    /// Indicates if a mirrored version of the construction prototype should be used (if available)
    /// </summary>
    [AutoNetworkedField, ViewVariables(VVAccess.ReadOnly)]
    public bool UseMirrorPrototype = false;

    /// <summary>
    /// RPD port from Goob-Station
    ///
    /// Indicates whether this is an RCD or an RPD
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool IsRpd = false;

    [DataField, AutoNetworkedField]
    public bool EnableIgnite = true;

    [DataField, AutoNetworkedField]
    public float IgniteChance = 0.25f;

    [DataField, AutoNetworkedField]
    public TimeSpan IgnitedTime = TimeSpan.FromSeconds(0.5);
    // WL-Changes-end

    /// <summary>
    /// The direction constructed entities will face upon spawning
    /// </summary>
    [DataField, AutoNetworkedField]
    public Direction ConstructionDirection
    {
        get => _constructionDirection;
        set
        {
            _constructionDirection = value;
            ConstructionTransform = new Transform(new(), _constructionDirection.ToAngle());
        }
    }

    private Direction _constructionDirection = Direction.South;

    /// <summary>
    /// Returns a rotated transform based on the specified ConstructionDirection
    /// </summary>
    /// <remarks>
    /// Contains no position data
    /// </remarks>
    [ViewVariables(VVAccess.ReadOnly)]
    public Transform ConstructionTransform { get; private set; }
}
