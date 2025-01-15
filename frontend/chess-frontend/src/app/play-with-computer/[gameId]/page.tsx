"use client";
import React, { useEffect, useState } from "react";
import { useParams } from "next/navigation";
import {
  fetchFen,
  fetchMoves,
  fetchWhoToMove,
  sendMove,
  fetchComputerMove,
  fetchGameState,
} from "../../services/gameService";
import { GameReviewContent } from "../../components/gameReview/gameReview";
import BackgroundUI from "app/components/backgroundUI/pages";
import { resign } from "../../services/gameService";

interface MoveHistoryEntry {
  moveNumber: number;
  fen: string;
  move: string;
  whiteRemainingTimeMs: number | null;
  blackRemainingTimeMs: number | null;
}

const ChessboardComponentComputer = () => {
  const [customSquareStyles, setCustomSquareStyles] = useState({});
  const [mappedMoves, setMappedMoves] = useState({});
  const [position, setPosition] = useState("start");
  const [whoToMove, setWhoToMove] = useState(0);
  const [moveHistory, setMoveHistory] = useState<MoveHistoryEntry[]>([]);
  const [currentMoveIndex, setCurrentMoveIndex] = useState<number>(-1);
  const [isPositionLoaded, setIsPositionLoaded] = useState(false);
  const [gameEnded, setGameEnded] = useState(false);
  const [gameResult, setGameResult] = useState("");

  const [navigationMode, setNavigationMode] = useState(false);
  const { gameId } = useParams();

  const color = 0; // 0 for white, 1 for black

  useEffect(() => {
    loadInitialData();
  }, [gameId]);

  const loadInitialData = async () => {
    try {
      const fenResponse = await fetchFen(gameId);
      const initialFen = fenResponse.data;
      setPosition(initialFen);
      setIsPositionLoaded(true);
  
      // Dodaj początkową pozycję do historii ruchów jako pierwszy wpis
      setMoveHistory([
        {
          moveNumber: 0,
          fen: initialFen,
          move: "Initial",
          whiteRemainingTimeMs: null,
          blackRemainingTimeMs: null,
        },
      ]);
      setCurrentMoveIndex(0);
  
      await loadMoves();
      await checkGameState();
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

  const onSquareClick = (square: string) => {
    const moves = mappedMoves[square] || [];
    const newStyles = moves.reduce((styles: any, target: string) => {
      styles[target] = {
        backgroundColor: "rgba(0, 255, 0, 0.5)",
        borderRadius: "50%",
      };
      return styles;
    }, {});
    setCustomSquareStyles(newStyles);
  };

  const onDrop = async (sourceSquare: string, targetSquare: string) => {
    const possibleMovesFromSource = mappedMoves[sourceSquare];
    if (possibleMovesFromSource?.includes(targetSquare)) {
      await makeMove(sourceSquare, targetSquare);
      return true;
    }
    return false;
  };

  const makeMove = async (
    sourceSquare: string,
    targetSquare: string,
    promotedPiece: string | null = null
  ) => {
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
          moveNumber: prev.length + 1,
          fen: "",
          move: move,
          whiteRemainingTimeMs: null,
          blackRemainingTimeMs: null,
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

      setMoveHistory((prev) => {
        if (prev.length === 0) return prev;
        const updatedHistory = [...prev];
        updatedHistory[updatedHistory.length - 1].fen = newFen;
        return updatedHistory;
      });

      const isGameEnded = await checkGameState();
      if (isGameEnded) return;

      setCurrentMoveIndex((prev) => prev + 1);
      if (newWhoToMove !== color) {
        const computerMove = await fetchComputerMove(gameId);
        const [source, target] = computerMove.data.split(" ");
        await makeMove(source, target);
        setCurrentMoveIndex(moveHistory.length + 1);
      } else {
        await loadMoves();
      }

      // Uaktualnienie indeksu po odświeżeniu stanu gry

    } catch (error) { 
      console.error("Error refreshing game state:", error);
    }
  };

  const loadMoves = async () => {
    try {
      const movesResponse = await fetchMoves(gameId);
      const movesMapping: { [key: string]: string[] } = {};

      movesResponse.data.forEach((move: string) => {
        const [source, target] = move.split(" ");
        if (!movesMapping[source]) movesMapping[source] = [];
        movesMapping[source].push(target);
      });
      setMappedMoves(movesMapping);
    } catch (error) {
      console.error("Error loading moves:", error);
    }
  };

  const handleSelectMoveIndex = (index: number) => {
    setCurrentMoveIndex(index);
    const selectedFen = moveHistory[index]?.fen;
    if (selectedFen) {
      setPosition(selectedFen);
    }
  };

  const handleMoveIndexChange = (index: number) => {
    setCurrentMoveIndex(index);
    const selectedFen = moveHistory[index]?.fen;
    if (selectedFen) {
      setPosition(selectedFen);
    }
  };
  const handleResign = async () => {
    try{

      await resign(gameId);
    }
    catch(error){
      console.error("Error resigning:", error);
    }
  };

  if (!isPositionLoaded) return <div>Loading...</div>;
  if (gameEnded) return <div>Game Over: {gameResult}</div>;

  return (
    <div>
      <h1 style={{ color: "white" }}>Computer</h1>
      <GameReviewContent
        moveHistory={moveHistory}
        currentMoveIndex={currentMoveIndex}
        position={position}
        disableAnimation={false}
        isInteractive={true}
        onSelectMoveIndex={handleSelectMoveIndex}
        onMoveIndexChange={handleMoveIndexChange}
        onSquareClick={onSquareClick}
        onPieceDrop={onDrop}
        customSquareStyles={customSquareStyles}
        isDraggablePiece={() => !navigationMode}
        onPromotionPieceSelect={(piece, from, to) =>
          makeMove(from, to, piece)
        }
      >
        <div style={buttonContainerStyle}>
          <button style={buttonStyle} onClick = {handleResign} title="Give up a game">
            Resign
          </button>
        </div>
      </GameReviewContent>
    </div>
  );
};

export default ChessboardComponentComputer;

const buttonContainerStyle = {
  display: "flex",
  justifyContent: "space-between",
  gap: "10px",
  marginTop: "auto",
  width: "100%",
};
const buttonStyle = {
  padding: "10px",
  fontSize: "16px",
  fontWeight: "bold",
  color: "#fff",
  backgroundColor: "#DD0000",
  border: "none",
  borderRadius: "5px",
  cursor: "pointer",
  boxShadow: "0 4px 30px rgba(0, 0, 0, 0.5)",
  width: "100%",
};
