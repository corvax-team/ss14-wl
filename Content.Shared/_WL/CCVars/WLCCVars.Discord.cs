using Robust.Shared.Configuration;

namespace Content.Shared._WL.CCVars;

/// <summary>
///     WL modules console variables
/// </summary>
public sealed partial class WLCVars
{
    /// <summary>
    ///     Через какое время все токены на подключение аккаунта игры к дискорду будут недействительны.
    /// </summary>
    public static readonly CVarDef<int> DiscordAuthTokensExpirationTime =
        CVarDef.Create("discord.auth_tokens_expiration_time", 300, CVar.REPLICATED);

    /// <summary>
    ///     URL of the Discord webhook which will relay manifest messages.
    /// </summary>
    public static readonly CVarDef<string> DiscordRoundManifestWebhook =
        CVarDef.Create("discord.round_manifest_webhook", string.Empty, CVar.SERVERONLY | CVar.CONFIDENTIAL);

    /// <summary>
    ///     HEX color of manifest discord webhook's embed.
    /// </summary>
    public static readonly CVarDef<string> DiscordRoundManifestWebhookEmbedColor =
        CVarDef.Create("discord.round_manifest_webhook_embed_color", Color.DeepSkyBlue.ToHex(), CVar.SERVERONLY);
}
