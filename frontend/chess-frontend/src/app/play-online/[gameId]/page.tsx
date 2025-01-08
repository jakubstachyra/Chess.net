  "use client";

  import React, { useEffect, useState } from "react";
  import ChessboardComponent from "../../components/chessBoard/chessBoard";
  import { Square } from "react-chessboard/dist/chessboard/types";
  import { useParams } from "next/navigation";
  import BackgroundUI  from "app/components/backgroundUI/pages";
  
  import { connectToHub } from "../../services/signalrClient";
  import {
    fetchFen,
    fetchMoves,
    fetchWhoToMove,
    sendMove,
  } from "../../services/gameService";
import { orange } from "@mui/material/colors";

  const ChessboardOnline = () => {
    const [position, setPosition] = useState("start");
    const [customSquareStyles, setCustomSquareStyles] = useState({});
    const [mappedMoves, setMappedMoves] = useState({});
    const [whoToMove, setWhoToMove] = useState(0);
    const [playerColor, setPlayerColor] = useState(null);
    const [isGameReady, setIsGameReady] = useState(false);
    const [connection, setConnection] = useState(null);
    const [boardOrientation, setBoardOrientation] = useState("white");  
    const { gameId } = useParams();
    
    useEffect(() => {
      const initHub = async () => {
        const handlers = {
          AssignPlayerColor: (color) =>{ 
            setPlayerColor(color),
            setBoardOrientation(color === "white" ? "white" : "black");
          },
          GameReady: () => setIsGameReady(true),
          PlayerDisconnected: () => {
            alert("Opponent disconnected");
            setIsGameReady(false);
          },
          OpponentMoved: async () => {
            await refreshGameState();
          },
        };

  useEffect(() => {
    if (!gameId) {
      console.error("Game ID is required to join a game.");
      return;
    }

    const initHub = async () => {
      try {
        const handlers = {
          GameState: (color) => {
            setPlayerColor(color);
            setIsGameReady(true);
          },
          PlayerDisconnected: () => {
            alert("Opponent disconnected. The game is over.");
            setIsGameReady(false);
          },
          OpponentMoved: async () => {
            await refreshGameState();
          },
        };

        const hub = await connectToHub(
          "https://localhost:7078/gamehub",
          handlers
        );
        setConnection(hub);

        if (hub) {
          // Fetch initial game state
          await hub.invoke("GetGameState");
        }
      } catch (error) {
        console.error("Error connecting to hub:", error);
      }
    };

    initHub();
    refreshGameState();
  }, [gameId]);

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

    const onDrop = async (sourceSquare, targetSquare) => {
      const possibleMoves = mappedMoves[sourceSquare];
      if (possibleMoves?.includes(targetSquare)) {
        await makeMove(sourceSquare, targetSquare);
        return true;
      }
      return false;
    };

      const movesResponse = await fetchMoves(gameId);
      const movesMapping = mapMoves(movesResponse.data);
      setMappedMoves(movesMapping);
    } catch (error) {
      console.error("Error refreshing game state:", error);
    }
  };

  const mapMoves = (moves) => {
    const movesMapping = {};
    moves.forEach((move) => {
      const [source, target] = move.split(" ");
      if (!movesMapping[source]) movesMapping[source] = [];
      movesMapping[source].push(target);
    });
    return movesMapping;
  };

  const makeMove = async (sourceSquare, targetSquare) => {
    try {
      const move = `${sourceSquare} ${targetSquare}`;
      setCustomSquareStyles({});

      // Send move to the server
      await sendMove(gameId, move);

      // Notify the server of the move
      if (connection) {
        await connection.invoke("YourMove");
      }

      // Refresh the game state
      await refreshGameState();
    } catch (error) {
      console.error("Error making move:", error);
    }
  };

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

  const onDrop = async (sourceSquare, targetSquare) => {
    // Check if it's this player's turn
    if (
      (whoToMove === 0 && playerColor === "white") ||
      (whoToMove === 1 && playerColor === "black")
    ) {
      const possibleMoves = mappedMoves[sourceSquare];
      if (possibleMoves?.includes(targetSquare)) {
        await makeMove(sourceSquare, targetSquare);
        return true;
      }
    } else {
      alert("It's not your turn!");
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
      <ChessboardComponent
        position={position}
        onSquareClick={onSquareClick}
        customSquareStyles={customSquareStyles}
        onPieceDrop={onDrop}
      />
    </div>
  );
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
    color: "white"
  };

  const buttonStyle = {
    padding: "10px 30px",
    fontSize: "16px",
    fontWeight: "bold",
    color: "#fff",
    backgroundColor: "#DD0000 ",
    border: "none",
    borderRadius: "5px",
    cursor: "pointer",
    boxShadow: "0 4px 30px rgba(0, 0, 0, 0.5)",
};