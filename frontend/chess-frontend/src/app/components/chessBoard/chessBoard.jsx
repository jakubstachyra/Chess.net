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
}) => {
  return (
    <div className="centered-container">
      <Chessboard
        id="BasicBoard"
        boardWidth={boardWidth}
        position={position}
        onSquareClick={onSquareClick}
        customSquareStyles={customSquareStyles}
        onPieceDrop={onPieceDrop}
        boardOrientation={boardOrientation}
        isDraggablePiece={isDraggablePiece}
      />
    </div>
  );
};

export default ChessboardComponent;
