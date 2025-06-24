using System.ComponentModel;
using Microsoft.SemanticKernel;

//實際情況下，這裡可能會查詢資料庫或調用外部API來獲取內容
public class HRPolicyService
{
    [KernelFunction, Description("Get detailed leave policy information")]
    public string GetLeavePolicy([Description("Type of leave")] string leaveType)
    {
        return @"請假政策說明：\n" +
               "1. 病假：每年提供30日，未使用部分得轉為特休。\n" +
               "2. 特休：依年資計算，滿一年提供7日，滿兩年提供10日。\n" +
               "3. 事假：每年最多7日，不扣薪但需主管核准。";
    }

    [KernelFunction, Description("Query specific benefit policy")]
    public string GetBenefitPolicy([Description("Benefit name")] string benefitName)
    {
        return @"福利政策說明：\n" +
               "1. 旅遊補助：每位員工每年可申請一次旅遊補助上限 NT$5,000。\n" +
               "2. 健康檢查：公司每兩年提供一次免費員工健康檢查。\n" +
               "3. 午餐補助：每日午餐補助 NT$100，自動匯入薪資帳戶。";
    }

    [KernelFunction, Description("Query attendance policy description")]
    public string GetAttendancePolicy()
    {
        return "上班時間為上午9:00至下午6:00，彈性時間為8:00至10:00，需每日完成8小時工作。";
    }
}

//實際情況下，這裡可能會查詢資料庫或調用外部API來獲取內容
public class ITSupportService
{
    [KernelFunction, Description("Get VPN setup tutorial")]
    public string GetVpnSetup()
    {
        return "VPN 設定步驟：1. 下載公司專用 VPN 軟體；2. 安裝後輸入員工帳號密碼；3. 連線至內部網段 vpn.company.com。";
    }

    [KernelFunction, Description("Query system account activation process")]
    public string GetAccountPolicy([Description("System name")] string systemName)
    {
        return $"系統「{systemName}」的帳號申請請透過 IT 申請單，由主管核准後開通，預計作業時間為1-2工作天。";
    }

    [KernelFunction, Description("Query backup policy")]
    public string GetBackupPolicy()
    {
        return "所有桌機與筆電會自動每日備份至雲端，保留期限為30日。如需還原請聯繫 IT 支援。";
    }
}


//實際情況下，這裡可能會查詢資料庫或調用外部API來獲取內容
public class ComplianceService
{
    [KernelFunction, Description("Query specific contract compliance clause")]
    public string GetContractPolicy([Description("Clause name")] string clauseName)
    {
        return clauseName.ToLower() switch
        {
            "保密條款" => "所有員工必須簽署 NDA，嚴禁洩漏公司資料予第三方。",
            "競業禁止" => "離職後六個月內不得任職於競爭公司，除非經公司書面同意。",
            _ => "找不到指定的合約條款。"
        };
    }

    [KernelFunction, Description("Query company data security policy")]
    public string GetDataSecurityPolicy()
    {
        return "公司所有機密文件必須儲存在加密磁碟區，傳輸時使用 TLS 加密，並定期進行權限稽核。";
    }

    [KernelFunction, Description("Query company governance policy")]
    public string GetGovernancePolicy()
    {
        return "公司治理依據董事會決議與公司章程執行，設有內部稽核與合規部門監督運作。";
    }
}

