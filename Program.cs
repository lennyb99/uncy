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
        StartPerftDebug(board, 4);


        

        


        //Stopwatch stopwatch = Stopwatch.StartNew();
        //for (int i = 0; i < 4_000_000; i++)
        //{a
        //    board.ToFen();
        //}
        //stopwatch.Stop();
        //Console.WriteLine(stopwatch.ToString());


        //StartSearch(board, 6);

        //StartGrpcServer();

        //int[] nums = new int[10000000];
        //char[] chars = new char[9_000_000];
        //for(int i = 0; i < nums.Length; i++)
        //{
        //    nums[i] = new Random().Next(0, 100);
        //}
        //for (int i = 0; i < chars.Length; i++)
        //{
        //    chars[i] = 'e';
        //}
        //Stopwatch sw = Stopwatch.StartNew();

        //foreach (int num in nums)
        //{
        //    if(num == 27)
        //    {

        //    }
        //}
        //sw.Stop();
        //Console.WriteLine(sw.ToString());

        //sw.Reset();
        //sw.Start();

        //for (int i = 0; i < 9_500_000; i++) 
        //{
        //    if (board.board[1,1] == 'e' || board.board[1, 1] == 'x')
        //    {

        //    }
        //}
        //sw.Stop();
        //Console.WriteLine(sw.ToString());

        //Test();
        //TestTwo();
    }

    private static void Test()
    {
        // 1. Vorbereitung: Ein großes Array erstellen, das unsere "Millionen von Feldern" darstellt.
        // Wir füllen es mit zufälligen Daten, damit der Compiler keine Muster erkennen kann.
        const int operations = 9_500_000;
        char[] data = new char[operations];
        Random rand = new Random();
        char[] pieces = { 'P', 'p', 'N', 'n', 'e', 'e', 'e', 'e', 'x' }; // Eine Auswahl an möglichen Werten

        for (int i = 0; i < data.Length; i++)
        {
            data[i] = pieces[rand.Next(pieces.Length)];
        }

        Console.WriteLine("Test-Setup abgeschlossen. Starte den Benchmark...");
        Console.WriteLine("-------------------------------------------------");

        // 2. Der eigentliche Test: Wir durchlaufen das Array und führen eine einfache Prüfung durch.
        // Das Ergebnis der Prüfung wird verwendet, um eine Zählervariable zu erhöhen.
        long emptyOrInactiveCounter = 0;

        Stopwatch sw = Stopwatch.StartNew();

        for (int i = 0; i < data.Length; i++)
        {
            // Diese Prüfung ist der Kern des Tests.
            // Sie greift jedes Mal auf eine NEUE Speicherstelle zu (data[i]).
            if (data[i] == 'e' || data[i] == 'x')
            {
                // WICHTIG: Wir führen eine echte Operation durch.
                // Dadurch kann der Compiler die Schleife nicht als "nutzlos" entfernen.
                emptyOrInactiveCounter++;
            }
        }

        sw.Stop();

        // 3. Das Ergebnis ausgeben.
        Console.WriteLine($"Zeit für 9.500.000 Prüfungen: {sw.ElapsedMilliseconds} ms");
        Console.WriteLine($"Leere oder inaktive Felder gefunden: {emptyOrInactiveCounter:N0}");
        Console.WriteLine("\nDieser Wert ist nicht mehr Null, weil der Compiler gezwungen wurde, die Arbeit tatsächlich auszuführen.");
    }

    private static void TestTwo()
    {
        // === VORBEREITUNG ===
        // Wir brauchen eine Größe, die sowohl für 1D als auch für 2D funktioniert.
        // Nehmen wir eine quadratische Größe, die ungefähr 9,5 Millionen Elementen entspricht.
        // Wurzel aus 9,5 Mio ist ca. 3082.
        const int size = 3082;
        const int totalElements = size * size; // ca. 9.5 Millionen

        // Zufällige Daten für beide Arrays erstellen
        char[] data1D = new char[totalElements];
        char[,] data2D = new char[size, size];
        Random rand = new Random();
        char[] pieces = { 'P', 'p', 'N', 'n', 'e', 'e', 'e', 'e', 'x' };

        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                char piece = pieces[rand.Next(pieces.Length)];
                data1D[i * size + j] = piece;
                data2D[i, j] = piece;
            }
        }

        Console.WriteLine($"Setup: {totalElements:N0} Elemente in einem 1D- und einem 2D-Array ({size}x{size}) erstellt.");
        Console.WriteLine("Die Tests werden nun ausgeführt. Compiler-Optimierungen sind durch die Zähler verhindert.");
        Console.WriteLine("--------------------------------------------------------------------------------------\n");

        // === TEST 1: Eindimensionales Array (Mailbox-Stil) ===
        long counter1D = 0;
        Stopwatch sw1D = Stopwatch.StartNew();

        for (int i = 0; i < totalElements; i++)
        {
            if (data1D[i] == 'e' || data1D[i] == 'x')
            {
                counter1D++;
            }
        }

        sw1D.Stop();
        Console.WriteLine($"Test 1 (1D-Array): {sw1D.ElapsedMilliseconds} ms");
        Console.WriteLine($"Gefunden: {counter1D:N0} Elemente\n");

        // === TEST 2: Zweidimensionales Array ===
        long counter2D = 0;
        Stopwatch sw2D = Stopwatch.StartNew();

        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                if (data2D[i, j] == 'e' || data2D[i, j] == 'x')
                {
                    counter2D++;
                }
            }
        }

        sw2D.Stop();
        Console.WriteLine($"Test 2 (2D-Array): {sw2D.ElapsedMilliseconds} ms");
        Console.WriteLine($"Gefunden: {counter2D:N0} Elemente\n");

        // === FAZIT ===
        Console.WriteLine("--------------------------------------------------------------------------------------");
        double difference = ((double)sw2D.ElapsedMilliseconds / sw1D.ElapsedMilliseconds - 1) * 100;
        Console.WriteLine($"Fazit: Das 2D-Array war in diesem Test um ca. {difference:F1}% langsamer.");
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