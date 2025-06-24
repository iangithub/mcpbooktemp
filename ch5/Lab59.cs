using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.Orchestration.Sequential;
using Microsoft.SemanticKernel.Agents.Runtime.InProcess;


public class Lab59
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


        var hrAgent = new ChatCompletionAgent()
        {
            Kernel = kernel,
            Name = "HRAgent",
            Description = "人資政策審查員",
            Instructions =
            """
            請審查使用者提交內容是否符合公司人資政策
            - 不得出現員工個人資料
            - 請假加班必須填單申請
            - 上下班必須打卡
            - 不得有性騷擾或性別歧視言論

            若有疑慮請給出建議或補充意見，若是合規則請回覆「合規」。

            ## 輸出格式：
            [草案內容]
            ...（草案）

            [人資部門審查意見]
            ...（內容)
            """
        };
        var itAgent = new ChatCompletionAgent()
        {
            Kernel = kernel,
            Name = "ITAgent",
            Description = "資訊/技術審查員",
            Instructions =
            """
            你將會收到一份公司政策草案以及前面部門的審查意見（若有）。請保留所有既有意見內容，再加上你本部門的審查意見，不可刪除任何意見。

            請檢查使用者提交內容是否涉及資訊安全
            - 不得出現帳號、密碼、伺服器主機位置資訊
            - 不得安裝私人軟體
            
            若有疑慮請給出建議或補充意見，若是合規則請回覆「合規」。

            ## 輸出格式：
            [草案內容]
            ...（草案）

            [人資部門審查意見]
            ...（內容)

             [IT部門審查意見]
            ...（內容）
            """
        };
        var complianceAgent = new ChatCompletionAgent()
        {
            Kernel = kernel,
            Name = "ComplianceAgent",
            Description = "法遵審查員",
            Instructions =
            """
            你將會收到一份公司政策草案以及前面部門的審查意見（若有）。請保留所有既有意見內容，再加上你本部門的審查意見，不可刪除任何意見。

            請檢查使用者提交內容有無違反公司內部守則或法規
            - 不得出現客戶資料
            - 不得涉及商業機密洩漏
            - 不得出現仇恨言論或歧視性內容

            若有疑慮請給出建議或補充意見，若是合規則請回覆「合規」。

            ## 輸出格式：
            [草案內容]
            ...（草案）

            [人資部門審查意見]
            ...（內容）

            [IT部門審查意見]
            ...（內容）

            [合規部門審查意見]
            ...（內容）
            """
        };

        // Define the orchestration
        SequentialOrchestration orchestration =
            new(hrAgent, itAgent, complianceAgent)
            {
                Name = "DocumentReviewOrchestration",
                Description = "多代理人協同審查文件內容，確保符合人資政策、資訊安全與合規要求。"
            };


        // 建立協作流程物件
        InProcessRuntime runtime = new();
        await runtime.StartAsync();

        // 模擬執行一份草案文件審查
        string input =
        """ 
        ### 以下是使用者的具體提交內容

        公司新政策草案：
        自2024年起，所有員工可申請每月1天遠端工作日，
        加班工時上限每月40小時，須提前主管同意。
        女性同仁不得申請生理假。
        另外為了提升資安意識，所有員工必須每半年參加資安線上訓練一次。

        ### 請審查此草案是否符合人資政策、資訊安全與合規要求。
        """;

        Console.WriteLine("開始進行文件審查協作流程...\n");
        Console.WriteLine($"草案內容：\n{input}\n");



        // 執行 Sequential Orchestration
        var result = await orchestration.InvokeAsync(input, runtime);

        string finalReport = await result.GetValueAsync(TimeSpan.FromSeconds(300));
        Console.WriteLine("\n審查完成：");
        Console.WriteLine(finalReport);

        await runtime.RunUntilIdleAsync();
    }
}