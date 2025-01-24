// src/types.ts

export interface Report {
    id: string;
    gameID: string;
    suspectID: string;
    // Dodaj inne pola, które są zwracane przez fetchReport
  }
  
  export interface Move {
    moveNumber: number;
    whiteFen?: string;
    whiteMove?: string;
    whiteRemainingTimeMs?: number;
    blackFen?: string;
    blackMove?: string;
    blackRemainingTimeMs?: number;
  }
  
  export interface GameHistory {
    startFen?: string;
    movesHistory?: Move[];
    // Dodaj inne pola, które są zwracane przez fetchGameHistoryByID
  }
  
  export interface MoveHistoryEntry {
    moveNumber: number;
    fen: string;
    move: string;
    whiteRemainingTimeMs: number | null;
    blackRemainingTimeMs: number | null;
  }
  
  export interface Request {
    id: string;
    userId: string;
  }
  
  export interface User {
    userID: string;
    username: string;
  }

  export interface SuccessResponse {
    message: string;
  }
  