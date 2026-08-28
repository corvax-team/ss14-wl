using Robust.Client.Graphics;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Timing;

namespace Content.Client._WL.UserInterface.Systems.Computers;

public sealed class BlinkingCaret : PanelContainer
{
    public Color Color
    {
        get => _color;
        set
        {
            if (_color == value) return;
            _color = value;
            UpdateStyle();
        }
    }

    /// <summary>
    /// How long the caret stays visible, in milliseconds.
    /// </summary>
    public int VisibleDuration
    {
        get => _visibleDuration;
        set => _visibleDuration = Math.Max(1, value);
    }

    /// <summary>
    /// How long the caret stays hidden, in milliseconds.
    /// </summary>
    public int HiddenDuration
    {
        get => _hiddenDuration;
        set => _hiddenDuration = Math.Max(1, value);
    }

    /// <summary>
    /// Whether the blinking animation is active.
    /// </summary>
    public bool IsBlinkingEnabled
    {
        get => _isBlinkingEnabled;
        set
        {
            if (_isBlinkingEnabled == value)
                return;
            _isBlinkingEnabled = value;

            if (value)
            {
                _accumulator = 0f;
                UpdateVisibility();
            }
            else
            {
                Modulate = Modulate.WithAlpha(1f);
            }
        }
    }

    private float _accumulator;
    private int _visibleDuration = 500;
    private int _hiddenDuration = 500;
    private Color _color = Color.White;
    private bool _isBlinkingEnabled = true;

    private readonly StyleBoxFlat _styleBox;

    public BlinkingCaret()
    {
        _styleBox = new StyleBoxFlat
        {
            BackgroundColor = Color
        };

        PanelOverride = _styleBox;
        MinSize = new(10, 3);
        ReservesSpace = true;
    }

    protected override void FrameUpdate(FrameEventArgs args)
    {
        if (!IsBlinkingEnabled)
            return;

        _accumulator += args.DeltaSeconds * 1000f;
        UpdateVisibility();
    }

    private void UpdateVisibility()
    {
        if (!IsBlinkingEnabled)
            return;

        var totalCycle = VisibleDuration + HiddenDuration;
        var positionInCycle = _accumulator % totalCycle;

        var shouldBeVisible = positionInCycle < VisibleDuration;
        var currentAlpha = Modulate.A;

        if (shouldBeVisible && currentAlpha < 1f)
        {
            Modulate = Modulate.WithAlpha(1f);
        }
        else if (!shouldBeVisible && currentAlpha > 0f)
        {
            Modulate = Modulate.WithAlpha(0f);
        }
    }

    private void UpdateStyle()
    {
        _styleBox.BackgroundColor = Color;
    }

    /// <summary>
    /// Resets the blinking animation cycle.
    /// </summary>
    public void ResetAnimation()
    {
        _accumulator = 0f;
        UpdateVisibility();
    }
}
