using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol.Transport;
using OpenAI;
using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

using var tracerProvider = Sdk.CreateTracerProviderBuilder()
    .AddHttpClientInstrumentation()
    .AddSource("*")
    .AddOtlpExporter()
    .Build();
using var metricsProvider = Sdk.CreateMeterProviderBuilder()
    .AddHttpClientInstrumentation()
    .AddMeter("*")
    .AddOtlpExporter()
    .Build();
using var loggerFactory = LoggerFactory.Create(builder =>
    builder.AddOpenTelemetry(opt => opt.AddOtlpExporter())
);

// Connect to an MCP server
Console.WriteLine("Connecting client to MCP 'ContosoMcp' server");

// Create OpenAI client (or any other compatible with IChatClient)
// Provide your own OPENAI_API_KEY via an environment variable.
var openAIClient = new OpenAIClient(
    Environment.GetEnvironmentVariable("OPENAI_API_KEY")
).GetChatClient("gpt-4o-mini");

// Create a sampling client.
using IChatClient samplingClient = openAIClient
    .AsIChatClient()
    .AsBuilder()
    .UseOpenTelemetry(loggerFactory: loggerFactory, configure: o => o.EnableSensitiveData = true)
    .Build();

// var mcpClient = await McpClientFactory.CreateAsync(
//     new StdioClientTransport(
//         new()
//         {
//             Command = "dotnet",
//             Arguments =
//             [
//                 "run",
//                 "--project",
//                 "C:\\Users\\donal\\src\\ms\\ContosoMcp\\src\\ContosoMcpServer\\ContosoMcpServer.csproj",
//             ],
//             Name = "ContosoMcpServer",
//         }
//     ),
//     clientOptions: new()
//     {
//         Capabilities = new()
//         {
//             Sampling = new() { SamplingHandler = samplingClient.CreateSamplingHandler() },
//         },
//     },
//     loggerFactory: loggerFactory
// );

var targetServerName = "ContosoMcpServer";
IMcpClient mcpClient = null;

ModelContextProtocolServerCatalog McpCatalog = ActionRuntimeFactory.CreateMCPCatalog();
ModelContextProtocolClientContext clientContext = McpCatalog.CreateClientContext();

// Loop through WMCP servers and being Client/Server Transports.
foreach (ModelContextProtocolServerInfo serverInfo in McpCatalog.GetServerInfos())
{
    Console.Write($"Found server {serverInfo.Name}...");

    // Check if the server is the one we want to connect to.
    if (serverInfo.Name != targetServerName)
    {
        Console.WriteLine($"skipping {serverInfo.Name}");
        continue;
    }


    // Invoke the proxy server to get launch arguments for server.
    IModelContextProtocolServer info = McpCatalog.ActivateServer(serverInfo.Id, clientContext);

    StdioClientTransportOptions transportOptions = new()
    {
        Name = serverInfo.Name,
        Command = info.Command,
        Arguments = info.GetCommandArguments()
    };

    StdioClientTransport clientTransport = new(transportOptions);
    McpClientOptions? clientOptions = null;

    // Connect to the WMCP server to begin MCP communication.
    mcpClient = await McpClientFactory.CreateAsync(clientTransport, clientOptions);
    Console.WriteLine($"connected!");
}
if (mcpClient == null)
{
    Console.WriteLine("Failed to connect to MCP server.");
    return;
}

// Get all available tools
Console.WriteLine("Tools available:");
var tools = await mcpClient.ListToolsAsync();
foreach (var tool in tools)
{
    Console.WriteLine($"  {tool}");
}

Console.WriteLine();

// Create an IChatClient that can use the tools.
using IChatClient chatClient = openAIClient
    .AsIChatClient()
    .AsBuilder()
    .UseFunctionInvocation()
    .UseOpenTelemetry(loggerFactory: loggerFactory, configure: o => o.EnableSensitiveData = true)
    .Build();

// Have a conversation, making all tools available to the LLM.
List<ChatMessage> messages = [];
while (true)
{
    Console.Write("Q: ");
    messages.Add(new(ChatRole.User, Console.ReadLine()));

    List<ChatResponseUpdate> updates = [];
    await foreach (
        var update in chatClient.GetStreamingResponseAsync(messages, new() { Tools = [.. tools] })
    )
    {
        Console.Write(update);
        updates.Add(update);
    }
    Console.WriteLine();

    messages.AddMessages(updates);
}
