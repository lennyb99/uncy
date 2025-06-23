using System.Data;
using System.Dynamic;


class PolymorphicChessBoard
{
    public (int, int) boardDimensions = (0, 0);
    public HashSet<Coordinate> squares = new HashSet<Coordinate>();
    public List<Piece> startingPieces = new List<Piece>();
    public Dictionary<Coordinate, Piece> piecePositions = new Dictionary<Coordinate, Piece>();
    public Dictionary<Coordinate, PrecomputedData> precomputedData = new Dictionary<Coordinate, PrecomputedData>();
}