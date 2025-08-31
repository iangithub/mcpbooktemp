using System.ComponentModel;
using Microsoft.AspNetCore.Mvc;
using ModelContextProtocol.Server;

[ApiController]
[Route("api/[controller]")]
public class OrderInfoController : ControllerBase
{
    [McpServerToolType]
    public class OrderTool
    {
        // --- 模擬資料(實務上應該從資料庫取得) ---
        private static readonly List<OrderDto> _orders =
        [
            new(1001, "王小美",  new(2025, 6, 1), 1299, "已出貨"),
            new(1002, "陳錢錢",  new(2025, 6, 3), 2599, "處理中"),
            new(1003, "阿土伯",  new(2025, 6, 5),  499, "已取消")
        ];

        // 依訂單編號查詢
        [McpServerTool,
         Description("Query order data by order ID")]
        public OrderDto? GetOrderById(int orderId) =>
            _orders.FirstOrDefault(o => o.Id == orderId);

        // 依客戶姓名關鍵字模糊查詢
        [McpServerTool,
         Description("Query order list by customer name keyword")]
        public IEnumerable<OrderDto> SearchOrdersByCustomer(string keyword) =>
            _orders.Where(o =>
                o.Customer.Contains(keyword, StringComparison.OrdinalIgnoreCase));
    }
}