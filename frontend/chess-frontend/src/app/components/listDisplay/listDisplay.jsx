import React, { useEffect, useRef } from "react";

const ListDisplay = ({ data, renderRow, containerHeight = "400px" }) => {
  const containerRef = useRef(null);

  useEffect(() => {
    if (containerRef.current) {
      containerRef.current.scrollTop = containerRef.current.scrollHeight;
    }
  }, [data]);

  return (
    <div style={{ ...listContainerStyles, height: containerHeight }} ref={containerRef}>
      {data.map((item, index) => (
        <div
          key={index}
          style={{
            ...listRowStyles,
            backgroundColor: index % 2 === 0 ? "rgba(255, 255, 255, 0.2)" : "rgba(255, 255, 255, 0.1)",
          }}
        >
          {renderRow(item, index)}
        </div>
      ))}
    </div>
  );
};

// Style dla komponentu
const listContainerStyles = {
  display: "flex",
  flexDirection: "column",
  alignItems: "stretch",
  gap: "5px",
  width: "100%",
  overflowY: "auto",
  padding: "10px",
  border: "1px solid rgba(255, 255, 255, 0.3)",
  borderRadius: "5px",
  backgroundColor: "rgba(0, 0, 0, 0.1)",
  backdropFilter: "blur(10px)",
};

const listRowStyles = {
  display: "flex",
  alignItems: "center",
  justifyContent: "space-between",
  padding: "10px",
  borderRadius: "5px",
  width: "100%",
};

export default ListDisplay;
