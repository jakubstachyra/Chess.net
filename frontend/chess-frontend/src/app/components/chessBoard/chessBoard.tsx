import React from "react";
import { Chessboard } from "react-chessboard";

interface ChessboardComponentProps {
  boardWidth?: number;
  position?: string;
  onSquareClick?: (square: string) => void;
  customSquareStyles?: { [square: string]: React.CSSProperties };
  onPieceDrop?: (sourceSquare: string, targetSquare: string) => boolean | Promise<boolean>;
  boardOrientation?: "white" | "black";
  isDraggablePiece?: (piece: string, sourceSquare: string) => boolean;
  onPromotionPieceSelect?: (piece: string, from: string, to: string) => void;
  disableAnimation?: boolean;
  onPieceDragStart?: (sourceSquare: string, piece: string) => boolean;
}

const ChessboardComponent: React.FC<ChessboardComponentProps> = ({
  boardWidth = 600,
  position = "start",
  onSquareClick,
  customSquareStyles = {},
  onPieceDrop,
  boardOrientation = "white",
  isDraggablePiece = () => true,
  onPromotionPieceSelect,
  disableAnimation = false,
  onPieceDragStart
}) => {
  return (
    <div className="centered-container">
      <Chessboard
        id="BasicBoard"
        boardWidth={boardWidth}
        position={position}
        onSquareClick={onSquareClick} 
        customSquareStyles={customSquareStyles}
        customDarkSquareStyle={{ backgroundColor: "rgba(255, 255, 255, 0.08)", boxShadow: "0 4px 30px rgba(0, 0, 0, 0.5)" }}
        customLightSquareStyle={{ backgroundColor: "rgba(255, 255, 255, 0.8)", boxShadow: "0 4px 30px rgba(0, 0, 0, 0.5)" }}
        onPieceDrop={onPieceDrop}
        boardOrientation={boardOrientation}
        isDraggablePiece={isDraggablePiece}
        onPromotionPieceSelect={onPromotionPieceSelect}
        animationDuration={disableAnimation ? 0 : 300}
        onPieceDragStart={onPieceDragStart} 
      />
    </div>
  );
};

export default ChessboardComponent;
