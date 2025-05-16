enum PieceType { KING, QUEEN, ROOK, BISHOP, KNIGHT, PAWN, EMPTY }
enum PieceColor { WHITE, BLACK }
class Piece
{
    public PieceType type;
    public PieceColor color;

    public Piece(PieceType type, PieceColor color)
    {
        this.type = type;
        this.color = color;
    }
}