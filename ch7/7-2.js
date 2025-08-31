import { z } from "zod";
import { createErrorResponse, createSuccessResponse, } from "../common/response.js";
import { AbstractTool } from "./AbstractTool.js";
import { NO_USER_ID_ERROR } from "../common/schema/constants.js";
import { textMessageSchema } from "../common/schema/textMessage.js";
export default class PushTextMessage extends AbstractTool {
    client;
    destinationId;
    constructor(client, destinationId) {
        super();
        this.client = client;
        this.destinationId = destinationId;
    }
    register(server) {
        const userIdSchema = z
            .string()
            .default(this.destinationId)
            .describe("The user ID to receive a message. Defaults to DESTINATION_USER_ID.");
        server.tool("push_text_message", "Push a simple text message to a user via LINE. Use this for sending plain text messages without formatting.", {
            userId: userIdSchema,
            message: textMessageSchema,
        }, async ({ userId, message }) => {
            if (!userId) {
                return createErrorResponse(NO_USER_ID_ERROR);
            }
            try {
                const response = await this.client.pushMessage({
                    to: userId,
                    messages: [message],
                });
                return createSuccessResponse(response);
            }
            catch (error) {
                return createErrorResponse(`Failed to push message: ${error.message}`);
            }
        });
    }
}
//# sourceMappingURL=pushTextMessage.js.map