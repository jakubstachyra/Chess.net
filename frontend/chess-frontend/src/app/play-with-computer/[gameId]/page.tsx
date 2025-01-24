"use client";

import React, { useEffect, useRef, useState } from "react";
import { HubConnection } from "@microsoft/signalr";
import { getConnection } from "../../services/signalrClient";

import { resign } from "../../services/gameService";
import { GameReviewContent } from "../../components/gameReview/gameReview";
import CustomDialog from "../../components/customDialog/customdialog";
import { Button } from "@mui/material";
import { useAppSelector } from "app/store/hooks";
import { Handlers } from "types/handlers";
import { Piece, Square } from "types/types";

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

  const [position, setPosition] = useState<string>("start");
  const [mappedMoves, setMappedMoves] = useState<{ [square: string]: string[] }>({});
  const [playerColor, setPlayerColor] = useState<"white" | "black">("white");
  const [moveHistory, setMoveHistory] = useState<MoveHistoryEntry[]>([]);
  const [currentMoveIndex, setCurrentMoveIndex] = useState<number>(0);
  const [dialogOpen, setDialogOpen] = useState<boolean>(false);
  const [dialogTitle, setDialogTitle] = useState<string>("");
  const [dialogContent, setDialogContent] = useState<React.ReactNode>(null);
  const [dialogActions, setDialogActions] = useState<React.ReactNode>(null);
  const [customSquareStyles, setCustomSquareStyles] = useState<{ [square: string]: React.CSSProperties }>({});
  const [gameEnded, setGameEnded] = useState<boolean>(false);

  const connectionRef = useRef<HubConnection | null>(null);

  const reduxUser = useAppSelector((state) => state.user);
  const user = reduxUser.user;

  // Refresh the game state when 'gameId' changes
  useEffect(() => {
    console.log("gameId changed to:", gameId);
    refreshGameState();
  }, [gameId]);

  // Keep the latest 'gameId' in a ref
  useEffect(() => {
    gameIdRef.current = gameId;
  }, [gameId]);

  useEffect(() => {
    let isMounted = true;

    // Partial<Handlers>: we only define some handler properties we actually need
    const handlers: Partial<Handlers> = {
      GameIsReady: async (gameId: number) => {
        if (!isMounted) return;
        console.log("GameIsReady => refresh state");
        if (!isMounted) return;

        // 1. Set local gameId
        setGameId(gameId);

        // 2. Assign the client to that game as Player1
        const hub = await getConnection();
        await hub.invoke("AssignClientIdToGame", gameId.toString());

        await refreshGameState();
      },
      OpponentMoved: async () => {
        if (!isMounted) return;
        console.log("OpponentMoved => refresh");
        await refreshGameState();
      },
      MoveHistoryUpdated: (entries: MoveHistoryEntry[]) => {
        console.log("MoveHistoryUpdated:", entries);
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

        if (entries.length > 0) {
          setPosition(entries[entries.length - 1].fen);
        }
      },
      PossibleMovesUpdated: (moves: string[]) => {
        console.log("PossibleMovesUpdated =>", moves);
        setMappedMoves(mapMoves(moves));
      },
      MoveAcknowledged: async () => {
        if (!isMounted) return;
        console.log("MoveAcknowledged => refresh state");
        await refreshGameState();
      },
      Error: async (error: string) => {
        console.warn("Server error:", error);
      },
      GameOver: (info: { gameId: number; winner: string; loser: string; reason: string; draw: string }) => {
        console.log("GameOver =>", info);
        setDialogTitle("Game Over");

        const isDraw = info.draw;
        setDialogContent(
          <div
            style={{
              display: "flex",
              flexDirection: "column",
              alignItems: "center",
              justifyContent: "center",
              width: "50%",
              height: "35%",
              margin: "0 auto",
              whiteSpace: "nowrap",
            }}
          >
            {isDraw ? (
              <p style={{ color: "yellow", margin: 0, textAlign: "center", fontWeight: "bold" }}>Draw</p>
            ) : info.winner === user?.userID ? (
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
        // The server forcibly calls for a disconnect
        // We'll stop the connection so the user can re-init if needed
        const hub = await getConnection();
        await hub.stop();
      },
    };

    async function initSignalR() {
      try {
        const hub = await getConnection(handlers);
        if (!isMounted) return;

        connectionRef.current = hub;
        console.log("SignalR connected, ID:", hub.connectionId);

        // Wait for connection to become active
        while (hub.state !== "Connected") {
          console.log("Waiting for connection to be active...");
          await new Promise((resolve) => setTimeout(resolve, 100));
        }

        // 4. Instruct the server to create a game with a computer
        //    The server will respond with "GameReadyServer"
        //    which sets local gameId & calls AssignClientIdToGame
        await hub.invoke("StartGameWithComputer", hub.connectionId, "classic");

      } catch (err) {
        console.error("SignalR init or StartGameWithComputer error:", err);
      }
    }

    initSignalR();

    return () => {
      isMounted = false;
      // If you do NOT want to keep the connection alive after unmount, uncomment:
      // connectionRef.current?.stop();
    };
  }, [user?.userID]);

  // Convert array of moves into a dictionary of from->target
  const mapMoves = (moves: string[]): { [square: string]: string[] } => {
    const result: { [square: string]: string[] } = {};
    for (const m of moves) {
      const [src, dst] = m.split(" ");
      if (!result[src]) {
        result[src] = [];
      }
      result[src].push(dst);
    }
    return result;
  };

  // Refresh game data (FEN, possible moves, etc.)
  async function refreshGameState(): Promise<void> {
    if (!gameId || !connectionRef.current) return;

    try {
      const hub = connectionRef.current;
      if (hub.state !== "Connected") {
        console.error("refreshGameState => Not connected");
        return;
      }
      if (!gameEnded) {
        const movesArray: string[] = await hub.invoke("GetPossibleMoves", gameId);
        setMappedMoves(mapMoves(movesArray));
      }
    } catch (err) {
      console.error("Error in refreshGameState:", err);
    }
  }

  // Make a move
  async function makeMove(sourceSquare: string, targetSquare: string, promotedPiece?: string): Promise<void> {
    const currentGameId = gameIdRef.current;
    if (!currentGameId || !connectionRef.current) return;

    if (gameEnded) {
      console.warn("Game ended. No more moves allowed");
      return;
    }

    try {
      const hub = connectionRef.current;
      if (hub.state !== "Connected") {
        console.error("Not connected => can't move");
        return;
      }

      const moveStr = promotedPiece
        ? `${sourceSquare}${targetSquare}${promotedPiece}`
        : `${sourceSquare}${targetSquare}`;
      console.log(`Attempting move: ${moveStr}`);
      await hub.invoke("ReceiveMoveAsync", currentGameId, moveStr);
    } catch (err) {
      if ((err as Error)?.message?.includes("Invocation canceled")) {
        console.warn("makeMove => invocation canceled");
      } else {
        console.error("makeMove error:", err);
      }
    }
  }

  // Called when user drops a piece on the board:
  const onDropWrapper = (sourceSquare: Square, targetSquare: Square, piece: Piece): boolean => {
    (async () => {
      const possibleMoves = mappedMoves[sourceSquare] || [];
      if (possibleMoves.includes(targetSquare)) {
        await makeMove(sourceSquare, targetSquare);
      } else {
        alert("Invalid move!");
      }
    })();
    return true;
  };

  // Promotion logic
  const handlePromotionSelect = (fromSquare?: string, toSquare?: string, promotionPiece?: string): boolean => {
    if (fromSquare && toSquare && promotionPiece) {
      console.log(`Promoting from ${fromSquare} to ${toSquare} as ${promotionPiece}`);
      makeMove(fromSquare, toSquare, promotionPiece).catch((err) => {
        console.error("promotion error:", err);
      });
      return true;
    }
    return false;
  };

  // Called when user clicks square
  const onSquareClick = (square: string): void => {
    const possible = mappedMoves[square] || [];
    const styles: Record<string, React.CSSProperties> = {};
    possible.forEach((target) => {
      styles[target] = {
        backgroundColor: "rgba(0, 255, 0, 0.5)",
        borderRadius: "50%",
      };
    });
    setCustomSquareStyles(styles);
  };

  // Resign
  async function resignGame(): Promise<void> {
    if (!gameId) return;
    try {
      await resign(gameId);
      // The server triggers "GameOver" => sets gameEnded => user can re-use connection
    } catch (err) {
      console.error("Resign error:", err);
    }
  }

  // handle move history navigation
  const handleSelectMoveIndex = (index: number): void => {
    setCurrentMoveIndex(index);
    const fen = moveHistory[index]?.fen;
    if (fen) setPosition(fen);
  };

  const handleMoveIndexChange = (index: number): void => {
    setCurrentMoveIndex(index);
    const fen = moveHistory[index]?.fen;
    if (fen) setPosition(fen);
  };

  return (
    <div style={{ display: "flex", flexDirection: "column", alignItems: "center" }}>
      <h2 style={{ color: "white" }}>Computer Game</h2>
      <GameReviewContent
        moveHistory={moveHistory}
        currentMoveIndex={currentMoveIndex}
        position={position}
        disableAnimation={false}
        isInteractive={!gameEnded}
        onSelectMoveIndex={handleSelectMoveIndex}
        onMoveIndexChange={handleMoveIndexChange}
        onSquareClick={onSquareClick}
        onPieceDrop={onDropWrapper}
        customSquareStyles={customSquareStyles}
        isDraggablePiece={() => !gameEnded}
        onPromotionPieceSelect={handlePromotionSelect}
        boardOrientation={playerColor}
      >
        <div style={{ marginTop: "10px", display: "flex", gap: "10px" }}>
          <Button onClick={resignGame} variant="contained" sx={{ color: "#fff" }}>
            Resign
          </Button>
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

// Style for your buttons container
const buttonsContainerStyles: React.CSSProperties = {
  display: "flex",
  justifyContent: "space-between",
  gap: "10px",
  marginTop: "auto",
  width: "100%",
};
