using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using uncy.board;

namespace uncy.model.boardAlt
{
    /*
     * Helper class for the Board to outsource initializing methods.
     */
    internal static class BoardInitializer
    {
        public static void SetInformationOfSquaresFromFen(char[,] board, Fen fen, int rankDimension)
        {
            for (int i = 0; i < board.GetLength(0); i++)
            {
                for (int j = 0; j< board.GetLength(1); j++)
                {
                    board[i,j] = FenParser.GetSquareOccupationInformation(fen.piecePositions, i, j, rankDimension);
                }
            }
        }

        public static bool SetSideToMove(Fen fen)
        {
            if(fen.isWhiteToMove.ToLower().Equals("w"))
            {
                return true;
            }else if (fen.isWhiteToMove.ToLower().Equals("b"))
            {
                return false;
            }
            else
            {
                Console.WriteLine("Couldn't determine Side To Move");
                throw new ArgumentException();
            }
        }

        /*
         * Returns the value for the en passant target square with coordinates for file and rank.
         */
        public static (int,int) SetEnPassantTargetSquare(Fen fen)
        {
            if(fen.possibleEnPassantCapture.Equals("-"))
            {
                return (-1, -1);
            }
            string[] coords = fen.possibleEnPassantCapture.Split(','); // Splitting the information of the two squares into an array that holds the two coordinates
            return (int.Parse(coords[0]), int.Parse(coords[1]));
        }

        public static int SetHalfMoveClock(Fen fen)        
        {
            return int.Parse(fen.halfMoveClock);
        }

        public static int SetFullMoveCount(Fen fen)
        {
            return int.Parse(fen.moveCount);
        }

        public static void UpdateCastlingInformation(Fen fen, Board board)
        {
            if (fen.castlingRights.Contains('-')){
                return;
            }
            if (fen.castlingRights.Contains('K')){
                board.whiteKingShortCastle = true;
            }
            else
            {
                board.whiteKingShortCastle = false;
            }
            if (fen.castlingRights.Contains('Q'))
            {
                board.whiteKingLongCastle = true;
            }
            else
            {
                board.whiteKingLongCastle = false;
            }
            if (fen.castlingRights.Contains('k'))
            {
                board.blackKingShortCastle = true;
            }
            else
            {
                board.blackKingShortCastle = false;
            }
            if (fen.castlingRights.Contains('q'))
            {
                board.blackKingLongCastle = true;
            }
            else
            {
                board.blackKingLongCastle = false;
            }
        }
    }
}
