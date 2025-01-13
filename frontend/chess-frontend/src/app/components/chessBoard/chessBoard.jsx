import React from "react";
import PropTypes, { string } from "prop-types";
import { Chessboard } from "react-chessboard";

const ChessboardComponent = ({
  boardWidth = 600,
  position = "start",
  onSquareClick = {},
  customSquareStyles = {},
  onPieceDrop = {},
  boardOrientation = "white",
  isDraggablePiece = () => true,
  onPromotionPieceSelect = {},
  disableAnimation = false
}) => {
  return (
    <div className="centered-container">
      <Chessboard
        id="BasicBoard"
        boardWidth={boardWidth}
        position={position}
        onSquareClick={onSquareClick} 
        customSquareStyles={customSquareStyles}
        customDarkSquareStyle={{ backgroundColor: "rgba(255, 255, 255, 0.08)" , boxShadow: "0 4px 30px rgba(0, 0, 0, 0.5)"}}
        customLightSquareStyle={{ backgroundColor: "rgba(255, 255, 255, 0.8)",boxShadow: "0 4px 30px rgba(0, 0, 0, 0.5)" }}
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
