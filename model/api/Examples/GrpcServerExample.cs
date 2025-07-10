using Microsoft.Extensions.Logging;
using Uncy.Model.Api;

namespace Uncy.Model.Api.Examples
{
    /// <summary>
    /// Example demonstrating how to integrate the gRPC server into the chess engine application
    /// </summary>
    public class GrpcServerExample
    {
        private GrpcServerHost? _grpcServer;
        private readonly ILogger<GrpcServerExample> _logger;

        public GrpcServerExample()
        {
            // Create a simple console logger for demonstration
            using var loggerFactory = LoggerFactory.Create(builder =>
                builder.AddConsole().SetMinimumLevel(LogLevel.Information));
            _logger = loggerFactory.CreateLogger<GrpcServerExample>();
        }

        /// <summary>
        /// Starts the gRPC server on the specified port
        /// </summary>
        /// <param name="port">Port to listen on (default: 5001)</param>
        public async Task StartServerAsync(int port = 5001)
        {
            try
            {
                _grpcServer = new GrpcServerHost(port);
                await _grpcServer.StartAsync();

                _logger.LogInformation("gRPC server is now running and ready to accept connections");
                _logger.LogInformation($"You can test the server using a gRPC client connecting to http://localhost:{port}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to start gRPC server");
                throw;
            }
        }

        /// <summary>
        /// Stops the gRPC server
        /// </summary>
        public async Task StopServerAsync()
        {
            if (_grpcServer != null)
            {
                await _grpcServer.StopAsync();
                _grpcServer = null;
                _logger.LogInformation("gRPC server stopped");
            }
        }

        /// <summary>
        /// Example of how to integrate this into your main application loop
        /// Call this method from your main form or application entry point
        /// </summary>
        public async Task RunServerInBackgroundAsync()
        {
            // Start the server in background
            await StartServerAsync();

            // Your application continues to run normally
            // The gRPC server will handle incoming requests in parallel

            // To stop the server when your application exits:
            AppDomain.CurrentDomain.ProcessExit += async (sender, e) =>
            {
                await StopServerAsync();
            };
        }

        /// <summary>
        /// Checks if the gRPC server is currently running
        /// </summary>
        public bool IsServerRunning => _grpcServer?.IsRunning ?? false;
    }
}