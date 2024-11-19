"use client";

import { useEffect, useState } from "react";
import * as signalR from "@microsoft/signalr";

export default function ChessGame() {
  const [messages, setMessages] = useState([]);
  const [clientId, setClientId] = useState("");
  const [opponentId, setOpponentId] = useState("");
  const [isWaiting, setIsWaiting] = useState(true);

  useEffect(() => {
    // Establish connection to SignalR hub
    const connection = new signalR.HubConnectionBuilder()
      .withUrl("https://localhost:7078/chessHub") // Adjust URL to your SignalR Hub
      .withAutomaticReconnect()
      .build();

    connection.start().then(() => {
      console.log("Connected to SignalR hub");
    });

    // Listen for client ID assignment
    connection.on("AssignClientId", (id) => {
      setClientId(id);
    });

    // Listen for game state update
    connection.on("GameStateUpdate", (opponentId) => {
      setOpponentId(opponentId);
      setIsWaiting(false); // Opponent has connected
    });

    // Listen for incoming messages
    connection.on("ReceiveMessage", (message) => {
      setMessages((prevMessages) => [...prevMessages, message]);
    });

    return () => {
      connection.stop();
    };
  }, []);

  return (
    <div>
      <h1>Chess Game</h1>
      <p>Your Client ID: {clientId}</p>
      {isWaiting ? (
        <p>Waiting for opponent...</p>
      ) : (
        <div>
          <p>Opponent connected: {opponentId}</p>
          {/* Replace this with your chessboard component */}
          <h2>Chessboard</h2>
        </div>
      )}
      <div>
        <h2>Messages:</h2>
        <ul>
          {messages.map((msg, idx) => (
            <li key={idx}>{msg}</li>
          ))}
        </ul>
      </div>
    </div>
  );
}
