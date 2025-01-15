"use client";

import React, { useEffect, useState } from "react";
import { useParams } from "next/navigation";
import { useSelector } from "react-redux";
import { GameReviewContent } from "../../components/gameReview/gameReview";
import { Button } from "@mui/material";
import CustomDialog from "../../components/customDialog/customdialog";
import Timer from "app/components/timer/timer";
import BackgroundUI from "app/components/backgroundUI/pages";

import {
  resign,
  fetchFen,
  fetchMoves,
  fetchWhoToMove,
  fetchGameState,
} from "../../services/gameService";

// 1) import metody do uzyskania połączenia
import { getConnection } from "../../services/signalrClient";

interface MoveHistoryEntry {
  moveNumber: number;
  fen: string;
  move: string;
  whiteRemainingTimeMs: number | null;
  blackRemainingTimeMs: number | null;
}

const ChessboardOnline = () => {
  const [position, setPosition] = useState("start");
  const [mappedMoves, setMappedMoves] = useState<{ [key: string]: string[] }>({});
  const [whoToMove, setWhoToMove] = useState<number | null>(null);
  const [playerColor, setPlayerColor] = useState<string>("white");
  const [isGameReady, setIsGameReady] = useState(false);
  const { gameId } = useParams();
  const reduxUser = useSelector((state) => state.user);
  const user = reduxUser.user;

  const [player1Time, setPlayer1Time] = useState(0);
  const [player2Time, setPlayer2Time] = useState(0);
  const [gameEnded, setGameEnded] = useState(false);
  const [gameResult, setGameResult] = useState("");

  const [customSquareStyles, setCustomSquareStyles] = useState<{ [square: string]: React.CSSProperties }>({});

  // States for move history and reviewing
  const [moveHistory, setMoveHistory] = useState<MoveHistoryEntry[]>([]);
  const [currentMoveIndex, setCurrentMoveIndex] = useState<number>(0);

  const [dialogOpen, setDialogOpen] = useState<boolean>(false);
  const [dialogTitle, setDialogTitle] = useState<string>("");
  const [dialogContent, setDialogContent] = useState<React.ReactNode>(null);
  const [dialogActions, setDialogActions] = useState<React.ReactNode>(null);

  // 2) Będziemy przechowywać obiekt połączenia globalnie (lub w stanie).
  //    Nie trzeba go tworzyć wielokrotnie przy każdym ruchu.
  let hubConnection: any = null;

  const addStaticData = () => {
    setMoveHistory(() => [
      { move: "start" },
      { move: "e4" },
      { move: "e5" },
      { move: "Bc4" },
      { move: "Nc6 "},
      { move: "Qh5 "},
      { move: "Nf6 "},
      { move: "Qxf7# "},
    ]);
  };

  // Helper for mapping moves to highlight squares
  const mapMoves = (moves: string[]): Record<string, string[]> => {
    const movesMapping: Record<string, string[]> = {};
    moves.forEach((move) => {
      // e.g. "e2 e4" => source="e2", target="e4"
      const [source, target] = move.split(" ");
      if (!movesMapping[source]) movesMapping[source] = [];
      movesMapping[source].push(target);
    });
    return movesMapping;
  };

  const refreshGameState = async () => {
    try {
      const fenResponse = await fetchFen(gameId);
      setPosition(fenResponse.data);

      const whoToMoveResponse = await fetchWhoToMove(gameId);
      setWhoToMove(whoToMoveResponse.data);

      const movesResponse = await fetchMoves(gameId);
      setMappedMoves(mapMoves(movesResponse.data));
    } catch (error) {
      console.error("Error refreshing game state:", error);
    }
  };

  // 3) Jednorazowa inicjalizacja handlerów i dołączenie do gry (useEffect).
  useEffect(() => {
    let isMounted = true;

    const initGameHandlers = async () => {
      // Definiujemy obiekt z handlerami (eventami z serwera -> klient).
      const gameHandlers = {
        PlayerDisconnected: () => {
          if (!isMounted) return;
          alert("Opponent disconnected. The game is over.");
          setIsGameReady(false);
          setGameEnded(true);
          setGameResult("Opponent disconnected");
        },
        OpponentMoved: async () => {
          if (!isMounted) return;
          // Refresh game state (moves, fen, etc.)
          await refreshGameState();
        },
        UpdateTimers: (p1Time: number, p2Time: number) => {
          if (!isMounted) return;
          setPlayer1Time(p1Time);
          setPlayer2Time(p2Time);
        },
        GameIsReady: async () => {
          if (!isMounted) return;
          setIsGameReady(true);
          addStaticData();
          // once game is ready, we fetch initial state
          await refreshGameState();
        },
        TimeOver: (color: string) => {
          if (!isMounted) return;
          alert(`Your time is over! You lost as ${color}.`);
          setGameEnded(true);
          setGameResult("Time out");
        },
        OpponentTimeOver: (color: string) => {
          if (!isMounted) return;
          alert(`Opponent's time is over! Opponent was ${color}. You win.`);
          setGameEnded(true);
          setGameResult("Opponent time out");
        },
        GameOver: (info: { gameId: number; winner: string; loser: string; reason: string }) => {
          setGameResult(`Game Over. Reason: ${info.reason} (Winner: ${info.winner})`);

          setDialogTitle("Game Over");
          setDialogContent(
            <div
              style={{
                display: "flex",
                flexDirection: "column",
                alignItems: "center",
                justifyContent: "center",
                width: "30%",
                height: "35%",
                margin: "0 auto",
              }}
            >
              {info.winner === user.userID ? (
                <p style={{ color: "green", margin: 0, textAlign: "center", fontStyle: "bold" }}>You Won</p>
              ) : (
                <p style={{ color: "red", margin: 0, textAlign: "center" }}>You Lost</p>
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
      };

      try {
        // Uzyskujemy (lub tworzymy) połączenie SignalR z przekazanymi handlerami.
        const hub = await getConnection(gameHandlers);

        // Zapamiętujemy je w zmiennej (lokalnie lub np. w ref). 
        // W tej wersji – zapiszemy je w zmiennej "hubConnection" globalnie w komponencie.
        hubConnection = hub;

        // Dołączamy do gry i pobieramy kolor, ale tylko jeśli mamy gameId
        if (gameId) {
          await hub.invoke("AssignClientIdToGame", gameId);
          const color = await hub.invoke("GetPlayerColor", gameId);
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
          if (existingHub?.connectionStarted) {
            await existingHub.stop();
            console.log("SignalR connection stopped in ChessboardOnline cleanup.");
          }
        } catch (err) {
          console.error("Error stopping the SignalR connection in cleanup:", err);
        }
      })();
    };
  }, [gameId]);

  const checkGameState = async () => {
    try {
      const response = await fetchGameState(gameId);
      const isGameEnded = response.data;
      if (isGameEnded) {
        setGameEnded(true);
        setGameResult("Game Over (checkmate/time)!");
      }
      return isGameEnded;
    } catch (error) {
      console.error("Error checking game state:", error);
      return false;
    }
  };

  // 4) Funkcja wysyłająca ruch do Huba (SignalR).
  //    Wywołujemy wewnątrz makeMove -> sendMove().
  async function sendMove(gameId: number, move: string) {
    try {
      // Pobieramy (lub reuse) istniejące połączenie
      const hub = await getConnection();
      await hub.invoke("ReceiveMoveAsync", gameId, move);
      console.log("Move sent via SignalR:", move);
    } catch (err) {
      console.error("Error sending move:", err);
    }
  }

  // Główna funkcja, która obsługuje wykonanie ruchu z perspektywy UI (drag&drop).
  const makeMove = async (sourceSquare: string, targetSquare: string, promotedPiece?: string) => {
    try {
      const move = promotedPiece
        ? `${sourceSquare}${targetSquare}${promotedPiece}`
        : `${sourceSquare}${targetSquare}`;

      // Wyślij ruch na serwer (SignalR).
      await sendMove(Number(gameId), move);

      console.log("Move sent to server via SignalR");

      // Powiadom serwer, że zakończyłeś ruch, aby wystartować zegar przeciwnika
      const hub = await getConnection();
      await hub.invoke("YourMove", gameId);

      // Odśwież stan gry
      await refreshGameState();
      await checkGameState();
    } catch (error) {
      console.error("Error making move:", error);
    }
  };

  // Wywoływana, gdy użytkownik upuści figurę na docelowe pole (react-chessboard onDrop).
  const onDrop = async (sourceSquare: string, targetSquare: string) => {
    const possibleMoves = mappedMoves[sourceSquare];
    if (possibleMoves?.includes(targetSquare)) {
      await makeMove(sourceSquare, targetSquare);
      return true;
    } else {
      alert("Invalid move!");
      return false;
    }
  };

  // Podświetlanie możliwych ruchów po kliknięciu pola
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

  const handleSelectMoveIndex = (index: number) => {
    setCurrentMoveIndex(index);
    const selectedFen = moveHistory[index]?.fen;
    if (selectedFen) setPosition(selectedFen);
  };

  const handleMoveIndexChange = (index: number) => {
    setCurrentMoveIndex(index);
    const selectedFen = moveHistory[index]?.fen;
    if (selectedFen) setPosition(selectedFen);
  };

  // Obsługa poddania
  const resignGame = async () => {
    try {
      await resign(gameId);
    } catch (error) {
      console.error("Error in resignGame:", error);
    }
  };

  return (
    <div style={{ display: "flex", flexDirection: "column", alignItems: "center", marginTop: "50px" }}>
      <div style={{ width: "90%", display: "flex" }}>
        <h1 style={{ color: "white", fontSize: "22px" }}>{"Guest"}</h1>
        <div style={{ width: "10%", height: "10%", display: "flex", alignItems: "start", gap: "350px" }}>
          <div></div>
          <div>
            <Timer timeMs={playerColor === "white" ? player2Time * 1000 : player1Time * 1000} />
          </div>
        </div>
      </div>

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
        boardOrientation={playerColor === "white" ? "white" : "black"}
      >
        <div style={buttonsContainerStyles}>
          <Button
            style={{ ...buttonStyle, backgroundColor: "#FF7700" }}
            title="Report opponent if you think he is cheating"
          >
            Report
          </Button>
          <Button
            style={{ ...buttonStyle, backgroundColor: "#4C9AFF" }}
            title="Propose draw to your opponent"
          >
            Draw
          </Button>
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

const buttonsContainerStyles = {
  display: "flex",
  justifyContent: "space-between",
  gap: "10px",
  marginTop: "auto",
};

const buttonStyle = {
  padding: "10px 30px",
  fontSize: "16px",
  fontWeight: "bold" as const,
  color: "#fff",
  backgroundColor: "#DD0000",
  border: "none",
  borderRadius: "5px",
  cursor: "pointer",
  boxShadow: "0 4px 30px rgba(0, 0, 0, 0.5)",
};
