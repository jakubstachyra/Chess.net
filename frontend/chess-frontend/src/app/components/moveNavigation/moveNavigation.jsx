import React, { useState, useEffect } from "react";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faChevronLeft, faChevronRight, faStepBackward, faStepForward } from "@fortawesome/free-solid-svg-icons";

const MoveNavigation = ({ moveHistory, setPosition, setNavigationMode }) => {
  const [currentMoveIndex, setCurrentMoveIndex] = useState(moveHistory.length);

  // Synchronizuj indeks, gdy długość moveHistory się zmienia
  useEffect(() => {
    if (currentMoveIndex > moveHistory.length) {
      setCurrentMoveIndex(moveHistory.length);
    }
  }, [moveHistory]);

  const handleMoveToStart = () => {
    if (moveHistory.length > 0) {
      setCurrentMoveIndex(0);
      const firstPosition = moveHistory[0]?.fen;
      if (firstPosition) setPosition(firstPosition);
      setNavigationMode(true); // Włącz tryb nawigacji
    }
  };

  const handleMoveBackward = () => {
    if (currentMoveIndex > 0) {
      const newIndex = currentMoveIndex - 1;
      setCurrentMoveIndex(newIndex);
      const newPosition = moveHistory[newIndex]?.fen || moveHistory[0]?.fen;
      if (newPosition) setPosition(newPosition);
      setNavigationMode(true); // Włącz tryb nawigacji
    }
  };

  const handleMoveForward = () => {
    if (currentMoveIndex < moveHistory.length - 1) {
      const newIndex = currentMoveIndex + 1;
      setCurrentMoveIndex(newIndex);
      const newPosition = moveHistory[newIndex]?.fen;
      if (newPosition) setPosition(newPosition);
      // Tryb nawigacji pozostaje włączony, jeśli nie jesteśmy na ostatnim ruchu
      setNavigationMode(newIndex < moveHistory.length - 1);
    } else if (currentMoveIndex === moveHistory.length - 1) {
      setNavigationMode(false);
    }
  };
  
  const handleMoveToEnd = () => {
    if (moveHistory.length > 0) {
      const lastIndex = moveHistory.length - 1;
      setCurrentMoveIndex(lastIndex);
      const lastPosition = moveHistory[lastIndex]?.fen;
      if (lastPosition) setPosition(lastPosition);
      setNavigationMode(false);
    }
  };
  

  return (
    <div style={navigationContainerStyles}>
      <button
        style={buttonStyle}
        onClick={handleMoveToStart}
        disabled={currentMoveIndex === 0}
        title="Go to start"
      >
        <FontAwesomeIcon icon={faStepBackward} />
      </button>
      <button
        style={buttonStyle}
        onClick={handleMoveBackward}
        disabled={currentMoveIndex === 0}
        title="Step backward"
      >
        <FontAwesomeIcon icon={faChevronLeft} />
      </button>
      <button
        style={buttonStyle}
        onClick={handleMoveForward}
        disabled={currentMoveIndex === moveHistory.length}
        title="Step forward"
      >
        <FontAwesomeIcon icon={faChevronRight} />
      </button>
      <button
        style={buttonStyle}
        onClick={handleMoveToEnd}
        disabled={currentMoveIndex === moveHistory.length}
        title="Go to end"
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
  padding: "15px",
  fontSize: "18px",
  fontWeight: "bold",
  color: "white",
  border: "none",
  borderRadius: "8px",
  cursor: "pointer",
  boxShadow: "0 4px 15px rgba(0, 0, 0, 0.5)",
  backdropFilter: "blur(5px)",
  backgroundColor: "rgba(0, 0, 0, 0.3)",
  width: "20%",
  textAlign: "center",
};
