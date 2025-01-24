// src/app/components/gameDetails/gameDetails.tsx

"use client";

import React, { useState, useEffect } from "react";
import BackgroundUI from "../backgroundUI/pages";
import { fetchGameHistoryByID } from "app/services/historyService";
import { GameReviewContent } from "../gameReview/gameReview";
import { MoveHistoryEntry, GameHistory } from "types/types";

const GameDetails: React.FC = () => {
  const [gameDetails, setGameDetails] = useState<GameHistory | null>(null);
  const [moveHistory, setMoveHistory] = useState<MoveHistoryEntry[]>([]);
  const [position, setPosition] = useState<string>("start");
  const [currentMoveIndex, setCurrentMoveIndex] = useState<number>(0);
  const [loading, setLoading] = useState<boolean>(true);
  const [disableAnimation, setDisableAnimation] = useState<boolean>(false);

  // Stany do zarządzania dialogiem (opcjonalnie, jeśli planujesz je dodać)
  // const [dialogOpen, setDialogOpen] = useState<boolean>(false);
  // const [dialogTitle, setDialogTitle] = useState<string>("");
  // const [dialogContent, setDialogContent] = useState<string>("");

  // Pobieranie parametrów z URL
  const searchParams = new URLSearchParams(window.location.search);
  const gameId = searchParams.get("gameId");

  useEffect(() => {
    if (gameId) {
      fetchGameDetails(gameId);
    }
  }, [gameId]);

  const fetchGameDetails = async (gameId: string) => {
    try {
      const data: GameHistory | null = await fetchGameHistoryByID(gameId);
      if (!data) throw new Error("Game history not found");

      setGameDetails(data);

      const transformedMovesHistory: MoveHistoryEntry[] = [];

      if (data.startFen) {
        transformedMovesHistory.push({
          moveNumber: 0,
          fen: data.startFen,
          move: "Start Position",
          whiteRemainingTimeMs: null,
          blackRemainingTimeMs: null,
        });
      } else {
        transformedMovesHistory.push({
          moveNumber: 0,
          fen: "start",
          move: "Start Position",
          whiteRemainingTimeMs: null,
          blackRemainingTimeMs: null,
        });
      }

      if (data.movesHistory) {
        data.movesHistory.forEach((move) => {
          if (move.whiteFen && move.whiteMove) {
            transformedMovesHistory.push({
              moveNumber: move.moveNumber,
              fen: move.whiteFen,
              move: move.whiteMove,
              whiteRemainingTimeMs: move.whiteRemainingTimeMs ?? null,
              blackRemainingTimeMs: null,
            });
          }
          if (move.blackFen && move.blackMove) {
            transformedMovesHistory.push({
              moveNumber: move.moveNumber,
              fen: move.blackFen,
              move: move.blackMove,
              whiteRemainingTimeMs: null,
              blackRemainingTimeMs: move.blackRemainingTimeMs ?? null,
            });
          }
        });
      }

      setMoveHistory(transformedMovesHistory);
      setPosition(transformedMovesHistory[0].fen);
      setCurrentMoveIndex(0);
    } catch (error: unknown) {
      if (error instanceof Error) {
        console.error("Failed to fetch game details:", error.message);
      } else {
        console.error("Failed to fetch game details:", error);
      }
    } finally {
      setLoading(false);
    }
  };

  const handleSetPosition = (fen: string, disableAnim = false): void => {
    if (disableAnim) {
      setDisableAnimation(true);
      setTimeout(() => setDisableAnimation(false), 100);
    }
    setPosition(fen);
  };

  const handleMoveIndexChange = (index: number): void => {
    setCurrentMoveIndex(index);
    handleSetPosition(moveHistory[index].fen, true);
    // Zmienna 'navigationMode' została usunięta, ponieważ była nieużywana
  };

  if (loading) {
    return (
      <BackgroundUI>
        <div style={{ color: "white", fontSize: "18px", textAlign: "center" }}>
          Loading game details...
        </div>
      </BackgroundUI>
    );
  }

  if (!gameDetails) {
    return (
      <BackgroundUI>
        <div style={{ color: "white", fontSize: "18px", textAlign: "center" }}>
          Game details not found.
        </div>
      </BackgroundUI>
    );
  }

  return (
    <GameReviewContent
      moveHistory={moveHistory}
      currentMoveIndex={currentMoveIndex}
      position={position}
      disableAnimation={disableAnimation}
      onSelectMoveIndex={handleMoveIndexChange}
      onMoveIndexChange={handleMoveIndexChange}
      isInteractive={false}
      boardOrientation="white" 
    />
  );
};

export default GameDetails;
