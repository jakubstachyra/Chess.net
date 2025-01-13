"use client";

import React, { useEffect, useState, useRef } from "react";
import ChessboardComponent from "../../components/chessBoard/chessBoard";
import { useParams } from "next/navigation";
import { connectToHub } from "../../services/signalrClient";
import {
  fetchFen,
  fetchMoves,
  fetchWhoToMove,
  sendMove,
  fetchGameState,
} from "../../services/gameService";

const ChessboardOnline = () => {
  const [position, setPosition] = useState("start");
  const [customSquareStyles, setCustomSquareStyles] = useState({});
  const [mappedMoves, setMappedMoves] = useState({});
  const [whoToMove, setWhoToMove] = useState(null); // 0: White, 1: Black
  const [playerColor, setPlayerColor] = useState(null); // "white" or "black"
  const [isGameReady, setIsGameReady] = useState(false);
  const [boardOrientation, setBoardOrientation] = useState("white");
  const { gameId } = useParams();
  const connectionRef = useRef(null); // Track connection instance
  const isInitialized = useRef(false); // Prevent duplicate initialization
  const [player1Time, setPlayer1Time] = useState(0); // Timer for Player 1
  const [player2Time, setPlayer2Time] = useState(0); // Timer for Player 2
  const [gameEnded, setGameEnded] = useState(false);
  const [gameResult, setGameResult] = useState("");

  // Refresh game state and fetch moves
  const refreshGameState = async () => {
    try {
      const fenResponse = await fetchFen(gameId);
      setPosition(fenResponse.data);

      const whoToMoveResponse = await fetchWhoToMove(gameId);
      setWhoToMove(whoToMoveResponse.data);

      const movesResponse = await fetchMoves(gameId);
      const movesMapping = mapMoves(movesResponse.data);
      setMappedMoves(movesMapping);
    } catch (error) {
      console.error("Error refreshing game state:", error);
    }
  };

  // Map moves for highlighting
  const mapMoves = (moves) => {
    const movesMapping = {};
    moves.forEach((move) => {
      const [source, target] = move.split(" ");
      if (!movesMapping[source]) movesMapping[source] = [];
      movesMapping[source].push(target);
    });
    return movesMapping;
  };

  useEffect(() => {
    if (!gameId) {
      console.error("Game ID is required to join a game.");
      return;
    }

    if (isInitialized.current) {
      console.log("Component already initialized.");
      return;
    }
    isInitialized.current = true;

    let isMounted = true; // Track component mount status

    const initHubConnection = async () => {
      try {
        if (connectionRef.current) {
          console.log("Connection already exists.");
          return; // Prevent duplicate connections
        }

        const handlers = {
          PlayerDisconnected: () => {
            alert("Opponent disconnected. The game is over.");
            setIsGameReady(false);
          },
          OpponentMoved: async () => {
            console.log("Opponent moved");
            await refreshGameState();
          },
          UpdateTimers: (p1Time, p2Time) => {
            console.log(
              `Game ${gameId} - Player 1: ${p1Time}s, Player 2: ${p2Time}s`
            );
            setPlayer1Time(p1Time);
            setPlayer2Time(p2Time);
          },
          GameIsReady: async () => {
            console.log("Game is ready!");
            setIsGameReady(true);
            await refreshGameState();
          },
        };

        const hub = await connectToHub(
          "https://localhost:7078/gamehub",
          handlers
        );
        connectionRef.current = hub; // Store connection instance
        console.log("Connection established with ID:", hub.connectionId);

        if (hub) {
          // Assign the client ID and get the player color
          await hub.invoke("AssignClientIdToGame", gameId);
          const color = await hub.invoke("GetPlayerColor", gameId);
          console.log("color");
          console.log(color);
          setPlayerColor(color);
          setBoardOrientation(color === "white" ? "white" : "black");
        }
      } catch (error) {
        console.error("Error connecting to hub:", error);
      }
    };

    initHubConnection();

    return () => {
      isMounted = false; // Cleanup
      if (connectionRef.current) {
        connectionRef.current.stop().then(() => {
          console.log("Connection stopped.");
          connectionRef.current = null;
        });
      }
    };
  }, [gameId]);

  // Make a move and send it to the server
  const makeMove = async (sourceSquare, targetSquare, promotedPiece = null) => {
    try {
      let move;
      if (promotedPiece) {
        move = `${sourceSquare}${targetSquare}${promotedPiece}`;
      } else {
        move = `${sourceSquare}${targetSquare}`;
      }
      setCustomSquareStyles({});

      await sendMove(gameId, move);
      const isGameEnded = await checkGameState();

      console.log("Move sent to server");

      if (connectionRef.current) {
        await connectionRef.current.invoke("YourMove", gameId);
      }

      await refreshGameState();
    } catch (error) {
      console.error("Error making move:", error);
    }
  };

  // Highlight possible moves
  const onSquareClick = (square) => {
    const moves = mappedMoves[square] || [];
    const styles = moves.reduce((acc, target) => {
      acc[target] = {
        backgroundColor: "rgba(0, 255, 0, 0.5)",
        borderRadius: "50%",
      };
      return acc;
    }, {});
    setCustomSquareStyles(styles);
  };

  const checkGameState = async () => {
    try {
      const response = await fetchGameState(gameId);
      const isGameEnded = response.data;
      console.log(response);
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

  // Invoke the GameEnded method in SignalR Hub when the game ends
  useEffect(() => {
    if (gameEnded && connectionRef.current) {
      connectionRef.current
        .invoke(
          "GameEnded",
          parseInt(gameId),
          connectionRef.current.connectionId
        )
        .then(() => {
          console.log("GameEnded method invoked on Hub.");
        })
        .catch((error) => {
          console.error("Error invoking GameEnded method:", error);
        });
    }
  }, [gameEnded, gameResult]);

  // Handle piece drop
  const onDrop = async (sourceSquare, targetSquare) => {
    const possibleMoves = mappedMoves[sourceSquare];

    if (possibleMoves?.includes(targetSquare)) {
      await makeMove(sourceSquare, targetSquare);
      return true;
    } else {
      alert("Invalid move!");
    }

    return false;
  };

  return (
    <div>
      <h2>
        {isGameReady
          ? `You are playing as ${playerColor}`
          : "Waiting for an opponent..."}
      </h2>
      <div
        style={{
          display: "flex",
          justifyContent: "space-between",
          margin: "10px 0",
        }}
      >
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
    </div>
  );
};

export default ChessboardOnline;
