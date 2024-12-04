"use client";

import React, { useEffect, useState } from "react";
import { HubConnectionBuilder, LogLevel } from "@microsoft/signalr";
import ChessboardComponent from "../components/chessBoard/chessBoard";
import { Square } from "react-chessboard/dist/chessboard/types";

const ChessboardOnline = () => {
  const [possibleMoves, setPossibleMoves] = useState([]);
  const [customSquareStyles, setCustomSquareStyles] = useState<{
    [key: string]: React.CSSProperties;
  }>({});
  const [mappedMoves, setMappedMoves] = useState<{ [key: string]: string[] }>(
    {}
  );
  const [position, setPosition] = useState("start");
  const [whoToMove, setWhoToMove] = useState(0); // 0 for white, 1 for black
  const [isPositionLoaded, setIsPositionLoaded] = useState(false);

  const [connection, setConnection] = useState(null);
  const [playerColor, setPlayerColor] = useState<string | null>(null); // "white" or "black"

  const [isGameReady, setIsGameReady] = useState(false);

  // Inicjalizacja SignalR
  useEffect(() => {
    const connectToHub = async () => {
      const hubConnection = new HubConnectionBuilder()
        .withUrl("https://localhost:7078/gamehub")
        .configureLogging(LogLevel.Information)
        .withAutomaticReconnect()
        .build();

      try {
        await hubConnection.start();
        console.log("Connected to SignalR");

        hubConnection.on("AssignPlayerColor", (color: string) => {
          setPlayerColor(color);
          console.log(`Assigned color: ${color}`);
        });

        hubConnection.on("GameReady", () => {
          setIsGameReady(true);
          console.log("Game is ready");
        });

        // Obsługa odłączenia gracza
        hubConnection.on("PlayerDisconnected", () => {
          alert("Your opponent has disconnected.");
          setIsGameReady(false);
        });

        // Add handler for OpponentMoved
        hubConnection.on("OpponentMoved", async () => {
          console.log("Opponent moved. Fetching updated position...");
          await getFenFromApi(); // Fetch the updated FEN position from the API
          await whoToMoveFromApi(); // Update turn information
        });

        setConnection(hubConnection);
      } catch (err) {
        console.error("Error connecting to SignalR:", err);
      }
    };

    connectToHub();
  }, []);

  // Pobieranie dostępnych ruchów z API
  useEffect(() => {
    const fetchData = async () => {
      try {
        const response = await fetch("https://localhost:7078/moves", {
          method: "GET",
          headers: {
            "Content-Type": "application/json",
          },
        });
        const data: string[] = await response.json();

        const movesMapping: { [key: string]: string[] } = {};

        data.forEach((move) => {
          const [source, target] = move.split(" ");
          if (!movesMapping[source]) {
            movesMapping[source] = [];
          }
          movesMapping[source].push(target);
        });

        setMappedMoves(movesMapping);
      } catch (error) {
        console.error("Error fetching moves:", error);
      }
    };
    fetchData();
  }, [position]);

  async function sendMoveToAPI(move: string) {
    try {
      const response = await fetch("https://localhost:7078/ReceiveMove", {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify(move),
      });

      if (!response.ok) {
        console.error("Failed to send move to API");
      } else {
        console.log("Move successfully sent to API:", move);
      }
    } catch (error) {
      console.error("Error sending move to API:", error);
    }
  }

  async function getFenFromApi() {
    try {
      const response = await fetch("https://localhost:7078/Fen", {
        method: "GET",
        headers: {
          "Content-Type": "application/json",
        },
      });

      if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`);
      }

      const fen = await response.text();
      setPosition(fen);
      setIsPositionLoaded(true); // Mark position as loaded
    } catch (err) {
      console.error("Błąd podczas pobierania pozycji:", err);
    }
  }

  async function whoToMoveFromApi() {
    try {
      const response = await fetch("https://localhost:7078/WhoToMove", {
        method: "GET",
        headers: {
          "Content-Type": "application/json",
        },
      });

      const data = await response.json();
      setWhoToMove(data); // 0 for white, 1 for black
    } catch (error) {
      console.error("Error fetching turn info:", error);
    }
  }

  async function makeMove(sourceSquare: Square, targetSquare: Square) {
    const move = `${sourceSquare} ${targetSquare}`;
    setCustomSquareStyles([]);
    console.log(position);
    await sendMoveToAPI(move);
    await getFenFromApi();
    await whoToMoveFromApi();
  }

  async function onSquareClick(square: Square) {
    console.log({ square });
    console.log(mappedMoves[square]);
    const moves = mappedMoves[square.toString()] || [];

    const newStyles: { [key: string]: React.CSSProperties } = {};

    moves.forEach((target) => {
      newStyles[target] = {
        backgroundColor: "rgba(0, 255, 0, 0.5)",
        borderRadius: "50%",
      };
    });
    setCustomSquareStyles(newStyles);
  }

  function onDrop(sourceSquare: Square, targetSquare: Square) {
    const possibleMovesFromSource = mappedMoves[sourceSquare.toString()];

    if (!possibleMovesFromSource) {
      return false;
    }
    if (possibleMovesFromSource.includes(targetSquare.toString())) {
      makeMove(sourceSquare, targetSquare);
      return true;
    }
  }

  useEffect(() => {
    whoToMoveFromApi();
    if (connection) {
      connection
        .invoke("YourMove")
        .catch((err: any) => console.error("Error sending YourMove:", err));
    }
  }, [whoToMove]);

  if (!isGameReady) {
    return <div>Waiting for opponent...</div>;
  }

  return (
    <div>
      <h2>You are playing as {playerColor}</h2>
      <ChessboardComponent
        position={position}
        onSquareClick={onSquareClick}
        customSquareStyles={customSquareStyles}
        onPieceDrop={onDrop}
      />
    </div>
  );
};

export default ChessboardOnline;
