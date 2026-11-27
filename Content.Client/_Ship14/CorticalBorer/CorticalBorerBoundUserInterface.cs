using Content.Shared._Ship14.CorticalBorer;
using Robust.Client.UserInterface.Controls;
using Robust.Client.GameObjects;
using Robust.Client.UserInterface;
using Content.Client.UserInterface.Controls;
using Robust.Shared.Utility;

namespace Content.Client._Ship14.CorticalBorer;

public sealed partial class CorticalBorerBoundUserInterface(EntityUid owner, Enum uiKey) : BoundUserInterface(owner, uiKey)
{
    private CorticalBorerWindow? _window;
    private SimpleRadialMenu? _menu;

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        if (state is not CorticalBorerBuiState borerState)
            return;

        switch (borerState.Mode)
        {
            case CorticalBorerMenuMode.Converse:
                EnsureWindow();
                CloseMenu();
                _window?.UpdateState(borerState);
                break;
            case CorticalBorerMenuMode.Punish:
                CloseWindow();
                EnsureMenu();
                _menu?.SetButtons(CreatePunishmentButtons());
                _menu?.OpenCentered();
                break;
            case CorticalBorerMenuMode.Inject:
                CloseWindow();
                EnsureMenu();
                _menu?.SetButtons(CreateInjectionButtons());
                _menu?.OpenCentered();
                break;
        }
    }

    private void EnsureWindow()
    {
        if (_window != null)
            return;

        _window = new CorticalBorerWindow();
        _window.MessageSubmitted += message => SendMessage(new CorticalBorerConverseBuiMessage(message));
        _window.OnClose += Close;
        _window.OpenCentered();
    }

    private void EnsureMenu()
    {
        if (_menu != null)
            return;

        _menu = new SimpleRadialMenu();
        _menu.OnClose += Close;
    }

    private IEnumerable<RadialMenuOptionBase> CreatePunishmentButtons()
    {
        return
        [
            new RadialMenuActionOption<CorticalBorerPunishment>(SendPunishment, CorticalBorerPunishment.Agony)
            {
                IconSpecifier = RadialMenuIconSpecifier.With(new SpriteSpecifier.Rsi(new ResPath("/Textures/_Ship14/Interface/Radial/cortical_borer.rsi"), "agony")),
                ToolTip = Loc.GetString("cortical-borer-punishment-agony")
            },
            new RadialMenuActionOption<CorticalBorerPunishment>(SendPunishment, CorticalBorerPunishment.Blindness)
            {
                IconSpecifier = RadialMenuIconSpecifier.With(new SpriteSpecifier.Rsi(new ResPath("/Textures/_Ship14/Interface/Radial/cortical_borer.rsi"), "blindness")),
                ToolTip = Loc.GetString("cortical-borer-punishment-blindness")
            },
            new RadialMenuActionOption<CorticalBorerPunishment>(SendPunishment, CorticalBorerPunishment.Paralysis)
            {
                IconSpecifier = RadialMenuIconSpecifier.With(new SpriteSpecifier.Rsi(new ResPath("/Textures/_Ship14/Interface/Radial/cortical_borer.rsi"), "paralysis")),
                ToolTip = Loc.GetString("cortical-borer-punishment-paralysis")
            },
        ];
    }

    private IEnumerable<RadialMenuOptionBase> CreateInjectionButtons()
    {
        return
        [
            new RadialMenuActionOption<CorticalBorerInjection>(SendInjection, CorticalBorerInjection.Epinephrine)
            {
                IconSpecifier = RadialMenuIconSpecifier.With(new SpriteSpecifier.Rsi(new ResPath("/Textures/_Ship14/Interface/Radial/cortical_borer.rsi"), "epinephrine")),
                ToolTip = Loc.GetString("reagent-name-epinephrine")
            },
            new RadialMenuActionOption<CorticalBorerInjection>(SendInjection, CorticalBorerInjection.SpaceDrugs)
            {
                IconSpecifier = RadialMenuIconSpecifier.With(new SpriteSpecifier.Rsi(new ResPath("/Textures/_Ship14/Interface/Radial/cortical_borer.rsi"), "space-drugs")),
                ToolTip = Loc.GetString("reagent-name-space-drugs")
            },
            new RadialMenuActionOption<CorticalBorerInjection>(SendInjection, CorticalBorerInjection.Tricordrazine)
            {
                IconSpecifier = RadialMenuIconSpecifier.With(new SpriteSpecifier.Rsi(new ResPath("/Textures/_Ship14/Interface/Radial/cortical_borer.rsi"), "tricordrazine")),
                ToolTip = Loc.GetString("reagent-name-tricordrazine")
            },
            new RadialMenuActionOption<CorticalBorerInjection>(SendInjection, CorticalBorerInjection.Ethanol)
            {
                IconSpecifier = RadialMenuIconSpecifier.With(new SpriteSpecifier.Rsi(new ResPath("/Textures/_Ship14/Interface/Radial/cortical_borer.rsi"), "ethanol")),
                ToolTip = Loc.GetString("reagent-name-ethanol")
            },
            new RadialMenuActionOption<CorticalBorerInjection>(SendInjection, CorticalBorerInjection.Leporazine)
            {
                IconSpecifier = RadialMenuIconSpecifier.With(new SpriteSpecifier.Rsi(new ResPath("/Textures/_Ship14/Interface/Radial/cortical_borer.rsi"), "leporazine")),
                ToolTip = Loc.GetString("reagent-name-leporazine")
            },
            new RadialMenuActionOption<CorticalBorerInjection>(SendInjection, CorticalBorerInjection.Desoxyephedrine)
            {
                IconSpecifier = RadialMenuIconSpecifier.With(new SpriteSpecifier.Rsi(new ResPath("/Textures/_Ship14/Interface/Radial/cortical_borer.rsi"), "desoxyephedrine")),
                ToolTip = Loc.GetString("reagent-name-desoxyephedrine")
            },
            new RadialMenuActionOption<CorticalBorerInjection>(SendInjection, CorticalBorerInjection.Dexalin)
            {
                IconSpecifier = RadialMenuIconSpecifier.With(new SpriteSpecifier.Rsi(new ResPath("/Textures/_Ship14/Interface/Radial/cortical_borer.rsi"), "dexalin")),
                ToolTip = Loc.GetString("reagent-name-dexalin")
            },
        ];
    }

    private void SendPunishment(CorticalBorerPunishment punishment)
    {
        SendMessage(new CorticalBorerPunishBuiMessage(punishment));
    }

    private void SendInjection(CorticalBorerInjection injection)
    {
        SendMessage(new CorticalBorerInjectBuiMessage(injection));
    }

    private void CloseWindow()
    {
        if (_window == null)
            return;

        _window.OnClose -= Close;
        _window.Close();
        _window = null;
    }

    private void CloseMenu()
    {
        if (_menu == null)
            return;

        _menu.OnClose -= Close;
        _menu.Close();
        _menu = null;
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (!disposing)
            return;

        CloseWindow();
        CloseMenu();
    }
}
