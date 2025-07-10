using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using uncy.board;

namespace uncy.model.boardAlt
{
    public class Board
    {
        // FEN information 
        public char[,]? board = null;
        public bool sideToMove; // bool true = white, false = black
        public (int, int) enPassantTargetSquare = (-1,-1); // Should be set to (-1,-1) if no en Passant is available
        public byte halfMoveClock = 0;
        public int fullMoveCount = 1;

        public bool whiteKingShortCastle;
        public bool whiteKingLongCastle;
        public bool blackKingShortCastle;
        public bool blackKingLongCastle;
        
        // (fileCount, rankCount)
        public (int, int) dimensionsOfBoard = (0,0);

        private (int, int) shortCastleWhiteRookPos = (-1,-1);
        private (int, int) longCastleWhiteRookPos = (-1, -1);
        private (int, int) shortCastleBlackRookPos = (-1, -1);
        private (int, int) longCastleBlackRookPos = (-1, -1);



        public Board(Fen fen)
        {
            dimensionsOfBoard = FenParser.GetDimensionsOfBoard(fen);

            if(dimensionsOfBoard == (0,0))
            {
                Console.WriteLine("Invalid Board size detected.");
                return;
            }

            InitializeBoard(dimensionsOfBoard.Item1, dimensionsOfBoard.Item2, fen);
        }


        private void InitializeBoard(int fileSize, int rankSize, Fen fen)
        {
            board = new char[fileSize, rankSize];

            BoardInitializer.SetInformationOfSquaresFromFen(board, fen, rankSize);
            sideToMove = BoardInitializer.SetSideToMove(fen);
            enPassantTargetSquare = BoardInitializer.SetEnPassantTargetSquare(fen);
            halfMoveClock = (byte) BoardInitializer.SetHalfMoveClock(fen);
            fullMoveCount = BoardInitializer.SetFullMoveCount(fen);
            BoardInitializer.UpdateCastlingInformation(fen, this);

            SetCastlingRooksAndUpdateCastlingRights();

            PrintBoardToConsole();
        }

        private void PrintBoardToConsole()
        {
            Console.WriteLine("--------------");
            if (board == null)
            {
                Console.WriteLine("Board is null!");
                return;
            }
            Console.WriteLine("Start printing Board to console.");
            for(int i = board.GetLength(0)-1; i >= 0; i--)
            {
                for (int j = 0; j < board.GetLength(1); j++)
                {
                    Console.Write(board[j,i].ToString() + " ");
                }
                Console.WriteLine();
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

            if (enPassantTargetSquare == (-1, -1))
            {
                Console.WriteLine("No En passant possible");
            }
            else
            {
                Console.WriteLine("En passant possible on: " + enPassantTargetSquare);
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
            for (int i = board.GetLength(0) - 1; i >= 0; i--)
            {
                for (int j = 0; j < board.GetLength(1); j++)
                {
                    Console.Write(board[j, i].ToString() + " ");
                }
                Console.WriteLine();
            }
            Console.WriteLine("--------------");
        }

        public string ToFen()
        {
            if (board is null || board.GetLength(0) != 8 || board.GetLength(1) != 8)
                throw new InvalidOperationException("Das Brett muss 8×8 Felder besitzen.");

            var fen = new StringBuilder();

            // 1. Brett-Teil (Reihen 8 → 1, Linien a → h)
            for (int rank = 7; rank >= 0; rank--)
            {
                int emptyCount = 0;

                for (int file = 0; file < 8; file++)
                {
                    char piece = board[file, rank];

                    if (piece == 'e')                       // leeres Feld
                    {
                        emptyCount++;
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
            if (enPassantTargetSquare == (-1, -1))
            {
                fen.Append('-');
            }
            else
            {
                fen.Append((char)('a' + enPassantTargetSquare.Item1));      // file
                fen.Append((char)('1' + enPassantTargetSquare.Item2));      // rank
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
                            this.enPassantTargetSquare, this.halfMoveClock, 999999);
            ApplyPieceMove(move);

            UpdateCastleRights(move);
            UpdateEnPassant(move);
            UpdateHalfMoveClock(move);

            sideToMove = !sideToMove;
            // TODO: Zobrist key


            // Legality Checks
            if (!IsPositionLegalAfterMoveFrom(IsPieceWhite(move.movedPiece), move))
            {
                UnmakeMove(move, undo);
                return false;
            }
            return true;
        }

        public void UnmakeMove(Move move, Undo undo)
        {
            RevertPieceMove(move, undo);
            this.whiteKingShortCastle = undo.whiteKingShortCastle;
            this.whiteKingLongCastle = undo.whiteKingLongCastle;
            this.blackKingShortCastle= undo.blackKingShortCastle;
            this.blackKingLongCastle= undo.blackKingLongCastle;
            this.enPassantTargetSquare = undo.enPassantTargetSquare;
            this.halfMoveClock = undo.halfMoveClock;
            this.sideToMove = !sideToMove;

            // TODO: zobrist key
        }

        private void ApplyPieceMove(Move move)
        {
            // Set current square empty
            board[move.fromFile, move.fromRank] = 'e';

            // Update destination square
            board[move.toFile, move.toRank] = move.movedPiece;

            // Check for Castling move
            if (move.castlingMoveFlag)
            {
                ApplyCastlingMove(move);
            }

            // Check for En Passant move
            if(move.enPassantCaptureFlag)
            {
                ApplyEnPassantMove(move);
            }

            // Check for promotion
            if(move.promotionPiece != 'e')
            {
                board[move.toFile, move.toRank] = move.promotionPiece;
            }
        }

        private void RevertPieceMove(Move move, Undo undo)
        {
            board[move.fromFile, move.fromRank] = move.movedPiece;

            board[move.toFile, move.toRank] = move.capturedPiece;

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
            (int, int) kingPosition;
            if (color == true)
            {
                kingPosition = GetWhiteKingPosition();
            }else
            {
                kingPosition = GetBlackKingPosition();
            }

            if (IsSquareAttackedByColor(!color, kingPosition.Item1, kingPosition.Item2)) return false;

            if (move.castlingMoveFlag && !IsCastlingLegal(move)) return false;
            
            return true;
        }

        private bool IsCastlingLegal(Move move)
        {
            (byte, byte)[] squaresToCheck = GetSquaresBetweenTwoCoords(move.fromFile, move.fromRank, move.toFile, move.toRank);

            foreach (var square in squaresToCheck)
            {
                if (IsSquareAttackedByColor(!IsPieceWhite(move.movedPiece), square.Item1, square.Item2)) return false;
            }

            return true;
        }

        public (byte,byte)[] GetSquaresBetweenTwoCoords(int xOri, int yOri, int xDest, int yDest)
        {
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
            if (move.toFile - move.fromFile > 0) // Get information whether its a king or queenside castle move, to know where to put the rook
            {
                // Short castle
                rookOffset = -1;
                if (IsPieceWhite(move.movedPiece))
                {
                    whiteKingShortCastle = false;
                    whiteKingLongCastle = false;
                    MovePieceWithoutMoveContext(shortCastleWhiteRookPos.Item1, shortCastleWhiteRookPos.Item2, move.toFile + rookOffset, move.toRank);
                }
                else
                {
                    blackKingShortCastle = false;
                    blackKingLongCastle = false;
                    MovePieceWithoutMoveContext(shortCastleBlackRookPos.Item1, shortCastleBlackRookPos.Item2, move.toFile + rookOffset, move.toRank);
                }
            }
            else  // long castle
            {
                rookOffset = 1;
                if (IsPieceWhite(move.movedPiece))
                {
                    whiteKingShortCastle = false;
                    whiteKingLongCastle = false;
                    MovePieceWithoutMoveContext(longCastleWhiteRookPos.Item1, longCastleWhiteRookPos.Item2, move.toFile + rookOffset, move.toRank);
                }
                else
                {
                    blackKingShortCastle = false;
                    blackKingLongCastle = false;
                    MovePieceWithoutMoveContext(longCastleBlackRookPos.Item1, longCastleBlackRookPos.Item2, move.toFile + rookOffset, move.toRank);
                }
            }
        }

       

        private void ApplyEnPassantMove(Move move)
        {
            int offset;
            if (IsPieceWhite(move.movedPiece))
            {
                offset = -1;
            }
            else
            {
                offset = 1;
            }

            board[move.toFile, move.toRank + offset] = 'e';
        }

        private void RevertCastlingMove(Move move, Undo undo)
        {
            if (WasCastlingMoveShortCastle(move))
            {
                if (IsPieceWhite(move.movedPiece))
                {

                    MovePieceWithoutMoveContext(move.toFile - 1, move.toRank, shortCastleWhiteRookPos.Item1, shortCastleWhiteRookPos.Item2);
                }
                else
                {
                    MovePieceWithoutMoveContext(move.toFile - 1, move.toRank, shortCastleBlackRookPos.Item1, shortCastleBlackRookPos.Item2);
                }
            }
            else
            {
                if (IsPieceWhite(move.movedPiece))
                {

                    MovePieceWithoutMoveContext(move.toFile + 1, move.toRank, longCastleWhiteRookPos.Item1, longCastleWhiteRookPos.Item2);
                }
                else
                {
                    MovePieceWithoutMoveContext(move.toFile + 1, move.toRank, longCastleBlackRookPos.Item1, longCastleBlackRookPos.Item2);
                }
            }
        }

        /* 
         * Only call this if you made sure that it's a castling move
         */
        private bool WasCastlingMoveShortCastle(Move move)
        {
            if (move.fromFile - move.toFile > 0) return false;
            return true;
        }

        private void RevertEnPassantMove(Move move)
        {
            if (IsPieceWhite(move.movedPiece))
            {
                board[move.toFile, move.toRank -1] = 'p';
            }
            else
            {
                board[move.toFile, move.toRank +1] = 'P';
            }

            board[move.toFile, move.toRank] = 'e';
        }

        /*
         * This method will act if the moved Piece in the current Move was a king or a rook. 
         * If a king was moved, all castling rights to that king are withdrawn. 
         * If a rook was moved, the program will determine if that rook was a rook intended for castling (since only one rook on each side of the king is a castling rook)
         *      if yes, castling rights for that side are withdrawn. 
         * 
         */
        private void UpdateCastleRights(Move move)
        {
            if (char.ToLower(move.movedPiece) != 'k' && char.ToLower(move.movedPiece) != 'r') return;
            (int, int) pos;
            switch(move.movedPiece)
            {
                case 'K':
                    if (!move.castlingMoveFlag) { 
                        whiteKingShortCastle = false;
                        whiteKingLongCastle = false;
                    }
                    break;
                case 'k':
                    if (!move.castlingMoveFlag) { 
                        blackKingShortCastle = false;
                        blackKingLongCastle = false;
                    }
                    break;
                case 'R':
                    pos = shortCastleWhiteRookPos;
                    if (move.fromFile == pos.Item1 && move.fromRank == pos.Item2)
                    {
                        whiteKingShortCastle = false;
                        break;
                    }

                    pos = longCastleWhiteRookPos;
                    if (move.fromFile == pos.Item1 && move.fromRank == pos.Item2)
                    {
                        whiteKingLongCastle = false;
                    }
                    break;
                case 'r':
                    pos = shortCastleBlackRookPos;
                    if (move.fromFile == pos.Item1 && move.fromRank == pos.Item2)
                    {
                        blackKingShortCastle = false;
                        break;
                    }

                    pos = longCastleBlackRookPos;
                    if (move.fromFile == pos.Item1 && move.fromRank == pos.Item2)
                    {
                        blackKingLongCastle = false;
                    }
                    break;

                default:
                    return;
            }

        }

        private void UpdateEnPassant(Move move)
        {
            if(move.doubleSquarePushFlag)
            {
                if (IsPieceWhite(move.movedPiece))
                {
                    enPassantTargetSquare = (move.toFile, move.toRank-1);
                }
                else
                {
                    enPassantTargetSquare = (move.toFile, move.toRank + 1);
                }
            }
            else
            {
                enPassantTargetSquare = (-1, -1);
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
        private void MovePieceWithoutMoveContext(int fromFile, int fromRank, int toFile, int toRank)
        {
            char piece = board[fromFile, fromRank];
            board[fromFile, fromRank] = 'e';
            board[toFile, toRank] = piece;
        }


        /*
         * This method is used to calculate whether a pawn has reached his promotion square
         * 
         */
        public bool IsSquareAtEndOfBoardForWhite(int file, int rank)
        {
            if (!(rank < 32 && file < 32 && file >= 0 && rank >= 0)) // Point inside of board bounds
            {
                Console.WriteLine("Point outside of board bounds.");
                return false;
            }
            if (rank == dimensionsOfBoard.Item2-1 || board[file,rank+1] == 'x')
            {
                return true;
            }
            return false;
        }

        public bool IsSquareAtEndOfBoardForBlack(int file, int rank)
        {
            if (!(rank < 32 && file < 32 && file >= 0 && rank >= 0)) // Point inside of board bounds
            {
                Console.WriteLine("Point outside of board bounds.");
                return false;
            }
            if (rank == 0 || board[file, rank - 1] == 'x')
            {
                return true;
            }
            return false;
        }

        public bool IsSquareOccuptiedByPiece(int file, int rank)
        {
            if(board != null && file >= 0 && rank >= 0 && file < dimensionsOfBoard.Item1 && rank < dimensionsOfBoard.Item2)
            {
                if (board[file,rank] == 'e' || board[file, rank] == 'x')
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            else
            {
                return false;
            }
        }

        /*
         * This method checks if any given square (file, rank) on the the board is attacked by a piece of a player.
         * if parameter 'white' is true, it checks whether the given square is attacked by white pieces. if false, by black pieces. 
         * Returns true as soon as a piece is found that attacks the square, skipping any other possible pieces.
         */
        public bool IsSquareAttackedByColor(bool white, int file, int rank)
        {

            if (board == null) return false;

            int[] dFileKnight = { 1, 2, 2, 1, -1, -2, -2, -1 }; // Knight Moves
            int[] dRankKnight = { 2, 1, -1, -2, -2, -1, 1, 2 };

            int[] dFileKing = { 0, 1, 1, 1, 0, -1, -1, -1 }; // King Moves
            int[] dRankKing = {1, 1, 0, -1, -1, -1, 0, 1};

            char[] pieces = { 'K', 'N', 'P', 'Q', 'R', 'B' };
            if (!white) { 
                for(int i = 0; i < pieces.Length; i++)
                {
                    pieces[i] = char.ToLower(pieces[i]);
                }
            }

            // King check
            for (int i = 0; i < 8; i++)
            {
                int nextFile = file + dFileKing[i];
                int nextRank = rank + dRankKing[i];

                if (nextFile < 0 || nextFile >= dimensionsOfBoard.Item1 || nextRank < 0 || nextRank >= dimensionsOfBoard.Item2) continue;

                if (board[nextFile,nextRank] == pieces[0])
                {
                    return true;   
                }
            }

            // Knight check
            for (int i = 0; i < 8; i++)
            {
                int nextFile = file + dFileKnight[i];
                int nextRank = rank + dRankKnight[i];

                if (nextFile < 0 || nextFile >= dimensionsOfBoard.Item1 || nextRank < 0 || nextRank >= dimensionsOfBoard.Item2) continue;

                if (board[nextFile, nextRank] == pieces[1])
                {
                    return true;
                }
            }

            // Pawn check
            if (white)
            {
                int[] dFilePawn = { 1, -1};
                int[] dRankPawn = { -1, -1};
                
                for (int i = 0; i < 2; i++) {
                    int nextFile = file + dFilePawn[i];
                    int nextRank = rank + dRankPawn[i];
                    if (nextFile < 0 || nextFile >= dimensionsOfBoard.Item1 || nextRank < 0 || nextRank >= dimensionsOfBoard.Item2) continue;

                    if (board[nextFile,nextRank] == 'P')
                    {
                        return true;
                    }
                }
            }
            else
            {
                int[] dFilePawn = { -1, 1};
                int[] dRankPawn = {  1, 1};

                for (int i = 0; i < 2; i++)
                {
                    int nextFile = file + dFilePawn[i];
                    int nextRank = rank + dRankPawn[i];
                    if (nextFile < 0 || nextFile >= dimensionsOfBoard.Item1 || nextRank < 0 || nextRank >= dimensionsOfBoard.Item2) continue;

                    if (board[nextFile, nextRank] == 'p')
                    {
                        return true;
                    }
                }
            }

            // Rook & Queen Check
            int[] dFileRook = { 0, 0, -1, 1};
            int[] dRankRook = { -1, 1, 0, 0};

            for (int i = 0; i < dFileRook.Length; i++)
            {
                for (int step = 1; step < 32; step++) // 32 because of the maximum technical board size
                {
                    
                    int nextFile = file + dFileRook[i] * step;
                    int nextRank = rank + dRankRook[i] * step;
                    
                    if (nextFile < 0 || nextFile >= dimensionsOfBoard.Item1 || nextRank < 0 || nextRank >= dimensionsOfBoard.Item2) break;
                    //Console.WriteLine("Checking square:" + nextFile + "," + nextRank + " --- " + dFileRook[i] + "*" + step +"," +dRankRook[i] + "*" + step);
                    char targetPiece = board[nextFile, nextRank];
                        
                    if (targetPiece == 'e') continue;
                    else if (targetPiece == pieces[3] || targetPiece == pieces[4]) return true;
                    else break;
                }
            }

            // Bishop & Queen Check
            int[] dFileBishop = { 1, 1, -1, -1 };
            int[] dRankBishop = { 1, -1, -1, 1 };

            for (int i = 0; i < dFileBishop.Length; i++)
            {
                for (int step = 1; step < 32; step++) // 32 because of the maximum technical board size
                {
                    
                    int nextFile = file + dFileBishop[i] * step;
                    int nextRank = rank + dRankBishop[i] * step;

                    if (nextFile < 0 || nextFile >= dimensionsOfBoard.Item1 || nextRank < 0 || nextRank >= dimensionsOfBoard.Item2) break;

                    char targetPiece = board[nextFile, nextRank];

                    if (targetPiece == 'e') continue;
                    else if (targetPiece == pieces[3] || targetPiece == pieces[5]) return true;
                    else break; 

                }
            }

            return false;
        }

        public (int,int) GetWhiteKingPosition()
        {
            for (int i = board.GetLength(0) - 1; i >= 0; i--)
            {
                for (int j = 0; j < board.GetLength(1); j++)
                {
                    if (board[i,j] == 'K')
                    {
                        return (i, j);
                    }
                }
            }
            return (-1, -1);
        }

        public (int,int) GetBlackKingPosition()
        {
            for (int i = 0; i < board.GetLength(0); i++)
            {
                for (int j = 0; j < board.GetLength(1); j++)
                {
                    if (board[i, j] == 'k')
                    {
                        return (i, j);
                    }
                }
            }
            return (-1, -1);
        }

        static bool IsPieceWhite(char c) => c >= 'A' && c <= 'Z';


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

            (int, int) whiteKingPos = GetWhiteKingPosition();
            (int, int) blackKingPos = GetBlackKingPosition();

            for(int i = whiteKingPos.Item1; i < dimensionsOfBoard.Item1; i++) // Looking for the short castle white rook
            {
                if (board[i,whiteKingPos.Item2] == 'R' && whiteKingShortCastle == true)
                {
                    shortCastleWhiteRookPos = (i, whiteKingPos.Item2);
                }
                if (board[i, whiteKingPos.Item2] == 'x')
                {
                    whiteKingShortCastle = false;
                    break;
                }
            }
            if (shortCastleWhiteRookPos == (-1, -1))
            {
                whiteKingShortCastle = false;
            }


            for (int i = whiteKingPos.Item1; i >= 0; i--) // Looking for the long castle white rook
            {
                if (board[i, whiteKingPos.Item2] == 'R' && whiteKingLongCastle == true)
                {
                    longCastleWhiteRookPos = (i, whiteKingPos.Item2);
                }
                if (board[i, whiteKingPos.Item2] == 'x')
                {
                    whiteKingLongCastle = false;
                    break;
                }
            }
            if (longCastleWhiteRookPos == (-1, -1))
            {
                whiteKingLongCastle = false;
            }


            for (int i = blackKingPos.Item1; i < dimensionsOfBoard.Item1; i++) // Looking for the short castle black rook
            {
                if (board[i, blackKingPos.Item2] == 'r' && blackKingShortCastle == true)
                {
                    shortCastleBlackRookPos = (i, blackKingPos.Item2);
                }
                if (board[i, blackKingPos.Item2] == 'x')
                {
                    blackKingShortCastle = false;
                    break;
                }
            }
            if (shortCastleBlackRookPos == (-1, -1))
            {
                blackKingShortCastle = false;
            }


            for (int i = blackKingPos.Item1; i >= 0; i--) // Looking for the long castle black rook
            {
                if (board[i, blackKingPos.Item2] == 'r' && blackKingLongCastle == true)
                {
                    longCastleBlackRookPos = (i, blackKingPos.Item2);
                }
                if (board[i, blackKingPos.Item2] == 'x')
                {
                    blackKingLongCastle = false;
                    break;
                }
            }
            if (longCastleBlackRookPos == (-1, -1))
            {
                blackKingLongCastle = false;
            }
        }
    }
}
