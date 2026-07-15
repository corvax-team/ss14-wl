using Robust.Shared.GameStates;

namespace Content.Shared._WL.Shaders.Components
{
    [RegisterComponent, NetworkedComponent]
    [AutoGenerateComponentState]
    public sealed partial class ShaderSourceComponent : Component
    {
        [DataField(required: true)]
        public string Shader = "";

        [DataField, AutoNetworkedField]
        public List<float> Vars = new List<float> {1f, 1f, 1f, 1f, 1f};

        [DataField]
        public LocId? ExamineMessage = "gunstandingrequired-component-examine";
    }
}
