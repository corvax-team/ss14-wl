using Content.Client.Message;
using Content.Client.UserInterface.Controls;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.CustomControls;
using Robust.Shared.Timing;
using System.Data;

namespace Content.Client._WL.DiscordAuth
{
    public sealed partial class PlayerSessionInfoWindow : DefaultWindow
    {
        [Dependency] private readonly IClipboardManager _clipboardMan = default!;
        private readonly ClientDiscordAuthSystem _discordAuth;

        private readonly TabContainer _mainTab;

        private readonly ProgressBar _discordProgressBar;

        public PlayerSessionInfoWindow(ClientDiscordAuthSystem auth)
        {
            IoCManager.InjectDependencies(this);

            VerticalExpand = true;
            HorizontalExpand = true;
            MinSize = new(800, 450);

            _discordAuth = auth;

            Title = "Информация о сессии";

            var main_tab = new TabContainer()
            {
                Access = Robust.Client.UserInterface.AccessLevel.Public,
                Margin = new(5, 40, 5, 5),
                VerticalExpand = true,
                HorizontalExpand = true
            };
            _mainTab = main_tab;

            #region Discord
            var discord_tab = new BoxContainer()
            {
                Orientation = BoxContainer.LayoutOrientation.Vertical,
                VerticalExpand = true,
                HorizontalExpand = true
            };

            #region content
            var discord_scroll = new ScrollContainer()
            {
                VerticalExpand = true,
                HorizontalExpand = true,
            };

            var discord_content_box = new BoxContainer()
            {
                Orientation = BoxContainer.LayoutOrientation.Vertical,
                VerticalExpand = true,
                HorizontalExpand = true
            };

            var discord_content_box_title = new Label()
            {
                HorizontalAlignment = HAlignment.Center,
                Text = "Аутентификация",
                Margin = new(0, 7)
            };
            discord_content_box_title.AddStyleClass("LabelKeyText");

            var discord_content_bot_ucode_box = new BoxContainer()
            {
                Orientation = BoxContainer.LayoutOrientation.Horizontal
            };

            var discord_content_bot_ucode_title = new Label()
            {
                Text = "Уникальный  код:",
                VerticalAlignment = VAlignment.Center,
                Margin = new(20, 0, 20, 0)
            };

            var style_box = new StyleBoxFlat(Color.Black);

            var token = _discordAuth.GetUserCode();
            var discord_content_bot_ucode_value = new RichTextLabel()
            {
                Text = token ?? "Unknown"
            };

            _discordAuth.SubscribeOnTokenChanged(token =>
            {
                discord_content_bot_ucode_value.SetMarkup(token);
            });

            var discord_content_bot_ucode_value_panel = new PanelContainer()
            {
                PanelOverride = style_box,
                MouseFilter = MouseFilterMode.Stop,
                ToolTip = "Кликните, чтобы скопировать код в буфер обмена."
            };

            discord_content_bot_ucode_value_panel.OnMouseEntered += _ =>
            {
                discord_content_bot_ucode_value_panel.PanelOverride = null;
            };
            discord_content_bot_ucode_value_panel.OnMouseExited += _ =>
            {
                discord_content_bot_ucode_value_panel.PanelOverride = style_box;
            };
            discord_content_bot_ucode_value_panel.OnKeyBindUp += _ =>
            {
                _clipboardMan.SetText(discord_content_bot_ucode_value.Text);
            };

            discord_content_bot_ucode_value.AddChild(discord_content_bot_ucode_value_panel);

            discord_content_bot_ucode_box.AddChild(discord_content_bot_ucode_title);
            discord_content_bot_ucode_box.AddChild(discord_content_bot_ucode_value);

            discord_content_box.AddChild(discord_content_box_title);
            discord_content_box.AddChild(discord_content_bot_ucode_box);

            discord_scroll.AddChild(discord_content_box);

            discord_tab.AddChild(discord_scroll);
            #endregion

            #region progress bar
            var stripe_back = new StripeBack()
            {
                HasBottomEdge = false
            };

            var progress_bar_box = new BoxContainer()
            {
                Orientation = BoxContainer.LayoutOrientation.Horizontal
            };

            var progress_bar_box_label = new Label()
            {
                Text = "До смены ключа авторизации:",
                Margin = new(10, 0)
            };
            progress_bar_box_label.AddStyleClass("LabelKeyText");

            var progress_bar = new ProgressBar()
            {
                MinValue = 0,
                MaxValue = 1,
                HorizontalExpand = true,
                Margin = new(1, 2, 1, 1)
            };
            _discordProgressBar = progress_bar;

            progress_bar_box.AddChild(progress_bar_box_label);
            progress_bar_box.AddChild(progress_bar);

            stripe_back.AddChild(progress_bar_box);

            discord_tab.AddChild(stripe_back);
            #endregion

            var discord_tab_box = new BoxContainer();
            discord_tab_box.AddChild(discord_tab);

            AddTab(0, "Дискорд", discord_tab_box);
            #endregion

            AddChild(main_tab);
        }

        protected override void FrameUpdate(FrameEventArgs args)
        {
            base.FrameUpdate(args);

            var fraction = _discordAuth.GetExpirationTimeFraction();

            UpdateDiscordAuthProgressBar(fraction);
        }

        public void AddTab(int place, string title, Control control)
        {
            _mainTab.AddChild(control);

            _mainTab.SetTabTitle(place, title);
        }

        public void UpdateDiscordAuthProgressBar(float value)
        {
            _discordProgressBar.Value = value;
        }
    }
}
