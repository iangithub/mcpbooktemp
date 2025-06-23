using System.ComponentModel;
using Microsoft.SemanticKernel;

public class BusinessAssistantService
{
    [KernelFunction]
    [Description("Query the customer's account and contract status")]
    public static string GetCustomerInfo(
        [Description("customer name")] string customerName)
    {
        // 模擬客戶資料查詢
        // 在實際應用中，這裡可能會連接到資料庫或其他資料來源
        return customerName switch
        {
            "大大銀行" => "大大銀行的帳號是 #SINO123，合約狀態為有效（到期日 2025/12/31）",
            "錢多多金控" => "錢多多金控的帳號是 #TAISHIN001，合約狀態為審核中",
            _ => $"找不到名為 {customerName} 的客戶資料。"
        };
    }

    [KernelFunction]
    [Description("Query the salesperson's contact information")]
    public static string GetSalespersonContact(
        [Description("salesperson name")] string name)
    {
        return name switch
        {
            "Ian" => "Ian 的聯絡方式：ian@company.com，電話 02-1688-0857",
            "Cheryl" => "Cheryl 的聯絡方式：cheryl@company.com，電話 02-9571-1688",
            _ => $"找不到 {name} 的聯絡資訊。"
        };
    }
}