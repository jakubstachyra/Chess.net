"use client";

import React, { useEffect, useState } from "react";
import { Chess } from "chess.js";
import { Chessboard } from "react-chessboard";
import * as signalR from "@microsoft/signalr";

const ChessGame = () => {
  const [game, setGame] = useState(new Chess());
  const [highlightSquares, setHighlightSquares] = useState({});
  const [playerColor, setPlayerColor] = useState(null);
  const [opponentConnected, setOpponentConnected] = useState(false);
  const [connection, setConnection] = useState(null);
  const [possibleMoves, setPossibleMoves] = useState([]);
  const [currentTurn, setCurrentTurn] = useState("white"); // State to track the turn

  useEffect(() => {
    const connectSignalR = async () => {
      const newConnection = new signalR.HubConnectionBuilder()
        .withUrl("https://localhost:7078/gamehub")
        .withAutomaticReconnect()
        .build();

      newConnection.on("AssignPlayerColor", (color) => {
        setPlayerColor(color);
        console.log(`Assigned color: ${color}`);
      });

      newConnection.on("GameReady", () => {
        setOpponentConnected(true);
        console.log("Opponent connected. Game ready!");
      });

      newConnection.on("OpponentMoved", (move) => {
        console.log("Opponent moved:", move);
        const [from, to] = move.split(" ");
        makeAMove(from, to);
        toggleTurn(); // Toggle the turn when an opponent moves
      });

      newConnection.on("PlayerDisconnected", () => {
        alert("Your opponent has disconnected.");
        setOpponentConnected(false);
      });

      try {
        await newConnection.start();
        console.log("SignalR Connected!");
      } catch (error) {
        console.error("SignalR Connection Error:", error);
      }

      setConnection(newConnection);
    };

    connectSignalR();

    // Fetch initial turn when the component mounts
    fetchWhoToMove();

    return () => {
      if (connection) {
        connection.stop();
      }
    };
  }, []);

  async function fetchWhoToMove() {
    try {
      const response = await fetch("https://localhost:7078/WhoToMove", {
        method: "GET",
        headers: {
          "Content-Type": "application/json",
        },
      });

      if (response.ok) {
        const data = await response.text();
        setCurrentTurn(data === "1" ? "black" : "white");
        console.log(
          "Initial turn fetched from API:",
          data === "1" ? "black" : "white"
        );
      } else {
        console.error("Failed to fetch who to move.");
      }
    } catch (error) {
      console.error("Error fetching who to move:", error);
    }
  }

  async function fetchMovesForSquare(square) {
    try {
      const response = await fetch(`https://localhost:7078/moves`, {
        method: "GET",
        headers: {
          "Content-Type": "application/json",
        },
      });
      const data = await response.json();
      const movesForSquare = data.filter((move) => move.startsWith(square));
      return movesForSquare;
    } catch (error) {
      console.error("Error fetching moves:", error);
      return [];
    }
  }

  async function onSquareClick(square) {
    const moves = await fetchMovesForSquare(square);
    const newHighlights = {};
    moves.forEach((move) => {
      const targetSquare = move.split(" ")[1];
      newHighlights[targetSquare] = true;
    });
    setPossibleMoves(moves);
    setHighlightSquares(newHighlights);
  }

  function makeAMove(from, to) {
    const piece = game.get(from);
    game.remove(from);
    game.put(piece, to);
    setGame(new Chess(game.fen())); // Update the game state
    return true;
  }

  async function sendMoveToServer(move) {
    try {
      if (connection) {
        await connection.invoke("NotifyMove", move);
      }
      console.log("Move sent to opponent:", move);
    } catch (error) {
      console.error("Error sending move to opponent:", error);
    }
  }

  async function sendMoveToAPI(move) {
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

  function toggleTurn() {
    setCurrentTurn((prevTurn) => (prevTurn === "white" ? "black" : "white"));
  }

  async function onDrop(sourceSquare, targetSquare) {
    if (!opponentConnected) {
      alert("Waiting for opponent...");
      return false;
    }

    const move = `${sourceSquare} ${targetSquare}`;

    if (
      possibleMoves.includes(move) &&
      playerColor === (currentTurn === "white" ? "white" : "black")
    ) {
      makeAMove(sourceSquare, targetSquare);
      setHighlightSquares({}); // Clear highlights

      // Send move to opponent
      await sendMoveToServer(move);

      // Send move to API
      await sendMoveToAPI(move);

      // Toggle the turn locally
      toggleTurn();

      return true;
    }

    return false;
  }

  return (
    <div style={{ textAlign: "center" }}>
      <h2>Chess Game</h2>
      {playerColor && (
        <p>
          You are playing as <strong>{playerColor}</strong>
        </p>
      )}
      {!opponentConnected && <p>Waiting for opponent...</p>}
      {opponentConnected && <p>Opponent connected. Game in progress!</p>}
      <p>
        <strong>Current Turn:</strong>{" "}
        {currentTurn === "white" ? "White" : "Black"}
      </p>
      <Chessboard
        id="PlayerVsPlayerBoard"
        boardWidth={600}
        position={game.fen()}
        onPieceDrop={onDrop}
        onSquareClick={onSquareClick}
        customSquareStyles={{
          ...Object.keys(highlightSquares).reduce((acc, square) => {
            acc[square] = { backgroundColor: "rgba(255, 255, 0, 0.4)" };
            return acc;
          }, {}),
        }}
      />
    </div>
  );
};

export default ChessGame;
