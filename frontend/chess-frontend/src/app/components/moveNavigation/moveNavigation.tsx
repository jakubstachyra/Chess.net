import React from "react";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import {
  faChevronLeft,
  faChevronRight,
  faStepBackward,
  faStepForward,
} from "@fortawesome/free-solid-svg-icons";

interface MoveNavigationProps {
  moveHistory: any[];
  currentMoveIndex: number;
  onMoveIndexChange: (newIndex: number) => void;
}

const MoveNavigation: React.FC<MoveNavigationProps> = ({
  moveHistory,
  currentMoveIndex,
  onMoveIndexChange,
}) => {
  // Przejdź do początku
  const handleMoveToStart = () => {
    onMoveIndexChange(0);
  };

  // Jeden ruch wstecz
  const handleMoveBackward = () => {
    if (currentMoveIndex > 0) {
      onMoveIndexChange(currentMoveIndex - 1);
    }
  };

  // Jeden ruch naprzód
  const handleMoveForward = () => {
    if (currentMoveIndex < moveHistory.length - 1) {
      onMoveIndexChange(currentMoveIndex + 1);
    }
  };

  // Przejdź na koniec historii
  const handleMoveToEnd = () => {
    onMoveIndexChange(moveHistory.length - 1);
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
        disabled={currentMoveIndex === moveHistory.length - 1}
        title="Step forward"
      >
        <FontAwesomeIcon icon={faChevronRight} />
      </button>
      <button
        style={buttonStyle}
        onClick={handleMoveToEnd}
        disabled={currentMoveIndex === moveHistory.length - 1}
        title="Go to end"
      >
        <FontAwesomeIcon icon={faStepForward} />
      </button>
    </div>
  );
};

export default MoveNavigation;

// Style
const navigationContainerStyles: React.CSSProperties = {
  display: "flex",
  justifyContent: "space-between",
  alignItems: "center",
  gap: "10px",
  marginTop: "10px",
  width: "100%",
};

const buttonStyle: React.CSSProperties = {
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
  transition: "transform 0.1s ease, background-color 0.2s ease",
};
