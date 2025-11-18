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
        public readonly ushort fromSquare, toSquare;
        public readonly byte movedPiece, capturedPiece, promotionPiece;
        public readonly bool castlingMoveFlag, enPassantCaptureFlag, doubleSquarePushFlag;

        public Move(
            ushort fromSquare,
            ushort toSquare,
            byte movedPiece,
            byte capturedPiece = Piece.Empty,
            byte promotionPiece = Piece.Empty,
            bool castlingMoveFlag = false,
            bool enPassantCaptureFlag = false,
            bool doubleSquarePushFlag = false)
        {
            this.fromSquare = fromSquare;
            this.toSquare = toSquare;
            this.movedPiece = movedPiece;
            this.capturedPiece = capturedPiece;
            this.promotionPiece = promotionPiece;
            this.castlingMoveFlag = castlingMoveFlag;
            this.enPassantCaptureFlag = enPassantCaptureFlag;
            this.doubleSquarePushFlag = doubleSquarePushFlag;
        }

        public bool IsCaptureOrPawnMove()
        {
            if (capturedPiece != Piece.Empty || Piece.IsPieceAPawn(movedPiece)) return true;
            return false;
        }

        public override string ToString()
        {
            //return "(" + movedPiece + ": " + GetFileChar(fromFile) + (fromRank+1) + "->" + GetFileChar(toFile) + (toRank+1) + ")";

            return $"({Piece.GiveCharIdentifier(movedPiece)}: Sq({fromSquare}) -> ({toSquare}))";

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
        public readonly int enPassantTargetSquare;
        public readonly byte halfMoveClock;
        public readonly ulong zobristKey;

        public Undo(
            bool whiteKingShortCastle,
            bool whiteKingLongCastle,
            bool blackKingShortCastle,
            bool blackKingLongCastle,
            int enPassantTargetSquare,
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
            for (int i = 0; i < board.boardSize; i++)
            {
                if (board.board[i] == Piece.Inactive || board.board[i] == Piece.Empty) continue; // Empty square or Inactive square

                if (IsPieceWhite(board.board[i]) && sideToMove || !IsPieceWhite(board.board[i]) && !sideToMove) // Check if its turn for this piece to move 
                {
                    IdentifyPieceAndGeneratePseudoMoves(i, board, moves);
                }
            }
        }

        private static void IdentifyPieceAndGeneratePseudoMoves(int square, Board board, List<Move> moves)
        {
            switch (Piece.GetPieceType(board.board[square]))
            {
                case Piece.King:
                    GeneratePseudoMovesForKing(square, board, moves);
                    break;
                case Piece.Queen:
                    GeneratePseudoMovesForQueen(square, board, moves);
                    break;
                case Piece.Rook:
                    GeneratePseudoMovesForRook(square, board, moves);
                    break;
                case Piece.Bishop:
                    GeneratePseudoMovesForBishop(square, board, moves);
                    break;
                case Piece.Knight:
                    GeneratePseudoMovesForKnight(square, board, moves);
                    break;
                case Piece.Pawn:
                    GeneratePseudoMovesForPawn(square, board, moves);
                    break;
                default:
                    Console.WriteLine("Unidentified Piece found: " + board.board[square]);
                    break;
            }
            //Console.WriteLine("Found " + moves.Count + " moves for: " +board.board[file, rank]);
        }

        private static void GeneratePseudoMovesForKing(int square, Board board, List<Move> moves)
        {
            byte currentPiece = board.board[square];
            bool isWhitePiece = IsPieceWhite(currentPiece);


            int fileSize = board.dimensionsOfBoard.Item1;

            // Offsets for "standard" king moves
            // King Moves
            List<int> dSquares = new List<int>();
            switch (square % fileSize)
            {
                case 0:
                    dSquares.AddRange(new int[] { fileSize, 1, -fileSize, fileSize + 1, -fileSize + 1 });
                    break;
                case var n when n == fileSize - 1:
                    dSquares.AddRange(new int[] { fileSize, -fileSize, -fileSize - 1, fileSize - 1, -1 });
                    break;
                default:
                    dSquares.AddRange(new int[] { fileSize, 1, -fileSize, -1, fileSize + 1, -fileSize + 1, -fileSize - 1, fileSize - 1 });
                    break;
            }

            

            for (int i = 0; i < dSquares.Count; i++)
            {
                int nextSquare = square + dSquares[i];
                    
                if (nextSquare < 0 || nextSquare > board.boardSize) continue; // if point is outside of specific board dimensions



                byte targetPiece = board.board[nextSquare];

                if (targetPiece == Piece.Inactive) continue;

                if (targetPiece == Piece.Empty) // Empty square
                {
                    moves.Add(new Move((ushort) square, (ushort) nextSquare, currentPiece));
                }
                else    // Piece on the square      
                {
                    bool isTargetWhite = IsPieceWhite(targetPiece);

                    if (isWhitePiece && !isTargetWhite || !isWhitePiece && isTargetWhite)
                    {
                        moves.Add(new Move((ushort) square, (ushort) nextSquare, currentPiece, capturedPiece: targetPiece));
                    }
                }
            }

            // Castling moves
            if (isWhitePiece) // white King
            {
                if (board.whiteKingShortCastle)
                {
                    for (int i = square+1; i <= square + board.stepUntilRightBoardBorder(square); i++)
                    {
                        byte nextPiece = board.board[i];
                        if (nextPiece == Piece.Empty)
                        {
                            continue;
                        }
                        else if (Piece.IsPieceAWhiteRook(nextPiece))
                        {
                            moves.Add(new Move((ushort)square, (ushort)(square+2), currentPiece, castlingMoveFlag: true));
                        }
                        else break;
                    }
                }
                if (board.whiteKingLongCastle)
                {
                    for (int i = square-1; i >= square - board.stepsUntilLeftBoardBorder(square); i--)
                    {
                        byte nextPiece = board.board[i];
                        if (nextPiece == Piece.Empty)
                        {
                            continue;
                        }
                        else if (Piece.IsPieceAWhiteRook(nextPiece))
                        {
                            moves.Add(new Move((ushort) square, (ushort) (square-2), currentPiece, castlingMoveFlag: true));
                        }
                        else break;
                    }
                }
            }
            else // black King
            {
                if (board.blackKingShortCastle)
                {
                    for (int i = square+1; i <= square + board.stepUntilRightBoardBorder(square); i++)
                    {
                        byte nextPiece = board.board[i];
                        if (nextPiece == Piece.Empty)
                        {
                            continue;
                        }
                        else if (Piece.IsPieceABlackRook(nextPiece))
                        {
                            moves.Add(new Move((ushort)(square), (ushort)(square+2), currentPiece, castlingMoveFlag: true));
                        }
                        else break;
                    }
                }
                if (board.blackKingLongCastle)
                {
                    for (int i = square-1; i >= square - board.stepsUntilLeftBoardBorder(square); i--)
                    {
                        byte nextPiece = board.board[i];
                        if (nextPiece == Piece.Empty)
                        {
                            continue;
                        }
                        else if (Piece.IsPieceABlackRook(nextPiece))
                        {
                            moves.Add(new Move((ushort)square, (ushort)(square-2), currentPiece, castlingMoveFlag: true));
                        }
                        else break;
                    }
                }
            }
        }


        private static void GeneratePseudoMovesForPawn(int square, Board board, List<Move> moves)
        {
            byte currentPiece = board.board[square];
            bool isWhitePiece = IsPieceWhite(currentPiece);

            int fileSize = board.dimensionsOfBoard.Item1;

            if (isWhitePiece)
            {
                // One Square Push
                if (!(square+fileSize >= board.boardSize)) // if point is inside of specific board dimensions
                {
                    byte targetPiece = board.board[square+fileSize];
                    if (targetPiece == Piece.Empty)
                    {
                        if (board.IsSquareAtEndOfBoardForWhite(square+fileSize))
                        {
                            GeneratePromotionMoves(square, square+fileSize, currentPiece, isWhitePiece, moves);
                        }
                        else
                        {
                            moves.Add(new Move((ushort) square, (ushort) (square+fileSize), currentPiece)); // Add valid Move
                        }

                        
                        // Two Squares Push
                        if (((int)square / 8) == 1)
                        {
                            if (!(square+fileSize*2 >= board.boardSize))
                            {
                                targetPiece = board.board[square + fileSize*2];
                                if (targetPiece == Piece.Empty)
                                {
                                    if (board.IsSquareAtEndOfBoardForWhite(square+fileSize*2))
                                    {
                                        GeneratePromotionMoves(square, square+fileSize*2, currentPiece, isWhitePiece, moves, doublePushPawnMove: true);
                                    }
                                    else
                                    {
                                        moves.Add(new Move((ushort)square, (ushort)(square+fileSize*2), currentPiece, doubleSquarePushFlag: true)); // Add valid Move
                                    }
                                }
                            }
                        }
                    }
                }

                    

                // Hit Diagonal
                // Left
                if ((square + fileSize-1 < board.boardSize) && square % fileSize != 0)
                {
                    byte targetPiece = board.board[square + fileSize-1];

                    if (board.enPassantTargetSquare == square + fileSize - 1)
                    {
                        byte enPaTargetPiece = board.board[board.enPassantTargetSquare-fileSize];
                        if (!IsPieceWhite(enPaTargetPiece))
                        {
                            moves.Add((new Move((ushort)square, (ushort)board.enPassantTargetSquare, currentPiece, capturedPiece: enPaTargetPiece, enPassantCaptureFlag: true)));
                        }
                    }

                    if (!IsPieceWhite(targetPiece) && board.IsSquareOccupiedByPiece(square + fileSize-1)) // Checks if piece is black since this is code is executed for white Pawns
                    {
                        if (board.IsSquareAtEndOfBoardForWhite(square+fileSize-1))
                        {
                            GeneratePromotionMoves(square, square+fileSize-1, currentPiece, isWhitePiece, moves, capturedPiece: targetPiece);
                        }
                        else
                        {
                            moves.Add(new Move((ushort)square, (ushort)(square+fileSize-1), currentPiece, capturedPiece: targetPiece));
                        }
                    }
                }

                // Right
                if ((square + fileSize + 1 < board.boardSize) && square % fileSize != fileSize-1)
                {
                    byte targetPiece = board.board[square+fileSize+1];

                    if (board.enPassantTargetSquare == square + fileSize + 1)
                    {
                        byte enPaTargetPiece = board.board[board.enPassantTargetSquare - fileSize];
                        if (!IsPieceWhite(enPaTargetPiece)) { 
                            moves.Add((new Move((ushort)square, (ushort)board.enPassantTargetSquare, currentPiece, capturedPiece: enPaTargetPiece, enPassantCaptureFlag: true)));
                        }
                    }

                    if (!IsPieceWhite(targetPiece) && board.IsSquareOccupiedByPiece(square+fileSize+1)) // Checks if piece is black since this is code is executed for white Pawns
                    {
                        if (board.IsSquareAtEndOfBoardForWhite(square + fileSize+1))
                        {
                            GeneratePromotionMoves(square, square+fileSize+1, currentPiece, isWhitePiece, moves, capturedPiece: targetPiece);
                        }
                        else
                        {
                            moves.Add(new Move((ushort)square, (ushort)(square+fileSize+1), currentPiece, capturedPiece: targetPiece));
                        }
                    }
                }

                // En Passant Capture
                //if (board.enPassantTargetSquare != -1)
                //{
                //    byte enPaTargetPiece = board.board[board.enPassantTargetSquare];
                //    if (board.enPassantTargetSquare.Item2-1 == rank && !IsPieceWhite(enPaTargetPiece))
                //    {
                //        if (board.enPassantTargetSquare.Item1 - file == 1 || board.enPassantTargetSquare.Item1 - file == -1) // En passant Piece is directly next to pawn
                //            {
                //                moves.Add((new Move((byte)file, (byte)rank, (byte)board.enPassantTargetSquare.Item1, (byte)(board.enPassantTargetSquare.Item2), currentPiece, capturedPiece: enPaTargetPiece, enPassantCaptureFlag: true)));
                //            }
                //    }
                //}
            }
            else // Handling for black pieces
            {
                // One Square Push
                if (!(square - fileSize < 0)) // if point is inside of specific board dimensions
                {
                    byte targetPiece = board.board[square-fileSize];
                    if (targetPiece == Piece.Empty)
                    {
                        if (board.IsSquareAtEndOfBoardForBlack(square-fileSize))
                        {
                            GeneratePromotionMoves(square, square-fileSize, currentPiece, isWhitePiece, moves);
                        }
                        else
                        {
                            moves.Add(new Move((ushort)square, (ushort)(square-fileSize), currentPiece)); // Add valid Move
                        }
                        // Two Squares Push
                        if (square / 8 == 6)
                        {
                            if (!(square - fileSize*2 < 0))
                            {
                                targetPiece = board.board[square - fileSize*2];
                                if (targetPiece == Piece.Empty)
                                {
                                    if (board.IsSquareAtEndOfBoardForBlack(square-fileSize*2))
                                    {
                                        GeneratePromotionMoves(square, square-fileSize*2, currentPiece, isWhitePiece, moves, doublePushPawnMove: true);
                                    }
                                    else
                                    {
                                        moves.Add(new Move((ushort)square, (ushort)(square-fileSize*2), currentPiece, doubleSquarePushFlag: true)); // Add valid Move
                                    }
                                }
                            }
                        }
                    } 
                }

                    

                // Hit Diagonal
                // Left
                if ((square - fileSize - 1 >= 0) && square % fileSize != 0)
                {
                    byte targetPiece = board.board[square-fileSize-1];

                    if (board.enPassantTargetSquare == square - fileSize - 1)
                    {
                        byte enPaTargetPiece = board.board[board.enPassantTargetSquare + fileSize];
                        if (IsPieceWhite(enPaTargetPiece))
                        {
                            moves.Add((new Move((ushort)square, (ushort)board.enPassantTargetSquare, currentPiece, capturedPiece: enPaTargetPiece, enPassantCaptureFlag: true)));
                        }
                    }

                    if (IsPieceWhite(targetPiece) && board.IsSquareOccupiedByPiece(square-fileSize-1)) // Checks if piece is white since this is code is executed for black Pawns
                    {
                        if (board.IsSquareAtEndOfBoardForBlack(square-fileSize-1))
                        {
                            GeneratePromotionMoves(square, square-fileSize-1, currentPiece, isWhitePiece, moves, capturedPiece: targetPiece);
                        }
                        else
                        {
                            moves.Add(new Move((ushort)square, (ushort)(square-fileSize-1), currentPiece, capturedPiece: targetPiece));
                        }
                    }
                }

                // Right
                if ((square - fileSize + 1 >= 0) && square % fileSize != fileSize - 1)
                {
                    byte targetPiece = board.board[square-fileSize+1];

                    if (board.enPassantTargetSquare == square - fileSize + 1)
                    {
                        byte enPaTargetPiece = board.board[board.enPassantTargetSquare + fileSize];
                        if (IsPieceWhite(enPaTargetPiece))
                        {
                            moves.Add((new Move((ushort)square, (ushort)board.enPassantTargetSquare, currentPiece, capturedPiece: enPaTargetPiece, enPassantCaptureFlag: true)));
                        }
                    }

                    if (IsPieceWhite(targetPiece) && board.IsSquareOccupiedByPiece(square-fileSize+1)) // Checks if piece is white since this is code is executed for black Pawns
                    {
                        if (board.IsSquareAtEndOfBoardForBlack(square - fileSize + 1))
                        {
                            GeneratePromotionMoves(square, square-fileSize+1, currentPiece, isWhitePiece, moves, capturedPiece: targetPiece);
                        }
                        else
                        {
                            moves.Add(new Move((ushort)square, (ushort)(square-fileSize+1), currentPiece, capturedPiece: targetPiece));
                        }
                    }
                }

                // En Passant Capture
                //if (board.enPassantTargetSquare != (-1, -1))
                //{
                //    char enPaTargetPiece = board.board[board.enPassantTargetSquare.Item1, board.enPassantTargetSquare.Item2+1];
                //    if (board.enPassantTargetSquare.Item2+1 == rank && IsPieceWhite(enPaTargetPiece))
                //    {
                //        if (board.enPassantTargetSquare.Item1 - file == 1 || board.enPassantTargetSquare.Item1 - file == -1) // En passant Piece is directly next to pawn
                //        {
                //            moves.Add((new Move((byte)file, (byte)rank, (byte)board.enPassantTargetSquare.Item1, (byte)(board.enPassantTargetSquare.Item2), currentPiece, capturedPiece: enPaTargetPiece, enPassantCaptureFlag: true)));
                //        }
                //    }
                //}
            }
        }

        private static void GeneratePromotionMoves(int fromSquare, int toSquare, byte currentPiece, bool isWhitePiece, List<Move> moves, byte capturedPiece = Piece.Empty, bool doublePushPawnMove = false)
        {

            byte[] pieces = { Piece.Queen, Piece.Rook, Piece.Bishop, Piece.Knight};
            if (isWhitePiece)
            {
                for (int i = 0; i < pieces.Length; i++)
                {
                    pieces[i] += Piece.White;
                }
            }
            else
            {
                for (int i = 0; i < pieces.Length; i++)
                {
                    pieces[i] += Piece.Black;
                }
            }
            if (capturedPiece == Piece.Empty)
            {
                for (int i = 0; i < pieces.Length; i++)
                {
                    moves.Add(new Move((ushort)fromSquare, (ushort)toSquare, currentPiece, promotionPiece: pieces[i], doubleSquarePushFlag: doublePushPawnMove));
                }
            }
            else
            {
                for (int i = 0; i < pieces.Length; i++)
                {
                    moves.Add(new Move((ushort)fromSquare, (ushort)toSquare, currentPiece, capturedPiece: capturedPiece, promotionPiece: pieces[i], doubleSquarePushFlag: doublePushPawnMove));
                }
            }
        }

        private static void GeneratePseudoMovesForKnight(int square, Board board, List<Move> moves)
        {
            byte currentPiece = board.board[square];
            bool isWhitePiece = IsPieceWhite(currentPiece);

            int fileSize = board.dimensionsOfBoard.Item1;

            List<int> dSquareKnight = new List<int>();

            switch (square % fileSize) // Only select those squares that won't result in an illegal overlap to the next rank
            {
                case 0:
                    dSquareKnight.AddRange(new int[] { 2 * fileSize + 1, fileSize + 2, -fileSize + 2, -2 * fileSize + 1 });
                    break;
                case 1:
                    dSquareKnight.AddRange(new int[] { 2 * fileSize + 1, fileSize + 2, -fileSize + 2, -2 * fileSize + 1, -2 * fileSize - 1, 2 * fileSize - 1 });
                    break;
                case var n when n == fileSize - 1:
                    dSquareKnight.AddRange(new int[] { -2 * fileSize - 1, -fileSize - 2, fileSize - 2, 2 * fileSize - 1 });
                    break;
                case var n when n == fileSize - 2:
                    dSquareKnight.AddRange(new int[] { -2 * fileSize - 1, -fileSize - 2, fileSize - 2, 2 * fileSize - 1 , 2 * fileSize + 1, -2 * fileSize + 1 });
                    break;
                default:
                    dSquareKnight.AddRange(new int[] {2*fileSize + 1, // NNE
                                    fileSize + 2, // NEE
                                    -fileSize + 2, // SEE
                                    -2 * fileSize + 1, // SSE
                                    -2 * fileSize - 1, // SSW
                                    - fileSize - 2, // SWW
                                    fileSize - 2, // NWW
                                    2*fileSize -1}); // NNW
                    break;
            }

            for (int i = 0; i < dSquareKnight.Count; i++)
            {
                int nextSquare = square + dSquareKnight[i];
                if (nextSquare < 0 || nextSquare >= board.boardSize) continue; // if point is outside of specific board dimensions



                byte targetPiece = board.board[nextSquare];

                if (targetPiece == Piece.Inactive) continue;

                if (targetPiece == Piece.Empty) // Empty square
                {
                    moves.Add(new Move((ushort)square, (ushort)nextSquare, currentPiece));
                }
                else    // Piece on the square      
                {
                    bool isTargetWhite = IsPieceWhite(targetPiece);

                    if (isWhitePiece && !isTargetWhite || !isWhitePiece && isTargetWhite)
                    {
                        moves.Add(new Move((ushort)square, (ushort)nextSquare, currentPiece, capturedPiece: targetPiece));
                    }
                }
            }

        }

        private static void GeneratePseudoMovesForRook(int square, Board board, List<Move> moves)
        {
            byte currentPiece = board.board[square];
            int fileSize = board.dimensionsOfBoard.Item1;
            
            List<int> dSquares = new List<int>();

            switch(square % fileSize)
            {
                case 0:
                    dSquares.AddRange(new int[] { fileSize, 1, -fileSize });
                    break;
                case var n when n == fileSize - 1:
                    dSquares.AddRange(new int[] { fileSize, -fileSize, -1 });
                    break;
                default:
                    dSquares.AddRange(new int[] { fileSize, 1, -fileSize, -1 });
                    break;
            }


            GenerateSlidingMoves(square, dSquares, board, moves);
        }

        private static void GeneratePseudoMovesForBishop(int square, Board board, List<Move> moves)
        {
            byte currentPiece = board.board[square];
            int fileSize = board.dimensionsOfBoard.Item1;

            List<int> dSquares = new List<int>();

            switch (square % fileSize)
            {
                case 0:
                    dSquares.AddRange(new int[] { fileSize + 1, -fileSize + 1 });
                    break;
                case var n when n == fileSize - 1:
                    dSquares.AddRange(new int[] { -fileSize - 1, fileSize - 1 });
                    break;
                default:
                    dSquares.AddRange(new int[] { fileSize + 1, -fileSize + 1, -fileSize - 1, fileSize - 1 });
                    break;
            }

            GenerateSlidingMoves(square, dSquares, board, moves);
        }

        private static void GeneratePseudoMovesForQueen(int square, Board board, List<Move> moves)
        {
            byte currentPiece = board.board[square];
            int fileSize = board.dimensionsOfBoard.Item1;

            List<int> dSquares = new List<int>();

            switch (square % fileSize)
            {
                case 0:
                    dSquares.AddRange(new int[] { fileSize, 1, -fileSize, fileSize + 1, -fileSize + 1 });
                    break;
                case var n when n == fileSize - 1:
                    dSquares.AddRange(new int[] { fileSize, -fileSize, -fileSize-1, fileSize-1, -1 });
                    break;
                default:
                    dSquares.AddRange(new int[] { fileSize, 1, -fileSize, -1, fileSize + 1, -fileSize + 1, -fileSize - 1, fileSize - 1 });
                    break;
            }


            GenerateSlidingMoves(square, dSquares, board, moves);

        }


        private static void GenerateSlidingMoves(int square, List<int> dSquares, Board board, List<Move> moves)
        {
            byte currentPiece = board.board[square];

            bool isWhitePiece = IsPieceWhite(currentPiece);

            for (int i = 0; i < dSquares.Count; i++)
            {
                for (int step = 1; step < 32; step++) // 32 because of the maximum technical board size
                {
                    int nextSquare = square + dSquares[i] * step;
                    

                    if (nextSquare < 0 || nextSquare >= board.boardSize) break; // if point is outside of specific board dimensions

                    byte targetPiece = board.board[nextSquare];

                    if (targetPiece == Piece.Inactive) break;

                    if (targetPiece == Piece.Empty) // Empty square
                    {
                        moves.Add(new Move((ushort) square, (ushort)nextSquare, currentPiece));
                    }
                    else    // Piece on the square      
                    {
                        bool isTargetWhite = IsPieceWhite(targetPiece);

                        if (isWhitePiece && !isTargetWhite || !isWhitePiece && isTargetWhite)
                        {
                            moves.Add(new Move((ushort)square, (ushort)nextSquare, currentPiece, capturedPiece: targetPiece));
                        }
                        break;
                    }

                    int pos = nextSquare % board.dimensionsOfBoard.Item1; // Check if left or right board bounds has been reached
                    //if ((pos == 0 || pos == 7) && (square - nextSquare) % board.boardSize == 0) break;
                    if ((pos == 0 || pos == 7) && !(step == 0 || (pos == square % board.dimensionsOfBoard.Item1))) // second condition checks for case where rook/queen is moving vertically on the right or left bounds
                    {
                        break;
                    }
                }
            }
        }

        static bool IsPieceWhite(byte b)
        {
            if (Piece.White == Piece.GetColor(b)) return true;
            return false;
        }

        //static bool IsPieceWhite(char c) => c >= 'A' && c <= 'Z';
    }
}
