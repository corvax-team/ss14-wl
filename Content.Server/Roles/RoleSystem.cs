using Content.Server.Preferences.Managers;
using Content.Shared.Preferences;
using Content.Shared.Roles;
using Content.Shared.Roles.Jobs;
using Robust.Server.Player;
using Robust.Shared.Player;
using Robust.Shared.Utility;
using System.Diagnostics.CodeAnalysis;

namespace Content.Server.Roles;

public sealed class RoleSystem : SharedRoleSystem
{
    [Dependency] private readonly IServerPreferencesManager _servPrefMan = default!;
    [Dependency] private readonly IPlayerManager _playMan = default!;

    public override void Initialize()
    {
        // TODO make roles entities
        base.Initialize();

        SubscribeAntagEvents<DragonRoleComponent>();
        SubscribeAntagEvents<InitialInfectedRoleComponent>();
        SubscribeAntagEvents<NinjaRoleComponent>();
        SubscribeAntagEvents<NukeopsRoleComponent>();
        SubscribeAntagEvents<RevolutionaryRoleComponent>();
        SubscribeAntagEvents<SubvertedSiliconRoleComponent>();
        SubscribeAntagEvents<TraitorRoleComponent>();
        SubscribeAntagEvents<ZombieRoleComponent>();
        SubscribeAntagEvents<ThiefRoleComponent>();

        SubscribeLocalEvent<JobComponent, MindGetAllRolesEvent>(OnJobGetAllRoles);
    }

    public string? MindGetBriefing(EntityUid? mindId)
    {
        if (mindId == null)
            return null;

        var ev = new GetBriefingEvent();
        RaiseLocalEvent(mindId.Value, ref ev);
        return ev.Briefing;
    }

    private void OnJobGetAllRoles(EntityUid uid, JobComponent component, ref MindGetAllRolesEvent args)
    {
        var name = "game-ticker-unknown-role";
        var prototype = "";
        string? playTimeTracker = null;

        if (component.Prototype != null && _prototypes.TryIndex(component.Prototype, out JobPrototype? job))
        {
            prototype = job.ID;
            playTimeTracker = job.PlayTimeTracker;

            var profile = GetProfileByEntity(uid);

            if (profile != null
                && profile.JobSubnames.TryGetValue(job.ID, out var subname))
            {
                name = subname;
            }
            else name = job.LocalizedName;
        }

        name = Loc.GetString(name);

        args.Roles.Add(new RoleInfo(component, name, false, playTimeTracker, prototype));
    }

    public HumanoidCharacterProfile? GetProfileByEntity(EntityUid? entity)
    {
        if (entity == null)
            return null;

        if (_playMan.TryGetSessionByEntity(entity.Value, out var session))
        {
            var genericProfile = _servPrefMan.GetSelectedProfilesForPlayers([session.UserId]).FirstOrNull()?.Value;

            if (genericProfile != null
                && genericProfile is HumanoidCharacterProfile certainProfile)
            {
                return certainProfile;
            }
        }

        return null;
    }

    public HumanoidCharacterProfile? GetProfileBySession(ICommonSession? session)
    {
        if (session == null)
            return null;

        var genericProfile = _servPrefMan.GetSelectedProfilesForPlayers([session.UserId]).FirstOrNull()?.Value;

        if (genericProfile != null
            && genericProfile is HumanoidCharacterProfile certainProfile)
        {
            return certainProfile;
        }

        return null;
    }

    public string? GetSubnameByEntity(EntityUid entity, string jobId)
    {
        var profile = GetProfileByEntity(entity);
        if (profile == null)
            return null;

        if (!profile.JobSubnames.TryGetValue(jobId, out var subname))
            return null;

        return subname;
    }

    public string? GetSubnameBySesssion(ICommonSession? session, string jobId)
    {
        if (session == null)
            return null;

        var profile = GetProfileBySession(session);
        if (profile == null)
            return null;

        if (!profile.JobSubnames.TryGetValue(jobId, out var subname))
            return null;

        return subname;
    }
}

/// <summary>
/// Event raised on the mind to get its briefing.
/// Handlers can either replace or append to the briefing, whichever is more appropriate.
/// </summary>
[ByRefEvent]
public sealed class GetBriefingEvent
{
    public string? Briefing;

    public GetBriefingEvent(string? briefing = null)
    {
        Briefing = briefing;
    }

    /// <summary>
    /// If there is no briefing, sets it to the string.
    /// If there is a briefing, adds a new line to separate it from the appended string.
    /// </summary>
    public void Append(string text)
    {
        if (Briefing == null)
        {
            Briefing = text;
        }
        else
        {
            Briefing += "\n" + text;
        }
    }
}
