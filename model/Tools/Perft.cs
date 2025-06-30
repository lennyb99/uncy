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
        private static ulong captures = 0;
        public static ulong Run_Perft(int depth, Board board)
        {
            if (depth == 0) return 1;

            ulong nodes = 0;
            

            foreach (Move move in MoveGenerator.GeneratePseudoMoves(board, board.sideToMove))
            {
                if (!board.MakeMove(move, out Undo undo)) continue;

                //board.PrintBoardToConsoleShort();

                nodes += Run_Perft(depth - 1, board);

                // capture counter to debug
                //if (move.capturedPiece != 'e')
                //{
                //    captures++;
                //}

                board.UnmakeMove(move, undo);
            }
            //Console.WriteLine("cap:" + captures);
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
