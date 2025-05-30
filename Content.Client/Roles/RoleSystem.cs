using Content.Client.Lobby;
using Content.Shared.Preferences;
using Content.Shared.Roles;

namespace Content.Client.Roles;

public sealed class RoleSystem : SharedRoleSystem
{
    //WL-changes-start
    [Dependency] private readonly IClientPreferencesManager _prefMan = default!;

    public string? GetChosenSubname(string jobId)
    {
        var notTrueProfile = _prefMan.Preferences?.SelectedCharacter;
        if (notTrueProfile == null || notTrueProfile is not HumanoidCharacterProfile profile)
            return null;

        if (!profile.JobSubnames.TryGetValue(jobId, out var subname))
            return null;

        return subname;
    }
    //WL-changes-end
}
