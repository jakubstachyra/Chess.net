"use client";
import React, { useState } from "react";
import { useRouter } from "next/navigation";
import { createGame } from "../../services/gameService";

export default function GameModeModal() {
  const router = useRouter();
  const [selectedMode, setSelectedMode] = useState("");
  const [selectedTimer, setSelectedTimer] = useState("");
  const [isRanked, setIsRanked] = useState(false); 

  const handleModeSelect = (mode) => {
    setSelectedMode(mode);

    if (mode === "computer") {
      setIsRanked(false);
    }
  };

  const handlePlay = async () => {
    try {
      switch (selectedMode) {
        case "computer":
          console.log("test");
          const gameId = await createGame();
          router.push(`/play-with-computer/${gameId}`);
          break;
        case "player":
          // obsługa gry online
          break;
        case "friend":
          // obsługa gry z przyjacielem
          break;
        default:
          console.error("No game mode selected!");
          break;
      }
    } catch (error) {
      console.error("Error creating game:", error);
    }
  };

  return (
    <div style={modalContentStyles}>
      <h2 style={titleStyles}>Select Game Mode</h2>
      <div style={buttonContainerStyles}>
        <button
          style={{
            ...modeButtonStyles,
            backgroundColor: selectedMode === "player" ? "#0056b3" : "#007bff",
          }}
          onClick={() => handleModeSelect("player")}
        >
          👥 Play vs Player
        </button>
        <button
          style={{
            ...modeButtonStyles,
            backgroundColor:
              selectedMode === "computer" ? "#0056b3" : "#007bff",
          }}
          onClick={() => handleModeSelect("computer")}
        >
          🤖 Play vs Computer
        </button>
        <button
          style={{
            ...modeButtonStyles,
            backgroundColor: selectedMode === "friend" ? "#0056b3" : "#007bff",
          }}
          onClick={() => handleModeSelect("friend")}
        >
          Play vs Friend
        </button>
      </div>

      <div style={toggleContainerStyles}>
        <label style={toggleLabelStyles}>Ranked Game:</label>
        <div
          style={{
            ...toggleSwitchStyles,
            backgroundColor:
              isRanked && selectedMode !== "computer" ? "#4CAF50" : "#ccc",
            cursor: selectedMode === "computer" ? "not-allowed" : "pointer",
            opacity: selectedMode === "computer" ? 0.5 : 1, // Wyszarzenie
          }}
          onClick={() => {
            if (selectedMode !== "computer") setIsRanked(!isRanked);
          }}
          title={
            selectedMode === "computer"
              ? "Cannot play ranked games against the computer"
              : "Enable or disable ranked mode"
          }
        >
          <div
            style={{
              ...toggleCircleStyles,
              transform: isRanked ? "translateX(20px)" : "translateX(0)",
            }}
          />
        </div>
      </div>

      {/* Timer */}
      <div>
        <h3 style={subtitleStyles}>Select Timer</h3>
        <select
          style={{
            ...dropdownStyles,
            backgroundColor: selectedMode === "computer" ? "#e0e0e0" : "#fff",
            cursor: selectedMode === "computer" ? "not-allowed" : "pointer",
            color: selectedMode === "computer" ? "#aaa" : "#333",
          }}
          value={selectedMode === "computer" ? "No Timer" : selectedTimer}
          onChange={(e) => {
            if (selectedMode !== "computer") {
              setSelectedTimer(e.target.value);
            }
          }}
          disabled={selectedMode === "computer"}
          title={
            selectedMode === "computer"
              ? "Timer is disabled for games against the computer"
              : "Select a time limit"
          }
        >
          {["3 min", "5 min", "10 min", "15 min", "30 min", "No Timer"].map(
            (time) => (
              <option key={time} value={time}>
                {time}
              </option>
            )
          )}
        </select>
      </div>

      <button style={playButtonStyle} onClick={handlePlay}>
        Play
      </button>
    </div>
  );
}

// Style
const modalContentStyles = {
  display: "flex",
  flexDirection: "column",
  alignItems: "center",
  padding: "20px",
  backgroundColor: "rgba(255, 255, 255, 0.1)",
  borderRadius: "15px",
  boxShadow: "0 4px 30px rgba(0, 0, 0, 0.5)",
  backdropFilter: "blur(10px)",
  border: "1px solid rgba(255, 255, 255, 0.2)",
  width: "100%",
  height: "100%",
  boxSizing: "border-box",
};

const dropdownStyles = {
  padding: "10px",
  fontSize: "16px",
  fontWeight: "bold",
  color: "#333",
  backgroundColor: "#e0e0e0",
  border: "none",
  borderRadius: "5px",
  marginBottom: "20px",
  cursor: "pointer",
};

const titleStyles = {
  fontSize: "20px",
  fontWeight: "bold",
  marginBottom: "20px",
  color: "#fff",
};

const buttonContainerStyles = {
  display: "flex",
  flexDirection: "column",
  gap: "10px",
  marginBottom: "20px",
};

const modeButtonStyles = {
  padding: "10px 20px",
  fontSize: "16px",
  fontWeight: "bold",
  color: "#fff",
  backgroundColor: "#007bff",
  border: "none",
  borderRadius: "5px",
  cursor: "pointer",
};

const playButtonStyle = {
  padding: "10px 20px",
  fontSize: "18px",
  fontWeight: "bold",
  color: "#fff",
  backgroundColor: "#007bff",
  border: "none",
  borderRadius: "5px",
  cursor: "pointer",
  width: "80%",
};

const subtitleStyles = {
  fontSize: "18px",
  fontWeight: "bold",
  marginBottom: "10px",
  color: "#fff",
};

const toggleContainerStyles = {
  display: "flex",
  alignItems: "center",
  gap: "10px",
  marginBottom: "20px",
};

const toggleLabelStyles = {
  fontSize: "16px",
  fontWeight: "bold",
  color: "#fff",
};

const toggleSwitchStyles = {
  width: "40px",
  height: "20px",
  backgroundColor: "#ccc",
  borderRadius: "10px",
  position: "relative",
  cursor: "pointer",
};

const toggleCircleStyles = {
  width: "18px",
  height: "18px",
  backgroundColor: "#fff",
  borderRadius: "50%",
  position: "absolute",
  top: "1px",
  transition: "transform 0.2s ease-in-out",
};
