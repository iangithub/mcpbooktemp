import asyncio
from agents import Agent, ItemHelpers, Runner, trace, OpenAIChatCompletionsModel, set_tracing_export_api_key
from openai import AsyncOpenAI

api_key = "sk-proj-xx"
set_tracing_export_api_key(api_key)

client = AsyncOpenAI(
    api_key = api_key,
)    

east_market_agent = Agent(
    name="east_market_agent",
    model=OpenAIChatCompletionsModel( 
        model="gpt-5",
        openai_client=client
    ),
    instructions="你是東市的駿馬商人，只販賣優質駿馬。",
)

west_market_agent = Agent(
    name="west_market_agent",
    model=OpenAIChatCompletionsModel( 
        model="gpt-5",
        openai_client=client
    ),
    instructions="你是西市的鞍韉商人，只販賣精美鞍韉。",
)

south_market_agent = Agent(
    name="south_market_agent",
    model=OpenAIChatCompletionsModel( 
        model="gpt-5",
        openai_client=client
    ),
    instructions="你是南市的轡頭商人，只販賣優質轡頭。",
)

north_market_agent = Agent(
    name="north_market_agent",
    model=OpenAIChatCompletionsModel( 
        model="gpt-5",
        openai_client=client
    ),
    instructions="你是北市的長鞭商人，只販賣精良長鞭。",
)

shopping_guide_agent = Agent(
    name="shopping_guide_agent",
    model=OpenAIChatCompletionsModel( 
        model="gpt-5",
        openai_client=client
    ),
    instructions="你是一位專業的馬具購物指南，請簡單回應使用者該去哪個市場買？",
)


async def main():
    msg = input("歡迎來到馬市！請告訴我您想要什麼樣的馬具？\n\n")

    with trace("花木蘭的任務"):
        east_res, west_res, south_res, north_res = await asyncio.gather(
            Runner.run(
                east_market_agent,
                f"客戶需求：{msg}",
            ),
            Runner.run(
                west_market_agent,
                f"客戶需求：{msg}",
            ),
            Runner.run(
                south_market_agent,
                f"客戶需求：{msg}",
            ),
            Runner.run(
                north_market_agent,
                f"客戶需求：{msg}",
            ),
        )

        recommendations = {
            "東市駿馬推薦": ItemHelpers.text_message_outputs(east_res.new_items),
            "西市鞍韉推薦": ItemHelpers.text_message_outputs(west_res.new_items),
            "南市轡頭推薦": ItemHelpers.text_message_outputs(south_res.new_items),
            "北市長鞭推薦": ItemHelpers.text_message_outputs(north_res.new_items),
        }

        print("\n\n=== 各市場推薦 ===")
        for market, recommendation in recommendations.items():
            print(f"\n【{market}】\n{recommendation}")

        all_recommendations = "\n\n".join([f"【{market}】\n{rec}" for market, rec in recommendations.items()])
        
        best_guide = await Runner.run(
            shopping_guide_agent,
            f"客戶需求：{msg}\n\n各市場推薦：\n{all_recommendations}",
        )

    print("\n\n" + "="*50)
    print("🛍️ 購物指南建議：")
    print(f"{best_guide.final_output}")


if __name__ == "__main__":
    asyncio.run(main())