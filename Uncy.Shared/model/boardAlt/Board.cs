using System.Diagnostics.Eventing.Reader;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using System.Text;
using Uncy.board;

namespace Uncy.Shared.boardAlt
{
    public class Board
    {
        // FEN information 
        public byte[]? board = null; // a1 = board[0]
        public bool sideToMove; // bool true = white, false = black
        public int enPassantTargetSquare = -1; // Should be set to -1 if no en Passant is available
        public byte halfMoveClock = 0;
        public int fullMoveCount = 1;

        public bool whiteKingShortCastle;
        public bool whiteKingLongCastle;
        public bool blackKingShortCastle;
        public bool blackKingLongCastle;


        // Zobrist hashing
        ZobristKeys zobristKeys;
        public ulong currentZobristKey;

        // (fileCount, rankCount)
        public (int, int) dimensionsOfBoard = (0, 0);
        public int boardSize = 0;

        private int shortCastleWhiteRookPos = -1;
        private int longCastleWhiteRookPos = -1;
        private int shortCastleBlackRookPos = -1;
        private int longCastleBlackRookPos = -1;


        // Piece Lists
        int whiteKingPos;
        int blackKingPos;



        public Board(Fen fen)
        {
            dimensionsOfBoard = FenParser.GetDimensionsOfBoard(fen);
            Console.WriteLine("Detected Board dimensions of" + dimensionsOfBoard);
            if (dimensionsOfBoard == (0, 0))
            {
                Console.WriteLine("Invalid Board size detected.");
                return;
            }

            InitializeBoard(dimensionsOfBoard.Item1, dimensionsOfBoard.Item2, fen);

            // Create the zobrist keys for this board
            zobristKeys = new ZobristKeys(dimensionsOfBoard.Item1, dimensionsOfBoard.Item2);
            currentZobristKey = CreateZobristKeyFromCurrentBoard();

            // Create the lookup tables for this board
            MoveGenerator.AssignTables(new MoveLookUpTables(this));

        }


        private void InitializeBoard(int fileSize, int rankSize, Fen fen)
        {
            boardSize = fileSize * rankSize;
            board = new byte[boardSize];

            BoardInitializer.SetInformationOfSquaresFromFen(board, fen, rankSize);
            sideToMove = BoardInitializer.SetSideToMove(fen);
            enPassantTargetSquare = BoardInitializer.SetEnPassantTargetSquare(fen, dimensionsOfBoard.Item1);
            halfMoveClock = (byte)BoardInitializer.SetHalfMoveClock(fen);
            fullMoveCount = BoardInitializer.SetFullMoveCount(fen);
            BoardInitializer.UpdateCastlingInformation(fen, this);
            SetupKingPositions();
            SetCastlingRooksAndUpdateCastlingRights();
            PrintBoardToConsole();
        }

        public ulong CreateZobristKeyFromCurrentBoard()
        {
            ulong zkey = 0;
            for (int i = 0; i < board.Length; i++)
            {
                if (Piece.IsSquareActive(board[i]) && Piece.GetPieceType(board[i]) != Piece.Empty)
                {
                    zkey ^= zobristKeys.GetZobristKeyFromTable(i, board[i]);
                }
            }

            if (!sideToMove) zkey ^= zobristKeys.zobrist_side;


            zkey ^= zobristKeys.zobrist_castle[ReadCastlingRightsToInt()];


            if (enPassantTargetSquare != -1) zkey ^= zobristKeys.zobrist_EP[enPassantTargetSquare % dimensionsOfBoard.Item1];

            return zkey;
        }

        private int ReadCastlingRightsToInt()
        {
            int codedCastlingRights =
                (blackKingLongCastle ? 1 : 0) |   // 2⁰
                (blackKingShortCastle ? 1 : 0) << 1 |   // 2¹
                (whiteKingLongCastle ? 1 : 0) << 2 |   // 2²
                (whiteKingShortCastle ? 1 : 0) << 3;    // 2³

            //Console.WriteLine($"I have read {codedCastlingRights} and returned it");
            return codedCastlingRights;
        }

        public void PrintBoardToConsole()
        {
            Console.WriteLine("--------------");
            if (board == null)
            {
                Console.WriteLine("Board is null!");
                return;
            }
            Console.WriteLine("Start printing Board to console.");
            //for (int rank = dimensionsOfBoard.Item2; rank >= 0; rank--)
            //{
            //    for (int file = 0; file < dimensionsOfBoard.Item1; file++)
            //    {
            //        int index = rank * dimensionsOfBoard.Item1 + file - 1;
            //        Console.WriteLine($"i:{index}");
            //        Console.Write(Piece.GiveCharIdentifier(board[index]));
            //    }
            //    Console.WriteLine();
            //}




            for (int rank = dimensionsOfBoard.Item2 - 1; rank >= 0; rank--)
            {
                Console.Write($"{rank + 1} | ");

                for (int file = 0; file < dimensionsOfBoard.Item1; file++)
                {
                    int index = rank * 8 + file;

                    byte piece = board[index];

                    char displayChar = Piece.GiveCharIdentifier(piece);

                    Console.Write($"{displayChar} ");
                }

                Console.WriteLine("|");
            }






            if (sideToMove)
            {
                Console.WriteLine("White to Move..");
            }
            else
            {
                Console.WriteLine("Black to Move..");
            }

            if (whiteKingShortCastle)
            {
                Console.WriteLine("White king can castle short side.");
            }
            if (whiteKingLongCastle)
            {
                Console.WriteLine("White king can castle long side.");
            }

            if (blackKingShortCastle)
            {
                Console.WriteLine("Black king can castle short side.");
            }
            if (blackKingLongCastle)
            {
                Console.WriteLine("Black king can castle long side.");
            }

            if (enPassantTargetSquare == -1)
            {
                Console.WriteLine("No En passant possible");
            }
            else
            {
                Console.WriteLine("En passant possible on square: " + enPassantTargetSquare);
            }

            Console.WriteLine("Halfmoves since last capture or pawn push: " + halfMoveClock);
            Console.WriteLine("Move: " + fullMoveCount);

            Console.WriteLine("POSITIONS OF White Castling rooks: short:" + shortCastleWhiteRookPos + ", long: " + longCastleWhiteRookPos);
            Console.WriteLine("black rooks: short: " + shortCastleBlackRookPos + ", long: " + longCastleBlackRookPos);

            Console.WriteLine("Done.");
            Console.WriteLine("--------------");
        }

        public void PrintBoardToConsoleShort()
        {
            Console.WriteLine("--------------");
            if (board == null)
            {
                Console.WriteLine("Board is null!");
                Console.WriteLine("--------------");
                return;
            }

            Console.Write("Move: " + fullMoveCount + ". To move: ");
            if (sideToMove)
            {
                Console.WriteLine("white");
            }
            else
            {
                Console.WriteLine("black");
            }
            for (int rank = dimensionsOfBoard.Item2; rank >= 0; rank--)
            {
                for (int file = 0; file < dimensionsOfBoard.Item1; file++)
                {
                    int index = (rank * dimensionsOfBoard.Item1 + file) - 1;
                    Console.WriteLine(index);
                    Console.Write(Piece.GiveCharIdentifier(board[index]));
                }
                Console.WriteLine();
            }
            Console.WriteLine("--------------");
        }

        public string ToFen()
        {
            if (board == null) throw new InvalidDataException("Tried to print FEN, but Board is null!");

            var fen = new StringBuilder();

            // 1. Brett-Teil (Reihen 8 → 1, Linien a → h)
            for (int rank = dimensionsOfBoard.Item2 - 1; rank >= 0; rank--)
            {
                int emptyCount = 0;

                for (int file = 0; file < dimensionsOfBoard.Item1; file++)
                {
                    char piece = Piece.GiveCharIdentifier(board[GetSquareFromFileAndRank(file, rank)]);

                    if (piece == Piece.Empty)                       // leeres Feld
                    {
                        emptyCount++;
                        if (emptyCount >= 8)
                        {
                            fen.Append(emptyCount);
                            emptyCount = 0;
                        }
                    }
                    else
                    {
                        if (emptyCount > 0)
                        {
                            fen.Append(emptyCount);
                            emptyCount = 0;
                        }
                        fen.Append(piece);
                    }
                }

                if (emptyCount > 0)
                    fen.Append(emptyCount);

                if (rank > 0)
                    fen.Append('/');
            }

            // 2. Seite am Zug
            fen.Append(' ');
            fen.Append(sideToMove ? 'w' : 'b');

            // 3. Rochaderechte
            fen.Append(' ');
            string castle =
                (whiteKingShortCastle ? "K" : "") +
                (whiteKingLongCastle ? "Q" : "") +
                (blackKingShortCastle ? "k" : "") +
                (blackKingLongCastle ? "q" : "");
            fen.Append(castle.Length == 0 ? "-" : castle);

            // 4. En-passant-Feld
            fen.Append(' ');
            if (enPassantTargetSquare == -1)
            {
                fen.Append('-');
            }
            else
            {
                (int, int) coords = GetFileAndRankFromSquare(enPassantTargetSquare);
                fen.Append(coords.Item1 + ",");      // file
                fen.Append(coords.Item2);      // rank
            }

            // 5. Halbzug-Zähler
            fen.Append(' ');
            fen.Append(halfMoveClock);

            // 6. Vollzug-Zahl
            fen.Append(' ');
            fen.Append(fullMoveCount);

            return fen.ToString();
        }

        public bool MakeMove(Move move, out Undo undo)
        {
            undo = new Undo(this.whiteKingShortCastle, this.whiteKingLongCastle, this.blackKingShortCastle, this.blackKingLongCastle,
                            this.enPassantTargetSquare, this.halfMoveClock, this.currentZobristKey);
            ApplyPieceMove(move);

            UpdateCastleRights(move);
            UpdateEnPassant(move);
            UpdateHalfMoveClock(move);

            sideToMove = !sideToMove;
            currentZobristKey ^= zobristKeys.zobrist_side;

            if (Piece.GetPieceType(move.movedPiece) == Piece.King)
            {
                if (Piece.GetColor(move.movedPiece) == Piece.White)
                {
                    whiteKingPos = move.toSquare;
                }
                else
                {
                    blackKingPos = move.toSquare;
                }
            }

            // Legality Checks
            if (!IsPositionLegalAfterMoveFrom(Piece.IsColor(move.movedPiece, Piece.White), move))
            {
                UnmakeMove(move, undo);
                return false;
            }


            //zobristKeys.CheckForCorrectZobristKeys(this, move);

            return true;
        }

        public void UnmakeMove(Move move, Undo undo)
        {
            RevertPieceMove(move, undo);
            this.whiteKingShortCastle = undo.whiteKingShortCastle;
            this.whiteKingLongCastle = undo.whiteKingLongCastle;
            this.blackKingShortCastle = undo.blackKingShortCastle;
            this.blackKingLongCastle = undo.blackKingLongCastle;
            this.enPassantTargetSquare = undo.enPassantTargetSquare;
            this.halfMoveClock = undo.halfMoveClock;
            this.currentZobristKey = undo.zobristKey;
            this.sideToMove = !sideToMove;

            if (Piece.GetPieceType(move.movedPiece) == Piece.King)
            {
                if (Piece.GetColor(move.movedPiece) == Piece.White)
                {
                    whiteKingPos = move.fromSquare;
                }
                else
                {
                    blackKingPos = move.fromSquare;
                }
            }
        }

        private void ApplyPieceMove(Move move)
        {
            // Set current square empty
            board[move.fromSquare] = Piece.Empty;
            currentZobristKey ^= zobristKeys.GetZobristKeyFromTable(move.fromSquare, move.movedPiece);

            if (enPassantTargetSquare != -1) currentZobristKey ^= zobristKeys.zobrist_EP[enPassantTargetSquare % dimensionsOfBoard.Item1];

            // Update destination square
            board[move.toSquare] = move.movedPiece;
            if (move.capturedPiece != Piece.Empty && !move.enPassantCaptureFlag) currentZobristKey ^= zobristKeys.GetZobristKeyFromTable(move.toSquare, move.capturedPiece);
            currentZobristKey ^= zobristKeys.GetZobristKeyFromTable(move.toSquare, move.movedPiece);

            // Check for Castling move
            if (move.castlingMoveFlag)
            {
                ApplyCastlingMove(move);
            }

            // Check for En Passant move
            if (move.enPassantCaptureFlag)
            {
                ApplyEnPassantMove(move);
            }

            // Check for promotion
            if (move.promotionPiece != Piece.Empty)
            {
                board[move.toSquare] = move.promotionPiece;
                currentZobristKey ^= zobristKeys.GetZobristKeyFromTable(move.toSquare, move.movedPiece);
                currentZobristKey ^= zobristKeys.GetZobristKeyFromTable(move.toSquare, move.promotionPiece);
            }
        }

        private void RevertPieceMove(Move move, Undo undo)
        {
            board[move.fromSquare] = move.movedPiece;

            board[move.toSquare] = move.capturedPiece;

            if (move.castlingMoveFlag)
            {
                RevertCastlingMove(move, undo);
            }

            if (move.enPassantCaptureFlag)
            {
                RevertEnPassantMove(move);
            }
        }

        /*
         * This method reads the current board properties of this board object and determines whether the position represented in the data is a legal chess position
         */
        private bool IsPositionLegalAfterMoveFrom(bool color, Move move)
        {
            int kingPosition;
            if (color == true)
            {
                kingPosition = whiteKingPos;
            }
            else
            {
                kingPosition = blackKingPos;
            }

            if (IsSquareAttackedByColor(!color, kingPosition)) return false;

            if (move.castlingMoveFlag && !IsCastlingLegal(move)) return false;

            return true;
        }

        public bool IsKingInCheck(bool color)
        {
            if (color == true)
            {
                if (IsSquareAttackedByColor(!color, whiteKingPos)) return true;
            }
            else
            {
                if (IsSquareAttackedByColor(!color, blackKingPos)) return true;
            }
            return false;
        }

        private bool IsCastlingLegal(Move move)
        {
            (byte, byte)[] squaresToCheck = GetSquaresBetweenTwoCoords(move.fromSquare, move.toSquare);

            foreach (var square in squaresToCheck)
            {
                int squareIndex = GetSquareFromFileAndRank(square.Item1, square.Item2);
                if (IsSquareAttackedByColor(!Piece.IsColor(move.movedPiece, Piece.White), squareIndex)) return false;
            }
            return true;
        }

        public (byte, byte)[] GetSquaresBetweenTwoCoords(int fromSquare, int toSquare)
        {
            (int, int) ori = GetFileAndRankFromSquare(fromSquare);
            (int, int) dest = GetFileAndRankFromSquare(toSquare);

            int xOri = ori.Item1;
            int yOri = ori.Item2;
            int xDest = dest.Item1;
            int yDest = dest.Item2;

            if (xOri != xDest && yOri != yDest)
                throw new ArgumentException("Only horizontal or vertical directions");

            int stepX = Math.Sign(xDest - xOri);
            int stepY = Math.Sign(yDest - yOri);

            int len = Math.Max(Math.Abs(xDest - xOri), Math.Abs(yDest - yOri)) + 1;

            var result = new (byte X, byte Y)[len];

            for (int i = 0; i < len; i++)
                result[i] = ((byte)(xOri + i * stepX),
                             (byte)(yOri + i * stepY));

            return result;
        }

        /*
         * Since the king is the move.movedPiece subject and will be moved automatically, we only need to move the rook to it's correct position
         * and change the corresponding castling rights
         */
        private void ApplyCastlingMove(Move move)
        {
            int rookOffset;
            currentZobristKey ^= zobristKeys.zobrist_castle[ReadCastlingRightsToInt()];
            if ((move.toSquare % dimensionsOfBoard.Item1) - (move.fromSquare % dimensionsOfBoard.Item1) > 0) // Get information whether its a king or queenside castle move, to know where to put the rook
            {
                // Short castle
                rookOffset = -1;
                if (Piece.IsColor(move.movedPiece, Piece.White))
                {
                    whiteKingShortCastle = false;
                    whiteKingLongCastle = false;
                    MovePieceWithoutMoveContext(shortCastleWhiteRookPos, move.toSquare + rookOffset);
                }
                else
                {
                    blackKingShortCastle = false;
                    blackKingLongCastle = false;
                    MovePieceWithoutMoveContext(shortCastleBlackRookPos, move.toSquare + rookOffset);
                }
            }
            else  // long castle
            {
                rookOffset = 1;
                if (Piece.IsColor(move.movedPiece, Piece.White))
                {
                    whiteKingShortCastle = false;
                    whiteKingLongCastle = false;
                    MovePieceWithoutMoveContext(longCastleWhiteRookPos, move.toSquare + rookOffset);
                }
                else
                {
                    blackKingShortCastle = false;
                    blackKingLongCastle = false;
                    MovePieceWithoutMoveContext(longCastleBlackRookPos, move.toSquare + rookOffset);
                }
            }
            currentZobristKey ^= zobristKeys.zobrist_castle[ReadCastlingRightsToInt()];
        }



        private void ApplyEnPassantMove(Move move)
        {
            int offset;
            if (Piece.IsColor(move.movedPiece, Piece.White))
            {
                offset = -1;
            }
            else
            {
                offset = 1;
            }
            board[move.toSquare + offset * dimensionsOfBoard.Item1] = Piece.Empty;
            currentZobristKey ^= zobristKeys.GetZobristKeyFromTable(move.toSquare + offset, move.capturedPiece);
        }

        private void RevertCastlingMove(Move move, Undo undo)
        {
            if (WasCastlingMoveShortCastle(move))
            {
                if (Piece.IsColor(move.movedPiece, Piece.White))
                {

                    MovePieceWithoutMoveContext(move.toSquare - 1, shortCastleWhiteRookPos);
                }
                else
                {
                    MovePieceWithoutMoveContext(move.toSquare - 1, shortCastleBlackRookPos);
                }
            }
            else
            {
                if (Piece.IsColor(move.movedPiece, Piece.White))
                {

                    MovePieceWithoutMoveContext(move.toSquare + 1, longCastleWhiteRookPos);
                }
                else
                {
                    MovePieceWithoutMoveContext(move.toSquare + 1, longCastleBlackRookPos);
                }
            }
        }

        /* 
         * Only call this if you made sure that it's a castling move
         */
        private bool WasCastlingMoveShortCastle(Move move)
        {
            if ((move.fromSquare % dimensionsOfBoard.Item1) - (move.toSquare % dimensionsOfBoard.Item1) > 0) return false;
            return true;
        }

        private void RevertEnPassantMove(Move move)
        {
            if (Piece.IsColor(move.movedPiece, Piece.White))
            {
                board[move.toSquare - (dimensionsOfBoard.Item1)] = Piece.Pawn + Piece.Black;
            }
            else
            {
                board[move.toSquare + (dimensionsOfBoard.Item1)] = Piece.Pawn + Piece.White;
            }

            board[move.toSquare] = Piece.Empty;
        }

        /*
         * This method will act if the moved Piece in the current Move was a king or a rook. 
         * If a king was moved, all castling rights to that king are withdrawn. 
         * If a rook was moved, the program will determine if that rook was a rook intended for castling (since only one rook on each side of the king is a castling rook)
         *      if yes, castling rights for that side are withdrawn. 
         */
        private void UpdateCastleRights(Move move)
        {
            if (Piece.GetPieceType(move.movedPiece) != Piece.King && Piece.GetPieceType(move.movedPiece) != Piece.Rook) return;
            int pos;
            currentZobristKey ^= zobristKeys.zobrist_castle[ReadCastlingRightsToInt()];
            switch (move.movedPiece)
            {
                case Piece.King + Piece.White: // white King 
                    if (!move.castlingMoveFlag)
                    {
                        whiteKingShortCastle = false;
                        whiteKingLongCastle = false;
                    }
                    break;
                case Piece.King + Piece.Black: // black King
                    if (!move.castlingMoveFlag)
                    {
                        blackKingShortCastle = false;
                        blackKingLongCastle = false;
                    }
                    break;
                case Piece.White + Piece.Rook: // white Rook
                    pos = shortCastleWhiteRookPos;
                    if (move.fromSquare == pos)
                    {
                        whiteKingShortCastle = false;
                        break;
                    }

                    pos = longCastleWhiteRookPos;
                    if (move.fromSquare == pos)
                    {
                        whiteKingLongCastle = false;
                    }
                    break;
                case Piece.Black + Piece.Rook: // black Rook
                    pos = shortCastleBlackRookPos;
                    if (move.fromSquare == pos)
                    {
                        blackKingShortCastle = false;
                        break;
                    }

                    pos = longCastleBlackRookPos;
                    if (move.fromSquare == pos)
                    {
                        blackKingLongCastle = false;
                    }
                    break;

                default:
                    break;
            }
            currentZobristKey ^= zobristKeys.zobrist_castle[ReadCastlingRightsToInt()];
        }

        private void UpdateEnPassant(Move move)
        {
            if (move.doubleSquarePushFlag)
            {
                if (Piece.IsColor(move.movedPiece, Piece.White))
                {
                    enPassantTargetSquare = (move.toSquare - dimensionsOfBoard.Item1);
                }
                else
                {
                    enPassantTargetSquare = (move.toSquare + dimensionsOfBoard.Item1);
                }
                currentZobristKey ^= zobristKeys.zobrist_EP[enPassantTargetSquare % dimensionsOfBoard.Item1]; // Update key for new EP target square
            }
            else
            {
                enPassantTargetSquare = -1;
            }
        }

        private void UpdateHalfMoveClock(Move move)
        {
            if (move.IsCaptureOrPawnMove())
            {
                halfMoveClock = 0;
            }
            else
            {
                halfMoveClock++;
            }
        }

        /*
       * helper util method to reduce code redundancy.
       */
        private void MovePieceWithoutMoveContext(int fromSquare, int toSquare)
        {
            byte piece = board[fromSquare];
            board[fromSquare] = Piece.Empty;
            currentZobristKey ^= zobristKeys.GetZobristKeyFromTable(fromSquare, piece);

            board[toSquare] = piece;
            currentZobristKey ^= zobristKeys.GetZobristKeyFromTable(toSquare, piece);
        }


        /*
         * This method is used to calculate whether a pawn has reached his promotion square
         * 
         */
        public bool IsSquareAtEndOfBoardForWhite(int square)
        {
            // Taken out since no square from outside of the bounds should be accessed anyway

            //if (!(rank < 32 && file < 32 && file >= 0 && rank >= 0)) // Point inside of board bounds
            //{
            //    Console.WriteLine("Point outside of board bounds.");
            //    return false;
            //}

            if (square >= board.Length - dimensionsOfBoard.Item1 || board[square + dimensionsOfBoard.Item1] == Piece.Inactive)
            {
                return true;
            }
            return false;
        }

        public bool IsSquareAtEndOfBoardForBlack(int square)
        {
            //if (!(rank < 32 && file < 32 && file >= 0 && rank >= 0)) // Point inside of board bounds
            //{
            //    Console.WriteLine("Point outside of board bounds.");
            //    return false;
            //}

            if (square < dimensionsOfBoard.Item1 || board[square - dimensionsOfBoard.Item1] == Piece.Inactive)
            {
                return true;
            }
            return false;
        }

        public bool IsSquareOccupiedByPiece(int square)
        {
            //if (file >= 0 && rank >= 0 && file < dimensionsOfBoard.Item1 && rank < dimensionsOfBoard.Item2)
            //{
            if (board[square] == Piece.Empty || board[square] == Piece.Inactive)
            {
                return false;
            }
            else
            {
                return true;
            }
            //}
            //else
            //{
            //    return false;
            //}
        }

        /*
         * This method checks if any given square (index) on the the board is attacked by a piece of a player.
         * if parameter 'white' is true, it checks whether the given square is attacked by white pieces. if false, by black pieces. 
         * Returns true as soon as a piece is found that attacks the square, skipping any other possible pieces.
         */
        public bool IsSquareAttackedByColor(bool white, int square)
        {
            // Nutze die optimierte Version mit Lookup-Tables
            return MoveGenerator.IsSquareAttackedByColor(this, white, square);
        }

        // OLD VERSION - wird nicht mehr verwendet, aber für Referenz behalten
        private bool IsSquareAttackedByColorOLD(bool white, int square)
        {
            int fileSize = dimensionsOfBoard.Item1;

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
                    dSquareKnight.AddRange(new int[] { -2 * fileSize - 1, -fileSize - 2, fileSize - 2, 2 * fileSize - 1, 2 * fileSize + 1, -2 * fileSize + 1 });
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



            List<int> dSquaresKing = new List<int>();
            switch (square % fileSize)
            {
                case 0:
                    dSquaresKing.AddRange(new int[] { fileSize, 1, -fileSize, fileSize + 1, -fileSize + 1 });
                    break;
                case var n when n == fileSize - 1:
                    dSquaresKing.AddRange(new int[] { fileSize, -fileSize, -fileSize - 1, fileSize - 1, -1 });
                    break;
                default:
                    dSquaresKing.AddRange(new int[] { fileSize, 1, -fileSize, -1, fileSize + 1, -fileSize + 1, -fileSize - 1, fileSize - 1 });
                    break;
            }



            byte[] pieces =
            {
                Piece.King, Piece.Knight, Piece.Pawn, Piece.Queen, Piece.Rook, Piece.Bishop
            };

            if (white)
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


            // King check
            for (int i = 0; i < dSquaresKing.Count; i++)
            {
                int nextSquare = square + dSquaresKing[i];

                if (nextSquare >= boardSize || nextSquare < 0) continue;

                if (board[nextSquare] == pieces[0])
                {
                    return true;
                }
            }

            // Knight check
            for (int i = 0; i < dSquareKnight.Count; i++)
            {
                int nextSquare = square + dSquareKnight[i];

                if (nextSquare >= boardSize || nextSquare < 0) continue;
                if (board[nextSquare] == pieces[1])
                {
                    return true;
                }
            }

            // Pawn check
            if (white)
            {
                List<int> dSquarePawn = new List<int>();
                if (square % fileSize == 0)
                {
                    dSquarePawn.Add(-fileSize + 1);
                }
                else if (square % fileSize == 7)
                {
                    dSquarePawn.Add(-fileSize - 1);
                }
                else
                {
                    dSquarePawn.Add(-fileSize - 1);
                    dSquarePawn.Add(-fileSize + 1);
                }

                for (int i = 0; i < dSquarePawn.Count; i++)
                {
                    int nextSquare = square + dSquarePawn[i];

                    if (nextSquare >= boardSize || nextSquare < 0) continue;

                    if (board[nextSquare] == pieces[2])
                    {
                        return true;
                    }
                }
            }
            else
            {
                List<int> dSquarePawn = new List<int>();
                if (square % fileSize == 0)
                {
                    dSquarePawn.Add(fileSize + 1);
                }
                else if (square % fileSize == 7)
                {
                    dSquarePawn.Add(fileSize - 1);
                }
                else
                {
                    dSquarePawn.Add(fileSize - 1);
                    dSquarePawn.Add(fileSize + 1);
                }
                for (int i = 0; i < dSquarePawn.Count; i++)
                {
                    int nextSquare = square + dSquarePawn[i];
                    if (nextSquare >= boardSize || nextSquare < 0) continue;

                    if (board[nextSquare] == pieces[2])
                    {
                        return true;
                    }
                }
            }

            // Rook & Queen Check
            List<int> dSquaresRookQueen = new List<int>();

            switch (square % fileSize)
            {
                case 0:
                    dSquaresRookQueen.AddRange(new int[] { fileSize, 1, -fileSize });
                    break;
                case var n when n == fileSize - 1:
                    dSquaresRookQueen.AddRange(new int[] { fileSize, -fileSize, -1 });
                    break;
                default:
                    dSquaresRookQueen.AddRange(new int[] { fileSize, 1, -fileSize, -1 });
                    break;
            }

            for (int i = 0; i < dSquaresRookQueen.Count; i++)
            {
                for (int step = 1; step < 32; step++) // 32 because of the maximum technical board size
                {
                    int nextSquare = square + dSquaresRookQueen[i] * step;
                    if (nextSquare >= boardSize || nextSquare < 0) break;

                    byte targetPiece = board[nextSquare];
                    int pos = nextSquare % fileSize; // Check if left or right board bounds has been reached

                    if (targetPiece == Piece.Empty)
                    {
                        if ((pos == 0 || pos == 7) && !(step == 0 || (pos == square % fileSize)))
                        {
                            break;
                        }
                        else
                        {
                            continue;
                        }
                    }

                    if (targetPiece == pieces[3] || targetPiece == pieces[4]) return true;
                    else break;
                }
            }

            // Bishop & Queen Check
            List<int> dSquaresBishopQueen = new List<int>();

            switch (square % fileSize)
            {
                case 0:
                    dSquaresBishopQueen.AddRange(new int[] { fileSize + 1, -fileSize + 1 });
                    break;
                case var n when n == fileSize - 1:
                    dSquaresBishopQueen.AddRange(new int[] { -fileSize - 1, fileSize - 1 });
                    break;
                default:
                    dSquaresBishopQueen.AddRange(new int[] { fileSize + 1, -fileSize + 1, -fileSize - 1, fileSize - 1 });
                    break;
            }


            for (int i = 0; i < dSquaresBishopQueen.Count; i++)
            {
                for (int step = 1; step < 32; step++) // 32 because of the maximum technical board size
                {

                    int nextSquare = square + dSquaresBishopQueen[i] * step;

                    if (nextSquare >= boardSize || nextSquare < 0) break;

                    //Console.WriteLine($"homesquare:{square} -> {nextSquare} & pos = {pos}");


                    byte targetPiece = board[nextSquare];
                    int pos = nextSquare % fileSize; // Check if left or right board bounds has been reached


                    if (targetPiece == Piece.Empty)
                    {
                        if ((pos == 0 || pos == 7) && step != 0)
                        {
                            break;
                        }
                        else
                        {
                            continue;
                        }
                    }
                    if (targetPiece == pieces[3] || targetPiece == pieces[5])
                    {
                        return true;
                    }
                    break;
                }
            }

            return false;
        }

        private void SetupKingPositions()
        {
            whiteKingPos = GetWhiteKingPosition();
            blackKingPos = GetBlackKingPosition();
        }

        public int GetWhiteKingPosition()
        {
            for (int i = board.Length - 1; i >= 0; i--)
            {
                if (board[i] == Piece.White + Piece.King)
                {
                    Console.WriteLine($"Found white king on square: {i}");
                    return i;
                }
            }
            return -1;
        }

        public int GetBlackKingPosition()
        {
            for (int i = 0; i < board.GetLength(0); i++)
            {
                if (board[i] == Piece.Black + Piece.King)
                {
                    Console.WriteLine($"Found black king on square: {i}");
                    return i;
                }
            }
            return -1;
        }



        /*
         * Since on a custom chessboard, the existance of only one rook to each side of the king (as in a standard chess board) isn't guaranteed, the program will dynamically
         * look for the existance and then the position of the closest rooks who are the ones the king can castle with. If there are no rooks to the side of the king,
         * the king will simply have no castling rights.
         * 
         * This way the engine may fully play by standard rules of chess / chess960 and custom polymorphic chess boards as well.
         */
        private void SetCastlingRooksAndUpdateCastlingRights()
        {
            if (board == null) return;

            for (int i = whiteKingPos + 1; i <= whiteKingPos + stepUntilRightBoardBorder(whiteKingPos); i++) // Looking for the short castle white rook
            {
                if (Piece.IsPieceAWhiteRook(board[i]) && whiteKingShortCastle == true)
                {
                    shortCastleWhiteRookPos = (i);
                }
                if (!Piece.IsSquareActive(board[i]))
                {
                    whiteKingShortCastle = false;
                    break;
                }
            }
            if (shortCastleWhiteRookPos == -1)
            {
                whiteKingShortCastle = false;
            }


            for (int i = whiteKingPos - 1; i >= whiteKingPos - stepsUntilLeftBoardBorder(whiteKingPos); i--) // Looking for the long castle white rook
            {
                if (Piece.IsPieceAWhiteRook(board[i]) && whiteKingLongCastle == true)
                {
                    longCastleWhiteRookPos = i;
                }
                if (!Piece.IsSquareActive(board[i]))
                {
                    whiteKingLongCastle = false;
                    break;
                }
            }
            if (longCastleWhiteRookPos == -1)
            {
                whiteKingLongCastle = false;
            }


            for (int i = blackKingPos + 1; i <= blackKingPos + stepUntilRightBoardBorder(blackKingPos); i++) // Looking for the short castle black rook
            {
                if (Piece.IsPieceABlackRook(board[i]) && blackKingShortCastle == true)
                {
                    shortCastleBlackRookPos = (i);
                }
                if (!Piece.IsSquareActive(board[i]))
                {
                    blackKingShortCastle = false;
                    break;
                }
            }
            if (shortCastleBlackRookPos == -1)
            {
                blackKingShortCastle = false;
            }


            for (int i = blackKingPos - 1; i >= blackKingPos - stepsUntilLeftBoardBorder(blackKingPos); i--) // Looking for the long castle black rook
            {
                if (Piece.IsPieceABlackRook(board[i]) && blackKingLongCastle == true)
                {
                    longCastleBlackRookPos = (i);
                }
                if (!Piece.IsSquareActive(board[i]))
                {
                    blackKingLongCastle = false;
                    break;
                }
            }
            if (longCastleBlackRookPos == -1)
            {
                blackKingLongCastle = false;
            }
        }

        /*
         * Helper method to give information about how many steps you'd have to take from a square to reach the border
         */
        public int stepsUntilLeftBoardBorder(int square)
        {
            return square % dimensionsOfBoard.Item1;
        }

        public int stepUntilRightBoardBorder(int square)
        {
            return dimensionsOfBoard.Item1 - (square % dimensionsOfBoard.Item1) - 1;
        }

        public string IsMoveLegal(Move move)
        {
            List<Move> legalMoves = MoveGenerator.GenerateLegalMoves(this);
            if (legalMoves.Contains(move))
            {
                MakeMove(move, out Undo undo);
            }
            return ToFen();
        }

        /*
         * a1 = (file=0, rank=0)
         */
        public (int, int) GetFileAndRankFromSquare(int square)
        {
            int rank = square / dimensionsOfBoard.Item1;
            int file = square % dimensionsOfBoard.Item1;
            return (file, rank);
        }

        /*
         * a1 = (file=0, rank=0)
         */
        public int GetSquareFromFileAndRank(int file, int rank)
        {
            return rank * dimensionsOfBoard.Item1 + file;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string GiveMoveAbbreviation(Move move)
        {
            // Optimiert: Direkte Berechnung statt Tuple-Allokation
            int fileSize = dimensionsOfBoard.Item1;

            int fromRank = move.fromSquare / fileSize;
            int fromFile = move.fromSquare % fileSize;
            int toRank = move.toSquare / fileSize;
            int toFile = move.toSquare % fileSize;

            // String-Builder wäre schneller, aber für kurze Strings ist Interpolation OK
            return $"{(char)('a' + fromFile)}{fromRank + 1}{(char)('a' + toFile)}{toRank + 1}";
        }
    }
}
