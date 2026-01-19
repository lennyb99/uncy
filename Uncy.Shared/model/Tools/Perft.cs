using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using Uncy.Shared.boardAlt;

namespace Uncy.Shared.Tools
{
    public class Perft
    {
        private static readonly List<Move> path = new();

        private static readonly List<Move> reusableMoveList = new List<Move>(256);

        public static ulong Run_Perft(int depth, Board board)
        {
#if DEBUG
            string fenBefore = board.ToFen();
            ulong zobristBefore = board.currentZobristKey;
#endif
            if (depth == 0)
            {
                return 1;
            }

            ulong nodes = 0;

            reusableMoveList.Clear();
            MoveGenerator.GeneratePseudoMoves(board, board.sideToMove, reusableMoveList);
            var movesToIterate = reusableMoveList.ToArray();

            //Console.WriteLine($"Side to move: {board.sideToMove}");
            //Console.WriteLine($"Position: {board.ToFen()}");

            foreach (Move move in movesToIterate)
            {
                if (!board.MakeMove(move, out Undo undo))
                {
                    continue;
                }
                path.Add(move);

                nodes += Run_Perft(depth - 1, board);

                board.UnmakeMove(move, undo);

#if DEBUG
                bool fenMismatch = fenBefore != board.ToFen();
                bool zobristMismatch = zobristBefore != board.currentZobristKey;

                if (fenMismatch || zobristMismatch)
                {
                    string line = string.Join(" ", path.Select(m => m.ToString()));
                    throw new InvalidOperationException(
                        $"UNDO-Error after {move}. Depth: {depth}\n" +
                        $"Path:            {line}\n\n" +
                        $"FEN before       : {fenBefore}\n" +
                        $"FEN after        : {board.ToFen()}\n\n" +
                        $"Zobrist before   : 0x{zobristBefore:X16}\n" +
                        $"Zobrist after     : {board.currentZobristKey:X16}");
                }
#endif
                path.RemoveAt(path.Count - 1);
            }
            return nodes;
        }

        public static void PerftDivide(int depth, Board board, bool verbose = true)
        {
            if (verbose)
                Console.WriteLine($"\n--- Perft Divide for Depth {depth} ---");

            ulong total = 0;

            reusableMoveList.Clear();
            MoveGenerator.GeneratePseudoMoves(board, board.sideToMove, reusableMoveList);

            // OPTIMIERT: Sortiere nur wenn verbose=true (für Ausgabe)
            // Wenn verbose=false, nutze direkt das Array ohne Sortierung
            Move[] movesToIterate;
            if (verbose)
            {
                // Sortiere nach fromSquare + toSquare (int-Vergleich) statt String!
                movesToIterate = reusableMoveList
                    .OrderBy(m => (m.fromSquare << 16) | m.toSquare) // Kombiniere beide Squares in einem int
                    .ToArray();
            }
            else
            {
                // Keine Sortierung nötig - direkt verwenden
                movesToIterate = reusableMoveList.ToArray();
            }

            foreach (Move m in movesToIterate)
            {
                if (!board.MakeMove(m, out Undo undo))
                {
                    continue;
                }

                // Nutze Fast-Version wenn nicht verbose (schneller)
                // Wenn verbose, nutze normale Version für path-Tracking (falls DEBUG aktiviert)
                ulong subNodes = verbose ? Run_Perft(depth - 1, board) : Run_PerftFast(depth - 1, board);

                board.UnmakeMove(m, undo);

                if (verbose)
                {
                    // Nur wenn verbose, rufe GiveMoveAbbreviation auf
                    Console.WriteLine($"{board.GiveMoveAbbreviation(m)}: {subNodes}");
                }

                total += subNodes;
            }

            if (verbose)
                Console.WriteLine($"\nTotal nodes for depth {depth}: {total}");
        }

        /// <summary>
        /// Optimierte Version von Run_Perft ohne DEBUG-Checks und ohne path-Tracking.
        /// Viel schneller für Performance-Tests.
        /// </summary>
        public static ulong Run_PerftFast(int depth, Board board)
        {
            if (depth == 0)
                return 1;

            ulong nodes = 0;
            reusableMoveList.Clear();
            MoveGenerator.GeneratePseudoMoves(board, board.sideToMove, reusableMoveList);
            var movesToIterate = reusableMoveList.ToArray();

            foreach (Move move in movesToIterate)
            {
                if (!board.MakeMove(move, out Undo undo))
                    continue;

                nodes += Run_PerftFast(depth - 1, board);
                board.UnmakeMove(move, undo);
            }

            return nodes;
        }

        /// <summary>
        /// Minimalste Version - nur für maximale Performance.
        /// Keine path-Tracking, keine DEBUG-Checks, keine ToArray() (nutzt direkt die List).
        /// </summary>
        public static ulong Run_PerftMinimal(int depth, Board board)
        {
            if (depth == 0)
                return 1;

            ulong nodes = 0;
            reusableMoveList.Clear();
            MoveGenerator.GeneratePseudoMoves(board, board.sideToMove, reusableMoveList);

            // Direkt über die List iterieren statt ToArray() - spart eine Allokation
            for (int i = 0; i < reusableMoveList.Count; i++)
            {
                Move move = reusableMoveList[i];
                if (!board.MakeMove(move, out Undo undo))
                    continue;

                nodes += Run_PerftMinimal(depth - 1, board);
                board.UnmakeMove(move, undo);
            }

            return nodes;
        }

        /// <summary>
        /// PerftDivide mit Fast-Version (ohne DEBUG-Checks).
        /// Schneller für Performance-Tests mit Ausgabe.
        /// </summary>
        public static void PerftDivideFast(int depth, Board board, bool verbose = true)
        {
            if (verbose)
                Console.WriteLine($"\n--- Perft Divide Fast (Depth {depth}) ---");

            ulong total = 0;
            reusableMoveList.Clear();
            MoveGenerator.GeneratePseudoMoves(board, board.sideToMove, reusableMoveList);

            Move[] movesToIterate;
            if (verbose)
            {
                movesToIterate = reusableMoveList
                    .OrderBy(m => (m.fromSquare << 16) | m.toSquare)
                    .ToArray();
            }
            else
            {
                movesToIterate = reusableMoveList.ToArray();
            }

            foreach (Move m in movesToIterate)
            {
                if (!board.MakeMove(m, out Undo undo))
                    continue;

                ulong subNodes = Run_PerftFast(depth - 1, board);
                board.UnmakeMove(m, undo);

                if (verbose)
                    Console.WriteLine($"{board.GiveMoveAbbreviation(m)}: {subNodes}");

                total += subNodes;
            }

            if (verbose)
                Console.WriteLine($"\nTotal nodes for depth {depth}: {total}");
        }

        /// <summary>
        /// Einfache Perft-Methode ohne Divide - nur die Gesamtzahl der Nodes.
        /// Schnellste Variante für reine Performance-Tests.
        /// </summary>
        public static ulong PerftSimple(int depth, Board board)
        {
            return Run_PerftFast(depth, board);
        }
    }
}
