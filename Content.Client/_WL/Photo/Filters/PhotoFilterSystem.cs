using Content.Client._WL.Overlays;
using Content.Shared._WL.Photo;
using Content.Shared._WL.Photo.Filters;
using Robust.Client.Graphics;
using Robust.Client.Player;

namespace Content.Client._WL.Photo.Filters;
public sealed partial class PhotoFilterSystem : EntitySystem
{
    [Dependency] private readonly IOverlayManager _overlay = default!;
    [Dependency] private readonly IPlayerManager _player = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PhotoFilterBaseComponent, ComponentInit>(OnFilterInit);
        SubscribeLocalEvent<PhotoFilterBaseComponent, ComponentShutdown>(OnFilterShutdown);

        SubscribeLocalEvent<PhotoShaderFilterComponent, TogglePhotoFilterEvent>(OnToggleShaderFilter);
        SubscribeLocalEvent<PhotoGhostFilterComponent, TogglePhotoFilterEvent>(OnToggleGhostFilter);
        SubscribeLocalEvent<PhotoFaceFilterComponent, TogglePhotoFilterEvent>(OnToggleFaceFilter);
        SubscribeLocalEvent<PhotoInfoFilterComponent, TogglePhotoFilterEvent>(OnToggleInfoFilter);
    }

    private void OnFilterInit(EntityUid uid, PhotoFilterBaseComponent component, ComponentInit args)
    {
        EnableFilter(uid);
    }

    private void OnFilterShutdown(EntityUid uid, PhotoFilterBaseComponent component, ComponentShutdown args)
    {
        DisableFilter(uid);
    }

    public void EnableFilter(EntityUid? uid)
    {
        if (uid == null || !CheckOverlay(uid.Value))
            return;

        var ev = new TogglePhotoFilterEvent(true);
        RaiseLocalEvent(uid.Value, ev);
    }

    public void DisableFilter(EntityUid? uid)
    {
        if (uid == null || !CheckOverlay(uid.Value))
            return;

        var ev = new TogglePhotoFilterEvent(false);
        RaiseLocalEvent(uid.Value, ev);
    }

    private bool CheckOverlay(EntityUid uid)
    {
        if (!EntityManager.TryGetComponent<PhotoCameraComponent>(uid, out var camera))
            return false;

        if (_player.LocalEntity != camera.User)
            return false;

        if (!EntityManager.TryGetComponent<PhotoFilterBaseComponent>(uid, out var filter) ||
            filter.LifeStage >= ComponentLifeStage.Stopping)
            return true;

        return true;
    }

    //Different Filters handle

    //Simple Shader Filter
    private void OnToggleShaderFilter(EntityUid uid, PhotoShaderFilterComponent component, TogglePhotoFilterEvent args)
    {
        if (args.State)
            _overlay.AddOverlay(new ShaderCameraOverlay());
        else
            _overlay.RemoveOverlay<ShaderCameraOverlay>();
    }

    //Ghost Filter
    private void OnToggleGhostFilter(EntityUid uid, PhotoGhostFilterComponent component, TogglePhotoFilterEvent args)
    {
        if (args.State)
            _overlay.AddOverlay(new GhostCameraOverlay());
        else
            _overlay.RemoveOverlay<GhostCameraOverlay>();
    }

    //Face Filter
    private void OnToggleFaceFilter(EntityUid uid, PhotoFaceFilterComponent component, TogglePhotoFilterEvent args)
    {
        if (args.State)
            _overlay.AddOverlay(new FaceCameraOverlay());
        else
            _overlay.RemoveOverlay<FaceCameraOverlay>();
    }

    //Info Filter
    private void OnToggleInfoFilter(EntityUid uid, PhotoInfoFilterComponent component, TogglePhotoFilterEvent args)
    {
        if (args.State)
            _overlay.AddOverlay(new InfoCameraOverlay());
        else
            _overlay.RemoveOverlay<InfoCameraOverlay>();
    }
}

public record struct TogglePhotoFilterEvent(bool State)
{
    public bool State = State;
}

