using Content.Shared._WL.Research;
using Content.Shared._WL.Research.Components;
using Content.Shared._WL.Research.Prototypes;
using Content.Shared.Power;
using Robust.Client.GameObjects;

namespace Content.Client._WL.Research;

public sealed partial class ComputerVisualizerSystem : VisualizerSystem<UniversalComputerVisualsComponent>
{
    protected override void OnAppearanceChange(EntityUid uid, UniversalComputerVisualsComponent component, ref AppearanceChangeEvent args)
    {
        AppearanceSystem.TryGetData<bool>(uid, PowerDeviceVisuals.Powered, out var powered, args.Component);
        AppearanceSystem.TryGetData<string>(uid, UniversalComputerVisuals.ProgramPrototype, out var currentProgram, args.Component);

        if (powered
            && currentProgram != null
            && ProtoMan.Resolve<ProgramPrototype>(currentProgram, out var proto))
        {
            SpriteSystem.LayerSetSprite((uid, args.Sprite), 0, proto.Icon);
            args.Sprite?.LayerSetShader(0, "unshaded");
        }
        else
        {
            SpriteSystem.LayerSetRsiState((uid, args.Sprite), 0, "error");
            args.Sprite?.LayerSetShader(0, null, null);
        }
    }
}
