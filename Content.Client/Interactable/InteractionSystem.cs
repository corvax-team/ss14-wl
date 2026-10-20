using System.Numerics;
using Content.Shared.Buckle.Components; // WL-changes
using Content.Shared.Interaction;
using Content.Shared.Storage;
using Robust.Client.GameObjects;
using Robust.Shared.Containers;

namespace Content.Client.Interactable
{
    // TODO Remove Shared prefix
    public sealed partial class InteractionSystem : SharedInteractionSystem
    {
        [Dependency] private TransformSystem _clientTransform = default!;

        protected override bool TryFaceInteraction(EntityUid user, Vector2 coordinates)
        {
            var faced = base.TryFaceInteraction(user, coordinates);

            if (faced)
            {
                // WL-changes-Start
                var rotationTarget = TryComp<BuckleComponent>(user, out var buckle) && buckle.BuckledTo is { } strap
                    ? strap
                    : user;
                _clientTransform.SnapRenderRotation(rotationTarget);
                // WL-changes-End
            }

            return faced;
        }
    }
}
