using Godot;
using goldfish.Core.Data;
using Side = goldfish.Core.Data.Side;

namespace chessium.scripts;

/// <summary>
/// Represents a slot where taken pieces are placed.
/// </summary>
public partial class PieceSlot : Node2D
{
	/// <summary>
	/// The player who captured the piece.
	/// </summary>
	public Side player;
	
	/// <summary>
	/// The index of the captured piece.
	/// </summary>
	private int index;

	/// <summary>
	/// The sprite of the captured piece.
	/// </summary>
	private Sprite2D sprite;
	
	/// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		sprite = GetNode<Sprite2D>("Sprite2D");
		sprite.QueueFree();
		
		RemoveChild(sprite);
	}

	/// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		// to be implemented
	}

	/// <summary>
	/// Prepares the piece slots for a new game (removes all previously captured pieces).
	/// </summary>
	public void NewGame()
	{
		foreach (var child in GetChildren())
		{
			child.QueueFree();
			RemoveChild(child);
		}
	}

	/// <summary>
	/// Adds a piece to a piece slot.
	/// </summary>
	/// <param name="piece">The captured piece.</param>
	public void AddPiece(PieceType piece)
	{
		var pieceSprite = new Sprite2D();
		pieceSprite.Centered = false;
		pieceSprite.Position = pieceSprite.Position with { X = index * Constants.tileSize / 4.0f };
		pieceSprite.Texture = GD.Load<Texture2D>("res://assets/pieces.png");
		pieceSprite.Hframes = 6;
		pieceSprite.Vframes = 2;
		pieceSprite.ZIndex = 1000 + index;
		pieceSprite.FrameCoords = new Vector2I((int) piece, 1 - (int) player);

		index++;
		AddChild(pieceSprite);
	}
}
