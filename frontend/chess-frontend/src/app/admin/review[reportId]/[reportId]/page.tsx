"use client";

import React, { useState, useRef, useEffect } from "react";
import BackgroundUI from "app/components/backgroundUI/pages";
import { useSelector } from "react-redux";
import { useRouter } from "next/navigation";
import { fetchGameHistoryByID } from "../../../services/historyService";
import { fetchReport, banUserWithReport } from "../../../services/adminService";
import ChessboardComponent from "app/components/chessBoard/chessBoard";
import MoveHistory from "app/components/MoveHistory/moveHistory";
import MoveNavigation from "app/components/MoveNavigation/moveNavigation";

interface MoveHistoryEntry {
    moveNumber: number;
    fen: string;
    move: string;
    whiteRemainingTimeMs: number | null;
    blackRemainingTimeMs: number | null;
}


export default function AdminPage() {
    const router = useRouter();

    const rightSectionRef = useRef(null);
    const [boardWidth, setBoardWidth] = useState(400);
    const [report, setReport] = useState(null);
    const [moveHistory, setMoveHistory] = useState([]);
    const [position, setPosition] = useState("start");
    const [navigationMode, setNavigationMode] = useState(false);

    const getReport = async () => {
        try {
            const report = await fetchReport();
            if (!report) throw new Error("Report not found");
            setReport(report);

            const reportData = await fetchGameHistoryByID(report.id);
            if (!reportData || !reportData.movesHistory) throw new Error("Game history not found");

            // Transformacja movesHistory
            const transformedMovesHistory: MoveHistoryEntry[] = [];
            reportData.movesHistory.forEach((move) => {
                if (move.whiteFen) {
                    transformedMovesHistory.push({
                        moveNumber: move.moveNumber,
                        fen: move.whiteFen,
                        move: move.whiteMove,
                        whiteRemainingTimeMs: move.whiteRemainingTimeMs,
                        blackRemainingTimeMs: null,
                    });
                }
                if (move.blackFen) {
                    transformedMovesHistory.push({
                        moveNumber: move.moveNumber,
                        fen: move.blackFen,
                        move: move.blackMove,
                        whiteRemainingTimeMs: null,
                        blackRemainingTimeMs: move.blackRemainingTimeMs,
                    });
                }
            });

            setReport(report);
            
            setMoveHistory(transformedMovesHistory);

            // Ustaw początkową pozycję
            // if (transformedMovesHistory.length > 0) {
            //     setPosition(transformedMovesHistory[0].fen[0]);
            // }
        } catch (error) {
            console.error("Failed to fetch the report:", error.message);
        }
    };

    useEffect(() => {
        getReport();
    }, []);

    useEffect(() => {
        if (rightSectionRef.current) {
            setBoardWidth(rightSectionRef.current.clientWidth * 0.8);
        }

        const handleResize = () => {
            if (rightSectionRef.current) {
                setBoardWidth(rightSectionRef.current.clientWidth * 0.8);
            }
        };

        window.addEventListener("resize", handleResize);
        return () => window.removeEventListener("resize", handleResize);
    }, []);

  const handleBanUser = async () => {
      if (!report) {
          console.error("No report available.");
          return;
      }

      try {
          const response = await banUserWithReport(report.suspectID, report.id); 
          console.log(response);
          if (response.status === 200) {
              alert("User has been banned successfully!");
              router.push("/admin");
          }
      } catch (error) {
          console.error("Failed to ban the user:", error);
      }
  };

    return (
        <div style={{ display: "flex", flexDirection: "column", alignItems: "center", justifyContent: "center", height: "100vh" }}>
          {/* Nagłówek Game Review */}
          <div style={{ width: "50%", display: "flex", justifyContent: "center" }}>
            <BackgroundUI>
              <h1 style={{ display: "flex", justifyContent: "center", color: "white" }}>Game Review</h1>
            </BackgroundUI>
          </div>
      
          {/* Główna sekcja */}
          <div style={containerStyles}>
            <div style={chessboardContainerStyles}>
              <div>
                <ChessboardComponent
                  onSquareClick={() => {}}
                  position={position}
                  boardOrientation={"white"} 
                  isDraggablePiece={() => false}
                />
              </div>
            </div>
            <div style={modalContainerStyles}>
              <BackgroundUI>
                <h1>Moves</h1>
                <MoveHistory moveHistory={moveHistory} />
                <MoveNavigation moveHistory={moveHistory} setPosition={setPosition} setNavigationMode={setNavigationMode} />
                <div style={buttonsContainerStyles}>
                  <button style={buttonStyle} onClick={handleBanUser} title="Ban suspect">
                    Ban suspect
                  </button>
                  <button style={{ ...buttonStyle, backgroundColor: "#673AB7" }} title="Reject report when user played fair">
                    Reject report
                  </button>
                </div>
              </BackgroundUI>
            </div>
          </div>
        </div>
      );
      
}
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
const backgroundContainerStyles = {
    display: "flex",
    justifyContent: "center",
    alignItems: "center",
    width: "50%",
    height: "70%",
    color: "white",
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
    flexDirection: "column",
    alignItems: "center",
    justifyContent: "center",
    gap: "10px",
};

const buttonStyle = {
    padding: "10px",
    fontSize: "16px",
    fontWeight: "bold",
    color: "#fff",
    backgroundColor: "#DD0000 ",
    border: "none",
    borderRadius: "5px",
    cursor: "pointer",
    boxShadow: "0 4px 30px rgba(0, 0, 0, 0.5)",
    width: "100%"
  };

  const buttonsContainerStyles = {
    display: "flex",
    justifyContent: "space-between",
    gap: "10px",
    marginTop: "auto",
    width: "100%",
  };    