using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;
public static class Lab55
{
    public static async Task Execution()
    {
        Kernel kernel = Kernel.CreateBuilder()
                            .AddOpenAIChatCompletion(
                                apiKey: Config.OpenAI_ApiKey,
                                modelId: Config.ModelId)
                            .Build();

        // 註冊工具
        kernel.Plugins.AddFromType<CustomerSupportService>();

        // 建立 Agent
        ChatCompletionAgent agent =
            new()
            {
                Name = "SupportAgent",
                Description = "一個可以回答訂單狀態與產品資訊的AI客服",
                Instructions = @"你是一位專業且有禮貌的 AI 客服專員，負責協助顧客查詢訂單狀態與產品資訊。請依照以下規則提供回覆：
                                1. 如果顧客詢問訂單狀態，請引導顧客提供訂單編號，並使用已提供的查詢工具（如：GetOrderStatus）查詢對應資料。
                                2. 如果顧客詢問產品資訊，請根據產品名稱，並使用產品查詢工具（如：GetProductInfo）提供詳細資訊。
                                3. 回覆時要友善、清楚，避免使用過於技術化的語言。
                                4. 若問題無法直接回答，請告知顧客「我會轉交給專人協助」，並避免捏造答案。
                                5. 僅限提供查詢訂單狀態與產品資訊，若超出職責範圍，請禮貌回覆並引導顧客聯繫客服專線0800-888-888。",
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