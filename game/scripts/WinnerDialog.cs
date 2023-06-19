using Godot;
using Side = goldfish.Core.Data.Side;

namespace chessium.scripts;

/// <summary>
/// Represents the dialog that shows up when a player has won.
/// </summary>
public partial class WinnerDialog : Dialog
{
	/// <summary>
	/// The width and height of this dialog.
	/// </summary>
	public const int winnerWidth = 54 * 2, winnerHeight = 32 * 2;
	
	/// <summary>
	/// The sprite of the player who won, as well as the winner message.
	/// </summary>
	private Sprite2D playerSprite = new (), winnerSprite = new ();
	
	/// <summary>
	/// The player who won.
	/// </summary>
	private Side player;

	/// <summary>
	/// Constructs a new WinnerDialog.
	/// </summary>
	/// <param name="winner">The player that won.</param>
	public WinnerDialog(Side winner) : base(winnerWidth, winnerHeight)
	{
		player = winner;
	}
	
	/// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		base._Ready();
		ZIndex = 2000;

		ConfigureSprite(playerSprite);
		ConfigureSprite(winnerSprite);

		playerSprite.FrameCoords = playerSprite.FrameCoords with { Y = (int) player };
		winnerSprite.FrameCoords = winnerSprite.FrameCoords with { Y = 2 };

		var y = winnerSprite.Position.Y;
		winnerSprite.Position = winnerSprite.Position with { Y = y + 32 };
	}

	/// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		// to be implemented
	}

	/// <summary>
	/// Configures a sprite for this dialog.
	/// </summary>
	/// <param name="sprite">The sprite to configure.</param>
	private void ConfigureSprite(Sprite2D sprite)
	{
		sprite.Texture = GD.Load<Texture2D>("res://assets/player.png");
		sprite.Centered = false;
		sprite.Hframes = 1;
		sprite.Vframes = 4;
		sprite.ZIndex = 2010;
		sprite.Position = new Vector2(size, size);
		
		AddChild(sprite);
	}
}
