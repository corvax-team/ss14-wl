using Robust.Shared.Audio;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared._WL.Languages;

[Prototype]
public sealed partial class LanguagePrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = string.Empty;

    ///TODO: <see cref="LocId"/>?
    [DataField(required: true)]
    public string Name = string.Empty;

    ///TODO: <see cref="LocId"/>?
    [DataField(required: true)]
    public string Description = string.Empty;

    [DataField]
    public SpriteSpecifier Icon = new SpriteSpecifier.Texture(new("/Textures/_WL/Interface/Languages/languages.rsi/default.png"));

    [DataField(required: true)]
    public ObfuscationMethod Obfuscation = ObfuscationMethod.Default;

    [DataField("keylang")]
    public char KeyLanguage = '\0';

    [DataField]
    public Color Color = Color.LightGray;

    [DataField]
    public bool NeedTTS = true;

    [DataField]
    public bool Emoting = false;

    [DataField]
    public float RadioPass = 1f;

    [DataField]
    public float PressurePass = 0f;

    [DataField]
    public string FontId = "Default";

    [DataField]
    public int FontSize = 12;

    [DataField]
    public bool CustomSound = false;

    [DataField]
    public SoundCollectionSpecifier Sound = new SoundCollectionSpecifier("TernarySounds");
}
