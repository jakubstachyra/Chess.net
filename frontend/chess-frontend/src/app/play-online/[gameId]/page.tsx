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

        const hub = await connectToHub(
          "https://localhost:7078/gamehub",
          handlers
        );
        setConnection(hub);
      };

      initHub();
      refreshGameState();
    }, []);

    const refreshGameState = async () => {
      try {
        const fenResponse = await fetchFen(gameId);
        setPosition(fenResponse.data);

        const whoToMoveResponse = await fetchWhoToMove(gameId);
        setWhoToMove(whoToMoveResponse.data);

        const movesResponse = await fetchMoves(gameId);
        const movesMapping = {};
        movesResponse.data.forEach((move) => {
          const [source, target] = move.split(" ");
          if (!movesMapping[source]) movesMapping[source] = [];
          movesMapping[source].push(target);
        });
        setMappedMoves(movesMapping);
      } catch (error) {
        console.error("Error refreshing game state:", error);
      }
    };

    const makeMove = async (sourceSquare, targetSquare) => {
      const move = `${sourceSquare} ${targetSquare}`;
      setCustomSquareStyles({});
      await sendMove(gameId, { move }); 
      await refreshGameState();
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
      const possibleMoves = mappedMoves[sourceSquare];
      if (possibleMoves?.includes(targetSquare)) {
        await makeMove(sourceSquare, targetSquare);
        return true;
      }
      return false;
    };

    if (!isGameReady) return <div>Waiting for opponent...</div>;

    return (
      <div>
        <h2>You are playing as {playerColor}</h2>
      <div style={containerStyles}>
        <div style={chessboardContainerStyles}>
          <ChessboardComponent
            position={position}
            onSquareClick={onSquareClick}
            customSquareStyles={customSquareStyles}
            onPieceDrop={onDrop}
            boardOrientation={boardOrientation}
            isDraggablePiece={() => true}
          />
        </div>
        <div style={modalContainerStyles}>
        <BackgroundUI>
          <h1>Moves</h1>
          <h5>Here will be history in the future</h5>
          <div style={buttonsContainerStyles}>
            <button
              style={{ ...buttonStyle, backgroundColor: "#FF7700" }}
              title="Report opponent if you think he is cheating"
            >
              Report
            </button>
            <button
              style={{ ...buttonStyle, backgroundColor: "#4C9AFF" }}
              title="Propose draw to your opponent"
            >
              Draw
            </button>
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

  export default ChessboardOnline;

  const buttonsContainerStyles = {
    display: "flex",
    justifyContent: "space-between",
    gap: "10px",
    marginTop: "auto",
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