using Microsoft.SemanticKernel;
public static class Lab51
{
    // 連接到 OpenAI 的聊天服務，並建立一個簡單的聊天機器人範例
    // 這個範例不包含 function calling 的功能
    // 不具短期記憶對話功能
    public static async Task Execution()
    {
        Kernel kernel = Kernel.CreateBuilder()     
                        .AddOpenAIChatCompletion(
                            apiKey: Config.OpenAI_ApiKey,
                            modelId: Config.ModelId)
                        .Build();

        while (true)
        {
            Console.Write("You: ");
            var input = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(input) || input.Equals("exit", StringComparison.OrdinalIgnoreCase))
                break;

            var response = await kernel.InvokePromptAsync(input);
            Console.WriteLine($"AI: {response} \n\n");
        }

        Console.WriteLine("Bye!");
    }
}