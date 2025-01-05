"use client";

import React, { useState, useRef, useEffect } from "react";
import BackgroundUI from "app/components/backgroundUI/pages";
import { Chessboard } from "react-chessboard";
import { useSelector } from "react-redux";
import { useRouter } from "next/navigation";
import { fetchReport } from "../services/adminService";
import ChessboardComponent from "app/components/chessBoard/chessBoard";

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
            setReport(reportData); // Zapisz report w stanie
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
                    Hello {user?.username}!
                </h1>
                <div style={splitContainerStyles}>
                    <div style={leftSectionStyles}>
                        <button style={buttonStyle}>Overwatch Left</button>
                    </div>
                    <div style={rightSectionStyles} ref={rightSectionRef}>
                        <div style={rightContentStyles}>
                            <h1>Suspect review</h1>
                            <ChessboardComponent
                                boardWidth={boardWidth}
                                isDraggablePiece={() => false}
                            />

                            <button style={buttonStyle} onClick={handleMakeReview}>Make a review</button>
                        </div>
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

const splitContainerStyles = {
    display: "flex", 
    width: "100%",
    height: "100%",
};

const leftSectionStyles = {
    flex: 1, // 30% szerokości
    display: "flex",
    justifyContent: "center",
    alignItems: "center",
    borderRight: "1px solid rgba(255, 255, 255, 0.2)", // Linia między sekcjami
};

const rightSectionStyles = {
    flex: 1, // 70% szerokości
    display: "flex",
    justifyContent: "center",
    alignItems: "center",
    position: "relative",
};

const rightContentStyles = {
    display: "flex",
    flexDirection: "column",
    alignItems: "center",
    justifyContent: "space-between",
    gap: "15px",
    padding: "8%",
    width: "90%", // Domyślna szerokość zawartości
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
};