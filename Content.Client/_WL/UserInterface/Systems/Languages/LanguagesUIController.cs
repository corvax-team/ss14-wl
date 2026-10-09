using Content.Client._WL.Languages;
using Content.Shared._WL.Languages;
using Content.Shared._WL.Languages.Components;
using Content.Client.Gameplay;
using Content.Client.Stylesheets;
using Content.Client.UserInterface.Systems.MenuBar.Widgets;
using Content.Client.UserInterface.Controls;
using Content.Client._WL.UserInterface.Systems.Languages.Controls;
using Content.Client._WL.UserInterface.Systems.Languages.Windows;
using Content.Client._WL.UserInterface.Systems.Languages.LanguageUI;
using Content.Shared.Input;
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
using static Robust.Client.UserInterface.Controls.BaseButton;

namespace Content.Client._WL.UserInterface.Systems.Languages;

[UsedImplicitly]
public sealed partial class LanguagesUIController : UIController, IOnStateEntered<GameplayState>, IOnStateExited<GameplayState>, IOnSystemChanged<ClientLanguagesSystem>
{
    [Dependency] private IEntityManager _ent = default!;
    [Dependency] private IPrototypeManager _protoMan = default!;
    [Dependency] private IPlayerManager _player = default!;

    [UISystemDependency] private readonly ClientLanguagesSystem _languages = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeNetworkEvent<LanguagesChangedEvent>(OnLanguageChanged);
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
        system.OnLanguagesUpdate += UpdateLanguages;
        _player.LocalPlayerDetached += LanguagesDetached;
    }

    public void OnSystemUnloaded(ClientLanguagesSystem system)
    {
        system.OnLanguagesUpdate -= UpdateLanguages;
        _player.LocalPlayerDetached -= LanguagesDetached;
    }

    public void UnloadButton()
    {
        if (LanguagesButton is null)
            return;

        LanguagesButton.OnPressed -= LanguagesButtonPressed;
    }

    public void LoadButton()
    {
        if (LanguagesButton is null)
            return;

        LanguagesButton.OnPressed += LanguagesButtonPressed;
    }

    private void DeactivateButton()
    {
        if (LanguagesButton is null)
            return;

        LanguagesButton.Pressed = false;
    }

    private void ActivateButton()
    {
        if (LanguagesButton is null)
            return;

        LanguagesButton.Pressed = true;
    }

    private void ClearLanguages(EntityUid entity)
    {
        if (_window is null)
            return;

        if (_player.LocalEntity != entity)
            return;

        _window.Languages.RemoveAllChildren();
        _window.LangPlaceholder.Visible = true;
    }

    private void UpdateLanguages()
    {
        if (_window is null)
            return;

        if (_player.LocalEntity is not { } ent)
            return;

        if (!_ent.TryGetComponent<LanguagesComponent>(ent, out var comp)
                || comp.Languages.Count == 0)
        {
            ClearLanguages(ent);
            return;
        }

        _window.Languages.RemoveAllChildren();
        _window.LangPlaceholder.Visible = false;

        var languageControl = new LanguagesControl
        {
            Orientation = BoxContainer.LayoutOrientation.Vertical,
            Modulate = Color.White
        };

        var languageText = new FormattedMessage();
        languageText.TryAddMarkup(Loc.GetString("ui-languages-knowed-languages"), out _);

        var languageLabel = new RichTextLabel
        {
            StyleClasses = { StyleClass.TooltipTitle }
        };

        languageLabel.SetMessage(languageText);
        languageControl.AddChild(languageLabel);

        foreach (var (protoId, level) in comp.Languages)
        {
            if (level <= 0)
                continue;

            if (!_protoMan.TryIndex(protoId, out var language))
                continue;

            var languageItemControl = new LanguageItemControl(protoId, level);

            languageItemControl.SetButtonState(comp.CurrentLanguage == protoId);
            languageItemControl.OnChoosePressed += LanguageChange;

            languageControl.AddChild(languageItemControl);
            _window.LangPlaceholder.Visible = false;
        }

        _window.Languages.AddChild(languageControl);
    }

    private void OnLanguageChanged(LanguagesChangedEvent ev, EntitySessionEventArgs _)
    {
        UpdateLanguages();
    }

    private void OnMindRoleChanged(MindRoleTypeChangedEvent ev, EntitySessionEventArgs _)
    {
        UpdateLanguages();
    }

    public void LanguageChange(ProtoId<LanguagePrototype> language)
    {
        if (_player.LocalEntity is not {} ent)
            return;

        _languages.SetLanguage(ent, language);
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
        if (_window is null)
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
