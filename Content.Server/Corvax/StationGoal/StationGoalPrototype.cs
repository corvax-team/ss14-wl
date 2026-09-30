using Content.Shared.Roles;
using Robust.Shared.Prototypes;

namespace Content.Server.Corvax.StationGoal
{
    [Prototype]
    public sealed partial class StationGoalPrototype : IPrototype
    {
        [IdDataFieldAttribute]
        public string ID { get; private set; } = default!;

        [DataField] public string Text { get; set; } = string.Empty;

        [DataField] public float Weight { get; private set; } = 1;

        [DataField]
        public HashSet<ProtoId<DepartmentPrototype>> Department = new();
//
//        [IdDataFieldAttribute]
//        public string ID { get; } = default!;

//        [DataField]
//        public string Text { get; set; } = string.Empty;

//        [DataField]
//        public int? MinPlayers;

//        [DataField]
//        public int? MaxPlayers;

//        /// <summary>
//        /// Goal may require certain items to complete. These items will appear near the receving fax machine at the start of the round.
//        /// TODO: They should be spun up at the tradepost instead of at the fax machine, but I'm too lazy to do that right now. Maybe in the future.
//        /// </summary>
//        [DataField]
//        public List<EntProtoId> Spawns = new();
//
    }
}
