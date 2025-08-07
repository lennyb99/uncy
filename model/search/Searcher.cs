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
        private TranspositionTable transpositionTable;

        public Search(IEvaluator eval, TranspositionTable tt)
        {
            this.evaluator = eval;
            this.transpositionTable = tt;
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
                
                int score = MiniMaxWithAlphaBeta(board, depth - 1, int.MinValue, int.MaxValue, !maximizingSide);

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

        public int MiniMaxWithAlphaBeta(Board board, int depth, int alpha, int beta, bool maxPlayer)
        {
            // Reading from the transposition table to determine whether this node needs to be calculated
            ulong zobristKey = board.currentZobristKey;
            if (transpositionTable.TryGetEntry(zobristKey, out TranspositionTableEntry entry) && entry.depth >= depth)
            {
                switch(entry.flag)
                {
                    case TranspositionTableFlag.EXACT:
                        return entry.score;

                    case TranspositionTableFlag.LOWERBOUND:
                        if (entry.score >= beta) return entry.score;
                        alpha = Math.Max(alpha, entry.score);
                        break;

                    case TranspositionTableFlag.UPPERBOUND:
                        if (entry.score <= alpha) return entry.score;
                        beta = Math.Min(beta, entry.score);
                        break;
                }
            }

            if (depth == 0)
            {
                return evaluator.Evaluate(board);
            }

            // Needed to determine if flag for score is UpperBound
            int originalAlpha = alpha;

            // Track the best move within the current node
            Move bestMoveInNode = default;

            if (maxPlayer)
            {
                int maxScore = int.MinValue;
                foreach (Move m in MoveGenerator.GenerateLegalMoves(board))
                {
                    if (!board.MakeMove(m, out Undo undo)) // If this returns wrong, the move wasn't legal, therefore will be skipped. MakeMove is handling the UnmakeMove()
                        continue;
                    int score = MiniMaxWithAlphaBeta(board, depth - 1, alpha, beta, !maxPlayer);
                    board.UnmakeMove(m, undo);
                    
                    if(score > maxScore)
                    {
                        maxScore = score;
                        bestMoveInNode = m;
                    }
                    

                    // Alpha beta pruning happens here:
                    alpha = Math.Max(alpha, score);
                    if (beta <= alpha)
                    {
                        transpositionTable.StoreEntry(zobristKey, maxScore, depth, TranspositionTableFlag.LOWERBOUND, m);
                        return maxScore;
                    }

                }

                TranspositionTableFlag flag;
                if(maxScore > originalAlpha)
                {
                    flag = TranspositionTableFlag.EXACT;
                }
                else
                {
                    flag = TranspositionTableFlag.UPPERBOUND;
                }

                transpositionTable.StoreEntry(zobristKey, maxScore, depth, flag, bestMoveInNode);
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
                    
                    if(score < minScore)
                    {
                        minScore = score;
                        bestMoveInNode = m;
                    }

                    // Pruning
                    beta = Math.Min(beta, score);
                    if (beta <= alpha) 
                    {
                        transpositionTable.StoreEntry(zobristKey, minScore, depth, TranspositionTableFlag.UPPERBOUND, m);
                        return minScore;
                    }

                }
                transpositionTable.StoreEntry(zobristKey, minScore, depth, TranspositionTableFlag.EXACT, bestMoveInNode);
                return minScore;
            }
        }



        /*
         * DEPRECATED
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
                foreach (Move m in MoveGenerator.GenerateLegalMoves(board))
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
                foreach (Move m in MoveGenerator.GenerateLegalMoves(board))
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
    }
}
