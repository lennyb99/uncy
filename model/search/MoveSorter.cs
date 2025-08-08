using System;
using System.Collections.Generic;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using uncy.model.boardAlt;
using uncy.model.eval;

namespace uncy.model.search
{
    internal class MoveSorter
    {
        private readonly List<Move> moves;
        private readonly int[] scores;
        private int currentIndex = 0;

        public MoveSorter(Board board, TranspositionTable tt) // Killer moves & depth, Historyheuristic for quiet moves,
        {
            moves = MoveGenerator.GenerateLegalMoves(board);
            scores = new int[moves.Count];

            Move pvMove = tt.GetBestMove(board.currentZobristKey);

            for (int i = 0; i<moves.Count; i++)
            {
                scores[i] = CalculateMoveScore(moves[i], pvMove);
            }

        }

        private int CalculateMoveScore(Move move, Move pvMove)
        {
            if (move.Equals(pvMove))
            {
                return 2_000_000;
            }

            if (move.capturedPiece != 'e')
            {
                return 1_000_000 + CalculateMvvLvaScore(move);
            }

            return 1_000_000;
        }

        private int CalculateMvvLvaScore(Move move)
        {
            int attackValue = PieceValues.GetValue(move.movedPiece);
            int captureValue = PieceValues.GetValue(move.capturedPiece);

            return captureValue * 100 - attackValue;
        }
    
        public Move? GetNextMove()
        {
            if (currentIndex >= moves.Count)
            {
                return null;
            }

            int bestScore = -1;
            int bestIndex = currentIndex;
            for (int i = currentIndex; i < moves.Count; i++)
            {
                if (scores[i] > bestScore)
                {
                    bestScore = scores[i];
                    bestIndex = i;
                }
            }

            Swap(currentIndex, bestIndex);
            return moves[currentIndex++];

        }

        private void Swap(int i, int j)
        {
            (moves[i], moves[j]) = (moves[j], moves[i]);
            (scores[i], scores[j]) = (scores[j], scores[i]);
        }
    }
}
