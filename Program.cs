using System;
using System.Diagnostics;
using System.IO.Pipelines;
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

        Fen debugFen = new Fen("r4rk1/1pp1qppp/p1np1n2/2b1p1B1/2B1P1b1/P1NP1N2/1PP1QPPP/R4RK1 w - - 0 10");

        Board board = new Board(debugFen);

        //List<Move> moves = MoveGenerator.GenerateLegalMoves(board, true);
        //Console.WriteLine(moves.Count);


        //var sw = Stopwatch.StartNew();
        //Console.WriteLine(Perft.Run_Perft(1, board));
        //sw.Stop();
        //Console.WriteLine($"Dauer: {sw.Elapsed}");
        //Console.WriteLine($"Dauer (ms): {sw.ElapsedMilliseconds} ms");



        Perft.PerftDivide(4, board);

        
        
        
        //Application.Run(form);
    }

    
}