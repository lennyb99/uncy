using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace uncy.board
{
    internal class PieceFactory
    {
        public static Piece? CreatePiece(char c)
        {
            switch(c)
            {
                case 'K':
                    return new Piece(PieceType.KING, PieceColor.WHITE);
                case 'Q':
                    return new Piece(PieceType.QUEEN, PieceColor.WHITE);
                case 'R':
                    return new Piece(PieceType.ROOK, PieceColor.WHITE);
                case 'B':
                    return new Piece(PieceType.BISHOP, PieceColor.WHITE);
                case 'N':
                    return new Piece(PieceType.KNIGHT, PieceColor.WHITE);
                case 'P':
                    return new Piece(PieceType.PAWN, PieceColor.WHITE);
                case 'k':
                    return new Piece(PieceType.KING, PieceColor.BLACK);
                case 'q':
                    return new Piece(PieceType.QUEEN, PieceColor.BLACK);
                case 'r':
                    return new Piece(PieceType.ROOK, PieceColor.BLACK);
                case 'b':
                    return new Piece(PieceType.BISHOP, PieceColor.BLACK);
                case 'n':
                    return new Piece(PieceType.KNIGHT, PieceColor.BLACK);
                case 'p':
                    return new Piece(PieceType.PAWN, PieceColor.BLACK);
                default:
                    return null;
            }
        }

    }
}
