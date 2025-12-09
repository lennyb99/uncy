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


    internal static class MoveGenerator
    {
        private static MoveLookUpTables tables;

        // Flag für Benchmarking: false = NEW (LUT), true = OLD
        private static bool useOldMethods = false;

        public static void AssignTables(MoveLookUpTables lookUpTables)
        {
            tables = lookUpTables;
        }

        public static void SetUseOldMethods(bool useOld)
        {
            useOldMethods = useOld;
        }

        public static List<Move> GenerateLegalMoves(Board board)
        {
            // Instantiate List 
            List<Move> legalMoves = new List<Move>();
            List<Move> newMoves = new List<Move>();
            GeneratePseudoMoves(board, board.sideToMove, newMoves);

            foreach (var move in newMoves)
            {
                if (!board.MakeMove(move, out Undo undo))
                    continue;

                legalMoves.Add(move);
                board.UnmakeMove(move, undo);
            }
            return legalMoves;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void GeneratePseudoMoves(Board board, bool sideToMove, List<Move> moves)
        {
            // Lokale Referenzen für besseren Cache-Zugriff
            var boardArray = board.board;
            int boardSize = board.boardSize;

            // Loop through each square
            for (int i = 0; i < boardSize; i++)
            {
                byte piece = boardArray[i];

                // Skip empty or inactive squares
                if (piece == Piece.Inactive || piece == Piece.Empty) continue;

                // Optimierter Color-Check: Direkter Bit-Vergleich statt Methodenaufruf
                bool isWhite = (piece & Piece.ColorMask) == Piece.White;
                if ((isWhite && sideToMove) || (!isWhite && !sideToMove))
                {
                    IdentifyPieceAndGeneratePseudoMoves(i, board, moves);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void IdentifyPieceAndGeneratePseudoMoves(int square, Board board, List<Move> moves)
        {
            switch (Piece.GetPieceType(board.board[square]))
            {
                case Piece.King:
                    if (useOldMethods)
                        GeneratePseudoMovesForKingOLD(square, board, moves);
                    else
                        GeneratePseudoMovesForKing(square, board, moves);
                    break;
                case Piece.Queen:
                    if (useOldMethods)
                        GeneratePseudoMovesForQueenOLD(square, board, moves);
                    else
                        GeneratePseudoMovesForQueen(square, board, moves);
                    break;
                case Piece.Rook:
                    if (useOldMethods)
                        GeneratePseudoMovesForRookOLD(square, board, moves);
                    else
                        GeneratePseudoMovesForRook(square, board, moves);
                    break;
                case Piece.Bishop:
                    if (useOldMethods)
                        GeneratePseudoMovesForBishopOLD(square, board, moves);
                    else
                        GeneratePseudoMovesForBishop(square, board, moves);
                    break;
                case Piece.Knight:
                    if (useOldMethods)
                        GeneratePseudoMovesForKnightOLD(square, board, moves);
                    else
                        GeneratePseudoMovesForKnight(square, board, moves);
                    break;
                case Piece.Pawn:
                    if (useOldMethods)
                        GeneratePseudoMovesForPawnOLD(square, board, moves);
                    else
                        GeneratePseudoMovesForPawn(square, board, moves);
                    break;
                default:
                    Console.WriteLine("Unidentified Piece found: " + board.board[square]);
                    break;
            }
            //Console.WriteLine("Found " + moves.Count + " moves for: " +board.board[file, rank]);
        }

        private static void GeneratePseudoMovesForKingOLD(int square, Board board, List<Move> moves)
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

            // Castling moves
            if (isWhitePiece) // white King
            {
                if (board.whiteKingShortCastle)
                {
                    for (int i = square + 1; i <= square + board.stepUntilRightBoardBorder(square); i++)
                    {
                        byte nextPiece = board.board[i];
                        if (nextPiece == Piece.Empty)
                        {
                            continue;
                        }
                        else if (Piece.IsPieceAWhiteRook(nextPiece))
                        {
                            moves.Add(new Move((ushort)square, (ushort)(square + 2), currentPiece, castlingMoveFlag: true));
                        }
                        else break;
                    }
                }
                if (board.whiteKingLongCastle)
                {
                    for (int i = square - 1; i >= square - board.stepsUntilLeftBoardBorder(square); i--)
                    {
                        byte nextPiece = board.board[i];
                        if (nextPiece == Piece.Empty)
                        {
                            continue;
                        }
                        else if (Piece.IsPieceAWhiteRook(nextPiece))
                        {
                            moves.Add(new Move((ushort)square, (ushort)(square - 2), currentPiece, castlingMoveFlag: true));
                        }
                        else break;
                    }
                }
            }
            else // black King
            {
                if (board.blackKingShortCastle)
                {
                    for (int i = square + 1; i <= square + board.stepUntilRightBoardBorder(square); i++)
                    {
                        byte nextPiece = board.board[i];
                        if (nextPiece == Piece.Empty)
                        {
                            continue;
                        }
                        else if (Piece.IsPieceABlackRook(nextPiece))
                        {
                            moves.Add(new Move((ushort)(square), (ushort)(square + 2), currentPiece, castlingMoveFlag: true));
                        }
                        else break;
                    }
                }
                if (board.blackKingLongCastle)
                {
                    for (int i = square - 1; i >= square - board.stepsUntilLeftBoardBorder(square); i--)
                    {
                        byte nextPiece = board.board[i];
                        if (nextPiece == Piece.Empty)
                        {
                            continue;
                        }
                        else if (Piece.IsPieceABlackRook(nextPiece))
                        {
                            moves.Add(new Move((ushort)square, (ushort)(square - 2), currentPiece, castlingMoveFlag: true));
                        }
                        else break;
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void GeneratePseudoMovesForKing(int square, Board board, List<Move> moves)
        {
            var boardArray = board.board;
            byte currentPiece = boardArray[square];
            bool isWhitePiece = IsPieceWhite(currentPiece);

            // --- TEIL A: Normale Königs-Moves (via LUT) ---

            var kMoves = tables.kingMoves;
            int startIndex = tables.kingMoveStartIndex[square];
            int count = tables.kingMoveCount[square];
            int endIndex = startIndex + count;

            for (int i = startIndex; i < endIndex; i++)
            {
                int nextSquare = (int)kMoves[i];
                byte targetPiece = boardArray[nextSquare];

                if (targetPiece == Piece.Inactive) continue;

                if (targetPiece == Piece.Empty)
                {
                    moves.Add(new Move((ushort)square, (ushort)nextSquare, currentPiece));
                }
                else
                {
                    bool isTargetWhite = IsPieceWhite(targetPiece);
                    if (isWhitePiece != isTargetWhite)
                    {
                        moves.Add(new Move((ushort)square, (ushort)nextSquare, currentPiece, capturedPiece: targetPiece));
                    }
                }
            }

            // --- TEIL B: Rochade (Castling) ---
            // Da Rochaden selten sind und spezielle Logik (Strecke frei? Turm da?) brauchen,
            // ist die prozedurale Logik hier oft besser als ein komplexer Lookup.
            // Ich habe die Logik beibehalten, aber leicht bereinigt.

            if (isWhitePiece)
            {
                if (board.whiteKingShortCastle) CheckCastling(square, board, moves, currentPiece, true, true);
                if (board.whiteKingLongCastle) CheckCastling(square, board, moves, currentPiece, true, false);
            }
            else
            {
                if (board.blackKingShortCastle) CheckCastling(square, board, moves, currentPiece, false, true);
                if (board.blackKingLongCastle) CheckCastling(square, board, moves, currentPiece, false, false);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void CheckCastling(int kingSquare, Board board, List<Move> moves, byte kingPiece, bool isWhite, bool isShortParams)
        {
            int direction = isShortParams ? 1 : -1;
            // Hier nutzen wir deine Logik für die Distanz zum Rand
            int stepsToBorder = isShortParams
                ? board.stepUntilRightBoardBorder(kingSquare)
                : board.stepsUntilLeftBoardBorder(kingSquare);

            // Wir suchen nach dem Turm. Der Turm ist irgendwo in Richtung Rand.
            for (int step = 1; step <= stepsToBorder; step++)
            {
                int checkSquare = kingSquare + (step * direction);
                byte pieceAtPos = board.board[checkSquare];

                if (pieceAtPos == Piece.Inactive) break; // Sollte bei Castling nicht passieren, aber sicher ist sicher

                if (pieceAtPos == Piece.Empty)
                {
                    continue; // Weiter suchen
                }
                else
                {
                    // Wir sind auf eine Figur gestoßen. Ist es der korrekte Turm?
                    bool isRook = isWhite ? Piece.IsPieceAWhiteRook(pieceAtPos) : Piece.IsPieceABlackRook(pieceAtPos);

                    if (isRook)
                    {
                        // Rochade möglich! 
                        // Ziel ist immer 2 Felder in die Richtung (gemäß Standard Schachregeln).
                        // ACHTUNG: Prüfen, ob der Weg dahin (Feld 1 und Feld 2) auch wirklich leer war.
                        // Deine ursprüngliche Logik prüfte "Empty -> continue", und wenn Turm -> Move.
                        // Das setzt voraus, dass step 1 und step 2 leer waren.
                        // Bei Standard-Schach ist der Turm auf Distanz 3 oder 4. 
                        // Deine Logik fügt den Move hinzu, sobald der Turm gefunden wird. 
                        // Das Ziel des Königs ist kingSquare + 2*direction.

                        moves.Add(new Move((ushort)kingSquare, (ushort)(kingSquare + (2 * direction)), kingPiece, castlingMoveFlag: true));
                    }
                    // Wenn es kein Turm ist (sondern Läufer/Springer/etc), ist der Weg blockiert -> Abbruch.
                    break;
                }
            }
        }

        private static void GeneratePseudoMovesForPawnOLD(int square, Board board, List<Move> moves)
        {
            byte currentPiece = board.board[square];
            bool isWhitePiece = IsPieceWhite(currentPiece);

            int fileSize = board.dimensionsOfBoard.Item1;

            if (isWhitePiece)
            {
                // One Square Push
                if (!(square + fileSize >= board.boardSize)) // if point is inside of specific board dimensions
                {
                    byte targetPiece = board.board[square + fileSize];
                    if (targetPiece == Piece.Empty)
                    {
                        if (board.IsSquareAtEndOfBoardForWhite(square + fileSize))
                        {
                            GeneratePromotionMoves(square, square + fileSize, currentPiece, isWhitePiece, moves);
                        }
                        else
                        {
                            moves.Add(new Move((ushort)square, (ushort)(square + fileSize), currentPiece)); // Add valid Move
                        }


                        // Two Squares Push
                        if (((int)square / 8) == 1)
                        {
                            if (!(square + fileSize * 2 >= board.boardSize))
                            {
                                targetPiece = board.board[square + fileSize * 2];
                                if (targetPiece == Piece.Empty)
                                {
                                    if (board.IsSquareAtEndOfBoardForWhite(square + fileSize * 2))
                                    {
                                        GeneratePromotionMoves(square, square + fileSize * 2, currentPiece, isWhitePiece, moves, doublePushPawnMove: true);
                                    }
                                    else
                                    {
                                        moves.Add(new Move((ushort)square, (ushort)(square + fileSize * 2), currentPiece, doubleSquarePushFlag: true)); // Add valid Move
                                    }
                                }
                            }
                        }
                    }
                }



                // Hit Diagonal
                // Left
                if ((square + fileSize - 1 < board.boardSize) && square % fileSize != 0)
                {
                    byte targetPiece = board.board[square + fileSize - 1];

                    if (board.enPassantTargetSquare == square + fileSize - 1)
                    {
                        byte enPaTargetPiece = board.board[board.enPassantTargetSquare - fileSize];
                        if (!IsPieceWhite(enPaTargetPiece))
                        {
                            moves.Add((new Move((ushort)square, (ushort)board.enPassantTargetSquare, currentPiece, capturedPiece: enPaTargetPiece, enPassantCaptureFlag: true)));
                        }
                    }

                    if (!IsPieceWhite(targetPiece) && board.IsSquareOccupiedByPiece(square + fileSize - 1)) // Checks if piece is black since this is code is executed for white Pawns
                    {
                        if (board.IsSquareAtEndOfBoardForWhite(square + fileSize - 1))
                        {
                            GeneratePromotionMoves(square, square + fileSize - 1, currentPiece, isWhitePiece, moves, capturedPiece: targetPiece);
                        }
                        else
                        {
                            moves.Add(new Move((ushort)square, (ushort)(square + fileSize - 1), currentPiece, capturedPiece: targetPiece));
                        }
                    }
                }

                // Right
                if ((square + fileSize + 1 < board.boardSize) && square % fileSize != fileSize - 1)
                {
                    byte targetPiece = board.board[square + fileSize + 1];

                    if (board.enPassantTargetSquare == square + fileSize + 1)
                    {
                        byte enPaTargetPiece = board.board[board.enPassantTargetSquare - fileSize];
                        if (!IsPieceWhite(enPaTargetPiece))
                        {
                            moves.Add((new Move((ushort)square, (ushort)board.enPassantTargetSquare, currentPiece, capturedPiece: enPaTargetPiece, enPassantCaptureFlag: true)));
                        }
                    }

                    if (!IsPieceWhite(targetPiece) && board.IsSquareOccupiedByPiece(square + fileSize + 1)) // Checks if piece is black since this is code is executed for white Pawns
                    {
                        if (board.IsSquareAtEndOfBoardForWhite(square + fileSize + 1))
                        {
                            GeneratePromotionMoves(square, square + fileSize + 1, currentPiece, isWhitePiece, moves, capturedPiece: targetPiece);
                        }
                        else
                        {
                            moves.Add(new Move((ushort)square, (ushort)(square + fileSize + 1), currentPiece, capturedPiece: targetPiece));
                        }
                    }
                }
            }
            else // Handling for black pieces
            {
                // One Square Push
                if (!(square - fileSize < 0)) // if point is inside of specific board dimensions
                {
                    byte targetPiece = board.board[square - fileSize];
                    if (targetPiece == Piece.Empty)
                    {
                        if (board.IsSquareAtEndOfBoardForBlack(square - fileSize))
                        {
                            GeneratePromotionMoves(square, square - fileSize, currentPiece, isWhitePiece, moves);
                        }
                        else
                        {
                            moves.Add(new Move((ushort)square, (ushort)(square - fileSize), currentPiece)); // Add valid Move
                        }
                        // Two Squares Push
                        if (square / 8 == 6)
                        {
                            if (!(square - fileSize * 2 < 0))
                            {
                                targetPiece = board.board[square - fileSize * 2];
                                if (targetPiece == Piece.Empty)
                                {
                                    if (board.IsSquareAtEndOfBoardForBlack(square - fileSize * 2))
                                    {
                                        GeneratePromotionMoves(square, square - fileSize * 2, currentPiece, isWhitePiece, moves, doublePushPawnMove: true);
                                    }
                                    else
                                    {
                                        moves.Add(new Move((ushort)square, (ushort)(square - fileSize * 2), currentPiece, doubleSquarePushFlag: true)); // Add valid Move
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
                    byte targetPiece = board.board[square - fileSize - 1];

                    if (board.enPassantTargetSquare == square - fileSize - 1)
                    {
                        byte enPaTargetPiece = board.board[board.enPassantTargetSquare + fileSize];
                        if (IsPieceWhite(enPaTargetPiece))
                        {
                            moves.Add((new Move((ushort)square, (ushort)board.enPassantTargetSquare, currentPiece, capturedPiece: enPaTargetPiece, enPassantCaptureFlag: true)));
                        }
                    }

                    if (IsPieceWhite(targetPiece) && board.IsSquareOccupiedByPiece(square - fileSize - 1)) // Checks if piece is white since this is code is executed for black Pawns
                    {
                        if (board.IsSquareAtEndOfBoardForBlack(square - fileSize - 1))
                        {
                            GeneratePromotionMoves(square, square - fileSize - 1, currentPiece, isWhitePiece, moves, capturedPiece: targetPiece);
                        }
                        else
                        {
                            moves.Add(new Move((ushort)square, (ushort)(square - fileSize - 1), currentPiece, capturedPiece: targetPiece));
                        }
                    }
                }

                // Right
                if ((square - fileSize + 1 >= 0) && square % fileSize != fileSize - 1)
                {
                    byte targetPiece = board.board[square - fileSize + 1];

                    if (board.enPassantTargetSquare == square - fileSize + 1)
                    {
                        byte enPaTargetPiece = board.board[board.enPassantTargetSquare + fileSize];
                        if (IsPieceWhite(enPaTargetPiece))
                        {
                            moves.Add((new Move((ushort)square, (ushort)board.enPassantTargetSquare, currentPiece, capturedPiece: enPaTargetPiece, enPassantCaptureFlag: true)));
                        }
                    }

                    if (IsPieceWhite(targetPiece) && board.IsSquareOccupiedByPiece(square - fileSize + 1)) // Checks if piece is white since this is code is executed for black Pawns
                    {
                        if (board.IsSquareAtEndOfBoardForBlack(square - fileSize + 1))
                        {
                            GeneratePromotionMoves(square, square - fileSize + 1, currentPiece, isWhitePiece, moves, capturedPiece: targetPiece);
                        }
                        else
                        {
                            moves.Add(new Move((ushort)square, (ushort)(square - fileSize + 1), currentPiece, capturedPiece: targetPiece));
                        }
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void GeneratePseudoMovesForPawn(int square, Board board, List<Move> moves)
        {
            // 1. Lokale Referenzen für Speed & Lesbarkeit
            var boardArray = board.board;
            var distToEdge = tables.squaresToEdge; // Das Array [64 * 8] (bzw. TotalSquares * 8)
            int width = tables.BoardWidth;
            int totalSquares = tables.TotalSquares;
            int squareIndexOffset = square * 8; // Offset im squaresToEdge Array

            byte currentPiece = boardArray[square];
            bool isWhitePiece = IsPieceWhite(currentPiece);

            // Hilfsvariablen für Promotion-Check (vermeidet Methodenaufruf IsSquareAtEndOfBoard)
            // Ein Bauer promoted, wenn er in die letzte Reihe zieht.
            // Für Weiß: Indizes [TotalSquares - Width ... TotalSquares - 1]
            // Für Schwarz: Indizes [0 ... Width - 1]

            if (isWhitePiece)
            {
                int up = square + width;

                // --- A. MOVES (PUSH) ---
                // Bounds-Check: Ist "oben" noch im Brett?
                if (up < totalSquares)
                {
                    // 1. Single Push
                    if (boardArray[up] == Piece.Empty)
                    {
                        // Ist das Zielfeld auf der letzten Reihe? -> Promotion
                        if (up >= totalSquares - width)
                        {
                            GeneratePromotionMoves(square, up, currentPiece, true, moves);
                        }
                        else
                        {
                            moves.Add(new Move((ushort)square, (ushort)up, currentPiece));

                            // 2. Double Push
                            // Nur möglich, wenn Feld davor frei war (checked) UND wir auf Startreihe stehen.
                            // Startreihe Weiß (Index 1): Zwischen width und 2*width.
                            // Ersetzt: ((int)square / 8) == 1
                            if (square >= width && square < width * 2)
                            {
                                int up2 = up + width;
                                // Prüfen ob Ziel leer
                                if (boardArray[up2] == Piece.Empty)
                                {
                                    // Double Push kann technisch keine Promotion sein (außer auf Miniboard 4x4, aber vernachlässigbar)
                                    moves.Add(new Move((ushort)square, (ushort)up2, currentPiece, doubleSquarePushFlag: true));
                                }
                            }
                        }
                    }
                }

                // --- B. ATTACKS (DIAGONAL) ---
                // Weiß schlägt nach NW (Direction 7) und NE (Direction 1)
                // Wir nutzen squaresToEdge, um zu sehen, ob wir am Rand kleben.

                // 1. Capture Left (NW - Direction Index 7)
                // Check: Haben wir Platz nach NW? (Vermeidet square % fileSize == 0)
                if (distToEdge[squareIndexOffset + 7] > 0)
                {
                    int targetSq = up - 1;
                    // Hier prüfen wir Piece.Inactive, falls das Board Löcher hat
                    byte targetPiece = boardArray[targetSq];

                    if (targetPiece != Piece.Inactive && targetPiece != Piece.Empty)
                    {
                        if (!IsPieceWhite(targetPiece)) // Gegner?
                        {
                            if (targetSq >= totalSquares - width) // Promotion Check
                                GeneratePromotionMoves(square, targetSq, currentPiece, true, moves, capturedPiece: targetPiece);
                            else
                                moves.Add(new Move((ushort)square, (ushort)targetSq, currentPiece, capturedPiece: targetPiece));
                        }
                    }
                    // En Passant Check (NW)
                    else if (targetSq == board.enPassantTargetSquare)
                    {
                        // Bei EnPassant steht der Bauer "unter" dem Ziel (bei Weiß) -> also targetSq - width
                        // Logik aus deinem Code übernommen: enPassantTargetSquare - fileSize
                        byte epPiece = boardArray[targetSq - width];
                        if (!IsPieceWhite(epPiece) && epPiece != Piece.Empty)
                        {
                            moves.Add(new Move((ushort)square, (ushort)targetSq, currentPiece, capturedPiece: epPiece, enPassantCaptureFlag: true));
                        }
                    }
                }

                // 2. Capture Right (NE - Direction Index 1)
                if (distToEdge[squareIndexOffset + 1] > 0)
                {
                    int targetSq = up + 1;
                    byte targetPiece = boardArray[targetSq];

                    if (targetPiece != Piece.Inactive && targetPiece != Piece.Empty)
                    {
                        if (!IsPieceWhite(targetPiece))
                        {
                            if (targetSq >= totalSquares - width)
                                GeneratePromotionMoves(square, targetSq, currentPiece, true, moves, capturedPiece: targetPiece);
                            else
                                moves.Add(new Move((ushort)square, (ushort)targetSq, currentPiece, capturedPiece: targetPiece));
                        }
                    }
                    // En Passant Check (NE)
                    else if (targetSq == board.enPassantTargetSquare)
                    {
                        byte epPiece = boardArray[targetSq - width];
                        if (!IsPieceWhite(epPiece) && epPiece != Piece.Empty)
                        {
                            moves.Add(new Move((ushort)square, (ushort)targetSq, currentPiece, capturedPiece: epPiece, enPassantCaptureFlag: true));
                        }
                    }
                }
            }
            else // BLACK PIECE
            {
                int down = square - width;

                // --- A. MOVES (PUSH) ---
                if (down >= 0)
                {
                    if (boardArray[down] == Piece.Empty)
                    {
                        // Promotion Check Schwarz (Reihe 0 -> Indizes < Width)
                        if (down < width)
                        {
                            GeneratePromotionMoves(square, down, currentPiece, false, moves);
                        }
                        else
                        {
                            moves.Add(new Move((ushort)square, (ushort)down, currentPiece));

                            // Double Push Schwarz
                            // Startreihe Schwarz (Index 6 bei Standard): Zwischen Total - 2*Width und Total - Width
                            // Beispiel 8x8: Quadrat 48 bis 55.
                            if (square >= totalSquares - (2 * width) && square < totalSquares - width)
                            {
                                int down2 = down - width;
                                if (boardArray[down2] == Piece.Empty)
                                {
                                    moves.Add(new Move((ushort)square, (ushort)down2, currentPiece, doubleSquarePushFlag: true));
                                }
                            }
                        }
                    }
                }

                // --- B. ATTACKS (DIAGONAL) ---
                // Schwarz schlägt nach SW (Direction 5) und SE (Direction 3)

                // 1. Capture Left (von Schwarz aus gesehen, also SW - Direction 5)
                // Achtung: SW (Index 5) auf dem Board bedeutet index - width - 1
                if (distToEdge[squareIndexOffset + 5] > 0)
                {
                    int targetSq = down - 1;
                    byte targetPiece = boardArray[targetSq];

                    if (targetPiece != Piece.Inactive && targetPiece != Piece.Empty)
                    {
                        if (IsPieceWhite(targetPiece)) // Gegner (Weiß)?
                        {
                            if (targetSq < width) // Promotion
                                GeneratePromotionMoves(square, targetSq, currentPiece, false, moves, capturedPiece: targetPiece);
                            else
                                moves.Add(new Move((ushort)square, (ushort)targetSq, currentPiece, capturedPiece: targetPiece));
                        }
                    }
                    // En Passant
                    else if (targetSq == board.enPassantTargetSquare)
                    {
                        // Bauer steht "über" dem Ziel -> targetSq + width
                        byte epPiece = boardArray[targetSq + width];
                        if (IsPieceWhite(epPiece) && epPiece != Piece.Empty)
                        {
                            moves.Add(new Move((ushort)square, (ushort)targetSq, currentPiece, capturedPiece: epPiece, enPassantCaptureFlag: true));
                        }
                    }
                }

                // 2. Capture Right (von Schwarz aus gesehen, also SE - Direction 3)
                // SE (Index 3) bedeutet index - width + 1
                if (distToEdge[squareIndexOffset + 3] > 0)
                {
                    int targetSq = down + 1;
                    byte targetPiece = boardArray[targetSq];

                    if (targetPiece != Piece.Inactive && targetPiece != Piece.Empty)
                    {
                        if (IsPieceWhite(targetPiece))
                        {
                            if (targetSq < width)
                                GeneratePromotionMoves(square, targetSq, currentPiece, false, moves, capturedPiece: targetPiece);
                            else
                                moves.Add(new Move((ushort)square, (ushort)targetSq, currentPiece, capturedPiece: targetPiece));
                        }
                    }
                    // En Passant
                    else if (targetSq == board.enPassantTargetSquare)
                    {
                        byte epPiece = boardArray[targetSq + width];
                        if (IsPieceWhite(epPiece) && epPiece != Piece.Empty)
                        {
                            moves.Add(new Move((ushort)square, (ushort)targetSq, currentPiece, capturedPiece: epPiece, enPassantCaptureFlag: true));
                        }
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void GeneratePromotionMoves(int fromSquare, int toSquare, byte currentPiece, bool isWhitePiece, List<Move> moves, byte capturedPiece = Piece.Empty, bool doublePushPawnMove = false)
        {

            byte[] pieces = { Piece.Queen, Piece.Rook, Piece.Bishop, Piece.Knight };
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

        private static void GeneratePseudoMovesForKnightOLD(int square, Board board, List<Move> moves)
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void GeneratePseudoMovesForKnight(int square, Board board, List<Move> moves)
        {
            // Lokale Referenzen für Speed (vermeidet Property-Access Overhead)
            var boardArray = board.board;
            var kMoves = tables.knightMoves; // Das Array mit den Ziel-Indizes

            // Startindex und Länge aus den LUTs holen
            int startIndex = tables.knightMoveStartIndex[square];
            int count = tables.knightMoveCount[square];
            int endIndex = startIndex + count;

            byte currentPiece = boardArray[square];
            bool isWhitePiece = IsPieceWhite(currentPiece);

            // Iteriere direkt über die vorberechneten Ziel-Felder
            for (int i = startIndex; i < endIndex; i++)
            {
                // Wir lesen den absoluten Index des Zielfeldes aus dem LUT
                // (Cast nach int, da Array-Index int erwartet)
                int nextSquare = (int)kMoves[i];

                // Bounds-Check entfällt, da kMoves nur valide Indizes enthält.

                byte targetPiece = boardArray[nextSquare];

                // 1. Check: Ist das Feld inaktiv (Loch im Brett)?
                if (targetPiece == Piece.Inactive) continue;

                // 2. Logik für Zug oder Schlag
                if (targetPiece == Piece.Empty)
                {
                    moves.Add(new Move((ushort)square, (ushort)nextSquare, currentPiece));
                }
                else
                {
                    // Wenn eine Figur drauf steht: Prüfen ob Gegner
                    bool isTargetWhite = IsPieceWhite(targetPiece);
                    if (isWhitePiece != isTargetWhite)
                    {
                        moves.Add(new Move((ushort)square, (ushort)nextSquare, currentPiece, capturedPiece: targetPiece));
                    }
                }
            }
        }

        private static void GeneratePseudoMovesForRookOLD(int square, Board board, List<Move> moves)
        {
            byte currentPiece = board.board[square];
            int fileSize = board.dimensionsOfBoard.Item1;

            List<int> dSquares = new List<int>();

            switch (square % fileSize)
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void GeneratePseudoMovesForRook(int square, Board board, List<Move> moves)
        {
            // Rook nutzt die Richtungen: N(0), E(2), S(4), W(6).
            // Start bei Index 0, Schrittweite 2.
            GenerateSlidingMoves(square, board, moves, 0, 2);
        }

        private static void GeneratePseudoMovesForBishopOLD(int square, Board board, List<Move> moves)
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void GeneratePseudoMovesForBishop(int square, Board board, List<Move> moves)
        {
            // Bishop nutzt die Richtungen: NE(1), SE(3), SW(5), NW(7).
            // Start bei Index 1, Schrittweite 2.
            GenerateSlidingMoves(square, board, moves, 1, 2);
        }

        private static void GeneratePseudoMovesForQueenOLD(int square, Board board, List<Move> moves)
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
                    dSquares.AddRange(new int[] { fileSize, -fileSize, -fileSize - 1, fileSize - 1, -1 });
                    break;
                default:
                    dSquares.AddRange(new int[] { fileSize, 1, -fileSize, -1, fileSize + 1, -fileSize + 1, -fileSize - 1, fileSize - 1 });
                    break;
            }


            GenerateSlidingMoves(square, dSquares, board, moves);

        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void GeneratePseudoMovesForQueen(int square, Board board, List<Move> moves)
        {
            // Queen nutzt alle Richtungen 0 bis 7.
            // Start bei Index 0, Schrittweite 1.
            GenerateSlidingMoves(square, board, moves, 0, 1);
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
                        moves.Add(new Move((ushort)square, (ushort)nextSquare, currentPiece));
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void GenerateSlidingMoves(int square, Board board, List<Move> moves, int startDirIndex, int stepDir)
        {
            byte currentPiece = board.board[square];
            bool isWhitePiece = IsPieceWhite(currentPiece); // Dein existierender Helper

            // Lokale Kopien der Arrays für schnelleren Zugriff (Micro-Optimization in C#)
            var offsets = tables.directionOffsets;
            var distToEdge = tables.squaresToEdge;
            var boardArray = board.board;

            // Wir berechnen den Start-Offset für das squaresToEdge Array einmalig.
            // Das Array ist flach (1D): [Square 0 Dir 0, Square 0 Dir 1, ... Square 1 Dir 0 ...]
            // Daher: square * 8.
            int squareIndexOffset = square * 8;

            // Äußere Schleife: Iteriert durch die Richtungen (z.B. N, E, S, W für Turm)
            // Wir nutzen hier feste Inkremente (stepDir), um 'if'-Checks zu vermeiden.
            for (int dirIndex = startDirIndex; dirIndex < 8; dirIndex += stepDir)
            {
                int directionOffset = offsets[dirIndex];

                // Hier ist der enorme Vorteil: Wir wissen sofort, wie weit wir laufen dürfen.
                // Kein "if nextSquare < 0" mehr nötig.
                int maxSteps = distToEdge[squareIndexOffset + dirIndex];

                int currentTargetSquare = square;

                // Innere Schleife: Läuft die Linie entlang ("Ray casting")
                for (int i = 0; i < maxSteps; i++)
                {
                    currentTargetSquare += directionOffset; // Addition ist sehr schnell

                    byte targetPiece = boardArray[currentTargetSquare];

                    // 1. Feld ist leer
                    if (targetPiece == Piece.Empty)
                    {
                        moves.Add(new Move((ushort)square, (ushort)currentTargetSquare, currentPiece));
                    }
                    // 2. Feld ist besetzt
                    else
                    {
                        // Wichtig: Wir müssen prüfen, ob es eine eigene oder gegnerische Figur ist.
                        // Hinweis: Falls du 'Piece.Inactive' (für Off-Board Sentinels) nutzt, 
                        // wird dies durch 'maxSteps' theoretisch schon verhindert, 
                        // aber der Check schadet nicht, falls die Logik auch blocker enthält.

                        if (targetPiece != Piece.Inactive)
                        {
                            bool isTargetWhite = IsPieceWhite(targetPiece);

                            // Wenn Farben ungleich sind -> Schlagzug (Capture)
                            if (isWhitePiece != isTargetWhite)
                            {
                                moves.Add(new Move((ushort)square, (ushort)currentTargetSquare, currentPiece, capturedPiece: targetPiece));
                            }
                        }

                        // Egal ob eigene oder gegnerische Figur: Hier ist der Weg zu Ende.
                        break;
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static bool IsPieceWhite(byte b)
        {
            return (b & Piece.ColorMask) == Piece.White;
        }

        /// <summary>
        /// Optimierte Version von IsSquareAttackedByColor, die Lookup-Tables nutzt.
        /// Prüft, ob ein Feld von einer bestimmten Farbe angegriffen wird.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsSquareAttackedByColor(Board board, bool white, int square)
        {
            if (tables == null) return false; // Fallback falls Tables nicht initialisiert

            var boardArray = board.board;
            byte targetKing = white ? (byte)(Piece.King + Piece.White) : (byte)(Piece.King + Piece.Black);
            byte targetKnight = white ? (byte)(Piece.Knight + Piece.White) : (byte)(Piece.Knight + Piece.Black);
            byte targetPawn = white ? (byte)(Piece.Pawn + Piece.White) : (byte)(Piece.Pawn + Piece.Black);
            byte targetQueen = white ? (byte)(Piece.Queen + Piece.White) : (byte)(Piece.Queen + Piece.Black);
            byte targetRook = white ? (byte)(Piece.Rook + Piece.White) : (byte)(Piece.Rook + Piece.Black);
            byte targetBishop = white ? (byte)(Piece.Bishop + Piece.White) : (byte)(Piece.Bishop + Piece.Black);

            // 1. King Check (nutzt kingMoves LUT)
            int kingStartIndex = tables.kingMoveStartIndex[square];
            int kingCount = tables.kingMoveCount[square];
            for (int i = 0; i < kingCount; i++)
            {
                int nextSquare = tables.kingMoves[kingStartIndex + i];
                if (boardArray[nextSquare] == targetKing)
                    return true;
            }

            // 2. Knight Check (nutzt knightMoves LUT)
            int knightStartIndex = tables.knightMoveStartIndex[square];
            int knightCount = tables.knightMoveCount[square];
            for (int i = 0; i < knightCount; i++)
            {
                int nextSquare = tables.knightMoves[knightStartIndex + i];
                if (boardArray[nextSquare] == targetKnight)
                    return true;
            }

            // 3. Pawn Check
            // Wir müssen prüfen, ob ein Bauer auf einem Feld steht, das 'square' angreift.
            // Weiße Bauern greifen nach oben an (NW, NE) -> Bauer auf square - BoardWidth - 1 oder square - BoardWidth + 1
            // Schwarze Bauern greifen nach unten an (SW, SE) -> Bauer auf square + BoardWidth - 1 oder square + BoardWidth + 1
            if (white)
            {
                // Prüfe, ob ein weißer Bauer auf einem Feld steht, das 'square' angreift
                // Weiße Bauern greifen nach oben an: NW (square - BoardWidth - 1) und NE (square - BoardWidth + 1)
                int pawnSquare1 = square - tables.BoardWidth - 1;
                int pawnSquare2 = square - tables.BoardWidth + 1;

                if (pawnSquare1 >= 0 && pawnSquare1 < tables.TotalSquares)
                {
                    byte piece = boardArray[pawnSquare1];
                    if (piece != Piece.Inactive && piece == targetPawn)
                        return true;
                }

                if (pawnSquare2 >= 0 && pawnSquare2 < tables.TotalSquares)
                {
                    byte piece = boardArray[pawnSquare2];
                    if (piece != Piece.Inactive && piece == targetPawn)
                        return true;
                }
            }
            else
            {
                // Prüfe, ob ein schwarzer Bauer auf einem Feld steht, das 'square' angreift
                // Schwarze Bauern greifen nach unten an: SW (square + BoardWidth - 1) und SE (square + BoardWidth + 1)
                int pawnSquare1 = square + tables.BoardWidth - 1;
                int pawnSquare2 = square + tables.BoardWidth + 1;

                if (pawnSquare1 >= 0 && pawnSquare1 < tables.TotalSquares)
                {
                    byte piece = boardArray[pawnSquare1];
                    if (piece != Piece.Inactive && piece == targetPawn)
                        return true;
                }

                if (pawnSquare2 >= 0 && pawnSquare2 < tables.TotalSquares)
                {
                    byte piece = boardArray[pawnSquare2];
                    if (piece != Piece.Inactive && piece == targetPawn)
                        return true;
                }
            }

            // 4. Rook & Queen Check (nutzt squaresToEdge für sliding pieces)
            // Richtungen: N(0), E(2), S(4), W(6)
            var offsets = tables.directionOffsets;
            var distToEdge = tables.squaresToEdge;
            int squareIndexOffset = square * 8;

            for (int dirIndex = 0; dirIndex < 8; dirIndex += 2) // N, E, S, W
            {
                int directionOffset = offsets[dirIndex];
                int maxSteps = distToEdge[squareIndexOffset + dirIndex];
                int currentSquare = square;

                for (int step = 0; step < maxSteps; step++)
                {
                    currentSquare += directionOffset;
                    byte targetPiece = boardArray[currentSquare];

                    if (targetPiece == Piece.Empty)
                        continue;

                    if (targetPiece == Piece.Inactive)
                        break;

                    if (targetPiece == targetRook || targetPiece == targetQueen)
                        return true;

                    // Blockiert durch andere Figur
                    break;
                }
            }

            // 5. Bishop & Queen Check (nutzt squaresToEdge für sliding pieces)
            // Richtungen: NE(1), SE(3), SW(5), NW(7)
            for (int dirIndex = 1; dirIndex < 8; dirIndex += 2) // NE, SE, SW, NW
            {
                int directionOffset = offsets[dirIndex];
                int maxSteps = distToEdge[squareIndexOffset + dirIndex];
                int currentSquare = square;

                for (int step = 0; step < maxSteps; step++)
                {
                    currentSquare += directionOffset;
                    byte targetPiece = boardArray[currentSquare];

                    if (targetPiece == Piece.Empty)
                        continue;

                    if (targetPiece == Piece.Inactive)
                        break;

                    if (targetPiece == targetBishop || targetPiece == targetQueen)
                        return true;

                    // Blockiert durch andere Figur
                    break;
                }
            }

            return false;
        }

        //static bool IsPieceWhite(char c) => c >= 'A' && c <= 'Z';
    }
}
