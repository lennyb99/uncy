using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using uncy.model.boardAlt;

namespace uncy.model.eval
{
    public interface IEvaluator
    {

        int Evaluate(Board board);
    }
}
