using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Uncy.Shared.model.board
{
    internal class FenBuilder
    {
        public string CalculateFen(List<byte> pieces)
        {
            Fen fen = buildFen(pieces);

            if (!FenInspector.reviewFen(fen)) return "";

            return fen.completeFEN;
        }

        private Fen buildFen(List<byte> pieces)
        {

            return new Fen();

        }


    }
}
