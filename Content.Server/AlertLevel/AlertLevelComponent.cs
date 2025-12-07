using Content.Shared.AlertLevel;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Server.AlertLevel;

/// <summary>
/// Alert level component. This is the component given to a station to
/// signify its alert level state.
/// </summary>
[RegisterComponent]
public sealed partial class AlertLevelComponent : Component
{
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    /// <summary>
    /// The current set of alert levels on the station.
    /// </summary>
    [ViewVariables]
    public AlertLevelsListPrototype? AlertLevels;

    // Once stations are a prototype, this should be used.
    [DataField(required: true, customTypeSerializer: typeof(PrototypeIdSerializer<AlertLevelsListPrototype>))]
    public string AlertLevelsListPrototype = default!;

    /// <summary>
    /// The current level on the station.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)] public string CurrentLevel = string.Empty;

    /// <summary>
    /// Is current station level can be changed by crew.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)] public bool IsLevelLocked = false;

    [ViewVariables] public float CurrentDelay = 0;
    [ViewVariables] public bool ActiveDelay;

    /// <summary>
    /// If the level can be selected on the station.
    /// </summary>
    [ViewVariables]
    public bool IsSelectable
    {
        get
        {
            if (AlertLevels == null
                || !_prototypeManager.TryIndex<AlertLevelPrototype>(CurrentLevel, out var prototype))
                return false;

            return prototype.Selectable && !prototype.DisableSelection && !IsLevelLocked;
        }
    }
}
