"use client";

import React, { useEffect, useState } from "react";
import ChessboardComponent from "../../components/chessBoard/chessBoard";
import { Square } from "react-chessboard/dist/chessboard/types";
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
  const [whoToMove, setWhoToMove] = useState(0);
  const [playerColor, setPlayerColor] = useState(null);
  const [isGameReady, setIsGameReady] = useState(false);
  const [connection, setConnection] = useState(null);

  const { gameId } = useParams();

  useEffect(() => {
    const initHub = async () => {
      const handlers = {
        AssignPlayerColor: (color) => setPlayerColor(color),
        GameReady: () => setIsGameReady(true),
        PlayerDisconnected: () => {
          alert("Opponent disconnected");
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
    };

    initHub();
    refreshGameState();
  }, []);

  const refreshGameState = async () => {
    try {
      const fenResponse = await fetchFen(gameId);
      setPosition(fenResponse.data);

      const whoToMoveResponse = await fetchWhoToMove(gameId);
      setWhoToMove(whoToMoveResponse.data);

      const movesResponse = await fetchMoves(gameId);
      const movesMapping = {};
      movesResponse.data.forEach((move) => {
        const [source, target] = move.split(" ");
        if (!movesMapping[source]) movesMapping[source] = [];
        movesMapping[source].push(target);
      });
      setMappedMoves(movesMapping);
    } catch (error) {
      console.error("Error refreshing game state:", error);
    }
  };

  const makeMove = async (sourceSquare, targetSquare) => {
    const move = `${sourceSquare} ${targetSquare}`;
    setCustomSquareStyles({});
    await sendMove(move);
    await refreshGameState();
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
    const possibleMoves = mappedMoves[sourceSquare];
    if (possibleMoves?.includes(targetSquare)) {
      await makeMove(sourceSquare, targetSquare);
      return true;
    }
    return false;
  };

  if (!isGameReady) return <div>Waiting for opponent...</div>;

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
