"use client";

import React, { useState, useEffect, useRef } from "react";
import { HubConnection } from "@microsoft/signalr";
import { getConnection } from "../../services/signalrClient";
import {
  fetchFen,
  fetchWhoToMove,
  fetchGameState,
  resign
} from "../../services/gameService";
import { GameReviewContent } from "../../components/gameReview/gameReview";
import CustomDialog from "../../components/customDialog/customdialog";

interface MoveHistoryEntry {
  moveNumber: number;
  fen: string;
  move: string;
  whiteRemainingTimeMs: number | null;
  blackRemainingTimeMs: number | null;
}

const ChessboardComputer: React.FC = () => {
  const [gameId, setGameId] = useState<number | null>(null);
  const gameIdRef = useRef<number | null>(null);
  const [isGameReady, setIsGameReady] = useState(false);
  const [position, setPosition] = useState("start");
  const [mappedMoves, setMappedMoves] = useState<{ [square: string]: string[] }>({});
  const [whoToMove, setWhoToMove] = useState<number | null>(null);
  const [playerColor, setPlayerColor] = useState<"white" | "black">("white");
  const [moveHistory, setMoveHistory] = useState<MoveHistoryEntry[]>([]);
  const [currentMoveIndex, setCurrentMoveIndex] = useState<number>(0);
  const [dialogOpen, setDialogOpen] = useState<boolean>(false);
  const [dialogTitle, setDialogTitle] = useState<string>("");
  const [dialogContent, setDialogContent] = useState<React.ReactNode>(null);
  const [dialogActions, setDialogActions] = useState<React.ReactNode>(null);
  const [customSquareStyles, setCustomSquareStyles] = useState<{ [square: string]: React.CSSProperties }>({});

  const connectionRef = useRef<HubConnection | null>(null);

  useEffect(() => {
  console.log("gameId został zaktualizowany do:", gameId);
  refreshGameState();
  }, [gameId]);

  useEffect(() => {
    gameIdRef.current = gameId; // Zapisz aktualną wartość do ref
  }, [gameId]);
  
  const mapMoves = (moves: string[]): { [square: string]: string[] } => {
    const result: { [square: string]: string[] } = {};
    moves.forEach((m) => {
      const [src, dst] = m.split(" ");
      if (!result[src]) {
        result[src] = [];
      }
      result[src].push(dst);
    });
    return result;
  };

  const refreshGameState = async () => {
    if (!gameId) return;
    
    if (!connectionRef.current) {
      console.error("SignalR connection is not ready yet.");
      return;
    }

    try {
      const fenResp = await fetchFen(gameId);
      setPosition(fenResp.data);
      console.log("Fen updated:", fenResp.data);  
      const whoResp = await fetchWhoToMove(gameId);
      setWhoToMove(whoResp.data);

      const hub = connectionRef.current;
      if (hub.state !== "Connected") {
        console.error("refreshGameState: Connection is not connected");
        return;
      }
      const movesArray = await hub.invoke("GetPossibleMoves", Number(gameId));
      setMappedMoves(mapMoves(movesArray));
    } catch (err) {
      console.error("Error in refreshGameState:", err);
    }
  };

  const makeMove = async (sourceSquare: string, targetSquare: string, promotedPiece?: string) => {
    console.log("Jestem w makeMove");
    const currentGameId = gameIdRef.current; // Użycie referencji
    console.log("currentGameId:", currentGameId);
  
    if (!currentGameId) {
      console.error("Brak gameId, ruch nie może zostać wykonany.");
      return;
    }
    if (!connectionRef.current) {
      console.error("SignalR connection is not ready yet.");
      return;
    }
  
    try {
      const moveStr = promotedPiece
        ? `${sourceSquare}${targetSquare}${promotedPiece}`
        : `${sourceSquare}${targetSquare}`;
      console.log(`moveStr: ${moveStr}`);
      const hub = connectionRef.current;
      if (hub.state !== "Connected") {
        console.error("makeMove: Connection is not connected");
        return;
      }
  
      console.log("Powinno wywołać ReceiveMove");
      await hub.invoke("ReceiveMoveAsync", currentGameId, moveStr);
  
      await refreshGameState();
      await checkGameEnd();
    } catch (err) {
      console.error("Error in makeMove:", err);
    }
  };
  
  
  const checkGameEnd = async () => {
    if (!gameId) return;
    try {
      const resp = await fetchGameState(gameId);
      if (resp.data) {
        console.log("Gra zakończyła się (checkGameEnd).");
      }
    } catch (err) {
      console.error("Error in checkGameEnd:", err);
    }
  };

  const handleSelectMoveIndex = (index: number) => {
    setCurrentMoveIndex(index);
    const fen = moveHistory[index]?.fen;
    if (fen) setPosition(fen);
  };

  const handleMoveIndexChange = (index: number) => {
    setCurrentMoveIndex(index);
    const fen = moveHistory[index]?.fen;
    if (fen) setPosition(fen);
  };

  const handleGameOverDialog = (info: any) => {
    const { winner, reason, draw } = info;
    setDialogTitle("Game Over");
    setDialogContent(
      <div style={{ display: "flex", flexDirection: "column", alignItems: "center" }}>
        {draw ? (
          <p style={{ color: "yellow", fontWeight: "bold" }}>Draw</p>
        ) : winner === "BOT_USER_ID" ? (
          <p style={{ color: "red", fontWeight: "bold" }}>You lost (Bot wins)</p>
        ) : (
          <p style={{ color: "green", fontWeight: "bold" }}>You won</p>
        )}
        <p style={{ color: "white", marginTop: "1rem" }}>{reason}</p>
      </div>
    );
    setDialogActions(
      <div style={{ display: "flex", gap: "10px", justifyContent: "center", marginTop: "10px" }}>
        <button onClick={() => setDialogOpen(false)}>OK</button>
      </div>
    );
    setDialogOpen(true);
  };

  useEffect(() => {
    let isMounted = true;

    const handlers = {
      GameReady: async (serverGameId: number) => {
        console.log("GameReady, gameId =", serverGameId);
        setGameId(serverGameId); // Aktualizacja stanu
        gameIdRef.current = serverGameId; // Aktualizacja referencji
        await refreshGameState();
      },
      
      GameIsReady: async () => {
        if (!isMounted) return;
        console.log("GameIsReady => pobieram stan gry...");
        setIsGameReady(true);
        gameIdRef.current = serverGameId;
        await refreshGameState();
      },
      OpponentMoved: async () => {
        if (!isMounted) return;
        console.log("OpponentMoved => odświeżam stan gry");
        await refreshGameState();
      },
      MoveHistoryUpdated: (entries: MoveHistoryEntry[]) => {
        console.log("MoveHistoryUpdated: ", entries);
        
        // Dodaj początkowy wpis, jeśli historia jest pusta lub pierwszy wpis nie jest stanem początkowym
        if (entries.length === 0 || entries[0].fen !== "start") {
          const initialEntry: MoveHistoryEntry = {
            moveNumber: 0,
            fen: "start",
            move: "",
            whiteRemainingTimeMs: null,
            blackRemainingTimeMs: null,
          };
          entries = [initialEntry, ...entries];
        }
        
        setMoveHistory(entries);
        setCurrentMoveIndex(entries.length - 1);
        
        // Ustawienie pozycji na ostatni FEN z historii
        if (entries.length > 0) {
          setPosition(entries[entries.length - 1].fen);
        }
      },
      PossibleMovesUpdated: (moves: string[]) => {
        setMappedMoves(mapMoves(moves));
      },
      MoveAcknowledged: async () =>
      {
        if (!isMounted) return;
        console.log("There was a move");
        await refreshGameState();
      },
      Error: async (error: string) => 
      {
        console.log(`Error on hub: ${error} `);
      }
      // Dodaj pozostałe handlery, jeśli potrzebne...
    };

    const initSignalR = async () => {
      try {
        const hub = await getConnection(handlers);
        if (!isMounted) return;
        connectionRef.current = hub;  // Ustawienie referencji po ustanowieniu połączenia
        console.log("Persistent SignalR connection established, ID:", hub.connectionId);
  
        const mode = "classic";
  
        // Upewnij się, że połączenie jest aktywne przed wywołaniem metody
        while (hub.state !== "Connected") {
          console.log("Waiting for connection to become active...");
          await new Promise(resolve => setTimeout(resolve, 100)); 
        }
  
        await hub.invoke("StartGameWithComputer", hub.connectionId, mode);
      } catch (err) {
        console.error("Błąd podczas inicjalizacji SignalR lub wywołania StartGameWithComputer:", err);
      }
    };
  
    initSignalR();
  
    return () => {
      isMounted = false;
      connectionRef.current?.stop();
    };
  }, []);

  const onSquareClick = (square: string) => {
    const moves = mappedMoves[square] || [];
    const styles = moves.reduce((acc: any, target: string) => {
      acc[target] = {
        backgroundColor: "rgba(0, 255, 0, 0.5)",
        borderRadius: "50%",
      };
      return acc;
    }, {});
    setCustomSquareStyles(styles);
  };
  const onDrop = async (sourceSquare: string, targetSquare: string) => {
    const possibleMoves = mappedMoves[sourceSquare];
    if (possibleMoves?.includes(targetSquare)) {
      console.log("Valid move:", sourceSquare, targetSquare);
      await makeMove(sourceSquare, targetSquare);
      console.log("Po makeMove")
      return true;
    } else {
      alert("Invalid move!");
      return false;
    }
  };

  const resignGame = async () => {
    try {
      await resign(gameId);
    } catch (error) {
      console.error("Error in resignGame:", error);
    }
  };

  return (
    <div style={{ display: "flex", flexDirection: "column", alignItems: "center" }}>
      <h2 style={{ color: "white" }}>Chess vs Computer (SignalR)</h2>
      <GameReviewContent
        moveHistory={moveHistory}
        currentMoveIndex={currentMoveIndex}
        position={position}
        disableAnimation={false}
        isInteractive={true}
        onSelectMoveIndex={handleSelectMoveIndex}
        onMoveIndexChange={handleMoveIndexChange}
        onSquareClick={onSquareClick}
        onPieceDrop={onDrop}
        customSquareStyles={customSquareStyles}
        isDraggablePiece={() => true}
        onPromotionPieceSelect={(piece, from, to) => makeMove(from, to, piece)}
        boardOrientation={playerColor}
      >
        <div style={{ marginTop: "10px", display: "flex", gap: "10px" }}>
          <button style={{ padding: "8px 15px", cursor: "pointer" }} onClick={resignGame}>
            Resign
          </button>
          <button style={{ padding: "8px 15px", cursor: "pointer" }} onClick={checkGameEnd}>
            Check Game State
          </button>
        </div>
      </GameReviewContent>
      <CustomDialog
        open={dialogOpen}
        onClose={() => setDialogOpen(false)}
        title={dialogTitle}
        content={dialogContent}
        actions={dialogActions}
      />
    </div>
  );
};

export default ChessboardComputer;
