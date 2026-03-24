using Content.Client._WL.Overlays;
using Content.Shared._WL.Photo.Filters;
using Content.Shared.Clothing.Components;
using Robust.Client.Graphics;

namespace Content.Client._WL.Photo.Filters;

public sealed partial class PhotoFilterSystem : EntitySystem
{
    [Dependency] private readonly IOverlayManager _overlay = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PhotoShaderFilterComponent, ComponentInit>(OnShaderFilterInit);
        SubscribeLocalEvent<PhotoShaderFilterComponent, ComponentRemove>(OnShaderFilterShutdown);

        SubscribeLocalEvent<PhotoGhostFilterComponent, ComponentInit>(OnGhostFilterInit);
        SubscribeLocalEvent<PhotoGhostFilterComponent, ComponentRemove>(OnGhostFilterShutdown);

        SubscribeLocalEvent<PhotoFaceFilterComponent, ComponentInit>(OnFaceFilterInit);
        SubscribeLocalEvent<PhotoFaceFilterComponent, ComponentRemove>(OnFaceFilterShutdown);

        SubscribeLocalEvent<PhotoInfoFilterComponent, ComponentInit>(OnInfoFilterInit);
        SubscribeLocalEvent<PhotoInfoFilterComponent, ComponentRemove>(OnInfoFilterShutdown);
    }

    //TODO: Do this more pretty

    //Shader
    private void OnShaderFilterInit(EntityUid uid, PhotoShaderFilterComponent component, ComponentInit args)
    {
        _overlay.AddOverlay(new ShaderCameraOverlay());
    }

    private void OnShaderFilterShutdown(EntityUid uid, PhotoShaderFilterComponent component, ComponentRemove args)
    {
        _overlay.RemoveOverlay<ShaderCameraOverlay>();
    }

    //Ghost
    private void OnGhostFilterInit(EntityUid uid, PhotoGhostFilterComponent component, ComponentInit args)
    {
        _overlay.AddOverlay(new GhostCameraOverlay());
    }

    private void OnGhostFilterShutdown(EntityUid uid, PhotoGhostFilterComponent component, ComponentRemove args)
    {
        _overlay.RemoveOverlay<GhostCameraOverlay>();
    }

    //Face Filter
    private void OnFaceFilterInit(EntityUid uid, PhotoFaceFilterComponent component, ComponentInit args)
    {
        _overlay.AddOverlay(new FaceCameraOverlay());
    }

    private void OnFaceFilterShutdown(EntityUid uid, PhotoFaceFilterComponent component, ComponentRemove args)
    {
        _overlay.RemoveOverlay<FaceCameraOverlay>();
    }

    //Face Filter
    private void OnInfoFilterInit(EntityUid uid, PhotoInfoFilterComponent component, ComponentInit args)
    {
        _overlay.AddOverlay(new InfoCameraOverlay());
    }

    private void OnInfoFilterShutdown(EntityUid uid, PhotoInfoFilterComponent component, ComponentRemove args)
    {
        _overlay.RemoveOverlay<InfoCameraOverlay>();
    }
}
