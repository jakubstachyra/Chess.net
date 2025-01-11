"use client";
import React from "react";
import GameHistory from "../components/gameHistory/gameHistory";
import "./HistoryPage.css";
import { Container, Box } from "@mui/material";

const HistoryPage = () => {
  return (
    <Container component="main" maxWidth="lg">
      {/* Rankings Table */}
      <GameHistory />
    </Container>
  );  
};

export default HistoryPage;

