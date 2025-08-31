#!/usr/bin/env node
import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { z } from "zod";
import { StdioServerTransport } from "@modelcontextprotocol/sdk/server/stdio.js";
import NodeGeocoder from "node-geocoder";
import fetch from "node-fetch";

const server = new McpServer({
  name: "Weather MCP",
  version: "1.0.0"
});

const options = {
  provider: "openstreetmap",
};
const geocoder = NodeGeocoder(options);

server.tool(
  "get_weather_forecast",
  "Retrieves the weather using Open-Meteo API for a given location (city) and a date (yyyy-mm-dd). Returns a list dictionary with the time and temperature for each hour.",
 { 
    location: z
      .string()
      .describe("The city and state, e.g., San Francisco, CA"),
    date: z
      .string()
      .describe(
        "the forecasting date for when to get the weather format (yyyy-mm-dd)"
      ),
  },
  async ({ location, date }) => {
    try {
      const geoResult = await geocoder.geocode(location);
      if (!geoResult || geoResult.length === 0) {
        return {
          content: [
            { type: "text", text: JSON.stringify({ error: "Location not found" }) },
          ],
        };
      }

      const { latitude, longitude } = geoResult[0];

      const apiUrl = `https://api.open-meteo.com/v1/forecast?latitude=${latitude}&longitude=${longitude}&hourly=temperature_2m&start_date=${date}&end_date=${date}`;
      const response = await fetch(apiUrl);

      if (!response.ok) {
         throw new Error(`HTTP error! status: ${response.status}`);
      }

      const data = await response.json();

      if (!data.hourly || !data.hourly.time || !data.hourly.temperature_2m) {
         return {
           content: [
             { type: "text", text: JSON.stringify({ error: "Weather data format incorrect" }) },
           ],
         };
      }

      const weatherForecast = {};
      data.hourly.time.forEach((time, index) => {
        weatherForecast[time] = data.hourly.temperature_2m[index];
      });

      return {
        content: [{ type: "text", text: JSON.stringify(weatherForecast) }],
      };
    } catch (error) {
      console.error("Error fetching weather:", error);
      return {
        content: [
          { type: "text", text: JSON.stringify({ error: error.message }) },
        ],
      };
    }
  }
);

const transport = new StdioServerTransport();
await server.connect(transport);
