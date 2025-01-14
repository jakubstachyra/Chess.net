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
  isInteractive: boolean; // nowy prop określający tryb interaktywności
  onSelectMoveIndex: (index: number) => void;
  onMoveIndexChange: (index: number) => void;
  children?: React.ReactNode;
  // Opcjonalne funkcje interaktywne
  onSquareClick?: (square: string) => void;
  onPieceDrop?: (sourceSquare: string, targetSquare: string) => Promise<boolean>;
  customSquareStyles?: { [square: string]: React.CSSProperties };
  isDraggablePiece?: (piece: any) => boolean;
  onPromotionPieceSelect?: (piece: string, from: string, to: string) => void;
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
  isInteractive,
  onSelectMoveIndex,
  onMoveIndexChange,
  children,
  onSquareClick,
  onPieceDrop,
  customSquareStyles,
  isDraggablePiece,
  onPromotionPieceSelect,
}) => {
  return (
    <div>
      <div style={containerStyles}>
        <div style={chessboardContainerStyles}>
          <ChessboardComponent
            onSquareClick={isInteractive ? onSquareClick ?? (() => {}) : undefined}
            position={position}
            boardOrientation={"white"}
            isDraggablePiece={isInteractive ? isDraggablePiece ?? (() => false) : () => false}
            disableAnimation={disableAnimation}
            onPieceDrop={isInteractive ? onPieceDrop : undefined}
            customSquareStyles={customSquareStyles}
            onPromotionPieceSelect={isInteractive ? onPromotionPieceSelect : undefined}
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
