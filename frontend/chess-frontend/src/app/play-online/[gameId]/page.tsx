"use client";

import React, { useEffect, useRef, useState, useCallback } from "react";
import { useParams } from "next/navigation";
import { useSelector } from "react-redux";
import { GameReviewContent } from "../../components/gameReview/gameReview";
import { Button } from "@mui/material";
import CustomDialog from "../../components/customDialog/customdialog";
import Timer from "app/components/timer/timer";

import {
  resign,
  reportPlayer,
} from "../../services/gameService";

// Import methods to get connection
import { getConnection } from "../../services/signalrClient";
import { HubConnection } from "@microsoft/signalr";
import {MoveHistoryEntry, Square , Piece, PromotionPieceOption}from "types/types";
// Zakładając, że masz zdefiniowany RootState w swoim store
import { RootState } from "../../store/store"; // Dostosuj ścieżkę w zależności od struktury projektu


const ChessboardOnline: React.FC = () => {
  const [position, setPosition] = useState<string>("start");
  const [mappedMoves, setMappedMoves] = useState<Record<string, string[]>>({});
  const [playerColor, setPlayerColor] = useState<string>("white");
  const { gameId } = useParams();
  const reduxUser = useSelector((state: RootState) => state.user);
  const user = reduxUser.user;

  const [player1Time, setPlayer1Time] = useState<number>(0);
  const [player2Time, setPlayer2Time] = useState<number>(0);
  const [gameEnded, setGameEnded] = useState<boolean>(false);

  const [customSquareStyles, setCustomSquareStyles] = useState<Record<string, React.CSSProperties>>({});

  // States for move history and reviewing
  const [moveHistory, setMoveHistory] = useState<MoveHistoryEntry[]>([]);
  const [currentMoveIndex, setCurrentMoveIndex] = useState<number>(0);

  const [dialogOpen, setDialogOpen] = useState<boolean>(false);
  const [dialogTitle, setDialogTitle] = useState<string>("");
  const [dialogContent, setDialogContent] = useState<React.ReactNode>(null);
  const [dialogActions, setDialogActions] = useState<React.ReactNode>(null);

  // States for proposing draw
  const [isProposingDraw, setIsProposingDraw] = useState<boolean>(false);
  const [drawAnimationText, setDrawAnimationText] = useState<string>("Draw");
  const [showDrawResponseButtons, setShowDrawResponseButtons] = useState<boolean>(false);

  const [opponentName, setOpponentName] = useState<string>("Loading...");
  const [opponentId, setOpponentId] = useState<string | null>(null);
  const [reported, setReported] = useState<boolean>(false);

  // UseRef for hubConnection
  const hubConnection = useRef<HubConnection | null>(null);

  // Remove unused imports
  // useMediaQuery and BackgroundUI have been removed from imports and are not used in the component

  useEffect(() => {
    let animationInterval: NodeJS.Timeout;
    if (isProposingDraw) {
      const states = [".", "..", "..."];
      let index = 0;
      animationInterval = setInterval(() => {
        setDrawAnimationText(states[index % states.length]);
        index++;
      }, 500);
    } else {
      setDrawAnimationText("Draw");
    }
    return () => {
      if (animationInterval) clearInterval(animationInterval);
    };
  }, [isProposingDraw]);

  // Helper for mapping moves to highlight squares
  const mapMoves = (moves: string[]): Record<string, string[]> => {
    const movesMapping: Record<string, string[]> = {};
    moves.forEach((move) => {
      const [source, target] = move.split(" ");
      if (!movesMapping[source]) movesMapping[source] = [];
      movesMapping[source].push(target);
    });
    return movesMapping;
  };

  // Memoizacja funkcji refreshGameState
  const refreshGameState = useCallback(async () => {
    if (!gameId) return;
    if (gameEnded) return;
    try {
      const hub = await getConnection();
      const movesArray: string[] = await hub.invoke("GetPossibleMoves", Number(gameId));
      setMappedMoves(mapMoves(movesArray));
    } catch (err) {
      console.error("Error in refreshGameState:", err);
    }
  }, [gameId, gameEnded]);

  // Initialize handlers and join game
  useEffect(() => {
    let isMounted = true;

    const initGameHandlers = async () => {
      const gameHandlers = {
        PlayerDisconnected: () => {
          if (!isMounted) return;
          alert("Opponent disconnected. The game is over.");
          setGameEnded(true);
        },
        OpponentInfo: (data: { username: string; userId: string }) => {
          if (!isMounted) return;
          console.log("Opponent info", data);
          setOpponentName(data.username);
          setOpponentId(data.userId);
        },
        OpponentInfo: (data: { username: string; userId: string }) => {
          if(!isMounted) return;
          console.log("Obieram przeciwnika", data);
            setOpponentName(data.username);
            setOpponentId(data.userId);
        },
        OpponentMoved: async () => {
          if (!isMounted) return;
          await refreshGameState();
        },
        UpdateTimers: (p1Time: number, p2Time: number) => {
          if (!isMounted) return;
          setPlayer1Time(p1Time);
          setPlayer2Time(p2Time);
        },
        GameIsReady: async () => {
          if (!isMounted) return;
          await refreshGameState();
          const hub = await getConnection();
          await hub.invoke("GetOpponentInfo", Number(gameId));
        },
        DrawProposed: async () => {
          if (!isMounted) return;
          setShowDrawResponseButtons(true);
        },
        DrawRejected: async () => {
          if (!isMounted) return;
          setShowDrawResponseButtons(false);
          setIsProposingDraw(false);
        },
        MoveHistoryUpdated: (entries: MoveHistoryEntry[]) => { // Usuń nieużywany parametr '_'
          // Add initial entry if not present
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
          const lastFen = entries.length > 0 ? entries[entries.length - 1].fen : "start";
          setPosition(lastFen);
        },
        PossibleMovesUpdated: (moves: string[]) => {
          setMappedMoves(mapMoves(moves));
        },
        GameOver: (info: { gameId: number; winner: string; loser: string; reason: string; draw: string }) => {
          console.log(info);
          setDialogTitle("Game Over");
          const isDraw = info.draw === "true";

          setGameEnded(true);

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
                whiteSpace: "nowrap"
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
        },
        Disconnect: async () => {
          const hub = await getConnection();
          await hub.stop();
        },
      };

      try {
        const hub = await getConnection(gameHandlers);
        hubConnection.current = hub; // Zapisz połączenie w useRef

        // Join the game and get color, if gameId is present
        if (gameId) {
          await hub.invoke("AssignClientIdToGame", gameId);
          const color: string = await hub.invoke("GetPlayerColor", gameId);
          setPlayerColor(color);
        }
      } catch (error) {
        console.error("Error connecting or invoking hub methods:", error);
      }
    };

    initGameHandlers();

    return () => {
      isMounted = false;
      (async () => {
        try {
          const existingHub = await getConnection();
          if (existingHub?.state == "Connected") {
            await existingHub.stop();
            console.log("SignalR connection stopped in ChessboardOnline cleanup.");
          }
        } catch (err) {
          console.error("Error stopping the SignalR connection in cleanup:", err);
        }
      })();
    };
  }, [gameId, refreshGameState, user!.userID]); // Dodaj refreshGameState jako zależność

  // Function to send move via SignalR
  const sendMove = async (gameId: number, move: string): Promise<void> => {
    try {
      const hub = await getConnection();
      await hub.invoke("ReceiveMoveAsync", gameId, move);
      declineDraw();
      console.log("Move sent via SignalR:", move);
    } catch (err) {
      console.error("Error sending move:", err);
    }
  };

  // Main function to handle making a move
  const makeMove = async (sourceSquare: string, targetSquare: string, promotedPiece?: string): Promise<void> => {
    try {
      const hub = await getConnection();
      const move = promotedPiece
        ? `${sourceSquare}${targetSquare}${promotedPiece}`
        : `${sourceSquare}${targetSquare}`;

      // Send move to server
      await sendMove(Number(gameId), move);

      // Notify server that you have moved, to start opponent's timer
      await hub.invoke("YourMove", gameId);

      // Refresh game state
      await refreshGameState();
    } catch (error) {
      console.error("Error making move:", error);
    }
  };

  // Function called when user drops a piece on board
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
  

  // Highlight possible moves after clicking a square
  const onSquareClick = (square: string): void => {
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

  const handleSelectMoveIndex = (index: number): void => {
    setCurrentMoveIndex(index);
    const selectedFen = moveHistory[index]?.fen;
    if (selectedFen) setPosition(selectedFen);
  };

  const handleMoveIndexChange = (index: number): void => {
    setCurrentMoveIndex(index);
    const selectedFen = moveHistory[index]?.fen;
    if (selectedFen) setPosition(selectedFen);
  };

  const resignGame = async (): Promise<void> => {
    try {
      await resign(gameId);
    } catch (error) {
      console.error("Error in resignGame:", error);
    }
  };

  const proposeDraw = async (): Promise<void> => {
    try {
      setIsProposingDraw(true);
      const hub = await getConnection();
      await hub.invoke("DrawProposed", Number(gameId));
    } catch (err) {
      console.error("Error proposing draw:", err);
      setIsProposingDraw(false);
    }
  };

  const acceptDraw = async (): Promise<void> => {
    try {
      const hub: HubConnection = await getConnection();
      await hub.invoke("DrawAccept", Number(gameId));
      setShowDrawResponseButtons(false);
    } catch (err) {
      console.error("Error accepting draw:", err);
    }
  };

  const declineDraw = async (): Promise<void> => {
    try {
      const hub: HubConnection = await getConnection();
      await hub.invoke("DrawRejected", Number(gameId));
      setShowDrawResponseButtons(false);
      setIsProposingDraw(false);
    } catch (err) {
      console.error("Error declining draw:", err);
    }
  };

  const report = async (): Promise<void> => {
    try {
      if (opponentId) {
        await reportPlayer(opponentId, gameId);
        setReported(true);
      }
    } catch (error) {
      console.error("Error in reportPlayer:", error);
    }
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
    <div style={{ display: "flex", flexDirection: "column", alignItems: "center", marginTop: "50px" }}>
      <div style={{ width: "90%", display: "flex" }}>
        <h1 style={{ color: "white", fontSize: "22px" }}>{opponentName || "Guest"}</h1>
        <div style={{ width: "10%", height: "10%", display: "flex", alignItems: "start", gap: "350px" }}>
          <div></div>
          <div>
            <Timer timeMs={playerColor === "white" ? player1Time * 1000 : player2Time * 1000} />
          </div>
        </div>
      </div> 

      <GameReviewContent
        moveHistory={moveHistory}
        currentMoveIndex={currentMoveIndex}
        position={position}
        disableAnimation={true}
        isInteractive={true}
        onSelectMoveIndex={handleSelectMoveIndex}
        onMoveIndexChange={handleMoveIndexChange}
        onSquareClick={onSquareClick}
        onPieceDrop={onDropWrapper}
        customSquareStyles={customSquareStyles}
        isDraggablePiece={() => true}
        onPromotionPieceSelect={handlePromotionSelect}
        boardOrientation={playerColor === "white" ? "white" : "black"}
      >
        <div style={buttonsContainerStyles}>
          <Button
            style={{
              ...buttonStyle,
              backgroundColor: reported ? "#999" : "#FF7700",
              cursor: reported ? "not-allowed" : "pointer",
            }}
            title="Report opponent if you think he is cheating"
            onClick={!reported ? report : undefined}
            disabled={reported}
          >
            {reported ? "Reported" : "Report"}
          </Button>

          {showDrawResponseButtons ? (
            <>
              <Button
                style={{ ...decisitionButtonStyle, backgroundColor: "green" }}
                onClick={acceptDraw}
                title="Accept draw"
              >
                ✓
              </Button>
              <Button
                style={{ ...decisitionButtonStyle, backgroundColor: "red" }}
                onClick={declineDraw}
                title="Decline draw"
              >
                ✕
              </Button>
            </>
          ) : (
            <Button
              style={{ ...fixedButtonStyle, backgroundColor: "#4C9AFF" }}
              title="Propose draw to your opponent"
              onClick={proposeDraw}
              disabled={isProposingDraw}
            >
              {drawAnimationText}
            </Button>
          )}

          <Button style={buttonStyle} onClick={resignGame} title="Give up a game">
            Resign
          </Button>
        </div>
      </GameReviewContent>

      <div style={{ width: "90%", display: "flex" }}>
        <h1 style={{ color: "white", fontSize: "22px" }}>{user?.username || "Guest"}</h1>
        <div style={{ width: "10%", height: "10%", display: "flex", alignItems: "start", gap: "350px" }}>
          <div></div>
          <div>
            <Timer timeMs={playerColor === "white" ? player1Time * 1000 : player2Time * 1000} />
          </div>
        </div>
      </div>     
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

export default ChessboardOnline;

// Styles
const buttonsContainerStyles: React.CSSProperties = {
  display: "flex",
  justifyContent: "space-between",
  gap: "10px",
  marginTop: "auto",
  width: "100%",
};

const buttonStyle: React.CSSProperties = {
  padding: "10px 30px",
  fontSize: "16px",
  fontWeight: "bold",
  color: "#fff",
  backgroundColor: "#DD0000",
  border: "none",
  borderRadius: "5px",
  cursor: "pointer",
  boxShadow: "0 4px 30px rgba(0, 0, 0, 0.5)",
  margin: "1px",
};

const fixedButtonStyle: React.CSSProperties = {
  ...buttonStyle,
  minWidth: "100px",
  textAlign: "center",
};

const decisitionButtonStyle: React.CSSProperties = {
  ...buttonStyle,
  maxWidth: "30px",
  textAlign: "center",
  padding: "5px",
};
