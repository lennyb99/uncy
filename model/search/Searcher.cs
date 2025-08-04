using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using uncy.model.boardAlt;
using uncy.model.eval;

namespace uncy.model.search
{
    public class Search 
    {
        private IEvaluator evaluator;

        public Search(IEvaluator eval)
        {
            this.evaluator = eval;
        }

        public Move FindBestMove(Board board,int depth)
        {
            if (depth <= 0) throw new ArgumentOutOfRangeException(nameof(depth));

            bool maximizingSide = board.sideToMove;
            Move bestMove = default;
            int bestScore = maximizingSide ? int.MinValue : int.MaxValue;


            foreach (Move move in MoveGenerator.GeneratePseudoMoves(board, board.sideToMove))
            {
                if (!board.MakeMove(move, out Undo undo)) // If this returns wrong, the move wasn't legal, therefore will be skipped. MakeMove is handling the UnmakeMove()
                    continue;
                
                int score = MiniMax(board, depth - 1, !maximizingSide);

                board.UnmakeMove(move, undo);

                bool isBetter = maximizingSide ? score > bestScore : score < bestScore;
                if (isBetter)
                {
                    bestScore = score;
                    bestMove = move;
                }
            }

            return bestMove;

        }



        /*
         * maxPlayer = true means white player. White favoring positions have positive scores therefore white is trying to be the maximizing player in minimax algorithm
         */
        public int MiniMax(Board board, int depth, bool maxPlayer)
        {
            if (depth == 0)
            {
                return evaluator.Evaluate(board);
            }

            if (maxPlayer)
            {
                int maxScore = int.MinValue;
                foreach(Move m in MoveGenerator.GenerateLegalMoves(board))
                {
                    if (!board.MakeMove(m, out Undo undo)) // If this returns wrong, the move wasn't legal, therefore will be skipped. MakeMove is handling the UnmakeMove()
                        continue;
                    int score = MiniMax(board, depth - 1, !maxPlayer);
                    board.UnmakeMove(m, undo);
                    maxScore = Math.Max(maxScore, score);
                }
                return maxScore;
            }
            else
            {
                int minScore = int.MaxValue;
                foreach(Move m in MoveGenerator.GenerateLegalMoves(board))
                {
                    if (!board.MakeMove(m, out Undo undo))
                        continue;
                    int score = MiniMax(board, depth - 1, !maxPlayer);
                    board.UnmakeMove(m, undo);
                    minScore = Math.Min(minScore, score);
                }
                return minScore;
            }
        }


        public int MiniMaxWithAlphaBeta(Board board, int depth, int alpha, int beta, bool maxPlayer)
        {
            if (depth == 0)
            {
                return evaluator.Evaluate(board);
            }

            if (maxPlayer)
            {
                int maxScore = int.MinValue;
                foreach (Move m in MoveGenerator.GenerateLegalMoves(board))
                {
                    if (!board.MakeMove(m, out Undo undo)) // If this returns wrong, the move wasn't legal, therefore will be skipped. MakeMove is handling the UnmakeMove()
                        continue;
                    int score = MiniMaxWithAlphaBeta(board, depth - 1, alpha, beta, !maxPlayer);
                    board.UnmakeMove(m, undo);
                    maxScore = Math.Max(maxScore, score);
                    
                    // Alpha beta pruning happens here:
                    alpha = Math.Max(alpha, score);
                    if (beta <= alpha) break;

                }
                return maxScore;
            }
            else
            {
                int minScore = int.MaxValue;
                foreach (Move m in MoveGenerator.GenerateLegalMoves(board))
                {
                    if (!board.MakeMove(m, out Undo undo))
                        continue;
                    int score = MiniMaxWithAlphaBeta(board, depth - 1, alpha, beta, !maxPlayer);
                    board.UnmakeMove(m, undo);
                    minScore = Math.Min(minScore, score);

                    // Pruning
                    beta = Math.Min(beta, score);
                    if (beta <= alpha) break;

                }
                return minScore;
            }
        }


    }

    public record SearchResult(Move BestMove, int Score, int Nodes, TimeSpan TimeTaken);
}
