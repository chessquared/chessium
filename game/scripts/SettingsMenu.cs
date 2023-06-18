using Godot;

namespace chessium.scripts;

/// <summary>
/// Represents the settings menu in the game.
/// </summary>
public partial class SettingsMenu : Dialog
{
    /// <summary>
    /// The width and height of this dialog.
    /// </summary>
    private static readonly int settingsWidth = 606, settingsHeight = 448;

    /// <summary>
    /// The section for the credits.
    /// </summary>
    private Dialog credits = new (522, 132);
    private Label titleLabel = new (), creditsLabel = new (), creditsTextLabel = new ();

    /// <summary>
    /// The button to close the settings menu.
    /// </summary>
    private Dialog close = new (64, 16);
    private Button closeButton = new ();

    /// <summary>
    /// The buttons to enable options such as board flipping, using the engine and various engine options.
    /// </summary>
    private Dialog boardFlip = new (144, 16), engineStart = new (190, 16), useEngine = new (144, 16);
    private Button boardFlipButton = new (), engineStartButton = new (), useEngineButton = new ();
    
    private Dialog allottedTime = new (166, 48), depth = new (166, 48);
    private Label allottedTimeLabel = new (), depthLabel = new ();
    private HSlider allottedTimeSlider = new (), depthSlider = new ();
    
    /// <summary>
    /// The font for labels.
    /// </summary>
    private FontFile font = GD.Load<FontFile>("res://assets/CooperBits.ttf");

    /// <summary>
    /// Constructs a new SettingsMenu.
    /// </summary>
    public SettingsMenu() : base(settingsWidth, settingsHeight)
    {
        // empty
    }

    /// <summary>
    /// Called when the node enters the scene tree for the first time.
    /// </summary>
    public override void _Ready()
    {
        base._Ready();
        ZIndex = 2000;
        
        titleLabel.AddThemeColorOverride("font_color", new Color(1, 1, 1));
        titleLabel.AddThemeFontOverride("font", font);
        titleLabel.AddThemeFontSizeOverride("font_size", 40);

        titleLabel.Text = "SETTINGS";
        titleLabel.Position = new Vector2(settingsWidth / 3.0f, size + size / 2.0f + 2);
        titleLabel.ZIndex = 2001;
        
        AddChild(titleLabel);

        closeButton.AddThemeColorOverride("font_color", new Color(1, 1, 1));
        closeButton.AddThemeFontOverride("font", font);
        closeButton.AddThemeFontSizeOverride("font_size", 20);
        
        closeButton.Text = "CLOSE";
        closeButton.TooltipText = "Close this settings menu?";
        closeButton.Position = new Vector2(size / 2.0f, size / 2.0f + 2);
        closeButton.Pressed += QueueFree;

        close.ZIndex = 2001;
        close.Position = new Vector2(settingsWidth - size - 64, size);
        
        close.AddChild(closeButton);
        AddChild(close);

        boardFlipButton.AddThemeColorOverride("font_color", new Color(1, 1, 1));
        boardFlipButton.AddThemeFontOverride("font", font);
        boardFlipButton.AddThemeFontSizeOverride("font_size", 20);
        
        boardFlipButton.Text = "FLIP BOARD?";
        boardFlipButton.TooltipText = "Should the board flip after every turn?";
        boardFlipButton.Position = new Vector2(size - size / 2.0f + 2, size - 5);
        boardFlipButton.Pressed += OnBoardFlipSelected;

        boardFlip.ZIndex = 2001;
        boardFlip.Position = new Vector2(size * 3, size * 5);

        boardFlip.AddChild(boardFlipButton);
        AddChild(boardFlip);
        
        useEngineButton.AddThemeColorOverride("font_color", new Color(1, 1, 1));
        useEngineButton.AddThemeFontOverride("font", font);
        useEngineButton.AddThemeFontSizeOverride("font_size", 20);
        
        useEngineButton.Text = "USE ENGINE?";
        useEngineButton.TooltipText = "Play against the engine or another human?";
        useEngineButton.Position = new Vector2(size - size / 2.0f + 2, size - 5);
        useEngineButton.Pressed += OnUseEngineSelected;

        useEngine.ZIndex = 2001;
        useEngine.Position = new Vector2(size * 3, size * 10);
        
        useEngine.AddChild(useEngineButton);
        AddChild(useEngine);
        
        engineStartButton.AddThemeColorOverride("font_color", new Color(1, 1, 1));
        engineStartButton.AddThemeFontOverride("font", font);
        engineStartButton.AddThemeFontSizeOverride("font_size", 20);
        
        engineStartButton.Text = "ENGINE STARTS?";
        engineStartButton.TooltipText = "Should the engine start first?";
        engineStartButton.Disabled = true;
        engineStartButton.Position = new Vector2(size - size / 2.0f + 2, size - 5);
        engineStartButton.Pressed += OnEngineStartSelected;

        engineStart.ZIndex = 2001;
        engineStart.Position = new Vector2(size * 3, size * 15);
        
        engineStart.AddChild(engineStartButton);
        AddChild(engineStart);

        allottedTimeLabel.AddThemeColorOverride("font_color", new Color(1, 1, 1));
        allottedTimeLabel.AddThemeFontOverride("font", font);
        allottedTimeLabel.AddThemeFontSizeOverride("font_size", 20);
        
        allottedTimeLabel.Text = "ALLOTED TIME";
        allottedTimeLabel.Position = new Vector2(size, size);
        allottedTimeLabel.ZIndex = 2002;
        
        allottedTimeSlider.ZIndex = 2002;
        allottedTimeSlider.MinValue = 15.0;
        allottedTimeSlider.MaxValue = 60.0;
        allottedTimeSlider.Step = 5.0;
        allottedTimeSlider.Value = 20.0;
        allottedTimeSlider.TickCount = 10;
        allottedTimeSlider.TicksOnBorders = true;
        allottedTimeSlider.TooltipText = "How many seconds should the engine think for?";
        allottedTimeSlider.Editable = false;
        allottedTimeSlider.Size = new Vector2(166, 32);
        allottedTimeSlider.Position = new Vector2(size, size * 2);
        allottedTimeSlider.ValueChanged += OnAllottedTimeChanged;
        
        allottedTime.ZIndex = 2001;
        allottedTime.Position = new Vector2(settingsWidth / 1.5f, size * 5);

        allottedTime.AddChild(allottedTimeLabel);
        allottedTime.AddChild(allottedTimeSlider);
        AddChild(allottedTime);
        
        depthLabel.AddThemeColorOverride("font_color", new Color(1, 1, 1));
        depthLabel.AddThemeFontOverride("font", font);
        depthLabel.AddThemeFontSizeOverride("font_size", 20);
        
        depthLabel.Text = "ENGINE DEPTH";
        depthLabel.Position = new Vector2(size, size);
        depthLabel.ZIndex = 2002;
        
        depthSlider.MinValue = 1.0;
        depthSlider.MaxValue = 6.0;
        depthSlider.Step = 1.0;
        depthSlider.Value = 5.0;
        depthSlider.TickCount = 6;
        depthSlider.TicksOnBorders = true;
        depthSlider.TooltipText = "How much depth should the engine search through?";
        depthSlider.Editable = false;
        depthSlider.Size = new Vector2(166, 32);
        depthSlider.Position = new Vector2(size, size * 2);
        depthSlider.ValueChanged += OnDepthChanged;

        depth.ZIndex = 2001;
        depth.Position = new Vector2(settingsWidth / 1.5f, size * 13);
        
        depth.AddChild(depthLabel);
        depth.AddChild(depthSlider);
        AddChild(depth);
        
        creditsLabel.AddThemeColorOverride("font_color", new Color(1, 1, 1));
        creditsLabel.AddThemeFontOverride("font", font);
        creditsLabel.AddThemeFontSizeOverride("font_size", 40);

        creditsLabel.Text = "CREDITS";
        creditsLabel.Position = new Vector2(settingsWidth / 3.0f - size * 2, size);
        creditsLabel.ZIndex = 2002;
        
        creditsTextLabel.AddThemeColorOverride("font_color", new Color(1, 1, 1));
        creditsTextLabel.AddThemeFontOverride("font", font);
        creditsTextLabel.AddThemeFontSizeOverride("font_size", 20);

        creditsTextLabel.Text = "GUI ~~~ Timothy Ivtchenko\n    Engine ~~~ Adam Chen";
        creditsTextLabel.Position = new Vector2(settingsWidth / 4.7f, size * 5);
        creditsTextLabel.ZIndex = 2002;

        credits.ZIndex = 2001;
        credits.Position = new Vector2(size * 3, size * 19);
        
        credits.AddChild(creditsLabel);
        credits.AddChild(creditsTextLabel);
        AddChild(credits);
    }

    /// <summary>
    /// Called when the node is about to leave the SceneTree.
    /// </summary>
    public override void _ExitTree()
    {
        foreach (var child in GetChildren())
        {
            child.QueueFree();
        }
    }

    /// <summary>
    /// An event handler for when the board flip button is clicked.
    /// </summary>
    private void OnBoardFlipSelected()
    {
        Constants.flipBoard = !Constants.flipBoard;
    }

    /// <summary>
    /// An event handler for when the engine start button is clicked.
    /// </summary>
    private void OnEngineStartSelected()
    {
        if (!Constants.isEngineRequested)
        {
            return;
        }
        
        Constants.engineStarts = !Constants.engineStarts;
    }

    /// <summary>
    /// An event handler for when the use engine button is clicked.
    /// </summary>
    private void OnUseEngineSelected()
    {
        engineStartButton.Disabled = Constants.isEngineRequested;
        allottedTimeSlider.Editable = !Constants.isEngineRequested;
        depthSlider.Editable = !Constants.isEngineRequested;
        
        Constants.isEngineRequested = !Constants.isEngineRequested;
    }

    /// <summary>
    /// The event handler for when the allotted time slider has its value changed.
    /// </summary>
    /// <param name="value"></param>
    private void OnAllottedTimeChanged(double value)
    {
        Constants.engineAllottedTime = (int) value;
    }

    /// <summary>
    /// The event handler for when the engine depth slider has its value changed.
    /// </summary>
    /// <param name="value"></param>
    private void OnDepthChanged(double value)
    {
        Constants.engineDepth = (int) value;
    }
}