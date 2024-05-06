using System.Linq;
using System.Numerics;
using Content.Shared.Preferences;
using Content.Shared.Preferences.Loadouts;
using Content.Shared.Preferences.Loadouts.Effects;
using Content.Shared.Roles;
using Content.Shared.StatusIcon;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Client.Utility;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Client.Preferences.UI;

public sealed class JobPrioritySelector : RequirementsSelector<JobPrototype>
{
    public JobPrototype Job { get; private set; }

    public JobPriority Priority
    {
        get => (JobPriority) Options.SelectedValue;
        set => Options.SelectByValue((int) value);
    }

    public event Action<JobPriority>? PriorityChanged;
    public Action<int/*ItemID*/, string/*SUBNAME*/, bool/*IsSILENT*/>? SubnameChanged;

    public Dictionary<string, int> SubnameIDs { get; private set; }
    public OptionButton? SubnameOptions { get; private set; }

    public JobPrioritySelector(RoleLoadout? loadout, JobPrototype proto, ButtonGroup btnGroup, IPrototypeManager protoMan)
        : base(proto, btnGroup)
    {
        Job = proto;
        SubnameIDs = new();

        Options.OnItemSelected += args => PriorityChanged?.Invoke(Priority);

        var items = new[]
        {
            ("humanoid-profile-editor-job-priority-high-button", (int) JobPriority.High),
            ("humanoid-profile-editor-job-priority-medium-button", (int) JobPriority.Medium),
            ("humanoid-profile-editor-job-priority-low-button", (int) JobPriority.Low),
            ("humanoid-profile-editor-job-priority-never-button", (int) JobPriority.Never),
        };

        var icon = new TextureRect
        {
            TextureScale = new Vector2(2, 2),
            VerticalAlignment = VAlignment.Center
        };
        var jobIcon = protoMan.Index<StatusIconPrototype>(proto.Icon);
        icon.Texture = jobIcon.Icon.Frame0();

        var mainBox = new BoxContainer()
        {
            Orientation = LayoutOrientation.Horizontal
        };

        if (proto.Subnames.Count > 0)
        {
            var allSubnames = proto.Subnames
                .Order()
                .ToList();

            var optionsLabel = new OptionButton()
            {
                ToolTip = proto.LocalizedDescription,
                HorizontalAlignment = HAlignment.Left,
            };

            SubnameOptions = optionsLabel;

            optionsLabel.AddItem(Job.LocalizedName, -1);
            optionsLabel.SelectId(-1);

            for (var i = 0; i < allSubnames.Count; i++)
            {
                optionsLabel.AddItem(allSubnames[i], i);
                SubnameIDs.Add(allSubnames[i], i);
            }

            optionsLabel.OnItemSelected += args =>
            {
                optionsLabel.SelectId(args.Id);

                if (args.Id == -1)
                    SubnameChanged?.Invoke(args.Id, proto.LocalizedName, false);
                else SubnameChanged?.Invoke(args.Id, allSubnames[args.Id], false);
            };

            var secondBox = new BoxContainer()
            {
                MinSize = new Vector2(260, 0),
            };

            secondBox.AddChild(optionsLabel);
            mainBox.AddChild(secondBox);
        }
        else
        {
            var titleLabel = new Label()
            {
                Margin = new Thickness(5f, 0, 5f, 0),
                Text = proto.LocalizedName,
                MinSize = new Vector2(250, 0),
                MouseFilter = MouseFilterMode.Stop,
                ToolTip = proto.LocalizedDescription
            };

            mainBox.AddChild(titleLabel);
        }

        Setup(loadout, items, mainBox, icon);
    }

    public void SelectJobSubname(string subname)
    {
        if (SubnameIDs.TryGetValue(subname, out var index))
            SubnameOptions?.SelectId(index);
    }

    public string? GetChosenSubname()
    {
        return SubnameIDs.FirstOrNull(x => x.Value == SubnameOptions?.SelectedId)?.Key;
    }
}
