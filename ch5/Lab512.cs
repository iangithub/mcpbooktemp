using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.Orchestration.Concurrent;
using Microsoft.SemanticKernel.Agents.Orchestration.GroupChat;
using Microsoft.SemanticKernel.Agents.Orchestration.Sequential;
using Microsoft.SemanticKernel.ChatCompletion;
using ModelContextProtocol.Client;



public class Lab512
{
    public static async Task Execute()
    {
        Console.WriteLine("Hello, Agent+MCP ! \n\n");

        Kernel kernel = Kernel.CreateBuilder()
                        .AddOpenAIChatCompletion(
                            apiKey: Config.OpenAI_ApiKey,
                            modelId: Config.ModelId)
                        .Build();


        //連線到 MCP Server (stdio模式,指定MCP Server Project Path)
        // var mcpClient = await McpClientFactory.CreateAsync(
        //     new StdioClientTransport(new()
        //     {
        //         Command = "dotnet",
        //         Arguments = ["run", "--project", "../ch5-mcp/McpOrderServer"], // Specify the project path for the MCP Server
        //         Name = "OrderServer"
        //     }));

        //連線到 MCP Server (SSE模式,指定 MCP Server Http URL)
        var mcpClient = await McpClientFactory.CreateAsync(
            new SseClientTransport(new()
            {
                Endpoint = new Uri("https://localhost:7147/sse"), // MCP Server 的 HTTP URL
                Name = "OrderServer"
            }));


        // 取得 MCP Server 工具清單
        var tools = await mcpClient.ListToolsAsync();

        //debug show tools from MCP Server
        Console.WriteLine("MCP Server 工具清單:");
        foreach (var tool in tools)
        {
            Console.WriteLine($"{tool.Name}: {tool.Description}");
        }

        // 匯入工具並組裝 Agent
        kernel.Plugins.AddFromFunctions("McpTools", tools.Select(t => t.AsKernelFunction()));


        // 建立 Agent
        ChatCompletionAgent agent =
            new()
            {
                Name = "SupportAgent",
                Description = "一個可以回答訂單資訊的助手",
                Instructions = @"你是一位專業且有禮貌的助手，負責協助顧客查詢訂單資訊。請依照以下規則提供回覆：
                                1. 如果詢問訂單狀態，請引導提供訂單編號或是顧客姓名，並使用已提供的查詢工具查詢對應資料。
                                2. 回覆時要友善、清楚，避免使用過於技術化的語言。
                                3. 若問題無法直接回答，請告知「我會轉交給專人協助」，並避免捏造答案。
                                4. 僅限提供查詢訂單狀態，若超出職責範圍，請禮貌回覆並引導聯繫客服專線0800-888-888。",
                Kernel = kernel,
                Arguments = new(new PromptExecutionSettings { FunctionChoiceBehavior = FunctionChoiceBehavior.Auto() })
            };

        // 建立對話歷史 thread
        ChatHistoryAgentThread agentThread = new();

        Console.Write("User > ");
        string? userInput;
        while ((userInput = Console.ReadLine()) is not null)
        {
            if (string.IsNullOrWhiteSpace(userInput) || userInput.Equals("exit", StringComparison.OrdinalIgnoreCase))
                break;

            ChatMessageContent message = new(AuthorRole.User, userInput);

            bool isFirst = false;
            await foreach (StreamingChatMessageContent response in agent.InvokeStreamingAsync(message, agentThread))
            {
                if (string.IsNullOrEmpty(response.Content))
                {
                    StreamingFunctionCallUpdateContent? functionCall = response.Items.OfType<StreamingFunctionCallUpdateContent>().SingleOrDefault();

                    //追蹤函數調用
                    if (!string.IsNullOrEmpty(functionCall?.Name))
                    {
                        Console.WriteLine($"\n# trace {response.Role} - {response.AuthorName ?? "*"}: FUNCTION CALL - {functionCall.Name}");
                    }
                    continue;
                }

                if (!isFirst)
                {
                    Console.Write($"{response.Role} - {response.AuthorName ?? "*"} > ");
                    isFirst = true;
                }

                Console.Write($"{response.Content}");
            }
            Console.WriteLine();
            Console.WriteLine($"\n# trace chat thread with agent: {agent.Name} - {agent.Description},threadId: {agentThread.Id} \n");
            Console.Write("User > ");
        }
    }
}
