using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Uncy.Shared.boardAlt;

namespace Uncy.Shared.eval
{
    public interface IEvaluator
    {

        int Evaluate(Board board);
    }
}
