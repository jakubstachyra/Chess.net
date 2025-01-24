"use client";

import React, { useState, useRef, useEffect } from "react";
import BackgroundUI from "app/components/backgroundUI/pages";
import { useSelector } from "react-redux";
import { useRouter } from "next/navigation";
import { fetchReport, fetchRequests, verifyUser, rejectRequest } from "../services/adminService"; 
import ChessboardComponent from "app/components/chessBoard/chessBoard";
import ListDisplay from "../components/listDisplay/listDisplay"; 
import CustomDialog from "../components/customDialog/customdialog"; // Import niestandardowego dialogu
import { Button } from "@mui/material";

export default function AdminPage() {
  const router = useRouter();
  const { user } = useSelector((state) => state.user);
  const rightSectionRef = useRef(null);
  const [boardWidth, setBoardWidth] = useState(400); 
  const [report, setReport] = useState(null);
  const [adminRequests, setAdminRequests] = useState([]);
  const [selectedRequest, setSelectedRequest] = useState(null);
  const [isDialogOpen, setIsDialogOpen] = useState(false);

  const getReport = async () => {
    try {
      const response = await fetchReport();
      setReport(response);
    } catch (error) {
      console.error("Failed to fetch the report:", error);
    }
  };

  const getAdminRequests = async () => {
    try {
      const response = await fetchRequests();
      const requestsArray = Array.isArray(response) ? response : [response];
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
      // Powiększenie szachownicy
    }

    const handleResize = () => {
      if (rightSectionRef.current) {
        setBoardWidth(rightSectionRef.current.clientWidth * 0.9);
      }
    };

    window.addEventListener("resize", handleResize);
    return () => window.removeEventListener("resize", handleResize);
  }, []);

  const openDialog = (request) => {
    setSelectedRequest(request);
    setIsDialogOpen(true);
  };

  const closeDialog = () => {
    setIsDialogOpen(false);
    setSelectedRequest(null);
  };

  const handleVerify = async () => {
    try {
      await verifyUser(selectedRequest.userID, selectedRequest.id);
      await getAdminRequests(); // Odświeżenie listy
      closeDialog();
    } catch (error) {
      console.error("Failed to verify user:", error);
    }
  };
  
  const handleReject = async () => {
    try {
      await rejectRequest(selectedRequest.id);
      await getAdminRequests(); // Odświeżenie listy
      closeDialog();
    } catch (error) {
      console.error("Failed to reject request:", error);
    }
  };
  
  return (
    <div style={backgroundContainerStyles}>
    <BackgroundUI>
      <h1 style={{ textAlign: "center", marginBottom: "20px" }}>
        Hello {user?.username}!
      </h1>
      <div style={splitContainerStyles}>
        {/* Lewa sekcja */}
        <div style={leftSectionStyles}>
      <h1 style={sectionTitleStyles}>Verify admin</h1>
      <div style={listContainerStyles}>
        <ListDisplay
          data={adminRequests}
          containerHeight="100%"
          renderRow={(item) => (
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
      <button
        style={{
          ...buttonStyle,
          visibility: "hidden", // Ukrycie przycisku
          pointerEvents: "none", // Wyłączenie interakcji
        }}
      >
        Invisible Button
      </button>
    </div>
  
        {/* Prawa sekcja */}
        <div style={rightSectionStyles} ref={rightSectionRef}>
          <h1 style={sectionTitleStyles}>Suspect review</h1>
          <div style={rightContentStyles}>
            <ChessboardComponent
              boardWidth={boardWidth}
              isDraggablePiece={() => false}
            />
          <Button
            style={report ? buttonStyle : disabledButtonStyle}
            onClick={handleMakeReview}
            disabled={!report} // Ustawienie disabled na podstawie wartości report
            variant="contained" // Dodanie stylu przycisku MUI
            color={!report ? 'secondary' : 'primary'} // Opcjonalnie zmień kolor w zależności od stanu
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

// Style
const backgroundContainerStyles = {
  display: "flex",
  justifyContent: "center",
  alignItems: "center",
  width: "50%",
  height: "70%",
  color: "white",
};

const listRowStyles = {
  padding: "10px",
  borderRadius: "5px",
  backgroundColor: "rgba(255, 255, 255, 0.1)", 
  marginBottom: "5px",
  width: "100%",
  boxSizing: "border-box",
};
const splitContainerStyles = {
  display: "flex", 
  width: "100%",
  height: "100%",
  alignItems: "stretch", // Rozciąga sekcje na równą wysokość
};

const leftSectionStyles = {
  flex: 1,
  display: "flex",
  flexDirection: "column",
  alignItems: "center",
  justifyContent: "space-between", // Umożliwia dodanie pustej przestrzeni pod listą
  padding: "2%",
  borderRight: "1px solid rgba(255, 255, 255, 0.2)",
  height: "100%", // Sekcja zajmuje całą wysokość
  boxSizing: "border-box",
};

const rightSectionStyles = {
  flex: 1,
  display: "flex",
  flexDirection: "column",
  alignItems: "center",
  justifyContent: "space-between", // Przycisk na dole
  padding: "2%",
  height: "100%", // Sekcja zajmuje całą wysokość
  boxSizing: "border-box",
};

const listContainerStyles = {
  flex: 1, // Rozciąga listę na dostępną przestrzeń
  width: "100%",
  overflow: "auto", // Dodaje scroll, jeśli lista jest zbyt długa
  maxHeight: "90%", // Ogranicza maksymalną wysokość listy względem kontenera
  boxSizing: "border-box", // Zapewnia poprawne działanie paddingów
};


const rightContentStyles = {
  display: "flex",
  flexDirection: "column",
  alignItems: "center",
  justifyContent: "space-between", // Przycisk na dole
  width: "100%",
  height: "100%", 
  gap: "15px",
  boxSizing: "border-box",
};

const sectionTitleStyles = {
  color: "white",
  marginBottom: "10px",
  textAlign: "center",
};

const buttonStyle = {
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
const disabledButtonStyle = {
  ...buttonStyle,
  backgroundColor: "gray",
  cursor: "not-allowed",
  boxShadow: "none",
  opacity: 0.6,
};
