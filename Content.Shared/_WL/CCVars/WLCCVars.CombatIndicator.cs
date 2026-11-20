using Content.Shared._WL.Barks;
using Robust.Shared.Configuration;

namespace Content.Shared._WL.CCVars;

public sealed partial class WLCVars
{
    public static readonly CVarDef<bool> CombatIndicatorAutoEnable =
        CVarDef.Create("game.combat_indicator_auto_enable", false, CVar.CLIENTONLY | CVar.ARCHIVE);
}
