"use client";

import React, { useEffect, useState } from "react";
import { useRouter } from "next/navigation";

import { connectToHub } from "../services/signalrClient";

const QueuePage = () => {
  const [playersInQueue, setPlayersInQueue] = useState(0);
  const [connection, setConnection] = useState(null);
  const router = useRouter();

  useEffect(() => {
    const initHub = async () => {
      try {
        const handlers = {
          WaitingForOpponent: (playerCount) => setPlayersInQueue(playerCount),
          GameReady: (gameId) => {
            router.push(`/play-online/${gameId}`);
          },
          Error: (message) => alert(message),
        };

        const hub = await connectToHub(
          "https://localhost:7078/gamehub",
          handlers
        );
        setConnection(hub);

        // Invoke the FindOpponent function on load
        await hub.invoke("FindOpponent", hub.connectionId);

        return () => {
          hub?.stop();
        };
      } catch (error) {
        console.error("Failed to connect to SignalR Hub:", error);
        alert("Connection error. Please refresh the page.");
      }
    };

    initHub();
  }, []);

  const leaveQueue = async () => {
    if (connection) {
      await connection.invoke("RemovePlayerFromQueue", connection.connectionId);
      alert("You left the queue.");
      setPlayersInQueue(0);
    }
  };

  return (
    <div>
      <h1>Waiting in Queue</h1>
      <p>Players in queue: {playersInQueue}</p>
      <button onClick={leaveQueue}>Leave Queue</button>
    </div>
  );
};

export default QueuePage;
