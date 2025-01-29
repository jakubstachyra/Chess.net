"use client";
import React from "react";
import Chessboard from "../components/chessBoard/chessBoard";
import GameModeModal from "../components/gameModeModal/gameModeModal";

export default function PlayPage() {
  return (
    <div style={containerStyles}>
      <div style={chessboardContainerStyles}>
        <Chessboard/>
      </div>
      <div style={modalContainerStyles}>
        <GameModeModal />
      </div>
    </div>
  );
}

const containerStyles = {
  display: "flex",
  justifyContent: "flex-end",
  alignItems: "flex-start",
  padding: "20px",
  gap: "30px",
};

const chessboardContainerStyles = {
  display: "flex",
  alignItems: "center",
  justifyContent: "center",
};

const modalContainerStyles = {
  display: "flex",
  alignItems: "center",
  justifyContent: "center",
  height: "600px",
  width: "400px",
  borderRadius: "15px",
  backgroundColor: "rgba(255, 255, 255, 0.1)",
  boxShadow: "0 4px 15px rgba(0, 0, 0, 0.3)",
  backdropFilter: "blur(10px)", // Rozmycie tła
};
