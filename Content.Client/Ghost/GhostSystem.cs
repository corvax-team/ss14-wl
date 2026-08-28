using Content.Client.Movement.Systems;
using Content.Shared.Actions;
using Content.Shared.Ghost;
using Content.Shared.NightVision;
using Content.Shared.Overlays;
using Content.Shared._WL.Ghost; // Wl-Changes: Ghost hair
using Content.Shared.Humanoid.Markings; // Wl-Changes: Ghost hair
using Robust.Client.Console;
using Robust.Client.GameObjects;
using Robust.Client.Player;
using Robust.Shared.Player;
using Robust.Shared.Utility; // Wl-Changes: Ghost hair
using Robust.Shared.Prototypes;
using Robust.Client.Graphics; // Wl-Changes: Ghost hair

namespace Content.Client.Ghost
{
    public sealed partial class GhostSystem : SharedGhostSystem
    {
        [Dependency] private IClientConsoleHost _console = default!;
        [Dependency] private IPlayerManager _playerManager = default!;
        [Dependency] private SharedActionsSystem _actions = default!;
        [Dependency] private ContentEyeSystem _contentEye = default!;
        [Dependency] private SpriteSystem _sprite = default!;
        [Dependency] private SharedNightVisionSystem _nv = default!;
        [Dependency] private MarkingManager _marking = default!; // Wl-Changes: Ghost hair
        [Dependency] private IPrototypeManager _prototypeManager = default!; // Wl-Changes: Ghost hair

        public int AvailableGhostRoleCount { get; private set; }

        private bool _ghostVisibility = true;

        private bool GhostVisibility
        {
            get => _ghostVisibility;
            set
            {
                if (_ghostVisibility == value)
                {
                    return;
                }

                _ghostVisibility = value;

                var query = AllEntityQuery<GhostComponent, SpriteComponent>();
                while (query.MoveNext(out var uid, out _, out var sprite))
                {
                    _sprite.SetVisible((uid, sprite), value || uid == _playerManager.LocalEntity);
                }
            }
        }

        public GhostComponent? Player => CompOrNull<GhostComponent>(_playerManager.LocalEntity);
        public bool IsGhost => Player != null;

        public event Action<GhostComponent>? PlayerRemoved;
        public event Action<GhostComponent>? PlayerUpdated;
        public event Action<GhostComponent>? PlayerAttached;
        public event Action? PlayerDetached;
        public event Action<GhostWarpsResponseEvent>? GhostWarpsResponse;
        public event Action<GhostUpdateGhostRoleCountEvent>? GhostRoleCountUpdated;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<GhostComponent, ComponentStartup>(OnStartup);
            SubscribeLocalEvent<GhostComponent, ComponentRemove>(OnGhostRemove);
            SubscribeLocalEvent<GhostComponent, AfterAutoHandleStateEvent>(OnGhostState);

            SubscribeLocalEvent<GhostComponent, LocalPlayerAttachedEvent>(OnGhostPlayerAttach);
            SubscribeLocalEvent<GhostComponent, LocalPlayerDetachedEvent>(OnGhostPlayerDetach);

            SubscribeNetworkEvent<GhostWarpsResponseEvent>(OnGhostWarpsResponse);
            SubscribeNetworkEvent<GhostUpdateGhostRoleCountEvent>(OnUpdateGhostRoleCount);

            SubscribeLocalEvent<EyeComponent, ToggleLightingActionEvent>(OnToggleLighting);
            SubscribeLocalEvent<EyeComponent, ToggleFoVActionEvent>(OnToggleFoV);
            SubscribeLocalEvent<GhostComponent, ToggleGhostsActionEvent>(OnToggleGhosts);

            SubscribeLocalEvent<GhostMarkingsComponent, ComponentStartup>(OnGhostHairStartup); // Wl-Changes: Ghost hair
            SubscribeLocalEvent<GhostMarkingsComponent, AfterAutoHandleStateEvent>(OnGhostHairState); // Wl-Changes: Ghost hair
        }

        private void OnStartup(EntityUid uid, GhostComponent component, ComponentStartup args)
        {
            if (TryComp(uid, out SpriteComponent? sprite))
                _sprite.SetVisible((uid, sprite), GhostVisibility || uid == _playerManager.LocalEntity);
        }

        // WL-Changes-Start: Ghost Hair
        private void OnGhostHairStartup(Entity<GhostMarkingsComponent> ent, ref ComponentStartup args)
        {
            ApplyGhostHair(ent);
        }

        private void OnGhostHairState(Entity<GhostMarkingsComponent> ent, ref AfterAutoHandleStateEvent args)
        {
            ApplyGhostHair(ent);
        }

        private void ApplyGhostHair(Entity<GhostMarkingsComponent> ent)
        {
            if (!TryComp<SpriteComponent>(ent, out var sprite))
                return;

            if (!TryComp<GhostComponent>(ent, out var ghost))
                return;

            foreach (var marking in ent.Comp.Markings)
            {
                if (!_marking.TryGetMarking(marking, out var proto))
                    continue;

                for (var i = 0; i < proto.Sprites.Count; i++)
                {
                    if (proto.Sprites[i] is not SpriteSpecifier.Rsi rsi)
                        continue;

                    var layerId = $"{proto.ID}-{rsi.RsiState}";

                    if (!_sprite.LayerMapTryGet((ent, sprite), layerId, out _, false))
                    {
                        var layerIndex = _sprite.AddLayer((ent, sprite), rsi);
                        _sprite.LayerMapSet((ent, sprite), layerId, layerIndex);

                        sprite.LayerSetShader(layerIndex, "unshaded");
                    }

                    _sprite.LayerSetSprite((ent, sprite), layerId, rsi);

                    if (marking.MarkingColors != null && i < marking.MarkingColors.Count)
                    {
                        _sprite.LayerSetColor((ent, sprite),
                            layerId,
                            new Color(marking.MarkingColors[i].R, marking.MarkingColors[i].G, marking.MarkingColors[i].B, marking.MarkingColors[i].A * ent.Comp.Alpha));
                    }
                }
            }
        }
        // WL-Changes-End: Ghost Hair

        private void OnToggleLighting(EntityUid uid, EyeComponent component, ToggleLightingActionEvent args)
        {
            if (args.Handled)
                return;

            if (!component.DrawLight)
            {
                // normal lighting
                Popup.PopupEntity(Loc.GetString("ghost-gui-toggle-lighting-manager-popup-normal"), args.Performer);
                _contentEye.RequestEye(component.DrawFov, true);
            }
            else if (TryComp<NightVisionComponent>(uid, out var nv) && !nv.Enabled)
            {
                Popup.PopupEntity(Loc.GetString("ghost-gui-toggle-lighting-manager-popup-half-bright"), args.Performer);
                _nv.SetEnabled((uid, nv), true);
            }
            else
            {
                // fullbright mode
                Popup.PopupEntity(Loc.GetString("ghost-gui-toggle-lighting-manager-popup-fullbright"), args.Performer);
                _contentEye.RequestEye(component.DrawFov, false);
                _nv.SetEnabled((uid, nv), false);
            }

            args.Handled = true;
        }

        private void OnToggleFoV(EntityUid uid, EyeComponent component, ToggleFoVActionEvent args)
        {
            if (args.Handled)
                return;

            Popup.PopupEntity(Loc.GetString("ghost-gui-toggle-fov-popup"), args.Performer);
            _contentEye.RequestToggleFov(uid, component);
            args.Handled = true;
        }

        private void OnToggleGhosts(EntityUid uid, GhostComponent component, ToggleGhostsActionEvent args)
        {
            if (args.Handled)
                return;

            var locId = GhostVisibility ? "ghost-gui-toggle-ghost-visibility-popup-off" : "ghost-gui-toggle-ghost-visibility-popup-on";
            Popup.PopupEntity(Loc.GetString(locId), args.Performer);
            if (uid == _playerManager.LocalEntity)
                ToggleGhostVisibility();

            args.Handled = true;
        }

        private void OnGhostRemove(EntityUid uid, GhostComponent component, ComponentRemove args)
        {
            _actions.RemoveAction(uid, component.ToggleLightingActionEntity);
            _actions.RemoveAction(uid, component.ToggleFoVActionEntity);
            _actions.RemoveAction(uid, component.ToggleGhostsActionEntity);
            _actions.RemoveAction(uid, component.ToggleGhostHearingActionEntity);

            if (uid != _playerManager.LocalEntity)
                return;

            GhostVisibility = false;
            PlayerRemoved?.Invoke(component);
        }

        private void OnGhostPlayerAttach(EntityUid uid, GhostComponent component, LocalPlayerAttachedEvent localPlayerAttachedEvent)
        {
            GhostVisibility = true;
            PlayerAttached?.Invoke(component);
        }

        private void OnGhostState(EntityUid uid, GhostComponent component, ref AfterAutoHandleStateEvent args)
        {
            if (TryComp<SpriteComponent>(uid, out var sprite))
                _sprite.LayerSetColor((uid, sprite), 0, component.Color);

            if (uid != _playerManager.LocalEntity)
                return;

            PlayerUpdated?.Invoke(component);
        }

        private void OnGhostPlayerDetach(EntityUid uid, GhostComponent component, LocalPlayerDetachedEvent args)
        {
            GhostVisibility = false;
            PlayerDetached?.Invoke();
        }

        private void OnGhostWarpsResponse(GhostWarpsResponseEvent msg)
        {
            if (!IsGhost)
            {
                return;
            }

            GhostWarpsResponse?.Invoke(msg);
        }

        private void OnUpdateGhostRoleCount(GhostUpdateGhostRoleCountEvent msg)
        {
            AvailableGhostRoleCount = msg.AvailableGhostRoles;
            GhostRoleCountUpdated?.Invoke(msg);
        }

        public void RequestWarps()
        {
            RaiseNetworkEvent(new GhostWarpsRequestEvent());
        }

        public void ReturnToBody()
        {
            var msg = new GhostReturnToBodyRequest();
            RaiseNetworkEvent(msg);
        }

        public void OpenGhostRoles()
        {
            _console.RemoteExecuteCommand(null, "ghostroles");
        }

        public void ToggleGhostVisibility(bool? visibility = null)
        {
            GhostVisibility = visibility ?? !GhostVisibility;
        }
    }
}
