namespace Content.Shared._WL.Economics.Components
{
    [RegisterComponent]
    public sealed partial class StationTypeComponent : Component
    {
        [DataField(required: true)]
        public StationType StationType;
    }
}
