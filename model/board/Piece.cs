public enum PieceType { KING, QUEEN, ROOK, BISHOP, KNIGHT, PAWN}
public enum PieceColor { WHITE, BLACK }
public class Piece
{
    public PieceType type { get; private set; }
    public PieceColor color { get; private set; }
    public Coordinate currentPosition { get; set; }

    public Piece(PieceType type, PieceColor color)
    {
        this.type = type;
        this.color = color;
    }

    public override string ToString() => $"({color} {type})";

}