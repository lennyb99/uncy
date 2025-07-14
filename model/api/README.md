# gRPC API für Uncy Chess Engine

Diese Implementierung stellt eine grundlegende gRPC-Server-Infrastruktur für die Uncy Chess Engine bereit.

## Überblick

Die gRPC-API ermöglicht es externen Anwendungen, mit der Chess Engine über das gRPC-Protokoll zu kommunizieren. Die Engine fungiert als **gRPC-Server**, während dein ASP.NET Core Backend als **Client** eine Verbindung herstellen kann.

## Komponenten

### 1. Proto-Definition (`Protos/chess_engine.proto`)

- Definiert die verfügbaren gRPC-Services und Message-Typen
- Enthält grundlegende Methoden für Ping und Engine-Status

### 2. Service-Implementierung (`ChessEngineGrpcService.cs`)

- Implementiert die in der Proto-Datei definierten Services
- Stellt aktuell vier Methoden bereit:
  - `Ping`: Für einfache Konnektivitätstests
  - `GetEngineStatus`: Gibt grundlegende Engine-Informationen zurück
  - `IsMoveLegal`: Prüft ob ein Zug legal ist und gibt den resultierenden FEN zurück
  - `FindBestMove`: Findet den besten Zug für eine gegebene Position und gibt den resultierenden FEN zurück

### 3. Server-Host (`GrpcServerHost.cs`)

- Verwaltet den gRPC-Server
- Konfiguriert Kestrel für HTTP/2 (erforderlich für gRPC)
- Läuft standardmäßig auf Port 5001

### 4. Beispiel-Integration (`Examples/GrpcServerExample.cs`)

- Zeigt, wie der gRPC-Server in die bestehende Anwendung integriert werden kann

## Verwendung

### Server starten

```csharp
// Einfache Verwendung
var example = new GrpcServerExample();
await example.StartServerAsync(5001); // Startet auf Port 5001

// Oder für Integration in deine Anwendung
var grpcServer = new GrpcServerHost(5001);
await grpcServer.StartAsync();
```

### Integration in die bestehende Anwendung

Du kannst den gRPC-Server in deiner `Program.cs` oder `Form1.cs` starten:

```csharp
// In deiner Main-Methode oder Form-Konstruktor
var grpcExample = new GrpcServerExample();
await grpcExample.RunServerInBackgroundAsync();
```

## Testen der Verbindung

### Mit gRPC-Client-Tools

Du kannst die Verbindung testen mit:

- **grpcurl** (Kommandozeilen-Tool)
- **Postman** (unterstützt gRPC)
- **BloomRPC** (gRPC-GUI-Client)

Beispiel mit grpcurl:

```bash
# Ping-Test
grpcurl -plaintext -d '{"message": "Hello"}' localhost:5001 chessengine.ChessEngineService/Ping

# Engine-Status
grpcurl -plaintext localhost:5001 chessengine.ChessEngineService/GetEngineStatus

# Zug-Legalität prüfen (Beispiel: e2 nach e4)
grpcurl -plaintext -d '{"fen": "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1", "origin_file": 4, "origin_rank": 1, "target_file": 4, "target_rank": 3, "promotion_piece": "e"}' localhost:5001 chessengine.ChessEngineService/IsMoveLegal

# Bauern-Promotion prüfen (Beispiel: a7 nach a8 mit Dame-Promotion)
grpcurl -plaintext -d '{"fen": "8/P7/8/8/8/8/8/8 w - - 0 1", "origin_file": 0, "origin_rank": 6, "target_file": 0, "target_rank": 7, "promotion_piece": "q"}' localhost:5001 chessengine.ChessEngineService/IsMoveLegal

# Besten Zug finden (Beispiel: Startposition)
grpcurl -plaintext -d '{"fen": "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1"}' localhost:5001 chessengine.ChessEngineService/FindBestMove
```

### Von deinem ASP.NET Core Backend

In deinem Backend kannst du einen gRPC-Client erstellen:

```csharp
// NuGet: Grpc.Net.Client, Google.Protobuf, Grpc.Tools
var channel = GrpcChannel.ForAddress("http://localhost:5001");
var client = new ChessEngineService.ChessEngineServiceClient(channel);

// Ping-Test
var pingResponse = await client.PingAsync(new PingRequest { Message = "Test" });

// Engine-Status abrufen
var statusResponse = await client.GetEngineStatusAsync(new EngineStatusRequest());

// Zug-Legalität prüfen (normaler Zug)
var moveRequest = new IsMoveLegalRequest
{
    Fen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1",
    OriginFile = 4,  // e-Spalte (0-basiert)
    OriginRank = 1,  // 2. Reihe (0-basiert)
    TargetFile = 4,  // e-Spalte (0-basiert)
    TargetRank = 3,  // 4. Reihe (0-basiert)
    PromotionPiece = "e"  // 'e' für keine Promotion
};

var moveResponse = await client.IsMoveLegalAsync(moveRequest);
if (moveResponse.IsLegal)
{
    Console.WriteLine($"Zug ist legal! Neuer FEN: {moveResponse.ResultingFen}");
}
else
{
    Console.WriteLine($"Zug ist illegal: {moveResponse.ErrorMessage}");
}

// Bauern-Promotion prüfen
var promotionRequest = new IsMoveLegalRequest
{
    Fen = "8/P7/8/8/8/8/8/8 w - - 0 1",
    OriginFile = 0,  // a-Spalte (0-basiert)
    OriginRank = 6,  // 7. Reihe (0-basiert)
    TargetFile = 0,  // a-Spalte (0-basiert)
    TargetRank = 7,  // 8. Reihe (0-basiert)
    PromotionPiece = "q"  // Dame-Promotion
};

var promotionResponse = await client.IsMoveLegalAsync(promotionRequest);
if (promotionResponse.IsLegal)
{
    Console.WriteLine($"Promotion ist legal! Neuer FEN: {promotionResponse.ResultingFen}");
}

// Besten Zug finden
var bestMoveRequest = new FindBestMoveRequest
{
    Fen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1"  // Startposition
};

var bestMoveResponse = await client.FindBestMoveAsync(bestMoveRequest);
if (bestMoveResponse.Success)
{
    Console.WriteLine($"Bester Zug gefunden! Neuer FEN: {bestMoveResponse.ResultingFen}");
}
else
{
    Console.WriteLine($"Kein bester Zug gefunden: {bestMoveResponse.ErrorMessage}");
}
```

## Nächste Schritte

Diese Implementierung stellt die Grundlage bereit. Du kannst:

1. **Weitere Services hinzufügen**: Erweitere die Proto-Datei um spezifische Chess-Engine-Funktionen
2. **Authentifizierung implementieren**: Füge Sicherheitsmaßnahmen hinzu
3. **Streaming unterstützen**: Für längere Berechnungen oder kontinuierliche Updates
4. **Error Handling erweitern**: Robustere Fehlerbehandlung

## Parameter-Erklärung

### Koordinatensystem:

- **File/Rank sind 0-basiert** (a1 = 0,0)
- **Origin**: Startposition des Zugs
- **Target**: Zielposition des Zugs

### Promotion-Piece-Parameter:

- **'q'**: Dame-Promotion
- **'r'**: Turm-Promotion
- **'b'**: Läufer-Promotion
- **'n'**: Springer-Promotion
- **'e'**: Keine Promotion (für normale Züge)

## Portkonfiguration

- **Standard-Port**: 5001 (HTTP/2)
- **Anpassbar**: Über den Constructor-Parameter des `GrpcServerHost`
- **Firewall**: Stelle sicher, dass der Port für dein Backend erreichbar ist

Die gRPC-Verbindung ist jetzt einsatzbereit für erste Tests!
