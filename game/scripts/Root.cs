using Godot;
using goldfish.Core.Game;
using Side = goldfish.Core.Data.Side;

namespace chessium.scripts;

/// <summary>
/// Represents the main scene of the game (the chess board + other UI elements).
/// </summary>
public partial class Root : Node2D
{
	/// <summary>
	/// The chess board.
	/// </summary>
	private Board board;
	
	/// <summary>
	/// The other UI elements.
	/// </summary>
	public UI ui;

	/// <summary>
	/// The current player and winner, if any.
	/// </summary>
	public Side Player => board.state.ToMove;
	public Side? Winner => board.state.GetGameState();
	
	/// <summary>
	/// The current state of the game.
	/// </summary>
	public Constants.GameState gameState = Constants.GameState.GETTING_PIECE;

	/// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		board = GetNode<Board>("Board");
		ui = GetNode<UI>("UI");
		
		NewGame();
	}

	/// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		// to be implemented
	}

	/// <summary>
	/// Starts a new game.
	/// </summary>
	public void NewGame()
	{
		gameState = Constants.GameState.GETTING_PIECE;

		board.NewGame();
		ui.NewGame();

		foreach (var child in GetChildren())
		{
			if (child is Dialog)
			{
				RemoveChild(child);
				child.QueueFree();
			}
		}
	}

	public void SwitchPlayer()
	{
		ui.SetPlayer(Player);
	}

	/// <summary>
	/// Captures a piece and removes it from the board.
	/// </summary>
	/// <param name="piece">The piece to capture.</param>
	public void Capture(Piece piece)
	{
		ui.CapturePiece(piece);
	}
}
