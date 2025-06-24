using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.Orchestration.GroupChat;
using Microsoft.SemanticKernel.Agents.Orchestration.Handoff;
using Microsoft.SemanticKernel.Agents.Runtime.InProcess;

public class Lab58
{

    // dotnet add package Microsoft.SemanticKernel.Agents.Orchestration --version 1.57.0-preview
    // dotnet add package Microsoft.SemanticKernel.Agents.Runtime.Core --version 1.57.0-preview
    // dotnet add package Microsoft.SemanticKernel.Agents.Runtime.InProcess --version 1.57.0-preview

    public static async Task Execution()
    {
        Console.WriteLine("Hello, Multi-Agent System! \n\n");

        Kernel kernel = Kernel.CreateBuilder()
                                .AddOpenAIChatCompletion(
                                    apiKey: Config.OpenAI_ApiKey,
                                    modelId: Config.ModelId)
                                .Build();

        // 註冊多個plugins
        kernel.Plugins.AddFromType<HRPolicyService>();
        kernel.Plugins.AddFromType<ITSupportService>();
        kernel.Plugins.AddFromType<ComplianceService>();

        // 建立多個聊天代理人
        var triageAgent = new ChatCompletionAgent
        {
            Kernel = kernel,
            Name = "TriageAgent",
            Description = "問題分流助理",
            Instructions = @"對使用者的提問進行分類的企業支援代理。任務是判斷使用者問題屬於哪一類：人資(HR)、IT、或合規(Compliance)。
            若不屬於任何類型，也可請使用者釐清問題類別。"
        };

        var hrAgent = new ChatCompletionAgent
        {
            Kernel = kernel,
            Name = "HRPolicyAgent",
            Description = "負責回應與人資制度與公司內部 HR 規章相關的問題",
            Instructions = """
你是公司的人資專員，請針對與以下主題相關的提問提供清楚、準確的解答：
- 人事制度（如考勤、升遷、轉調等）
- 請假規定（如病假、事假、特休等）
- 員工福利（如保險、補助、活動等）

如果問題不在你的職責範圍，請回覆「這部分建議您洽詢其他單位，我無法提供正確資訊」。
"""
        };

        var itAgent = new ChatCompletionAgent
        {
            Kernel = kernel,
            Name = "ITSupportAgent",
            Description = "提供 IT 設備、VPN、帳號權限等技術支援服務",
            Instructions = """
你是公司的 IT 支援人員，請協助處理以下類型的問題：
- IT 設備（如電腦、印表機、網路）
- 帳號與系統存取權限（如登入問題、權限申請）
- VPN、遠端連線與系統備份設定

請用清楚的步驟或指引協助提問者解決問題。若問題非 IT 支援職責，請委婉引導至相關單位。
"""
        };

        var complianceAgent = new ChatCompletionAgent
        {
            Kernel = kernel,
            Name = "ComplianceAgent",
            Description = "協助處理合規與資訊安全條例的詢問",
            Instructions = """
你是法遵與資安顧問，請專注回答以下範圍的問題：
- 公司合約條款與合規要求
- 資訊安全政策（如存取控制、資料保護）
- 公司治理規定與內部稽核事項

若遇到與 HR 或 IT 技術相關的問題，請提醒使用者改由其他專責人員處理。
"""
        };



        //選擇合適的聊天代理人 (用 HandoffOrchestration)
        // Define the orchestration
        HandoffOrchestration orchestration =
            new(OrchestrationHandoffs
                    .StartWith(triageAgent)
                    .Add(triageAgent, hrAgent, itAgent, complianceAgent)
                    .Add(triageAgent, hrAgent, "Transfer to this agent if the issue is about leave, salary, HR policy, or attendance.")
                    .Add(triageAgent, itAgent, "Transfer to this agent if the issue is about IT, account, VPN, password, or device.")
                    .Add(triageAgent, complianceAgent, "Transfer to this agent if the issue is about compliance, security, or internal company policy."),
                triageAgent, hrAgent, itAgent, complianceAgent);


        // 建立協作流程物件
        InProcessRuntime runtime = new();

        // 使用 Azure AI Agent 進行對話
        Console.Write("User > ");
        string? userInput;
        while ((userInput = Console.ReadLine()) is not null)
        {
            //啟動協作流程
            await runtime.StartAsync();
            if (string.IsNullOrWhiteSpace(userInput) || userInput.Equals("exit", StringComparison.OrdinalIgnoreCase))
                break;

            // 將使用者輸入傳遞給協作流程
            var result = await orchestration.InvokeAsync(userInput, runtime);
            Console.Write("AI > ");
            var response = result.GetValueAsync(TimeSpan.FromSeconds(300));
            Console.Write(response);
            await runtime.RunUntilIdleAsync();

            Console.WriteLine("\n\n");
            Console.Write("User > ");
        }
    }
}