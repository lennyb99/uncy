using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using uncy.model.boardAlt;

namespace uncy.model.eval
{

    internal class MaterialEvaluator : IEvaluator
    {
        private static readonly int[] pieceTable = InitPieceTable();

        public int Evaluate(Board board)
        {
            int score = 0;

            for(int i = 0; i < board.boardSize; i++)
            {
                byte piece = board.board[i];
                if (piece == Piece.Empty || piece == Piece.Inactive) continue;

                int val = GetPieceScore(piece);
                score += val;
            }
            return score;
        }

        private static int[] InitPieceTable()
        {
            var t = new int[128];
            t['P'] = +100; t['p'] = -100;
            t['N'] = +320; t['n'] = -320;
            t['B'] = +330; t['b'] = -330;
            t['R'] = +500; t['r'] = -500;
            t['Q'] = +900; t['q'] = -900;
            t['K'] = +20000; t['k'] = -20000;
            return t;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int GetPieceScore(byte p) => pieceTable[Piece.GiveCharIdentifier(p)];
        

    }
}
