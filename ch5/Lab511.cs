using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.Orchestration.Concurrent;
using Microsoft.SemanticKernel.Agents.Orchestration.GroupChat;
using Microsoft.SemanticKernel.Agents.Orchestration.Sequential;
using Microsoft.SemanticKernel.Agents.Runtime.InProcess;

public class Lab511
{
    public static async Task Execute()
    {
        Console.WriteLine("Hello, Multi-Agent System! \n\n");

        Kernel kernel = Kernel.CreateBuilder()
                                .AddOpenAIChatCompletion(
                                    apiKey: Config.OpenAI_ApiKey,
                                    modelId: Config.ModelId)
                                .Build();

        // 法務審查員
        var legalAgent = new ChatCompletionAgent()
        {
            Kernel = kernel,
            Name = "LegalAgent",
            Description = "法務審查員",
            Instructions =
            """
            你是專業企業法務審查員，請根據下列重點對收到的合約內容進行逐項詳細審查，逐點列舉每一項條款之合規性、潛在法律風險，以及改善建議：

            1. 合約雙方名稱、身份、權利義務描述是否明確？
            2. 合約主體（標的）及服務內容是否清楚完整、無歧義？
            3. 履約期限、重要時程、交付/驗收條件是否具體明載？
            4. 價金與付款條件有無明確、是否約定幣別、支付方式、分期規範？
            5. 保密條款是否周延，包含資訊、技術、個資等範圍與存續期間？
            6. 智慧財產權歸屬、授權、使用限制是否明確，是否有爭議風險？
            7. 違約責任及損害賠償機制（定義、計算方式、範圍）是否合理？
            8. 不可抗力條款（如天災、疫情等）有無明定及其處理方式？
            9. 合約終止、解約之條件、程序及其權利義務分配是否合理？
            10. 爭議解決條款：管轄法院/仲裁機制、準據法條款是否明確？
            11. 合約是否提及相關法律規範（如公司法、民法、個資法等），有無抵觸？
            12. 其他特殊條款（如保證、保固、再委託、合約轉讓等）之合規性檢查。

            請針對每一項重點逐一檢查，指出合格/不合格/需補充之處，並列出潛在法律風險與建議修正內容。  
            最後，請總結整體合約法律風險等級（低/中/高）及必須優先修正之重大條款。
            """
        };

        // 財務審查員
        var financeAgent = new ChatCompletionAgent()
        {
            Kernel = kernel,
            Name = "FinanceAgent",
            Description = "財務審查員",
            Instructions =
             """
            你是企業財務審查專家，請依下列重點細查合約內容：

            1. 付款條件（付款時間、分期方式、預付款與尾款設計）是否明確合理？
            2. 金額、貨幣單位及總額是否正確，與預算/報價是否一致？
            3. 有無約定逾期付款之罰則或利息？
            4. 退費、折讓、違約賠償之計算方式是否明確？
            5. 有無未明定的附加費用、隱藏成本？
            6. 若合約涉外，是否考慮匯率波動、國際稅法、境外帳戶等風險？
            7. 付款憑證（如發票、收據）取得及存查方式有無具體規定？
            8. 合約終止、變更、解約條款對財務影響？
            9. 有無雙方授信額度/付款擔保機制（如保證金、擔保函等）？
            10. 其他涉及重大財務風險或不利於本公司條款。

            請針對上述審查點，逐一檢查並列出建議與風險。
            """
        };

        // 資訊安全審查員
        var infosecAgent = new ChatCompletionAgent()
        {
            Kernel = kernel,
            Name = "InfoSecAgent",
            Description = "資訊安全審查員",
            Instructions =
            """
            你是企業資安審查專家，請根據以下清單，針對收到的合約內容進行逐項審查，務必指出具體條文對應的資安疑慮與建議改善方式。

            請重點審查下列面向（如未提及，請標註風險）：
            1. 是否明確規範雙方的資料存取權限、責任歸屬？
            2. 是否要求數據傳輸與儲存全程加密？（如AES、TLS等）
            3. 系統登入、管理、維護的認證與權限設計是否充分？
            4. 合約有無明定資訊安全事故的通報機制與責任劃分？
            5. 是否遵循個資法、GDPR 或相關法令要求？
            6. 第三方（如供應商、分包）存取權限及風險管理條款是否明確？
            7. 合約期間結束或終止時，資料移轉、刪除等交接程序是否完備？
            8. 有無規範定期安全稽核、弱點掃描或滲透測試之義務？
            9. 有無明確懲處/賠償條款，若一方造成資安事故損失？
            10. 是否有明訂系統異動/維護必須經過雙方書面同意的規範？

            請依上列審查點，針對本合約逐條點評（如「合格/有缺漏/風險高」），並提出具體改善建議。
            最後給出整體資訊安全風險總結評語。
            """
        };


        // Define the orchestration
        ConcurrentOrchestration orchestration =
                    new(legalAgent, financeAgent, infosecAgent);


        // 建立 Runtime
        var runtime = new InProcessRuntime();
        await runtime.StartAsync();

        // 合約內容
        string contract =
        """
        本合約規範甲乙雙方於2025年資訊系統採購合作事宜。
        甲方將於30日內支付首期款項，其餘分三期付款。
        雙方須遵守資料保護法，所有系統存取需經加密認證。
        但為求效率，系統個資可以不加密或不去識別化處理。
        合約期間若乙方未達KPI，甲方有權不經通知即可逕自終止合作。
        本案如有爭議，雙方同意於台北地方法院處理。
        """;

        Console.WriteLine($"【合約草案】\n{contract}\n");
        var result = await orchestration.InvokeAsync(contract, runtime);

        // 顯示彙整回覆
        var finalReport = await result.GetValueAsync(TimeSpan.FromSeconds(300));
        Console.WriteLine("【各部門審查意見】：\n");
        Console.WriteLine($"{string.Join("\n\n", finalReport.Select(text => $"{text}"))}");

        await runtime.RunUntilIdleAsync();
    }
}