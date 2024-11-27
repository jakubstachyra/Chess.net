import React from "react";
import PropTypes, { string } from "prop-types";
import { Chessboard } from "react-chessboard";

const ChessboardComponent = ({
  position = "start",
  onSquareClick = {},
  customSquareStyles = {},
  onPieceDrop = {},
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
      />
    </div>
  );
};

export default ChessboardComponent;
