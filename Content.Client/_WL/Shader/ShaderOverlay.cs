using System.Numerics;
using Content.Shared.CCVar;
using Content.Shared._WL.Shaders.Components;
using Robust.Client.Graphics;
using Robust.Client.UserInterface.CustomControls;
using Robust.Shared.Configuration;
using Robust.Shared.Enums;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Client._WL.Shaders
{
    public sealed partial class ShadersOverlay : Overlay, IEntityEventSubscriber
    {
        [Dependency] private IEntityManager _entMan = default!;
        [Dependency] private IPrototypeManager _prototypeManager = default!;
        [Dependency] private IConfigurationManager _configManager = default!;
        [Dependency] private IGameTiming _timing = default!;
        private SharedTransformSystem? _xformSystem = null;

        /// <summary>
        ///     Maximum number of distortions that can be shown on screen at a time.
        ///     If this value is changed, the shader itself also needs to be updated.
        /// </summary>
        public const int MaxCount = 5;

        private const float MaxDistance = 20f;

        public override OverlaySpace Space => OverlaySpace.WorldSpace;
        public override bool RequestScreenTexture => true;

        private List<ShaderInstance>? _shaders;

        private bool _reducedMotion;

        public ShadersOverlay()
        {
            IoCManager.InjectDependencies(this);
            ZIndex = 101; // Should be drawn after the placement overlay so admins placing items near the singularity can tell where they're going.

            _configManager.OnValueChanged(CCVars.ReducedMotion, (b) => { _reducedMotion = b; }, invokeImmediately: true);
        }

        private readonly Vector2[] _positions = new Vector2[MaxCount];
        private readonly float[][] _vars = new float[MaxCount][];
        private readonly TimeSpan[] _times = new TimeSpan[MaxCount];
        private int _count = 0;

        protected override bool BeforeDraw(in OverlayDrawArgs args)
        {
            if (_reducedMotion)
                return false;
            if (args.Viewport.Eye == null)
                return false;
            if (_xformSystem is null && !_entMan.TrySystem(out _xformSystem))
                return false;

            _count = 0;
            var query = _entMan.EntityQueryEnumerator<ShaderSourceComponent, TransformComponent>();
            _shaders = new List<ShaderInstance>();
            while (query.MoveNext(out var uid, out var source, out var xform))
            {
                if (xform.MapID != args.MapId)
                    continue;

                if (!_prototypeManager.TryIndex<ShaderPrototype>(source.Shader, out var Shader))
                    continue;

                _shaders.Add(Shader.Instance().Duplicate());

                var mapPos = _xformSystem.GetWorldPosition(uid);

                // is the distortion in range?
                if ((mapPos - args.WorldAABB.ClosestPoint(mapPos)).LengthSquared() > MaxDistance * MaxDistance)
                    continue;

                // To be clear, this needs to use "inside-viewport" pixels.
                // In other words, specifically NOT IViewportControl.WorldToScreen (which uses outer coordinates).
                var tempCoords = args.Viewport.WorldToLocal(mapPos);
                tempCoords.Y = args.Viewport.Size.Y - tempCoords.Y; // Local space to fragment space.

                _positions[_count] = tempCoords;
                _vars[_count] = source.Vars.ToArray();
                _times[_count] = source.CreationTick.Value*_timing.TickPeriod;
                _count++;

                if (_count == MaxCount)
                    break;
            }

            return (_count > 0);
        }

        protected override void Draw(in OverlayDrawArgs args)
        {
            if (ScreenTexture == null || args.Viewport.Eye == null)
                return;

            _count = 0;
            if (_shaders is null)
                return;

            var worldHandle = args.WorldHandle;

            foreach (ShaderInstance shader in _shaders)
            {
                shader?.SetParameter("maxDistance", MaxDistance * EyeManager.PixelsPerMeter);
                shader?.SetParameter("renderScale", args.Viewport.RenderScale * args.Viewport.Eye.Scale);
                shader?.SetParameter("position", _positions[_count]);

                shader?.SetParameter("vars", _vars[_count]);

                shader?.SetParameter("SCREEN_TEXTURE", ScreenTexture);

                shader?.SetParameter("curTime", (float)((_timing.CurTime - _times[_count]).TotalMilliseconds));

                worldHandle.UseShader(shader);
                _count++;

                worldHandle.DrawRect(args.WorldAABB, Color.White);
            }
            worldHandle.UseShader(null);
        }
    }
}
