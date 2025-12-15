// WL-Changes-start: Alert Level Rework
using Content.Shared.AlertLevel;
using Robust.Shared.Prototypes;
// WL-Changes-end

namespace Content.Server.AlertLevel;

/// <summary>
/// Alert level component. This is the component given to a station to
/// signify its alert level state.
/// </summary>
[RegisterComponent]
public sealed partial class AlertLevelComponent : Component
{
    /// <summary>
    /// The current set of alert levels on the station.
    /// </summary>
    [ViewVariables]
    // WL-Changes-start: Alert Level Rework
    public AlertLevelsListPrototype? AlertLevels;

    // Once stations are a prototype, this should be used.
    [DataField(required: true)]
    public ProtoId<AlertLevelsListPrototype> AlertLevelsListPrototype;
    // WL-Changes-end

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
            var prototypeManager = IoCManager.Resolve<IPrototypeManager>();
            if (AlertLevels == null
            // WL-Changes-start: Alert Level Rework
                || !prototypeManager.TryIndex<AlertLevelPrototype>(CurrentLevel, out var level)) // TryGetValue -> TryIndex
                return false;
            // WL-Changes-end

            return level.Selectable && !level.DisableSelection && !IsLevelLocked;
        }
    }
}
