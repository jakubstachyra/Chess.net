import React, { useEffect, useRef } from "react";

const MoveHistory = ({ moveHistory }) => {
  const containerRef = useRef(null);

  useEffect(() => {
    if (containerRef.current) {
      containerRef.current.scrollTop = containerRef.current.scrollHeight;
    }
  }, [moveHistory]); // Przewijaj na dół przy każdej zmianie historii ruchów

  const moves = moveHistory.reduce((rows, entry, index) => {
    const pairIndex = Math.floor(index / 2);
    if (!rows[pairIndex]) rows[pairIndex] = { white: null, black: null };
    if (index % 2 === 0) rows[pairIndex].white = entry.move; // Ruch białych
    else rows[pairIndex].black = entry.move; // Ruch czarnych
    return rows;
  }, []);

  return (
    <div style={movesContainerStyles} ref={containerRef}>
      {moves.map((row, index) => (
        <div
          key={index}
          style={{
            ...moveRowStyles,
            backgroundColor: index % 2 === 0 ? "rgba(255, 255, 255, 0.2)" : "rgba(255, 255, 255, 0.1)",
            backdropFilter: "blur(20px)", // Rozmycie tła
          }}
        >
          <span style={moveNumberStyles}>{index + 1}.</span>
          <span style={moveStyles}>{row.white || ""}</span>
          <span style={moveStyles}>{row.black || ""}</span>
        </div>
      ))}
    </div>
  );
};

// Style dla komponentu
const movesContainerStyles = {
  display: "flex",
  flexDirection: "column",
  alignItems: "stretch", // Wypełnij całą szerokość kontenera
  gap: "5px",
  width: "100%",
  height: "400px",
  overflowY: "auto", // Przewijanie w pionie, gdy lista jest zbyt długa
  padding: "10px",
  border: "1px solid rgba(255, 255, 255, 0.3)",
  borderRadius: "5px", // Zaokrąglone rogi dla estetyki
  backgroundColor: "rgba(0, 0, 0, 0.1)", // Tło kontenera
  backdropFilter: "blur(10px)", // Rozmycie tła kontenera
};

const moveRowStyles = {
  display: "flex",
  alignItems: "center",
  justifyContent: "space-between", // Rozciągnij ruchy na całą szerokość
  padding: "10px", // Większe wewnętrzne odstępy
  borderRadius: "5px", // Zaokrąglone rogi dla wierszy
  width: "100%", // Wypełnij całą szerokość kontenera
};

const moveNumberStyles = {
  color: "white",
  fontWeight: "bold",
  minWidth: "30px", // Utrzymanie stałej szerokości dla numeracji
};

const moveStyles = {
  color: "white",
  textAlign: "center", // Wyśrodkowanie tekstu ruchu
  flex: 1, // Równe rozciągnięcie obu ruchów
  fontFamily: "monospace", // Lepsza czytelność dla tekstu ruchów
};

export default MoveHistory;
