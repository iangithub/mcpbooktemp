from agents import Agent, Runner, GuardrailFunctionOutput, InputGuardrailTripwireTriggered, OpenAIChatCompletionsModel, input_guardrail
from agents.extensions.handoff_prompt import RECOMMENDED_PROMPT_PREFIX
from openai.types.responses import ResponseTextDeltaEvent
from pydantic import BaseModel
from typing import Literal
from openai import AsyncOpenAI
from agents import set_tracing_export_api_key
from agents.mcp.server import MCPServerStreamableHttp  

api_key = "sk-proj-xx"
client = AsyncOpenAI(
    api_key = api_key,
)    

set_tracing_export_api_key(api_key)

mcp_server = MCPServerStreamableHttp(
    params={
        "url": "https://xx.ngrok-free.app/mcp/mcp", 
    },
    cache_tools_list=True            
)

inventory_assistant = Agent(
    name="Inventory assistant",
    instructions="你是一個庫存小助手，可以幫助使用者查詢產品的庫存。",
    model=OpenAIChatCompletionsModel( 
        model="gpt-4.1",
        openai_client=client
    ),
    handoff_description="查詢產品的庫存",
    mcp_servers=[mcp_server]
)

therapy_assistant = Agent(
    name="Therapy assistant",
    instructions="你是一個心理諮詢助手，可以幫助使用者解決情感上的困擾",
    model=OpenAIChatCompletionsModel( 
        model="gpt-4.1",
        openai_client=client
    )
)

class SafetyCheckOutput(BaseModel):
    category: Literal["inventory", "therapy", "general"]
    should_block: bool  # 只有違規內容才設 True
    
guardrail_agent = Agent(
    name="Guardrail check",
    instructions=(
        "你是內容審核助手。"
        "請判斷 user_input 屬於下列哪一類："
        "1. inventory  → 詢問商品存貨、庫存、價格…"
        "2. therapy    → 心理支持、情感抒發、尋求建議…"
        "3. general    → 其他一般商務或聊天內容"
        "若問題含暴力、違法、色情、自殘等禁忌，請將 should_block 設為 true，"
        "否則應為 false。請依下列 JSON 輸出："
        '{"category": <category>, "should_block": <true/false>}'
    ),
    model=OpenAIChatCompletionsModel(
        model="gpt-4.1",
        openai_client=client
    ),
    output_type=SafetyCheckOutput,
)

@input_guardrail
async def safety_guardrail(ctx, agent, user_input):
    result = await Runner.run(guardrail_agent, user_input, context=ctx.context)
    verdict = result.final_output_as(SafetyCheckOutput)

    tripwire = verdict.should_block

    return GuardrailFunctionOutput(
        output_info={"category": verdict.category},
        tripwire_triggered=tripwire
    )


gm_assistant = Agent(
    name="General Manager Assistant",
    instructions=f"""
    {RECOMMENDED_PROMPT_PREFIX}
    你是一個總經理小助手，
    如果總經理問庫存，你就幫他轉接庫存小助手，
    同時你還要解決總經理的人生難題""",
    model=OpenAIChatCompletionsModel( 
        model="gpt-4.1",
        openai_client=client
    ),
    handoffs=[inventory_assistant,therapy_assistant],
    input_guardrails=[safety_guardrail]

)

async def main():
    await mcp_server.connect()

    try:
        result = await Runner.run(
            gm_assistant, 
            input="洋芋片還有多少庫存",
        )

        print(f"Response: {result.final_output}")

        result = Runner.run_streamed(gm_assistant, input="我好痛苦，請幫我解決人生難題")
        async for event in result.stream_events():
            if event.type == "raw_response_event" and isinstance(event.data, ResponseTextDeltaEvent):
                print(event.data.delta, end="", flush=True)

        try:
            result = await Runner.run(
                gm_assistant, 
                input="我想要做偏門生意，請幫我想個好方法",
            )

            print(f"Response: {result.final_output}")
            
        except InputGuardrailTripwireTriggered:
            print("Response: 你的輸入不合規！！")
    finally:
        await mcp_server.cleanup()

if __name__ == "__main__":
    import asyncio
    asyncio.run(main())
