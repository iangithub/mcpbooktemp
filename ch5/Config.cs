public static class Config
{
    public static string OpenAI_ApiKey { get; } = "";
    public static string ModelId { get; } = "gpt-4.1";
    public static string Azure_OpenAI_ApiKey { get; } = "";
    public static string Azure_AI_Foundry_EndPoint { get; } = "";
    public static string Azure_ModelId { get; } = "gpt-4.1";

    // Ollama 設定
    public static string Ollama_Endpoint { get; } = "http://localhost:11434";
    public static string Ollama_ModelId { get; } = "gpt-oss:20b";

}