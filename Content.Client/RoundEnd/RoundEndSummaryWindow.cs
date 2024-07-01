using System.Linq;
using System.Numerics;
using Content.Client._WL.Skills;
using Content.Client.Message;
using Content.Shared._WL.Skills;
using Content.Shared._WL.Skills.Systems;
using Content.Shared.GameTicking;
using Robust.Client.Player;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.CustomControls;
using Robust.Shared.Network;
using Robust.Shared.Utility;
using static Robust.Client.UserInterface.Controls.BoxContainer;

namespace Content.Client.RoundEnd
{
    public sealed class RoundEndSummaryWindow : DefaultWindow
    {
        private readonly IEntityManager _entityManager;
        public int RoundId;

        //WL-Skills-start
        private readonly ClientSkillsSystem _skills;
        [Dependency] private readonly IPlayerManager _playMan = default!;
        //WL-Skills-end

        public RoundEndSummaryWindow(string gm, string roundEnd, TimeSpan roundTimeSpan, int roundId,
            RoundEndMessageEvent.RoundEndPlayerInfo[] info, IEntityManager entityManager)
        {
            _entityManager = entityManager;

            //WL-Skills-start
            IoCManager.InjectDependencies(this);
            _skills = _entityManager.System<ClientSkillsSystem>();
            //WL-Skills-end

            MinSize = SetSize = new Vector2(520, 580);

            Title = Loc.GetString("round-end-summary-window-title");

            // The round end window is split into two tabs, one about the round stats
            // and the other is a list of RoundEndPlayerInfo for each player.
            // This tab would be a good place for things like: "x many people died.",
            // "clown slipped the crew x times.", "x shots were fired this round.", etc.
            // Also good for serious info.

            RoundId = roundId;
            var roundEndTabs = new TabContainer();
            roundEndTabs.AddChild(MakeRoundEndSummaryTab(gm, roundEnd, roundTimeSpan, roundId));
            roundEndTabs.AddChild(MakePlayerManifestTab(info));

            //WL-Skills-start
            roundEndTabs.AddChild(MakeRoundEndSkillsTab(info));
            //WL-Skills-end

            Contents.AddChild(roundEndTabs);

            OpenCenteredRight();
            MoveToFront();
        }

        //WL-Skills-start
        private BoxContainer MakeRoundEndSkillsTab(RoundEndMessageEvent.RoundEndPlayerInfo[] infos)
        {
            var scrollBox = new ScrollContainer()
            {
                VerticalExpand = true
            };

            var roundEndSummaryTab = new BoxContainer()
            {
                Margin = new(10),
                Name = "Навыки",
                Orientation = LayoutOrientation.Vertical
            };

            var main = new BoxContainer()
            {
                Orientation = LayoutOrientation.Vertical
            };

            var playersSkills = new Dictionary<(string Name, Color Color), Dictionary<string, SkillLevel>>();
            foreach (var info in infos)
            {
                var entityNullable = _entityManager.GetEntity(info.PlayerNetEntity);
                if (entityNullable == null)
                    continue;

                var oocName = info.PlayerOOCName;
                var entity = entityNullable.Value;

                var skillsInfos = _skills.GetSkillInfosFromEntity(entity);
                foreach (var skillInfo in skillsInfos)
                {
                    //УвУжасы
                    if (!playersSkills.TryAdd((skillInfo.Name, skillInfo.Color), new() { { oocName, skillInfo.Level } }))
                        playersSkills[(skillInfo.Name, skillInfo.Color)][oocName] = skillInfo.Level;
                }
            }

            foreach (var playerSkill in playersSkills.OrderBy(s => s.Key))
            {
                var separatedBox = new BoxContainer()
                {
                    Margin = new(5, 5, 5, 20),
                    Orientation = LayoutOrientation.Vertical
                };

                var skillNameLabel = new RichTextLabel();
                var labelColor = playerSkill.Key.Color.ToHex();
                var labelName = playerSkill.Key.Name;
                skillNameLabel.SetMarkup($"[head=2][color={labelColor}]{labelName}[/color][/head]");

                separatedBox.AddChild(skillNameLabel);

                foreach (var userSkillLevel in playerSkill.Value)
                {
                    var name = userSkillLevel.Key;
                    var level = userSkillLevel.Value;

                    var label = new RichTextLabel();
                    if (_playMan.LocalSession?.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase) == true)
                        name = $"[color=yellow]{name}[/color]";

                    label.SetMarkup($"\t\t{name}: {SharedSkillsSystem.GetSkillLocName(level)}");

                    separatedBox.AddChild(label);
                }

                main.AddChild(separatedBox);
            }

            scrollBox.AddChild(main);
            roundEndSummaryTab.AddChild(scrollBox);

            return roundEndSummaryTab;
        }
        //WL-Skills-end

        private BoxContainer MakeRoundEndSummaryTab(string gamemode, string roundEnd, TimeSpan roundDuration, int roundId)
        {
            var roundEndSummaryTab = new BoxContainer
            {
                Orientation = LayoutOrientation.Vertical,
                Name = Loc.GetString("round-end-summary-window-round-end-summary-tab-title")
            };

            var roundEndSummaryContainerScrollbox = new ScrollContainer
            {
                VerticalExpand = true,
                Margin = new Thickness(10)
            };
            var roundEndSummaryContainer = new BoxContainer
            {
                Orientation = LayoutOrientation.Vertical
            };

            //Gamemode Name
            var gamemodeLabel = new RichTextLabel();
            var gamemodeMessage = new FormattedMessage();
            gamemodeMessage.AddMarkup(Loc.GetString("round-end-summary-window-round-id-label", ("roundId", roundId)));
            gamemodeMessage.AddText(" ");
            gamemodeMessage.AddMarkup(Loc.GetString("round-end-summary-window-gamemode-name-label", ("gamemode", gamemode)));
            gamemodeLabel.SetMessage(gamemodeMessage);
            roundEndSummaryContainer.AddChild(gamemodeLabel);

            //Duration
            var roundTimeLabel = new RichTextLabel();
            roundTimeLabel.SetMarkup(Loc.GetString("round-end-summary-window-duration-label",
                                                   ("hours", roundDuration.Hours),
                                                   ("minutes", roundDuration.Minutes),
                                                   ("seconds", roundDuration.Seconds)));
            roundEndSummaryContainer.AddChild(roundTimeLabel);

            //Round end text
            if (!string.IsNullOrEmpty(roundEnd))
            {
                var roundEndLabel = new RichTextLabel();
                roundEndLabel.SetMarkup(roundEnd);
                roundEndSummaryContainer.AddChild(roundEndLabel);
            }

            roundEndSummaryContainerScrollbox.AddChild(roundEndSummaryContainer);
            roundEndSummaryTab.AddChild(roundEndSummaryContainerScrollbox);

            return roundEndSummaryTab;
        }

        private BoxContainer MakePlayerManifestTab(RoundEndMessageEvent.RoundEndPlayerInfo[] playersInfo)
        {
            var playerManifestTab = new BoxContainer
            {
                Orientation = LayoutOrientation.Vertical,
                Name = Loc.GetString("round-end-summary-window-player-manifest-tab-title")
            };

            var playerInfoContainerScrollbox = new ScrollContainer
            {
                VerticalExpand = true,
                Margin = new Thickness(10)
            };
            var playerInfoContainer = new BoxContainer
            {
                Orientation = LayoutOrientation.Vertical
            };

            //Put observers at the bottom of the list. Put antags on top.
            var sortedPlayersInfo = playersInfo.OrderBy(p => p.Observer).ThenBy(p => !p.Antag);

            //Create labels for each player info.
            foreach (var playerInfo in sortedPlayersInfo)
            {
                var hBox = new BoxContainer
                {
                    Orientation = LayoutOrientation.Horizontal,
                };

                var playerInfoText = new RichTextLabel
                {
                    VerticalAlignment = VAlignment.Center,
                    VerticalExpand = true,
                };

                if (playerInfo.PlayerNetEntity != null)
                {
                    hBox.AddChild(new SpriteView(playerInfo.PlayerNetEntity.Value, _entityManager)
                        {
                            OverrideDirection = Direction.South,
                            VerticalAlignment = VAlignment.Center,
                            SetSize = new Vector2(32, 32),
                            VerticalExpand = true,
                        });
                }

                if (playerInfo.PlayerICName != null)
                {
                    if (playerInfo.Observer)
                    {
                        playerInfoText.SetMarkup(
                            Loc.GetString("round-end-summary-window-player-info-if-observer-text",
                                          ("playerOOCName", playerInfo.PlayerOOCName),
                                          ("playerICName", playerInfo.PlayerICName)));
                    }
                    else
                    {
                        //TODO: On Hover display a popup detailing more play info.
                        //For example: their antag goals and if they completed them sucessfully.
                        var icNameColor = playerInfo.Antag ? "red" : "white";
                        playerInfoText.SetMarkup(
                            Loc.GetString("round-end-summary-window-player-info-if-not-observer-text",
                                ("playerOOCName", playerInfo.PlayerOOCName),
                                ("icNameColor", icNameColor),
                                ("playerICName", playerInfo.PlayerICName),
                                ("playerRole", Loc.GetString(playerInfo.Role))));
                    }
                }
                hBox.AddChild(playerInfoText);
                playerInfoContainer.AddChild(hBox);
            }

            playerInfoContainerScrollbox.AddChild(playerInfoContainer);
            playerManifestTab.AddChild(playerInfoContainerScrollbox);

            return playerManifestTab;
        }
    }

}
