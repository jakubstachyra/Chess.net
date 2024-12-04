import React, { useState } from "react";
import { HubConnectionBuilder } from "@microsoft/signalr";

const GameClient = () => {
  const [status, setStatus] = useState("Idle");
  const [connection, setConnection] = useState(null);

  const handlePlay = async () => {
    if (!connection) {
      const newConnection = new HubConnectionBuilder()
        .withUrl("https://localhost:5001/gamehub") // Replace with your backend URL
        .build();

      newConnection.on("WaitingForOpponent", () => {
        setStatus("Waiting for opponent...");
      });

      newConnection.on("OpponentFound", (opponentId) => {
        setStatus(`Opponent found! Opponent ID: ${opponentId}`);
      });

      await newConnection.start();
      setConnection(newConnection);

      // Request to find a match
      newConnection.invoke("FindMatch");
    } else {
      // Request to find a match if connection is already established
      connection.invoke("FindMatch");
    }
  };

  return (
    <div>
      <h1>Game Client</h1>
      <p>Status: {status}</p>
      <button onClick={handlePlay}>Play</button>
    </div>
  );
};

export default GameClient;
