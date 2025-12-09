using Robust.Shared.Configuration;

namespace Content.Shared._WL.CCVars;

/// <summary>
///     WL modules console variables
/// </summary>
public sealed partial class WLCVars
{
    /// <summary>
    /// Доступна ли игрокам возможность вызвать шаттл голосованием?
    /// </summary>
    public static readonly CVarDef<bool> VoteShuttleEnabled =
        CVarDef.Create("vote.evacuation_shuttle_vote_enabled", true, CVar.SERVERONLY);

    /// <summary>
    /// Сколько требуется согласных игроков для вызова.
    /// В процентах.
    /// </summary>
    public static readonly CVarDef<float> VoteShuttlePlayersRatio =
        CVarDef.Create("vote.evacuation_shuttle_vote_ratio", 0.6f, CVar.SERVERONLY);

    /// <summary>
    /// Время голосования.
    /// </summary>
    public static readonly CVarDef<int> VoteShuttleTimer =
        CVarDef.Create("vote.evacuation_shuttle_vote_time", 40, CVar.SERVERONLY);
}
