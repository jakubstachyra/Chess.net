"use client";
import React from "react";
import GameHistory from "../components/gameHistory/gameHistory";
import { Container, Box } from "@mui/material";

const HistoryPage = () => {
  return (
    <div style={{width: "70%",height: "85%", marginTop: "50px" }}>
      {/* Rankings Table */}
      <GameHistory />
    </div>
  );  
};

export default HistoryPage;

