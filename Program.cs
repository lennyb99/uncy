using System;
using System.Diagnostics;
using System.IO.Pipelines;
using System.Threading.Tasks;
using System.Windows.Forms;
using uncy.board;
using System.Linq;
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

        Fen tempFen = new Fen("r4rk1/1pp1qppp/p1np1n2/2b1p1B1/2B1P1b1/P1NP1N2/1PP1QPPP/R4RK1 w - - 0 10");

        Fen debugFen = new Fen("r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R w KQkq - 0 1");
        Fen debugFenTwo = new Fen("rnbqkbnr/p1pppppp/1p6/8/8/1P6/P1PPPPPP/RNBQKBNR w KQkq - 0 1");
        Fen maxSizeFen = new Fen("K29/30/30/30/30/30/30/30/30/30/30/30/30/30/30/30/30/30/30/30/30/30/30/30/30/30/30/30/30/29k w - - 0 1");
        Fen closedPositionWithBishops = new Fen("5b2/3k4/1p1p1p1p/pPpPpPpP/P1P1P1P1/8/3BK3/8 w - - 0 1");
        Fen mateToFind = new Fen("r2r2k1/pp3ppp/2p2b2/5q2/4RB2/1P3PP1/P4P1P/3QR1K1 w - - 0 1");

        Board board = new Board(fen);
        //board.PrintBoardToConsole();

        //Console.WriteLine(board.ToFen());

        // Original Perft (misst viel mehr als nur Move-Generation)
        //StartPerftDebug(board, 5);
        //StartSearch(board, 8);

        // Vergleich: Alle Perft-Varianten
        //CompareAllPerftVariants(board, 5);

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
        board.PrintBoardToConsole();
    }

    private static void StartPerftDebug(Board board, int depth)
    {
        Console.WriteLine("--------");
        Console.WriteLine($"Starting PERFT (depth:{depth}) debugging:");
        var sw = Stopwatch.StartNew();

        Perft.PerftDivideFast(depth, board);

        sw.Stop();
        Console.WriteLine($"Duration of: {sw.Elapsed}");
        Console.WriteLine($"Dauer (ms): {sw.ElapsedMilliseconds} ms");
        Console.WriteLine("--------");
    }


    private static void CompareAllPerftVariants(Board board, int depth)
    {
        Console.WriteLine("╔════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║         ALL PERFT VARIANTS COMPARISON                     ║");
        Console.WriteLine("╚════════════════════════════════════════════════════════════╝");
        Console.WriteLine($"\nTesting all Perft variants at depth {depth}...\n");

        ulong expectedNodes = 4865609; // Bekannte Anzahl für Startposition depth 5

        // 1. Run_Perft (mit DEBUG-Checks - langsam)
        Console.WriteLine("1️⃣  Run_Perft (with DEBUG checks)...");
        var sw1 = Stopwatch.StartNew();
        ulong nodes1 = Perft.Run_Perft(depth, board);
        sw1.Stop();
        Console.WriteLine($"   Nodes: {nodes1:N0} | Time: {sw1.ElapsedMilliseconds} ms\n");

        // 2. Run_PerftFast (ohne DEBUG-Checks)
        Console.WriteLine("2️⃣  Run_PerftFast (without DEBUG checks)...");
        var sw2 = Stopwatch.StartNew();
        ulong nodes2 = Perft.Run_PerftFast(depth, board);
        sw2.Stop();
        Console.WriteLine($"   Nodes: {nodes2:N0} | Time: {sw2.ElapsedMilliseconds} ms\n");

        // 3. Run_PerftMinimal (minimalste Version)
        Console.WriteLine("3️⃣  Run_PerftMinimal (minimal overhead)...");
        var sw3 = Stopwatch.StartNew();
        ulong nodes3 = Perft.Run_PerftMinimal(depth, board);
        sw3.Stop();
        Console.WriteLine($"   Nodes: {nodes3:N0} | Time: {sw3.ElapsedMilliseconds} ms\n");

        // 4. PerftSimple (Wrapper für Run_PerftFast)
        Console.WriteLine("4️⃣  PerftSimple (alias for Run_PerftFast)...");
        var sw4 = Stopwatch.StartNew();
        ulong nodes4 = Perft.PerftSimple(depth, board);
        sw4.Stop();
        Console.WriteLine($"   Nodes: {nodes4:N0} | Time: {sw4.ElapsedMilliseconds} ms\n");

        // Ergebnisse
        Console.WriteLine("╔════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║                      RESULTS                              ║");
        Console.WriteLine("╚════════════════════════════════════════════════════════════╝\n");

        Console.WriteLine($"Expected nodes: {expectedNodes:N0}\n");

        var results = new[]
        {
            ("Run_Perft (DEBUG)", sw1.ElapsedMilliseconds, nodes1),
            ("Run_PerftFast", sw2.ElapsedMilliseconds, nodes2),
            ("Run_PerftMinimal", sw3.ElapsedMilliseconds, nodes3),
            ("PerftSimple", sw4.ElapsedMilliseconds, nodes4)
        };

        long fastestTime = results.Min(r => r.Item2);

        foreach (var (name, time, nodes) in results)
        {
            double speedup = time / (double)fastestTime;
            string status = nodes == expectedNodes ? "✅" : "❌";
            Console.WriteLine($"{status} {name,-20} {time,8} ms | {speedup:F2}x | Nodes: {nodes:N0}");
        }

        Console.WriteLine($"\n💡 Empfehlung für Performance-Tests:");
        Console.WriteLine($"   - Schnellste: Run_PerftFast oder PerftSimple");
        Console.WriteLine($"   - Minimalster Overhead: Run_PerftMinimal");
        Console.WriteLine($"   - Mit Divide-Ausgabe: PerftDivideFast(verbose: false)\n");
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