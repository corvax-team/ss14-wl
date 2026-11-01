using Content.Client.Graphics;
using Content.Shared._WL.Implants;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Shared.Prototypes;

namespace Content.Client._WL.Implants;

public sealed partial class ImitatorDisguiseSystem : EntitySystem
{
    [Dependency] private SpriteSystem _sprite = default!;
    private const string ShaderId = "imitator-glitch";
    private static readonly ProtoId<ShaderPrototype> Shader = "ImitatorGlitch";
    private readonly Dictionary<EntityUid, ShaderInstance> _shaders = new();

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ImitatorDisguiseComponent, BeforePostShaderRenderEvent>(OnRender);
    }

    public void RefreshShader(Entity<ImitatorDisguiseComponent> ent)
    {
        if (!TryComp<SpriteComponent>(ent, out var sprite))
            return;
        if (ent.Comp.Instability <= 0)
        {
            _sprite.RemovePostShader((ent, sprite), ShaderId);
            return;
        }
        if (!_shaders.TryGetValue(ent, out var shader))
        {
            shader = ProtoMan.Index(Shader).InstanceUnique();
            _shaders.Add(ent, shader);
        }
        _sprite.SetPostShader((ent, sprite), new SpriteComponent.PostShaderArgs(ShaderId, shader)
        {
            RaiseShaderEvent = true,
            Before = ContentPostShaderIds.BeforeOutlines,
        });
    }

    private void OnRender(Entity<ImitatorDisguiseComponent> ent, ref BeforePostShaderRenderEvent args)
    {
        if (args.Id == ShaderId)
            args.Shader.SetParameter("strength", ent.Comp.Instability);
    }

    public void ClearShader(EntityUid ent)
    {
        if (TryComp<SpriteComponent>(ent, out var sprite))
            _sprite.RemovePostShader((ent, sprite), ShaderId);
        if (_shaders.Remove(ent, out var shader))
            shader.Dispose();
    }
}
