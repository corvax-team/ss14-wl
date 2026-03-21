using Content.Shared._WL.Emergency.Prototype;
using Robust.Shared.Prototypes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Content.Server._WL.Emergency.Commponents;

[RegisterComponent]
public sealed partial class EmergencyLevelComponent : Component
{
    [DataField(required: true)]
    public ProtoId<EmergencyListPrototype> EmengercyList;

    [ViewVariables]
    public EmergencyListPrototype? Emengercys;

    [ViewVariables(VVAccess.ReadWrite)]
    public string CurrentEmengercy = string.Empty;

    [ViewVariables]
    public float CurrentDelay = 0;

    [ViewVariables]
    public bool ActiveDelay;
}
