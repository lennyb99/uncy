using Uncy.board;
using Uncy.Shared.boardAlt;
using Uncy.Shared.eval;
using Uncy.Shared.search;

namespace Uncy.Shared.core { 
    public class Engine
    {
        private Board board;
        private Search search;

        private TranspositionTable transpositionTable;

        private IEvaluator evaluator;



        public Engine()
        {
            this.transpositionTable = new TranspositionTable(256);
            this.evaluator = new CompositeEvaluator(
                (new MaterialEvaluator(), 100));

            this.search = new Search(evaluator, transpositionTable);

            this.board = new Board(new Fen("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1"));

        
        }

        public void SetupBoard(Fen fen)
        {
            board = new Board(fen);
        }

        public void InitializeEngine()
        {
            this.transpositionTable = new TranspositionTable(256);
            this.evaluator = new CompositeEvaluator(
                (new MaterialEvaluator(), 100));

            this.search = new Search(evaluator, transpositionTable);
        }

        public string StartSearch(int depth)
        {
            Move move = search.FindBestMove(board, depth);

            Console.WriteLine(move.ToString());
            board.MakeMove(move, out Undo undo);
            board.PrintBoardToConsole();

            return "";
        }


    }


}