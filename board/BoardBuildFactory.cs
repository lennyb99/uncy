using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace uncy.board
{
    internal class BoardBuildFactory
    {
        public static PolymorphicChessBoard CreateBoard(Fen fen)
        {
            PolymorphicChessBoard board = new PolymorphicChessBoard();

            CreateCoordinates(board, fen);

            board.precomputedData = PrecomputedSquareDataFactory.GenerateAllData(board.squares);

            
            return board;
        }


        private static void CreateCoordinates(PolymorphicChessBoard board, Fen fen)
        {
            int fileCount = FenDataExtractor.GetDimensionsOfBoard(fen).Item1;
            int rankCount = FenDataExtractor.GetDimensionsOfBoard(fen).Item2;
           
            for (int i = 0; i < fileCount; i++)
            {
                for (int j = 0; j < rankCount; j++)
                {
                    Console.WriteLine(i + "," + j);
                    Coordinate coord = new Coordinate(i, j);
                    board.squares.Add(coord);
                }
            }
        }

        private static void PlacePieces(PolymorphicChessBoard board, string occupationData)
        {

        }

        private static void DeleteSquares(PolymorphicChessBoard board, string occupationData)
        {

        }



    }
}
