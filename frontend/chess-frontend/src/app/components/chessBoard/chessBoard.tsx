import React from "react";
import { Chessboard } from "react-chessboard";
import { Square, Piece, PromotionPieceOption } from "types/types"; // Import typów

interface ChessboardComponentProps {
  boardWidth?: number;
  position?: string;
  onSquareClick?: (square: Square) => void;
  customSquareStyles?: Record<Square, React.CSSProperties>;
  onPieceDrop?: (sourceSquare: Square, targetSquare: Square, piece: Piece) => boolean;
  boardOrientation?: "white" | "black";
  isDraggablePiece?: (args: { piece: Piece; sourceSquare: Square }) => boolean;
  onPromotionPieceSelect?: (
    promotionPiece?: PromotionPieceOption,
    fromSquare?: Square,
    toSquare?: Square
  ) => boolean;
  disableAnimation?: boolean;
  onPieceDragStart?: (sourceSquare: Square, piece: Piece) => boolean;
}

const ChessboardComponent: React.FC<ChessboardComponentProps> = ({
  boardWidth = 600,
  position = "start",
  onSquareClick,
  customSquareStyles = {},
  onPieceDrop = (_sourceSquare: Square, _targetSquare: Square, _piece: Piece) => true,
  boardOrientation = "white",
  isDraggablePiece = (_args: { piece: Piece; sourceSquare: Square }) => true,
  onPromotionPieceSelect = (
    _promotionPiece?: PromotionPieceOption,
    _fromSquare?: Square,
    _toSquare?: Square
  ) => true,
  disableAnimation = false,
}) => {
  return (
    <div className="centered-container">
      <Chessboard
        id="BasicBoard"
        boardWidth={boardWidth}
        position={position}
        onSquareClick={onSquareClick}
        customSquareStyles={customSquareStyles}
        customDarkSquareStyle={{
          backgroundColor: "rgba(255, 255, 255, 0.08)",
          boxShadow: "0 4px 30px rgba(0, 0, 0, 0.5)",
        }}
        customLightSquareStyle={{
          backgroundColor: "rgba(255, 255, 255, 0.8)",
          boxShadow: "0 4px 30px rgba(0, 0, 0, 0.5)",
        }}
        onPieceDrop={onPieceDrop}
        boardOrientation={boardOrientation}
        isDraggablePiece={isDraggablePiece}
        onPromotionPieceSelect={onPromotionPieceSelect}
        animationDuration={disableAnimation ? 0 : 300}
      />
    </div>
  );
};

export default ChessboardComponent;
