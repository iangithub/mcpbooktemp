import os, asyncio
from openai import AsyncOpenAI
from openai.types.responses import ResponseTextDeltaEvent

api_key = "sk-proj-xx"

client = AsyncOpenAI(api_key=api_key)

tools =[
            {
                "type": "mcp",
                "server_url": "https://ad8a8f6657c9.ngrok-free.app/sse/sse",
                "server_label": "KOKO-Store",
                "require_approval": "never",
            }
        ]

async def main():
    response = await client.responses.create(
        model="gpt-4.1",
        input=[
            {
                "role": "user",
                "content": [
                    {"type": "input_text", "text": "茶葉蛋還有多少庫存？"}
                ],
            }
        ],
        tools=tools,
        temperature=0.3,
    )

    print(response.output_text)
    
    stream = await client.responses.create(
        model="gpt-4.1",
        input=[
            {
                "role": "user",
                "content":[
                    {"type":"input_text","text":"咖啡還有嗎？"}
                ]
            }
        ],
        tools=tools,
        stream=True,
    )
    async for chunk in stream:
        if isinstance(chunk, ResponseTextDeltaEvent):
            print(chunk.delta, end="", flush=True)

asyncio.run(main())
