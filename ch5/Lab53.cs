using System.ComponentModel;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

// 具短期記憶對話與自動 function calling(Native function) 的聊天範例
public static class Lab53
{
    public static async Task Execution()
    {
        Kernel kernel = Kernel.CreateBuilder()
                        .AddOpenAIChatCompletion(
                            apiKey: Config.OpenAI_ApiKey,
                            modelId: Config.ModelId)
                        .Build();

        kernel.Plugins.AddFromType<WeatherService>();

        // 建立對話歷史
        ChatHistory history = [];

        // 從 kernel 取得聊天服務
        var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

        // 開始聊天對話
        Console.Write("User > ");
        string? userInput;
        while ((userInput = Console.ReadLine()) is not null)
        {

            if (string.IsNullOrWhiteSpace(userInput) || userInput.Equals("exit", StringComparison.OrdinalIgnoreCase))
                break;

            // 加入使用者訊息到對話歷史
            history.AddUserMessage(userInput);

            // 開啟自動 function calling 機制
            OpenAIPromptExecutionSettings settings =
                    new() { FunctionChoiceBehavior = FunctionChoiceBehavior.Auto() };

            // Get the response from the AI
            var result = chatCompletionService.GetStreamingChatMessageContentsAsync(
                                history,
                                executionSettings: settings,
                                kernel: kernel);

            // Stream the results
            string fullMessage = "";
            var first = true;
            await foreach (var content in result)
            {
                if (content.Role.HasValue && first)
                {
                    Console.Write("Assistant > ");
                    first = false;
                }
                Console.Write(content.Content);
                fullMessage += content.Content;
            }
            Console.WriteLine();
            // 加入 AI 回應到對話歷史
            history.AddAssistantMessage(fullMessage);

            // Get user input again
            Console.Write("User > ");
        }


        Console.WriteLine("Bye!");
    }
}