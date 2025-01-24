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
  export interface Game {
    gameId: string;
    lastFen: string;
    result: string;
    whitePlayer: string;
    blackPlayer: string;
  }
  
  export interface ApiGame {
    gameId: string;
    lastFen: string;
    result: string;
    whitePlayer: string;
    blackPlayer: string;
  }
  // src/types/index.ts

export interface User {
    userID: string;
    userEmail: string;
    username: string;
  }
  
  export interface UserState {
    user: User | null;
    token: string | null;
    isAdmin: boolean;
  }
  
  export interface RegisterFormState {
    username: string;
    email: string;
    password: string;
    confirmPassword: string;
    acceptTerms: boolean;
    errors: Record<string, string>;
    success: boolean;
    loading: boolean;
  }
  
  export interface LoginFormState {
    email: string;
    password: string;
    errors: Record<string, string>;
    success: boolean;
    loading: boolean;
  }

  
  