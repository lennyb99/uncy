using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using uncy.model.boardAlt;

namespace uncy.model.Tools
{
    internal class Perft
    {
        private static readonly List<Move> path = new();

        // Diese Liste ist weiterhin GOLD WERT, um Millionen von Allokationen
        // IM MoveGenerator zu verhindern. Wir verwenden sie als "Sammelbehälter".
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

            // Schritt 1: Fülle unseren wiederverwendbaren "Sammelbehälter".
            reusableMoveList.Clear();
            MoveGenerator.GeneratePseudoMoves(board, board.sideToMove, reusableMoveList);

            // Schritt 2 (Die Lösung): Erstelle eine LOKALE KOPIE für die Iteration.
            // ToArray() ist hier sehr effizient. Diese Kopie wird von den rekursiven
            // Aufrufen NICHT verändert.
            var movesToIterate = reusableMoveList.ToArray();

            // Schritt 3: Iteriere über die sichere, lokale Kopie.
            foreach (Move move in movesToIterate)
            {
                if (!board.MakeMove(move, out Undo undo))
                {
                    continue;
                }

                path.Add(move);

                // Der rekursive Aufruf kann jetzt die reusableMoveList nach Belieben
                // verändern, es stört unsere 'movesToIterate'-Schleife nicht mehr.
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
                        $"Zobrist after    : {board.currentZobristKey:X16}");
                }
#endif
                path.RemoveAt(path.Count - 1);
            }
            return nodes;
        }

        public static void PerftDivide(int depth, Board board)
        {
            Console.WriteLine($"\n--- Perft Divide for Depth {depth} ---");
            ulong total = 0;

            // Die gleiche Logik hier anwenden:
            reusableMoveList.Clear();
            MoveGenerator.GeneratePseudoMoves(board, board.sideToMove, reusableMoveList);

            // Sortiere die Züge für eine konsistente Ausgabe. Hier ist ToList() gut,
            // da OrderBy() sowieso eine neue Sequenz erzeugt.
            var sortedMoves = reusableMoveList.OrderBy(m => m.ToString()).ToList();

            foreach (Move m in sortedMoves)
            {
                if (!board.MakeMove(m, out Undo undo))
                {
                    continue;
                }

                ulong subNodes = Run_Perft(depth - 1, board);

                board.UnmakeMove(m, undo);

                Console.WriteLine($"{m}: {subNodes}");
                total += subNodes;
            }
            Console.WriteLine($"\nTotal nodes for depth {depth}: {total}");
        }
    }
}
