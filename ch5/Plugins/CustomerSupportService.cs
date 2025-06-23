using System.ComponentModel;
using Microsoft.SemanticKernel;

public class CustomerSupportService
{
    [KernelFunction]
    [Description("Get the status of an order.")]
    public static string GetOrderStatus(
       [Description("Order ID")] string orderId)
    {
        //實際情況下，這裡可能會查詢資料庫或調用外部API來獲取訂單狀態
        return $"您查詢的訂單 {orderId} 的狀態為：已出貨";
    }

    [KernelFunction]
    [Description("Get the product information.")]
    public static string GetProductInfo(
            [Description("Product Name")] string productName)
    {
        //實際情況下，這裡可能會查詢資料庫或調用外部API來獲取產品資訊
        return $"{productName} 是我們的長年熱銷商品，價格為 NT$599。";
    }
}