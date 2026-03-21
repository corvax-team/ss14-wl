using Content.Shared._WL.Emergency.Prototype;
using Robust.Shared.Prototypes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Content.Server._WL.Emergency.Components;

[RegisterComponent]
public sealed partial class EmergencyLevelComponent : Component
{
    [DataField(required: true)]
    public ProtoId<EmergencyListPrototype> emergencyList;

    [ViewVariables]
    public EmergencyListPrototype? Emergencies;

    [ViewVariables(VVAccess.ReadWrite)]
    public string CurrentEmergency = string.Empty;

    [ViewVariables]
    public float CurrentDelay = 0;

    [ViewVariables]
    public bool ActiveDelay;
}
