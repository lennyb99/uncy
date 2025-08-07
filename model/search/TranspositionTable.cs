using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using uncy.model.boardAlt;

namespace uncy.model.search
{
    public enum TranspositionTableFlag
    {
        EXACT, LOWERBOUND, UPPERBOUND
    }
    public class TranspositionTable
    {
        private readonly TranspositionTableEntry[] table;

        private readonly ulong indexMask;

        public TranspositionTable(int sizeInMB)
        {
            long targetSizeInBytes = (long)sizeInMB * 1024 * 1024;
            int entrySize; 
            unsafe
            {
            entrySize = sizeof(TranspositionTableEntry);
            }
            
            long idealEntryCount = targetSizeInBytes / entrySize;
            long actualEntryCount = 1;
            while (actualEntryCount * 2 <= idealEntryCount)
            {
                actualEntryCount *= 2;
            }

            this.table = new TranspositionTableEntry[actualEntryCount];
            this.indexMask = (ulong)actualEntryCount - 1;

            Console.WriteLine("-------------");
            Console.WriteLine($"Transposition Table initialized");
            Console.WriteLine($"  - Size of one entry: {entrySize} Bytes");
            Console.WriteLine($"  - Number of possible entries: {actualEntryCount:N0} (2^{Math.Log2(actualEntryCount)})");
            Console.WriteLine($"  - Size of tables: {(actualEntryCount * entrySize) / (1024.0 * 1024.0):F2} MB");
            Console.WriteLine("-------------");
        }

        private int GetIndex(ulong zobristKey)
        {
            return (int)(zobristKey & this.indexMask);
        }

        public void StoreEntry(ulong zkey, int score, int depth, TranspositionTableFlag flag, Move bestMove)
        {
            int index = GetIndex(zkey);
            // Only retrieve a reference of the original struct. should enhance performance.
            ref TranspositionTableEntry existingEntry = ref table[index];

            if (depth >= existingEntry.depth)
            {
                existingEntry.zobristKey = zkey;
                existingEntry.score = score;
                existingEntry.depth = depth;
                existingEntry.flag = flag;
                existingEntry.bestMove = bestMove;
            }
        }

        public bool TryGetEntry(ulong zkey, out TranspositionTableEntry entry)
        {
            int index = GetIndex(zkey);
            TranspositionTableEntry candidate = table[index];
            if(candidate.zobristKey == zkey)
            {
                entry = candidate;
                return true;
            }
            entry = default;
            return false;
        }
    }

    public struct TranspositionTableEntry
    {
        public ulong zobristKey;
        public int score;
        public int depth;
        public TranspositionTableFlag flag;
        public Move bestMove;

        public TranspositionTableEntry(ulong zk, int s, int d, TranspositionTableFlag f, Move bestMove)
        {
            this.zobristKey = zk;
            this.score = s;
            this.depth = d;
            this.flag = f;
            this.bestMove = bestMove;
        }
    }
}
