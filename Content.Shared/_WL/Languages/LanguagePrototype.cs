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

    [DataField("icon")]
    public SpriteSpecifier Icon = new SpriteSpecifier.Texture(new ("/Textures/_WL/Interface/Languages/languages.rsi/default.png"));

    [DataField(required: true)]
    public ObfuscationMethod Obfuscation = ObfuscationMethod.Default;

    [DataField("keylang")]
    public char KeyLanguage = '\0';

    [DataField("color")]
    public Color Color = Color.LightGray;

    [DataField("needtts")]
    public bool NeedTTS = true;

    [DataField("emoting")]
    public bool Emoting = false;

    [DataField("radioPass")]
    public float RadioPass = 1f;

    [DataField("pressurePass")]
    public float PressurePass = 0f;

    [DataField("fontId")]
    public string FontId = "Default";

    [DataField("fontSize")]
    public int FontSize = 12;

    [DataField("customSound")]
    public bool CustomSound = false;

    [DataField("sound")]
    public SoundCollectionSpecifier Sound = new SoundCollectionSpecifier("TernarySounds");
}
