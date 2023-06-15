﻿using System.Diagnostics;
using engine_test;
using goldfish.Core.Data;
using goldfish.Core.Game;
using goldfish.Core.Game.FEN;
using goldfish.Engine;
using goldfish.Engine.Searcher;
using Spectre.Console;

var searcher = new GoldFishSearcher(TimeSpan.FromSeconds(6), 12);

int depth = 0;

// var state = FenConvert.Parse("r3k2r/8/3Q4/8/8/5q2/8/R3K2R b KQkq - 0 1");
// var state = FenConvert.Parse("3k2r1/8/8/3PK3/8/8/5R2/8 b - - 0 1");
// var state = FenConvert.Parse("3r2kr/p4pp1/4pn1p/2R5/7P/1P6/2P2PP1/4KB2 w - - 0 0");
var state = ChessState.DefaultState();
try
{
    while (state.GetGameState() is null)
    {
        GoldFishSearcher.SearchResult res = null;
        if (state.ToMove == Side.Black)
        {
            res = searcher.ParallelSearch(state, 6, CancellationToken.None);
            Console.WriteLine($"A - {res} {FenConvert.ToFen(res.BestMove.NewState)}");
        }
        else
        {
            res = searcher.StartSearch(state);
            Console.WriteLine($"B - {res} {FenConvert.ToFen(res.BestMove.NewState)}");
        }
        AnsiConsole.Write(BoardPrinter.PrintBoard(res.BestMove.NewState, res.BestMove));
        state = res.BestMove.NewState;
    }

}
catch(Exception e)
{
    Console.WriteLine(e.Message + e.StackTrace);
}

Console.WriteLine("END");

// static int CountNextGames(ChessState state, int depth)
// {
//     if (depth == 0)
//     {
//         return 1;
//     }
//     Span<ChessMove> tMoves = stackalloc ChessMove[30];
//     int cnt = 0;
//     for (var i = 0; i < 8; i++)
//     for (var j = 0; j < 8; j++)
//     {
//         var piece = state.GetPiece(i, j);
//         if (!piece.IsSide(state.ToMove) || piece.GetLogic() is null) continue;
//         int moveCnt = state.GetValidMovesForSquare(i, j, tMoves);
//         if (depth == 1)
//         {
//             cnt += moveCnt;
//         }
//         else
//         {
//             for(int m = 0; m < moveCnt; m++)
//             {
//                 cnt += CountNextGames(tMoves[m].NewState, depth - 1);
//             }
//         }
//     }
//
//     return cnt;
// }
//
// for (int i = 0; i < 10; i++)
// {
//     var start = Stopwatch.GetTimestamp();
//     Console.WriteLine(CountNextGames(ChessState.DefaultState(), 5));
//     Console.WriteLine(Stopwatch.GetElapsedTime(start));
// }

Console.ReadLine();

// var game = ChessState.DefaultState();
//
// // var game = FenConvert.Parse(Console.ReadLine());
//
// // Console.WriteLine(game.GetGameState());
// //
// // return;
//
// Span<(ChessMove, double)> moves = new (ChessMove, double)[6];
// ulong positions = 0;
// Console.Write(Tst.Get(game).Hash);
// var start = Stopwatch.GetTimestamp();
// var tEval = GoldFishEngine.NextOptimalMoves(game, 6, ref moves, ref positions);
// var end = Stopwatch.GetElapsedTime(start);
//
// Console.WriteLine($"Calculated - {tEval} {positions} positions in {end}");
//
// foreach (var (move, eval) in moves)
// {
//     Console.WriteLine($"Step -- E: {eval} --");
//     
//     Console.WriteLine($"{move.Type} to {move.NewPos}");
//
//     AnsiConsole.Write(BoardPrinter.PrintBoard(in move.NewState, move));
//     
//     Console.WriteLine($"{FenConvert.ToFen(move.NewState)}");
// }
//
// Console.Read();