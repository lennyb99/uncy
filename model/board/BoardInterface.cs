using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using uncy.board;
using uncy.controller;

namespace uncy.model.board
{
    public class BoardInterface
    {
        MainController controller;
        PolymorphicChessBoard board;

        public BoardInterface(MainController cont)
        {
            controller = cont;

            InitializeBoard();
        }

        private void InitializeBoard()
        {
            string standardBoard = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";

            Fen fen = new Fen(standardBoard);

            Stopwatch stopwatch = new Stopwatch();

            stopwatch.Start();

            board = BoardBuildFactory.CreateBoard(fen);

            stopwatch.Stop();
            Console.WriteLine("Done!");
            Console.WriteLine($"Dauer: {stopwatch.ElapsedMilliseconds} ms");

            
            List<BigInteger> list = new List<BigInteger>();
            long memory = Process.GetCurrentProcess().WorkingSet64;
            Console.WriteLine($"Arbeitsspeicher: {memory / 1024 / 1024} MB");
            for (int i = 0; i < 10000000; i++)
            {
                list.Add(BigInteger.Parse("10000000000000000000000000000000000000000000000000"));
            }
            Console.WriteLine(list.Count);
            memory = Process.GetCurrentProcess().WorkingSet64;
            Console.WriteLine($"Arbeitsspeicher: {memory / 1024 / 1024} MB");


            //foreach (var kvp in board.pieceStartingPositions)
            //{
            //    if (kvp.Value != null)
            //    {
            //        Console.WriteLine(kvp.Key.ToString() + ": " + kvp.Value.ToString());
            //    }
            //}


        }

        public void SendBoardDataToController()
        {
            controller.InputNewBoardData(board.squares, board.piecePositions);
        }


        public (int,int) GetBoardDimensions()
        {
            if(board == null)
            {
                return (0, 0);
            }
            else
            {
                return board.boardDimensions;
            }
        }
    }
}
