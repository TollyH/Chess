using System;
using System.Collections.Generic;
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

            string bestMove = Regex.Match(lines[^1], @"bestmove ((?:[a-h][1-8]){2}[qnbr]?)").Groups[1].Value;
            Match moveInfo = Regex.Match(lines[^2], @"info .+ score (cp|mate) (-?[0-9]+)");
            double moveValue = int.Parse(moveInfo.Groups[2].Value) / 100d;
            bool mateFound = moveInfo.Groups[1].Value == "mate";
            bool whiteMateFound = mateFound && moveValue < 0;
            bool blackMateFound = mateFound && moveValue > 0;

            return new BoardAnalysis.PossibleMove(bestMove[..2].FromChessCoordinate(), bestMove[2..4].FromChessCoordinate(),
                blackMateFound ? double.NegativeInfinity : whiteMateFound ? double.PositiveInfinity : game.CurrentTurnWhite ? moveValue : -moveValue,
                whiteMateFound, blackMateFound, whiteMateFound ? (int)moveValue + 1 : 0,
                blackMateFound ? Math.Abs((int)moveValue) + 1 : 0);
        }
    }
}
