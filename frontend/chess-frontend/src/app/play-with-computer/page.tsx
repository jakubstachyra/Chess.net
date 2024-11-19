"use client";

import React, { useState } from "react";
import { Chess } from "chess.js";
import { Chessboard } from "react-chessboard";

const ChessboardComponent = () => {
  const [game, setGame] = useState(new Chess());
  const [highlightSquares, setHighlightSquares] = useState({});
  const [possibleMoves, setPossibleMoves] = useState([]); // Store possible moves for the selected piece

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
    setGame(game);
    return true;
  }

  // Function to send white's move to the API
  async function sendWhiteMoveToAPI(move) {
    try {
      const response = await fetch("https://localhost:7078/ReceiveMove", {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify(move), // Send the move made by the player as a JSON object
      });

      if (!response.ok) {
        throw new Error("Failed to send white move");
      }

      console.log("White move sent:", move);
    } catch (error) {
      console.error("Error sending white move to API:", error);
    }
  }

  // Function to wait for and retrieve the black move from the API
  async function getBlackMoveFromAPI() {
    try {
      const response = await fetch("https://localhost:7078/GetBlackMove", {
        method: "GET",
        headers: {
          "Content-Type": "application/json",
        },
      });

      const blackMove = await response.text(); // Expecting plain text (e.g., "e7 e5")
      console.log("Black move received:", blackMove);
      return blackMove;
    } catch (error) {
      console.error("Error getting black move from API:", error);
      return null;
    }
  }

  // Function to handle piece drop event
  async function onDrop(sourceSquare, targetSquare) {
    const move = `${sourceSquare} ${targetSquare}`;

    // If the move exists in the list of possible moves, apply it
    if (possibleMoves.includes(move)) {
      makeAMove(sourceSquare, targetSquare);
      setHighlightSquares({}); // Clear highlights after the move is made

      // Send the white move to the API
      await sendWhiteMoveToAPI(move);

      // Get and apply the black move from the API
      const blackMove = await getBlackMoveFromAPI();
      if (blackMove) {
        const [from, to] = blackMove.split(" ");
        makeAMove(from, to);
      }

      return true; // Move is accepted
    }
    return false; // If move is not in the possible moves list, do nothing
  }

  return (
    <div>
      <Chessboard
        id="BasicBoard"
        boardWidth={600}
        position={game.fen()}
        onPieceDrop={onDrop}
        onSquareClick={onSquareClick} // Trigger fetching possible moves on square click
        customDarkSquareStyle={{ backgroundColor: "rgba(60,58,60,0.9)" }}
        customLightSquareStyle={{ backgroundColor: "rgba(230,230,230,0.8)" }}
        customSquareStyles={{
          ...Object.keys(highlightSquares).reduce((acc, square) => {
            acc[square] = {
              backgroundColor: "rgba(255, 255, 0, 0.4)",
            };
            return acc;
          }, {}),
        }}
      />
    </div>
  );
};

export default ChessboardComponent;
