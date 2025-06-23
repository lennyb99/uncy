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
class Program
{ 
    static void Main(string[] args)
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Form1 form = new Form1();





        //MainController controller = new MainController(form);

        Fen fen = new Fen("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1");
        Fen fenRook = new Fen("8/8/8/8/8/8/8/R7 w KQkq - 0 1");
        Fen fenQueen = new Fen("8/8/8/8/3Q4/8/8/8 w KQkq - 0 1");
        Fen fenBishop = new Fen("8/8/8/8/3B4/8/8/8 w KQkq - 0 1");
        Fen fenKnight = new Fen("8/8/8/8/1N6/8/8/8 w KQkq - 0 1");


        Board board = new Board(fen);

        //List<Move> moves = MoveGenerator.GenerateLegalMoves(board, true);
        //Console.WriteLine(moves.Count);
        //foreach (Move move in moves)
        //{
        //    Console.WriteLine(move.ToString());
        //}

        List<Move> moves = new List<Move>();



        //Application.Run(form);
    }

    
}