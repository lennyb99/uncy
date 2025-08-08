using System;
using System.Diagnostics;
using System.IO.Pipelines;
using System.Threading.Tasks;
using System.Windows.Forms;
using uncy.board;
using System.Linq;
using uncy.gui;
using uncy.controller;
using uncy.model.board;
using uncy.view;
using uncy.model.boardAlt;
using System.Runtime.CompilerServices;
using uncy.model.Tools;
using uncy.model.eval;
using uncy.model.search;
using Uncy.Model.Api.Examples;
class Program
{
    static void Main(string[] args)
    {
        Fen fen = new Fen("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1");
        Fen fenRook = new Fen("k7/8/8/8/3R4/8/8/K7 w KQkq - 0 1");
        Fen fenQueen = new Fen("k7/8/8/8/3Q4/8/8/K7 w KQkq - 0 1");
        Fen fenBishop = new Fen("8/8/8/8/3B4/8/8/8 w KQkq - 0 1");
        Fen fenKnight = new Fen("k7/8/8/8/3N4/8/8/K7 w KQkq - 0 1");

        Fen debugFen = new Fen("2Q3K1/8/8/5n2/8/8/8/7k b - - 0 1");
        Fen debugFenTwo = new Fen("r1q1r1k1/1p2bppp/p2p1n2/2pP4/P1b1P3/2N1B3/1P2NPPP/R2QR1K1 w - - 3 16");
        Fen maxSizeFen = new Fen("K29/30/30/30/30/30/30/30/30/30/30/30/30/30/30/30/30/30/30/30/30/30/30/30/30/30/30/30/30/29k w - - 0 1");
        Fen closedPositionWithBishops = new Fen("5b2/3k4/1p1p1p1p/pPpPpPpP/P1P1P1P1/8/3BK3/8 w - - 0 1");

        Board board = new Board(debugFenTwo);

        //StartPerftDebug(board, 4);

        StartSearch(board, 6);
        
        //StartGrpcServer();
    }

    private static void StartSearch(Board board, int depth)
    {
        TranspositionTable tt = new TranspositionTable(256);
        IEvaluator evaluator = new CompositeEvaluator(
            (new MaterialEvaluator(), 100));

        Search search = new Search(evaluator, tt);


        Move move = search.FindBestMove(board, depth);


        Console.WriteLine(move.ToString());
        board.MakeMove(move, out Undo undo);
        board.PrintBoardToConsoleShort();
    }

    private static void StartPerftDebug(Board board, int depth)
    {
        Console.WriteLine("--------");
        Console.WriteLine($"Starting PERFT (depth:{depth}) debugging:");
        var sw = Stopwatch.StartNew();

        Perft.PerftDivide(depth, board);

        sw.Stop();
        Console.WriteLine($"Duration of: {sw.Elapsed}");
        Console.WriteLine($"Dauer (ms): {sw.ElapsedMilliseconds} ms");
        Console.WriteLine("--------");
    }

    public static void StartGrpcServer()
    {
        Console.WriteLine("Starting gRPC Server...");
        var grpcExample = new GrpcServerExample();

        // Start server and keep the application alive
        StartServerAndWait(grpcExample).GetAwaiter().GetResult();
    }

    private static async Task StartServerAndWait(GrpcServerExample grpcExample)
    {
        try
        {
            await grpcExample.StartServerAsync(5001);
            Console.WriteLine("gRPC Server is running on port 5001");
            Console.WriteLine("Press 'q' and Enter to quit the server...");

            // Keep the application alive until user wants to quit
            string input;
            do
            {
                input = Console.ReadLine();
            } while (input?.ToLower() != "q");

            Console.WriteLine("Stopping gRPC Server...");
            await grpcExample.StopServerAsync();
            Console.WriteLine("gRPC Server stopped. Application exiting.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error with gRPC Server: {ex.Message}");
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }


}