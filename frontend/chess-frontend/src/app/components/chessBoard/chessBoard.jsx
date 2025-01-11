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
        customDarkSquareStyle={{ backgroundColor: "green" }}
        customLightSquareStyle={{ backgroundColor: "lightgrey" }}
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
