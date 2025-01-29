// src/app/services/signalrClient.ts

import { HubConnection, HubConnectionBuilder, LogLevel } from "@microsoft/signalr";
import { Handlers } from "types/handlers"; // Adjust the path as necessary

// Initialize the connection variable
let connection: HubConnection | null = null;

/**
 * Establishes and manages a SignalR Hub connection.
 *
 * @template T - The type of handlers extending the Handlers interface.
 * @param {Partial<T>} handlers - An object containing event handler functions.
 * @returns {Promise<HubConnection>} - The established HubConnection instance.
 */
export async function getConnection<T extends Handlers>(
  handlers?: Partial<T>
): Promise<HubConnection> {
  if (!connection) {
    const SIGNALR_URL = process.env.NEXT_PUBLIC_SIGNALR_URL || "http://localhost:5000/gamehub/";

    connection = new HubConnectionBuilder()
      .withUrl(SIGNALR_URL)
      .configureLogging(LogLevel.Information)
      .withAutomaticReconnect() // Added automatic reconnection
      .build();

    try {
      await connection.start();
      console.log("SignalR connection started, ID:", connection.connectionId);
    } catch (error: unknown) {
      console.error("Failed to start SignalR connection:", error);
      throw error; // Rethrow to let the caller handle it
    }
  }

  if (handlers) {
    Object.entries(handlers).forEach(([eventName, handlerFn]) => {
      // Ensure eventName is a key of T
      if (eventName in handlers && handlerFn) {
        connection?.off(eventName);
        connection?.on(eventName, handlerFn as (...args: Handlers[]) => void);
      }
    });
  }

  return connection;
}

/**
 * Stops the SignalR Hub connection if it's active.
 */
export async function stopConnection(): Promise<void> {
  if (connection && connection.state === "Connected") {
    try {
      await connection.stop();
      console.log("SignalR connection stopped.");
      connection = null;
    } catch (error: unknown) {
      console.error("Error stopping SignalR connection:", error);
    }
  }
}
