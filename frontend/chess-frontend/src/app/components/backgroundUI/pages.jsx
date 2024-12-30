"use client";
import React, { useState } from "react";

export default function BackgroundUI({children}) {

  return (
    <div style={modalContentStyles}>
      {children}
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
