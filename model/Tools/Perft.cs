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
        private static int checkCounter = 0;


        public static ulong Run_Perft(int depth, Board board)
        {
#if DEBUG   // in Release rausoptimiert
            string fenBefore = board.ToFen();
            ulong zobristBefore = board.currentZobristKey;
#endif
            if (depth == 0) return 1;

            ulong nodes = 0;

            foreach (Move move in MoveGenerator.GeneratePseudoMoves(board, board.sideToMove))
            {
                if (!board.MakeMove(move, out Undo undo))
                    continue;

                path.Add(move);               // Pfad verlängern

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
                        $"Zobrist after    : 0x{board.currentZobristKey:X16}");
                }
#endif
                path.RemoveAt(path.Count - 1);   
            }
            return nodes;
        }


        public static void PerftDivide(int depth, Board board)
        {
            ulong total = 0;
            foreach (Move m in MoveGenerator.GeneratePseudoMoves(board, board.sideToMove))
            {
                if (!board.MakeMove(m, out Undo undo)) continue;

                ulong sub = Run_Perft(depth - 1, board);

                board.UnmakeMove(m, undo);

                Console.WriteLine($"{m} : {sub}");
                total += sub;
            }
            Console.WriteLine($"Total {depth}: {total}");
        }
    }
}
