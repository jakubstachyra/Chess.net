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
