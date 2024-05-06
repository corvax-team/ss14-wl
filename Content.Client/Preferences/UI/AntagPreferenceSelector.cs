using Content.Client.Players.PlayTimeTracking;
using Content.Shared.Roles;
using Robust.Client.UserInterface.Controls;
using System.Numerics;

namespace Content.Client.Preferences.UI;

public sealed class AntagPreferenceSelector : RequirementsSelector<AntagPrototype>
{
    // 0 is yes and 1 is no
    public bool Preference
    {
        get => Options.SelectedValue == 0;
        set => Options.Select((value && !Disabled) ? 0 : 1);
    }

    public event Action<bool>? PreferenceChanged;

    public AntagPreferenceSelector(AntagPrototype proto, ButtonGroup btnGroup)
        : base(proto, btnGroup)
    {
        Options.OnItemSelected += args => PreferenceChanged?.Invoke(Preference);

        var items = new[]
        {
            ("humanoid-profile-editor-antag-preference-yes-button", 0),
            ("humanoid-profile-editor-antag-preference-no-button", 1)
        };
        var title = Loc.GetString(proto.Name);
        var description = Loc.GetString(proto.Objective);

        var titleLabel = new Label()
        {
            Margin = new Thickness(5f, 0, 5f, 0),
            Text = title,
            MinSize = new Vector2(250, 0),
            MouseFilter = MouseFilterMode.Stop,
            ToolTip = description
        };

        Setup(null, items, titleLabel);

        // immediately lock requirements if they arent met.
        // another function checks Disabled after creating the selector so this has to be done now
        var requirements = IoCManager.Resolve<JobRequirementsManager>();
        if (proto.Requirements != null && !requirements.CheckRoleTime(proto.Requirements, out var reason))
        {
            LockRequirements(reason);
        }
    }
}
