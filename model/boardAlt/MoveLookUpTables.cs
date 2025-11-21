using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace uncy.model.boardAlt
{
    internal class MoveLookUpTables
    {
        public int BoardWidth { get; }
        public int BoardHeight { get; }
        public int TotalSquares {  get; }

        // Direction offsets
        // N , NE , E , SE , S , SW , W , NW
        public readonly int[] directionOffsets;

        // NNE, NEE, SEE, SSE, SSW, SWW, NWW, NNW
        public readonly int[] knightOffsets;

        // Distance to Edge: This array gives information about the distance to an edge from a specified square. Look at directionOffsets for the sequence order. 
        // This is used for all sliding pieces.
        public readonly byte[] squaresToEdge;

        // Lookup table for knights for each square of the board
        // knightMoves will only store squares that are in fact reachable. a1 would only have b3 and c2 and therefore we don't want to store the additional 6 squares as -1.
        // Therefore we use the startIndex and count to accurately address the correct index and to know where and for how long to look up possible squares. 
        public readonly short[] knightMoves;
        public readonly int[] knightMoveStartIndex;
        public readonly int[] knightMoveCount;

        // Lookup for kings. follow similar pattern as knights.
        public readonly short[] kingMoves;
        public readonly int[] kingMoveStartIndex;
        public readonly int[] kingMoveCount;

        public readonly short[] pawnAttackMoves;


        public MoveLookUpTables(Board board)
        {
            BoardWidth = board.dimensionsOfBoard.Item1;
            BoardHeight = board.dimensionsOfBoard.Item2;
            TotalSquares = BoardWidth * BoardHeight;

            directionOffsets = new int[8];
            DetermineOffsets();

            knightOffsets = new int[8];
            DetermineKnightOffsets();

            squaresToEdge = new byte[TotalSquares * directionOffsets.Length]; // files * ranks * 8 (there will for now always only be 8 directions for sliding pieces)
            InitializeSquaresToEdge();

            knightMoveStartIndex = new int[TotalSquares];
            knightMoveCount = new int[TotalSquares];
            kingMoveStartIndex = new int[TotalSquares];
            kingMoveCount = new int[TotalSquares];


            List<short> knightMoveList = CalculateKnightMoves();
            knightMoves = new short[knightMoveList.Count];
            InitializeKnightMoveTable(knightMoveList);

            List<short> kingMoveList = CalculateKingMoves();
            kingMoves = new short[kingMoveList.Count];
            InitializeKingMoveTable(kingMoveList);

            pawnAttackMoves = new short[2 * TotalSquares * 2]; //  white and black * Totalsquares * two attack squares for each pawn 
            InitializePawnAttackMoves();

            //PrintSquaresToEdgeTable();
            //PrintKnightMovesTable();
            //PrintKingMovesTable();
            //PrintPawnAttackTable();
        }

        private void InitializePawnAttackMoves()
        {
            FillPawnAttackTableStupid();
            RemoveInvalidPawnAttackMoves();
        }

        private void RemoveInvalidPawnAttackMoves()
        {
            for (int i = 0; i < pawnAttackMoves.Length; i+= 2)
            {
                int currentSquare = i / 2;
                int file = currentSquare % BoardWidth;



                if (file == 0)
                {
                    pawnAttackMoves[i] = -1;
                }
                else if (file == BoardWidth - 1)
                {
                    pawnAttackMoves[i + 1] = -1;
                }

                if (pawnAttackMoves[i] < 0 || pawnAttackMoves[i] >= TotalSquares) pawnAttackMoves[i] = -1;
                if (pawnAttackMoves[i+1] < 0 || pawnAttackMoves[i+1] >= TotalSquares) pawnAttackMoves[i+1] = -1;
            }
        }

        private void FillPawnAttackTableStupid()
        {
            int[] pawnAttackDirections = { BoardWidth - 1, BoardWidth + 1, -BoardWidth - 1, - BoardWidth + 1  }; // NW , NE , SW , SE

            int blackColorOffset = TotalSquares * 2;

            int currentSquare = 0;
            for (int i = 0; i < pawnAttackMoves.Length / 2; i+=2)
            {
                pawnAttackMoves[i + 0] = (short) (currentSquare + pawnAttackDirections[0]);
                pawnAttackMoves[i + 1] = (short) (currentSquare + pawnAttackDirections[1]);
                pawnAttackMoves[i + 0 + blackColorOffset] = (short) (currentSquare + pawnAttackDirections[2]);
                pawnAttackMoves[i + 1 + blackColorOffset] = (short) (currentSquare + pawnAttackDirections[3]);

                currentSquare++;
            }
        }

        private void DetermineOffsets()
        {
            directionOffsets[0] = BoardWidth;
            directionOffsets[1] = BoardWidth+1;
            directionOffsets[2] = 1;
            directionOffsets[3] = -BoardWidth+1;
            directionOffsets[4] = -BoardWidth;
            directionOffsets[5] = -BoardWidth-1;
            directionOffsets[6] = -1;
            directionOffsets[7] = BoardWidth-1;
        }

        private void DetermineKnightOffsets()
        {
            knightOffsets[0] = 2*BoardWidth + 1;
            knightOffsets[1] = 1*BoardWidth + 2;
            knightOffsets[2] =-1*BoardWidth + 2;
            knightOffsets[3] =-2*BoardWidth + 1;
            knightOffsets[4] =-2*BoardWidth - 1;
            knightOffsets[5] =-1*BoardWidth - 2;
            knightOffsets[6] = 1*BoardWidth - 2;
            knightOffsets[7] = 2*BoardWidth - 1;
        }

        private void InitializeSquaresToEdge()
        {
            for (int i = 0; i < TotalSquares; i++)
            {
                for (int j = 0; j < directionOffsets.Length; j++)
                {
                    int currentSquare = i;
                    int offset = directionOffsets[j];
                    byte distanceCount = 0;


                    if (offset == BoardWidth) // North movements
                    {
                        while (currentSquare < TotalSquares-BoardWidth) // Not on the first or last rank
                        {
                            distanceCount++;
                            currentSquare += offset;
                        }
                    }

                    else if (offset == -BoardWidth) // South Movements
                    {
                        while (currentSquare >= BoardWidth)
                        {
                            distanceCount++;
                            currentSquare += offset;
                        }
                    }

                    else if (offset == 1) // East
                    {
                        
                        while (currentSquare % BoardWidth != BoardWidth - 1) // Not on the left border of the board
                        {
                            distanceCount++;
                            currentSquare += offset;
                        }
                    }

                    else if (offset == -1) // west
                    {
                        while (currentSquare % BoardWidth != 0)  // Not on the right border of the board
                        {
                            distanceCount++;
                            currentSquare += offset;
                        }
                    }

                    else if (offset == BoardWidth + 1) // Northeast
                    {
                        while (currentSquare < TotalSquares - BoardWidth && currentSquare % BoardWidth != BoardWidth - 1) // Not on the top rank or right border
                        {
                            distanceCount++;
                            currentSquare += offset;
                        }
                    }

                    else if (offset == -BoardWidth + 1) // Southeast
                    {
                        while (currentSquare >= BoardWidth && currentSquare % BoardWidth != BoardWidth - 1) // Not bottom rank or right border
                        {
                            distanceCount++;
                            currentSquare += offset;
                        }
                    }

                    else if (offset == -BoardWidth - 1) // Southwest
                    {
                        while (currentSquare >= BoardWidth && currentSquare % BoardWidth != 0) // Not bottom rank or left border
                        {
                            distanceCount++;
                            currentSquare += offset;
                        }
                    }

                    else if (offset == BoardWidth - 1) // Northwest
                    {
                        while (currentSquare < TotalSquares - BoardWidth && currentSquare % BoardWidth != 0) // Not top rank or left border
                        {
                            distanceCount++;
                            currentSquare += offset;
                        }
                    }

                    squaresToEdge[i * 8 + j] = distanceCount;

                }
            }
        }

        private void InitializeKnightMoveTable(List<short> moveList)
        {
            for (int i = 0; i < knightMoves.Length; i++)
            {
                knightMoves[i] = moveList[i];
            }
        }

        private List<short> CalculateKnightMoves()
        {
            List<short> moves = new List<short>();

            int startIndex = 0; // Counts the total squares. This is used for indexing purposes with knightMoveStartIndex.

            for (int currentSquare = 0; currentSquare < TotalSquares; currentSquare++) // Loop through all squares
            {
                knightMoveStartIndex[currentSquare] = startIndex;
                int moveCount = 0; // Counts the squares that were found from the currentSquare

                bool[] targets = { true, true, true, true, true, true, true, true}; // This list determines which squares will be looked at (depending on currentsquare)

                DisableKnightPossibleSquares(targets, currentSquare);

                for(int i = 0; i < targets.Length; i++)
                {
                    if (targets[i])
                    {
                        int nextSquare = currentSquare + knightOffsets[i];
                        moves.Add((short)nextSquare);
                        moveCount++;
                        startIndex++;
                    }
                }

                knightMoveCount[currentSquare] = moveCount;
                
            }


            return moves;
        }

        private void DisableKnightPossibleSquares(bool[] targets, int currentSquare)
        {
            int currentFile = currentSquare % BoardWidth;

            if (currentSquare < BoardWidth)
            {
                targets[2] = false;
                targets[3] = false;
                targets[4] = false;
                targets[5] = false;
            }

            if (currentSquare < BoardWidth * 2)
            {
                targets[3] = false;
                targets[4] = false;
            }

            if (currentSquare >= TotalSquares - BoardWidth)
            {
                targets[0] = false;
                targets[1] = false;
                targets[6] = false;
                targets[7] = false;
            }

            if (currentSquare >= TotalSquares - BoardWidth * 2)
            {
                targets[0] = false;
                targets[7] = false;
            }

            if (currentFile == BoardWidth - 1)
            {
                targets[0] = false;
                targets[1] = false;
                targets[2] = false;
                targets[3] = false;
            }

            if (currentFile == BoardWidth - 2)
            {
                targets[1] = false;
                targets[2] = false;
            }

            if (currentFile == 0)
            {
                targets[4] = false;
                targets[5] = false;
                targets[6] = false;
                targets[7] = false;
            }

            if (currentFile == 1)
            {
                targets[5] = false;
                targets[6] = false;
            }
        }

        private void InitializeKingMoveTable(List<short> moveList)
        {
            for (int i = 0; i < kingMoves.Length; i++)
            {
                kingMoves[i] = moveList[i];
            }
        }

        private List<short> CalculateKingMoves()
        {
            List<short> moves = new List<short>();

            int startIndex = 0;

            for (int currentSquare = 0; currentSquare < TotalSquares; currentSquare++)
            {
                kingMoveStartIndex[currentSquare] = startIndex;

                int moveCount = 0;

                bool[] targets = { true, true, true, true, true, true, true, true };
                DisableKingPossibleSquares(targets, currentSquare);

                for (int i = 0; i < targets.Length; i++)
                {
                    if(targets[i])
                    {
                        int nextSquare = currentSquare + directionOffsets[i];
                        moves.Add((short)nextSquare);
                        moveCount++;
                        startIndex++;
                    }
                }
                kingMoveCount[currentSquare] = moveCount;
            }
            return moves;
        }

        private void DisableKingPossibleSquares(bool[] targets, int currentSquare)
        {
            int currentFile = currentSquare % BoardWidth;

            if (currentSquare < BoardWidth)
            {
                targets[3] = false;
                targets[4] = false;
                targets[5] = false;
            }
            if(currentSquare >= TotalSquares - BoardWidth)
            {
                targets[0] = false;
                targets[1] = false;
                targets[7] = false;
            }
            if (currentFile == 0)
            {
                targets[5] = false;
                targets[6] = false;
                targets[7] = false;
            }
            if (currentFile == BoardWidth - 1)
            {
                targets[1] = false;
                targets[2] = false;
                targets[3] = false;
            }
        }
        
        private void PrintSquaresToEdgeTable()
        {
            Console.WriteLine($"Squares to edge Table has {squaresToEdge.Length} entries");
            for (int i = 0; i < squaresToEdge.Length; i++)
            {
                if (i % directionOffsets.Length == 0)
                {
                    Console.Write($"Square {(int)i / 8}:");
                }

                Console.Write(squaresToEdge[i]);


                if (i % directionOffsets.Length == 7)
                {
                    Console.WriteLine("");
                }
            }

        }
        
        private void PrintKnightMovesTable()
        {
            Console.WriteLine("------------------");
            Console.WriteLine($"Knight moves look up table has {knightMoves.Length} entries");
            for (int i = 0; i < TotalSquares; i++)
            {
                Console.Write($"Square {i}: ");
                for(int j = 0; j < knightMoveCount[i]; j++)
                {
                    Console.Write(knightMoves[knightMoveStartIndex[i] + j] + " ");
                }
                Console.WriteLine("");
            }
            Console.WriteLine("------------------");
        }

        private void PrintKingMovesTable()
        {
            Console.WriteLine("------------------");
            Console.WriteLine($"King move lookup table has {kingMoves.Length} entries");
            for (int i = 0; i < TotalSquares; i++)
            {
                Console.Write($"Square {i}: ");
                for (int j = 0; j < kingMoveCount[i]; j++)
                {
                    Console.Write(kingMoves[kingMoveStartIndex[i] + j] + " ");
                }
                Console.WriteLine("");
            }
            Console.WriteLine("------------------");
        }

        private void PrintPawnAttackTable()
        {
            Console.WriteLine("------------------");
            Console.WriteLine($"Pawn Attack lookup Table has {pawnAttackMoves.Length} entries");

            for (int i = 0; i < pawnAttackMoves.Length / 2; i += 2)
            {
                Console.WriteLine($"Square: {i / 2}: NW:{pawnAttackMoves[i]}, NE:{pawnAttackMoves[i + 1]}, SW:{pawnAttackMoves[i + TotalSquares * 2]}, SE:{pawnAttackMoves[i + TotalSquares * 2 + 1]}");
            }

            //foreach (var x in pawnAttackMoves)
            //{
            //    Console.WriteLine(x);
            //}
        }
    }
}
