using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Uncy.board
{
    /*
     * Forsyth-Edwards-Notation (FEN) https://de.wikipedia.org/wiki/Forsyth-Edwards-Notation
     * Differences to the standard:
     * 
     * Piece Positional Information:
     * - allows different board sizes than 8x8. For that, the program counts the amount of rows and files that are declared.
     * - allows symbol 'x' to declare an inactive square
     * 
     * Castling Rights (FOR NOW):
     * - Only nearest Rook to the king can be used for castling. This way, the standard notation still works.
     * 
     * EnPassant Captures:
     * - Since the board may be bigger than standard size, the alphabet from a-z might not be sufficient for the task of representing a square.
     *      Therefore a new syntax is used that goes by: file,rank      
     *      example: a3 -> 0,2   g6 -> 6,5
     *      - Note that we start counting from 0 which means to decrement numbers from "standard" notation.
     */
    public struct Fen
    {
        public string completeFEN;

        public string piecePositions;
        public string isWhiteToMove;
        public string castlingRights;
        public string possibleEnPassantCapture;
        public string halfMoveClock;
        public string moveCount;

        public Fen(string str)
        {
            this.completeFEN = str;

            string[] subFens = completeFEN.Split(" ");

            this.piecePositions = subFens[0];
            this.isWhiteToMove = subFens[1];
            this.castlingRights = subFens[2];
            this.possibleEnPassantCapture = subFens[3];
            this.halfMoveClock = subFens[4];
            this.moveCount = subFens[5];

        }
    }
}
