'use client'
import React from 'react';
import { Chessboard } from 'react-chessboard';

const ChessboardComponent = () => {
  return (
    <div>
      <Chessboard 
        id="BasicBoard" 
        boardWidth={600} 
        position="start" 
        customDarkSquareStyle = {{backgroundColor: 'rgba(60,58,60,0.9)'}}
        customLightSquareStyle = {{backgroundColor: 'rgba(230,230,230,0.8)'}}
      />
    </div>
  );
};

export default ChessboardComponent;
