using Content.Shared.Speech.EntitySystems;
using Robust.Shared.GameStates;

namespace Content.Shared.Speech.Components;

/// Lhotacism
[RegisterComponent, NetworkedComponent]
[Access(typeof(RhotacismSystem))]
public sealed partial class RhotacismComponent : BaseAccentComponent;
