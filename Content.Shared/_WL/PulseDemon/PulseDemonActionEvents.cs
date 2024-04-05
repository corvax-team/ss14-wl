using Content.Shared.Actions;
using Content.Shared.DoAfter;
using JetBrains.Annotations;
using Robust.Shared.Serialization;

namespace Content.Shared._WL.PulseDemon;

/// <summary>
/// It is needed to synchronize the battery of the demon and the currency in the store.
/// </summary>
[Serializable, NetSerializable, DataDefinition]
public sealed partial class PulseDemonShopPurchaseEvent : EntityEventArgs
{
}

[Serializable, NetSerializable, MeansImplicitUse, DataDefinition]
public sealed partial class PulseDemonShopUpgradeEvent : EntityEventArgs
{
    [DataField("characteristicUpgrade")]
    public string? CharacteristicUpgrade;
}

public sealed partial class PulseDemonShopActionEvent : InstantActionEvent { }


public abstract partial class PulseDemonEntityCostActionEvent : EntityTargetActionEvent
{
    [DataField("cost")]
    public float Cost = 0f;
}
public abstract partial class PulseDemonWorldCostActionEvent : WorldTargetActionEvent
{
    [DataField("cost")]
    public float Cost = 0f;
}
public abstract partial class PulseDemonInstantCostActionEvent : InstantActionEvent
{
    [DataField("cost")]
    public float Cost = 0f;
}


//Hide
[Serializable, NetSerializable]
public sealed partial class PulseDemonHideDoAfterEvent : SimpleDoAfterEvent { }

public sealed partial class PulseDemonHideActionEvent : PulseDemonInstantCostActionEvent
{
    /// <summary>
    /// The maximum time a demon can spend in wires
    /// </summary>
    [DataField("hidingTime")]
    public float HidingTime = 180f;
}

//ApcHijack
[Serializable, NetSerializable]
public sealed partial class PulseDemonApcHijackDoAfterEvent : SimpleDoAfterEvent { }
public sealed partial class PulseDemonApcHijackActionEvent : PulseDemonEntityCostActionEvent { }

//Absorption
[Serializable, NetSerializable]
public sealed partial class PulseDemonAbsorptionDoAfterEvent : SimpleDoAfterEvent { }

public sealed partial class PulseDemonAbsorptionActionEvent : PulseDemonEntityCostActionEvent { }

//CableHop
public sealed partial class PulseDemonCableHopActionEvent : PulseDemonWorldCostActionEvent { }

//Self-sustaining
[Serializable, NetSerializable]
public sealed partial class PulseDemonSelfSustainingDoAfterEvent : SimpleDoAfterEvent { }

public sealed partial class PulseDemonSelfSustainingActionEvent : PulseDemonInstantCostActionEvent
{
    /// <summary>
    /// The maximum time a demon can spend outside wires
    /// </summary>
    [DataField("timeOutside")]
    public float TimeOutside = 15f;
}

//Emp
public sealed partial class PulseDemonEmpActionEvent : PulseDemonWorldCostActionEvent
{
    [DataField("radius")]
    public float Radius = 2f;

    [DataField("batteryDamage")]
    public float BatteryDamage = 50000;
}

//Overload
[Serializable, NetSerializable]
public sealed partial class PulseDemonOverloadMachineDoAfterEvent : SimpleDoAfterEvent { }

public sealed partial class PulseDemonOverloadMachineActionEvent : PulseDemonEntityCostActionEvent
{
    [DataField("explosionForce")]
    public float ExplosionForce = 50f;

    [DataField("radius")]
    public float Radius = 1f;

    [DataField("explosionPreparation")]
    public float ExplosionPreparation = 5f;
}

//Electromagnetic Tamper
public sealed partial class PulseDemonElectromagneticTamperActionEvent : PulseDemonEntityCostActionEvent { }
