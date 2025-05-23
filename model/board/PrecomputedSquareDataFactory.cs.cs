using System.Data;
using System.Security.Cryptography.X509Certificates;

public class PrecomputedSquareDataFactory
{
    public static Dictionary<Coordinate, PrecomputedData> GenerateAllData(HashSet<Coordinate> squares)
    {
        Dictionary<Coordinate, PrecomputedData> precomputedData = new Dictionary<Coordinate, PrecomputedData>();
        foreach (Coordinate square in squares)
        {
            precomputedData.Add(square, ComputeDataForSquare(square, squares));
        }
        return precomputedData;
    }

    private static PrecomputedData ComputeDataForSquare(Coordinate square, HashSet<Coordinate> allSquares)
    {
        return new PrecomputedData(CalculateKingMoves(square, allSquares),
                                         CalculateKnightMoves(square, allSquares),
                                         CalculateDiagonalSlidingRays(square, allSquares),
                                         CalculateVerticalSlidingRays(square, allSquares),
                                         CalculateWhitePawnPushTargets(square, allSquares),
                                         CalculateWhitePawnCaptureTargets(square, allSquares),
                                         CalculateBlackPawnPushTargets(square, allSquares),
                                         CalculateBlackPawnCaptureTargets(square, allSquares)
                                         );
    }

    private static List<Coordinate> CalculateKingMoves(Coordinate square, HashSet<Coordinate> allSquares)
    {
        List<Coordinate> possibleMoves = new List<Coordinate>();
        int[] dx = { 0, 1, 1, 1, 0, -1, -1, -1 };
        int[] dy = { 1, 1, 0, -1, -1, -1, 0, 1 };
        for (int i = 0; i < 8; i++)
        {
            Coordinate targetSquare = new Coordinate(square.X + dx[i], square.Y + dy[i]);
            if (allSquares.Contains(targetSquare))
            {
                possibleMoves.Add(targetSquare);
            }
        }
        return possibleMoves;
    }

    private static List<Coordinate> CalculateKnightMoves(Coordinate square, HashSet<Coordinate> allSquares)
    {
        List<Coordinate> possibleMoves = new List<Coordinate>();
        int[] dx = { 1, 1, -1, -1, 2, 2, -2, -2 };
        int[] dy = { 2, -2, 2, -2, 1, -1, 1, -1 };
        for (int i = 0; i < 8; i++)
        {
            Coordinate targetSquare = new Coordinate(square.X + dx[i], square.Y + dy[i]);
            if (allSquares.Contains(targetSquare))
            {
                possibleMoves.Add(targetSquare);
            }
        }
        return possibleMoves;
    }

    private static Dictionary<Direction, List<Coordinate>> CalculateDiagonalSlidingRays(Coordinate square, HashSet<Coordinate> allSquares)
    {
        Dictionary<Direction, List<Coordinate>> results = new Dictionary<Direction, List<Coordinate>>();
        List<Direction> directions = new List<Direction> { Direction.NE, Direction.SE, Direction.SW, Direction.NW };
        foreach (Direction dir in directions)
        {
            results.Add(dir, CalculateRayPath(square, allSquares, dir));
        }
        return results;
    }

    private static Dictionary<Direction, List<Coordinate>> CalculateVerticalSlidingRays(Coordinate square, HashSet<Coordinate> allSquares)
    {
        Dictionary<Direction, List<Coordinate>> results = new Dictionary<Direction, List<Coordinate>>();
        List<Direction> directions = new List<Direction> { Direction.N, Direction.E, Direction.S, Direction.W };
        foreach (Direction dir in directions)
        {
            results.Add(dir, CalculateRayPath(square, allSquares, dir));
        }
        return results;
    }

    private static List<Coordinate> CalculateRayPath(Coordinate square, HashSet<Coordinate> allSquares, Direction dir)
    {
        List<Coordinate> currentRayPath = new List<Coordinate>();
        Coordinate currentPos = new Coordinate(square.X, square.Y);
        while (true)
        {
            int dx = 0;
            int dy = 0;

            switch (dir)
            {
                case Direction.NE:
                    dx = 1; dy = 1;
                    break;
                case Direction.SE:
                    dx = 1; dy = -1;
                    break;
                case Direction.SW:
                    dx = -1; dy = -1;
                    break;
                case Direction.NW:
                    dx = -1; dy = 1;
                    break;
                case Direction.N:
                    dx = 0; dy = 1;
                    break;
                case Direction.S:
                    dx = 0; dy = -1;
                    break;
                case Direction.W:
                    dx = -1; dy = 0;
                    break;
                case Direction.E:
                    dx = 1; dy = 0;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(dir), $"Invalid or unsupported direction: {dir}. This case should not be reached.");
            }

            Coordinate nextPos = new Coordinate(currentPos.X + dx, currentPos.Y + dy);

            if (allSquares.Contains(nextPos))
            {
                currentRayPath.Add(nextPos);
                currentPos = nextPos;
            }
            else
            {
                break;
            }
        }
        return currentRayPath;
    }

    private static List<Coordinate> CalculateWhitePawnPushTargets(Coordinate square, HashSet<Coordinate> allSquares)
    {
        List<Coordinate> possibleMoves = new List<Coordinate>();
        int[] dx = { 0, 0 };
        int[] dy = { 1, 2 };
        for (int i = 0; i < 2; i++)
        {
            Coordinate targetSquare = new Coordinate(square.X + dx[i], square.Y + dy[i]);
            if (allSquares.Contains(targetSquare))
            {
                possibleMoves.Add(targetSquare);
            }
        }
        return possibleMoves;
    }

    private static List<Coordinate> CalculateWhitePawnCaptureTargets(Coordinate square, HashSet<Coordinate> allSquares)
    {
        List<Coordinate> possibleMoves = new List<Coordinate>();
        int[] dx = { -1, 1 };
        int[] dy = { 1, 1 };
        for (int i = 0; i < 2; i++)
        {
            Coordinate targetSquare = new Coordinate(square.X + dx[i], square.Y + dy[i]);
            if (allSquares.Contains(targetSquare))
            {
                possibleMoves.Add(targetSquare);
            }
        }
        return possibleMoves;
    }

    private static List<Coordinate> CalculateBlackPawnPushTargets(Coordinate square, HashSet<Coordinate> allSquares)
    {
        List<Coordinate> possibleMoves = new List<Coordinate>();
        int[] dx = { 0, 0 };
        int[] dy = { -1, -2 };
        for (int i = 0; i < 2; i++)
        {
            Coordinate targetSquare = new Coordinate(square.X + dx[i], square.Y + dy[i]);
            if (allSquares.Contains(targetSquare))
            {
                possibleMoves.Add(targetSquare);
            }
        }
        return possibleMoves;
    }

    private static List<Coordinate> CalculateBlackPawnCaptureTargets(Coordinate square, HashSet<Coordinate> allSquares)
    {
        List<Coordinate> possibleMoves = new List<Coordinate>();
        int[] dx = { -1, 1 };
        int[] dy = { -1, -1 };
        for (int i = 0; i < 2; i++)
        {
            Coordinate targetSquare = new Coordinate(square.X + dx[i], square.Y + dy[i]);
            if (allSquares.Contains(targetSquare))
            {
                possibleMoves.Add(targetSquare);
            }
        }
        return possibleMoves;
    }
}