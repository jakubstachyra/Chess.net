"use client";
import React, { useEffect, useState } from "react";
import ChessboardComponent from "../../components/chessBoard/chessBoard";
import { useParams } from "next/navigation";
import MoveHistory from "../../components/MoveHistory/moveHistory";
import MoveNavigation from "../../components/MoveNavigation/moveNavigation";

import {
  fetchFen,
  fetchMoves,
  fetchWhoToMove,
  sendMove,
  fetchComputerMove,
  fetchGameState,
} from "../../services/gameService";
import BackgroundUI from "app/components/backgroundUI/pages";

const ChessboardComponentComputer = () => {
  const [customSquareStyles, setCustomSquareStyles] = useState({});
  const [mappedMoves, setMappedMoves] = useState({});
  const [position, setPosition] = useState("start");
  const [whoToMove, setWhoToMove] = useState(0);
  const [moveHistory, setMoveHistory] = useState([]);
  const [isPositionLoaded, setIsPositionLoaded] = useState(false);
  const [gameEnded, setGameEnded] = useState(false);
  const [gameResult, setGameResult] = useState("");
  const [navigationMode, setNavigationMode] = useState(false);

  // Jeśli color = 0 -> białe, 1 -> czarne
  // Tutaj zakładamy, że user (gracz) gra białymi, a komputer czarnymi
  const color = 0; 

  const { gameId } = useParams();

  useEffect(() => {
    loadInitialData();
  }, [gameId]);

  const loadInitialData = async () => {
    try {
      // Pobierz początkowy FEN z serwera i ustaw go na szachownicy
      const fenResponse = await fetchFen(gameId);
      setPosition(fenResponse.data);
      setIsPositionLoaded(true);

      // Załaduj możliwe ruchy (mapa start->lista celów)
      await loadMoves();

      // Sprawdź, czy gra nie skończyła się jeszcze przed naszym ruchem
      await checkGameState();
    } catch (error) {
      console.error("Error loading initial data:", error);
    }
  };

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

  // Kliknięcie na pole (podświetlenie możliwych ruchów itp.)
  const onSquareClick = (square) => {
    const moves = mappedMoves[square] || [];
    const newStyles = moves.reduce((styles, target) => {
      styles[target] = {
        backgroundColor: "rgba(0, 255, 0, 0.5)",
        borderRadius: "50%",
      };
      return styles;
    }, {});
    setCustomSquareStyles(newStyles);
  };

  // Gdy upuścisz figurę z sourceSquare na targetSquare
  const onDrop = async (sourceSquare, targetSquare) => {
    const possibleMovesFromSource = mappedMoves[sourceSquare];
    if (possibleMovesFromSource?.includes(targetSquare)) {
      // Ruch jest legalny
      await makeMove(sourceSquare, targetSquare);
      return true;
    }
    return false;
  };

  /**
   * Wysyła ruch do backendu.
   * Gdy isComputerMove=false -> na końcu wywołuje refreshGameState,
   * żeby ewentualnie pobrać ruch komputera i zaktualizować szachownicę.
   */
  const makeMove = async (sourceSquare, targetSquare, promotedPiece = null, isComputerMove = false) => {
    try {
      let move;
      if (promotedPiece) {
        move = `${sourceSquare}${targetSquare}${promotedPiece}`;
      } else {
        move = `${sourceSquare}${targetSquare}`;
      }
      setCustomSquareStyles({});
      await sendMove(gameId, move);

      // Dodaj do historii (po stronie frontu)
      setMoveHistory((prev) => [
        ...prev,
        {
          move: `${sourceSquare}-${targetSquare}`,
          fen: null,
        },
      ]);

      // Jeżeli ruch wykonuje gracz, to sprawdź czy teraz nie pora na czarnych (komputer)
      if (!isComputerMove) {
        await refreshGameState();
      }
    } catch (error) {
      console.error("Error making move:", error);
    }
  };

  /**
   * Odświeża stan:
   * 1) Pobiera nowy FEN i whoToMove,
   * 2) Jeśli to ruch komputera (whoToMove == 1), pyta silnik o ruch,
   * 3) Wykonuje ruch komputera (już z isComputerMove=true, żeby uniknąć pętli),
   * 4) Na koniec loadMoves().
   */
  const refreshGameState = async () => {
    try {
      const fenResponse = await fetchFen(gameId);
      const newFen = fenResponse.data;

      const whoToMoveResponse = await fetchWhoToMove(gameId);
      const newWhoToMove = whoToMoveResponse.data;

      setPosition(newFen);
      setWhoToMove(newWhoToMove);

      // Zaktualizuj Fen w ostatnim ruchu
      setMoveHistory((prev) => {
        if (prev.length === 0) return prev;
        const updatedHistory = [...prev];
        updatedHistory[updatedHistory.length - 1].fen = newFen;
        return updatedHistory;
      });

      // Sprawdź czy gra się skończyła
      const ended = await checkGameState();
      if (ended) return;

      // Jeżeli nowWhoToMove != color => ruch komputera
      if (newWhoToMove !== color) {
        // Pobieramy ruch Stockfisha (np. "d7d5")
        const computerMove = await fetchComputerMove(gameId);
        // Rozdziel: source= "d7", target="d5"
        const moveStr = computerMove.data;
        const source = moveStr.substring(0, 2);
        const target = moveStr.substring(2, 4);

        // Wykonaj ruch komputera (z flagą isComputerMove=true)
        await makeMove(source, target, null, true);

        // Po ruchu komputera warto ponownie pobrać nowy FEN, whoToMove i moves
        // (tym razem ruch powinien wrócić do gracza)
        const fenResponse2 = await fetchFen(gameId);
        setPosition(fenResponse2.data);

        const whoToMoveResponse2 = await fetchWhoToMove(gameId);
        setWhoToMove(whoToMoveResponse2.data);

        await loadMoves(); 
      } else {
        // Ruch gracza, załaduj legalne ruchy
        await loadMoves();
      }
    } catch (error) {
      console.error("Error refreshing game state:", error);
    }
  };

  const loadMoves = async () => {
    try {
      const movesResponse = await fetchMoves(gameId);
      const movesMapping = {};

      // Każdy ruch w formacie "e2 e4"
      movesResponse.data.forEach((move) => {
        const [source, target] = move.split(" ");
        if (!movesMapping[source]) movesMapping[source] = [];
        movesMapping[source].push(target);
      });
      setMappedMoves(movesMapping);
    } catch (error) {
      console.error("Error loading moves:", error);
    }
  };

  if (!isPositionLoaded) {
    return <div>Loading...</div>;
  }

  if (gameEnded) {
    return <div>Game Over: {gameResult}</div>;
  }

  return (
    <div>
      <h1 style={{ color: "white" }}>Computer</h1>
      <div style={containerStyles}>
        <div style={chessboardContainerStyles}>
          <ChessboardComponent
            position={position}
            onSquareClick={onSquareClick}
            customSquareStyles={customSquareStyles}
            onPieceDrop={onDrop}
            boardOrientation={"white"} // Możesz zmienić na "black" jeśli chcesz odwrotną perspektywę
            isDraggablePiece={() => !navigationMode}
            onPromotionPieceSelect={(piece, from, to) =>
              makeMove(from, to, piece)
            }
          />
        </div>
        <div style={modalContainerStyles}>
          <BackgroundUI>
            <h1>Moves</h1>
            <MoveHistory moveHistory={moveHistory} />
            <MoveNavigation
              moveHistory={moveHistory}
              setPosition={setPosition}
              setNavigationMode={setNavigationMode}
            />
            <div style={buttonsContainerStyles}>
              <button style={buttonStyle} title="Give up a game">
                Resign
              </button>
            </div>
          </BackgroundUI>
        </div>
      </div>
    </div>
  );
};

export default ChessboardComponentComputer;

// Style

const buttonsContainerStyles = {
  display: "flex",
  justifyContent: "space-between",
  gap: "10px",
  marginTop: "auto",
  width: "100%",
};

const containerStyles = {
  display: "flex",
  justifyContent: "flex-end",
  alignItems: "flex-start",
  padding: "20px",
  gap: "30px",
};

const chessboardContainerStyles = {
  display: "flex",
  alignItems: "center",
  justifyContent: "center",
};

const modalContainerStyles = {
  display: "flex",
  alignItems: "center",
  flexDirection: "column",
  justifyContent: "space-between",
  height: "600px",
  width: "400px",
  borderRadius: "15px",
  backgroundColor: "rgba(255, 255, 255, 0.1)",
  boxShadow: "0 4px 15px rgba(0, 0, 0, 0.3)",
  backdropFilter: "blur(10px)",
  color: "white",
};

const buttonStyle = {
  padding: "10px",
  fontSize: "16px",
  fontWeight: "bold",
  color: "#fff",
  backgroundColor: "#DD0000",
  border: "none",
  borderRadius: "5px",
  cursor: "pointer",
  boxShadow: "0 4px 30px rgba(0, 0, 0, 0.5)",
  width: "100%",
};
