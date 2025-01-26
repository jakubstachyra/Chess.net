// src/app/services/signalrClient.ts

import { HubConnection, HubConnectionBuilder, LogLevel } from "@microsoft/signalr";
import { Handlers } from "types/handlers"; // Adjust the path as necessary

// Initialize the connection variable
let connection: HubConnection | null = null;

// Map to store multiple handlers per event
const eventHandlers: { [eventName: string]: Function[] } = {};

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
    const SIGNALR_URL = process.env.NEXT_PUBLIC_SIGNALR_URL || "https://localhost:7078/gamehub";

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
      if (eventName && handlerFn) {
        // Initialize the array if not present
        if (!eventHandlers[eventName]) {
          eventHandlers[eventName] = [];
          connection?.on(eventName, (...args: any[]) => {
            eventHandlers[eventName].forEach((fn) => fn(...args));
          });
        }

        // Add the handler to the array
        eventHandlers[eventName].push(handlerFn as (...args: any[]) => void);
      }
    });
  }

  return connection;
}

/**
 * Removes a specific handler for a given event.
 *
 * @param {string} eventName - The name of the event.
 * @param {Function} handlerFn - The handler function to remove.
 */
export function removeHandler(eventName: string, handlerFn: Function) {
  if (eventHandlers[eventName]) {
    eventHandlers[eventName] = eventHandlers[eventName].filter((fn) => fn !== handlerFn);
    if (eventHandlers[eventName].length === 0) {
      delete eventHandlers[eventName];
      connection?.off(eventName);
    }
  }
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
