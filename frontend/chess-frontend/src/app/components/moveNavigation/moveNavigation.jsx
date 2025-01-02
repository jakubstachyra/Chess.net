import React, { useState } from "react";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faChevronLeft, faChevronRight, faStepBackward, faStepForward } from "@fortawesome/free-solid-svg-icons";

const MoveNavigation = ({ moveHistory, setPosition, setNavigationMode }) => {
  const [currentMoveIndex, setCurrentMoveIndex] = useState(moveHistory.length);

  const handleMoveToStart = () => {
    setCurrentMoveIndex(0);
    const firstPosition = moveHistory[0]?.fen;
    if (firstPosition) setPosition(firstPosition);
    setNavigationMode(true); // Włącz tryb nawigacji
  };

  const handleMoveBackward = () => {
    if (currentMoveIndex > 0) {
      const newIndex = currentMoveIndex - 1;
      setCurrentMoveIndex(newIndex);
      const newPosition = moveHistory[newIndex]?.fen;
      if (newPosition) setPosition(newPosition);
      setNavigationMode(true); // Włącz tryb nawigacji
    }
  };

  const handleCurrentPosition = () => {
    setCurrentMoveIndex(moveHistory.length);
    const currentPosition = moveHistory[moveHistory.length - 1]?.fen;
    if (currentPosition) setPosition(currentPosition);
    setNavigationMode(false); // Wyłącz tryb nawigacji
  };

  const handleMoveForward = () => {
    if (currentMoveIndex < moveHistory.length) {
      const newIndex = currentMoveIndex + 1;
      setCurrentMoveIndex(newIndex);
      const newPosition = moveHistory[newIndex]?.fen;
      if (newPosition) setPosition(newPosition);
      setNavigationMode(true); // Włącz tryb nawigacji
    }
  };

  const handleMoveToEnd = () => {
    setCurrentMoveIndex(moveHistory.length);
    const lastPosition = moveHistory[moveHistory.length - 1]?.fen;
    if (lastPosition) setPosition(lastPosition);
    setNavigationMode(false); // Wyłącz tryb nawigacji
  };

  return (
    <div style={navigationContainerStyles}>
      <button
        style={buttonStyle}
        onClick={handleMoveToStart}
        disabled={currentMoveIndex === 0}
      >
        <FontAwesomeIcon icon={faStepBackward} />
      </button>
      <button
        style={buttonStyle}
        onClick={handleMoveBackward}
        disabled={currentMoveIndex === 0}
      >
        <FontAwesomeIcon icon={faChevronLeft} />
      </button>
      <button
        style={buttonStyle}
        onClick={handleMoveForward}
        disabled={currentMoveIndex === moveHistory.length}
      >
        <FontAwesomeIcon icon={faChevronRight} />
      </button>
      <button
        style={buttonStyle}
        onClick={handleMoveToEnd}
        disabled={currentMoveIndex === moveHistory.length}
      >
        <FontAwesomeIcon icon={faStepForward} />
      </button>
    </div>
  );
};

export default MoveNavigation;

// Style
const navigationContainerStyles = {
  display: "flex",
  justifyContent: "space-between",
  alignItems: "center",
  gap: "10px",
  marginTop: "10px",
  width: "100%",
};

const buttonStyle = {
  padding: "15px", // Większe przyciski
  fontSize: "18px", // Większy rozmiar ikon
  fontWeight: "bold",
  color: "white",
  border: "none",
  borderRadius: "8px",
  cursor: "pointer",
  boxShadow: "0 4px 15px rgba(0, 0, 0, 0.5)", // Delikatny cień
  backdropFilter: "blur(5px)", // Transparentne z rozmyciem
  backgroundColor: "rgba(0, 0, 0, 0.3)", // Transparentny kolor
  width: "20%", // Przyciski o większej szerokości
  textAlign: "center",
};
