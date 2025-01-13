"use client";

import React, { useEffect, useState } from "react";
import { useRouter, useSearchParams } from "next/navigation";
import { connectToHub } from "../services/signalrClient";

const QueuePage = () => {
  const [playersInQueue, setPlayersInQueue] = useState(0);
  const [connection, setConnection] = useState(null);
  const router = useRouter();
  const searchParams = useSearchParams(); // Hook to get query parameters

  // Extract query parameters
  const mode = searchParams.get("mode") || "unknown";
  const timerRaw = searchParams.get("timer") || "No Timer";

  // Convert timer to seconds
  const timer = (() => {
    if (timerRaw === "No Timer") return 0;
    const [value, unit] = timerRaw.split(" ");
    const multiplier = unit === "min" ? 60 : 1; // Assume minutes, adjust as needed
    return parseInt(value) * multiplier;
  })();

  useEffect(() => {
    console.log("Game mode:", mode);
    console.log("Timer (seconds):", timer);

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

        // Invoke the FindOpponent function on load with timer in seconds
        await hub.invoke("FindOpponent", hub.connectionId, mode, timer);

        return () => {
          hub?.stop();
        };
      } catch (error) {
        console.error("Failed to connect to SignalR Hub:", error);
        alert("Connection error. Please refresh the page.");
      }
    };

    initHub();
  }, [mode, timer]); // Dependency array to ensure these values are logged on change

  const leaveQueue = async () => {
    if (connection) {
      await connection.invoke("RemovePlayerFromQueue", connection.connectionId); // Use the connectionId
      alert("You left the queue.");
      setPlayersInQueue(0);
    }
  };

  return (
    <div>z
      <h1>Waiting in Queue</h1>
      <p>Game Mode: {mode}</p>
      <p>Timer (seconds): {timer}</p>
      <p>Players in queue: {playersInQueue}</p>
      <button onClick={leaveQueue}>Leave Queue</button>
    </div>
  );
};

export default QueuePage;
