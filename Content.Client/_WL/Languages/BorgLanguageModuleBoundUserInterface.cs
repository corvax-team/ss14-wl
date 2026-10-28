using System.Linq;
using Content.Client.UserInterface.Controls;
using Content.Shared._WL.Languages;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Prototypes;

namespace Content.Client._WL.Languages;

public sealed class BorgLanguageModuleBoundUserInterface(EntityUid owner, Enum uiKey) : BoundUserInterface(owner, uiKey)
{
    private BorgLanguageModuleWindow? _window;

    protected override void Open()
    {
        base.Open();
        _window = this.CreateWindow<BorgLanguageModuleWindow>();
        _window.Confirmed += languages => SendMessage(new BorgLanguageModuleConfirmMessage(languages));
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        if (state is BorgLanguageModuleState card)
            _window?.UpdateState(card);
    }
}

public sealed partial class BorgLanguageModuleWindow : FancyWindow
{
    [Dependency] private IPrototypeManager _prototypes = default!;
    private readonly BoxContainer _languages;
    private readonly Label _notice;
    private readonly Label _warning;
    private readonly Button _confirm;
    private readonly Dictionary<ProtoId<LanguagePrototype>, CheckBox> _choices = new();
    private bool _configured;
    public event Action<List<ProtoId<LanguagePrototype>>>? Confirmed;

    public BorgLanguageModuleWindow()
    {
        IoCManager.InjectDependencies(this);
        Title = Loc.GetString("borg-language-module-title");
        MinWidth = 420;
        MinHeight = 260;
        SetHeight = 520;
        var content = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Vertical,
            SeparationOverride = 8,
            VerticalExpand = true,
            Margin = new Thickness(12),
        };
        ContentsContainer.AddChild(content);
        _notice = new Label { HorizontalAlignment = HAlignment.Center };
        content.AddChild(_notice);
        var scroll = new ScrollContainer
        {
            VerticalExpand = true,
        };
        content.AddChild(scroll);
        _languages = new BoxContainer { Orientation = BoxContainer.LayoutOrientation.Vertical };
        scroll.AddChild(_languages);
        _warning = new Label
        {
            Text = Loc.GetString("borg-language-module-warning"),
            FontColorOverride = Color.Red,
            HorizontalAlignment = HAlignment.Center,
        };
        content.AddChild(_warning);
        _confirm = new Button { Text = Loc.GetString("borg-language-module-confirm"), Disabled = true };
        _confirm.OnPressed += _ => Confirmed?.Invoke(Selected());
        content.AddChild(_confirm);
    }

    private List<ProtoId<LanguagePrototype>> Selected() => _choices.Where(pair => pair.Value.Pressed).Select(pair => pair.Key).ToList();

    public void UpdateState(BorgLanguageModuleState state)
    {
        _configured = state.Configured;
        _choices.Clear();
        _languages.RemoveAllChildren();
        foreach (var id in state.Available.OrderBy(id => Loc.GetString(_prototypes.Index(id).Name)))
        {
            var language = _prototypes.Index(id);
            var check = new CheckBox
            {
                Text = Loc.GetString(language.Name),
                ToolTip = Loc.GetString(language.Description),
                Pressed = state.Selected.Contains(id),
            };
            _choices.Add(id, check);
            check.OnToggled += _ => RefreshChoices();
            _languages.AddChild(check);
        }
        RefreshChoices();
    }

    private void RefreshChoices()
    {
        var count = Selected().Count;
        foreach (var choice in _choices.Values)
        {
            choice.Disabled = _configured || !choice.Pressed && count >= BorgLanguageModuleComponent.MaxLanguages;
            choice.Modulate = choice.Disabled ? Color.White.WithAlpha(0.55f) : Color.White;
        }
        _warning.Visible = !_configured;
        _confirm.Visible = !_configured;
        _confirm.Disabled = _configured || count is < 1 or > BorgLanguageModuleComponent.MaxLanguages;
        _notice.Text = _configured ? Loc.GetString("borg-language-module-configured") :
            Loc.GetString("borg-language-module-count", ("count", count), ("max", BorgLanguageModuleComponent.MaxLanguages));
    }
}
