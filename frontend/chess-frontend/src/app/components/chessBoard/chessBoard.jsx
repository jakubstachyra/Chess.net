import React from "react";
import PropTypes, { string } from "prop-types";
import { Chessboard } from "react-chessboard";

const ChessboardComponent = ({
  position = "start",
  onSquareClick = {},
  customSquareStyles = {},
  onPieceDrop = {},
  boardOrientation = "white",

}) => {
  return (
    <div className="centered-container">
      <Chessboard
        id="BasicBoard"
        boardWidth={600}
        position={position}
        onSquareClick={onSquareClick}
        customSquareStyles={customSquareStyles}
        onPieceDrop={onPieceDrop}
        boardOrientation={boardOrientation}
      />
    </div>
  );
};

export default ChessboardComponent;
