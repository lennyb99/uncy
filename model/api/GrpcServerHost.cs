using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Uncy.Model.Api;

namespace Uncy.Model.Api
{
    public class GrpcServerHost
    {
        private WebApplication? _app;
        private readonly int _port;
        private readonly ILogger<GrpcServerHost> _logger;
        private CancellationTokenSource? _cancellationTokenSource;

        public GrpcServerHost(int port = 5001, ILogger<GrpcServerHost>? logger = null)
        {
            _port = port;
            _logger = logger ?? CreateDefaultLogger();
        }

        public async Task StartAsync()
        {
            try
            {
                var builder = WebApplication.CreateBuilder();

                // Configure Kestrel to listen on HTTP/2
                builder.WebHost.ConfigureKestrel(options =>
                {
                    options.ListenLocalhost(_port, listenOptions =>
                    {
                        listenOptions.Protocols = HttpProtocols.Http2;
                    });
                });

                // Add services
                builder.Services.AddGrpc();
                builder.Services.AddLogging();

                _app = builder.Build();

                // Configure the HTTP request pipeline
                _app.MapGrpcService<ChessEngineGrpcService>();

                // Optional: Add gRPC reflection for testing
                _app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client.");

                _cancellationTokenSource = new CancellationTokenSource();

                Console.WriteLine($"🚀 Starting gRPC server on port {_port}...");
                _logger.LogInformation($"Starting gRPC server on port {_port}");
                await _app.StartAsync(_cancellationTokenSource.Token);
                Console.WriteLine($"✅ gRPC server started successfully!");
                Console.WriteLine($"📡 Listening on: http://localhost:{_port}");
                Console.WriteLine($"🔧 Available services:");
                Console.WriteLine($"   - ChessEngineService/Ping");
                Console.WriteLine($"   - ChessEngineService/GetEngineStatus");
                Console.WriteLine($"   - ChessEngineService/IsMoveLegal");
                Console.WriteLine($"🎯 Ready to accept gRPC connections...");
                Console.WriteLine();
                _logger.LogInformation($"gRPC server started successfully on http://localhost:{_port}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Failed to start gRPC server!");
                Console.WriteLine($"   Error: {ex.Message}");
                _logger.LogError(ex, "Failed to start gRPC server");
                throw;
            }
        }

        public async Task StopAsync()
        {
            try
            {
                if (_app != null)
                {
                    Console.WriteLine("🛑 Stopping gRPC server...");
                    _logger.LogInformation("Stopping gRPC server");
                    _cancellationTokenSource?.Cancel();
                    await _app.StopAsync();
                    await _app.DisposeAsync();
                    Console.WriteLine("✅ gRPC server stopped successfully");
                    _logger.LogInformation("gRPC server stopped successfully");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error stopping gRPC server");
                throw;
            }
            finally
            {
                _cancellationTokenSource?.Dispose();
            }
        }

        public bool IsRunning => _app != null;

        private static ILogger<GrpcServerHost> CreateDefaultLogger()
        {
            using var loggerFactory = LoggerFactory.Create(builder =>
                builder.AddConsole().SetMinimumLevel(LogLevel.Information));
            return loggerFactory.CreateLogger<GrpcServerHost>();
        }
    }
}