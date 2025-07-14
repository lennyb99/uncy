using Grpc.Core;
using Microsoft.Extensions.Logging;
using Uncy.Model.Api;

namespace Uncy.Model.Api
{
    public class ChessEngineGrpcService : ChessEngineService.ChessEngineServiceBase
    {
        private readonly ILogger<ChessEngineGrpcService> _logger;

        public ChessEngineGrpcService(ILogger<ChessEngineGrpcService> logger)
        {
            _logger = logger;
        }

        public override Task<PingResponse> Ping(PingRequest request, ServerCallContext context)
        {
            Console.WriteLine("=== PING REQUEST RECEIVED ===");
            Console.WriteLine($"Client IP: {context.Peer}");
            Console.WriteLine($"Received message: '{request.Message}'");

            _logger.LogInformation($"Received ping: {request.Message}");

            var response = new PingResponse
            {
                Message = $"Pong: {request.Message}",
                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            };

            Console.WriteLine($"Sending response: '{response.Message}' with timestamp: {response.Timestamp}");
            Console.WriteLine("=== PING RESPONSE SENT ===");
            Console.WriteLine();

            return Task.FromResult(response);
        }

        public override Task<EngineStatusResponse> GetEngineStatus(EngineStatusRequest request, ServerCallContext context)
        {
            Console.WriteLine("=== ENGINE STATUS REQUEST RECEIVED ===");
            Console.WriteLine($"Client IP: {context.Peer}");
            Console.WriteLine("Request: GetEngineStatus");

            _logger.LogInformation("Engine status requested");

            var response = new EngineStatusResponse
            {
                EngineName = "Uncy Chess Engine",
                Version = "1.0.0",
                IsReady = true
            };

            Console.WriteLine($"Sending response:");
            Console.WriteLine($"  Engine Name: {response.EngineName}");
            Console.WriteLine($"  Version: {response.Version}");
            Console.WriteLine($"  Is Ready: {response.IsReady}");
            Console.WriteLine("=== ENGINE STATUS RESPONSE SENT ===");
            Console.WriteLine();

            return Task.FromResult(response);
        }

        public override Task<IsMoveLegalResponse> IsMoveLegal(IsMoveLegalRequest request, ServerCallContext context)
        {
            Console.WriteLine("=== MOVE LEGALITY REQUEST RECEIVED ===");
            Console.WriteLine($"Client IP: {context.Peer}");
            Console.WriteLine($"Request parameters:");
            Console.WriteLine($"  FEN: {request.Fen}");
            Console.WriteLine($"  Origin: ({request.OriginFile},{request.OriginRank})");
            Console.WriteLine($"  Target: ({request.TargetFile},{request.TargetRank})");
            Console.WriteLine($"  Promotion Piece: '{request.PromotionPiece}'");
            Console.WriteLine($"Processing move...");

            _logger.LogInformation($"Move legality check requested: {request.Fen} from ({request.OriginFile},{request.OriginRank}) to ({request.TargetFile},{request.TargetRank}) with promotion: '{request.PromotionPiece}'");

            try
            {
                // Create coordinate arrays for origin and target
                int[] origin = { request.OriginFile, request.OriginRank };
                int[] target = { request.TargetFile, request.TargetRank };

                // Get promotion piece (default to 'e' if not provided or invalid)
                char promotionPiece = string.IsNullOrEmpty(request.PromotionPiece) ? 'e' : request.PromotionPiece[0];

                Console.WriteLine($"Calling engine interface with coordinates: origin[{origin[0]},{origin[1]}] -> target[{target[0]},{target[1]}] with promotion: '{promotionPiece}'");

                // Call the engine interface method directly with FEN string and promotion piece
                string resultingFen = EngineInterface.IsMoveLegal(request.Fen, origin, target, promotionPiece);

                Console.WriteLine($"Engine returned FEN: '{resultingFen}'");

                // Check if the move was legal (assuming non-empty result means legal)
                bool isLegal = !string.IsNullOrEmpty(resultingFen);

                var response = new IsMoveLegalResponse
                {
                    IsLegal = isLegal,
                    ResultingFen = isLegal ? resultingFen : "",
                    ErrorMessage = isLegal ? "" : "Move is not legal"
                };

                Console.WriteLine($"Sending response:");
                Console.WriteLine($"  Is Legal: {response.IsLegal}");
                Console.WriteLine($"  Resulting FEN: '{response.ResultingFen}'");
                Console.WriteLine($"  Error Message: '{response.ErrorMessage}'");
                Console.WriteLine("=== MOVE LEGALITY RESPONSE SENT ===");
                Console.WriteLine();

                _logger.LogInformation($"Move legality result: {isLegal}, Resulting FEN: {resultingFen}");
                return Task.FromResult(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ ERROR in IsMoveLegal:");
                Console.WriteLine($"  Exception: {ex.GetType().Name}");
                Console.WriteLine($"  Message: {ex.Message}");
                Console.WriteLine($"  StackTrace: {ex.StackTrace}");

                _logger.LogError(ex, "Error checking move legality");

                var errorResponse = new IsMoveLegalResponse
                {
                    IsLegal = false,
                    ResultingFen = "",
                    ErrorMessage = $"Error processing move: {ex.Message}"
                };

                Console.WriteLine($"Sending error response:");
                Console.WriteLine($"  Is Legal: {errorResponse.IsLegal}");
                Console.WriteLine($"  Error Message: '{errorResponse.ErrorMessage}'");
                Console.WriteLine("=== MOVE LEGALITY ERROR RESPONSE SENT ===");
                Console.WriteLine();

                return Task.FromResult(errorResponse);
            }
        }

        public override Task<FindBestMoveResponse> FindBestMove(FindBestMoveRequest request, ServerCallContext context)
        {
            Console.WriteLine("=== FIND BEST MOVE REQUEST RECEIVED ===");
            Console.WriteLine($"Client IP: {context.Peer}");
            Console.WriteLine($"Request parameters:");
            Console.WriteLine($"  FEN: {request.Fen}");
            Console.WriteLine($"Processing best move search...");

            _logger.LogInformation($"Best move search requested for position: {request.Fen}");

            try
            {
                Console.WriteLine($"Calling engine interface with FEN: '{request.Fen}'");

                // Call the engine interface method directly with FEN string
                string resultingFen = EngineInterface.FindBestMove(request.Fen);

                Console.WriteLine($"Engine returned FEN: '{resultingFen}'");

                // Check if a best move was found (assuming non-empty and different result means success)
                bool success = !string.IsNullOrEmpty(resultingFen) && resultingFen != request.Fen;

                var response = new FindBestMoveResponse
                {
                    Success = success,
                    ResultingFen = success ? resultingFen : "",
                    ErrorMessage = success ? "" : "No valid move found or engine returned same position"
                };

                Console.WriteLine($"Sending response:");
                Console.WriteLine($"  Success: {response.Success}");
                Console.WriteLine($"  Resulting FEN: '{response.ResultingFen}'");
                Console.WriteLine($"  Error Message: '{response.ErrorMessage}'");
                Console.WriteLine("=== FIND BEST MOVE RESPONSE SENT ===");
                Console.WriteLine();

                _logger.LogInformation($"Best move search result: {success}, Resulting FEN: {resultingFen}");
                return Task.FromResult(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ ERROR in FindBestMove:");
                Console.WriteLine($"  Exception: {ex.GetType().Name}");
                Console.WriteLine($"  Message: {ex.Message}");
                Console.WriteLine($"  StackTrace: {ex.StackTrace}");

                _logger.LogError(ex, "Error finding best move");

                var errorResponse = new FindBestMoveResponse
                {
                    Success = false,
                    ResultingFen = "",
                    ErrorMessage = $"Error processing best move search: {ex.Message}"
                };

                Console.WriteLine($"Sending error response:");
                Console.WriteLine($"  Success: {errorResponse.Success}");
                Console.WriteLine($"  Error Message: '{errorResponse.ErrorMessage}'");
                Console.WriteLine("=== FIND BEST MOVE ERROR RESPONSE SENT ===");
                Console.WriteLine();

                return Task.FromResult(errorResponse);
            }
        }
    }
}