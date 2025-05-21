using System;
using System.Diagnostics;
using System.IO.Pipelines;
using System.Windows.Forms;
using uncy.board;
using System.Linq;
class Program
{ 
    static void Main(string[] args)
    {
        string test = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";

        Fen fen = new Fen(test);

        //Console.WriteLine(fen.completeFEN);
        //Console.WriteLine(fen.piecePositions);
        //Console.WriteLine(fen.isWhiteToMove);
        //Console.WriteLine(fen.castlingRights);
        //Console.WriteLine(fen.possibleEnPassantCapture);
        //Console.WriteLine(fen.moveCountSinceLastCaptureOrPawnMove);
        //Console.WriteLine(fen.moveCount);

        //Console.WriteLine(fen.completeFEN.Length);
        //Console.WriteLine(fen.piecePositions.Length);
        //Console.WriteLine(fen.isWhiteToMove.Length);
        //Console.WriteLine(fen.castlingRights.Length);
        //Console.WriteLine(fen.possibleEnPassantCapture.Length);
        //Console.WriteLine(fen.moveCountSinceLastCaptureOrPawnMove.Length);
        //Console.WriteLine(fen.moveCount.Length);

       

        
        
        
        
        Stopwatch stopwatch = new Stopwatch();

        stopwatch.Start();

        PolymorphicChessBoard board = BoardBuildFactory.CreateBoard(fen);


        Console.WriteLine(FenDataExtractor.GetDimensionsOfBoard(fen));

        Console.WriteLine(FenDataExtractor.GetSquareOccupationInformation(fen.piecePositions, 1,0, board.boardDimensions.Item2));

        stopwatch.Stop();
        Console.WriteLine("Done!");


        //board.squares.Remove(new Coordinate(5, 5));
        //board.squares.Remove(new Coordinate(5, 4));
        //board.squares.Remove(new Coordinate(4, 4));
        //board.squares.Remove(new Coordinate(4, 5));



        //foreach (var kvp in board.precomputedData)
        //{
        //    int count = kvp.Value.kingMoves.Count
        //                + kvp.Value.knightMoves.Count
        //                + kvp.Value.diagonalSlidingRays.Values.Sum(list => list?.Count ?? 0)
        //                + kvp.Value.verticalSlidingRays.Values.Sum(list => list?.Count ?? 0)
        //                + kvp.Value.whitePawnPushTargets.Count
        //                + kvp.Value.whitePawnCaptureTargets.Count
        //                + kvp.Value.blackPawnPushTargets.Count
        //                + kvp.Value.blackPawnCaptureTargets.Count;

        //    Console.WriteLine(kvp.Key.ToString() + ": possible Reaching Squares sum: " + count + "; kingmoves:" + kvp.Value.kingMoves.Count + "; knightmoves:" + kvp.Value.knightMoves.Count +
        //                        "; diagonal slide moves: " + kvp.Value.diagonalSlidingRays.Values.Sum(list => list?.Count ?? 0) + "; vertical slide moves: " + kvp.Value.verticalSlidingRays.Values.Sum(list => list?.Count ?? 0) + "; white pawn push moves:" + kvp.Value.whitePawnPushTargets.Count + "; white pawn capture moves:" + kvp.Value.whitePawnCaptureTargets.Count + "; black pawn push moves:" + kvp.Value.blackPawnPushTargets.Count + "; black pawn capture moves:" + kvp.Value.blackPawnCaptureTargets.Count);
        //}

        Console.WriteLine($"Dauer: {stopwatch.ElapsedMilliseconds} ms");


        //long memory = Process.GetCurrentProcess().WorkingSet64;
        // Console.WriteLine($"Arbeitsspeicher: {memory / 1024 / 1024} MB");


        foreach (var kvp in board.piecePositions)
        {
            if (kvp.Value != null)
            {
                Console.WriteLine(kvp.Key.ToString() + ": " + kvp.Value.ToString());
            }
        }
        
        
        OpenForm(board.squares);
    }

    private static void OpenForm(HashSet<Coordinate> initialSquares)
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new MainForm(initialSquares));


    }
}