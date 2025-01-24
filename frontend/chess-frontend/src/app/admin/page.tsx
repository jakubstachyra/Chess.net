"use client";

import React, { useState, useRef, useEffect, useCallback } from "react";
import BackgroundUI from "app/components/backgroundUI/pages";
import { useAppSelector } from "../store/hooks";
import { useRouter } from "next/navigation";
import {
  fetchReport,
  fetchRequests,
  verifyUser,
  rejectRequest,
} from "../services/adminService";
import ChessboardComponent from "app/components/chessBoard/chessBoard";
import ListDisplay from "../components/listDisplay/listDisplay";
import CustomDialog from "../components/customDialog/customdialog"; // Import custom dialog
import { Button } from "@mui/material";
import { AdminRequest } from "types/types";
// Define the Report type
type Report = {
  id: string;
  //@ts-expect-error tak ma byc
  [key: string]: value;
};


export default function AdminPage() {
  const router = useRouter();
  const { user } = useAppSelector((state) => state.user);
  const rightSectionRef = useRef<HTMLDivElement | null>(null);
  const [boardWidth, setBoardWidth] = useState<number>(400);
  const [report, setReport] = useState<Report | null>(null);
  const [adminRequests, setAdminRequests] = useState<AdminRequest[]>([]);
  const [selectedRequest, setSelectedRequest] = useState<AdminRequest | null>(null);
  const [isDialogOpen, setIsDialogOpen] = useState<boolean>(false);

  // Fetch the report data
  const getReport = async () => {
    try {
      const response: Report | null = await fetchReport();
      setReport(response);
    } catch (error) {
      console.error("Failed to fetch the report:", error);
    }
  };

  // Fetch the admin requests data
  const getAdminRequests = async () => {
    try {
      const response: AdminRequest[] = await fetchRequests();
      // Map the response to AdminRequest[]
      const requestsArray: AdminRequest[] = Array.isArray(response)
        ? response.map((req: AdminRequest) => ({
            id: req.id,
            userID: req.userID,
            userName: req.userName,
            reason: req.reason,
          }))
        : [
            {
              id: "",
              userID: "",
              userName: "",
              reason: "",
            },
          ];
      setAdminRequests(requestsArray);
    } catch (error) {
      console.error("Failed to fetch admin requests:", error);
    }
  };

  useEffect(() => {
    getReport();
    getAdminRequests();
  }, []);

  const handleMakeReview = () => {
    if (report) {
      router.push(`/admin/review/${report.id}`);
    } else {
      console.error("Report is not loaded yet.");
    }
  };

  useEffect(() => {
    if (rightSectionRef.current) {
      setBoardWidth(rightSectionRef.current.clientWidth * 0.9);
    }

    const handleResize = () => {
      if (rightSectionRef.current) {
        setBoardWidth(rightSectionRef.current.clientWidth * 0.9);
      }
    };

    window.addEventListener("resize", handleResize);
    return () => window.removeEventListener("resize", handleResize);
  }, []);

  const openDialog = useCallback((request: AdminRequest) => {
    setSelectedRequest(request);
    setIsDialogOpen(true);
  }, []);

  const closeDialog = useCallback(() => {
    setIsDialogOpen(false);
    setSelectedRequest(null);
  }, []);

  const handleVerify = async () => {
    try {
      if (selectedRequest) {
        await verifyUser(selectedRequest.userID, selectedRequest.id);
        await getAdminRequests();
        closeDialog();
      }
    } catch (error) {
      console.error("Failed to verify user:", error);
    }
  };

  const handleReject = async () => {
    try {
      if (selectedRequest) {
        await rejectRequest(selectedRequest.id);
        await getAdminRequests();
        closeDialog();
      }
    } catch (error) {
      console.error("Failed to reject request:", error);
    }
  };

  return (
    <div style={backgroundContainerStyles}>
      <BackgroundUI>
        <h1 style={sectionTitleStyles}>Hello {user?.username}!</h1>
        <div style={splitContainerStyles}>
          {/* Left Section */}
          <div style={leftSectionStyles}>
            <h1 style={sectionTitleStyles}>Verify admin</h1>
            <div style={listContainerStyles}>
              <ListDisplay
                data={adminRequests}
                containerHeight="100%"
                renderRow={(item: AdminRequest) => (
                  <div
                    style={{
                      ...listRowStyles,
                      cursor: "pointer",
                      textAlign: "center",
                    }}
                    onClick={() => openDialog(item)}
                  >
                    {item.userName}
                  </div>
                )}
              />
            </div>
            <Button
              style={{
                ...buttonStyle,
                visibility: "hidden", // Hide the button
                pointerEvents: "none", // Disable interaction
              }}
            >
              Invisible Button
            </Button>
          </div>

          {/* Right Section */}
          <div style={rightSectionStyles} ref={rightSectionRef}>
            <h1 style={sectionTitleStyles}>Suspect review</h1>
            <div style={rightContentStyles}>
              <ChessboardComponent
                boardWidth={boardWidth}
                isDraggablePiece={() => false} // Changed to boolean
              />
              <Button
                style={report ? buttonStyle : disabledButtonStyle}
                onClick={handleMakeReview}
                disabled={!report}
                variant="contained"
                color={!report ? "secondary" : "primary"}
              >
                Make a review
              </Button>
            </div>
          </div>
        </div>

        {selectedRequest && (
          <CustomDialog
            open={isDialogOpen}
            onClose={closeDialog}
            title={`Details request no. ${selectedRequest.id}`}
            content={
              <p style={{ color: "white", textAlign: "center" }}>
                <strong>User:</strong> {selectedRequest.userName}
                <br />
                <strong>Reason:</strong> {selectedRequest.reason}
              </p>
            }
            actions={
              <>
                <Button
                  variant="contained"
                  sx={{
                    backgroundColor: "#4caf50",
                    color: "white",
                    marginLeft: "10px",
                  }}
                  onClick={handleVerify}
                >
                  Verify
                </Button>
                <Button
                  color="primary"
                  variant="contained"
                  sx={{
                    backgroundColor: "#d32f2f",
                    color: "white",
                    marginLeft: "10px",
                  }}
                  onClick={handleReject}
                >
                  Reject
                </Button>
                <Button
                  color="primary"
                  variant="outlined"
                  sx={{ color: "white", borderColor: "white" }}
                  onClick={closeDialog}
                >
                  Close
                </Button>
              </>
            }
          />
        )}
      </BackgroundUI>
    </div>
  );
}

// Style Definitions with Explicit Typing
const backgroundContainerStyles: React.CSSProperties = {
  display: "flex",
  justifyContent: "center",
  alignItems: "center",
  width: "50%",
  height: "70%",
  color: "white",
};

const listRowStyles: React.CSSProperties = {
  padding: "10px",
  borderRadius: "5px",
  backgroundColor: "rgba(255, 255, 255, 0.1)",
  marginBottom: "5px",
  width: "100%",
  boxSizing: "border-box",
};

const splitContainerStyles: React.CSSProperties = {
  display: "flex",
  width: "100%",
  height: "100%",
  alignItems: "stretch",
};

const leftSectionStyles: React.CSSProperties = {
  flex: 1,
  display: "flex",
  flexDirection: "column",
  alignItems: "center",
  justifyContent: "space-between",
  padding: "2%",
  borderRight: "1px solid rgba(255, 255, 255, 0.2)",
  height: "100%",
  boxSizing: "border-box",
};

const rightSectionStyles: React.CSSProperties = {
  flex: 1,
  display: "flex",
  flexDirection: "column",
  alignItems: "center",
  justifyContent: "space-between",
  padding: "2%",
  height: "100%",
  boxSizing: "border-box",
};

const listContainerStyles: React.CSSProperties = {
  flex: 1,
  width: "100%",
  overflow: "auto",
  maxHeight: "90%",
  boxSizing: "border-box",
};

const rightContentStyles: React.CSSProperties = {
  display: "flex",
  flexDirection: "column",
  alignItems: "center",
  justifyContent: "space-between",
  width: "100%",
  height: "100%",
  gap: "15px",
  boxSizing: "border-box",
};

const sectionTitleStyles: React.CSSProperties = {
  color: "white",
  marginBottom: "10px",
  textAlign: "center",
};

const buttonStyle: React.CSSProperties = {
  padding: "10px 20px",
  fontSize: "16px",
  fontWeight: "bold",
  color: "#fff",
  backgroundColor: "#007bff",
  border: "none",
  borderRadius: "5px",
  cursor: "pointer",
  boxShadow: "0 4px 30px rgba(0, 0, 0, 0.5)",
  width: "100%",
};

const disabledButtonStyle: React.CSSProperties = {
  ...buttonStyle,
  backgroundColor: "gray",
  cursor: "not-allowed",
  boxShadow: "none",
  opacity: 0.6,
};
