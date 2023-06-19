using Godot;

namespace chessium.scripts;

/// <summary>
/// A utility class to store constants.
/// </summary>
public partial class Constants : Node2D
{
	/// <summary>
	/// The size of the entire chess board.
	/// </summary>
	public const int boardSize = 480;
	
	/// <summary>
	/// The size of an individual tile (that contains a piece).
	/// </summary>
	public const int tileSize = boardSize / 8;
	
	public static bool flipBoard = false, isEngineRequested, engineStarts;
	public static int engineAllottedTime, engineDepth;

	/// <summary>
	/// An enum representing the current state of the game.
	/// </summary>
	public enum GameState
	{
		/// <summary>
		/// Are we clicking on a piece?
		/// </summary>
		GETTING_PIECE,
		/// <summary>
		/// Are we moving a piece?
		/// </summary>
		MAKING_A_MOVE,
		/// <summary>
		/// Are we stalemated?
		/// </summary>
		STALEMATE, // TODO: implement with adam's code
		/// <summary>
		/// Are we checkmated?
		/// </summary>
		CHECKMATE,
		/// <summary>
		/// Are we waiting for the user to make a choice?
		/// </summary>
		WAITING_FOR_USER
	}
}
