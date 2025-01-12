"use client";
import React from "react";
import GameDetails from "../../components/gameDetails/gameDetails";

const DetailsPage = () => {
  return (
    <div style={{width: "70%",height: "85%", marginTop: "50px" }}>
      {/* Rankings Table */}
      <GameDetails />
    </div>
  );  
};

export default DetailsPage;
