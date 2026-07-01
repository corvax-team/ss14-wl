using System.Diagnostics.CodeAnalysis;
using Robust.Shared.Player;
using Robust.Shared.Utility;

namespace Content.Shared.Preferences.Loadouts.Effects;

public sealed partial class CkeyRequirementLoadoutEffect : LoadoutEffect
{
    [DataField(required: true)]
    public string Ckey = default!;

    public override bool Validate(
        HumanoidCharacterProfile profile,
        RoleLoadout loadout,
        LoadoutPrototype proto, // Corvax-Sponsors
        ICommonSession? session,
        IDependencyCollection collection,
        [NotNullWhen(false)] out FormattedMessage? reason)
    {
        reason = null;

        if (session == null) reason = FormattedMessage.Empty;

        if (session?.Name != Ckey) reason = FormattedMessage.Empty;

        return reason == null;
    }
}
