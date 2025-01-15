"use client";
import React, { useEffect, useState } from "react";
import { Typography } from "@mui/material";

interface TimerProps {
  timeMs: number;
}

const PlayerTimer: React.FC<TimerProps> = ({ timeMs }) => {
  const [currentTime, setCurrentTime] = useState(timeMs);

  // Synchronizuj lokalny stan z przekazanym czasem
  useEffect(() => {
    setCurrentTime(timeMs);
  }, [timeMs]);

  const minutes = Math.floor(currentTime / 60000);
  const seconds = Math.floor((currentTime % 60000) / 1000);
  const formattedTime = `${minutes}:${seconds < 10 ? "0" : ""}${seconds}`;

  return (
    <div
      style={{
        display: "flex",
        justifyContent: "center",
        alignItems: "center",
        width: "100%", // Opcjonalnie: szerokość kontenera
        backgroundColor: "rgba(255, 255, 255, 0.1)",
        borderRadius: "15px",
        boxShadow: "0 4px 30px rgba(0, 0, 0, 0.5)",
        backdropFilter: "blur(10px)",
        border: "1px solid rgba(255, 255, 255, 0.2)",
      }}
    >
      <Typography
        variant="h4"
        style={{
          textAlign: "center",
          color: "#fff",
        }}
      >
        {formattedTime}
      </Typography>
    </div>
  );
};

export default PlayerTimer;
