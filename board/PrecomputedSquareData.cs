using System.Reflection.Metadata.Ecma335;

public enum Direction { N, NE, E, SE, S, SW, W, NW }
public class PrecomputedData
{
    public List<Coordinate> kingMoves = new List<Coordinate>();
    public List<Coordinate> knightMoves = new List<Coordinate>();
    public Dictionary<Direction, List<Coordinate>> diagonalSlidingRays = new Dictionary<Direction, List<Coordinate>>();
    public Dictionary<Direction, List<Coordinate>> verticalSlidingRays = new Dictionary<Direction, List<Coordinate>>();
    public List<Coordinate> pawnPushTargets = new List<Coordinate>();
    public List<Coordinate> pawnCaptureTargets = new List<Coordinate>();

    public PrecomputedData(List<Coordinate> kingMoves, List<Coordinate> knightMoves, Dictionary<Direction, List<Coordinate>> diagonalSlidingRays, Dictionary<Direction, List<Coordinate>> horizontalSlidingRays, List<Coordinate> pawnPushTargets, List<Coordinate> pawnCaptureTargets)
    {
        this.kingMoves = kingMoves;
        this.knightMoves = knightMoves;
        this.diagonalSlidingRays = diagonalSlidingRays;
        this.verticalSlidingRays = horizontalSlidingRays;
        this.pawnPushTargets = pawnPushTargets;
        this.pawnCaptureTargets = pawnCaptureTargets;
    }
}