using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Chess
{
    public static class CommunicateUCI
    {
        /// <summary>
        /// Communicate with a chess engine at the given path over the UCI protocol
        /// to find the best move that can be made in the current game.
        /// </summary>
        /// <param name="depth">The maximum number of half-moves to search</param>
        /// <returns>The best move that can be played according to the engine, or null if an error occured</returns>
        public static async Task<BoardAnalysis.PossibleMove?> GetBestMove(ChessGame game, string enginePath, uint depth,
            CancellationToken cancellationToken)
        {
            System.Diagnostics.Process engine = new();
            engine.StartInfo.FileName = enginePath;
            engine.StartInfo.UseShellExecute = false;
            engine.StartInfo.CreateNoWindow = true;
            engine.StartInfo.RedirectStandardInput = true;
            engine.StartInfo.RedirectStandardOutput = true;
            if (!engine.Start())
            {
                return null;
            }

            await engine.StandardInput.WriteLineAsync("uci");
            await engine.StandardInput.WriteLineAsync("ucinewgame");
            await engine.StandardInput.WriteLineAsync("isready");

            // Wait for engine to be ready
            string? line;
            do
            {
                line = await engine.StandardOutput.ReadLineAsync();
            }
            while (line != "readyok");

            await engine.StandardInput.WriteLineAsync($"position fen {game}");
            await engine.StandardInput.WriteLineAsync($"go depth {depth}");

            List<string> lines = new();
            do
            {
                line = await engine.StandardOutput.ReadLineAsync();
                lines.Add(line ?? "");
            }
            while ((line is null || !line.StartsWith("best")) && !cancellationToken.IsCancellationRequested);

            if (cancellationToken.IsCancellationRequested)
            {
                return null;
            }

            engine.Close();

            string bestMove = lines[^1] == "bestmove (none)"
                ? "a1a1"
                : Regex.Match(lines[^1], @"bestmove ((?:[a-h][1-8]){2}[qnbr]?)").Groups[1].Value;
            Match moveInfo = Regex.Match(lines[^2], @"info .+ score (cp|mate) (-?[0-9]+) .+ pv ((?:(?:[a-h][1-8]){2}[qnbr]?(?: |$))+)");
            bool mateFound = moveInfo.Groups[1].Value == "mate";
            double moveValue = mateFound ? int.Parse(moveInfo.Groups[2].Value) : int.Parse(moveInfo.Groups[2].Value) / 100d;
            bool whiteMateFound = mateFound && ((game.CurrentTurnWhite && moveValue <= 0) || (!game.CurrentTurnWhite && moveValue > 0));
            bool blackMateFound = mateFound && ((!game.CurrentTurnWhite && moveValue <= 0) || (game.CurrentTurnWhite && moveValue > 0));

            Type? promotionType = null;
            if (bestMove.Length == 5)
            {
                promotionType = bestMove[4] switch
                {
                    'q' => typeof(Pieces.Queen),
                    'n' => typeof(Pieces.Knight),
                    'b' => typeof(Pieces.Bishop),
                    'r' => typeof(Pieces.Rook),
                    _ => null
                };
            }

            List<(Point, Point, Type)> bestLine = new();
            foreach(string move in moveInfo.Groups[3].Value.Split(' '))
            {
                bestLine.Add((move[..2].FromChessCoordinate(), move[2..4].FromChessCoordinate(),
                    move.Length == 5 ? move[4] switch
                    {
                        'q' => typeof(Pieces.Queen),
                        'n' => typeof(Pieces.Knight),
                        'b' => typeof(Pieces.Bishop),
                        'r' => typeof(Pieces.Rook),
                        _ => typeof(Pieces.Queen)
                    } : typeof(Pieces.Queen)));
            }

            return new BoardAnalysis.PossibleMove(bestMove[..2].FromChessCoordinate(), bestMove[2..4].FromChessCoordinate(),
                blackMateFound ? double.PositiveInfinity : whiteMateFound ? double.NegativeInfinity : game.CurrentTurnWhite ? moveValue : -moveValue,
                // Multiply mate depth by 2 as PossibleMove expects depth in half-moves, engine gives it in full-moves
                whiteMateFound, blackMateFound, whiteMateFound ? Math.Abs((int)moveValue) * 2 : 0,
                blackMateFound ? Math.Abs((int)moveValue) * 2 : 0, promotionType, bestLine);
        }
    }
}
