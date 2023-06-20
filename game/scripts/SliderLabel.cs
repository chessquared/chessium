using Godot;

namespace chessium.scripts;

/// <summary>
/// Represents a label that appears when a slider is hovered over.
/// Converted from https://github.com/KoBeWi/Godot-Slider-Label.
/// </summary>
public partial class SliderLabel : Label
{
    private enum VisibilityRule
    {
        ON_CLICK, ON_HOVER, ON_FOCUS, ALWAYS
    }

    private enum Placement
    {
        TOP_RIGHT, BOTTOM_LEFT
    }

    private VisibilityRule visibilityRule = VisibilityRule.ON_HOVER;
    private Placement placement = Placement.BOTTOM_LEFT;
    private int separation = 8;
    private string customFormat = "";

    private Slider slider;
    private bool vertical;

    public override void _EnterTree()
    {
        if (!HasMeta("_edit_initialized"))
        {
            SetMeta("_edit_initialized", true);
            HorizontalAlignment = HorizontalAlignment.Center;
            VerticalAlignment = VerticalAlignment.Center;
            SizeFlagsHorizontal = SizeFlags.ShrinkCenter;
            Text = "100";
        }
    }

    public override void _Ready()
    {
        if (Engine.IsEditorHint())
        {
            return;
        }

        slider = GetParent<Slider>();

        if (slider is VSlider)
        {
            vertical = true;
        }

        slider.ValueChanged += UpdateWithDiscard;

        if (visibilityRule == VisibilityRule.ALWAYS)
        {
            Show();
        }
        else
        {
            Hide();

            switch (visibilityRule)
            {
                case VisibilityRule.ON_CLICK:
                    slider.GuiInput += OnSliderGuiInput;
                    break;
                
                case VisibilityRule.ON_HOVER:
                    slider.MouseEntered += () => OnSliderHoverFocus(true);
                    slider.MouseExited += () => OnSliderHoverFocus(false);
                    break;
                
                case VisibilityRule.ON_FOCUS:
                    slider.FocusEntered += () => OnSliderHoverFocus(true);
                    slider.FocusExited += () => OnSliderHoverFocus(false);
                    break;
            }
        }

        UpdateLabel();
    }

    private void OnSliderGuiInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton button)
        {
            Visible = button.Pressed;
            UpdateLabel();
        }
    }

    private void OnSliderHoverFocus(bool hover)
    {
        Visible = hover;
        UpdateLabel();
    }

    private void UpdateWithDiscard(double discard)
    {
        UpdateLabel();
    }

    private void UpdateLabel()
    {
        if (!IsVisibleInTree())
        {
            return;
        }

        Text = customFormat == "" ? slider.Value.ToString() : string.Format(customFormat, slider.Value);
        
        Hide();
        Show();

        Size = new Vector2();

        var grabberSize = slider.GetThemeIcon("Grabber").GetSize();
        if (vertical)
        {
            Position = Position with { Y = (float) (1 - slider.Ratio) * (slider.Size.Y - grabberSize.Y) + grabberSize.Y * 0.5f - Size.Y * 0.5f };
            if (placement == Placement.TOP_RIGHT)
            {
                Position = Position with { X = slider.Size.X + separation };
            }
            else
            {
                Position = Position with { X = -Size.X - separation };
            }
        }
        else
        {
            Position = Position with { X = (float) slider.Ratio * (slider.Size.X - grabberSize.X) + grabberSize.X * 0.5f - Size.X * 0.5f };
            if (placement == Placement.TOP_RIGHT)
            {
                Position = Position with { Y = -Size.Y - separation };
            }
            else
            {
                Position = Position with { Y = slider.Size.Y + separation };
            }
        }
    }
}