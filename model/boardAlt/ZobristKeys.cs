using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace uncy.model.boardAlt
{
    internal class ZobristKeys
    {

        /*
         *  three dimensional array
         *  first value represents the file, second value the rank and the third value the piecetype 
         *  
         *  piecetypes  0 = P  6 = p
         *              1 = N  7 = n
         *              2 = B  8 = b
         *              3 = R  9 = r
         *              4 = Q 10 = q
         *              5 = K 11 = k
        */
        ulong[,,] table;
        public ulong zobrist_side;
        public ulong[] zobrist_EP;
        public ulong[] zobrist_castle;


        private Dictionary<ulong, string> debugDict = new Dictionary<ulong, string>();

        public ZobristKeys(int files, int ranks)
        {
            Console.WriteLine("Starting Generation of Zobrist keys..");
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            table = new ulong[files, ranks, 12];

            FillTableWithRandomKeys();
            zobrist_side = CreateRandomUlong();
            FillZobristEPWithKeys(files);
            FillZobristCastleWithKeys();

            stopwatch.Stop();
            Console.WriteLine("Finished Generation of Zobrist keys in: " + stopwatch.ToString());
        }

        /*
         * This method creates the zobrist keys. Could be optimized by not creating keys for inactive squares, but time cost is insignificant here
         */
        private void FillTableWithRandomKeys()
        {
            int count = 0;
            for (int i = 0; i < table.GetLength(0); i++)
            {
                for(int j = 0; j < table.GetLength(1); j++)
                {
                    for(int k = 0;  k < table.GetLength(2); k++)
                    {
                        table[i, j, k] = CreateRandomUlong();
                        debugDict.Add(table[i, j, k], $"Piece {k} on i:{i},j:{j}");
                        count++;
                    }
                }
            }

            Console.WriteLine("Created: " + count + " zobrist keys for the Board.");
        }

        public ulong GetZobristKeyFromTable(int file, int rank, char pieceType)
        {
            int pieceId;
            switch (pieceType)
            {
                case 'P':
                    pieceId = 0; break;
                case 'N':
                    pieceId = 1; break;
                case 'B':
                    pieceId = 2; break;
                case 'R':
                    pieceId = 3; break;
                case 'Q':
                    pieceId = 4; break;
                case 'K':
                    pieceId = 5; break;
                case 'p':
                    pieceId = 6; break;
                case 'n':
                    pieceId = 7; break;
                case 'b':
                    pieceId = 8; break;
                case 'r':
                    pieceId = 9; break;
                case 'q':
                    pieceId = 10; break;
                case 'k':
                    pieceId = 11; break;
                default:
                    throw new UnreachableException("Invalid Piece given as argument for zobrist table: " + pieceType);
            }
            return table[file, rank, pieceId];
        }

        /*
         * Each file that has the en passant target square corresponds to each element of the array with the same index (a-file (index 0 in board) is zobrist_EP[0])
         */
        private void FillZobristEPWithKeys(int fileCount)
        {
            zobrist_EP = new ulong[fileCount];
            for (int i = 0; i < zobrist_EP.Length; i++)
            {
                zobrist_EP[i] = CreateRandomUlong();
                debugDict.Add(zobrist_EP[i], $"EP file: {i}");
            }
        }

        /*
         * Each element of zobrist_castle represents the state which the current castle rights can hold. 
         * 15 = KQkq    11 = Kkq    7 = Qkq     3 = kq
         * 14 = KQk     10 = Kk     6 = Qk      2 = k
         * 13 = KQq     9 =  Kq     5 = Qq      1 = q
         * 12 = KQ      8 =  K      4 = Q       0 = -
         */
        private void FillZobristCastleWithKeys()
        {
            zobrist_castle = new ulong[16];
            for(int i = 0; i< zobrist_castle.Length; i++)
            {
                zobrist_castle[i] = CreateRandomUlong();
                debugDict.Add(zobrist_castle[i], $"Castle Mask: {i}");
            }
        }

        private ulong CreateRandomUlong()
        {
            Span<byte> buf = stackalloc byte[8];
            Random.Shared.NextBytes(buf);               
            return BinaryPrimitives.ReadUInt64LittleEndian(buf);
        }

        public void CheckForCorrectZobristKeys(Board board, Move move)
        {
            ulong fresh = board.CreateZobristKeyFromCurrentBoard();
            ulong diff = board.currentZobristKey ^ fresh;   // XOR-Rest
            if (diff == 0)
            {
                return;
            }

            if(debugDict.TryGetValue(diff, out string what))
            {
                throw new InvalidDataException($"Zobrist Hashing Fehler: {what} wurde nicht korrekt getoggled. Current Move: " + move.ToString());
            }
            else
            {
                foreach (var kvp in debugDict)
                {
                    if (debugDict.ContainsKey(diff ^ kvp.Key))
                    {
                        diff = diff ^ kvp.Key;
                        if(debugDict.TryGetValue(diff, out string definition)) { 
                            throw new InvalidDataException($"{kvp.Key + "," + kvp.Value} & {definition} cause the mistake. Current Move: " + move.ToString());
                        }
                        else
                        {
                            throw new UnreachableException("if this code is ever reached, the author of this code is stupid.");
                        }
                    }
                }

                throw new InvalidDataException($"Zobrist Hashing Fehler konnte nicht gefunden werden. diff:  {diff} | Current Move: " + move.ToString());
            }
        }

    }
}
