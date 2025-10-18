using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared._WL.Languages;

[Prototype]
public sealed partial class LanguagePrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = string.Empty;

    [DataField(required: true)]
    public string Name = string.Empty;

    [DataField(required: true)]
    public string Description = string.Empty;

    [DataField("icon")]
    public SpriteSpecifier Icon = new SpriteSpecifier.Texture(new ("/Textures/_WL/Interface/Languages/languages.rsi/default.png"));

    [DataField(required: true)]
    public ObfuscationMethod Obfuscation = ObfuscationMethod.Default;

    [DataField("color")]
    public Color Color = Color.White;
}
