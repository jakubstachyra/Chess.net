"use client";
import apiClient from "./apiClient";

export const fetchFen = async (gameId) => await apiClient.get(`/Fen/${gameId}`);
export const fetchMoves = async (gameId) =>
  await apiClient.get(`/moves/${gameId}`);
export const fetchWhoToMove = async (gameId) =>
  await apiClient.get(`/WhoToMove/${gameId}`);
export const sendMove = async (gameId, move) =>
  await apiClient.post(`/ReceiveMove/${gameId}`, JSON.stringify(move));
export const fetchComputerMove = async (gameId) =>
  await apiClient.get(`/getComputerMove/${gameId}`);
export const createGame = async () => await apiClient.post("/createGame");
