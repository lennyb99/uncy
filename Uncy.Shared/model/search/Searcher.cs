using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uncy.Shared.boardAlt;
using Uncy.Shared.eval;

namespace Uncy.Shared.search
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

        public Move FindBestMove(Board board, int max_depth)
        {
            Console.WriteLine("---------");
            Console.WriteLine("Starting Search for best Move..");
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            Move m = StartIterativeDeepening(board, max_depth);

            stopwatch.Stop();
            Console.WriteLine("Finished Search in time: " + stopwatch.ToString());
            Console.WriteLine("---------");

            return m;



        }

        public Move StartIterativeDeepening(Board board, int max_depth)
        {
            Move bestMove = default;
            for (int currentDepth = 1; currentDepth <= max_depth; currentDepth++)
            {
                Move bestMoveForCurrentDepth = StartMinimaxSearch(board, currentDepth);
                bestMove = bestMoveForCurrentDepth;

                // TODO: Implement time limit for search
                // if(timeIsUp()) break;
            }
            return bestMove;
        }

        public Move StartMinimaxSearch(Board board, int depth)
        {
            if (depth <= 0) throw new ArgumentOutOfRangeException(nameof(depth));

            bool maximizingSide = board.sideToMove;
            Move bestMove = default;
            int alpha = int.MinValue;
            int beta = int.MaxValue;
            int bestScore = maximizingSide ? int.MinValue : int.MaxValue;

            MoveSorter moveSorter = new MoveSorter(board, transpositionTable);
            Move? m;

            //foreach (Move move in MoveGenerator.GeneratePseudoMoves(board, board.sideToMove))
            //foreach (Move moves in SortedMoves(board))
            while ((m = moveSorter.GetNextMove()) != null)
            {
                if (!board.MakeMove(m.Value, out Undo undo)) // If this returns wrong, the move wasn't legal, therefore will be skipped. MakeMove is handling the UnmakeMove()
                    continue;

                int score = MiniMaxWithAlphaBeta(board, depth - 1, alpha, beta, !maximizingSide);

                board.UnmakeMove(m.Value, undo);

                bool isBetter = maximizingSide ? score > bestScore : score < bestScore;
                if (isBetter)
                {
                    bestScore = score;
                    bestMove = m.Value;
                }

                alpha = Math.Max(alpha, bestScore);
                if (alpha >= beta) break;

            }
            transpositionTable.StoreEntry(board.currentZobristKey, bestScore, depth, TranspositionTableFlag.EXACT, bestMove);
            return bestMove;
        }

        public int MiniMaxWithAlphaBeta(Board board, int depth, int alpha, int beta, bool maxPlayer)
        {
            // Reading from the transposition table to determine whether this node needs to be calculated
            ulong zobristKey = board.currentZobristKey;
            if (transpositionTable.TryGetEntry(zobristKey, out TranspositionTableEntry entry) && entry.depth >= depth)
            {
                switch (entry.flag)
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

            MoveSorter moveSorter = new MoveSorter(board, transpositionTable);
            Move? m;

            if (moveSorter.HasNoMoreMoves())
            {
                if (board.IsKingInCheck(board.sideToMove))
                {
                    if (board.sideToMove)
                    {
                        return int.MinValue + depth;
                    }
                    else
                    {
                        return int.MaxValue - depth;
                    }
                }
                else
                {
                    return 0;
                }
            }

            // Needed to determine if flag for score is UpperBound
            int originalAlpha = alpha;

            // Track the best move within the current node
            Move bestMoveInNode = default;

            if (maxPlayer)
            {
                int maxScore = int.MinValue;
                //foreach (Move m in MoveGenerator.GenerateLegalMoves(board))
                //foreach (Move m in SortedMoves(board))
                while ((m = moveSorter.GetNextMove()) != null)
                {
                    if (!board.MakeMove(m.Value, out Undo undo)) // If this returns wrong, the move wasn't legal, therefore will be skipped. MakeMove is handling the UnmakeMove()
                        continue;
                    int score = MiniMaxWithAlphaBeta(board, depth - 1, alpha, beta, !maxPlayer);
                    board.UnmakeMove(m.Value, undo);

                    if (score > maxScore)
                    {
                        maxScore = score;
                        bestMoveInNode = m.Value;
                    }


                    // Alpha beta pruning happens here:
                    alpha = Math.Max(alpha, score);
                    if (beta <= alpha)
                    {
                        transpositionTable.StoreEntry(zobristKey, maxScore, depth, TranspositionTableFlag.LOWERBOUND, m.Value);
                        return maxScore;
                    }

                }

                TranspositionTableFlag flag;
                if (maxScore > originalAlpha)
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
                //foreach (Move m in MoveGenerator.GenerateLegalMoves(board))
                //foreach (Move m in SortedMoves(board))
                while ((m = moveSorter.GetNextMove()) != null)
                {
                    if (!board.MakeMove(m.Value, out Undo undo))
                        continue;
                    int score = MiniMaxWithAlphaBeta(board, depth - 1, alpha, beta, !maxPlayer);
                    board.UnmakeMove(m.Value, undo);

                    if (score < minScore)
                    {
                        minScore = score;
                        bestMoveInNode = m.Value;
                    }

                    // Pruning
                    beta = Math.Min(beta, score);
                    if (beta <= alpha)
                    {
                        transpositionTable.StoreEntry(zobristKey, minScore, depth, TranspositionTableFlag.UPPERBOUND, m.Value);
                        return minScore;
                    }

                }
                transpositionTable.StoreEntry(zobristKey, minScore, depth, TranspositionTableFlag.EXACT, bestMoveInNode);
                return minScore;
            }
        }

        public List<Move> SortedMoves(Board b)
        {
            List<Move> possibleMoves = new List<Move>();
            MoveGenerator.GeneratePseudoMoves(b, b.sideToMove, possibleMoves);

            if (transpositionTable.TryGetEntry(b.currentZobristKey, out TranspositionTableEntry entry))
            {
                for (int i = 0; i < possibleMoves.Count; i++)
                {
                    if (possibleMoves[i].Equals(entry.bestMove) && i != 0)
                    {
                        var tmp = possibleMoves[i];
                        possibleMoves[i] = possibleMoves[0];
                        possibleMoves[0] = tmp;
                    }
                }
            }

            return possibleMoves;
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
