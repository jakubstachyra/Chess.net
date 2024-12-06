"use client";

import React, { useEffect, useState } from "react";
import ChessboardComponent from "../../components/chessBoard/chessBoard";
import { Square } from "react-chessboard/dist/chessboard/types";
import { useParams } from "next/navigation";

const ChessboardComponentOnline = () => {
  const [possibleMoves, setPossibleMoves] = useState([]);
  const [customSquareStyles, setCustomSquareStyles] = useState<{
    [key: string]: React.CSSProperties;
  }>({});
  let color = 0; // 0 for white, 1 for black
  const [mappedMoves, setMappedMoves] = useState<{ [key: string]: string[] }>(
    {}
  );
  const [position, setPosition] = useState("start");
  const [whoToMove, setWhoToMove] = useState(0); // 0 for white, 1 for black
  const [isPositionLoaded, setIsPositionLoaded] = useState(false); // New state to check if FEN is loaded

  const { gameId } = useParams(); //

  useEffect(() => {
    getFenFromApi();
  }, []);

  useEffect(() => {
    const fetchData = async () => {
      try {
        console.log(gameId);
        const response = await fetch(`https://localhost:7078/moves/${gameId}`, {
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

  async function getComputerMoveFromApi() {
    try {
      const response = await fetch("https://localhost:7078/getBlackMove", {
        method: "GET",
        headers: {
          "Content-Type": "application/json",
        },
      });

      if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`);
      }

      const move = await response.text(); // Move will be in format "e2 e4"
      console.log("Computer move:", move);
      const [source, target] = move.split(" ");
      const sourceSquare = source as Square;
      const targetSquare = target as Square;

      // Update the position and send the move to API
      await makeMove(sourceSquare, targetSquare);
    } catch (err) {
      console.error("Error fetching computer move:", err);
    }
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

  async function makeMove(sourceSquare: Square, targetSquare: Square) {
    const move = `${sourceSquare} ${targetSquare}`;
    setCustomSquareStyles([]);
    console.log(position);
    await sendMoveToAPI(move);
    await getFenFromApi();
    await whoToMoveFromApi();
  }

  useEffect(() => {
    whoToMoveFromApi();
    if (whoToMove !== color) {
      // It's the computer's turn
      getComputerMoveFromApi();
    }
  }, [whoToMove]);

  if (!isPositionLoaded) {
    return <div>Loading...</div>; // Display loading message until FEN is fetched
  }

  return (
    <div>
      <ChessboardComponent
        position={position}
        onSquareClick={onSquareClick}
        customSquareStyles={customSquareStyles}
        onPieceDrop={onDrop}
      />
    </div>
  );
};

export default ChessboardComponentOnline;
