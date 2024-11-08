'use client'
import React from 'react';
import { Chessboard } from 'react-chessboard';

const ChessboardComponent = () => {
  return (
    <div className="centered-container">
      <Chessboard 
        id="BasicBoard" 
        boardWidth={600} 
        position="start" 
      />
    </div>
  );
};

export default ChessboardComponent;
