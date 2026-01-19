using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uncy.board;

namespace Uncy.Shared.boardAlt
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
            
            /*
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
            */

            fileCount = CalculateScore(firstRank);
            return (fileCount, rankCount);

        }

        public static int CalculateScore(string input)
        {
            if (input is null) throw new ArgumentNullException(nameof(input));

            int score = 0;
            for (int i = 0; i < input.Length;)
            {
                char c = input[i];

                if (char.IsDigit(c))
                {
                    int start = i;
                    while (i < input.Length && char.IsDigit(input[i])) i++;

                    string numberPart = input.Substring(start, i - start);
                    if (int.TryParse(numberPart, out int value))
                        score += value;
                }
                else if (char.IsLetter(c))
                {
                    score += 1;
                    i++;
                }
                else
                {
                    i++;
                }
            }

            return score;
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
        public static void GetSquareOccupationInformation(byte[] board, string str, int rankDimension)
        {
            string[] stringArray = str.Split('/');
            Array.Reverse(stringArray);
            str = string.Join("/", stringArray);

            Console.WriteLine($"Board size is: {board.Length}");

            int boardIndex = 0;
            for(int i = 0; i < str.Length; i++)
            {
                if (str[i] == '/') continue;
                if (char.IsLetter((char)str[i]))
                {
                    board[boardIndex] = IdentifyOccupation(str[i]);
                    boardIndex++;
                }
                else if (char.IsDigit(str[i]))
                {
                    for (int j = (int)char.GetNumericValue(str[i]); j > 0; j--)
                    {
                        board[boardIndex] = 0;
                        boardIndex++;
                    }
                }
                else
                {
                    throw new ArgumentException($"Invalid char has been found in the FEN: {str[i]}");
                }
            }
        }

        private static byte IdentifyOccupation(char c)
        {
            byte p = 0;

            switch (char.ToLower(c))
            {
                case 'p':
                    p += Piece.Pawn;
                    break;
                case 'n':
                    p += Piece.Knight;
                    break;
                case 'b':
                    p += Piece.Bishop;
                    break;
                case 'r':
                    p += Piece.Rook;
                    break;
                case 'q':
                    p += Piece.Queen;
                    break;
                case 'k':
                    p += Piece.King;
                    break;
                case 'x':
                    p += Piece.Inactive;
                    return p;
                case 'e':
                    p += Piece.Empty;
                    return p;
                default:
                    throw new ArgumentException($"Invalid char has been found in the FEN: {c}");
            }

            if (char.IsUpper(c))
            {
                p += Piece.White;
            }
            else
            {
                p += Piece.Black;
            }

            return p;
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
            if (char.IsDigit(str[0]))
            {
                return (Convert.ToInt32(str.Split(",")[0]), Convert.ToInt32(str.Split(",")[1]));
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

