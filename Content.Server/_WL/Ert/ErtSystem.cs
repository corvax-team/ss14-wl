using Content.Server._WL.Ert.Prototypes;
using Content.Server.Shuttles.Components;
using Content.Shared._WL.Entity.Extensions;
using Content.Shared._WL.Ert;
using Content.Shared._WL.Math.Extensions;
using Content.Shared._WL.Random.Extensions;
using JetBrains.Annotations;
using Robust.Server.GameObjects;
using Robust.Shared.EntitySerialization;
using Robust.Shared.EntitySerialization.Systems;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Content.Server._WL.Ert
{
    public sealed partial class ErtSystem : EntitySystem
    {
        [Dependency] private readonly IPrototypeManager _protoMan = default!;
        [Dependency] private readonly MapLoaderSystem _mapLoader = default!;
        [Dependency] private readonly IRobustRandom _random = default!;
        [Dependency] private readonly TransformSystem _transform = default!;
        [Dependency] private readonly EntityLookupSystem _lookup = default!;

        private ErtConfigurationPrototype _config = default!;

        private Dictionary<ErtType, int> _spawned = default!;

        public override void Initialize()
        {
            base.Initialize();

            _spawned = new();

            _config = _protoMan.EnumeratePrototypes<ErtConfigurationPrototype>().FirstOrDefault()!;
        }

        [PublicAPI]
        public bool TrySpawn(
            ErtType ert,
            MapId map,
            [NotNullWhen(true)] out Entity<MapGridComponent>? root,
            MapLoadOptions? opts)
        {
            var path = _config.ShuttlePath(ert);

            if (_mapLoader.TryLoadGrid(map, path, out root, null, opts?.Offset ?? default, opts?.Rotation ?? default))
            {
                if (!_spawned.TryAdd(ert, 1))
                    _spawned[ert] += 1;

                return true;
            }

            return false;
        }

        /// <summary>
        /// Спавнит грид рядом со станцией цк.
        /// </summary>
        /// <param name="ert"></param>
        /// <returns></returns>
        [PublicAPI]
        public bool TrySpawn(
            ErtType ert,
            [NotNullWhen(true)] out Entity<MapGridComponent>? grid,
            Entity<StationCentcommComponent>? concreteCenctom = null)
        {
            grid = null;

            if (concreteCenctom == null)
            {
                var query = EntityQueryEnumerator<StationCentcommComponent>().GetEntities();

                if (query.Count == 0)
                    return false;

                concreteCenctom = _random.Pick(query);
            }

            var mapEntNull = concreteCenctom.Value.Comp.MapEntity;
            var ccEntNull = concreteCenctom.Value.Comp.Entity;
            if (mapEntNull == null || ccEntNull == null)
                return false;

            var mapEnt = mapEntNull.Value;
            var ccEnt = ccEntNull.Value;

            var (coord, angle) = _transform.GetWorldPositionRotation(ccEnt);

            var aabb = _lookup.GetAABBNoContainer(ccEnt, coord, angle);

            var shuttle_offset = _config.ShuttleOffset(ert);

            var x = MathF.Abs(shuttle_offset + aabb.Center.X);
            var y = MathF.Abs(shuttle_offset + aabb.Center.Y);

            var new_box = new Box2(-x, -y, x, y);

            var subtracted = new_box.Subtract(aabb);
            if (subtracted.Count == 0)
                return false;

            var box = _random.Pick(subtracted);
            var result_coord = _random.Next(box);

            var options = new MapLoadOptions()
            {
                Rotation = _random.NextAngle(),
                Offset = result_coord
            };

            var mapId = Comp<MapComponent>(mapEnt);

            return TrySpawn(ert, mapId.MapId, out grid, options);
        }

        public bool IsSpawned(ErtType ert, out int spawned_count)
        {
            if (_spawned.TryGetValue(ert, out spawned_count))
                return spawned_count != 0;

            spawned_count = 0;

            return false;
        }
    }
}
