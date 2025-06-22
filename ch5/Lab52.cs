using System.ComponentModel;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
public static class Lab52
{
    // 連接到 OpenAI 的聊天服務，並建立一個簡單的聊天機器人範例
    // 包含 function calling(Native function) 的功能
    // 不具短期記憶對話功能
    public static async Task Execution()
    {
        Kernel kernel = Kernel.CreateBuilder()
                        .AddOpenAIChatCompletion(
                            apiKey: Config.OpenAI_ApiKey,
                            modelId: Config.ModelId)
                        .Build();

        kernel.Plugins.AddFromType<WeatherService>();
        OpenAIPromptExecutionSettings settings =
        new() { FunctionChoiceBehavior = FunctionChoiceBehavior.Auto() };


        while (true)
        {
            Console.Write("You: ");
            var input = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(input) || input.Equals("exit", StringComparison.OrdinalIgnoreCase))
                break;

            var response = await kernel.InvokePromptAsync(input, new(settings));
            Console.WriteLine($"AI: {response} \n\n");
        }

        Console.WriteLine("Bye!");
    }



}