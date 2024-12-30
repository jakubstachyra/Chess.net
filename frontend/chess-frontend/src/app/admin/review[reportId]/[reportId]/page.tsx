"use client";

import React, { useState, useRef, useEffect } from "react";
import BackgroundUI from "app/components/backgroundUI/pages";
import { Chessboard } from "react-chessboard";
import { useSelector } from "react-redux";
import { useRouter } from "next/navigation";
import { fetchReport } from "../../../services/reportService";

export default function AdminPage() {
    const router = useRouter();
    const { user } = useSelector((state) => state.user); 
    const rightSectionRef = useRef(null);
    const [boardWidth, setBoardWidth] = useState(400); // Domyślna szerokość
    const [report, setReport] = useState(null); 
    
    const getReport = async () => {
        try {
            const reportData = await fetchReport();
            console.log(reportData);
            setReport(reportData); // Zapisz raport w stanie
        } catch (error) {
            console.error("Failed to fetch the report:", error);
        }
    };

    useEffect(() => {
        getReport(); // Pobierz raport po załadowaniu komponentu
    }, []);

    // Funkcja obsługująca przekierowanie
    const handleMakeReview = () => {
        if (report) {
            router.push(`/admin/review/${report.id}`); // Przejdź do strony recenzji
        } else {
            console.error("Report is not loaded yet.");
        }
    };

    useEffect(() => {
        // Ustaw boardWidth na szerokość rightSectionStyles
        if (rightSectionRef.current) {
            setBoardWidth(rightSectionRef.current.clientWidth * 0.8);
        }

        // Aktualizacja przy zmianie rozmiaru okna
        const handleResize = () => {
            if (rightSectionRef.current) {
                setBoardWidth(rightSectionRef.current.clientWidth * 0.8);
            }
        };

        window.addEventListener("resize", handleResize);
        return () => window.removeEventListener("resize", handleResize);
    }, []);

    return (
        <div style={backgroundContainerStyles}>
            <BackgroundUI>
                <h1 style={{ textAlign: "center", marginBottom: "20px" }}>
                    Review of report no !
                </h1>
                <div style={chessboardContainerStyles}>
                    <Chessboard
                        boardWidth={boardWidth}
                        isDraggablePiece={() => false}
                    />
                    <div style={{ ...buttonsContainerStyles, width: boardWidth }}>
                        <button style={{ ...buttonStyle, width: "50%" }} onClick={handleMakeReview}>Ban user</button>
                        <button style={{ ...buttonStyle, width: "50%", backgroundColor: "#00ff00" }} onClick={handleMakeReview}>Reject report</button>
                    </div>
                </div>
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

const buttonStyle = {
    padding: "10px 20px",
    fontSize: "16px",
    fontWeight: "bold",
    color: "#fff",
    backgroundColor: "#dd0000",
    border: "none",
    borderRadius: "5px",
    cursor: "pointer",
    boxShadow: "0 4px 30px rgba(0, 0, 0, 0.5)",
};

  // Style
  const chessboardContainerStyles = {
    display: "flex",
    flexDirection: "column",
    alignItems: "center",
    justifyContent: "center",
    gap: "10px",
};

const buttonsContainerStyles = {
    display: "flex",
    justifyContent: "space-between",
    gap: "10px",
    marginTop: "20px",
};
