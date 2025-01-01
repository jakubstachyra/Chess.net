"use client";

import React, { useEffect, useState } from "react";
import ChessboardComponent from "../../components/chessBoard/chessBoard";
import { Square } from "react-chessboard/dist/chessboard/types";
import { useParams } from "next/navigation";
import {
  fetchFen,
  fetchMoves,
  fetchWhoToMove,
  sendMove,
  fetchComputerMove,
} from "../../services/gameService";
import BackgroundUI from "app/components/backgroundUI/pages";

const ChessboardComponentOnline = () => {
  const [customSquareStyles, setCustomSquareStyles] = useState({});
  const [mappedMoves, setMappedMoves] = useState({});
  const [position, setPosition] = useState("start");
  const [whoToMove, setWhoToMove] = useState(0);
  const [isPositionLoaded, setIsPositionLoaded] = useState(false);

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
    } catch (error) {
      console.error("Error loading initial data:", error);
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

  const makeMove = async (sourceSquare, targetSquare) => {
    try {
      const move = `${sourceSquare} ${targetSquare}`;
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

      const whoToMoveResponse = await fetchWhoToMove(gameId);
      setWhoToMove(whoToMoveResponse.data);

      if (whoToMoveResponse.data !== color) {
        const computerMove = await fetchComputerMove(gameId);
        const [source, target] = computerMove.data.split(" ");
        await makeMove(source, target);
      } else loadMoves();
    } catch (error) {
      console.error("Error refreshing game state:", error);
    }
  };
  async function loadMoves() {
    const movesResponse = await fetchMoves(gameId);
    const movesMapping = {};

    movesResponse.data.forEach((move) => {
      const [source, target] = move.split(" ");
      if (!movesMapping[source]) movesMapping[source] = [];
      movesMapping[source].push(target);
    });
    setMappedMoves(movesMapping);
  }
  if (!isPositionLoaded) return <div>Loading...</div>;

  return (
    <div>
    <h1>Computer</h1>
    <div style={containerStyles}>
      <div style={chessboardContainerStyles}>
        
        <div>
        <ChessboardComponent
          position={position}
          onSquareClick={onSquareClick}
          customSquareStyles={customSquareStyles}
          onPieceDrop={onDrop}
          boardOrientation={"white"} // do poprawy jeśli można by grać z komputerem czarnymi
          isDraggablePiece={() => true}
        />
        </div>
      </div>
      <div style={modalContainerStyles}>
      <BackgroundUI>
        <h1>Moves</h1>
        <h5>Here will be history in the future</h5>
        <div style={buttonsContainerStyles}>
          <button style={buttonStyle} title="Give up a game">
            Resign
          </button>
        </div>
    </BackgroundUI>

      </div>
    </div>
    </div>

  );
};

export default ChessboardComponentOnline;


const buttonsContainerStyles = {
  display: "flex",
  justifyContent: "space-between",
  gap: "10px",
  marginTop: "auto",
  width: "100%",
};
const containerStyles = {
  display: "flex",
  justifyContent: "flex-end",
  alignItems: "flex-start",
  padding: "20px",
  gap: "30px",
};

const chessboardContainerStyles = {
  display: "flex",
  alignItems: "center",
  justifyContent: "center",
};

const modalContainerStyles = {
  display: "flex",
  alignItems: "center",
  flexDirection: "column", 
  justifyContent: "space-between",
  height: "600px",
  width: "400px",
  borderRadius: "15px",
  backgroundColor: "rgba(255, 255, 255, 0.1)",
  boxShadow: "0 4px 15px rgba(0, 0, 0, 0.3)",
  backdropFilter: "blur(10px)", 
  color: "white"
};

const buttonStyle = {
  padding: "10px",
  fontSize: "16px",
  fontWeight: "bold",
  color: "#fff",
  backgroundColor: "#DD0000 ",
  border: "none",
  borderRadius: "5px",
  cursor: "pointer",
  boxShadow: "0 4px 30px rgba(0, 0, 0, 0.5)",
  width: "100%"
};