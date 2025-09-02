
import json
import asyncio
from typing import Literal

from pydantic import BaseModel
from openai import AsyncOpenAI
from openai.types.responses import ResponseTextDeltaEvent

from agents import (
    Agent,
    Runner,
    GuardrailFunctionOutput,
    OpenAIChatCompletionsModel,
    input_guardrail,
    set_tracing_export_api_key,
)
from agents.extensions.models.litellm_model import LitellmModel
from agents.extensions.handoff_prompt import RECOMMENDED_PROMPT_PREFIX
from agents.extensions.visualization import draw_graph
from agents.lifecycle import AgentHooks
import warnings

warnings.filterwarnings("ignore", category=UserWarning, module="pydantic")


OPENAI_KEY = "sk-proj-xx"
GEMINI_KEY = "xx"  

client = AsyncOpenAI(api_key=OPENAI_KEY)
set_tracing_export_api_key(OPENAI_KEY)


inventory_json = """
{
  "台北店": {"咖啡": 12, "洋芋片": 7},
  "台中店": {"咖啡": 9, "洋芋片": 14}
}
"""
inventory_db = json.loads(inventory_json)


def make_inventory_tool(branch: str):
    from agents import function_tool

    @function_tool
    def get_inventory(商品: str) -> str:
        qty = inventory_db.get(branch, {}).get(商品, 0)
        return f"{branch}{商品}庫存量為 {qty}"

    return get_inventory


tool_a = make_inventory_tool("台北店")
tool_b = make_inventory_tool("台中店")


class AuditHooks(AgentHooks):
    async def on_tool_start(self, context, agent, tool):
        print(f"[Audit] {agent.name} 將使用工具 {tool.name}")

    async def on_tool_end(self, context, agent, tool, result):
        print(f"[Audit] {tool.name} 完成，結果：{result}")

    async def on_handoff(
        self,
        context,
        agent, 
        source,
        **_
    ):
        print(f"[Audit] {source.name} → 交接給 {agent.name}")


branch_a_agent = Agent(
    name="Branch A assistant",
    instructions="你是台北店的小助手，只回答台北店產品庫存。",
    model=OpenAIChatCompletionsModel(model="gpt-5", openai_client=client),
    tools=[tool_a],
)

branch_b_agent = Agent(
    name="Branch B assistant",
    instructions="你是台中店的小助手，只回答台中店產品庫存。",
    model=OpenAIChatCompletionsModel(model="gpt-5", openai_client=client),
    tools=[tool_b],
)


therapy_agent = Agent(
    name="Therapy assistant",
    instructions="你是一位具備同理與專業知識的心理諮詢助手。",
    model=LitellmModel(
        model="gemini/gemini-1.5-flash",
        api_key=GEMINI_KEY,
    ),
)


class SafetyCheck(BaseModel):
    category: Literal["inventory", "therapy", "general"]
    should_block: bool


guardrail_agent = Agent(
    name="Guardrail check",
    instructions=(
        "你是內容審核助手。"
        "請判斷 user_input 屬於下列哪一類："
        "1. inventory → 商品存貨、價格…"
        "2. therapy   → 心理支持、情感抒發…"
        "3. general   → 其他內容\n"
        "若內容涉及暴力、違法、色情、自殘等禁忌，should_block 設為 true\n"
        '輸出 JSON，例如 {"category": "inventory", "should_block": false}'
    ),
    model=OpenAIChatCompletionsModel(model="gpt-5", openai_client=client),
    output_type=SafetyCheck,
)


@input_guardrail
async def safety_guardrail(ctx, agent, user_input):
    result = await Runner.run(guardrail_agent, user_input, context=ctx.context)
    verdict = result.final_output_as(SafetyCheck)
    return GuardrailFunctionOutput(
        output_info={"category": verdict.category},
        tripwire_triggered=verdict.should_block,
    )


gm_agent = Agent(
    name="General Manager assistant",
    instructions=(
        f"{RECOMMENDED_PROMPT_PREFIX}\n"
        "你是總經理小助手：\n"
        "‧ 若詢問庫存→交接到正確分店 Agent。\n"
        "‧ 若尋求心理支持→交接 Therapy assistant。\n"
        "‧ 其他問題直接回答。"
    ),
    model=OpenAIChatCompletionsModel(model="gpt-5", openai_client=client),
    handoffs=[branch_a_agent, branch_b_agent, therapy_agent],
    input_guardrails=[safety_guardrail],
    hooks=AuditHooks(),
)


async def parallel_inventory_query(product: str) -> str:
    a_task = Runner.run(branch_a_agent, f"{product} 庫存？")
    b_task = Runner.run(branch_b_agent, f"{product} 庫存？")
    res_a, res_b = await asyncio.gather(a_task, b_task)
    return f"{res_a.final_output}；{res_b.final_output}"



async def main() -> None:
    result = await Runner.run(gm_agent, input="台北店分別洋芋片還有多少？")
    print("─" * 40)
    print("[庫存回覆]", result.final_output)

    print("─" * 40)
    stream = Runner.run_streamed(gm_agent, input="我最近很焦慮，該怎麼辦？")
    async for event in stream.stream_events():
        if (
            event.type == "raw_response_event"
            and isinstance(event.data, ResponseTextDeltaEvent)
        ):
            print(event.data.delta, end="", flush=True)
    print()

    print("─" * 40)
    combined = await parallel_inventory_query("咖啡")
    print("[平行查詢]", combined)

    try:
        from graphviz.backend import ExecutableNotFound
        draw_graph(gm_agent, filename="agent_graph")
        print("Agent結構圖已輸出為 agent_graph.png")
    except ExecutableNotFound:
        print("[提示] 系統未安裝 Graphviz，已跳過繪圖")


if __name__ == "__main__":
    asyncio.run(main())

