using uncy.model.boardAlt;
using uncy.model.board;
using uncy.board;


public static class EngineInterface
{
    /* 
    * This method will take in a fen and then check if the move is legal. It will reply with a new fen of the board after the move.
    */
    public static string IsMoveLegal(string fen, int[] origin, int[] target, char promotionPiece)
    {
        // Convert string to Fen object
        Fen fenObject = new Fen(fen);
        Board board = new Board(fenObject);

        // Convert int coordinates to byte for Move constructor
        byte originFile = (byte)origin[0];
        byte originRank = (byte)origin[1];
        byte targetFile = (byte)target[0];
        byte targetRank = (byte)target[1];

        // Get pieces from board
        char movedPiece = board.board[originFile, originRank];
        char capturedPiece = board.board[targetFile, targetRank];

        // Promotion piece handling
        if (char.ToLower(promotionPiece) != 'q' &&
            char.ToLower(promotionPiece) != 'r' &&
             char.ToLower(promotionPiece) != 'b' &&
              char.ToLower(promotionPiece) != 'n') promotionPiece = 'e';

        // Detect double pawn Push
        bool doublePushPawnMove = false;
        if (char.ToLower(movedPiece) == 'p' && Math.Abs(originRank - targetRank) == 2) doublePushPawnMove = true;

        // Detect Castling move
        bool castlingMove = false;
        if (char.ToLower(movedPiece) == 'k' && Math.Abs(originFile - targetFile) >= 2) castlingMove = true;

        // Detect En Passant move
        bool enPassantMove = false;
        if (char.ToLower(movedPiece) == 'p' &&
            Math.Abs(originFile - targetFile) == 1 &&
            Math.Abs(originRank - targetRank) == 1 &&
            board.enPassantTargetSquare != (-1, -1)) // true, when diagonal hit by a pawn while en Passant is possible
        {
            (int, int) ePTSquare = board.enPassantTargetSquare;
            if (targetFile == ePTSquare.Item1 && Math.Abs(targetRank - ePTSquare.Item2) == 1)
            {
                enPassantMove = true;
                if (char.IsUpper(movedPiece)) {
                    capturedPiece = 'p';
                } else {
                    capturedPiece = 'P';
                }
                    
            }
        }


        Move move = new Move(originFile, originRank, targetFile, targetRank, movedPiece, capturedPiece,promotionPiece:promotionPiece, doubleSquarePushFlag:doublePushPawnMove, castlingMoveFlag:castlingMove, enPassantCaptureFlag:enPassantMove);

        return board.IsMoveLegal(move);
    }

    /* 
    * This method takes in a fen and then looks for the best move. It will reply with only another fen of the board after the move. 
    */
    public static string GetBestMove(string fen)
    {
        // Convert string to Fen object
        Fen fenObject = new Fen(fen);
        Board board = new Board(fenObject);

        // TODO: Implement best move logic
        return fen; // Placeholder - return original FEN for now
    }
}