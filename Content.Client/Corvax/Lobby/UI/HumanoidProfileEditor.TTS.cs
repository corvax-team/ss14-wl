using Content.Client.ADT.Bark;
using Content.Client.Corvax.Barks;
using Content.Client.Corvax.TTS;
using Content.Shared.Corvax.Barks;
using Content.Shared.Corvax.CCCVars;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;

namespace Content.Client.Lobby.UI;

public sealed partial class HumanoidProfileEditor
{
    private TTSTab? _ttsTab;
    private BarkTab? _barkTab;
    private OptionButton? _speechModeButton;

    private void RefreshVoiceTab()
    {
        _ttsTab = new TTSTab();
        _barkTab = new BarkTab();
        var speechTabs = new TabContainer
        {
            HorizontalExpand = true,
            VerticalExpand = true,
            Margin = new Thickness(5, 0, 5, 5),
        };
        speechTabs.AddChild(_ttsTab);
        speechTabs.AddChild(_barkTab);
        speechTabs.SetTabTitle(0, Loc.GetString("ui-options-speech-mode-tts"));
        speechTabs.SetTabTitle(1, Loc.GetString("ui-options-speech-mode-barks"));

        _speechModeButton = new OptionButton
        {
            HorizontalAlignment = HAlignment.Right,
            MinWidth = 180,
        };
        _speechModeButton.AddItem(Loc.GetString("ui-options-speech-mode-tts"), (int) SpeechMode.Tts);
        _speechModeButton.AddItem(Loc.GetString("ui-options-speech-mode-barks"), (int) SpeechMode.Barks);
        _speechModeButton.AddItem(Loc.GetString("ui-options-speech-mode-disabled"), (int) SpeechMode.Disabled);
        _speechModeButton.SelectId((int) _cfgManager.GetCVar(CCCVars.SpeechMode));
        _speechModeButton.OnItemSelected += args =>
        {
            _speechModeButton.SelectId(args.Id);
            _cfgManager.SetCVar(CCCVars.SpeechMode, (SpeechMode) args.Id);
        };

        var modeText = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Vertical,
            HorizontalExpand = true,
        };
        modeText.AddChild(new Label
        {
            Text = Loc.GetString("ui-options-speech-mode"),
            StyleClasses = { "LabelHeading" },
        });
        modeText.AddChild(new Label
        {
            Text = Loc.GetString("humanoid-profile-editor-speech-mode-description"),
        });

        var modeRow = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Horizontal,
            HorizontalExpand = true,
            Margin = new Thickness(10, 7),
            VerticalAlignment = VAlignment.Center,
        };
        modeRow.AddChild(modeText);
        modeRow.AddChild(_speechModeButton);

        var voiceContainer = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Vertical,
            HorizontalExpand = true,
            VerticalExpand = true,
        };
        voiceContainer.AddChild(modeRow);
        voiceContainer.AddChild(speechTabs);

        var children = new List<Control>();
        foreach (var child in TabContainer.Children)
            children.Add(child);

        TabContainer.RemoveAllChildren();

        for (int i = 0; i < children.Count; i++)
        {
            if (i == 1) // Set the tab to the 2nd place.
            {
                TabContainer.AddChild(voiceContainer);
            }
            TabContainer.AddChild(children[i]);
        }

        TabContainer.SetTabTitle(1, Loc.GetString("humanoid-profile-editor-voice-tab"));

        _ttsTab.OnVoiceSelected += voiceId =>
        {
            SetVoice(voiceId);
            _ttsTab.SetSelectedVoice(voiceId);
        };

        _ttsTab.OnPreviewRequested += voiceId =>
        {
            _entManager.System<TTSSystem>().RequestPreviewTTS(voiceId, _ttsTab.PreviewTextEdit.Text);
        };

        _barkTab.OnBarkSelected += barkId =>
        {
            Profile = Profile?.WithBarkVoice(barkId);
            SetDirty();
        };
        _barkTab.OnPitchChanged += pitch =>
        {
            Profile = Profile?.WithBarkPitch(pitch);
            SetDirty();
        };
        _barkTab.OnMinVarChanged += delay =>
        {
            Profile = Profile?.WithBarkMinDelay(delay);
            SetDirty();
        };
        _barkTab.OnMaxVarChanged += delay =>
        {
            Profile = Profile?.WithBarkMaxDelay(delay);
            SetDirty();
        };
    }

    private void UpdateTTSVoicesControls()
    {
        if (Profile is null || _ttsTab is null)
            return;

        _ttsTab.UpdateControls(Profile, Profile.Sex);
        _ttsTab.SetSelectedVoice(Profile.TTSVoice);
        _barkTab?.SetSelectedBark(
            Profile.BarkVoice,
            Profile.BarkPitch,
            Profile.BarkMinDelay,
            Profile.BarkMaxDelay);
    }
    private void SetVoice(string newVoice)
    {
        Profile = Profile?.WithVoice(newVoice);
        IsDirty = true;
    }
}
