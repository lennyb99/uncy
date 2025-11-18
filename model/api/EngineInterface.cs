using uncy.model.boardAlt;
using uncy.board;
using uncy.model.search;
using uncy.model.eval;

public static class EngineInterface
{
    /* 
    * This method will take in a fen and then check if the move is legal. It will reply with a new fen of the board after the move.
    */
    public static string IsMoveLegal(string fen, int[] origin, int[] target, byte promotionPiece)
    {
        // Convert string to Fen object
        Fen fenObject = new Fen(fen);
        Board board = new Board(fenObject);

        // Convert to ushort for Move constructor
        ushort originSquare = (ushort)origin[0];
        ushort targetSquare = (ushort)target[0];

        // Get pieces from board
        byte movedPiece = board.board[originSquare];
        byte capturedPiece = board.board[targetSquare];

        // Promotion piece handling
        if (Piece.GetPieceType(promotionPiece) != Piece.Queen &&
            Piece.GetPieceType(promotionPiece) != Piece.Rook &&
             Piece.GetPieceType(promotionPiece) != Piece.Bishop &&
              Piece.GetPieceType(promotionPiece) != Piece.Knight) promotionPiece = Piece.Empty;
        if (promotionPiece != 'e')
        {
            promotionPiece = Piece.GetPieceType(promotionPiece);
            if (Piece.IsColor(movedPiece,Piece.White))
            {
                promotionPiece += Piece.White;
            }
            else
            {
                promotionPiece += Piece.Black;
            }
        }

        // Detect double pawn Push
        bool doublePushPawnMove = false;
        if (Piece.GetPieceType(movedPiece) == Piece.Pawn && Math.Abs(originSquare - targetSquare) == 2*board.dimensionsOfBoard.Item1) doublePushPawnMove = true;

        // Detect Castling move
        bool castlingMove = false;
        if (Piece.GetPieceType(movedPiece) == Piece.King && Math.Abs(originSquare - targetSquare) >= 2) castlingMove = true;

        // Detect En Passant move
        bool enPassantMove = false;

        if (Piece.GetPieceType(movedPiece) == Piece.Pawn &&
            Math.Abs(originSquare - targetSquare) == board.dimensionsOfBoard.Item1+1 && // Checks if en passant square is top right or bottom left of originalSquare
            Math.Abs(originSquare-targetSquare) == board.dimensionsOfBoard.Item1-1 && // Checks for top left or bottom right
            board.enPassantTargetSquare != -1) // true, when diagonal hit by a pawn while en Passant is possible.
        {

            int ePTPiece = board.enPassantTargetSquare;

            if (Piece.IsColor(movedPiece,Piece.White))
            {
                ePTPiece -= board.dimensionsOfBoard.Item1;
            }
            else
            {
                ePTPiece += board.dimensionsOfBoard.Item1;
            }
            if (originSquare == ePTPiece && Math.Abs(targetSquare - ePTPiece) <= board.dimensionsOfBoard.Item1)
            {
                enPassantMove = true;
                if (Piece.IsColor(movedPiece,Piece.White))
                {
                    capturedPiece = Piece.Pawn;
                    capturedPiece += Piece.Black;
                }
                else
                {
                    capturedPiece = Piece.Pawn;
                    capturedPiece += Piece.Black;
                }

            }
        }


        Move move = new Move(originSquare, originSquare, movedPiece, capturedPiece, promotionPiece: promotionPiece, doubleSquarePushFlag: doublePushPawnMove, castlingMoveFlag: castlingMove, enPassantCaptureFlag: enPassantMove);
        Console.WriteLine(move.ToString() + " promotionPiece: " + promotionPiece + " capturedPiece: " + capturedPiece + " doublePushPawnMove: " + doublePushPawnMove + " castlingMove: " + castlingMove + " enPassantMove: " + enPassantMove);
        return board.IsMoveLegal(move);
    }

    /* 
    * This method takes in a fen and then looks for the best move. It will reply with only another fen of the board after the move. 
    */
    public static string FindBestMove(string fen)
    {
        Fen fenObject = new Fen(fen);
        Board board = new Board(fenObject);

        IEvaluator evaluator = new CompositeEvaluator(
            (new MaterialEvaluator(), 100));

        Search search = new Search(evaluator, new TranspositionTable(256));

        Move move = search.FindBestMove(board, 4);
        board.MakeMove(move, out Undo undo);
        fen = board.ToFen();
        board.UnmakeMove(move, undo);
        return fen;
    }
}