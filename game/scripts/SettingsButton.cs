using Godot;

namespace chessium.scripts;

public partial class SettingsButton : Dialog
{
	/// <summary>
	/// Whether or not the mouse is currently hovering over the button.
	/// </summary>
	private bool mouseIn, lastMouseIn;

	/// <summary>
	/// The text to display on the button.
	/// </summary>
	private Label text;

	/// <summary>
	/// The font for the text to display.
	/// </summary>
	private FontFile font = GD.Load<FontFile>("res://assets/CooperBits.ttf");
    
    /// <summary>
    /// The width and height of the button.
    /// </summary>
    public const int settingsWidth = 108, settingsHeight = 32;

    /// <summary>
    /// Constructs a new SettingsButton.
    /// </summary>
    public SettingsButton() : base(settingsWidth, settingsHeight)
    {
        // empty
    }

    /// Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
    	base._Ready();
    	text = new Label();

        text.AddThemeColorOverride("font_color", new Color(1, 1, 1));
        text.AddThemeFontOverride("font", font);
        text.AddThemeFontSizeOverride("font_size", 20);
        
        text.Text = "SETTINGS";
    	text.Position = new Vector2(size, size + 7);
    	
    	AddChild(text);
    }

    /// <summary>
    /// Handles user input for the button.
    /// </summary>
    /// <param name="event">The input event.</param>
    public override void _Input(InputEvent @event)
    {
    	if (@event is InputEventMouseButton { Pressed: false, ButtonIndex: MouseButton.Left } && mouseIn)
        {
	        var settings = new SettingsMenu();
	        GetNode("/root/Root").AddChild(settings);
        }
    }

    /// Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    	var mouse = GetLocalMousePosition();
    	mouse.X -= size;
    	mouse.Y -= size;

    	// are we hovering over the button?
    	if (mouse.X is >= 0 and <= settingsWidth && mouse.Y is >= 0 and <= settingsHeight)
    	{
    		mouseIn = true;
    	}
    	else
    	{
    		mouseIn = false;
    	}

    	if (mouseIn != lastMouseIn)
    	{
    		QueueRedraw();
    	}

    	lastMouseIn = mouseIn;
    }

    /// <summary>
    /// Draws a red rectangle to signify the button being hovered over.
    /// </summary>
    public override void _Draw()
    {
    	if (mouseIn)
    	{
    		// draw the red rectangle when hovered over
    		const float offset = size * 0.5f;
    		var vec1 = new Vector2(offset, offset);
    		var vec2 = new Vector2(settingsWidth + size, settingsHeight + size);
    		
    		DrawRect(new Rect2(vec1, vec2), new Color(1, 0, 0));
    	}
    }
}