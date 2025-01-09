import React from "react";
import ListDisplay from "../listDisplay/listDisplay";

const MoveHistory = ({ moveHistory }) => {
  const moves = moveHistory.reduce((rows, entry, index) => {
    const pairIndex = Math.floor(index / 2);
    if (!rows[pairIndex]) rows[pairIndex] = { white: null, black: null };
    if (index % 2 === 0) rows[pairIndex].white = entry.move;
    else rows[pairIndex].black = entry.move;
    return rows;
  }, []);

  const renderMoveRow = (row, index) => (
    <>
      <span style={moveNumberStyles}>{index + 1}.</span>
      <span style={moveStyles}>{row.white || ""}</span>
      <span style={moveStyles}>{row.black || ""}</span>
    </>
  );

  return <ListDisplay data={moves} renderRow={renderMoveRow} />;
};

// Style specyficzne dla ruchów
const moveNumberStyles = {
  color: "white",
  fontWeight: "bold",
  minWidth: "30px",
};

const moveStyles = {
  color: "white",
  textAlign: "center",
  flex: 1,
  fontFamily: "monospace",
};

export default MoveHistory;
