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
            SetBoardDimensions(board, fen);
            CreateCoordinates(board, fen);
            DeleteSquares(board, fen.piecePositions);
            board.precomputedData = PrecomputedSquareDataFactory.GenerateAllData(board.squares);



            PlacePieces(board, fen.piecePositions);

            

            
            return board;
        }

        private static void SetBoardDimensions(PolymorphicChessBoard board, Fen fen)
        {
            board.boardDimensions = FenDataExtractor.GetDimensionsOfBoard(fen);
        }

        private static void CreateCoordinates(PolymorphicChessBoard board, Fen fen)
        {
            for (int i = 0; i < board.boardDimensions.Item1; i++)
            {
                for (int j = 0; j < board.boardDimensions.Item1; j++)
                {
                    //Console.WriteLine(i + "," + j);
                    Coordinate coord = new Coordinate(i, j);
                    board.squares.Add(coord);
                }
            }
        }

        private static void PlacePieces(PolymorphicChessBoard board, string occupationData)
        {
            foreach (Coordinate coord in board.squares)
            {
                char pieceIdentifier = FenDataExtractor.GetSquareOccupationInformation(occupationData, coord.X, coord.Y, board.boardDimensions.Item2);
                Piece p = PieceFactory.CreatePiece(pieceIdentifier);
                if (p != null)
                {
                    board.piecePositions.Add(coord, p);
                }
            }
        }

        private static void DeleteSquares(PolymorphicChessBoard board, string occupationData)
        {
            foreach (Coordinate coord in board.squares)
            {
                if (FenDataExtractor.GetSquareOccupationInformation(occupationData, coord.X, coord.Y, board.boardDimensions.Item2) == 'x')
                {
                    board.squares.Remove(new Coordinate(coord.X, coord.Y));

                    // shouldn't happen, but you never know
                    if (board.piecePositions.ContainsKey(new Coordinate(coord.X, coord.Y))){
                        board.piecePositions.Remove(new Coordinate(coord.X, coord.Y));
                    }
                }
            }
        }



    }
}
