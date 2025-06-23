using System.ComponentModel;
using System.Net;
using Azure.AI.Agents.Persistent;
using Azure.Identity;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.AzureAI;
using Microsoft.SemanticKernel.ChatCompletion;

public static class LabTest
{
    // dotnet add package Microsoft.SemanticKernel.Agents.AzureAI --version 1.57.0-preview
    // dotnet add package Azure.Identity
    // use entra ID to authenticate, az login --tenant <tenant-id>


    public static async Task Execution()
    {
        Console.WriteLine("Hello, Azure AI Agent! \n\n");
        PersistentAgentsClient client = AzureAIAgent.CreateAgentsClient(Config.Azure_AI_Foundry_EndPoint, new AzureCliCredential());


        // Define the agent
        // AzureAIAgent agent = await CreateAzureAgentAsync(client,
        //         plugin: KernelPluginFactory.CreateFromType<MenuPlugin>(),
        //         instructions: "Answer questions about the menu.",
        //         name: "Host");

        // Create a thread for the agent conversation.
        //AgentThread thread = new AzureAIAgentThread(client);

        PersistentAgent definition = await client.Administration.GetAgentAsync("asst_eRTOxkkGDZLCkm7bvICX7lYm");
        AzureAIAgent agent = new(definition, client);
        agent.Kernel.Plugins.Add(KernelPluginFactory.CreateFromType<MenuPlugin>());
        AzureAIAgentThread thread = new(agent.Client);


        // Respond to user input
        await InvokeAgentAsync(agent, thread, "Hello");
        await InvokeAgentAsync(agent, thread, "What is the special soup and its price?");
        await InvokeAgentAsync(agent, thread, "What is the special drink and its price?");
        await InvokeAgentAsync(agent, thread, "Thank you");
        // try
        // {
        //     await InvokeAgentAsync(agent, thread, "Hello");
        //     await InvokeAgentAsync(agent, thread, "What is the special soup and its price?");
        //     await InvokeAgentAsync(agent, thread, "What is the special drink and its price?");
        //     await InvokeAgentAsync(agent, thread, "Thank you");
        // }
        // finally
        // {
        //     await thread.DeleteAsync();
        //     await client.Administration.DeleteAgentAsync(agent.Id);
        // }
    }

    private static async Task<AzureAIAgent> CreateAzureAgentAsync(PersistentAgentsClient client, KernelPlugin plugin, string? instructions = null, string? name = null)
    {
        // Define the agent
        PersistentAgent definition = await client.Administration.CreateAgentAsync(
            Config.Azure_ModelId,
            name,
            null,
            instructions);

        // AzureAIAgent agent =
        //     new(definition, client)
        //     {
        //         Kernel = this.CreateKernelWithChatCompletion(),
        //     };

        AzureAIAgent agent = new(definition, client)
        {
            Kernel = Kernel.CreateBuilder()
                .AddAzureOpenAIChatCompletion(
                    Config.Azure_ModelId,
                    Config.Azure_AI_Foundry_EndPoint,
                    new AzureCliCredential()).Build()
        };

        // Add to the agent's Kernel
        if (plugin != null)
        {
            agent.Kernel.Plugins.Add(plugin);
        }

        return agent;
    }

    private static async Task InvokeAgentAsync(AzureAIAgent agent, AgentThread thread, string input)
    {
        ChatMessageContent message = new(AuthorRole.User, input);
        Console.WriteLine(message);

        await foreach (ChatMessageContent response in agent.InvokeAsync(message, thread))
        {
            Console.WriteLine(response);
        }
    }

}


public sealed class MenuPlugin
{
    [KernelFunction, Description("Provides a list of specials from the menu.")]
    public MenuItem[] GetMenu()
    {
        return s_menuItems;
    }

    [KernelFunction, Description("Provides a list of specials from the menu.")]
    public MenuItem[] GetSpecials()
    {
        return [.. s_menuItems.Where(i => i.IsSpecial)];
    }

    [KernelFunction, Description("Provides the price of the requested menu item.")]
    public float? GetItemPrice(
        [Description("The name of the menu item.")]
        string menuItem)
    {
        return s_menuItems.FirstOrDefault(i => i.Name.Equals(menuItem, StringComparison.OrdinalIgnoreCase))?.Price;
    }

    private static readonly MenuItem[] s_menuItems =
        [
            new()
            {
                Category = "Soup",
                Name = "Clam Chowder",
                Price = 4.95f,
                IsSpecial = true,
            },
            new()
            {
                Category = "Soup",
                Name = "Tomato Soup",
                Price = 4.95f,
                IsSpecial = false,
            },
            new()
            {
                Category = "Salad",
                Name = "Cobb Salad",
                Price = 9.99f,
            },
            new()
            {
                Category = "Salad",
                Name = "House Salad",
                Price = 4.95f,
            },
            new()
            {
                Category = "Drink",
                Name = "Chai Tea",
                Price = 2.95f,
                IsSpecial = true,
            },
            new()
            {
                Category = "Drink",
                Name = "Soda",
                Price = 1.95f,
            },
        ];

    public sealed class MenuItem
    {
        public string Category { get; init; }
        public string Name { get; init; }
        public float Price { get; init; }
        public bool IsSpecial { get; init; }
    }
}