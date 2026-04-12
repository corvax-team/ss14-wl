using Content.Client._WL.Photo.Filters;
using Content.Client._WL.Photo.UI;
using Content.Shared._WL.Photo;
using Robust.Shared.Graphics;

namespace Content.Client._WL.Photo;

public sealed partial class PhotoSystem : SharedPhotoSystem
{
    [Dependency] private readonly PhotoFilterSystem _filter = default!;

    public Dictionary<PhotoCameraComponent, PhotoCameraBoundUserInterface> ActiveCameras = new();
    public Dictionary<IEye, EntityUid> ActiveEyes = new();

    public override void Initialize()
    {
        base.Initialize();
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        foreach (var (component, window) in ActiveCameras)
        {
            window.UpdateControl(component, frameTime);
        }
    }

    public void OpenCameraUi(EntityUid? uid, PhotoCameraComponent component, PhotoCameraBoundUserInterface window)
    {
        if (!ActiveCameras.ContainsKey(component))
            ActiveCameras.Add(component, window);

        if (EntityManager.TryGetComponent<EyeComponent>(uid, out var eye) && !ActiveEyes.ContainsKey(eye.Eye))
            ActiveEyes.Add(eye.Eye, uid.Value);

        _filter.EnableFilter(uid);
    }

    public void CloseCameraUi(EntityUid? uid, PhotoCameraComponent component)
    {
        if (ActiveCameras.ContainsKey(component))
            ActiveCameras.Remove(component);

        if (EntityManager.TryGetComponent<EyeComponent>(uid, out var eye) && ActiveEyes.ContainsKey(eye.Eye))
            ActiveEyes.Remove(eye.Eye);
    }
}
