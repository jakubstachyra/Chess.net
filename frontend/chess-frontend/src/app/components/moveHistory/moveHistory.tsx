import React from "react";
import ListDisplay from "../listDisplay/listDisplay";
import { MoveHistoryEntry, MoveRow } from "types/types";

interface Props {
  moveHistory: MoveHistoryEntry[];
  currentMoveIndex: number;
  onSelectMoveIndex: (index: number) => void;
}

const MoveHistory: React.FC<Props> = ({
  moveHistory,
  currentMoveIndex,
  onSelectMoveIndex,
}) => {
  // Pomijamy pierwszy element, bo to "startFen" / "start"
  const relevantMoves = moveHistory.slice(1);

  // Grupujemy co dwa ruchy (biały, czarny) w jednym wierszu
  // Ale musimy pamiętać, że w oryginalnej tablicy index=1 to pierwszy ruch białych,
  // index=2 to pierwszy ruch czarnych, itd.
  const moves = relevantMoves.reduce((rows: MoveRow[], entry, index) => {
    const pairIndex = Math.floor(index / 2);
    if (!rows[pairIndex]) {
      rows[pairIndex] = {
        white: null,
        black: null,
        whiteIndex: null,
        blackIndex: null,
      };
    }
    if (index % 2 === 0) {
      rows[pairIndex].white = entry.move;
      // Zapamiętujemy oryginalny "globalny" index w moveHistory (który tu jest 1 + index)
      rows[pairIndex].whiteIndex = 1 + index;
    } else {
      rows[pairIndex].black = entry.move;
      rows[pairIndex].blackIndex = 1 + index;
    }
    return rows;
  }, []);

  const renderMoveRow = (row: MoveRow, rowIndex: number) => {
    // Numer sekwencji ruchów (1-based)
    const moveNumber = rowIndex + 1;

    const whiteIsCurrent =
      row.whiteIndex !== null && row.whiteIndex === currentMoveIndex;
    const blackIsCurrent =
      row.blackIndex !== null && row.blackIndex === currentMoveIndex;

    return (
      <>
        <span style={moveNumberStyles}>{moveNumber}.</span>

        {/* Ruch białych */}
        <span
          style={{
            ...moveStyles,
            fontWeight: whiteIsCurrent ? "bold" : "normal",
            textDecoration: whiteIsCurrent ? "underline" : "none",
            cursor: row.whiteIndex !== null ? "pointer" : "default",
          }}
          onClick={() => {
            if (row.whiteIndex !== null) onSelectMoveIndex(row.whiteIndex);
          }}
        >
          {row.white || ""}
        </span>

        {/* Ruch czarnych */}
        <span
          style={{
            ...moveStyles,
            fontWeight: blackIsCurrent ? "bold" : "normal",
            textDecoration: blackIsCurrent ? "underline" : "none",
            cursor: row.blackIndex !== null ? "pointer" : "default",
          }}
          onClick={() => {
            if (row.blackIndex !== null) onSelectMoveIndex(row.blackIndex);
          }}
        >
          {row.black || ""}
        </span>
      </>
    );
  };

  return <ListDisplay data={moves} renderRow={renderMoveRow} />;
};

// Style zgodnie z poprzednią wersją
const moveNumberStyles: React.CSSProperties = {
  color: "white",
  fontWeight: "bold",
  minWidth: "30px",
};

const moveStyles: React.CSSProperties = {
  color: "white",
  textAlign: "center", // Ensure the value matches one of the allowed types
  flex: 1,
  fontFamily: "monospace",
};


export default MoveHistory;
