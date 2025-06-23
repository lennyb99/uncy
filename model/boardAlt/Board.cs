using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using uncy.board;

namespace uncy.model.boardAlt
{
    internal class Board
    {
        // FEN information 
        public char[,]? board = null;
        public bool sideToMove; // bool true = white, false = black
        public (int, int) enPassantTargetSquare = (-1,-1); // Should be set to (-1,-1) if no en Passant is available
        public int halfMoveClock = 0;
        public int fullMoveCount = 1;

        public bool whiteKingShortCastle;
        public bool whiteKingLongCastle;
        public bool blackKingShortCastle;
        public bool blackKingLongCastle;
        
        // (fileCount, rankCount)
        public (int, int) dimensionsOfBoard = (0,0);

        public Board(Fen fen)
        {
            dimensionsOfBoard = FenParser.GetDimensionsOfBoard(fen);

            if(dimensionsOfBoard == (0,0))
            {
                Console.WriteLine("Invalid Board size detected.");
                return;
            }

            InitializeBoard(dimensionsOfBoard.Item1, dimensionsOfBoard.Item2, fen);
        }


        private void InitializeBoard(int fileSize, int rankSize, Fen fen)
        {
            board = new char[fileSize, rankSize];

            BoardInitializer.SetInformationOfSquaresFromFen(board, fen, rankSize);
            sideToMove = BoardInitializer.SetSideToMove(fen);
            enPassantTargetSquare = BoardInitializer.SetEnPassantTargetSquare(fen);
            halfMoveClock =  BoardInitializer.SetHalfMoveClock(fen);
            fullMoveCount = BoardInitializer.SetFullMoveCount(fen);
            BoardInitializer.UpdateCastlingInformation(fen, this);


            PrintBoardToConsole();
        }

        private void PrintBoardToConsole()
        {
            Console.WriteLine("--------------");
            if (board == null)
            {
                Console.WriteLine("Board is null!");
                return;
            }
            Console.WriteLine("Start printing Board to console.");
            for(int i = board.GetLength(0)-1; i >= 0; i--)
            {
                for (int j = 0; j < board.GetLength(1); j++)
                {
                    Console.Write(board[j,i].ToString() + " ");
                }
                Console.WriteLine();
            }

            if (sideToMove)
            {
                Console.WriteLine("White to Move..");
            }
            else
            {
                Console.WriteLine("Black to Move..");
            }

            if (whiteKingShortCastle)
            {
                Console.WriteLine("White king can castle short side.");
            }
            if (whiteKingLongCastle)
            {
                Console.WriteLine("White king can castle long side.");
            }

            if (blackKingShortCastle)
            {
                Console.WriteLine("Black king can castle short side.");
            }
            if (blackKingLongCastle)
            {
                Console.WriteLine("Black king can castle long side.");
            }

            if (enPassantTargetSquare == (-1, -1))
            {
                Console.WriteLine("No En passant possible");
            }
            else
            {
                Console.WriteLine("En passant possible on: " + enPassantTargetSquare);
            }

            Console.WriteLine("Halfmoves since last capture or pawn push: " + halfMoveClock);
            Console.WriteLine("Move: " + fullMoveCount);


            Console.WriteLine("Done.");
            Console.WriteLine("--------------");
        }

        private void MakeMove()
        {

        }

        private void UndoMove()
        {

        }

        /*
         * This method is used to calculate whether a pawn has reached his promotion square
         * 
         */
        public bool IsSquareAtEndOfBoardForWhite(int file, int rank)
        {
            if (!(rank < 32 && file < 32 && file >= 0 && rank >= 0)) // Point inside of board bounds
            {
                Console.WriteLine("Point outside of board bounds.");
                return false;
            }
            if (rank == dimensionsOfBoard.Item2-1 || board[file,rank+1] == 'x')
            {
                return true;
            }
            return false;
        }

        public bool IsSquareAtEndOfBoardForBlack(int file, int rank)
        {
            if (!(rank < 32 && file < 32 && file >= 0 && rank >= 0)) // Point inside of board bounds
            {
                Console.WriteLine("Point outside of board bounds.");
                return false;
            }
            if (rank == 0 || board[file, rank - 1] == 'x')
            {
                return true;
            }
            return false;
        }
    }
}
