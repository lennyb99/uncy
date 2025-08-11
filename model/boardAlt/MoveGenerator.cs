using Microsoft.VisualBasic.Devices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace uncy.model.boardAlt
{
    public readonly struct Move
    {
        public readonly byte fromFile, fromRank, toFile, toRank;
        public readonly char movedPiece, capturedPiece, promotionPiece;
        public readonly bool castlingMoveFlag, enPassantCaptureFlag, doubleSquarePushFlag;

        public Move(
            byte fromFile,
            byte fromRank,
            byte toFile,
            byte toRank,
            char movedPiece,
            char capturedPiece = 'e',
            char promotionPiece = 'e',
            bool castlingMoveFlag = false,
            bool enPassantCaptureFlag = false,
            bool doubleSquarePushFlag = false)
        {
            this.fromFile = fromFile;
            this.fromRank = fromRank;
            this.toFile = toFile;
            this.toRank = toRank;
            this.movedPiece = movedPiece;
            this.capturedPiece = capturedPiece;
            this.promotionPiece = promotionPiece;
            this.castlingMoveFlag = castlingMoveFlag;
            this.enPassantCaptureFlag = enPassantCaptureFlag;
            this.doubleSquarePushFlag = doubleSquarePushFlag;
        }

        public bool IsCaptureOrPawnMove()
        {
            if (capturedPiece != 'e' || char.ToLower(movedPiece) == 'p') return true;


            return false;
        }

        public override string ToString()
        {
            return "(" + movedPiece + ": " + GetFileChar(fromFile) + (fromRank+1) + "->" + GetFileChar(toFile) + (toRank+1) + ")";

        }

        private char GetFileChar(byte file)
        {
            switch (file)
            {
                case 0:
                    return 'a';
                case 1:
                    return 'b';
                case 2:
                    return 'c';
                case 3:
                    return 'd';
                case 4:
                    return 'e';
                case 5:
                    return 'f';
                case 6:
                    return 'g';
                case 7:
                    return 'h';
            }
            return 'x';
        }
    }
        public readonly struct Undo
        {
            public readonly bool whiteKingShortCastle, whiteKingLongCastle, blackKingShortCastle, blackKingLongCastle;
            public readonly (int, int) enPassantTargetSquare;
            public readonly byte halfMoveClock;
            public readonly ulong zobristKey;

            public Undo(
                bool whiteKingShortCastle,
                bool whiteKingLongCastle,
                bool blackKingShortCastle,
                bool blackKingLongCastle,
                (int, int) enPassantTargetSquare,
                byte halfmoveClock,
                ulong zobristKey)
            {
                this.whiteKingShortCastle = whiteKingShortCastle;
                this.whiteKingLongCastle = whiteKingLongCastle;
                this.blackKingShortCastle = blackKingShortCastle;
                this.blackKingLongCastle = blackKingLongCastle;
                this.enPassantTargetSquare = enPassantTargetSquare;
                this.halfMoveClock = halfmoveClock;
                this.zobristKey = zobristKey;
            }
        }


        internal class MoveGenerator
        {
            public static List<Move> GenerateLegalMoves(Board board)
            {
                // Instantiate List 
                List<Move> legalMoves = new List<Move>();
                List<Move> newMoves = new List<Move>();
                GeneratePseudoMoves(board, board.sideToMove, newMoves);

                foreach (var move in newMoves) {
                    if (!board.MakeMove(move, out Undo undo))
                        continue;

                    legalMoves.Add(move);
                    board.UnmakeMove(move, undo);   
                }
                return legalMoves;
            }


            public static void GeneratePseudoMoves(Board board, bool sideToMove, List<Move> moves)
            {
                // Loop through each square
                for (int i = 0; i < board.dimensionsOfBoard.Item1; i++)
                {
                    for (int j = 0; j < board.dimensionsOfBoard.Item2; j++)
                    {
                        if (board.board[i, j] == 'x' || board.board[i, j] == 'e') continue; // Empty square or Inactive square

                        if (IsPieceWhite(board.board[i, j]) && sideToMove || !IsPieceWhite(board.board[i, j]) && !sideToMove) // Check if its turn for this piece to move 
                        {
                            IdentifyPieceAndGeneratePseudoMoves(i, j, board, moves);
                        }
                    }
                }
            }

            private static void IdentifyPieceAndGeneratePseudoMoves(int file, int rank, Board board, List<Move> moves)
            {
                switch (char.ToLower(board.board[file, rank]))
                {
                    case 'k':
                        GeneratePseudoMovesForKing(file, rank, board, moves);
                        break;
                    case 'q':
                        GeneratePseudoMovesForQueen(file, rank, board, moves);
                        break;
                    case 'r':
                        GeneratePseudoMovesForRook(file, rank, board, moves);
                        break;
                    case 'b':
                        GeneratePseudoMovesForBishop(file, rank, board, moves);
                        break;
                    case 'n':
                        GeneratePseudoMovesForKnight(file, rank, board, moves);
                        break;
                    case 'p':
                        GeneratePseudoMovesForPawn(file, rank, board, moves);
                        break;
                    default:
                        Console.WriteLine("Unidentified Piece found: " + board.board[file, rank]);
                        break;
                }
                //Console.WriteLine("Found " + moves.Count + " moves for: " +board.board[file, rank]);
            }

            private static void GeneratePseudoMovesForKing(int file, int rank, Board board, List<Move> moves)
            {
                char currentPiece = board.board[file, rank];
                bool isWhitePiece = IsPieceWhite(currentPiece);



                // Offsets for "standard" king moves
                int[] dFile = { 0, 1, 1, 1, 0, -1, -1, -1 };
                int[] dRank = { 1, 1, 0, -1, -1, -1, 0, 1 };

                for (int i = 0; i < 8; i++)
                {
                    int nextFile = file + dFile[i];
                    int nextRank = rank + dRank[i];
                    if (nextFile < 0 || nextFile >= board.dimensionsOfBoard.Item1 || nextRank < 0 || nextRank >= board.dimensionsOfBoard.Item2) continue; // if point is outside of specific board dimensions



                    char targetPiece = board.board[nextFile, nextRank];

                    if (targetPiece == 'x') continue;

                    if (targetPiece == 'e') // Empty square
                    {
                        moves.Add(new Move((byte)file, (byte)rank, (byte)nextFile, (byte)nextRank, currentPiece));
                    }
                    else    // Piece on the square      
                    {
                        bool isTargetWhite = IsPieceWhite(targetPiece);

                        if (isWhitePiece && !isTargetWhite || !isWhitePiece && isTargetWhite)
                        {
                            moves.Add(new Move((byte)file, (byte)rank, (byte)nextFile, (byte)nextRank, currentPiece, capturedPiece: targetPiece));
                        }
                    }
                }

                // Castling moves
                if (isWhitePiece) // white King
                {
                    if (board.whiteKingShortCastle)
                    {
                        for (int i = file+1; i < board.dimensionsOfBoard.Item1; i++)
                        {
                            char nextPiece = board.board[i, rank];
                            if (nextPiece == 'e')
                            {
                                continue;
                            }
                            else if (nextPiece == 'R')
                            {
                                moves.Add(new Move((byte)file, (byte)rank, (byte)(file + 2), (byte)rank, currentPiece, castlingMoveFlag: true));
                            }
                            else break;
                        }
                    }
                    if (board.whiteKingLongCastle)
                    {
                        for (int i = file-1; i >= 0; i--)
                        {
                            char nextPiece = board.board[i, rank];
                            if (nextPiece == 'e')
                            {
                                continue;
                            }
                            else if (nextPiece == 'R')
                            {
                                moves.Add(new Move((byte)file, (byte)rank, (byte)(file - 2), (byte)rank, currentPiece, castlingMoveFlag: true));
                            }
                            else break;
                        }
                    }
                }
                else // black King
                {
                    if (board.blackKingShortCastle)
                    {
                        for (int i = file+1; i < board.dimensionsOfBoard.Item1; i++)
                        {
                            char nextPiece = board.board[i, rank];
                            if (nextPiece == 'e')
                            {
                                continue;
                            }
                            else if (nextPiece == 'r')
                            {
                                moves.Add(new Move((byte)file, (byte)rank, (byte)(file + 2), (byte)rank, currentPiece, castlingMoveFlag: true));
                            }
                            else break;
                        }
                    }
                    if (board.blackKingLongCastle)
                    {
                        for (int i = file-1; i >= 0; i--)
                        {
                            char nextPiece = board.board[i, rank];
                            if (nextPiece == 'e')
                            {
                                continue;
                            }
                            else if (nextPiece == 'r')
                            {
                                moves.Add(new Move((byte)file, (byte)rank, (byte)(file - 2), (byte)rank, currentPiece, castlingMoveFlag: true));
                            }
                            else break;
                        }
                    }
                }
            }


            private static void GeneratePseudoMovesForPawn(int file, int rank, Board board, List<Move> moves)
            {
                char currentPiece = board.board[file, rank];
                bool isWhitePiece = IsPieceWhite(currentPiece);


                if (isWhitePiece)
                {
                    // One Square Push
                    if (!(file < 0 || file >= board.dimensionsOfBoard.Item1 || rank + 1 < 0 || rank + 1 >= board.dimensionsOfBoard.Item2)) // if point is inside of specific board dimensions
                    {
                        char targetPiece = board.board[file, rank + 1];
                        if (targetPiece == 'e')
                        {
                            if (board.IsSquareAtEndOfBoardForWhite(file, rank + 1))
                            {
                                GeneratePromotionMoves(file, rank, file, rank + 1, currentPiece, isWhitePiece, moves);
                            }
                            else
                            {
                                moves.Add(new Move((byte)file, (byte)rank, (byte)file, (byte)(rank + 1), currentPiece)); // Add valid Move
                            }

                            // Two Squares Push
                            if (rank == 1)
                            {
                                if (!(file < 0 || file >= board.dimensionsOfBoard.Item1 || rank + 2 < 0 || rank + 2 >= board.dimensionsOfBoard.Item2))
                                {
                                    targetPiece = board.board[file, rank + 2];
                                    if (targetPiece == 'e')
                                    {
                                        if (board.IsSquareAtEndOfBoardForWhite(file, rank + 2))
                                        {
                                            GeneratePromotionMoves(file, rank, file, rank + 2, currentPiece, isWhitePiece, moves, doublePushPawnMove: true);
                                        }
                                        else
                                        {
                                            moves.Add(new Move((byte)file, (byte)rank, (byte)file, (byte)(rank + 2), currentPiece, doubleSquarePushFlag: true)); // Add valid Move
                                        }
                                    }
                                }
                            }
                        }
                    }

                    

                    // Hit Diagonal
                    // Left
                    if (!(file - 1 < 0 || file - 1 >= board.dimensionsOfBoard.Item1 || rank + 1 < 0 || rank + 1 >= board.dimensionsOfBoard.Item2))
                    {
                        char targetPiece = board.board[file - 1, rank + 1];
                        if (!IsPieceWhite(targetPiece) && board.IsSquareOccuptiedByPiece(file - 1, rank + 1)) // Checks if piece is black since this is code is executed for white Pawns
                        {
                            if (board.IsSquareAtEndOfBoardForWhite(file - 1, rank + 1))
                            {
                                GeneratePromotionMoves(file, rank, file - 1, rank + 1, currentPiece, isWhitePiece, moves, capturedPiece: targetPiece);
                            }
                            else
                            {
                                moves.Add(new Move((byte)file, (byte)rank, (byte)(file - 1), (byte)(rank + 1), currentPiece, capturedPiece: targetPiece));
                            }
                        }
                    }

                    // Right
                    if (!(file + 1 < 0 || file + 1 >= board.dimensionsOfBoard.Item1 || rank + 1 < 0 || rank + 1 >= board.dimensionsOfBoard.Item2))
                    {
                        char targetPiece = board.board[file + 1, rank + 1];
                        if (!IsPieceWhite(targetPiece) && board.IsSquareOccuptiedByPiece(file + 1, rank + 1)) // Checks if piece is black since this is code is executed for white Pawns
                        {
                            if (board.IsSquareAtEndOfBoardForWhite(file + 1, rank + 1))
                            {
                                GeneratePromotionMoves(file, rank, file + 1, rank + 1, currentPiece, isWhitePiece, moves, capturedPiece: targetPiece);
                            }
                            else
                            {
                                moves.Add(new Move((byte)file, (byte)rank, (byte)(file + 1), (byte)(rank + 1), currentPiece, capturedPiece: targetPiece));
                            }
                        }
                    }

                    // En Passant Capture
                    if (board.enPassantTargetSquare != (-1, -1))
                    {
                        char enPaTargetPiece = board.board[board.enPassantTargetSquare.Item1, board.enPassantTargetSquare.Item2-1];
                        if (board.enPassantTargetSquare.Item2-1 == rank && !IsPieceWhite(enPaTargetPiece))
                        {
                            if (board.enPassantTargetSquare.Item1 - file == 1 || board.enPassantTargetSquare.Item1 - file == -1) // En passant Piece is directly next to pawn
                                {
                                    moves.Add((new Move((byte)file, (byte)rank, (byte)board.enPassantTargetSquare.Item1, (byte)(board.enPassantTargetSquare.Item2), currentPiece, capturedPiece: enPaTargetPiece, enPassantCaptureFlag: true)));
                                }
                        }
                    }
                }
                else // Handling for black pieces
                {
                    // One Square Push
                    if (!(file < 0 || file >= board.dimensionsOfBoard.Item1 || rank - 1 < 0 || rank - 1 >= board.dimensionsOfBoard.Item2)) // if point is inside of specific board dimensions
                    {
                        char targetPiece = board.board[file, rank - 1];
                        if (targetPiece == 'e')
                        {
                            if (board.IsSquareAtEndOfBoardForBlack(file, rank - 1))
                            {
                                GeneratePromotionMoves(file, rank, file, rank - 1, currentPiece, isWhitePiece, moves);
                            }
                            else
                            {
                                moves.Add(new Move((byte)file, (byte)rank, (byte)file, (byte)(rank - 1), currentPiece)); // Add valid Move
                            }
                            // Two Squares Push
                            if (rank == 6)
                            {
                                if (!(file < 0 || file >= board.dimensionsOfBoard.Item1 || rank - 2 < 0 || rank - 2 >= board.dimensionsOfBoard.Item2))
                                {
                                    targetPiece = board.board[file, rank - 2];
                                    if (targetPiece == 'e')
                                    {
                                        if (board.IsSquareAtEndOfBoardForBlack(file, rank - 2))
                                        {
                                            GeneratePromotionMoves(file, rank, file, rank - 2, currentPiece, isWhitePiece, moves, doublePushPawnMove: true);
                                        }
                                        else
                                        {
                                            moves.Add(new Move((byte)file, (byte)rank, (byte)file, (byte)(rank - 2), currentPiece, doubleSquarePushFlag: true)); // Add valid Move
                                        }
                                    }
                                }
                            }
                        } 
                    }

                    

                    // Hit Diagonal
                    // Left
                    if (!(file - 1 < 0 || file - 1 >= board.dimensionsOfBoard.Item1 || rank - 1 < 0 || rank - 1 >= board.dimensionsOfBoard.Item2))
                    {
                        char targetPiece = board.board[file - 1, rank - 1];
                        if (IsPieceWhite(targetPiece) && board.IsSquareOccuptiedByPiece(file - 1, rank - 1)) // Checks if piece is white since this is code is executed for black Pawns
                        {
                            if (board.IsSquareAtEndOfBoardForBlack(file - 1, rank - 1))
                            {
                                GeneratePromotionMoves(file, rank, file - 1, rank - 1, currentPiece, isWhitePiece, moves, capturedPiece: targetPiece);
                            }
                            else
                            {
                                moves.Add(new Move((byte)file, (byte)rank, (byte)(file - 1), (byte)(rank - 1), currentPiece, capturedPiece: targetPiece));
                            }
                        }
                    }

                    // Right
                    if (!(file + 1 < 0 || file + 1 >= board.dimensionsOfBoard.Item1 || rank - 1 < 0 || rank - 1 >= board.dimensionsOfBoard.Item2))
                    {
                        char targetPiece = board.board[file + 1, rank - 1];
                        if (IsPieceWhite(targetPiece) && board.IsSquareOccuptiedByPiece(file + 1, rank - 1)) // Checks if piece is white since this is code is executed for black Pawns
                        {
                            if (board.IsSquareAtEndOfBoardForBlack(file + 1, rank - 1))
                            {
                                GeneratePromotionMoves(file, rank, file + 1, rank - 1, currentPiece, isWhitePiece, moves, capturedPiece: targetPiece);
                            }
                            else
                            {
                                moves.Add(new Move((byte)file, (byte)rank, (byte)(file + 1), (byte)(rank - 1), currentPiece, capturedPiece: targetPiece));
                            }
                        }
                    }

                    // En Passant Capture
                    if (board.enPassantTargetSquare != (-1, -1))
                    {
                        char enPaTargetPiece = board.board[board.enPassantTargetSquare.Item1, board.enPassantTargetSquare.Item2+1];
                        if (board.enPassantTargetSquare.Item2+1 == rank && IsPieceWhite(enPaTargetPiece))
                        {
                            if (board.enPassantTargetSquare.Item1 - file == 1 || board.enPassantTargetSquare.Item1 - file == -1) // En passant Piece is directly next to pawn
                            {
                                moves.Add((new Move((byte)file, (byte)rank, (byte)board.enPassantTargetSquare.Item1, (byte)(board.enPassantTargetSquare.Item2), currentPiece, capturedPiece: enPaTargetPiece, enPassantCaptureFlag: true)));
                            }
                        }
                    }
                }
            }

            private static void GeneratePromotionMoves(int fromFile, int fromRank, int toFile, int toRank, char currentPiece, bool isWhitePiece, List<Move> moves, char capturedPiece = 'e', bool doublePushPawnMove = false)
            {

                char[] pieces = { 'q', 'r', 'b', 'n' };
                if (isWhitePiece)
                {
                    for (int i = 0; i < pieces.Length; i++)
                    {
                        pieces[i] = char.ToUpper(pieces[i]);
                    }
                }
                if (capturedPiece == 'e')
                {
                    for (int i = 0; i < pieces.Length; i++)
                    {
                        moves.Add(new Move((byte)fromFile, (byte)fromRank, (byte)toFile, (byte)toRank, currentPiece, promotionPiece: pieces[i], doubleSquarePushFlag: doublePushPawnMove));
                    }
                }
                else
                {
                    for (int i = 0; i < pieces.Length; i++)
                    {
                        moves.Add(new Move((byte)fromFile, (byte)fromRank, (byte)toFile, (byte)toRank, currentPiece, capturedPiece: capturedPiece, promotionPiece: pieces[i], doubleSquarePushFlag: doublePushPawnMove));
                    }
                }
            }

            private static void GeneratePseudoMovesForKnight(int file, int rank, Board board, List<Move> moves)
            {
                char currentPiece = board.board[file, rank];
                bool isWhitePiece = IsPieceWhite(currentPiece);


                int[] dFile = { 1, 2, 2, 1, -1, -2, -2, -1 };
                int[] dRank = { 2, 1, -1, -2, -2, -1, 1, 2 };


                for (int i = 0; i < 8; i++)
                {
                    int nextFile = file + dFile[i];
                    int nextRank = rank + dRank[i];
                    if (nextFile < 0 || nextFile >= board.dimensionsOfBoard.Item1 || nextRank < 0 || nextRank >= board.dimensionsOfBoard.Item2) continue; // if point is outside of specific board dimensions



                    char targetPiece = board.board[nextFile, nextRank];

                    if (targetPiece == 'x') continue;

                    if (targetPiece == 'e') // Empty square
                    {
                        moves.Add(new Move((byte)file, (byte)rank, (byte)nextFile, (byte)nextRank, currentPiece));
                    }
                    else    // Piece on the square      
                    {
                        bool isTargetWhite = IsPieceWhite(targetPiece);

                        if (isWhitePiece && !isTargetWhite || !isWhitePiece && isTargetWhite)
                        {
                            moves.Add(new Move((byte)file, (byte)rank, (byte)nextFile, (byte)nextRank, currentPiece, capturedPiece: targetPiece));
                        }
                    }
                }

            }

            private static void GeneratePseudoMovesForRook(int file, int rank, Board board, List<Move> moves)
            {
                char currentPiece = board.board[file, rank];
                int[] dFile = { 0, 0, -1, 1 };
                int[] dRank = { -1, 1, 0, 0 };

                GenerateSlidingMoves(file, rank, dFile, dRank, board, moves);
            }

            private static void GeneratePseudoMovesForBishop(int file, int rank, Board board, List<Move> moves)
            {
                char currentPiece = board.board[file, rank];


                int[] dFile = { 1, 1, -1, -1 };
                int[] dRank = { 1, -1, -1, 1 };

                GenerateSlidingMoves(file, rank, dFile, dRank, board, moves);
            }

            private static void GeneratePseudoMovesForQueen(int file, int rank, Board board, List<Move> moves)
            {
                char currentPiece = board.board[file, rank];


                int[] dFile = { 1, 1, -1, -1, 0, 0, -1, 1 };
                int[] dRank = { 1, -1, -1, 1, -1, 1, 0, 0 };

                GenerateSlidingMoves(file, rank, dFile, dRank, board, moves);

            }


            private static void GenerateSlidingMoves(int file, int rank, int[] dFile, int[] dRank, Board board, List<Move> moves)
            {
                char currentPiece = board.board[file, rank];

                bool isWhitePiece = IsPieceWhite(currentPiece);

                for (int i = 0; i < dFile.Length; i++)
                {
                    for (int step = 1; step < 32; step++) // 32 because of the maximum technical board size
                    {
                        int nextFile = file + dFile[i] * step;
                        int nextRank = rank + dRank[i] * step;


                        if (nextFile < 0 || nextFile >= board.dimensionsOfBoard.Item1 || nextRank < 0 || nextRank >= board.dimensionsOfBoard.Item2) break; // if point is outside of specific board dimensions

                        char targetPiece = board.board[nextFile, nextRank];

                        if (targetPiece == 'x') break;

                        if (targetPiece == 'e') // Empty square
                        {
                            moves.Add(new Move((byte)file, (byte)rank, (byte)nextFile, (byte)nextRank, currentPiece));
                        }
                        else    // Piece on the square      
                        {
                            bool isTargetWhite = IsPieceWhite(targetPiece);

                            if (isWhitePiece && !isTargetWhite || !isWhitePiece && isTargetWhite)
                            {
                                moves.Add(new Move((byte)file, (byte)rank, (byte)nextFile, (byte)nextRank, currentPiece, capturedPiece: targetPiece));
                            }
                            break;
                        }

                    }
                }
            }



            static bool IsPieceWhite(char c) => c >= 'A' && c <= 'Z';
        }
}
