using Content.Shared.VoiceMask;
using Content.Shared._WL.Barks; // WL-Changes
using Robust.Client.GameObjects;
using Robust.Client.UserInterface;
using Robust.Shared.Prototypes;

namespace Content.Client.VoiceMask;

public sealed partial class VoiceMaskBoundUserInterface : BoundUserInterface
{
    [Dependency] private IPrototypeManager _protomanager = default!;

    [ViewVariables]
    private VoiceMaskNameChangeWindow? _window;

    public VoiceMaskBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        base.Open();

        _window = this.CreateWindow<VoiceMaskNameChangeWindow>();
        _window.ReloadVerbs(_protomanager);
        _window.AddVerbs();

        _window.OnNameChange += OnNameSelected;
        _window.OnVerbChange += verb => SendMessage(new VoiceMaskChangeVerbMessage(verb));
        _window.OnToggle += OnToggle;
        _window.OnAccentToggle += OnAccentToggle;
        _window.OnVoiceChange += voice => SendMessage(new VoiceMaskChangeVoiceMessage(voice)); // Corvax-TTS
        // WL-Changes-Start: Speech barks
        _window.OnBarkChange += bark => SendMessage(new VoiceMaskChangeBarkMessage(bark));
        _window.OnBarkPitchChange += pitch => SendMessage(new VoiceMaskChangeBarkPitchMessage(pitch));
        // WL-Changes-End
    }

    private void OnNameSelected(string name)
    {
        SendMessage(new VoiceMaskChangeNameMessage(name));
    }

    private void OnToggle()
    {
        SendMessage(new VoiceMaskToggleMessage());
    }

    private void OnAccentToggle()
    {
        SendMessage(new VoiceMaskAccentToggleMessage());
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        if (state is not VoiceMaskBuiState cast || _window == null)
        {
            return;
        }

        // WL-Changes-Start: Speech barks
        _window.UpdateState(
            cast.Name,
            cast.Verb,
            cast.Active,
            cast.AccentHide,
            cast.TitleText,
            cast.TTSVoice,
            cast.BarkVoice,
            cast.BarkPitch);
        // WL-Changes-End
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        _window?.Close();
    }
}
