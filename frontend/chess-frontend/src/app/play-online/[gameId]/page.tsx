"use client";

import React, { useEffect, useState, useRef } from "react";
import ChessboardComponent from "../../components/chessBoard/chessBoard";
import { useParams } from "next/navigation";
import { getConnection } from "../../services/signalrClient"; // zmienione
import {
  fetchFen,
  fetchMoves,
  fetchWhoToMove,
  sendMove,
  fetchGameState,
} from "../../services/gameService";

const ChessboardOnline = () => {
  const [position, setPosition] = useState("start");
  const [mappedMoves, setMappedMoves] = useState({});
  const [whoToMove, setWhoToMove] = useState<number | null>(null); 
  const [playerColor, setPlayerColor] = useState<string | null>(null);
  const [isGameReady, setIsGameReady] = useState(false);
  const [boardOrientation, setBoardOrientation] = useState("white");
  const { gameId } = useParams();
  
  const [player1Time, setPlayer1Time] = useState(0);
  const [player2Time, setPlayer2Time] = useState(0);
  const [gameEnded, setGameEnded] = useState(false);
  const [gameResult, setGameResult] = useState("");

  // Map moves for highlighting
  const mapMoves = (moves: string[]): Record<string, string[]> => {
    const movesMapping: Record<string, string[]> = {};
    moves.forEach((move) => {
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

  useEffect(() => {
    let isMounted = true;

    const initGameHandlers = async () => {
      // Zdefiniuj handlery ściśle związane z rozgrywką
      const gameHandlers = {
        PlayerDisconnected: () => {
          if (!isMounted) return;
          alert("Opponent disconnected. The game is over.");
          setIsGameReady(false);
        },
        OpponentMoved: async () => {
          if (!isMounted) return;
          console.log("Opponent moved");
          await refreshGameState();
        },
        UpdateTimers: (p1Time: number, p2Time: number) => {
          if (!isMounted) return;
          console.log(`Game ${gameId} - Player 1: ${p1Time}s, Player 2: ${p2Time}s`);
          setPlayer1Time(p1Time);
          setPlayer2Time(p2Time);
        },
        GameIsReady: async () => {
          if (!isMounted) return;
          console.log("Game is ready!");
          setIsGameReady(true);
          await refreshGameState();
        },
        // Przykład obsługi czasu przeciwnika: TimeOver / OpponentTimeOver
        TimeOver: (color: string) => {
          if (!isMounted) return;
          alert(`Your time is over (${color})!`);
          setGameEnded(true);
        },
        OpponentTimeOver: (color: string) => {
          if (!isMounted) return;
          alert(`Opponent's time is over (${color})! You win.`);
          setGameEnded(true);
        }
      };

      try {
        // Odbierz to samo (globalne) połączenie:
        const hub = await getConnection(gameHandlers);
        
        // Teraz dołącz do gry (lub przypisz swój clientId) - w Twoim kodzie:
        await hub.invoke("AssignClientIdToGame", gameId);

        // Pobierz kolor
        const color = await hub.invoke("GetPlayerColor", gameId);
        console.log("Assigned color:", color);
        setPlayerColor(color);
        setBoardOrientation(color === "white" ? "white" : "black");

        // (Opcjonalnie) sprawdź, czy gra nie jest już gotowa:
        // albo poczekaj aż serwer wyśle GameIsReady.  
        // Na wszelki wypadek możesz wymusić refresh stanu:
        await refreshGameState();
        
      } catch (error) {
        console.error("Error connecting or invoking hub methods:", error);
      }
    };

    initGameHandlers();

    return () => {
      isMounted = false;
      // Uwaga: nie robimy hub.stop() – chyba że WYRAŹNIE chcemy zakończyć połączenie 
      // i opuścić grę. Zostawiamy je aktywne, dopóki user nie opuści całkiem aplikacji
      // (albo sam z niej nie wyjdzie).
    };
  }, [gameId]);

  // Sprawdzenie stanu gry – np. czy mat/pat
  const checkGameState = async () => {
    try {
      const response = await fetchGameState(gameId);
      const isGameEnded = response.data;
      if (isGameEnded) {
        setGameEnded(true);
        setGameResult("Game Over!");
      }
      return isGameEnded;
    } catch (error) {
      console.error("Error checking game state:", error);
      return false;
    }
  };

  // Wykonanie ruchu
  const makeMove = async (sourceSquare: string, targetSquare: string, promotedPiece?: string) => {
    try {
      const move = promotedPiece
        ? `${sourceSquare}${targetSquare}${promotedPiece}`
        : `${sourceSquare}${targetSquare}`;
      await sendMove(gameId, move);
      
      console.log("Move sent to server");
      
      // Zgłaszamy na hubie, że wykonaliśmy ruch – w Twoim kodzie:
      const hub = await getConnection(); 
      await hub.invoke("YourMove", gameId);

      await refreshGameState();
      await checkGameState();
    } catch (error) {
      console.error("Error making move:", error);
    }
  };

  // Obsługa drag&drop na ChessboardComponent
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

  // Dodatkowe: klikanie pola -> highlight możliwych ruchów
  const [customSquareStyles, setCustomSquareStyles] = useState({});
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

  return (
    <div>
      <h2>
        {isGameReady
          ? `You are playing as ${playerColor}`
          : "Waiting for an opponent..."}
      </h2>
      <div style={{ display: "flex", justifyContent: "space-between", margin: "10px 0" }}>
        <div>
          <strong>Player 1 (White):</strong> {player1Time}s
        </div>
        <div>
          <strong>Player 2 (Black):</strong> {player2Time}s
        </div>
      </div>
      <ChessboardComponent
        position={position}
        orientation={boardOrientation}
        onSquareClick={onSquareClick}
        customSquareStyles={customSquareStyles}
        onPieceDrop={onDrop}
        onPromotionPieceSelect={(piece, from, to) => makeMove(from, to, piece)}
      />
      {gameEnded && <h3>{gameResult}</h3>}
    </div>
  );
};

export default ChessboardOnline;
