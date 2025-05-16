using System;
using System.Diagnostics;
class Program
{
    static void Main(string[] args)
    {
        Stopwatch stopwatch = new Stopwatch();

        stopwatch.Start();

        PolymorphicChessBoard board = new PolymorphicChessBoard();

        for (int i = 1; i <= 10; i++)
        {
            for (int j = 1; j <= 10; j++)
            {
                Coordinate coord = new Coordinate(i, j);
                board.squares.Add(coord);
            }
        }

        board.precomputedData = PrecomputedSquareDataFactory.GenerateAllData(board.squares);







        stopwatch.Stop();
        Console.WriteLine("Done!");

        // foreach (Coordinate coord in board.squares)
        // {
        //     Console.WriteLine(coord.ToString());
        // }
        foreach (var kvp in board.precomputedData)
        {
            int count = kvp.Value.kingMoves.Count
                        + kvp.Value.knightMoves.Count
                        + kvp.Value.diagonalSlidingRays.Values.Sum(list => list?.Count ?? 0)
                        + kvp.Value.verticalSlidingRays.Values.Sum(list => list?.Count ?? 0)
                        + kvp.Value.pawnPushTargets.Count
                        + kvp.Value.pawnCaptureTargets.Count;
            Console.WriteLine(kvp.Key.ToString() + ": possible Reaching Squares sum: " + count + "; kingmoves:" + kvp.Value.kingMoves.Count + "; knightmoves:" + kvp.Value.knightMoves.Count +
                                "; diagonal slide moves: " + kvp.Value.diagonalSlidingRays.Values.Sum(list => list?.Count ?? 0) + "; vertical slide moves: " + kvp.Value.verticalSlidingRays.Values.Sum(list => list?.Count ?? 0) + "; pawn push moves:" + kvp.Value.pawnPushTargets.Count + "; pawn capture moves:" + kvp.Value.pawnCaptureTargets.Count);
        }

        Console.WriteLine($"Dauer: {stopwatch.ElapsedMilliseconds} ms");


        long memory = Process.GetCurrentProcess().WorkingSet64;
        // Console.WriteLine($"Arbeitsspeicher: {memory / 1024 / 1024} MB");


        // for (int i = 0; i < 4000000; i++)
        // {

        // }

        memory = Process.GetCurrentProcess().WorkingSet64;
        Console.WriteLine($"Arbeitsspeicher: {memory / 1024 / 1024} MB");
    }
}