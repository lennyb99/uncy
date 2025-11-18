using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using uncy.model.boardAlt;

namespace uncy.model.eval
{
    public static class PieceValues
    {
        private static readonly int[] mvv_lva_values = new int[128];


        static PieceValues()
        {
            mvv_lva_values['P'] = 1; mvv_lva_values['p'] = 1;
            mvv_lva_values['N'] = 1; mvv_lva_values['n'] = 1;
            mvv_lva_values['B'] = 1; mvv_lva_values['b'] = 1;
            mvv_lva_values['R'] = 1; mvv_lva_values['r'] = 1;
            mvv_lva_values['Q'] = 1; mvv_lva_values['q'] = 1;

            mvv_lva_values['K'] = 1000; mvv_lva_values['k'] = 1000;
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static int GetValue(byte piece)
        {
            return mvv_lva_values[Piece.GiveCharIdentifier(piece)];
        }
    }
}
