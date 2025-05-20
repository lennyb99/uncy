using System.Data;
using System.Dynamic;


class PolymorphicChessBoard
{
    public HashSet<Coordinate> squares = new HashSet<Coordinate>();
    public List<Piece> pieces = new List<Piece>();
    public Dictionary<Coordinate, Piece> piecePositions = new Dictionary<Coordinate, Piece>();
    public Dictionary<Coordinate, PrecomputedData> precomputedData = new Dictionary<Coordinate, PrecomputedData>();
}