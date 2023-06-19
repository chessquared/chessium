using System.Collections.Generic;
using System.Threading;
using Godot;
using goldfish.Core.Data;
using Side = goldfish.Core.Data.Side;

namespace chessium.scripts;

/// <summary>
/// Represents the dialog that shows up when a pawn is promoting.
/// </summary>
public partial class PromotionDialog : Dialog
{
	/// <summary>
	/// The width and height of this dialog.
	/// </summary>
	public const float promotionWidth = Constants.tileSize * 4.0f;
	public const float promotionHeight = Constants.tileSize;

	public readonly SemaphoreSlim selectionCallback = new (0);
	
	/// <summary>
	/// All possible choices for promotion.
	/// </summary>
	private readonly List<PieceType> pieces = new () { PieceType.Rook, PieceType.Knight, PieceType.Bishop, PieceType.Queen };

	/// <summary>
	/// The position of the user's mouse.
	/// </summary>
	private readonly Vector2 invalidPosition = new (-1, -1);
	private Vector2 mousePosition, lastMousePosition;

	/// <summary>
	/// The player who owns the promoting pawn.
	/// </summary>
	private Side player;

	/// <summary>
	/// The piece to promote to.
	/// </summary>
	public PieceType? selectedPiece;

	/// <summary>
	/// Constructs a new PromotionDialog.
	/// </summary>
	/// <param name="player">The player who owns the promoting pawn.</param>
	public PromotionDialog(Side player) : base((int) promotionWidth, (int) promotionHeight)
	{
		this.player = player;
	}
	
	/// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		base._Ready();
		ZIndex = 2000;
		
		mousePosition = invalidPosition;
		lastMousePosition = invalidPosition;
		selectedPiece = null;

		// adds the possible choices to the dialog
		var i = 0;
		foreach (var piece in pieces)
		{
			var sprite = new Sprite2D();
			sprite.Texture = GD.Load<Texture2D>("res://assets/pieces.png");
			sprite.Hframes = 6;
			sprite.Vframes = 2;
			sprite.FrameCoords = new Vector2I((int) piece, 1 - (int) player);
			sprite.Position = new Vector2(i * Constants.tileSize + size, size);
			sprite.ZIndex = 2010;
			sprite.Centered = false;

			i++;
			AddChild(sprite);
		}
	}

	/// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		var mouse = GetLocalMousePosition();
		mouse.X -= size;
		mouse.Y -= size;

		mouse.X = Mathf.Floor(mouse.X / Constants.tileSize);
		mouse.Y = Mathf.Floor(mouse.Y / Constants.tileSize);

		if (mouse.X < 0 || mouse.X >= 4 || mouse.Y != 0)
		{
			mouse = invalidPosition;
		}

		// record the user's mouse position
		mousePosition = mouse;

		if (mouse != lastMousePosition)
		{
			QueueRedraw();
		}

		lastMousePosition = mouse;
	}

	/// <summary>
	/// Draws a border around the piece to be selected.
	/// </summary>
	public override void _Draw()
	{
		if (mousePosition != invalidPosition)
		{
			var vec1 = new Vector2(size + Constants.tileSize * mousePosition.X, size);
			var vec2 = new Vector2(Constants.tileSize, Constants.tileSize);
			
			DrawRect(new Rect2(vec1, vec2), new Color(1, 0, 0));
		}
	}

	/// <summary>
	/// Handles user input.
	/// </summary>
	/// <param name="event">The input event.</param>
	public override void _Input(InputEvent @event)
	{
		if(@event is InputEventMouseButton && mousePosition != invalidPosition)
		{
			selectedPiece = pieces[(int) mousePosition.X];
			selectionCallback.Release(1);
		}
	}
}
