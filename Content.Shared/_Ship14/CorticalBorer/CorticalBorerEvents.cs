using Content.Shared.Actions;
using Content.Shared.DoAfter;
using Robust.Shared.Serialization;
using Robust.Shared.GameObjects;

namespace Content.Shared._Ship14.CorticalBorer;

[ByRefEvent]
public sealed partial class CorticalBorerParalyzeActionEvent : InstantActionEvent;

[ByRefEvent]
public sealed partial class CorticalBorerInfestActionEvent : EntityTargetActionEvent;

[ByRefEvent]
public sealed partial class CorticalBorerConverseActionEvent : InstantActionEvent;

[ByRefEvent]
public sealed partial class CorticalBorerPunishActionEvent : InstantActionEvent;

[ByRefEvent]
public sealed partial class CorticalBorerInjectActionEvent : InstantActionEvent;

[ByRefEvent]
public sealed partial class CorticalBorerScreechActionEvent : InstantActionEvent;

[ByRefEvent]
public sealed partial class CorticalBorerReproduceActionEvent : InstantActionEvent;

[ByRefEvent]
public sealed partial class CorticalBorerControlActionEvent : InstantActionEvent;

[ByRefEvent]
public sealed partial class CorticalBorerLeaveActionEvent : InstantActionEvent;

[ByRefEvent]
public sealed partial class CorticalBorerReleaseActionEvent : InstantActionEvent;

[ByRefEvent]
public sealed partial class CorticalBorerResistActionEvent : InstantActionEvent;

[Serializable, NetSerializable]
public sealed partial class CorticalBorerInfestDoAfterEvent : SimpleDoAfterEvent;

[Serializable, NetSerializable]
public sealed partial class CorticalBorerVentDoAfterEvent : SimpleDoAfterEvent;

[Serializable, NetSerializable]
public sealed partial class CorticalBorerLeaveDoAfterEvent : SimpleDoAfterEvent;

[Serializable, NetSerializable]
public sealed partial class CorticalBorerControlDoAfterEvent : SimpleDoAfterEvent;

[Serializable, NetSerializable]
public sealed partial class CorticalBorerResistDoAfterEvent : SimpleDoAfterEvent;

[Serializable, NetSerializable]
public enum CorticalBorerPunishment : byte
{
    Agony,
    Blindness,
    Paralysis,
}

[Serializable, NetSerializable]
public enum CorticalBorerInjection : byte
{
    Epinephrine,
    SpaceDrugs,
    Tricordrazine,
    Ethanol,
    Leporazine,
    Desoxyephedrine,
    Dexalin,
}

[Serializable, NetSerializable]
public enum CorticalBorerUiKey : byte
{
    Key,
}

[Serializable, NetSerializable]
public enum CorticalBorerMenuMode : byte
{
    Converse,
    Punish,
    Inject,
}

[Serializable, NetSerializable]
public sealed class CorticalBorerBuiState(CorticalBorerMenuMode mode) : BoundUserInterfaceState
{
    public CorticalBorerMenuMode Mode = mode;
}

[Serializable, NetSerializable]
public sealed class CorticalBorerPunishBuiMessage(CorticalBorerPunishment punishment) : BoundUserInterfaceMessage
{
    public CorticalBorerPunishment Punishment = punishment;
}

[Serializable, NetSerializable]
public sealed class CorticalBorerInjectBuiMessage(CorticalBorerInjection injection) : BoundUserInterfaceMessage
{
    public CorticalBorerInjection Injection = injection;
}

[Serializable, NetSerializable]
public sealed class CorticalBorerConverseBuiMessage(string message) : BoundUserInterfaceMessage
{
    public string Message = message;
}
