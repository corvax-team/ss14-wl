using Content.Shared.FixedPoint;
using Robust.Shared.Serialization;
using System;
using System.Collections.Generic;
using System.Text;

namespace Content.Shared._WL.Stakc;

[Serializable, NetSerializable]
public sealed class StakcAmountSetValueMessage(int value, NetEntity user) : BoundUserInterfaceMessage
{
    /// <summary>
    /// The new transfer amount.
    /// </summary>
    public int Value = value;

    public NetEntity User = user;
}

[Serializable, NetSerializable]
public enum StakcAmountUiKey
{
    Key,
}

