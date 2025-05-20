using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace uncy.board
{
    /*
     * Forsyth-Edwards-Notation (FEN) https://de.wikipedia.org/wiki/Forsyth-Edwards-Notation
     * Differences to the standard:
     * - allows different board sizes than 8x8. For that, the program counts the amount of rows and files that are declared.
     * - allows symbol 'x' to declare an inactive square
     */
    public struct Fen
    {
        public string completeFEN;

        public string piecePositions;
        public string isWhiteToMove;
        public string castlingRights;
        public string possibleEnPassantCapture;
        public string moveCountSinceLastCaptureOrPawnMove;
        public string moveCount;

        public Fen(string str)
        {
            this.completeFEN = str;

            string[] subFens = completeFEN.Split(" ");

            this.piecePositions = subFens[0];
            this.isWhiteToMove = subFens[1];
            this.castlingRights = subFens[2];
            this.possibleEnPassantCapture = subFens[3];
            this.moveCountSinceLastCaptureOrPawnMove = subFens[4];
            this.moveCount = subFens[5];

        }
    }
}
