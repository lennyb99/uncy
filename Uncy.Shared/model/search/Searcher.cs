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

        // Added for Move Ordering
        public Move[,] killerMoves = new Move[100, 2];
        public int[,] historyTable = new int[32, 128];

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
                // Nur gültige Züge übernehmen – sonst würde z. B. ein TT-Cutoff oder eine tiefere Suche
                // ohne Root-Move ein früher gefundenes gültiges Move mit default überschreiben.
                if (bestMoveForCurrentDepth.IsValid)
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

            ulong rootZobristKey = board.currentZobristKey;
            MoveSorter moveSorter = new MoveSorter(board, transpositionTable, this, 0);
            Move? m;
            int movesTried = 0;

            //foreach (Move move in MoveGenerator.GeneratePseudoMoves(board, board.sideToMove))
            //foreach (Move moves in SortedMoves(board))
            while ((m = moveSorter.GetNextMove()) != null)
            {
                movesTried++;
                if (!board.MakeMove(m.Value, out Undo undo)) // If this returns wrong, the move wasn't legal, therefore will be skipped. MakeMove is handling the UnmakeMove()
                    continue;

                try
                {
                    int score = MiniMaxWithAlphaBeta(board, depth - 1, 1, alpha, beta, !maximizingSide);

                    bool isBetter = maximizingSide ? score > bestScore : score < bestScore;
                    if (isBetter)
                    {
                        bestScore = score;
                        bestMove = m.Value;
                    }

                    alpha = Math.Max(alpha, bestScore);
                    if (alpha >= beta) break;
                }
                finally
                {
                    board.UnmakeMove(m.Value, undo);
                }
            }

            // Fallback: Es gab Pseudo-Moves, aber alle wurden als illegal verworfen – trotzdem ersten legalen Zug finden.
            // Nur wenn Board unverändert (Zobrist gleich), sonst wurde das Brett in der Rekursion nicht korrekt zurückgesetzt.
            if (!bestMove.IsValid && movesTried > 0 && board.currentZobristKey == rootZobristKey)
            {
                MoveSorter fallbackSorter = new MoveSorter(board, transpositionTable, this, 0);
                while ((m = fallbackSorter.GetNextMove()) != null)
                {
                    if (board.MakeMove(m.Value, out Undo undo))
                    {
                        board.UnmakeMove(m.Value, undo);
                        bestMove = m.Value;
                        break;
                    }
                }
            }

            transpositionTable.StoreEntry(board.currentZobristKey, bestScore, depth, TranspositionTableFlag.EXACT, bestMove);
            return bestMove;
        }

        public int MiniMaxWithAlphaBeta(Board board, int depth, int ply, int alpha, int beta, bool maxPlayer, bool allowNull = true)
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

            if (depth <= 0)
            {
                return QuiescenceSearch(board, alpha, beta, maxPlayer);
            }

            bool inCheck = board.IsKingInCheck(board.sideToMove);

            // --- Null Move Pruning ---
            int R = 2; // Null move reduction
            if (allowNull && depth >= 1 + R && !inCheck)
            {
                board.MakeNullMove(out Undo nullUndo);
                int nullScore = MiniMaxWithAlphaBeta(board, depth - 1 - R, ply + 1, alpha, beta, !maxPlayer, false);
                board.UnmakeNullMove(nullUndo);

                if (maxPlayer)
                {
                    if (nullScore >= beta) return nullScore; // Fail-high
                }
                else
                {
                    if (nullScore <= alpha) return nullScore; // Fail-low
                }
            }

            MoveSorter moveSorter = new MoveSorter(board, transpositionTable, this, ply);
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
                bool foundPv = false;
                int movesTried = 0;
                while ((m = moveSorter.GetNextMove()) != null)
                {
                    movesTried++;
                    if (!board.MakeMove(m.Value, out Undo undo))
                        continue;
                    try
                    {
                        int score;
                        if (foundPv)
                        {
                            bool isQuiet = m.Value.capturedPiece == Piece.Empty && m.Value.promotionPiece == Piece.Empty;
                            bool givesCheck = board.IsKingInCheck(board.sideToMove);

                            if (depth >= 3 && movesTried > 4 && isQuiet && !inCheck && !givesCheck)
                            {
                                score = MiniMaxWithAlphaBeta(board, depth - 2, ply + 1, alpha, alpha + 1, !maxPlayer);
                                if (score > alpha)
                                {
                                    score = MiniMaxWithAlphaBeta(board, depth - 1, ply + 1, alpha, beta, !maxPlayer);
                                }
                            }
                            else
                            {
                                score = MiniMaxWithAlphaBeta(board, depth - 1, ply + 1, alpha, alpha + 1, !maxPlayer);
                                if (score > alpha && score < beta)
                                {
                                    score = MiniMaxWithAlphaBeta(board, depth - 1, ply + 1, alpha, beta, !maxPlayer);
                                }
                            }
                        }
                        else
                        {
                            score = MiniMaxWithAlphaBeta(board, depth - 1, ply + 1, alpha, beta, !maxPlayer);
                        }

                        if (score > maxScore)
                        {
                            maxScore = score;
                            bestMoveInNode = m.Value;
                        }

                        // Alpha beta pruning happens here:
                        alpha = Math.Max(alpha, score);
                        if (beta <= alpha)
                        {
                            if (m.Value.capturedPiece == Piece.Empty) StoreKillerAndHistory(m.Value, ply, depth);
                            transpositionTable.StoreEntry(zobristKey, maxScore, depth, TranspositionTableFlag.LOWERBOUND, m.Value);
                            return maxScore;
                        }
                        foundPv = true;
                    }
                    finally
                    {
                        board.UnmakeMove(m.Value, undo);
                    }
                }

                // Kein legaler Zug für den Max-Spieler → Matt oder Patt
                if (maxScore == int.MinValue)
                {
                    return board.IsKingInCheck(maxPlayer) ? int.MinValue + depth : 0;
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
                bool foundPv = false;
                int movesTried = 0;
                while ((m = moveSorter.GetNextMove()) != null)
                {
                    movesTried++;
                    if (!board.MakeMove(m.Value, out Undo undo))
                        continue;
                    try
                    {
                        int score;
                        if (foundPv)
                        {
                            bool isQuiet = m.Value.capturedPiece == Piece.Empty && m.Value.promotionPiece == Piece.Empty;
                            bool givesCheck = board.IsKingInCheck(board.sideToMove);

                            if (depth >= 3 && movesTried > 4 && isQuiet && !inCheck && !givesCheck)
                            {
                                score = MiniMaxWithAlphaBeta(board, depth - 2, ply + 1, beta - 1, beta, !maxPlayer);
                                if (score < beta)
                                {
                                    score = MiniMaxWithAlphaBeta(board, depth - 1, ply + 1, alpha, beta, !maxPlayer);
                                }
                            }
                            else
                            {
                                score = MiniMaxWithAlphaBeta(board, depth - 1, ply + 1, beta - 1, beta, !maxPlayer);
                                if (score < beta && score > alpha)
                                {
                                    score = MiniMaxWithAlphaBeta(board, depth - 1, ply + 1, alpha, beta, !maxPlayer);
                                }
                            }
                        }
                        else
                        {
                            score = MiniMaxWithAlphaBeta(board, depth - 1, ply + 1, alpha, beta, !maxPlayer);
                        }

                        if (score < minScore)
                        {
                            minScore = score;
                            bestMoveInNode = m.Value;
                        }

                        // Pruning
                        beta = Math.Min(beta, score);
                        if (beta <= alpha)
                        {
                            if (m.Value.capturedPiece == Piece.Empty) StoreKillerAndHistory(m.Value, ply, depth);
                            transpositionTable.StoreEntry(zobristKey, minScore, depth, TranspositionTableFlag.UPPERBOUND, m.Value);
                            return minScore;
                        }
                        foundPv = true;
                    }
                    finally
                    {
                        board.UnmakeMove(m.Value, undo);
                    }
                }
                // Kein legaler Zug für den Min-Spieler → Matt oder Patt
                if (minScore == int.MaxValue)
                {
                    return board.IsKingInCheck(board.sideToMove) ? int.MaxValue - depth : 0;
                }
                transpositionTable.StoreEntry(zobristKey, minScore, depth, TranspositionTableFlag.EXACT, bestMoveInNode);
                return minScore;
            }
        }

        public int QuiescenceSearch(Board board, int alpha, int beta, bool maxPlayer)
        {
            int standPat = evaluator.Evaluate(board);

            if (maxPlayer)
            {
                if (standPat >= beta) return beta;
                if (alpha < standPat) alpha = standPat;

                MoveSorter moveSorter = new MoveSorter(board, transpositionTable, this, 100);
                Move? m;

                while ((m = moveSorter.GetNextMove()) != null)
                {
                    if (m.Value.capturedPiece == Piece.Empty && m.Value.promotionPiece == Piece.Empty)
                        continue;

                    if (!board.MakeMove(m.Value, out Undo undo))
                        continue;

                    try
                    {
                        int score = QuiescenceSearch(board, alpha, beta, !maxPlayer);

                        if (score >= beta) return beta;
                        if (score > alpha) alpha = score;
                    }
                    finally
                    {
                        board.UnmakeMove(m.Value, undo);
                    }
                }
                return alpha;
            }
            else
            {
                if (standPat <= alpha) return alpha;
                if (beta > standPat) beta = standPat;

                MoveSorter moveSorter = new MoveSorter(board, transpositionTable, this, 100);
                Move? m;

                while ((m = moveSorter.GetNextMove()) != null)
                {
                    if (m.Value.capturedPiece == Piece.Empty && m.Value.promotionPiece == Piece.Empty)
                        continue;

                    if (!board.MakeMove(m.Value, out Undo undo))
                        continue;

                    try
                    {
                        int score = QuiescenceSearch(board, alpha, beta, !maxPlayer);

                        if (score <= alpha) return alpha;
                        if (score < beta) beta = score;
                    }
                    finally
                    {
                        board.UnmakeMove(m.Value, undo);
                    }
                }
                return beta;
            }
        }

        private void StoreKillerAndHistory(Move move, int ply, int depth)
        {
            if (ply < 100)
            {
                // Store Killer Move
                if (!killerMoves[ply, 0].Equals(move))
                {
                    killerMoves[ply, 1] = killerMoves[ply, 0];
                    killerMoves[ply, 0] = move;
                }
            }
            // Store History Move
            historyTable[move.movedPiece, move.toSquare] += depth * depth;
        }

        public List<Move> SortedMoves(Board b)
        {
            List<Move> possibleMoves = new List<Move>();
            MoveGenerator.GeneratePseudoMoves(b, b.sideToMove, possibleMoves);

            if (transpositionTable.TryGetEntry(b.currentZobristKey, out TranspositionTableEntry entry) && entry.bestMove.IsValid)
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
    }
}
