import boto3
import json
import uuid
import pprint
import logging
# setting logger
logging.basicConfig(
    format="[%(asctime)s] p%(process)s {%(filename)s:%(lineno)d} %(levelname)s - %(message)s",
    level=logging.INFO,
)
logger = logging.getLogger(__name__)

# getting boto3 clients for required AWS services
sts_client = boto3.client("sts")
bedrock_agent_runtime_client = boto3.client("bedrock-agent-runtime")
session = boto3.session.Session()
region = session.region_name
account_id = sts_client.get_caller_identity()["Account"]
region, account_id


def invoke_agent_helper(
    query,
    session_id,
    agent_id,
    alias_id,
    enable_trace=False,
    memory_id=None,
    session_state=None,
    end_session=False,
    show_code_use=False,
):

    if not session_state:
        session_state = {}

    # invoke the agent API
    agent_response = bedrock_agent_runtime_client.invoke_agent(
        inputText=query,
        agentId=agent_id,
        agentAliasId=alias_id,
        sessionId=session_id,
        enableTrace=(
            enable_trace | show_code_use
        ),  # Force tracing on if showing code use
        endSession=end_session,
        memoryId=memory_id,
        sessionState=session_state,
    )
    return process_response(
        agent_response, enable_trace=enable_trace, show_code_use=show_code_use
    )


def process_response(resp, enable_trace: bool = False, show_code_use: bool = False):
    if enable_trace:
        logger.info(pprint.pprint(resp))

    event_stream = resp["completion"]
    try:
        for event in event_stream:
            if "chunk" in event:
                data = event["chunk"]["bytes"]
                if enable_trace:
                    logger.info(f"Final answer ->\n{data.decode('utf8')}")
                agent_answer = data.decode("utf8")
                return agent_answer
                # End event indicates that the request finished successfully
            elif "trace" in event:
                if "codeInterpreterInvocationInput" in json.dumps(event["trace"]):
                    if show_code_use:
                        print("Invoked code interpreter")
                if enable_trace:
                    logger.info(json.dumps(event["trace"], indent=2))
            else:
                raise Exception("unexpected event.", event)
    except Exception as e:
        raise Exception("unexpected event.", e)


# call exist agent

## create a random id for session initiator id
query = "How are you?"
memory_id = "TestMemoryId"
invoke_agent_helper(
    query=query,
    session_id=str(uuid.uuid1()),
    agent_id="xxxxxxxx",  # replace with your agent id
    alias_id="xxxxxxxx",  # replace with your alias id
    enable_trace=False,
    memory_id=memory_id,
    session_state=None,
    end_session=False,
    show_code_use=False,
)