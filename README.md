# Contoso Agent

ContosoAgent is a console-based AI agent that leverages the Model Context Protocol (MCP) and OpenAI's Large Language Models (LLMs) to interact with the Contoso product service. This project demonstrates how to integrate advanced AI capabilities with a robust protocol for tool invocation and data exchange.

## Features

- **Model Context Protocol (MCP):** Enables seamless communication with the Contoso product service.
- **OpenAI Integration:** Utilizes OpenAI's GPT models for natural language understanding and response generation.
- **Tool Invocation:** Supports a variety of tools for enhanced functionality, such as sampling, long-running operations, and resource management.
- **OpenTelemetry:** Provides observability with tracing, metrics, and logging.

## Prerequisites

- .NET SDK 9.0 or later
- An OpenAI API key (set as the `OPENAI_API_KEY` environment variable)
- Node.js (for running the MCP server)

## Getting Started

### Creating the Solution

```pwsh
dotnet new sln -n ContosoAgent
```

### Creating the Project

```pwsh
dotnet new console -n src/ContosoAgent
```

### Adding the Project to the Solution

```pwsh
dotnet sln ContosoAgent.sln add src/ContosoAgent/ContosoAgent.csproj
```

### Installing Dependencies

```pwsh
dotnet add src/ContosoAgent/ContosoAgent.csproj package Microsoft.Extensions.Logging
```

### Running the MCP Server

The MCP server is started automatically by the ContosoAgent application. There is no need to manually start the server. Simply run the ContosoAgent project, and it will handle the server initialization programmatically.

### Building the Solution

```pwsh
dotnet build ContosoAgent.sln
```

### Running the Project

```pwsh
dotnet run --project src/ContosoAgent/ContosoAgent.csproj
```

## Usage

1. Run the ContosoAgent project.
2. Interact with the AI agent by typing your queries into the console.

## Tools Available

The following tools are available for use by the AI agent:

- `search_products`
- `get_product`

## Observability

The project uses OpenTelemetry for tracing, metrics, and logging. Ensure that the OpenTelemetry exporters are properly configured to view observability data.

## License

This project is licensed under the MIT License.
