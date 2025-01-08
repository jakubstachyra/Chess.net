"use client";
import apiClient from "./apiClient";
export const fetchFen = async (gameId) => await apiClient.get(`/Fen/${gameId}`);

export const fetchMoves = async (gameId) =>
  await apiClient.get(`/moves/${gameId}`);

  export const fetchWhoToMove = async (gameId) =>
  await apiClient.get(`/WhoToMove/${gameId}`);

  export const sendMove = async (gameId, move) =>  
  await apiClient.post(`/ReceiveMove/${gameId}`, {move});

  
export const fetchComputerMove = async (gameId) =>
  await apiClient.get(`/getComputerMove/${gameId}`);
export const fetchGameState = async (gameId) =>
  await apiClient.get(`/State/${gameId}`);
export const createGame = async () => {
  try {
    const response = await apiClient.post("/InitializeWithComputer");
    console.log(response.data);
    const id = response.data.gameId;
    console.log(id);
    return id;
  } catch (error) {
    console.error("Error creating game:", error);
    throw error;
  }
};
export const sendFen = async (gameId, fen) => {
  try {
    const response = await apiClient.post(`/GetFen/${gameId}`, fen, {
      headers: { "Content-Type": "application/json" },
    });
    return response.data;
  } catch (error) {
    console.error("Error sending FEN:", error);
    throw error;
  }
};

/*
export async function createGame(mode, timer) {
  const response = await fetch("/api/game/create", {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ mode, timer }),
  });
  if (!response.ok) {
    throw new Error("Failed to create game");
  }
  const data = await response.json();
  return data.gameId;
}

export async function fetchFen(gameId) {
  const response = await fetch(`/api/game/${gameId}/fen`);
  const data = await response.json();
  return { data: data.fen };
}

export async function fetchMoves(gameId) {
  const response = await fetch(`/api/game/${gameId}/moves`);
  const data = await response.json();
  return { data: data.moves };
}

export async function fetchWhoToMove(gameId) {
  const response = await fetch(`/api/game/${gameId}/who-to-move`);
  const data = await response.json();
  return { data: data.player };
}

export async function sendMove(gameId, move) {
  const response = await fetch(`/api/game/${gameId}/move`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ move }), 
  });
  if (!response.ok) {
    throw new Error("Failed to send move");
  }
}

export async function fetchComputerMove(gameId) {
  const response = await fetch(`/api/game/${gameId}/computer-move`);
  const data = await response.json();
  return { data: data.move };
}
*/
