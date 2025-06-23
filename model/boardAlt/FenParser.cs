using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using uncy.board;

namespace uncy.model.boardAlt
{
    internal class FenParser
    {
        public static (int, int) GetDimensionsOfBoard(Fen fen)
        {
            string str = fen.piecePositions;

            if (str == null || str.Length == 0)
            {
                throw new ArgumentException("Invalid Position Retrieved from FEN)");
            }

            int fileCount = 0;
            int rankCount = 1;

            foreach (char c in str)
            {
                if (c == '/')
                {
                    rankCount++;
                }
            }

            string firstRank = str.Split("/")[0]; // retrieves the first rank of the positional info from the fen notation. 

            // Loops through the first rank and counts its length
            for (int i = 0; i < firstRank.Length; i++)
            {
                if (char.IsLetter(firstRank[i]))
                {
                    fileCount++;
                }
                else if (char.IsDigit(firstRank[i]))
                {
                    string digit = "";
                    for (int j = i; j < firstRank.Length; j++)
                    {
                        if (char.IsDigit(firstRank[j]))
                        {
                            digit += firstRank[j];
                            continue;
                        }
                        else
                        {
                            i = j - 1;
                            break;
                        }
                    }
                    fileCount += int.Parse(digit);
                }
            }
            return (fileCount, rankCount);
        }

        public static int IdentifyDigitGroup(string str)
        {
            string digit = "";
            for (int j = 0; j < str.Length; j++)
            {
                if (char.IsDigit(str[j]))
                {
                    digit += str[j];
                    continue;
                }
                else
                {
                    break;
                }
            }
            return int.Parse(digit);
        }

        /*
         * Gives information about a square (file,rank) and/or its piece.
         * This should be used when the board hasn't learned this information yet.
         * Note, that the technical representation of files and ranks in this application starts at 0.  
         * 
         * identifier:
         * e = empty, but active    P = white Pawn      p = black Pawn
         * x = inactive square      N = white Knight    n = black Knight
         *                          B = white Bishop    b = black Bishop
         *                          R = white Rook      r = black Rook
         *                          Q = white Queen     q = black Queen
         *                          K = white King      k = black King
         */
        public static char GetSquareOccupationInformation(string str, int file, int rank, int rankDimension)
        {
            str = str.Split("/")[Math.Abs(rank - (rankDimension - 1))];
            str += "/";

            int count = file;

            while (count > 0)
            {
                if (char.IsLetter(str[0]))
                {
                    str = str.Substring(1);
                    count--;
                }
                else if (char.IsDigit(str[0]))
                {
                    count -= IdentifyDigitGroup(str);
                    if (count < 0)
                    {
                        return 'e';
                    }
                    while (char.IsDigit(str[0]))
                    {
                        str = str.Substring(1);
                    }

                }
            }
            if (char.IsDigit(str[0]))
            {
                return 'e';
            }
            else
            {
                return str[0];
            }
        }

        public static bool GetIfWhiteToMove(string str)
        {
            if (str.Equals("w"))
            {
                return true;
            }
            else if (str.Equals("b"))
            {
                return false;
            }
            else
            {
                throw new ArgumentException("Invalid argument. Needs to be 'w' for white or 'b' for black.");
            }
        }

        public static bool CanWhiteKingCastleKingSide(string str)
        {
            if (str.Contains("K"))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool CanWhiteKingCastleQueenSide(string str)
        {
            if (str.Contains("Q"))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool CanBlackKingCastleKingSide(string str)
        {
            if (str.Contains("k"))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool CanBlackKingCastleQueenSide(string str)
        {
            if (str.Contains("q"))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static (int, int) GetEnPassant(string str)
        {
            if (char.IsLetter(str[0]))
            {
                return (str[0] - 'a', str[1]);
            }
            return (-1, -1);
        }

        public static int GetHalfMoveCountSinceLastCaptureOrPawnMove(string str)
        {
            return int.Parse(str);
        }

        public static int GetMoveCount(string str)
        {
            return int.Parse(str);
        }
        /*
         * 
         */
    }

}

