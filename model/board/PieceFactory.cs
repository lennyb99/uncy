using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace uncy.board
{
    public class PieceFactory
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

        public static char GetPieceIdentifier(Piece piece)
        {
            char c = ' ';
            switch (piece.type)
            {
                case PieceType.KING:
                    c = 'K';
                    break;
                case PieceType.QUEEN:
                    c = 'Q';
                    break;
                case PieceType.ROOK:
                    c = 'R';
                    break;
                case PieceType.BISHOP:
                    c = 'B';
                    break;
                case PieceType.KNIGHT:
                    c = 'N';
                    break;
                case PieceType.PAWN:
                    c = 'P';
                    break;
                default:
                    break;
            }
            if(piece.color == PieceColor.BLACK)
            {
                c = char.Parse(c.ToString().ToLower());
            }

            return c;
        }

    }
}
