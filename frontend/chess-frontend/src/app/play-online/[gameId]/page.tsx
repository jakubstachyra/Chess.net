"use client";

import React, { useEffect, useState } from "react";
import ChessboardComponent from "../../components/chessBoard/chessBoard";
import { useParams } from "next/navigation";

import { connectToHub } from "../../services/signalrClient";
import {
  fetchFen,
  fetchMoves,
  fetchWhoToMove,
  sendMove,
} from "../../services/gameService";

const ChessboardOnline = () => {
  const [position, setPosition] = useState("start");
  const [customSquareStyles, setCustomSquareStyles] = useState({});
  const [mappedMoves, setMappedMoves] = useState({});
  const [whoToMove, setWhoToMove] = useState(null);
  const [playerColor, setPlayerColor] = useState(null);
  const [isGameReady, setIsGameReady] = useState(false);
  const [connection, setConnection] = useState(null);

  const { gameId } = useParams();

  useEffect(() => {
    if (!gameId) {
      console.error("Game ID is required to join a game.");
      return;
    }

    const initHub = async () => {
      try {
        const handlers = {
          GameState: (color) => {
            setPlayerColor(color);
            setIsGameReady(true);
          },
          PlayerDisconnected: () => {
            alert("Opponent disconnected. The game is over.");
            setIsGameReady(false);
          },
          OpponentMoved: async () => {
            await refreshGameState();
          },
        };

        const hub = await connectToHub(
          "https://localhost:7078/gamehub",
          handlers
        );
        setConnection(hub);

        if (hub) {
          // Fetch initial game state
          await hub.invoke("GetGameState");
        }
      } catch (error) {
        console.error("Error connecting to hub:", error);
      }
    };

    initHub();
    refreshGameState();
  }, [gameId]);

  const refreshGameState = async () => {
    try {
      const fenResponse = await fetchFen(gameId);
      setPosition(fenResponse.data);

      const whoToMoveResponse = await fetchWhoToMove(gameId);
      setWhoToMove(whoToMoveResponse.data);

      const movesResponse = await fetchMoves(gameId);
      const movesMapping = mapMoves(movesResponse.data);
      setMappedMoves(movesMapping);
    } catch (error) {
      console.error("Error refreshing game state:", error);
    }
  };

  const mapMoves = (moves) => {
    const movesMapping = {};
    moves.forEach((move) => {
      const [source, target] = move.split(" ");
      if (!movesMapping[source]) movesMapping[source] = [];
      movesMapping[source].push(target);
    });
    return movesMapping;
  };

  const makeMove = async (sourceSquare, targetSquare) => {
    try {
      const move = `${sourceSquare} ${targetSquare}`;
      setCustomSquareStyles({});

      // Send move to the server
      await sendMove(gameId, move);

      // Notify the server of the move
      if (connection) {
        await connection.invoke("YourMove");
      }

      // Refresh the game state
      await refreshGameState();
    } catch (error) {
      console.error("Error making move:", error);
    }
  };

  const onSquareClick = (square) => {
    const moves = mappedMoves[square] || [];
    const styles = moves.reduce((acc, target) => {
      acc[target] = {
        backgroundColor: "rgba(0, 255, 0, 0.5)",
        borderRadius: "50%",
      };
      return acc;
    }, {});
    setCustomSquareStyles(styles);
  };

  const onDrop = async (sourceSquare, targetSquare) => {
    // Check if it's this player's turn
    if (
      (whoToMove === 0 && playerColor === "white") ||
      (whoToMove === 1 && playerColor === "black")
    ) {
      const possibleMoves = mappedMoves[sourceSquare];
      if (possibleMoves?.includes(targetSquare)) {
        await makeMove(sourceSquare, targetSquare);
        return true;
      }
    } else {
      alert("It's not your turn!");
    }
    return false;
  };

  return (
    <div>
      <h2>
        {isGameReady
          ? `You are playing as ${playerColor}`
          : "Waiting for an opponent..."}
      </h2>
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
