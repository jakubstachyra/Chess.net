// components/GameReviewContent.tsx
"use client";

import React from "react";
import BackgroundUI from "app/components/backgroundUI/pages";
import ChessboardComponent from "app/components/chessBoard/chessBoard";
import MoveHistory from "app/components/MoveHistory/moveHistory";
import MoveNavigation from "app/components/MoveNavigation/moveNavigation";
import {MoveHistoryEntry } from "../../../types/types";

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
  isDraggablePiece?: (piece: string, sourceSquare: string) => boolean;
  onPromotionPieceSelect?: (piece: string, from: string, to: string) => void;
  boardOrientation: string;
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
  justifyContent: "center", // Zmiana na wyśrodkowanie szachownicy
  alignItems: "center", // Dopasowanie do osi pionowej
  padding: "20px",
  gap: "30px", // Odstęp między szachownicą a panelami
};

const chessboardContainerStyles: React.CSSProperties = {
  display: "flex",
  flexDirection: "column",
  alignItems: "center",
  position: "relative", // Dodane dla timerów
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
  boardOrientation,
}) => {
  return (
    <div>
      <div style={containerStyles}>
        <div style={chessboardContainerStyles}>
          <ChessboardComponent
            onSquareClick={isInteractive ? onSquareClick ?? (() => {}) : undefined}
            position={position}
            isDraggablePiece={isInteractive ? isDraggablePiece ?? (() => false) : () => false}
            disableAnimation={disableAnimation}
            onPieceDrop={isInteractive ? onPieceDrop : undefined}
            customSquareStyles={customSquareStyles}
            onPromotionPieceSelect={isInteractive ? onPromotionPieceSelect : undefined}
            boardOrientation={boardOrientation}
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
