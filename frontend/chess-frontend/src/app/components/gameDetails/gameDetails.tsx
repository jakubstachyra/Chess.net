// pages/GameDetails.tsx (lub odpowiednia ścieżka)
"use client";

import React, { useState, useEffect } from "react";
import { useRouter, useSearchParams } from "next/navigation";
import BackgroundUI from "app/components/backgroundUI/pages";
import { fetchGameHistoryByID } from "app/services/historyService";
import { GameReviewContent } from "../gameReview/gameReview";

interface MoveHistoryEntry {
  moveNumber: number;
  fen: string;
  move: string;
  whiteRemainingTimeMs: number | null;
  blackRemainingTimeMs: number | null;
}

const GameDetails = () => {
  const router = useRouter();
  const searchParams = useSearchParams();
  const gameId = searchParams.get("gameId");
  const [gameDetails, setGameDetails] = useState<any>(null);
  const [moveHistory, setMoveHistory] = useState<MoveHistoryEntry[]>([]);
  const [position, setPosition] = useState("start");
  const [currentMoveIndex, setCurrentMoveIndex] = useState(0);
  const [navigationMode, setNavigationMode] = useState(true);
  const [loading, setLoading] = useState(true);
  const [disableAnimation, setDisableAnimation] = useState(false);

  useEffect(() => {
    if (gameId) {
      fetchGameDetails(gameId);
    }
  }, [gameId]);

  const fetchGameDetails = async (gameId: string) => {
    try {
      const data = await fetchGameHistoryByID(gameId);
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
        data.movesHistory.forEach((move: any) => {
          if (move.whiteFen) {
            transformedMovesHistory.push({
              moveNumber: move.moveNumber,
              fen: move.whiteFen,
              move: move.whiteMove,
              whiteRemainingTimeMs: move.whiteRemainingTimeMs,
              blackRemainingTimeMs: null,
            });
          }
          if (move.blackFen) {
            transformedMovesHistory.push({
              moveNumber: move.moveNumber,
              fen: move.blackFen,
              move: move.blackMove,
              whiteRemainingTimeMs: null,
              blackRemainingTimeMs: move.blackRemainingTimeMs,
            });
          }
        });
      }

      setMoveHistory(transformedMovesHistory);
      setPosition(transformedMovesHistory[0].fen);
      setCurrentMoveIndex(0);
    } catch (error: any) {
      console.error("Failed to fetch game details:", error.message);
    } finally {
      setLoading(false);
    }
  };

  const handleSetPosition = (fen: string, disableAnim = false) => {
    if (disableAnim) {
      setDisableAnimation(true);
      setTimeout(() => setDisableAnimation(false), 100);
    }
    setPosition(fen);
  };

  const handleMoveIndexChange = (index: number) => {
    setCurrentMoveIndex(index);
    handleSetPosition(moveHistory[index].fen, true);
    if (index === moveHistory.length - 1) {
      setNavigationMode(false);
    } else {
      setNavigationMode(true);
    }
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
    />
  );
};

export default GameDetails;
