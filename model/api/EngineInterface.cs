using uncy.model.boardAlt;
using uncy.model.board;
using uncy.board;


public static class EngineInterface
{
    /* 
    * This method will take in a fen and then check if the move is legal. It will reply with a new fen of the board after the move.
    */
    public static string IsMoveLegal(string fen, int[] origin, int[] target)
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

        Move move = new Move(originFile, originRank, targetFile, targetRank, movedPiece, capturedPiece);

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