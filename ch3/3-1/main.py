from typing import List, Dict
from mcp.server.fastmcp import FastMCP
from fastapi import FastAPI
import uvicorn, os

# ---------- 庫存資料 ----------
inventory: Dict[str, int] = {
    "咖啡": 42,
    "茶葉蛋": 18,
    "洋芋片": 30,
    "牛奶": 25
}

# ---------- MCP Server ----------
mcp = FastMCP("KOKO-Store")

@mcp.tool(name="search", description="依關鍵字搜尋商品並提供摘要")
def search(query: str, limit: int = 10) -> List[Dict]:
    results = []
    query = query.lower()
    for product, qty in inventory.items():
        if query in product.lower() and len(results) < limit:
            results.append({
                "id": product,
                "title": product,
                "snippet": f"{product} 庫存 {qty} 件"
            })
    return results

@mcp.tool(name="fetch", description="依 ID 取回商品完整庫存資訊")
def fetch(ids: List[str]) -> List[Dict]:
    docs = []
    for _id in ids:
        if _id in inventory:
            docs.append({
                "id": _id,
                "text": f"{_id} 目前庫存 {inventory[_id]} 件"
            })
    return docs

# ---------- FastAPI ----------

app = FastAPI(title="KOKO便利商店",lifespan=lambda app: mcp.session_manager.run())

app.mount("/mcp", mcp.streamable_http_app())
app.mount("/sse", mcp.sse_app())

@app.get("/")
def index():
    return {"message": "歡迎來到 KOKO 便利商店 的 MCP 服務！"}

if __name__ == "__main__":
    uvicorn.run(app, host="0.0.0.0", port=8000, log_level="info")