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
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Form1 form = new Form1();





        //MainController controller = new MainController(form);

        
        Fen fen = new Fen("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1");
        Fen fenRook = new Fen("k7/8/8/8/3R4/8/8/K7 w KQkq - 0 1");
        Fen fenQueen = new Fen("k7/8/8/8/3Q4/8/8/K7 w KQkq - 0 1");
        Fen fenBishop = new Fen("8/8/8/8/3B4/8/8/8 w KQkq - 0 1");
        Fen fenKnight = new Fen("k7/8/8/8/3N4/8/8/K7 w KQkq - 0 1");

        Fen debugFen = new Fen("2Q3K1/8/8/5n2/8/8/8/7k b - - 0 1");
        Fen debugFenTwo = new Fen("r1bqk2r/1ppp1ppp/2n2n2/2b1p3/p1B1P3/P4N2/1PPP1PPP/RNBQK2R w KQkq - 0 1");
        Fen maxSizeFen = new Fen("K29/30/30/30/30/30/30/30/30/30/30/30/30/30/30/30/30/30/30/30/30/30/30/30/30/30/30/30/30/29k w - - 0 1");

        Board board = new Board(debugFenTwo);


        //List<Move> moves = MoveGenerator.GenerateLegalMoves(board, true);
        //Console.WriteLine(moves.Count);


        //var sw = Stopwatch.StartNew();
        //Console.WriteLine(Perft.Run_Perft(1, board));
        //sw.Stop();
        //Console.WriteLine($"Dauer: {sw.Elapsed}");
        //Console.WriteLine($"Dauer (ms): {sw.ElapsedMilliseconds} ms");


        Perft.PerftDivide(4, board);
        //board.MakeMove(new Move(1, 1, 1, 2, 'P'), out Undo undo);


        /*
        IEvaluator evaluator = new CompositeEvaluator(
            (new MaterialEvaluator(), 100));

        Search search = new Search(evaluator);

        Console.WriteLine(evaluator.Evaluate(board));

        Move move = search.FindBestMove(board, 4);
        Console.WriteLine(move.ToString());
        board.MakeMove(move, out Undo undo);
        board.PrintBoardToConsoleShort();
        Console.WriteLine(evaluator.Evaluate(board));
        */


        //StartGrpcServer();

        //Application.Run(form);
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