using Robust.Server.GameObjects;
using Robust.Shared.Map.Components;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;
using System.Linq;
using System.Numerics;

namespace Content.Server._WL.DayNight
{
    public sealed partial class DayNightSystem : EntitySystem
    {
        [Dependency] private readonly IGameTiming _gameTime = default!;
        [Dependency] private readonly IPrototypeManager _protoMan = default!;
        [Dependency] private readonly MapSystem _mapSys = default!;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<DayNightComponent, MapInitEvent>(OnMapInit, after: [typeof(SharedMapSystem)]);
        }

        public override void Update(float frameTime)
        {
            base.Update(frameTime);

            var query = EntityQueryEnumerator<DayNightComponent>();
            while (query.MoveNext(out var map, out var dayNightComp))
            {
                if (!TryComp<MapLightComponent>(map, out var mapLightComp))
                    continue;

                if (!TryComp<MapComponent>(map, out var mapComponent))
                    continue;

                if (!dayNightComp.WasInit || mapComponent.MapPaused)
                    continue;

                if (_gameTime.CurTime >= dayNightComp.NextCycle)
                    dayNightComp.NextCycle += dayNightComp.FullCycle;

                var color = CalculateColor(
                    _gameTime.CurTime,
                    dayNightComp.FullCycle,
                    dayNightComp.NextCycle,
                    Color.FromHex(dayNightComp.DayHex),
                    Color.FromHex(dayNightComp.NightHex),
                    dayNightComp.DayNightRatio);

                if (color == mapLightComp.AmbientLightColor) //Оптимизация для случаев, если цикл дня и ночи огромен.
                    continue;

                _mapSys.SetAmbientLight(mapComponent.MapId, color);
            }
        }

        private void OnMapInit(EntityUid station, DayNightComponent comp, MapInitEvent args)
        {
            if (!TryComp<MapComponent>(station, out var mapComponent))
                return;

            _mapSys.SetAmbientLight(mapComponent.MapId, Color.FromHex(comp.DayHex));
            comp.NextCycle = _gameTime.CurTime + comp.FullCycle;
            comp.WasInit = true;
        }

        public static Color CalculateColor(TimeSpan currentTime, TimeSpan fullCycle, TimeSpan nextCycle, Color dayColor, Color nightColor, Vector2 dayNightRatio)
        {
            currentTime = currentTime - (nextCycle - fullCycle);

            var pair = dayNightRatio.X + dayNightRatio.Y;

            var dayTime = fullCycle.TotalMinutes / pair * dayNightRatio.X;
            var nightTime = fullCycle.TotalMinutes / pair * dayNightRatio.Y;

            var isDay = currentTime.TotalMinutes <= dayTime;

            var filledPercentage = isDay
                ? currentTime.TotalMinutes / dayTime
                : (currentTime.TotalMinutes - dayTime) / nightTime;

            var r = isDay
                ? dayColor.R + (nightColor.R - dayColor.R) * filledPercentage
                : nightColor.R + (dayColor.R - nightColor.R) * filledPercentage;
            var g = isDay
                ? dayColor.G + (nightColor.G - dayColor.G) * filledPercentage
                : nightColor.G + (dayColor.G - nightColor.G) * filledPercentage;
            var b = isDay
                ? dayColor.B + (nightColor.B - dayColor.B) * filledPercentage
                : nightColor.B + (dayColor.B - nightColor.B) * filledPercentage;

            var result = new Color((float) r, (float) g, (float) b);

            return result;
        }
    }
}
