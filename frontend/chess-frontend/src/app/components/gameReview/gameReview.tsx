// components/GameReviewContent.tsx
"use client";

import React from "react";
import BackgroundUI from "app/components/backgroundUI/pages";
import ChessboardComponent from "app/components/chessBoard/chessBoard";
import MoveHistory from "app/components/MoveHistory/moveHistory";
import MoveNavigation from "app/components/MoveNavigation/moveNavigation";

interface MoveHistoryEntry {
  moveNumber: number;
  fen: string;
  move: string;
  whiteRemainingTimeMs: number | null;
  blackRemainingTimeMs: number | null;
}

interface GameReviewContentProps {
  moveHistory: MoveHistoryEntry[];
  currentMoveIndex: number;
  position: string;
  disableAnimation: boolean;
  onSelectMoveIndex: (index: number) => void;
  onMoveIndexChange: (index: number) => void;
  children?: React.ReactNode;
}

const modalContainerStyles: React.CSSProperties = {
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
  color: "white",
};

const containerStyles: React.CSSProperties = {
  display: "flex",
  justifyContent: "flex-end",
  alignItems: "flex-start",
  padding: "20px",
  gap: "30px",
};

const chessboardContainerStyles: React.CSSProperties = {
  display: "flex",
  flexDirection: "column",
  alignItems: "center",
  justifyContent: "center",
  gap: "10px",
};

export const GameReviewContent: React.FC<GameReviewContentProps> = ({
  moveHistory,
  currentMoveIndex,
  position,
  disableAnimation,
  onSelectMoveIndex,
  onMoveIndexChange,
  children,
}) => {
  return (
    <div>
      {/* Główna sekcja */}
      <div style={containerStyles}>
        <div style={chessboardContainerStyles}>
          <ChessboardComponent
            onSquareClick={() => {}}
            position={position}
            boardOrientation={"white"}
            isDraggablePiece={() => false}
            disableAnimation={disableAnimation}
          />
        </div>

        <div style={modalContainerStyles}>
          <BackgroundUI>
            <h1>Moves</h1>
            <MoveHistory
              moveHistory={moveHistory}
              currentMoveIndex={currentMoveIndex}
              onSelectMoveIndex={onSelectMoveIndex}
            />
            <MoveNavigation
              moveHistory={moveHistory}
              currentMoveIndex={currentMoveIndex}
              onMoveIndexChange={onMoveIndexChange}
            />
            {children}
          </BackgroundUI>
        </div>
      </div>
    </div>
  );
};
