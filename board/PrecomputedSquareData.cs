using System.Reflection.Metadata.Ecma335;

public enum Direction { N, NE, E, SE, S, SW, W, NW }
public class PrecomputedData
{
    public List<Coordinate> kingMoves = new List<Coordinate>();
    public List<Coordinate> knightMoves = new List<Coordinate>();
    public Dictionary<Direction, List<Coordinate>> diagonalSlidingRays = new Dictionary<Direction, List<Coordinate>>();
    public Dictionary<Direction, List<Coordinate>> verticalSlidingRays = new Dictionary<Direction, List<Coordinate>>();
    public List<Coordinate> whitePawnPushTargets = new List<Coordinate>();
    public List<Coordinate> whitePawnCaptureTargets = new List<Coordinate>();
    public List<Coordinate> blackPawnPushTargets = new List<Coordinate>();
    public List<Coordinate> blackPawnCaptureTargets = new List<Coordinate>();

    public PrecomputedData(List<Coordinate> kingMoves, List<Coordinate> knightMoves, Dictionary<Direction, List<Coordinate>> diagonalSlidingRays, Dictionary<Direction, List<Coordinate>> horizontalSlidingRays, List<Coordinate> whitePawnPushTargets, List<Coordinate> whitePawnCaptureTargets, List<Coordinate> blackPawnPushTargets, List<Coordinate> blackPawnCaptureTargets)
    {
        this.kingMoves = kingMoves;
        this.knightMoves = knightMoves;
        this.diagonalSlidingRays = diagonalSlidingRays;
        this.verticalSlidingRays = horizontalSlidingRays;
        this.whitePawnPushTargets = whitePawnPushTargets;
        this.whitePawnCaptureTargets = whitePawnCaptureTargets;
        this.blackPawnPushTargets = blackPawnPushTargets;
        this.blackPawnCaptureTargets = blackPawnCaptureTargets;
    }
}