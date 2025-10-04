using System.Linq;
using Content.Client._WL.Languages;
using Content.Shared._WL.Languages;
using Content.Shared._WL.Languages.Components;
using Content.Client.CharacterInfo;
using Content.Client.Gameplay;
using Content.Client.Stylesheets;
using Content.Client.UserInterface.Systems.MenuBar.Widgets;
using Content.Client.UserInterface.Controls;
using Content.Client.UserInterface.Systems.Character.Controls;
using Content.Client.UserInterface.Systems.Character.Windows;
using Content.Client.UserInterface.Systems.Objectives.Controls;
using Content.Client._WL.UserInterface.Systems.Languages.Controls;
using Content.Client._WL.UserInterface.Systems.Languages.Windows;
using Content.Client._WL.UserInterface.Systems.Languages.LanguageUI;
using Content.Shared.Input;
using Content.Shared.Mind;
using Content.Shared.Mind.Components;
using Content.Shared.Roles;
using JetBrains.Annotations;
using Robust.Client.GameObjects;
using Robust.Client.Player;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controllers;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Input.Binding;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Robust.Client.GameObjects;
using static Content.Client.CharacterInfo.CharacterInfoSystem;
using static Robust.Client.UserInterface.Controls.BaseButton;

namespace Content.Client._WL.UserInterface.Systems.Languages;

[UsedImplicitly]
public sealed class LanguagesUIController : UIController, IOnStateEntered<GameplayState>, IOnStateExited<GameplayState>, IOnSystemChanged<ClientLanguagesSystem>
{
    [Dependency] private readonly IEntityManager _ent = default!;
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;

    [UISystemDependency] private readonly ClientLanguagesSystem _languages = default!;
    [UISystemDependency] private readonly SpriteSystem _sprite = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeNetworkEvent<AfterLanguageChangeEvent>(OnLanguageChanged);
        SubscribeNetworkEvent<MindRoleTypeChangedEvent>(OnMindRoleChanged);
    }

    private LanguagesWindow? _window;
    private MenuButton? LanguagesButton => UIManager.GetActiveUIWidgetOrNull<GameTopMenuBar>()?.LanguagesButton;

    public void OnStateEntered(GameplayState state)
    {
        DebugTools.Assert(_window == null);

        _window = UIManager.CreateWindow<LanguagesWindow>();
        LayoutContainer.SetAnchorPreset(_window, LayoutContainer.LayoutPreset.CenterTop);

        _window.OnClose += DeactivateButton;
        _window.OnOpen += ActivateButton;

        CommandBinds.Builder
            .Bind(ContentKeyFunctions.LanguageChoose,
                InputCmdHandler.FromDelegate(_ => ToggleWindow()))
            .Register<LanguagesUIController>();
    }

    public void OnStateExited(GameplayState state)
    {
        if (_window != null)
        {
            _window.Close();
            _window = null;
        }

        CommandBinds.Unregister<LanguagesUIController>();
    }

    public void OnSystemLoaded(ClientLanguagesSystem system)
    {
        system.OnLanguagesUpdate += LanguagesUpdated;
        _player.LocalPlayerDetached += LanguagesDetached;
    }

    public void OnSystemUnloaded(ClientLanguagesSystem system)
    {
        system.OnLanguagesUpdate -= LanguagesUpdated;
        _player.LocalPlayerDetached -= LanguagesDetached;
    }

    public void UnloadButton()
    {
        if (LanguagesButton == null)
        {
            return;
        }

        LanguagesButton.OnPressed -= LanguagesButtonPressed;
    }

    public void LoadButton()
    {
        if (LanguagesButton == null)
        {
            return;
        }

        LanguagesButton.OnPressed += LanguagesButtonPressed;
    }

    private void DeactivateButton()
    {
        if (LanguagesButton == null)
        {
            return;
        }

        LanguagesButton.Pressed = false;
    }

    private void ActivateButton()
    {
        if (LanguagesButton == null)
        {
            return;
        }

        LanguagesButton.Pressed = true;
    }

    private void LanguagesUpdated(LanguagesData data)
    {
        if (_window == null)
        {
            return;
        }

        var (entity, current, speeking, understood) = data;

        if (_player.LocalEntity != entity)
            return;
        //UpdateRoleType();

        _window.Languages.RemoveAllChildren();

        List<string> groups = new List<string>() { Loc.GetString("ui-languages-knowed-languages") };

        var isPlace = true;

        foreach (var title in groups)
        {
            var languageControl = new LanguagesControl
            {
                Orientation = BoxContainer.LayoutOrientation.Vertical,
                Modulate = Color.White
            };

            var languageText = new FormattedMessage();
            languageText.TryAddMarkup(title, out _);

            var languageLabel = new RichTextLabel
            {
                StyleClasses = { StyleNano.StyleClassTooltipActionTitle }
            };
            languageLabel.SetMessage(languageText);

            languageControl.AddChild(languageLabel);


            foreach (var protoid in speeking)
            {
                var language = _languages.GetLanguagePrototype(protoid);
                if (language == null)
                    continue;

                var languageItemControl = new LanguageItemControl();
                languageItemControl.SetLanguage(protoid);
                languageItemControl.OnChoosePressed += LanguageChange;
                languageItemControl.Icon.Texture = _sprite.Frame0(language.Icon);
                var titleMessage = new FormattedMessage();
                var descriptionMessage = new FormattedMessage();
                titleMessage.AddText(Loc.GetString(language.Name));
                descriptionMessage.AddText(Loc.GetString(language.Description));

                languageItemControl.Title.SetMessage(titleMessage);
                languageItemControl.Description.SetMessage(descriptionMessage);

                if (current == (string)protoid)
                {
                    languageItemControl.ChooseButton.Pressed = true;
                }
                languageControl.AddChild(languageItemControl);
                isPlace = false;
            }


            _window.Languages.AddChild(languageControl);
        }

        _window.RolePlaceholder.Visible = isPlace;
    }

    private void OnLanguageChanged(AfterLanguageChangeEvent ev, EntitySessionEventArgs _)
    {
        UpdateLanguages();
    }

    private void OnMindRoleChanged(MindRoleTypeChangedEvent ev, EntitySessionEventArgs _)
    {
        UpdateLanguages();
    }

    private void UpdateLanguages()
    {
        if (_window == null || !_window.IsOpen)
            return;

        if (!_ent.TryGetComponent<LanguagesComponent>(_player.LocalEntity, out var comp))
            return;

        var entity = _player.LocalEntity;
        EntityUid entt;
        if (entity.HasValue)
            entt = entity.Value;
        else
            return;

        var data = new LanguagesData(entt, comp.CurrentLanguage, comp.Speaking, comp.Understood);

        LanguagesUpdated(data);
    }

    public void LanguageChange(ProtoId<LanguagePrototype> language)
    {
        if (!_player.LocalEntity.HasValue)
            return;
        if (!_ent.TryGetNetEntity(_player.LocalEntity.Value, out var netEntity))
            return;
        _languages.TryChangeLanguage(netEntity.Value, language);
    }

    private void LanguagesDetached(EntityUid uid)
    {
        CloseWindow();
    }

    private void LanguagesButtonPressed(ButtonEventArgs args)
    {
        ToggleWindow();
    }

    private void CloseWindow()
    {
        _window?.Close();
    }

    private void ToggleWindow()
    {
        if (_window == null)
            return;

        LanguagesButton?.SetClickPressed(!_window.IsOpen);

        if (_window.IsOpen)
        {
            CloseWindow();
        }
        else
        {
            _window.Open();
            UpdateLanguages();
        }
    }
}
