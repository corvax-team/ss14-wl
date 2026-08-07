using Content.Shared.Atmos.Components;
using Content.Shared.RCD.Systems;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Physics;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared.RCD.Components;

/// <summary>
/// Main component for the RCD
/// Optionally uses LimitedChargesComponent.
/// Charges can be refilled with RCD ammo
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(RCDSystem))]
public sealed partial class RCDComponent : Component
{
    /// <summary>
    /// List of RCD prototypes that the device comes loaded with
    /// </summary>
    [DataField, AutoNetworkedField]
    public HashSet<ProtoId<RCDPrototype>> AvailablePrototypes { get; set; } = new();

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
    /// Range for interaction, if Range <= 0, range is infinity(max for interaction sistem - 100f(tiles))
    /// </summary>
    [DataField, AutoNetworkedField]
    public float Range = 1.5f;

    [DataField, AutoNetworkedField]
    public bool EnableIgnite = true;

    [DataField, AutoNetworkedField]
    public float IgniteChance = 0.25f;

    [DataField, AutoNetworkedField]
    public TimeSpan IgnitedTime = TimeSpan.FromSeconds(0.5f);

    /// <summary>
    /// rpd port from FunkyStation
    ///
    /// Indicates if a mirrored version of the construction prototype should be used (if available)
    /// </summary>
    [AutoNetworkedField, ViewVariables(VVAccess.ReadOnly)]
    public bool UseMirrorPrototype = false;

    /// <summary>
    /// rpd port from FunkyStation
    ///
    /// Indicates whether this is an RCD or an RPD
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool IsRpd { get; set; } = false;
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

    // WL-Changes-start: rpd port from FunkyStation
    /// <summary>
    /// Stores player rotation
    /// This is a workaround to the fact eye rotation is not currently networked and required for pipe layering
    /// Sent only when needed
    /// </summary>
    [DataField, AutoNetworkedField]
    public float? LastKnownEyeRotation { get; set; } = null;

    /// <summary>
    /// Current pipe layer / build mode for RPD
    /// </summary>
    [DataField, AutoNetworkedField]
    public RpdMode CurrentMode { get; set; } = RpdMode.Free;

    [DataField, AutoNetworkedField, Access(Other = AccessPermissions.ReadWrite)] // WL-Changes
    public AtmosPipeLayer CurrentLayer { get; set; } = AtmosPipeLayer.Primary;

    [DataField]
    public SoundSpecifier SoundSwitchMode { get; set; } = new SoundPathSpecifier("/Audio/Machines/quickbeep.ogg");
}

[Serializable, NetSerializable]
public enum RpdMode : byte
{
    Primary = 0,
    Secondary = 1,
    Tertiary = 2,
    Free = 3,
}
// WL-Changes-end
