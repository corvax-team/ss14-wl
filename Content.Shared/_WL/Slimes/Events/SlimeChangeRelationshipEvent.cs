namespace Content.Shared._WL.Slimes.Events
{
    public sealed partial class SlimeChangeRelationshipEvent : EntityEventArgs
    {
        public int PreviousPoints;

        public int NewPoints;

        public readonly EntityUid Slime;

        public readonly EntityUid Target;

        public SlimeChangeRelationshipEvent(EntityUid slime, EntityUid target, int newPoints, int previousPoints)
        {
            Slime = slime;
            PreviousPoints = previousPoints;
            NewPoints = newPoints;
            Target = target;
        }
    }
}
