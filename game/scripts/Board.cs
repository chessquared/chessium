using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using goldfish.Core.Data;
using goldfish.Core.Game;
using goldfish.Engine.Searcher;
using Environment = System.Environment;
using Side = goldfish.Core.Data.Side;

namespace chessium.scripts;

/// <summary>
/// Represents the chess board.
/// </summary>
public partial class Board : Node2D
{
	/// <summary>
	/// Represents an invalid tile (one that is not on the board).
	/// </summary>
	private readonly Vector2 invalidTile = new (-1, -1);
	
	/// <summary>
	/// Represents the tile that the mouse has selected and is currently selecting.
	/// </summary>
	private Vector2 mouseTile, previousMouseTile, mousePos, previousMousePosition;

	/// <summary>
	/// The piece selected by the user's mouse.
	/// </summary>
	private Piece selectedPiece;
	
	/// <summary>
	/// The position of the piece selected by the user's mouse.
	/// </summary>
	private Vector2 selectedPiecePosition;
	
	/// <summary>
	/// Holds a list of all valid move for a selected piece.
	/// </summary>
	private List<ChessMove> validMoves;
	
	/// <summary>
	/// Maps a piece to an index that corresponds with a piece's position.
	/// </summary>
	private Dictionary<int, Piece> pieces;

	/// <summary>
	/// Represents the Root scene.
	/// </summary>
	private Root root;

	/// <summary>
	/// Is the user holding left click down?
	/// </summary>
	private bool heldDown;

	/// <summary>
	/// The current state of the board.
	/// </summary>
	public ChessState state;

	/// <summary>
	/// Represents the chess engine.
	/// </summary>
	public GoldFishSearcher searcher;
	
	/// <summary>
	/// Represents the move that is considered the best by the engine (for the engine).
	/// </summary>
	private ChessMove bestEngineMove;
	
	/// <summary>
	/// Is the engine making a move?
	/// </summary>
	private bool isEngineMoving;

	/// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		mouseTile = invalidTile;
		previousMouseTile = invalidTile;
		
		validMoves = new List<ChessMove>();
		pieces = new Dictionary<int, Piece>();
		root = GetParent<Root>();

		searcher = new GoldFishSearcher(TimeSpan.FromSeconds(Constants.engineAllottedTime), Constants.engineDepth, (int) (Environment.ProcessorCount / 1.2));
		searcher.SearchUpdate += result =>
		{
			GetWindow().Title = $"Engine is thinking at depth: {result}";
		};
	}

	/// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		var mouse = GetGlobalMousePosition();
		
		if(selectedPiece is not null)
		{
			selectedPiece.Position = mousePos - new Vector2(Constants.tileSize / 2.0f, Constants.tileSize / 2.0f);
		}
		
		// sets the current mouse tile to where the user is hovering their mouse (or has clicked)
		mouseTile = mouse.X > Constants.boardSize ? invalidTile : MapGlobalCoordsToBoard(mouse);
		mousePos = mouse;
		if (mouseTile == previousMouseTile)
		{
			return;
		}
		
		// redraw borders if the mouse position has changed
		previousMouseTile = mouseTile;
		QueueRedraw();
	}

	/// <summary>
	/// Handles user input.
	/// </summary>
	/// <param name="event">Any user input.</param>
	public override void _Input(InputEvent @event)
	{
		if(@event is not InputEventMouseButton button)
		{
			// only focus on events concerning the user's mouse
			return;
		}

		switch (root.gameState)
		{
			case Constants.GameState.WAITING_FOR_USER or Constants.GameState.CHECKMATE or Constants.GameState.STALEMATE:
				// are we waiting for a user to pick an option (for example, pawn promotion)?
				return;
			
			case Constants.GameState.GETTING_PIECE:
				// has a user clicked a piece? if so, handle it
				if (!heldDown)
				{
					HandlePieceClick(button);
				}
				break;
			
			case Constants.GameState.MAKING_A_MOVE:
				// is the user making a move with a piece? if so, handle it
				HandlePieceMove(button);
				break;
		}
	}

	/// <summary>
	/// Called when the Node is being drawn to the Scene.
	/// </summary>
	public override void _Draw()
	{
		switch (root.gameState)
		{
			case Constants.GameState.GETTING_PIECE: // grabbing a piece
				if (mouseTile == invalidTile)
				{
					return;
				}

				var piece = GetPieceFromVector2(mouseTile);
				if (piece != null)
				{
					if (piece.player == root.Player)
					{
						// draw a border around the piece being hovered over
						DrawTileWithBorder(mouseTile, new Color(1, 1, 0));
					}
				}
				break;
			
			case Constants.GameState.MAKING_A_MOVE: // making a move
				if (selectedPiecePosition != invalidTile)
				{
					DrawTileWithBorder(selectedPiecePosition, new Color(0, 1, 0));
				}

				foreach (var mov in validMoves)
				{
					// draw a border around the possible move being hovered over
					var move = TransformCoord(mov.NewPos).ToVector();
					if (mouseTile == move)
					{
						DrawTileWithBorder(move, new Color(1, 1, 0));
					}
					else if (GetPieceFromVector2(move) != null)
					{
						if (GetPieceFromVector2(move).player == root.Player)
						{
							DrawTileWithBorder(move, new Color(0, 1, 0));
						}
						else
						{
							DrawTileWithBorder(move, new Color(1, 0, 0));
						}
					}
					else
					{
						DrawTileWithBorder(move, new Color(0, 0, 1));
					}
				}
				break;
		}
	}

	/// <summary>
	/// Maps pixels coordinates to grid coordinates (0 - 7, 0 - 7).
	/// </summary>
	/// <param name="position">The original position, in pixels.</param>
	/// <returns>A new Vector2 representing a position on the chess board.</returns>
	private Vector2 MapGlobalCoordsToBoard(Vector2 position)
	{
		return new Vector2(Mathf.Floor(position.Y / Constants.tileSize), Mathf.Floor(position.X / Constants.tileSize));
	}

	/// <summary>
	/// Gets the piece from a Vector2 position.
	/// </summary>
	/// <param name="position">The position to fetch from.</param>
	/// <returns>The piece at the position, if any.</returns>
	public Piece GetPieceFromVector2(Vector2 position)
	{
		return GetPiece((int) position.X, (int) position.Y);
	}

	/// <summary>
	/// Draws a border around a tile on the board.
	/// </summary>
	/// <param name="position">The position of the tile.</param>
	/// <param name="color">The color of the border.</param>
	private void DrawTileWithBorder(Vector2 position, Color color)
	{
		DrawRect(new Rect2(new Vector2(position.Y * Constants.tileSize, position.X * Constants.tileSize), new Vector2(Constants.tileSize, Constants.tileSize)), color, false, 5);
	}

	/// <summary>
	/// Handles left clicks on a piece (selection).
	/// </summary>
	/// <param name="event">The click event.</param>
	private void HandlePieceClick(InputEventMouseButton @event)
	{
		if (mouseTile == invalidTile || @event.ButtonIndex != MouseButton.Left)
		{
			return;
		}

		var piece = GetPiece((int)mouseTile.X, (int)mouseTile.Y);
		if (piece == null)
		{
			return;
		}

		if (piece.player == state.ToMove && piece.GetValidMovesFromVector2(mouseTile).Any())
		{
			// we're good for making a move, set relevant vars to the needed values to do so
			selectedPiece = piece;
			selectedPiecePosition = mouseTile;
			previousMousePosition = selectedPiece.Position;
			validMoves = piece.GetValidMovesFromVector2(mouseTile);
			heldDown = true;

			root.gameState = Constants.GameState.MAKING_A_MOVE;
			QueueRedraw();
		}
	}

	/// <summary>
	/// Deselects a piece (the user clicked on a different piece or right clicked on the same piece).
	/// </summary>
	private void Deselect()
	{
		if(selectedPiece is not null)
		{
			selectedPiece.Position = previousMousePosition;
		}
		
		selectedPiece = null;
		selectedPiecePosition = invalidTile;
		validMoves = new List<ChessMove>();
		root.gameState = Constants.GameState.GETTING_PIECE;
		
		QueueRedraw();
	}

	/// <summary>
	/// Handles right clicks and mouse dragging (user deselected or is moving a piece).
	/// </summary>
	/// <param name="event">The click or drag event.</param>
	private void HandlePieceMove(InputEventMouseButton @event)
	{
		if (@event.ButtonIndex == MouseButton.Right || mouseTile == invalidTile || selectedPiece == null)
		{
			Deselect();
			heldDown = false;
			return;
		}

		if (mouseTile == invalidTile)
		{
			heldDown = false;
			return;
		}

		// did the user drag the piece to a valid move location?
		if (validMoves.Any(move => TransformCoord(move.NewPos).ToVector() == mouseTile))
		{
			var chessMove = validMoves.First(move => TransformCoord(move.NewPos).ToVector() == mouseTile);

			MovePiece(chessMove);
			
			// handle pawns trying to promote
			if (chessMove.IsPromotion)
			{
				GetTree().Paused = true;
				PromotePawn(mouseTile);
			}

			// reset all states after move has been completed
			selectedPiece = null;
			selectedPiecePosition = invalidTile;
			validMoves = new List<ChessMove>();

			state = chessMove.NewState;

			// make sure the game should still be continuing, end it if not
			if (root.Winner is null && !Constants.isEngineRequested)
			{
				root.gameState = Constants.GameState.GETTING_PIECE;
				
				root.SwitchPlayer();
				QueueRedraw();
			}
			else if (root.Winner is null)
			{
				GetWindow().Title = "Engine is thinking...";
				
				GetTree().Paused = true;
				root.SwitchPlayer();
				
				new Task(() =>
				{
					var (_, bestMove, _, _) = searcher.StartSearch(state);
					bestEngineMove = bestMove;
					
					CallDeferred(nameof(EngineFinishedMoving));
				}).Start();

				root.gameState = Constants.GameState.GETTING_PIECE;
				
				GetWindow().Title = "chessium";
				QueueRedraw();
			}
			else
			{
				var dialog = new WinnerDialog(root.Winner.Value);
				dialog.Position = new Vector2(Constants.boardSize / 2.0f - (WinnerDialog.winnerWidth - Dialog.size) / 2.0f, Constants.boardSize / 2.0f - (WinnerDialog.winnerHeight - Dialog.size) / 2.0f);
				
				root.AddChild(dialog);
				root.gameState = Constants.GameState.CHECKMATE;
			}
			
			heldDown = false;
		}
		else
		{
			heldDown = false;
			Deselect();
		}
	}

	/// <summary>
	/// Callback method for when the engine has finished making a move.
	/// </summary>
	public void EngineFinishedMoving()
	{
		MovePiece(bestEngineMove);
		
		state = bestEngineMove.NewState;
		isEngineMoving = false;
		
		GetTree().Paused = false;
		root.SwitchPlayer();
	}

	/// <summary>
	/// Handle the promotion of a pawn.
	/// </summary>
	/// <param name="position">The position of the promoting pawn.</param>
	private void PromotePawn(Vector2 position)
	{
		root.gameState = Constants.GameState.WAITING_FOR_USER;

		var dialog = new PromotionDialog(root.Player);
		dialog.Position = new Vector2(Constants.boardSize / 2.0f - (PromotionDialog.promotionWidth - Dialog.size) / 2.0f, Constants.boardSize / 2.0f - (PromotionDialog.promotionHeight - Dialog.size) / 2.0f);
		dialog.ProcessMode = ProcessModeEnum.WhenPaused;
		AddChild(dialog);

		// capture user's choice of piece type
		async void Action()
		{
			await dialog.selectionCallback.WaitAsync();
			CallDeferred(MethodName.FinishedPromotion, position, dialog);
		}

		new Task(Action).Start();
	}

	/// <summary>
	/// Callback method for when a player has finished promoting their piece.
	/// </summary>
	/// <param name="position">The position of the piece that has promoted.</param>
	/// <param name="dialog">The promotion dialog.</param>
	private void FinishedPromotion(Vector2 position, PromotionDialog dialog)
	{
		var newPiece = (PromotionType) dialog.selectedPiece!;
		RemoveChild(dialog);
		
		GetTree().Paused = false;
		
		// update the pawn to its new sprite & type
		var piece = GetPiece((int) position.X, (int) position.Y);
		piece.type = (PieceType) newPiece;
		piece.UpdateSprite();
		state.Promote(TransformCoord(((int, int))(position.X, position.Y)), newPiece);
	}

	/// <summary>
	/// Prepares the board for a new game (resets piece positions & sprites).
	/// </summary>
	public void NewGame()
	{
		if (state.ToMove == Side.Black)
		{
			root.ui.SetPlayer(Side.White);
		}
		
		state = ChessState.DefaultState();
		pieces = new Dictionary<int, Piece>();

		foreach (var child in GetChildren())
		{
			if (child is not Sprite2D)
			{
				RemoveChild(child);
				child.QueueFree();
			}
		}
		
		DrawBoard();

		foreach (var pair in pieces)
		{
			AddChild(pair.Value);
		}
	}

	/// <summary>
	/// Converts board coordinates to their flipped version if the board is flipped.
	/// </summary>
	/// <param name="coord">The coordinates to flip.</param>
	/// <returns>The flipped version of the original coordinates.</returns>
	public static (int, int) TransformCoord((int, int) coord)
	{
		return (Constants.flipBoard ? 7 - coord.Item1 : coord.Item1, coord.Item2);
	}
	
	/// <summary>
	/// Moves a selected piece to its new position.
	/// </summary>
	/// <param name="chessMove">The move that a player has made.</param>
	private void MovePiece(ChessMove chessMove)
	{
		if (chessMove.Taken is not null)
		{
			if (chessMove.IsCastle)
			{
				var (rx, ry) = TransformCoord(chessMove.Castle!.Value.GetCastleRookPos());

				SetPiecePos(rx, ry, TransformCoord(chessMove.Taken.Value));
			}
			else
			{
				var (x, y) = TransformCoord(chessMove.Taken.Value);
				RemovePiece(x, y);
			}
		}

		var (ox, oy) = TransformCoord(chessMove.OldPos);
		SetPiecePos(ox, oy, TransformCoord(chessMove.NewPos));
	}

	/// <summary>
	/// Sets the new position of a piece from its old position.
	/// </summary>
	/// <param name="x">The original x coordinate of its position.</param>
	/// <param name="y">The original y coordinate of its position.</param>
	/// <param name="newP">The new position to set.</param>
	private void SetPiecePos(int x, int y, (int, int) newP)
	{
		var (tx, ty) = newP;
		var p = pieces[CoordinatesToKey(x, y)];
		p.Position = new Vector2(ty * Constants.tileSize, tx * Constants.tileSize);
		pieces[CoordinatesToKey(tx, ty)] = p;
	}

	/// <summary>
	/// Gets a piece (or null) from a square on the chess board.
	/// </summary>
	/// <param name="x">The x coordinate.</param>
	/// <param name="y">The y coordinate.</param>
	/// <returns>An instance of a piece, or null if there is nothing.</returns>
	private Piece GetPiece(int x, int y)
	{
		return pieces.GetValueOrDefault(CoordinatesToKey(x, y), null);
	}

	/// <summary>
	/// Sets a piece in the global dictionary of pieces.
	/// </summary>
	/// <param name="x">The x coordinate of the piece's position.</param>
	/// <param name="y">The y coordinate of the piece's position.</param>
	/// <param name="piece">The piece to set.</param>
	private void SetPiece(int x, int y, Piece piece)
	{
		pieces[CoordinatesToKey(x, y)] = piece;
	}

	/// <summary>
	/// Removes a piece from the board.
	/// </summary>
	/// <param name="x">The x coordinate of the piece to be removed.</param>
	/// <param name="y">The y coordinate of the piece to be removed.</param>
	private void RemovePiece(int x, int y)
	{
		var pos = CoordinatesToKey(x, y);
		var pie = pieces[pos];
		root.Capture(pie);
		RemoveChild(pie);
		pie.QueueFree();
		pieces.Remove(pos);
	}

	/// <summary>
	/// Draws all pieces on the board.
	/// </summary>
	private void DrawBoard()
	{
		for (var x = 0; x < 8; x++)
		{
			for (var y = 0; y < 8; y++)
			{
				var (r, c) = TransformCoord((x, y));
				var dat = state.GetPiece(r, c);
				
				if(dat.IsPieceType(PieceType.Space))
				{
					continue;
				}
				
				var piece = new Piece(dat.GetSide(), dat.GetPieceType());
				piece.Position = new Vector2(y * Constants.tileSize, x * Constants.tileSize);
				
				SetPiece(x, y, piece);
			}
		}
	}

	/// <summary>
	/// Converts (x, y) coordinates to unique keys for accessing the current board state.
	/// </summary>
	/// <param name="x">The row of a piece.</param>
	/// <param name="y">The column of a piece.</param>
	/// <returns>An int that represents a unique value pertaining to a piece on the board.</returns>
	private int CoordinatesToKey(int x, int y)
	{
		return (x << 3) + y;
	}
}
