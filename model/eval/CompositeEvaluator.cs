using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using uncy.model.boardAlt;

namespace uncy.model.eval
{
    internal class CompositeEvaluator : IEvaluator
    {
        private readonly IReadOnlyList<(IEvaluator eval, int weight)> terms;


        public CompositeEvaluator(params (IEvaluator eval, int weight)[] terms)
        {
            this.terms = terms;
        }


        public int Evaluate(Board board)
        {
            int sum = 0;
            foreach (var (eval, weight) in terms)
            {
                sum += weight * eval.Evaluate(board);
            }
            return sum;

        }
    }
}
