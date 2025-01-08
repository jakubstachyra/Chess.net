"use client";

import React, { useEffect, useState } from "react";
import ChessboardComponent from "../../components/chessBoard/chessBoard";
import { useParams } from "next/navigation";
import {
  fetchFen,
  fetchMoves,
  fetchWhoToMove,
  sendMove,
  fetchComputerMove,
  fetchGameState,
} from "../../services/gameService";

const ChessboardComponentOnline = () => {
  const [customSquareStyles, setCustomSquareStyles] = useState({});
  const [mappedMoves, setMappedMoves] = useState({});
  const [position, setPosition] = useState("start");
  const [isPositionLoaded, setIsPositionLoaded] = useState(false);
  const [gameEnded, setGameEnded] = useState(false);
  const [gameResult, setGameResult] = useState("");

  const { gameId } = useParams();
  const color = 0; // 0 for white, 1 for black

  useEffect(() => {
    loadInitialData();
  }, []);

  const loadInitialData = async () => {
    try {
      const fenResponse = await fetchFen(gameId);
      setPosition(fenResponse.data);
      setIsPositionLoaded(true);

      loadMoves();
      checkGameState();
    } catch (error) {
      console.error("Error loading initial data:", error);
    }
  };

  const checkGameState = async () => {
    try {
      const response = await fetchGameState(gameId);
      const isGameEnded = response.data;
      if (isGameEnded) {
        setGameEnded(true);
        setGameResult("Game Over!");
      }
      return isGameEnded;
    } catch (error) {
      console.error("Error checking game state:", error);
      return false;
    }
  };

  const onSquareClick = (square) => {
    const moves = mappedMoves[square] || [];
    const newStyles = moves.reduce((styles, target) => {
      styles[target] = {
        backgroundColor: "rgba(0, 255, 0, 0.5)",
        borderRadius: "50%",
      };
      return styles;
    }, {});
    setCustomSquareStyles(newStyles);
  };

  const onDrop = async (sourceSquare, targetSquare) => {
    const possibleMovesFromSource = mappedMoves[sourceSquare];
    if (possibleMovesFromSource?.includes(targetSquare)) {
      await makeMove(sourceSquare, targetSquare);
      return true;
    }
    return false;
  };

  const makeMove = async (sourceSquare, targetSquare, promotedPiece = null) => {
    try {
      let move;
      if (promotedPiece) {
        move = `${sourceSquare}${targetSquare}${promotedPiece}`;
      } else {
        move = `${sourceSquare}${targetSquare}`;
      }
      setCustomSquareStyles({});
      await sendMove(gameId, move);
      await refreshGameState();
    } catch (error) {
      console.error("Error making move:", error);
    }
  };

  const refreshGameState = async () => {
    try {
      const fenResponse = await fetchFen(gameId);
      setPosition(fenResponse.data);

      const isGameEnded = await checkGameState();
      if (isGameEnded) return;

      const whoToMoveResponse = await fetchWhoToMove(gameId);
      if (whoToMoveResponse.data !== color) {
        const computerMove = await fetchComputerMove(gameId);
        const [source, target] = computerMove.data.split(" ");
        await makeMove(source, target);
      } else {
        loadMoves();
      }
    } catch (error) {
      console.error("Error refreshing game state:", error);
    }
  };

  const loadMoves = async () => {
    try {
      const movesResponse = await fetchMoves(gameId);
      const movesMapping = {};

      movesResponse.data.forEach((move) => {
        const [source, target] = move.split(" ");
        if (!movesMapping[source]) movesMapping[source] = [];
        movesMapping[source].push(target);
      });
      setMappedMoves(movesMapping);
    } catch (error) {
      console.error("Error loading moves:", error);
    }
  };

  if (!isPositionLoaded) return <div>Loading...</div>;

  if (gameEnded) {
    alert(`Game Over: ${gameResult}`);
  }

  return (
    <div>
      <ChessboardComponent
        position={position}
        onSquareClick={onSquareClick}
        customSquareStyles={customSquareStyles}
        onPieceDrop={onDrop}
        onPromotionPieceSelect={(piece, from, to) => makeMove(from, to, piece)}
      />
    </div>
  );
};

export default ChessboardComponentOnline;
