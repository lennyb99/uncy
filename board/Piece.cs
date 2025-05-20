enum PieceType { KING, QUEEN, ROOK, BISHOP, KNIGHT, PAWN}
enum PieceColor { WHITE, BLACK }
class Piece
{
    public PieceType type { get; private set; }
    public PieceColor color { get; private set; }
    public Coordinate currentPosition { get; set; } 

    public Piece(PieceType type, PieceColor color)
    {
        this.type = type;
        this.color = color;
    }

}