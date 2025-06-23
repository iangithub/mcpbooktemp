using System.Net;
using Azure.AI.Agents.Persistent;
using Azure.Identity;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents.AzureAI;
using Microsoft.SemanticKernel.ChatCompletion;

public static class Lab57
{
    // dotnet add package Microsoft.SemanticKernel.Agents.AzureAI --version 1.57.0-preview
    // dotnet add package Azure.Identity
    // use entra ID to authenticate, az login --tenant <tenant-id>
    public static async Task Execution()
    {
        Console.WriteLine("Hello, Azure AI Agent! \n\n");

        PersistentAgentsClient client = AzureAIAgent.CreateAgentsClient(Config.Azure_AI_Foundry_EndPoint, new AzureCliCredential());
        // 註冊商務助理服務Plugin
        // 這個 Plugin 將會被 Azure AI Agent 使用
        KernelPlugin plugin = KernelPluginFactory.CreateFromType<BusinessAssistantService>();

        // 建立全新的 Azure AI Agent (一旦建立後，會自動部署到 Azure AI Foundry，不需要每次都重新部署)
        // await CreatingAnAzureAIAgent(client, plugin);
        // return;

        // 根據 ID 取得已存在的 Azure AI Agent
        PersistentAgent definition = await client.Administration.GetAgentAsync("asst_nplGgHzAc1Z3sam0n9kps6u7");
        AzureAIAgent agent = new(definition, client);
        // Azure foundry 中的 Agent 並不會host Plugin，所以需要每次啟動時重新附掛 Plugin。
        agent.Kernel.Plugins.Add(plugin);
        AzureAIAgentThread agentThread = new(agent.Client);


        // 使用 Azure AI Agent 進行對話
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

    private static async Task CreatingAnAzureAIAgent(PersistentAgentsClient client, KernelPlugin plugin)
    {
        // 定義商務助理 AI 的指令與說明
        // 這些指令將會被 Azure AI Agent 使用
        string instructions = @"你是一位專業的商務助理 AI，負責協助查詢以下資訊：
1. 根據客戶名稱，查詢客戶的帳戶資訊與合約狀態。
2. 根據業務代表姓名，查詢對應業務代表的聯絡資訊。

請遵守以下原則：
- 回覆時要簡潔清楚、準確不誤導。
- 當使用者輸入不明確或資訊不足時，請主動詢問更多細節。
- 絕對不能憑空編造任何資訊（例如：合約狀態、聯絡方式）。
- 一律透過內部資料庫或指定 API 工具來取得資料，不要自行假設。
- 如果查無資料，請禮貌地說明找不到相關資訊。

若使用者的問題超出你的職責範圍，請清楚說明你能處理的類型，並建議他重新描述需求或洽詢相關單位。
";

        // 建立 PersistentAgentsClient 用於 Azure AI Agents
        PersistentAgent definition = await client.Administration.CreateAgentAsync(
            Config.Azure_ModelId,
            name: "Business Assistant",
            description: "這是一位專業的商務助理 AI，專門協助內部人員快速查詢客戶帳戶資訊、合約狀態，以及對應業務聯絡方式。",
            instructions: instructions);

        //建立一個新的 Azure AI Agent
        AzureAIAgent agent = new(definition, client)
        {
            Kernel = Kernel.CreateBuilder()
            .AddAzureOpenAIChatCompletion(
                Config.Azure_ModelId,
                Config.Azure_AI_Foundry_EndPoint,
                new AzureCliCredential()).Build()
        };
        agent.Kernel.Plugins.Add(plugin);
        Console.WriteLine("Azure AI Agent 已建立，請在 Azure AI Foundry 中查看詳細資訊。");
    }
}