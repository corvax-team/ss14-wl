using Robust.Shared.Configuration;

namespace Content.Shared._WL.CCVars;

/// <summary>
///     WL modules console variables
/// </summary>
public sealed partial class WLCVars
{
    /// <summary>
    /// Токен для авторизации htpp-запросов на api сервера.
    /// </summary>
    public static readonly CVarDef<string> WLApiToken =
        CVarDef.Create(
            "admin.wl_api_token", string.Empty,
            CVar.SERVERONLY | CVar.CONFIDENTIAL,
            "Строковой токен, использующийся для авторизации HTTP-запросов, отправленных на http API сервера.");
}
