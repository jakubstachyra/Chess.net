// src/app/admin/review/[reportId]/page.tsx

"use client";

import React, { useState, useEffect } from "react";
import { useRouter } from "next/navigation";
import BackgroundUI from "app/components/backgroundUI/pages";
import { fetchGameHistoryByID } from "app/services/historyService";
import { fetchReport, banUserWithReport, rejectReport } from "app/services/adminService";
import { GameReviewContent } from "../../../components/gameReview/gameReview";
import CustomDialog from "app/components/customDialog/customDialog";
import { Button } from "@mui/material";
import { Report, GameHistory, MoveHistoryEntry } from "../../../types"; // Importuj zdefiniowane typy

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

const AdminReviewPage: React.FC = () => {
  const router = useRouter();
  const [report, setReport] = useState<Report | null>(null);
  const [gameDetails, setGameDetails] = useState<GameHistory | null>(null);
  const [moveHistory, setMoveHistory] = useState<MoveHistoryEntry[]>([]);
  const [position, setPosition] = useState<string>("start");
  const [currentMoveIndex, setCurrentMoveIndex] = useState<number>(0);
  const [loading, setLoading] = useState<boolean>(true);
  const [disableAnimation, setDisableAnimation] = useState<boolean>(false);

  // Stany do zarządzania dialogiem
  const [dialogOpen, setDialogOpen] = useState<boolean>(false);
  const [dialogTitle, setDialogTitle] = useState<string>("");
  const [dialogContent, setDialogContent] = useState<string>("");

  useEffect(() => {
    const loadReportAndGame = async () => {
      try {
        const fetchedReport: Report = await fetchReport();
        if (!fetchedReport) throw new Error("Report not found");
        setReport(fetchedReport);

        const data: GameHistory = await fetchGameHistoryByID(fetchedReport.gameID);
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
          data.movesHistory.forEach((move) => {
            if (move.whiteFen && move.whiteMove) {
              transformedMovesHistory.push({
                moveNumber: move.moveNumber,
                fen: move.whiteFen,
                move: move.whiteMove,
                whiteRemainingTimeMs: move.whiteRemainingTimeMs ?? null,
                blackRemainingTimeMs: null,
              });
            }
            if (move.blackFen && move.blackMove) {
              transformedMovesHistory.push({
                moveNumber: move.moveNumber,
                fen: move.blackFen,
                move: move.blackMove,
                whiteRemainingTimeMs: null,
                blackRemainingTimeMs: move.blackRemainingTimeMs ?? null,
              });
            }
          });
        }

        setMoveHistory(transformedMovesHistory);
        setPosition(transformedMovesHistory[0].fen);
        setCurrentMoveIndex(0);
      } catch (error: unknown) {
        if (error instanceof Error) {
          console.error("Failed to load report or game details:", error.message);
        } else {
          console.error("Failed to load report or game details:", error);
        }
      } finally {
        setLoading(false);
      }
    };

    loadReportAndGame();
  }, []);

  const handleSetPosition = (fen: string, disableAnim = false): void => {
    if (disableAnim) {
      setDisableAnimation(true);
      setTimeout(() => setDisableAnimation(false), 100);
    }
    setPosition(fen);
  };

  const handleMoveIndexChange = (index: number): void => {
    setCurrentMoveIndex(index);
    handleSetPosition(moveHistory[index].fen, true);
    // Zmienna navigationMode była nieużywana, więc została usunięta
  };

  const closeDialog = (): void => {
    setDialogOpen(false);
    // Po zamknięciu dialogu możemy przekierować użytkownika, jeśli potrzeba.
    router.push("/admin");
  };

  const handleBanUser = async (): Promise<void> => {
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
    } catch (error: unknown) {
      if (error instanceof Error) {
        console.error("Failed to ban the user:", error.message);
      } else {
        console.error("Failed to ban the user:", error);
      }
    }
  };

  const handleRejectReport = async (): Promise<void> => {
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
    } catch (error: unknown) {
      if (error instanceof Error) {
        console.error("Failed to reject the report:", error.message);
      } else {
        console.error("Failed to reject the report:", error);
      }
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
        isInteractive={false}
        boardOrientation="white" // Ustaw prawidłową wartość, np. "white" lub "black"
      >
        <div style={buttonsContainerStyles}>
          <Button
            variant="contained"
            sx={{
              backgroundColor: "#d32f2f",
              color: "white",
              marginLeft: "10px",
              width: "100%",
            }}
            onClick={handleBanUser}
            title="Ban suspect"
          >
            Ban suspect
          </Button>
          <Button
            variant="contained"
            sx={{
              backgroundColor: "#ba68c8", // Usunięto podwójny #
              color: "white", // Poprawiono kolor, np. na "white"
              marginLeft: "10px",
              width: "100%",
            }}
            onClick={handleRejectReport}
            title="Reject report when user played fair"
          >
            Reject report
          </Button>
        </div>
      </GameReviewContent>
      <CustomDialog
        open={dialogOpen}
        onClose={closeDialog}
        title={dialogTitle}
        content={<div style={{ color: "white" }}>{dialogContent}</div>}
        actions={
          <button
            style={{ ...buttonStyle, backgroundColor: "blue" }}
            onClick={closeDialog}
          >
            OK
          </button>
        }
      />
    </>
  );
};

export default AdminReviewPage;
