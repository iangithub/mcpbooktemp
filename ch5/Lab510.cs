using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.Orchestration.GroupChat;
using Microsoft.SemanticKernel.Agents.Orchestration.Sequential;
using Microsoft.SemanticKernel.Agents.Runtime.InProcess;

public class Lab510
{
    public static async Task Execute()
    {
        Console.WriteLine("Hello, Multi-Agent System! \n\n");

        Kernel kernel = Kernel.CreateBuilder()
                                .AddOpenAIChatCompletion(
                                    apiKey: Config.OpenAI_ApiKey,
                                    modelId: Config.ModelId)
                                .Build();

        // 正方 Agent(降價支持者)
        var proAgent = new ChatCompletionAgent()
        {
            Kernel = kernel,
            Name = "ProAgent",
            Description = "降價支持者",
            Instructions =
            """
            你是一位行銷專家，專責從產品市場面積極推動限時降價的角度思考，根據以下幾個關鍵因素進行評估與決策
            - 產品屬性與生命週期
            - 競爭對手的定價策略
            - 目標市場與消費者客群

            請詳細說明降價促銷帶來的好處、正面效益與潛在機會。
            """
        };

        // 反方 Agent(降價反對者)
        var conAgent = new ChatCompletionAgent()
        {
            Kernel = kernel,
            Name = "ConAgent",
            Description = "降價反對者",
            Instructions =
            """
            你是一位行銷專家，專責從產品市場面反對大幅度降價的角度思考，，根據以下幾個關鍵因素進行評估與決策
            - 產品屬性與生命週期
            - 競爭對手的定價策略
            - 目標市場與消費者客群
            
            請詳細說明降價可能帶來的壞處、風險與品牌損害。
            """
        };

        // 中立策略 Agent
        var strategyAgent = new ChatCompletionAgent()
        {
            Kernel = kernel,
            Name = "StrategyAgent",
            Description = "策略顧問",
            Instructions = @"你是一位中立的策略顧問，請綜合正反兩方的觀點，提出平衡、創新的行銷方案，或提醒團隊注意雙方可能忽略的重點。"
        };

        // 建立 GroupChatOrchestration，配置最多討論次數為 5 次
        GroupChatOrchestration groupOrchestration =
            new(new RoundRobinGroupChatManager()
            {
                MaximumInvocationCount = 5
            },
            proAgent, conAgent, strategyAgent);


        // 4. 建立 Runtime
        var runtime = new InProcessRuntime();
        await runtime.StartAsync();

        // 5. 討論主題
        string topic =
        """
        【產品資料】
        產品名稱：U Smart Watch
        產品定位：中高階運動健康智慧手錶，支援睡眠偵測、24小時心率監控，主打時尚設計。
        售價：新台幣5,990元

        【公司背景】
        品牌：UGO，專注於年輕消費者運動穿戴
        品牌調性：活力、創新、時尚
        市佔：約台灣市場第三

        【目標客群（TA）】
        18-35歲運動族群，重視外型與功能兼具、願意為品牌買單、偏好線上購物。

        【市場與競品】
        主要競品：Apple Watch SE、小米手環Pro、Garmin Venu Sq
        競品價格區間：2,000~9,000元

        【歷史促銷】
        過去618節日曾嘗試全站9折，銷量提升30%，但次月回落，部分客戶反映對品牌價值認知下滑。

        【本次行銷討論主題】
        考慮在U Smart Watch上市首月推動『限時新品上市降價促銷』，請各方針對上述背景資料進行具體討論與建議。
        """;

        Console.WriteLine($"【群組討論主題】\n{topic}\n");
        var result = await groupOrchestration.InvokeAsync(topic, runtime);

        // 6. 顯示所有 Agent 回覆
        var finalReport = await result.GetValueAsync(TimeSpan.FromSeconds(300));
        Console.WriteLine($"\n# RESULT: {finalReport}");

        await runtime.RunUntilIdleAsync();
    }
}