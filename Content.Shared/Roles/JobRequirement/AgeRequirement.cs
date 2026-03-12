using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Content.Shared._WL.CCVars;
using Content.Shared.Preferences;
using JetBrains.Annotations;
using Robust.Shared.Configuration;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;

namespace Content.Shared.Roles;

/// <summary>
/// Requires the character to be older or younger than a certain age (inclusive)
/// </summary>
[UsedImplicitly]
[Serializable, NetSerializable]
public sealed partial class AgeRequirement : JobRequirement
{
    [DataField]
    public int Age = 0; //WL-Changes

    public override bool Check(
        IEntityManager entManager,
        IPrototypeManager protoManager,
        IConfigurationManager cfgMan, //WL-Changes
        HumanoidCharacterProfile? profile,
        JobPrototype? job, //WL-Changes
        IReadOnlyDictionary<string, TimeSpan> playTimes,
        [NotNullWhen(false)] out FormattedMessage? reason)
    {
        reason = new FormattedMessage();

        if (profile is null) //the profile could be null if the player is a ghost. In this case we don't need to block the role selection for ghostrole
            return true;

        //WL-Changes-start
        if (!protoManager.TryIndex(profile.Species, out var specie))
            return true;

        if (specie is null)
            return true;

        if (job is null)
            return true;

        if (cfgMan.GetCVar(WLCVars.IsAgeCheckNeeded) == false)
            return true;

        var isNeeded = true;
        if (profile.JobUnblockings.ContainsKey(job.ID))
            isNeeded = false;

        if (isNeeded)
        {
            if (profile.Age < specie.MinAge + Age)
            {
                reason = FormattedMessage.FromMarkupPermissive(Loc.GetString("role-timer-age-too-young",
                    ("age", Age)));
                return false;
            }
        }
        //WL-Changes-end

        return true;
    }
}
