using Content.Shared.DoAfter;
using Robust.Shared.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Content.Shared._WL.StandFallCrouch
{
    [Serializable, NetSerializable]
    public sealed partial class StandFallDoAfterEvent : SimpleDoAfterEvent
    {
    }
}
