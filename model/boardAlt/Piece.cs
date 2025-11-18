using Google.Protobuf.WellKnownTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace uncy.model.boardAlt
{
    public static class Piece
    {
        public const byte Empty = 0;
        public const byte Pawn = 1;
        public const byte Knight = 2;
        public const byte Bishop = 3;
        public const byte Rook = 4;
        public const byte Queen = 5;
        public const byte King = 6;

        public const byte White = 8;
        public const byte Black = 16;

        public const byte Inactive = 128;

        public const byte PieceTypeMask = 0b00000111; 
        public const byte ColorMask = 0b00011000;
        public const byte InactiveMask = 0b10000000;

        public static bool IsColor(byte piece, byte color)
        {
            return (piece & color) == color;
        }
        public static byte GetPieceType(byte piece)
        {
            return (byte) (piece & PieceTypeMask);
        }
        public static byte GetColor(byte piece)
        {
            return (byte) (piece & ColorMask);
        }

        public static bool IsSquareActive(byte square)
        {
            return (square & InactiveMask) != InactiveMask;
        }

        /*
         * Helper methods for cleaner code in Board.cs and MoveGenerator.cs
         */
        public static bool IsPieceAWhiteRook(byte piece)
        {
            if (GetPieceType(piece) == Rook && IsColor(piece, Piece.White)) return true;
            return false;
        }

        public static bool IsPieceABlackRook(byte piece)
        {
            if (GetPieceType(piece) == Rook && IsColor(piece, Piece.Black)) return true;
            return false;
        }

        public static bool IsPieceAPawn(byte piece)
        {
            if (GetPieceType(piece) == Pawn) return true;
            return false;
        }


        public static byte GivePieceFromChar(char pieceChar)
        {
            if (char.ToLower(pieceChar) == 'x') return Piece.Inactive;

            byte piece = 0;
            switch (char.ToLower(pieceChar))
            {
                case 'p':
                    piece += Piece.Pawn;
                    break;
                case 'n':
                    piece += Piece.Knight;
                    break;
                case 'b':
                    piece += Piece.Bishop;
                    break;
                case 'r':
                    piece += Piece.Rook;
                    break;
                case 'q':
                    piece += Piece.Queen;
                    break;
                case 'k':
                    piece += Piece.King;
                    break;
            }

            if (char.IsUpper(pieceChar))
            {
                piece += Piece.White;
            }
            else
            {
                piece += Piece.Black;
            }

            return piece;
        }

        public static char GiveCharIdentifier(byte piece)
        {
            if (!IsSquareActive(piece)) return 'x';
            char c;
            switch (GetPieceType(piece))
            {
                case Piece.Empty:
                    return 'e';
                case Piece.Pawn:
                    c = 'p';
                    break;
                case Piece.Knight:
                    c = 'n';
                    break;
                case Piece.Bishop:
                    c = 'b';
                    break;
                case Piece.Rook:
                    c = 'r';
                    break;
                case Piece.Queen:
                    c = 'q'; break;
                case Piece.King:
                    c = 'k'; break;
                default:
                    return 'e';
            }

            if (IsColor(piece, Piece.White))
            {
                return char.ToUpper(c);
            }
            else
            {
                return c;
            }
        }
    }
}
