"use client"
import React, { useEffect, useState } from "react";
import ChessboardComponent from "../../components/chessBoard/chessBoard";
import { useParams } from "next/navigation";
import MoveHistory from "../../components/MoveHistory/moveHistory";
import MoveNavigation from "../../components/MoveNavigation/moveNavigation";

import {
  fetchFen,
  fetchMoves,
  fetchWhoToMove,
  sendMove,
  fetchComputerMove,
  fetchGameState,
} from "../../services/gameService";
import BackgroundUI from "app/components/backgroundUI/pages";

const ChessboardComponentComputer = () => {
  const [customSquareStyles, setCustomSquareStyles] = useState({});
  const [mappedMoves, setMappedMoves] = useState({});
  const [position, setPosition] = useState("start");
  const [whoToMove, setWhoToMove] = useState(0);
  const [moveHistory, setMoveHistory] = useState([]);
  const [isPositionLoaded, setIsPositionLoaded] = useState(false);
  const [gameEnded, setGameEnded] = useState(false);
  const [gameResult, setGameResult] = useState("");

  const [navigationMode, setNavigationMode] = useState(false);
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
  
      setMoveHistory((prev) => [
        ...prev,
        {
          move: targetSquare, 
          fen: null,          // FEN zostanie uzupełniony później
        },
      ]);
  
      await refreshGameState();
    } catch (error) {
      console.error("Error making move:", error);
    }
  };

  const refreshGameState = async () => {
    try {
      const fenResponse = await fetchFen(gameId);
      const newFen = fenResponse.data;
  
      const whoToMoveResponse = await fetchWhoToMove(gameId);
      const newWhoToMove = whoToMoveResponse.data;
  
      setPosition(newFen);
      setWhoToMove(newWhoToMove);
  
      // Uzupełnij ostatni ruch o notację FEN
      setMoveHistory((prev) => {
        if (prev.length === 0) return prev; // Jeśli nie ma ruchów, nic nie rób
        const updatedHistory = [...prev];
        updatedHistory[updatedHistory.length - 1].fen = newFen;
        return updatedHistory;
      });
  
      if (newWhoToMove !== color) {
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
    <h1 style={{color: "white"}}>Computer</h1>
    <div style={containerStyles}>
      <div style={chessboardContainerStyles}>
        
        <div>
        <ChessboardComponent
          position={position}
          onSquareClick={onSquareClick}
          customSquareStyles={customSquareStyles}
          onPieceDrop={onDrop}
          boardOrientation={"white"} // do poprawy jeśli można by grać z komputerem czarnymi
          isDraggablePiece={() => !navigationMode}
          onPromotionPieceSelect={(piece, from, to) => makeMove(from, to, piece)}
        />
        </div>
      </div>
      <div style={modalContainerStyles}>
      <BackgroundUI>
        <h1>Moves</h1>
        <MoveHistory moveHistory={moveHistory} />
        <MoveNavigation moveHistory={moveHistory} setPosition={setPosition} setNavigationMode={setNavigationMode}/>
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

export default ChessboardComponentComputer;


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
const movesContainerStyles = {
  display: "flex",
  flexDirection: "column",
  alignItems: "flex-start",
  gap: "5px",
  width: "100%",
  height: "400px",
  overflowY: "scroll",
  padding: "10px",
  border: "1px solid rgba(255, 255, 255, 0.3)",
};

const moveRowStyles = {
  display: "flex",
  alignItems: "center",
  gap: "10px",
};

const moveNumberStyles = {
  color: "white",
  fontWeight: "bold",
};

const moveStyles = {
  color: "white",
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