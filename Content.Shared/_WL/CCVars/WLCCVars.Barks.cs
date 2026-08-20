using Content.Shared._WL.Barks;
using Robust.Shared.Configuration;

namespace Content.Shared._WL.CCVars;

public sealed partial class WLCVars
{
    /// <summary>
    /// Selects how spoken chat messages are voiced on this client.
    /// </summary>
    public static readonly CVarDef<SpeechMode> SpeechMode =
        CVarDef.Create(
            "audio.speech_mode",
            Barks.SpeechMode.Tts,
            CVar.CLIENTONLY | CVar.ARCHIVE);

    /// <summary>
    /// Client volume for speech barks.
    /// </summary>
    public static readonly CVarDef<float> BarksVolume =
        CVarDef.Create("audio.barks_volume", 1f, CVar.CLIENTONLY | CVar.ARCHIVE);
}
