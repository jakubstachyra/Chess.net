"use client";

import React, { useState, useEffect, useRef } from "react";
import { HubConnection } from "@microsoft/signalr";
import { getConnection } from "../../services/signalrClient";

import {
  resign
} from "../../services/gameService";
import { GameReviewContent } from "../../components/gameReview/gameReview";
import CustomDialog from "../../components/customDialog/customdialog";
import { Button } from "@mui/material";
import { useAppSelector } from "app/store/hooks";
import { Handlers } from "types/handlers";
import {Piece, Square, PromotionPieceOption} from "types/types"

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
  const [position, setPosition] = useState("start");
  const [mappedMoves, setMappedMoves] = useState<{ [square: string]: string[] }>({});
  const [playerColor, setPlayerColor] = useState<"white" | "black">("white");
  const [moveHistory, setMoveHistory] = useState<MoveHistoryEntry[]>([]);
  const [currentMoveIndex, setCurrentMoveIndex] = useState<number>(0);
  const [dialogOpen, setDialogOpen] = useState<boolean>(false);
  const [dialogTitle, setDialogTitle] = useState<string>("");
  const [dialogContent, setDialogContent] = useState<React.ReactNode>(null);
  const [dialogActions, setDialogActions] = useState<React.ReactNode>(null);
  const [customSquareStyles, setCustomSquareStyles] = useState<{ [square: string]: React.CSSProperties }>({});
  const [gameEnded, setGameEnded] = useState(false);

  const connectionRef = useRef<HubConnection | null>(null);

  const reduxUser = useAppSelector((state) => state.user);
  const user = reduxUser.user;
  
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
      //const fenResp = await fetchFen(gameId);
      //setPosition(fenResp.data);
      //console.log("Fen updated:", fenResp.data);  
      //const whoResp = await fetchWhoToMove(gameId);
      //setWhoToMove(whoResp.data);

      const hub = connectionRef.current;
      if (hub.state !== "Connected") {
        console.error("refreshGameState: Connection is not connected");
        return;
      }
      if(!gameEnded)
      {
        const movesArray = await hub.invoke("GetPossibleMoves", Number(gameId));
        setMappedMoves(mapMoves(movesArray));
      }

    } catch (err) {
      console.error("Error in refreshGameState:", err);
    }
  };

  const makeMove = async (sourceSquare: string, targetSquare: string, promotedPiece?: string) => {
    const currentGameId = gameIdRef.current; // Użycie referencji
  
    if (!currentGameId) {
      console.error("Brak gameId, ruch nie może zostać wykonany.");
      return;
    }
    if (!connectionRef.current) {
      console.error("SignalR connection is not ready yet.");
      return;
    }
  
    try {
      // Sprawdzenie, czy gra już się skończyła
      if (gameEnded) {
        return;
      }
  
      const hub = connectionRef.current;
      if (hub.state !== "Connected") {
        console.error("makeMove: Connection is not connected");
        return;
      }
  
      // Tworzenie ciągu ruchu
      const moveStr = promotedPiece
        ? `${sourceSquare}${targetSquare}${promotedPiece}`
        : `${sourceSquare}${targetSquare}`;
      console.log(`moveStr: ${moveStr}`);
  
      // Wywołanie tylko jeśli połączenie nadal jest aktywne
      if (!gameEnded && hub.state === "Connected") {
        await hub.invoke("ReceiveMoveAsync", currentGameId, moveStr);
      }
    } catch (err) {
      if ((err as Error)?.name === "Error" && (err as Error)?.message.includes("Invocation canceled")) {
        console.warn("makeMove: Połączenie zostało zamknięte. Ignoruję ruch.");
      } else {
        console.error("Error in makeMove:", err);
      }
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

  useEffect(() => {
    let isMounted = true;

    const handlers: Partial<Handlers> = {
      GameReadyServer: async (serverGameId: number) => {
        console.log("GameReady, gameId =", serverGameId);
        setGameId(serverGameId); // Aktualizacja stanu
        gameIdRef.current = serverGameId; // Aktualizacja referencji
        await refreshGameState();
      },
      GameIsReady: async () => {
        if (!isMounted) return;
        console.log("GameIsReady => pobieram stan gry...");
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
      },
      GameOver: (info: { gameId: number; winner: string; loser: string; reason: string; draw: string }) => {
        
        console.log(info);
        setDialogTitle("Game Over");
        
        const isDraw = info.draw;
      
        setDialogContent(
          <div
            style={{
              display: "flex",
              flexDirection: "column",
              alignItems: "center",
              justifyContent: "center",
              width: "50%",  // zwiększona szerokość
              height: "35%",
              margin: "0 auto",
              whiteSpace: "nowrap"  // zapobiega zawijaniu tekstu
            }}
          >
            {isDraw ? (
              <p style={{ color: "yellow", margin: 0, textAlign: "center", fontWeight: "bold" }}>Draw</p>
            ) : info.winner === user!.userID ? (
              <p style={{ color: "green", margin: 0, textAlign: "center", fontWeight: "bold" }}>You Won</p>
            ) : (
              <p style={{ color: "red", margin: 0, textAlign: "center", fontWeight: "bold" }}>You Lost</p>
            )}
            <p style={{ color: "white", textAlign: "center", marginTop: "1rem" }}>
              {info.reason}
            </p>
          </div>
        );
      
        setDialogActions(
          <div style={buttonsContainerStyles}>
            <Button
              variant="contained"
              sx={{
                backgroundColor: "darkgreen",
                color: "white",
              }}
              onClick={() => setDialogOpen(false)}
            >
              Play Again
            </Button>
            <Button
              onClick={() => setDialogOpen(false)}
              color="primary"
              variant="outlined"
              sx={{ color: "white", borderColor: "white" }}
            >
              Close
            </Button>
          </div>
        );
        setDialogOpen(true);
        setGameEnded(true);
      },
      Disconnect: async () => {

        const hub = await getConnection();
        await hub.stop();
      },
      
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
        const color: string = await hub.invoke("GetPlayerColor", gameIdRef.current);
        setPlayerColor(color === "white" ? "white": "black");
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
    const styles: Record<string, React.CSSProperties> = moves.reduce((acc, target: string) => {
      acc[target] = {
        backgroundColor: "rgba(0, 255, 0, 0.5)",
        borderRadius: "50%",
      };
      return acc;
    }, {} as Record<string, React.CSSProperties>);
    setCustomSquareStyles(styles);
  };

  const resignGame = async () => {
    try {
      await resign(gameId);
    } catch (error) {
      console.error("Error in resignGame:", error);
    }
  };

  const onDropWrapper = (
    sourceSquare: Square,
    targetSquare: Square,
    piece: Piece
  ): boolean => {
    // Asynchroniczne działanie zostaje wywołane, ale wynik nie blokuje synchronizacji
    (async () => {
      const possibleMoves = mappedMoves[sourceSquare];
      if (possibleMoves?.includes(targetSquare)) {
        await makeMove(sourceSquare, targetSquare);
      } else {
        alert("Invalid move!");
      }
    })();
  
    // Zawsze zwracamy `true` dla synchronizacji
    return true;
  };
  const handlePromotionSelect = (
    fromSquare?: string,
    toSquare?: string,
    promotionPiece?: string
  ): boolean => {
    if (fromSquare && toSquare && promotionPiece) {
      console.log(`Promoting from ${fromSquare} to ${toSquare} as ${promotionPiece}`);
      makeMove(fromSquare, toSquare, promotionPiece).catch((err) => {
        console.error("Error during promotion move:", err);
      });
      return true;
    }
    return false;
  };
  
  return (
    <div style={{ display: "flex", flexDirection: "column", alignItems: "center" }}>
      <h2 style={{ color: "white" }}>Computer</h2>
      <GameReviewContent
        moveHistory={moveHistory}
        currentMoveIndex={currentMoveIndex}
        position={position}
        disableAnimation={false}
        isInteractive={true}
        onSelectMoveIndex={handleSelectMoveIndex}
        onMoveIndexChange={handleMoveIndexChange}
        onSquareClick={onSquareClick}
        onPieceDrop={onDropWrapper}
        customSquareStyles={customSquareStyles}
        isDraggablePiece={() => true}
        onPromotionPieceSelect={handlePromotionSelect}
        boardOrientation={playerColor}
      >
        <div style={{ marginTop: "10px", display: "flex", gap: "10px" }}>
          <button style={{ padding: "8px 15px", cursor: "pointer" }} onClick={resignGame}>
            Resign
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


const buttonsContainerStyles = {
  display: "flex",
  justifyContent: "space-between",
  gap: "10px",
  marginTop: "auto",
  width: "100%",
};

