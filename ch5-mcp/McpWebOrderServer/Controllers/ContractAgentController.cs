using System.ComponentModel;
using McpWebOrderServer.Agents;
using Microsoft.AspNetCore.Mvc;
using ModelContextProtocol.Server;


[ApiController]
[Route("api/[controller]")]
public class ContractAgentController : ControllerBase
{
    [McpServerToolType]
    public class ContractTool
    {
        // 執行合約審查
        [McpServerTool,
         Description("Execute contract review based on contract content")]
        public static async Task<string> Contract_Review(string contract)
        {
            return await ContractAgent.Execute(contract);
        }
    }
}