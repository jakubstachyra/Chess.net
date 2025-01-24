
export interface QueueHandlers {
    WaitingForOpponent: (playerCount: number) => void;
    GameReady: (gameId: string) => void;
    Error: (message: string) => void;
  }
  
  export interface GameHandlers {
    PlayerDisconnected: () => void;
    OpponentInfo: (data: OpponentInfoData) => void;
    OpponentMoved: () => Promise<void>;
    UpdateTimers: (p1Time: number, p2Time: number) => void;
    GameIsReady: (gameId: number) => Promise<void>;
    DrawProposed: () => Promise<void>;
    DrawRejected: () => Promise<void>;
    MoveHistoryUpdated: (entries: MoveHistoryEntry[]) => void;
    PossibleMovesUpdated: (moves: string[]) => void;
    GameOver: (info: GameOverInfo) => void;
    Disconnect: () => Promise<void>;
    MoveAcknowledged: () => Promise<void>;
  }
  
  // Define additional data structures used in handlers
  export interface OpponentInfoData {
    username: string;
    userId: string;
  }
  
  export interface MoveHistoryEntry {
    moveNumber: number;
    fen: string;
    move: string;
    whiteRemainingTimeMs: number | null;
    blackRemainingTimeMs: number | null;
  }
  
  export interface GameOverInfo {
    gameId: number;
    winner: string;
    loser: string;
    reason: string;
    draw: string;
  }
  
  // Combined Handlers Interface
  export type Handlers = QueueHandlers & GameHandlers;
  