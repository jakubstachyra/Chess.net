// pages/AdminPage.tsx (lub odpowiednia ścieżka)
"use client";

import React, { useState, useEffect } from "react";
import { useRouter } from "next/navigation";
import BackgroundUI from "app/components/backgroundUI/pages";
import { fetchGameHistoryByID } from "app/services/historyService";
import { fetchReport, banUserWithReport, rejectReport } from "app/services/adminService";
import { GameReviewContent } from "../../../components/gameReview/gameReview";
import CustomDialog from "app/components/customDialog/customDialog";

interface MoveHistoryEntry {
  moveNumber: number;
  fen: string;
  move: string;
  whiteRemainingTimeMs: number | null;
  blackRemainingTimeMs: number | null;
}

const buttonStyle: React.CSSProperties = {
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

const buttonsContainerStyles: React.CSSProperties = {
  display: "flex",
  justifyContent: "space-between",
  gap: "10px",
  marginTop: "auto",
  width: "100%",
};

const AdminPage = () => {
  const router = useRouter();
  const [report, setReport] = useState<any>(null);
  const [gameDetails, setGameDetails] = useState<any>(null);
  const [moveHistory, setMoveHistory] = useState<MoveHistoryEntry[]>([]);
  const [position, setPosition] = useState("start");
  const [currentMoveIndex, setCurrentMoveIndex] = useState(0);
  const [navigationMode, setNavigationMode] = useState(true);
  const [loading, setLoading] = useState(true);
  const [disableAnimation, setDisableAnimation] = useState(false);

  // Stany do zarządzania dialogiem
  const [dialogOpen, setDialogOpen] = useState(false);
  const [dialogTitle, setDialogTitle] = useState("");
  const [dialogContent, setDialogContent] = useState("");
  
  useEffect(() => {
    const loadReportAndGame = async () => {
      try {
        const fetchedReport = await fetchReport();
        if (!fetchedReport) throw new Error("Report not found");
        setReport(fetchedReport);

        const data = await fetchGameHistoryByID(fetchedReport.id);
        if (!data) throw new Error("Game history not found");
        setGameDetails(data);

        const transformedMovesHistory: MoveHistoryEntry[] = [];

        if (data.startFen) {
          transformedMovesHistory.push({
            moveNumber: 0,
            fen: data.startFen,
            move: "Start Position",
            whiteRemainingTimeMs: null,
            blackRemainingTimeMs: null,
          });
        } else {
          transformedMovesHistory.push({
            moveNumber: 0,
            fen: "start",
            move: "Start Position",
            whiteRemainingTimeMs: null,
            blackRemainingTimeMs: null,
          });
        }

        if (data.movesHistory) {
          data.movesHistory.forEach((move: any) => {
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
        }

        setMoveHistory(transformedMovesHistory);
        setPosition(transformedMovesHistory[0].fen);
        setCurrentMoveIndex(0);
      } catch (error: any) {
        console.error("Failed to load report or game details:", error.message);
      } finally {
        setLoading(false);
      }
    };

    loadReportAndGame();
  }, []);

  const handleSetPosition = (fen: string, disableAnim = false) => {
    if (disableAnim) {
      setDisableAnimation(true);
      setTimeout(() => setDisableAnimation(false), 100);
    }
    setPosition(fen);
  };

  const handleMoveIndexChange = (index: number) => {
    setCurrentMoveIndex(index);
    handleSetPosition(moveHistory[index].fen, true);
    if (index === moveHistory.length - 1) {
      setNavigationMode(false);
    } else {
      setNavigationMode(true);
    }
  };

  const closeDialog = () => {
    setDialogOpen(false);
    // Po zamknięciu dialogu możemy przekierować użytkownika, jeśli potrzeba.
    router.push("/admin");
  };

  const handleBanUser = async () => {
    if (!report) {
      console.error("No report available.");
      return;
    }
    try {
      const response = await banUserWithReport(report.suspectID, report.id);
      console.log(response);
      if (response.status === 200) {
        setDialogTitle("Success");
        setDialogContent("User has been banned successfully!");
        setDialogOpen(true);
      }
    } catch (error) {
      console.error("Failed to ban the user:", error);
    }
  };

  const handleRejectReport = async () => {
    if (!report) {
      console.error("No report available.");
      return;
    }
    try {
      const response = await rejectReport(report.id);
      console.log(response);
      if (response.status === 200) {
        setDialogTitle("Success");
        setDialogContent("Report has been rejected successfully!");
        setDialogOpen(true);
      }
    } catch (error) {
      console.error("Failed to reject the report:", error);
    }
  };

  if (loading) {
    return (
      <BackgroundUI>
        <div style={{ color: "white", fontSize: "18px", textAlign: "center" }}>
          Loading game details...
        </div>
      </BackgroundUI>
    );
  }

  if (!gameDetails) {
    return (
      <BackgroundUI>
        <div style={{ color: "white", fontSize: "18px", textAlign: "center" }}>
          Game details not found.
        </div>
      </BackgroundUI>
    );
  }

  return (
    <>
      <GameReviewContent
        moveHistory={moveHistory}
        currentMoveIndex={currentMoveIndex}
        position={position}
        disableAnimation={disableAnimation}
        onSelectMoveIndex={handleMoveIndexChange}
        onMoveIndexChange={handleMoveIndexChange}
      >
        <div style={buttonsContainerStyles}>
          <button style={buttonStyle} onClick={handleBanUser} title="Ban suspect">
            Ban suspect
          </button>
          <button
            style={{ ...buttonStyle, backgroundColor: "#673AB7" }}
            onClick={handleRejectReport}
            title="Reject report when user played fair"
          >
            Reject report
          </button>
        </div>
      </GameReviewContent>
      <CustomDialog
        open={dialogOpen}
        onClose={closeDialog}
        title={dialogTitle}
        content={<div style={{ color: "white" }}>{dialogContent}</div>}
        actions={
          <button style={{...buttonStyle, backgroundColor: "blue"} } onClick={closeDialog}>
            OK
          </button>
        }
      />
    </>
  );
};

export default AdminPage;
