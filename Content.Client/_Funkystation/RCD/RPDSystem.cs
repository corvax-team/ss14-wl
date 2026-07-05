using Content.Client.Items;
using Content.Client.Message;
using Content.Shared.RCD.Components;
using Content.Shared.RCD.Systems;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Client.RCD.Systems;

// WL-Changes: ported original file & modifided | deleted RCDSystem
public sealed class RPDSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        Subs.ItemStatus<RCDComponent>(OnItemStatus);
    }

    private Control OnItemStatus(Entity<RCDComponent> entity)
    {
        return new RPDModeStatusControl(entity);
    }

    private sealed class RPDModeStatusControl : Control
    {
        private readonly RichTextLabel _label = new()
        {
            StyleClasses = { "ItemStatus" }
        };

        private readonly Entity<RCDComponent> _uid;
        private readonly bool _isRpd;

        public RPDModeStatusControl(Entity<RCDComponent> entity)
        {
            _uid = entity; // WL-Changes
            _isRpd = entity.Comp.IsRpd;
            AddChild(_label);
        }

        protected override void FrameUpdate(FrameEventArgs args)
        {
            if (!_isRpd) return;

            base.FrameUpdate(args);

            var currentMode = _uid.Comp.CurrentMode; // WL-Changes

            var modeKey = $"rcd-rpd-mode-{currentMode.ToString().ToLowerInvariant()}";
            var modeName = Robust.Shared.Localization.Loc.GetString(modeKey);

            _label.SetMarkup(Robust.Shared.Localization.Loc.GetString("rcd-item-status-mode",
                ("mode", $"[color=cyan]{modeName}[/color]")));
        }
    }
}
